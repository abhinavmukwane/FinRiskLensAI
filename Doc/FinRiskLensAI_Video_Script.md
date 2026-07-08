# FinRiskLens AI — Demo Video Voiceover Script (v2 — expanded ML/scoring detail)

**IDBI Innovate 2026 | Track 03: Financial Inclusion, Digital Lending & Credit Decisioning**
**Team: ASM FinTech Developer**

**Target length:** ~7:30 as written. ✂️ trim markers show what to cut if you need it shorter — cutting all ✂️ sections gets you to ~5:30, and a hard cut of the ML Engine section down to its first paragraph gets you to ~4:00.
**Pace guide:** ~140 wpm. Leave a 1–2s pause at every screen transition.

---

## 0:00–0:20 — Opening Hook
**[SCREEN: title card or landing page hero]**

> "India has over 6 crore MSMEs — but most of them are invisible to formal credit. No bureau file, inconsistent bookkeeping, and lenders forced to reject viable businesses simply because they can't 'prove' themselves the traditional way. This is the gap FinRiskLens AI closes."

---

## 0:20–0:45 — Solution Overview
**[SCREEN: Landing page — hero + six dimension cards]**

> "FinRiskLens AI is an AI/ML-driven MSME Financial Health Card. It aggregates alternate data — GST returns, Income Tax filings, Account Aggregator bank statements, and Udyam registration — and computes a single, explainable, multidimensional credit health score. No bureau file needed. Just consent, and five verified data sources."

---

## 0:45–1:30 — Onboarding Flow
**[SCREEN: Enter Udyam Number → "Initiating the Udyam check…" → Review & Proceed → OTP Verification]**

> "Onboarding starts with just one number — the Udyam Registration Number. The system fetches enterprise and PAN records automatically, validates the format, and pre-fills business identity, GSTIN, PAN, and a geotagged registered location — cutting manual data entry to almost zero. The business owner reviews their details, gives explicit consent to access their MSME credit profile, and confirms identity with a one-time email OTP."

---

## 1:30–1:50 — Login, Security & Accessibility
**[SCREEN: Login page with language dropdown → Source IP Security Audit modal]**

> "Every login runs through a real-time source IP security audit — checking VPN status, proxy usage, Tor exit nodes, and bot traffic — so lenders can trust that the applicant is genuine. And because financial inclusion means language inclusion too, the entire platform is available in seven Indian languages, powered by Bhashini."

✂️ *(Trim option: cut the last sentence if running long.)*

---

## 1:50–2:45 — Consent-First Data Aggregation
**[SCREEN: Dashboard "Let's get started" → GST fetch modal → ITR fetch modal → AA consent modal → Finvu consent screen → Consent Complete]**

> "From the dashboard, the enterprise connects three consent-based data sources. GST returns verify tax compliance and turnover. Income Tax Returns confirm declared income and financial stability. And through the RBI-regulated Account Aggregator framework — powered by Finvu — the business securely shares six to twelve months of bank statements, with full transparency on what's shared, how it's used, and for how long. Every consent is explicit, auditable, and revocable at any time."

---

## 2:45–3:00 — In-App AI Assistant
**[SCREEN: chat widget on dashboard]**

> "An in-app AI assistant is available throughout, ready to explain any score, status, or number on the dashboard — in plain language, for a founder, not a data scientist."

✂️ *(Trim option: cut this whole section if you need time back.)*

---

## 3:00–3:45 — Deep-Dive Analytics
**[SCREEN: Udyam Details page → GST Analysis page]**

> "Behind the scenes, each data source is independently analyzed. The Udyam Analysis engine scores business identity and stability — flagging strengths like operating history, GST registration, and PAN verification. The GST Intelligence engine goes further: filing consistency, ITC utilization, vendor concentration risk, cash-flow direction, and fraud auditing for circular invoicing or fake credit claims — with a 12-month predictive revenue outlook and a confidence-scored AI assessment rationale."

---

## 3:45–5:25 — NEW: Inside the ML Engine — How the Score Is Actually Computed
**[SCREEN: optional architecture/pipeline diagram, or hold on GST Analysis / Udyam pages while narrating; cut to Financial Health Card just before this section ends]**

