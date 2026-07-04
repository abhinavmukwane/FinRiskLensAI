using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinRiskLensAI.Core.Models.Onboarding
{
    public class UdyamResponseModel
    {
        public string client_id { get; set; }
        public string uan { get; set; }
        public string certificate_url { get; set; }

        public MainDetailsModel main_details { get; set; }

        public List<LocationOfPlantModel> location_of_plant_details { get; set; }

        public List<NicCodeModel> nic_code { get; set; }

        public class MainDetailsModel
        {
            public List<EnterpriseTypeModel> enterprise_type_list { get; set; }

            public string name_of_enterprise { get; set; }
            public string major_activity { get; set; }
            public string social_category { get; set; }
            public DateTime? date_of_commencement { get; set; }
            public string dic_name { get; set; }
            public string state { get; set; }
            public DateTime? applied_date { get; set; }
            public string flat { get; set; }
            public string name_of_building { get; set; }
            public string road { get; set; }
            public string village { get; set; }
            public string block { get; set; }
            public string city { get; set; }
            public string pin { get; set; }
            public string mobile_number { get; set; }
            public string email { get; set; }
            public string organization_type { get; set; }
            public string gender { get; set; }
            public DateTime? date_of_incorporation { get; set; }
            public string msme_dfo { get; set; }
            public DateTime? registration_date { get; set; }
            public string gstin { get; set; }
            public string Pan { get; set; }
        }

        public class EnterpriseTypeModel
        {
            public string classification_year { get; set; }
            public string enterprise_type { get; set; }
            public DateTime? classification_date { get; set; }
        }

        public class LocationOfPlantModel
        {
            public string unit_name { get; set; }
            public string line_1 { get; set; }
            public string building { get; set; }
            public string village { get; set; }
            public string street { get; set; }
            public string road { get; set; }
            public string city { get; set; }
            public string pin { get; set; }
            public string state { get; set; }
            public string district { get; set; }
        }

        public class NicCodeModel
        {
            public string nic_2_digit { get; set; }
            public string nic_4_digit { get; set; }
            public string nic_5_digit { get; set; }
            public string activity_type { get; set; }
            public DateTime? added_on { get; set; }
        }
    }
}
