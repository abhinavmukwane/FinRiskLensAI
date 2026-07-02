using FinRiskLensAI.ML.Models;
using Newtonsoft.Json.Linq;

namespace FinRiskLensAI.ML.Features
{
    /// <summary>
    /// Extracts revenue and compliance features from GST API responses:
    /// taxpayer profile (Search Taxpayer), monthly GSTR-3B details, monthly GSTR-1
    /// summaries, and optional GSTR-1 B2B invoice payloads.
    /// </summary>
    public class GstFeatureExtractor
    {
        public void Extract(
            string? taxpayerJson,
            IReadOnlyList<string>? gstr3bJsons,
            IReadOnlyList<string>? gstr1SummaryJsons,
            IReadOnlyList<string>? gstr1B2bJsons,
            MsmeFeatureSet features)
        {
            var anyData = false;

            if (!string.IsNullOrWhiteSpace(taxpayerJson))
            {
                anyData = true;
                var profile = JObject.Parse(taxpayerJson);
                features.GstRegistrationActive = string.Equals(
                    profile.Value<string>("sts"), "Active", StringComparison.OrdinalIgnoreCase);
            }

            // ── Monthly turnover from GSTR-3B outward taxable supplies (osup_det.txval)
            foreach (var json in gstr3bJsons ?? Array.Empty<string>())
            {
                if (string.IsNullOrWhiteSpace(json)) continue;
                var ret = JObject.Parse(json);
                var period = ret.Value<string>("ret_period");           // MMyyyy
                if (string.IsNullOrEmpty(period) || period.Length != 6) continue;

                anyData = true;
                var sortablePeriod = period[2..] + period[..2];          // yyyyMM for ordering
                var txval = ret.SelectToken("sup_details.osup_det.txval")?.Value<double>() ?? 0;
                var zeroRated = ret.SelectToken("sup_details.osup_zero.txval")?.Value<double>() ?? 0;
                features.GstMonthlyTurnover[sortablePeriod] = txval + zeroRated;
            }

            // ── B2B share + counterparty diversity from GSTR-1 summaries
            double b2bTax = 0, totalTax = 0;
            var counterparties = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var json in gstr1SummaryJsons ?? Array.Empty<string>())
            {
                if (string.IsNullOrWhiteSpace(json)) continue;
                var summary = JObject.Parse(json);
                anyData = true;

                foreach (var sec in summary.SelectTokens("sec_sum[*]").OfType<JObject>())
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

            // ── Counterparties from B2B invoice payloads (richer source when provided)
            foreach (var json in gstr1B2bJsons ?? Array.Empty<string>())
            {
                if (string.IsNullOrWhiteSpace(json)) continue;
                anyData = true;
                var b2b = JObject.Parse(json);
                foreach (var ctin in b2b.SelectTokens("b2b[*].ctin"))
                {
                    var value = ctin.Value<string>();
                    if (!string.IsNullOrEmpty(value)) counterparties.Add(value);
                }
            }

            if (!anyData) return;
            features.HasGst = true;
            features.GstB2bShare = totalTax > 0 ? Math.Clamp(b2bTax / totalTax, 0, 1) : 0;
            features.GstCounterpartyCount = counterparties.Count;

            if (features.GstMonthlyTurnover.Count > 0)
            {
                var series = features.GstMonthlyTurnover.Values.ToArray();
                features.GstTurnoverTrendSlope = TrendMath.NormalizedSlope(series);
                features.GstMonthlyAvgTurnover = series.Average();
                features.GstAnnualisedTurnover = series.Average() * 12;

                // Filed periods vs the span they should cover (expects one 3B per month)
                var months = MonthsBetween(
                    features.GstMonthlyTurnover.Keys.First(),
                    features.GstMonthlyTurnover.Keys.Last()) + 1;
                features.GstFilingRegularity = Math.Clamp(
                    (double)features.GstMonthlyTurnover.Count / Math.Max(1, months), 0, 1);
            }
        }

        private static int MonthsBetween(string fromYyyyMm, string toYyyyMm)
        {
            var fy = int.Parse(fromYyyyMm[..4]); var fm = int.Parse(fromYyyyMm[4..]);
            var ty = int.Parse(toYyyyMm[..4]); var tm = int.Parse(toYyyyMm[4..]);
            return (ty - fy) * 12 + (tm - fm);
        }
    }
}
