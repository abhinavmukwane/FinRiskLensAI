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
