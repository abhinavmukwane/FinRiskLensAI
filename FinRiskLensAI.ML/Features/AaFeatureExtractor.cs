using System.Globalization;
using System.Text.RegularExpressions;
using FinRiskLensAI.Core.Models.Scoring;
using Newtonsoft.Json.Linq;

namespace FinRiskLensAI.ML.Features
{
    /// <summary>
    /// Extracts cash-flow, UPI transaction-quality, and debt features from the
    /// Account Aggregator FI data response (body[] → fiObjects[] → Summary/Transactions).
    /// </summary>
    public class AaFeatureExtractor
    {
        private static readonly Regex BouncePattern = new("RETURN|BOUNCE|DISHONOU?R|INSUFF", RegexOptions.IgnoreCase);
        private static readonly Regex EmiPattern = new(@"\bEMI\b|\bLOAN\b|NACH.*LOAN", RegexOptions.IgnoreCase);

        public void Extract(string aaJson, MsmeFeatureSet features)
        {
            if (string.IsNullOrWhiteSpace(aaJson)) return;

            var root = JObject.Parse(aaJson);
            var accounts = root.SelectTokens("body[*].fiObjects[*]").OfType<JObject>().ToList();
            if (accounts.Count == 0) return;

            features.HasAa = true;

            double totalBalance = 0;
            var seenTxnIds = new HashSet<string>();
            var transactions = new List<(DateTime Date, string Type, string Mode, double Amount, string Narration)>();

            foreach (var account in accounts)
            {
                var balance = account.SelectToken("Summary.currentBalance")?.Value<string>();
                if (double.TryParse(balance, NumberStyles.Any, CultureInfo.InvariantCulture, out var bal))
                    totalBalance += bal;

                foreach (var txn in account.SelectTokens("Transactions.Transaction[*]").OfType<JObject>())
                {
                    // The same consent session can return the same transaction under
                    // multiple FIPs — dedupe on txnId so aggregates aren't inflated.
                    var txnId = txn.Value<string>("txnId") ?? Guid.NewGuid().ToString();
                    if (!seenTxnIds.Add(txnId)) continue;

                    var date = txn.Value<string>("valueDate");
                    if (!DateTime.TryParse(date, CultureInfo.InvariantCulture, DateTimeStyles.None, out var valueDate))
                        continue;

                    transactions.Add((
                        valueDate,
                        txn.Value<string>("type") ?? string.Empty,
                        txn.Value<string>("mode") ?? string.Empty,
                        txn.Value<double?>("amount") ?? 0,
                        txn.Value<string>("narration") ?? string.Empty));
                }
            }

            features.AaCurrentBalanceTotal = totalBalance;
            features.AaTransactionCount = transactions.Count;
            if (transactions.Count == 0) return;

            // ── Monthly aggregates
            foreach (var group in transactions.GroupBy(t => t.Date.ToString("yyyyMM")))
            {
                features.AaMonthlyCredits[group.Key] = group.Where(t => t.Type == "CREDIT").Sum(t => t.Amount);
                features.AaMonthlyDebits[group.Key] = group.Where(t => t.Type == "DEBIT").Sum(t => t.Amount);
            }
            features.AaMonthsCovered = features.AaMonthlyCredits.Count;

            var credits = features.AaMonthlyCredits.Values.ToArray();
            features.AaAvgMonthlyCredit = credits.Average();
            features.AaMonthlyAvgBankCredit = features.AaAvgMonthlyCredit;
            features.AaInflowVolatility = TrendMath.CoefficientOfVariation(credits);

            var avgDailyOutflow = features.AaMonthlyDebits.Values.DefaultIfEmpty(0).Average() / 30.44;
            features.AaDaysCashOnHand = avgDailyOutflow > 0 ? totalBalance / avgDailyOutflow : 365;

            // ── Discipline signals
            features.AaBounceCount = transactions.Count(t => BouncePattern.IsMatch(t.Narration));

            var emiDebits = transactions.Where(t => t.Type == "DEBIT" && EmiPattern.IsMatch(t.Narration)).ToList();
            features.AaEmiMonthlyOutflow = emiDebits.Count > 0
                ? emiDebits.Sum(t => t.Amount) / Math.Max(1, features.AaMonthsCovered) : 0;
            features.AaEmiToInflowRatio = features.AaAvgMonthlyCredit > 0
                ? Math.Clamp(features.AaEmiMonthlyOutflow / features.AaAvgMonthlyCredit, 0, 1) : 0;

            // ── UPI transaction quality
            features.AaUpiTxnShare = (double)transactions.Count(t => t.Mode == "UPI") / transactions.Count;

            var creditTxns = transactions.Where(t => t.Type == "CREDIT").ToList();
            if (creditTxns.Count > 0)
            {
                var payers = creditTxns
                    .Select(t => NormalizeCounterparty(t.Narration))
                    .Where(p => p.Length > 0)
                    .ToList();
                var distinct = payers.Distinct(StringComparer.OrdinalIgnoreCase).Count();
                features.AaUpiCounterpartyCount = distinct;
                features.AaRepeatPayerRatio = payers.Count > 0
                    ? 1.0 - (double)distinct / payers.Count : 0;
            }
        }

        /// <summary>Rough counterparty key from a narration like "NEFT/CLIENT PAYMENT" or "UPI/name".</summary>
        private static string NormalizeCounterparty(string narration)
        {
            var parts = narration.Split('/', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            return parts.Length > 1 ? parts[^1].ToUpperInvariant() : narration.Trim().ToUpperInvariant();
        }
    }
}
