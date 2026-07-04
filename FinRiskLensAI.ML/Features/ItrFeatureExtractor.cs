using System.Globalization;
using FinRiskLensAI.Core.Models.Scoring;
using Newtonsoft.Json.Linq;

namespace FinRiskLensAI.ML.Features
{
    /// <summary>
    /// Extracts income and filing-discipline features from the ITR API response
    /// (result → one entry per assessment year, ITR form shape varies by itrType).
    /// </summary>
    public class ItrFeatureExtractor
    {
        public void Extract(string itrJson, MsmeFeatureSet features)
        {
            if (string.IsNullOrWhiteSpace(itrJson)) return;

            var root = JObject.Parse(itrJson);
            var result = root["result"] as JObject;
            if (result == null || !result.Properties().Any()) return;

            features.HasItr = true;
            int onTime = 0, timelinessKnown = 0;

            foreach (var yearProp in result.Properties())
            {
                var year = yearProp.Value as JObject;
                if (year == null) continue;

                // Income: form shape differs (ITR1 vs ITR3 etc.) — probe common paths.
                var income = FirstNumber(year,
                    "$..ITR1_IncomeDeductions.GrossTotIncome",
                    "$..['PartB-TI'].GrossTotalIncome",
                    "$..['PartB-TI'].TotalIncome",
                    "$..ITR1_IncomeDeductions.TotalIncome");
                if (income.HasValue)
                    features.ItrYearlyIncome[yearProp.Name] = income.Value;

                // Timeliness: filingDate (dd/MM/yyyy) vs ItrFilingDueDate (yyyy-MM-dd)
                var filingDate = ParseDate(year.Value<string>("filingDate"), "dd/MM/yyyy");
                var dueDateToken = year.SelectTokens("$..ItrFilingDueDate").FirstOrDefault();
                var dueDate = ParseDate(dueDateToken?.Value<string>(), "yyyy-MM-dd");
                if (filingDate.HasValue && dueDate.HasValue)
                {
                    timelinessKnown++;
                    if (filingDate.Value.Date <= dueDate.Value.Date) onTime++;
                }
            }

            features.ItrYearsFiled = features.ItrYearlyIncome.Count;
            features.ItrFilingTimeliness = timelinessKnown > 0 ? (double)onTime / timelinessKnown : 0.5;

            if (features.ItrYearlyIncome.Count > 0)
            {
                var series = features.ItrYearlyIncome.Values.ToArray(); // sorted by year label
                features.ItrLatestIncome = series[^1];
                features.ItrIncomeTrendSlope = TrendMath.NormalizedSlope(series);
                features.ItrMonthlyAvgIncome = series.Average() / 12.0;
            }

            ExtractFinancialRatios(result, features);
        }

        /// <summary>
        /// P&amp;L / balance-sheet ratios for business filers (ITR-3/5/6 with books).
        /// Latest year with a trading account wins. Ratios stay null when the source
        /// figures aren't in the return — never fabricated.
        /// </summary>
        private static void ExtractFinancialRatios(JObject result, MsmeFeatureSet features)
        {
            foreach (var yearProp in result.Properties().OrderByDescending(p => p.Name))
            {
                var year = yearProp.Value as JObject;
                if (year == null) continue;

                var turnover = FirstNumber(year,
                    "$..TradingAccount.SalesGrossReceiptsTotal",
                    "$..TradingAccount.TotRevenueFrmOperations",
                    "$..PARTA_PL.NoBooksOfAccPL.GrossReceipt");
                if (!turnover.HasValue || turnover.Value <= 0) continue;

                features.ItrFinancialsYear = yearProp.Name;
                features.ItrBusinessTurnover = turnover.Value;

                var pbidta = FirstNumber(year, "$..DebitsToPL.PBIDTA");            // EBITDA proxy
                if (pbidta.HasValue)
                    features.ItrEbitdaMargin = Math.Round(pbidta.Value / turnover.Value, 4);

                var pat = FirstNumber(year,
                    "$..TaxProvAppr.ProfitAfterTax",
                    "$..PARTA_PL.NoBooksOfAccPL.NetProfit");
                if (pat.HasValue)
                    features.ItrNetProfitMargin = Math.Round(pat.Value / turnover.Value, 4);

                var debtors = FirstNumber(year,
                    "$..SundryDebtors", "$..SndryDebtors", "$..TradeReceivables");
                if (debtors.HasValue && debtors.Value > 0)
                    features.ItrDebtorDays = Math.Round(debtors.Value / turnover.Value * 365, 1);

                var totalAssets = FirstNumber(year, "$..TotalAssets", "$..TotAssets");
                if (totalAssets.HasValue && totalAssets.Value > 0)
                    features.ItrAssetTurnover = Math.Round(turnover.Value / totalAssets.Value, 2);

                return;   // latest business year only
            }
        }

        private static double? FirstNumber(JObject scope, params string[] jsonPaths)
        {
            foreach (var path in jsonPaths)
            {
                var token = scope.SelectTokens(path).FirstOrDefault();
                if (token != null && double.TryParse(token.ToString(),
                        NumberStyles.Any, CultureInfo.InvariantCulture, out var value))
                    return value;
            }
            return null;
        }

        private static DateTime? ParseDate(string? value, string format)
            => DateTime.TryParseExact(value, format, CultureInfo.InvariantCulture, DateTimeStyles.None, out var d)
                ? d : null;
    }
}
