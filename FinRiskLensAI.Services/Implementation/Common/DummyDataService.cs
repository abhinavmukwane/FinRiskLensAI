using FinRiskLensAI.Core.Interfaces.IServices.Common;
using FinRiskLensAI.Core.Models.Onboarding;
using Newtonsoft.Json;

namespace FinRiskLensAI.Services.Implementation.Common
{
    /// <summary>
    /// Generates random-but-valid dummy payloads for demos / onboarding when the
    /// real government APIs aren't wired up. Swap out per source as APIs go live.
    /// </summary>
    public class DummyDataService : IDummyDataService
    {
        // GST state code + name keyed by the 2-letter Udyam state token (UDYAM-MH-...).
        private static readonly Dictionary<string, (string Code, string Name)> _states = new()
        {
            ["MH"] = ("27", "Maharashtra"), ["KA"] = ("29", "Karnataka"), ["DL"] = ("07", "Delhi"),
            ["GJ"] = ("24", "Gujarat"), ["TN"] = ("33", "Tamil Nadu"), ["UP"] = ("09", "Uttar Pradesh"),
            ["RJ"] = ("08", "Rajasthan"), ["WB"] = ("19", "West Bengal"), ["TS"] = ("36", "Telangana"),
            ["HR"] = ("06", "Haryana"), ["MP"] = ("23", "Madhya Pradesh"), ["PB"] = ("03", "Punjab"),
        };

        /// <summary>
        /// Builds a random-but-valid dummy Udyam response for a UAN — valid PAN,
        /// GSTIN (real checksum), mobile, dates — as a JSON string in the same
        /// shape as the real API, so SaveMsmeData / the blob pipeline accept it.
        /// </summary>
        public string GetDummyUdyam(string uan)
        {
            var rng = Random.Shared;
            string L(int n) => new(Enumerable.Range(0, n).Select(_ => (char)('A' + rng.Next(26))).ToArray());
            string D(int n) => new(Enumerable.Range(0, n).Select(_ => (char)('0' + rng.Next(10))).ToArray());

            var stateTok = (uan.Split('-').ElementAtOrDefault(1) ?? "MH").ToUpperInvariant();
            var (stateCode, stateName) = _states.TryGetValue(stateTok, out var s) ? s : ("27", "Maharashtra");

            var names = new[] { "Sharma", "Patel", "Reddy", "Gupta", "Iyer", "Nair", "Verma" };
            var kinds = new[] { "Textiles", "Traders", "Enterprises", "Industries", "Agro", "Exports" };
            var surname = names[rng.Next(names.Length)];
            var enterprise = $"{surname} {kinds[rng.Next(kinds.Length)]} Pvt Ltd";

            var pan = $"{L(3)}C{surname[0]}{D(4)}{L(1)}";   // 4th char C = company
            var gstin = BuildGstin(stateCode, pan, rng.Next(1, 10).ToString());
            var mobile = $"{rng.Next(6, 10)}{D(9)}";
            var email = $"{surname.ToLowerInvariant()}.{D(3)}@example.com";
            var incorp = DateTime.Today.AddDays(-rng.Next(365 * 2, 365 * 12));

            var model = new UdyamResponseModel
            {
                client_id = "FRL-DUMMY-" + D(6),
                uan = uan,
                certificate_url = $"https://udyamregistration.gov.in/certificate/{uan}.pdf",
                main_details = new UdyamResponseModel.MainDetailsModel
                {
                    enterprise_type_list = new()
                    {
                        new() { classification_year = incorp.Year.ToString(), enterprise_type = "Micro", classification_date = incorp }
                    },
                    name_of_enterprise = enterprise,
                    major_activity = rng.Next(2) == 0 ? "Manufacturing" : "Services",
                    social_category = "General",
                    date_of_commencement = incorp,
                    dic_name = stateName,
                    state = stateName,
                    applied_date = incorp.AddDays(10),
                    flat = D(2), name_of_building = $"{surname} Complex", road = "MG Road",
                    village = stateName, block = "Block " + L(1), city = stateName, pin = D(6),
                    mobile_number = mobile, email = email,
                    organization_type = "Private Limited Company", gender = "Male",
                    date_of_incorporation = incorp, msme_dfo = stateName,
                    registration_date = incorp.AddDays(15),
                    gstin = gstin, Pan = pan
                },
                location_of_plant_details = new()
                {
                    new() { unit_name = enterprise, line_1 = "Plot " + D(2), building = $"{surname} Complex",
                            village = stateName, street = "MG Road", road = "MG Road",
                            city = stateName, pin = D(6), state = stateName, district = stateName }
                },
                nic_code = new()
                {
                    new() { nic_2_digit = D(2), nic_4_digit = D(4), nic_5_digit = D(5),
                            activity_type = "Manufacturing", added_on = incorp }
                }
            };

            return JsonConvert.SerializeObject(model);
        }

        // Standard GSTIN check-digit: base-36, alternating weights 1,2 over the first 14 chars.
        private static string BuildGstin(string stateCode, string pan, string entityNo)
        {
            const string alphabet = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            var body = $"{stateCode}{pan}{entityNo}Z";   // 14 chars; 15th is the checksum
            int sum = 0;
            for (int i = 0; i < body.Length; i++)
            {
                int v = alphabet.IndexOf(body[i]) * (i % 2 == 0 ? 1 : 2);
                sum += v / 36 + v % 36;
            }
            return body + alphabet[(36 - sum % 36) % 36];
        }
    }
}
