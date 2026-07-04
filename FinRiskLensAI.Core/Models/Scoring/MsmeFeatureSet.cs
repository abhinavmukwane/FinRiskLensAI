namespace FinRiskLensAI.Core.Models.Scoring
{
    /// <summary>
    /// All engineered features for one MSME, extracted from the raw source payloads
    /// (Udyam, GST, ITR, AA). Availability flags drive the missing-dimension weight
    /// redistribution described in Doc/03_SCORING_ENGINE.md.
    /// </summary>
    public class MsmeFeatureSet
    {
        // ── Source availability
        public bool HasUdyam { get; set; }
        public bool HasGst { get; set; }
        public bool HasItr { get; set; }
        public bool HasAa { get; set; }
        public bool HasEpfo { get; set; }     // data source arrives later — always false for now
        public bool HasBureau { get; set; }   // NTC borrowers have no bureau record

        // ── Udyam (identity / stability)
        public double BusinessVintageMonths { get; set; }
        public string EnterpriseType { get; set; } = string.Empty;   // Micro / Small / Medium
        public string MajorActivity { get; set; } = string.Empty;
        public int PlantLocationCount { get; set; }
        public int NicCodeCount { get; set; }
        /// <summary>Leading 2 digits of the primary NIC code (sector classification key).</summary>
        public string? SectorNic2 { get; set; }
        public string SectorName { get; set; } = string.Empty;
        /// <summary>Sector risk weight 0..1 (higher = more favourable industry outlook).</summary>
        public double SectorRiskWeight { get; set; } = 0.65;

        // ── GST (revenue + compliance)
        /// <summary>Monthly outward taxable turnover keyed by return period (yyyyMM sortable).</summary>
        public SortedDictionary<string, double> GstMonthlyTurnover { get; } = new();
        public double GstTurnoverTrendSlope { get; set; }        // normalized slope, -1..1
        public double GstFilingRegularity { get; set; }          // periods filed / periods expected, 0..1
        public double GstB2bShare { get; set; }                  // B2B taxable value / total, 0..1
        public int GstCounterpartyCount { get; set; }
        public bool GstRegistrationActive { get; set; }
        public double GstAnnualisedTurnover { get; set; }
        /// <summary>Credit/debit note value (GSTR-1 CDNR) relative to turnover — high = heavy revenue reversals.</summary>
        public double GstCreditNoteRatio { get; set; }
        /// <summary>GSTR-2A inward (purchase) value relative to outward turnover — sanity band for a real trading business.</summary>
        public double GstPurchaseToSalesRatio { get; set; }
        /// <summary>Distinct HSN/SAC codes sold (GSTR-1 HSN summary) — product/service mix diversity.</summary>
        public int GstHsnProductCount { get; set; }
        public double GstMonthlyAvgPurchases { get; set; }

        // ── GST deep-dive
        /// <summary>GSTR-1 vs GSTR-3B declared-sales agreement, 0..1 (1 = identical).</summary>
        public double GstR1Vs3bConsistency { get; set; }
        public bool GstHasTaxPaymentData { get; set; }
        public double GstCashTaxShare { get; set; }              // cash / (cash + ITC), 0..1
        public double GstItcMonthlyAvg { get; set; }
        public double GstTopCustomerShare { get; set; }          // 0..1 of B2B sales
        public double GstTopVendorShare { get; set; }            // 0..1 of purchases
        public Dictionary<string, double> GstTopCustomers { get; } = new();
        public Dictionary<string, double> GstTopVendors { get; } = new();

        // ── ITR (income + compliance)
        /// <summary>Gross total income keyed by assessment year label (e.g. "2024-2025").</summary>
        public SortedDictionary<string, double> ItrYearlyIncome { get; } = new();
        public double ItrIncomeTrendSlope { get; set; }          // normalized slope, -1..1
        public double ItrFilingTimeliness { get; set; }          // on-time filings / total, 0..1
        public int ItrYearsFiled { get; set; }
        public double ItrLatestIncome { get; set; }

        // ── ITR financial ratios (business filers with books — ITR-3/5/6 only)
        public string? ItrFinancialsYear { get; set; }
        public double ItrBusinessTurnover { get; set; }
        public double? ItrEbitdaMargin { get; set; }
        public double? ItrNetProfitMargin { get; set; }
        public double? ItrDebtorDays { get; set; }
        public double? ItrAssetTurnover { get; set; }

        // ── AA bank statements (cash flow + transaction quality + debt)
        public SortedDictionary<string, double> AaMonthlyCredits { get; } = new();
        public SortedDictionary<string, double> AaMonthlyDebits { get; } = new();
        public double AaCurrentBalanceTotal { get; set; }
        public double AaAvgMonthlyCredit { get; set; }
        public double AaInflowVolatility { get; set; }           // coefficient of variation of monthly credits
        public double AaDaysCashOnHand { get; set; }
        public int AaBounceCount { get; set; }
        public double AaUpiTxnShare { get; set; }                // UPI txns / all txns, 0..1
        public int AaUpiCounterpartyCount { get; set; }
        public double AaRepeatPayerRatio { get; set; }           // repeat credit payers / credit txns, 0..1
        public double AaEmiMonthlyOutflow { get; set; }
        public double AaEmiToInflowRatio { get; set; }
        public int AaTransactionCount { get; set; }
        public int AaMonthsCovered { get; set; }
        public double AaCashflowTrendSlope { get; set; }         // filled by the SSA/trend analyzer, -1..1

        // ── AA deep-dive (bank statement behaviour)
        public SortedDictionary<string, double> AaMonthlyAvgBalance { get; } = new();
        public double AaAvgMonthlyBalance { get; set; }          // AMB across the window
        public double AaPeakBalance { get; set; }
        public double AaCashDepositsMonthly { get; set; }
        public double AaSalaryCreditsMonthly { get; set; }
        public double AaCustomerReceiptsMonthly { get; set; }
        public double AaSupplierPaymentsMonthly { get; set; }
        public int AaChequeReturnCount { get; set; }
        public int AaEcsNachReturnCount { get; set; }
        public double AaOdLimitTotal { get; set; }
        public int AaOverdrawnTxnCount { get; set; }             // balance snapshots below zero
        public int AaMinBalanceBreachCount { get; set; }         // balance snapshots below threshold
        public Dictionary<string, double> AaModeSplitAmount { get; } = new();

        // ── Cross-source income signals (for the fraud/anomaly check)
        public double GstMonthlyAvgTurnover { get; set; }
        public double ItrMonthlyAvgIncome { get; set; }
        public double AaMonthlyAvgBankCredit { get; set; }
    }
}
