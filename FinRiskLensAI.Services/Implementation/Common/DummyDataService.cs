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

        /// <summary>
        /// Dummy MCA response mirroring the real API shape. Company name is taken
        /// from the caller (kept in sync with Udyam/GST); directors are derived from
        /// the company name, and charges (secured-loan liens) are randomised each
        /// build so every company shows a different, realistic charge history.
        /// </summary>
        public string GetDummyMca(string uan, string companyName, string? pan = null)
        {
            var rng = Random.Shared;
            string D(int n) => new(Enumerable.Range(0, n).Select(_ => (char)('0' + rng.Next(10))).ToArray());

            var stateTok = (uan.Split('-').ElementAtOrDefault(1) ?? "MH").ToUpperInvariant();
            var (stateCode, stateName) = _states.TryGetValue(stateTok, out var s) ? s : ("27", "Maharashtra");

            companyName = string.IsNullOrWhiteSpace(companyName) ? "FinRiskLens Enterprises Pvt Ltd" : companyName.Trim();
            var isPrivate = companyName.ToUpperInvariant().Contains("PVT") || companyName.ToUpperInvariant().Contains("PRIVATE");
            var incYear = DateTime.Today.Year - rng.Next(3, 20);
            var incorp = new DateTime(incYear, rng.Next(1, 13), rng.Next(1, 28));

            // CIN: <listed><5-digit industry><state token><year><PTC|PLC><6-digit reg>
            var cin = $"U{D(5)}{stateTok}{incYear}{(isPrivate ? "PTC" : "PLC")}{D(6)}";

            // Directors — first is the promoter (derived from the company's first token).
            var firstNames = new[] { "Rajesh", "Anita", "Vikram", "Priya", "Suresh", "Neha", "Arun", "Kavita" };
            var promoter = companyName.Split(' ').FirstOrDefault() ?? "Owner";
            var directors = new[]
            {
                new { din_number = D(8), director_name = $"{promoter} {firstNames[rng.Next(firstNames.Length)]}",
                      start_date = incorp.ToString("yyyy-MM-dd"), end_date = "1800-01-01", surrendered_din = (string?)null },
                new { din_number = D(8), director_name = $"{firstNames[rng.Next(firstNames.Length)]} {promoter}",
                      start_date = incorp.AddDays(rng.Next(30, 400)).ToString("yyyy-MM-dd"), end_date = "1800-01-01", surrendered_din = (string?)null },
            };

            // Charges — dynamic count, amounts and dates; a mix of open/modified liens.
            var assets = new[] { "Lien on Fixed Deposits (FD)", "Hypothecation of Stock & Book Debts",
                                 "Mortgage of Immovable Property", "Charge on Plant & Machinery", "Lien on Current Assets" };
            var charges = Enumerable.Range(0, rng.Next(3, 9)).Select(_ =>
            {
                var created = incorp.AddDays(rng.Next(60, (DateTime.Today - incorp).Days));
                var modified = rng.Next(2) == 0 ? created.AddDays(rng.Next(30, 300)) : new DateTime(1800, 1, 1);
                return new
                {
                    assets_under_charge = " " + assets[rng.Next(assets.Length)],
                    charge_amount = (rng.Next(5, 400) * 100000L).ToString(),
                    date_of_creation = created.ToString("yyyy-MM-dd"),
                    date_of_modification = modified.ToString("yyyy-MM-dd"),
                    status = "OPEN"
                };
            }).ToArray();

            var response = new
            {
                rrn = $"{D(8)}_{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}",
                cin,
                status_code = "200",
                message = new
                {
                    client_id = "company_" + D(8),
                    company_id = cin,
                    company_type = "Company",
                    company_name = companyName,
                    details = new
                    {
                        company_info = new
                        {
                            cin,
                            roc_code = $"RoC-{stateName}",
                            registration_number = D(6),
                            company_category = "Company limited by Shares",
                            class_of_company = isPrivate ? "Private" : "Public",
                            company_sub_category = "Non-govt company",
                            authorized_capital = (rng.Next(1, 50) * 100000L).ToString(),
                            paid_up_capital = (rng.Next(1, 50) * 100000L).ToString(),
                            number_of_members = rng.Next(2, 50).ToString(),
                            date_of_incorporation = incorp.ToString("yyyy-MM-dd"),
                            registered_address = $"Plot {D(2)}, Industrial Area, {stateName} {stateCode}0015 IN",
                            address_other_than_ro = "-",
                            email_id = "info@example.com",
                            listed_status = "Unlisted",
                            active_compliance = (string?)null,
                            suspended_at_stock_exchange = "-",
                            last_agm_date = incorp.AddYears(1).ToString("yyyy-MM-dd"),
                            last_bs_date = incorp.AddYears(1).ToString("yyyy-MM-dd"),
                            company_status = "Active",
                            status_under_cirp = (string?)null
                        },
                        directors,
                        charges
                    }
                },
                tran_ref_no = D(8)
            };

            return JsonConvert.SerializeObject(response);
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
