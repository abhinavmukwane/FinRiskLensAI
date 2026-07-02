using System.Globalization;
using FinRiskLensAI.ML.Models;
using Newtonsoft.Json.Linq;

namespace FinRiskLensAI.ML.Features
{
    /// <summary>Extracts stability features from the Udyam Aadhaar API response.</summary>
    public class UdyamFeatureExtractor
    {
        public void Extract(string udyamJson, MsmeFeatureSet features)
        {
            if (string.IsNullOrWhiteSpace(udyamJson)) return;

            var root = JObject.Parse(udyamJson);
            var main = root["main_details"] as JObject;
            if (main == null) return;

            features.HasUdyam = true;

            var incorporation = ParseDate(main.Value<string>("date_of_incorporation"))
                             ?? ParseDate(main.Value<string>("date_of_commencement"));
            if (incorporation.HasValue)
                features.BusinessVintageMonths = Math.Max(0,
                    (DateTime.UtcNow - incorporation.Value).TotalDays / 30.44);

            features.MajorActivity = main.Value<string>("major_activity") ?? string.Empty;

            // Latest classification wins (list is per classification year)
            var types = main["enterprise_type_list"] as JArray;
            var latest = types?
                .OfType<JObject>()
                .OrderByDescending(t => t.Value<string>("classification_year"))
                .FirstOrDefault();
            features.EnterpriseType = latest?.Value<string>("enterprise_type") ?? string.Empty;

            features.PlantLocationCount = (root["location_of_plant_details"] as JArray)?.Count ?? 0;
            features.NicCodeCount = (root["nic_code"] as JArray)?.Count ?? 0;
        }

        private static DateTime? ParseDate(string? value)
            => DateTime.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.None, out var d) ? d : null;
    }
}
