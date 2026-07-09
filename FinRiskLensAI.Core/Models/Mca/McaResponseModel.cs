using System.Collections.Generic;

namespace FinRiskLensAI.Core.Models.Mca
{
    /// <summary>
    /// Strongly-typed MCA (Ministry of Corporate Affairs) company-details API
    /// response, stored in m_StaticResponces.MCAResponce. Property names match
    /// the raw JSON (company_info / directors / charges envelope).
    /// </summary>
    public class McaResponseModel
    {
        public string? rrn { get; set; }
        public string? cin { get; set; }
        public string? status_code { get; set; }
        public McaMessageModel? message { get; set; }
        public string? tran_ref_no { get; set; }

        public class McaMessageModel
        {
            public string? client_id { get; set; }
            public string? company_id { get; set; }
            public string? company_type { get; set; }
            public string? company_name { get; set; }
            public McaDetailsNode? details { get; set; }
        }

        public class McaDetailsNode
        {
            public McaCompanyInfoModel? company_info { get; set; }
            public List<McaDirectorModel>? directors { get; set; }
            public List<McaChargeModel>? charges { get; set; }
        }

        public class McaCompanyInfoModel
        {
            public string? cin { get; set; }
            public string? roc_code { get; set; }
            public string? registration_number { get; set; }
            public string? company_category { get; set; }
            public string? class_of_company { get; set; }
            public string? company_sub_category { get; set; }
            public string? authorized_capital { get; set; }
            public string? paid_up_capital { get; set; }
            public string? number_of_members { get; set; }
            public string? date_of_incorporation { get; set; }
            public string? registered_address { get; set; }
            public string? address_other_than_ro { get; set; }
            public string? email_id { get; set; }
            public string? listed_status { get; set; }
            public string? active_compliance { get; set; }
            public string? suspended_at_stock_exchange { get; set; }
            public string? last_agm_date { get; set; }
            public string? last_bs_date { get; set; }
            public string? company_status { get; set; }
            public string? status_under_cirp { get; set; }
        }

        public class McaDirectorModel
        {
            public string? din_number { get; set; }
            public string? director_name { get; set; }
            public string? start_date { get; set; }
            public string? end_date { get; set; }
            public string? surrendered_din { get; set; }
        }

        public class McaChargeModel
        {
            public string? assets_under_charge { get; set; }
            public string? charge_amount { get; set; }
            public string? date_of_creation { get; set; }
            public string? date_of_modification { get; set; }
            public string? status { get; set; }
        }
    }
}
