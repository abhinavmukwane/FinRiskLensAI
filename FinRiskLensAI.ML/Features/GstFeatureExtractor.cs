using FinRiskLensAI.Core.Models.Scoring;
using Newtonsoft.Json.Linq;

namespace FinRiskLensAI.ML.Features
{
    /// <summary>
    /// Extracts revenue and compliance features from GST API responses: taxpayer
    /// profile, monthly GSTR-3B details, GSTR-1 summaries / B2B invoices / CDNR
    /// (credit-debit notes) / HSN summaries, and GSTR-2A inward (purchase) invoices.
    /// Real API responses arrive wrapped in a response.message.data envelope —
    /// <see cref="Unwrap"/> handles both wrapped and bare payloads.
    /// </summary>
    public class GstFeatureExtractor
    {
        public void Extract(
            string? taxpayerJson,
            IReadOnlyList<string>? gstr3bJsons,
            IReadOnlyList<string>? gstr1SummaryJsons,
            IReadOnlyList<string>? gstr1B2bJsons,
            IReadOnlyList<string>? gstr1CdnrJsons,
            IReadOnlyList<string>? gstr1HsnJsons,
            IReadOnlyList<string>? gstr2aB2bJsons,
            MsmeFeatureSet features)
        {
            var anyData = false;

            if (!string.IsNullOrWhiteSpace(taxpayerJson))
            {
                anyData = true;
                var profile = Unwrap(JObject.Parse(taxpayerJson));
                features.GstRegistrationActive = string.Equals(
                    profile.Value<string>("sts"), "Active", StringComparison.OrdinalIgnoreCase);
            }

            // ── Monthly turnover from GSTR-3B outward taxable supplies (osup_det.txval)
            foreach (var data in ParseAll(gstr3bJsons, ref anyData))
            {
                var period = data.Value<string>("ret_period");           // MMyyyy
                if (string.IsNullOrEmpty(period) || period.Length != 6) continue;

                var sortablePeriod = period[2..] + period[..2];           // yyyyMM for ordering
                var txval = data.SelectToken("sup_details.osup_det.txval")?.Value<double>() ?? 0;
                var zeroRated = data.SelectToken("sup_details.osup_zero.txval")?.Value<double>() ?? 0;
                features.GstMonthlyTurnover[sortablePeriod] = txval + zeroRated;
            }

            // ── B2B share + counterparty diversity from GSTR-1 summaries
            double b2bTax = 0, totalTax = 0;
            var counterparties = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var data in ParseAll(gstr1SummaryJsons, ref anyData))
            {
                foreach (var sec in data.SelectTokens("sec_sum[*]").OfType<JObject>())
                {
                    var name = sec.Value<string>("sec_nm") ?? string.Empty;
                    var tax = sec.Value<double?>("ttl_tax") ?? 0;
                    if (name is "B2B" or "B2CL" or "B2CS" or "EXP") totalTax += tax;
                    if (name == "B2B") b2bTax += tax;

                    foreach (var ctin in sec.SelectTokens("$..cpty_sum[*].ctin"))
                    {
                        var value = ctin.Value<string>();
                        if (!string.IsNullOrEmpty(value)) counterparties.Add(value);
                    }
                }
            }

            // ── Counterparties from GSTR-1 B2B / e-invoice payloads
            foreach (var data in ParseAll(gstr1B2bJsons, ref anyData))
            {
                foreach (var ctin in data.SelectTokens("b2b[*].ctin"))
                {
                    var value = ctin.Value<string>();
                    if (!string.IsNullOrEmpty(value)) counterparties.Add(value);
                }
            }

            // ── Credit/debit notes (CDNR): total note value → revenue-reversal signal
            double creditNoteValue = 0;
            foreach (var data in ParseAll(gstr1CdnrJsons, ref anyData))
            {
                foreach (var txval in data.SelectTokens("cdnr[*].nt[*].itms[*].itm_det.txval"))
                    creditNoteValue += txval.Value<double?>() ?? 0;
            }

            // ── HSN summaries: distinct product/service codes sold
            var hsnCodes = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var data in ParseAll(gstr1HsnJsons, ref anyData))
            {
                foreach (var code in data.SelectTokens("hsn.hsn_b2b[*].hsn_sc")
                             .Concat(data.SelectTokens("hsn.hsn_b2c[*].hsn_sc")))
                {
                    var value = code.Value<string>();
                    if (!string.IsNullOrEmpty(value)) hsnCodes.Add(value);
                }
            }

            // ── GSTR-2A: inward (purchase) invoice values
            double purchaseValue = 0;
            int purchaseMonths = 0;
            foreach (var data in ParseAll(gstr2aB2bJsons, ref anyData))
            {
                purchaseMonths++;
                foreach (var txval in data.SelectTokens("b2b[*].inv[*].itms[*].itm_det.txval"))
                    purchaseValue += txval.Value<double?>() ?? 0;
            }

            if (!anyData) return;
            features.HasGst = true;
            features.GstB2bShare = totalTax > 0 ? Math.Clamp(b2bTax / totalTax, 0, 1) : 0;
            features.GstCounterpartyCount = counterparties.Count;
            features.GstHsnProductCount = hsnCodes.Count;

            if (features.GstMonthlyTurnover.Count > 0)
            {
                var series = features.GstMonthlyTurnover.Values.ToArray();
                features.GstTurnoverTrendSlope = TrendMath.NormalizedSlope(series);
                features.GstMonthlyAvgTurnover = series.Average();
                features.GstAnnualisedTurnover = series.Average() * 12;

                var totalTurnover = series.Sum();
                features.GstCreditNoteRatio = totalTurnover > 0
                    ? Math.Clamp(creditNoteValue / totalTurnover, 0, 1) : 0;

                if (purchaseMonths > 0)
                {
                    features.GstMonthlyAvgPurchases = purchaseValue / purchaseMonths;
                    features.GstPurchaseToSalesRatio = features.GstMonthlyAvgTurnover > 0
                        ? Math.Clamp(features.GstMonthlyAvgPurchases / features.GstMonthlyAvgTurnover, 0, 3) : 0;
                }

                // Filed periods vs the span they should cover (expects one 3B per month)
                var months = MonthsBetween(
                    features.GstMonthlyTurnover.Keys.First(),
                    features.GstMonthlyTurnover.Keys.Last()) + 1;
                features.GstFilingRegularity = Math.Clamp(
                    (double)features.GstMonthlyTurnover.Count / Math.Max(1, months), 0, 1);
            }
        }

        /// <summary>
        /// Real GST APIs wrap the payload as response.message.data; schema samples and
        /// fixtures are bare. Accept both.
        /// </summary>
        private static JObject Unwrap(JObject root)
            => root.SelectToken("response.message.data") as JObject ?? root;

        private static IEnumerable<JObject> ParseAll(IReadOnlyList<string>? jsons, ref bool anyData)
        {
            var result = new List<JObject>();
            foreach (var json in jsons ?? Array.Empty<string>())
            {
                if (string.IsNullOrWhiteSpace(json)) continue;
                anyData = true;
                result.Add(Unwrap(JObject.Parse(json)));
            }
            return result;
        }

        private static int MonthsBetween(string fromYyyyMm, string toYyyyMm)
        {
            var fy = int.Parse(fromYyyyMm[..4]); var fm = int.Parse(fromYyyyMm[4..]);
            var ty = int.Parse(toYyyyMm[..4]); var tm = int.Parse(toYyyyMm[4..]);
            return (ty - fy) * 12 + (tm - fm);
        }
    }
}