> "Now — what's actually happening under the hood. Every score starts from raw Udyam, GST, ITR, and Account Aggregator API responses, broken down by dedicated feature extractors into roughly thirty engineered signals: turnover trend, filing regularity, ITC claim behavior, bank inflow volatility, counterparty diversity, and more.
>
> Each of the six dimensions starts as a transparent, auditable rules-engine formula — nothing hidden. If a data source is missing, say AA consent wasn't granted, we never zero that dimension out. Its weight redistributes proportionally across what is available, and the exclusion is reported openly on the card, so a credit officer never mistakes a partial score for a complete one. And for a true New-to-Credit borrower with no bureau file, Debt Serviceability defaults to a neutral midpoint, not a penalty — because absence of credit history isn't bad credit.
>
> Those rule-based scores are then calibrated by a LightGBM model trained in ML.NET — the final score blends sixty percent rules engine with forty percent machine-learning judgment, so it stays explainable but still captures patterns a fixed formula alone would miss. A separate ML.NET SSA time-series model reads bank inflows to score whether cash flow is accelerating or decelerating, with an analytical fallback for thin-file businesses under eight months of data. And a RandomizedPca anomaly detector cross-checks three independent income signals — GST turnover, ITR income, and actual bank credits — together with their ratios. When two sources agree and a third diverges sharply, that's flagged as a data-integrity signal for manual review, kept completely separate from the creditworthiness score itself.
>
> And every result ships with an explanation, not just a number: permutation feature importance identifies exactly which signals moved each dimension for this specific business, translated through a fixed template dictionary into plain, audit-reviewable sentences — like 'GST returns filed for every expected period' — never freeform, unverifiable AI text."

✂️ *(Trim option: keep only the first two paragraphs — rules engine + missing-data fairness — and cut the LightGBM/SSA/anomaly/PFI paragraph if you're tight on time. That still shows judges you understand explainability and fairness, just not the full ML stack.)*

---

## 5:25–7:00 — The Financial Health Card (Real Result Walkthrough)
**[SCREEN: "Run AI Analysis Now" → full Financial Health Card result]**

> "Here's a real run against Gupta Enterprises: an overall score of 722 out of 1000 — a 'Good' band — blending a 724 rules-engine score with a 719 ML-calibrated score across forty-five data files. Six dimensions, each with its points and its maximum: Revenue Vitality 165 of 250, Cash Flow Health 169 of 200, Transaction Trustworthiness 70 of 150, Compliance Quotient 112 of 150, Business Stability 128 of 150, and Debt Serviceability 80 of 100 — visualized as a radar chart and a bar comparison against the maximum possible.
>
> And the card doesn't stop at a score — it becomes an actual lending assessment. A working capital limit of six point oh nine crore, using the RBI's Nayak Committee turnover method; five point five lakh in term loan capacity; six point one five crore total indicative eligibility — scaled by the score band, never a sanction, always labelled indicative. Underneath that sit fourteen credit-appraisal ratios — DSCR, FOIR, banking penetration, GSTR-1 versus GSTR-3B consistency, ITC claimed versus available — each benchmarked Strong, Adequate, or Weak, so a credit officer sees exactly why the number is what it is, not just what the number is."

✂️ *(Trim option: cut the second paragraph's ratio list down to "DSCR, FOIR, and banking penetration" if you need brevity.)*

---

## 7:00–7:20 — Ecosystem Fit & Impact
**[SCREEN: recommended credit products / key strengths panel]**

> "By anchoring on Udyam identity and layering GST, ITR, and Account Aggregator data with AI-driven scoring, FinRiskLens AI is built to plug directly into IDBI's Loan Origination System and the ULI/OCEN network — enabling near real-time credit decisions for New-to-Credit and New-to-Bank MSMEs, without waiting on a credit bureau file."

---

## 7:20–7:40 — Closing
**[SCREEN: FinRiskLens AI logo / tagline]**

> "One score. Five data sources. Zero paperwork. FinRiskLens AI — turning India's credit-invisible MSMEs into credit-ready businesses. Built by team ASM FinTech Developer for IDBI Innovate 2026."

---

### Quick reference: screen-to-timestamp order
1. Landing page hero
2. Onboarding — Udyam number entry → processing → review → OTP
3. Login + language selector + IP security audit
4. Dashboard → GST modal → ITR modal → AA consent modal → Finvu external consent → consent complete
5. AI chat assistant
6. Udyam Details analysis page
7. GST Analysis deep-dive page
8. *(new)* ML engine explainer — pipeline diagram or hold on an analysis screen
9. Financial Health Card (run analysis → full result: score gauge, six dimensions, radar/bar charts, lending assessment, ratio table)
10. Closing card

### Why this section was added
You asked for more depth on score generation and the ML.NET engine specifically — this now covers, in judge-friendly language:
- The 9-stage pipeline (extraction → six dimension scores → weight redistribution → LightGBM calibration → anomaly detection → PFI explanations → recommendations → lending ratios → deep-dives)
- The **fairness mechanics** that are core to the "New-to-Credit" problem statement: never zero a missing dimension, proportional weight redistribution, neutral-default Debt Serviceability for NTC borrowers
- The **actual ML.NET components**: LightGBM (score calibration, 60/40 blend with the rules engine), SSA (cash-flow trend forecasting with a thin-file fallback), RandomizedPca (three-way GST/ITR/bank-credit anomaly detection)
- **Explainability**: permutation feature importance translated through a fixed template dictionary — auditable, not free-text AI generation
- A **real, verified score** (722/Good, 45 files, six dimension breakdown) rather than a hypothetical, which is stronger for judges than describing the mechanism in the abstract
