# Session Handoff — FinRiskLensAI (as of 2026-07-02)

Context file for Claude Code. This summarizes the project state, decisions made,
and working conventions from the development session on the office PC, so work can
continue seamlessly. Read `Doc/00_README.md` → `Doc/08_ML_ENGINE.md` for the full
product/business context; this file covers what's *built and decided*.

## What this project is

MSME Financial Health Score platform for **IDBI Innovate 2026** (deadline **July 31,
2026**). Aggregates alternate data (Udyam, GST, ITR, AA bank statements; EPFO later)
for New-to-Credit MSMEs, computes a 6-dimension 0–1000 score with ML.NET, and serves
it to IDBI's LOS and the ULI/OCEN ecosystem. ASP.NET Core 8 MVC, EF Core 8, Autofac,
Serilog. Layering: Core → Data → Services → Web, plus a new ML project.

## Current state — everything below is BUILT, TESTED, and PUSHED to main

### 1. Database (SQL Server, remote)
- Connection: remote SQL Server at `103.21.58.192` (moved from 4.247.173.231 on
  2026-07-03), DB `FinRiskLensAI`, SQL auth (login `FinRiskLensAI`) — full string in
  `FinRiskLensAI/appsettings.json` (`Database:ConnectionStrings:SqlServer`) and
  mirrored in `ApplicationDbContextFactory`. `TrustServerCertificate=True` required.
  Tables land in the login's default schema `FinRiskLensAI`, not `dbo`. PostgreSQL
  branch kept in code but unused.
- **PK convention (2026-07-03): int identity, named `<EntityName>ID`**
  (`AdmLoginID`, `MsmeEnquiryID`, …) — NOT Guid, NOT a shared base `Id`.
  `BaseEntity` was deleted; `AuditableEntity` has audit fields only; no
  `IsDeleted`/soft-delete. `IRepository<T>.GetByIdAsync` takes `int`. Migrations
  were re-baselined to a single `InitialCreate` under this convention.
- ⚠ `m_StaticResponces` table exists in the DB but its entity/config/migration code
  was never pushed to git (created directly by a teammate). It must be re-added as
  an entity with `StaticResponcesID` (int identity) under the new convention.
- DbContext registered via `DataServiceCollectionExtensions.AddAppDbContext()` (in
  Data project), called from `Program.cs`. Generic `IRepository<T>` →
  `RepositoryBase<T>` registered in `DataModule`.
- EF migrations live in `FinRiskLensAI.Data/Migrations/SqlServer/` (design-time
  factory fixed — no separate migrations assemblies). History table: `cre.__EFMigrationsHistory`.
- **Tables applied to the remote DB:**
  - `ADM_Login` — admin login (Username/Email unique, PasswordHash, Role, IsActive,
    LastLoginAt). Seeded admin: username `admin`; the password was shared with
    Abhinav during the session (ask him — deliberately not written here). Hash is
    ASP.NET Identity V3 PBKDF2 format (verify with `PasswordHasher<T>`).
  - `t_MsmeEnquiry` — Udyam API response cache, one row per UAN (unique index).
    Columns from `main_details` (excl. `enterprise_type_list`) + full raw JSON in
    `Payload` (nvarchar(max)). Cache flow: hit table by UAN first, call API only on miss.
  - `t_MsmeLocations` / `t_MsmeNicCodes` — child tables (FK `MsmeEnquiryId`, cascade)
    for `location_of_plant_details` and `nic_code` arrays.
- Entities in `Core/Models/{Admin,Onboarding}`, configs in `Data/Configurations/`,
  all derive from `AuditableEntity`.
- **Migration commands** (run from the solution root, `F:\FinRiskLensAI_Git`; needs
  the `dotnet-ef` global tool — `dotnet tool install --global dotnet-ef`):

  ```powershell
  # add a new migration (SQL Server is the active provider)
  dotnet ef migrations add <MigrationName> --project FinRiskLensAI.Data --startup-project FinRiskLensAI --output-dir Migrations/SqlServer

  # apply pending migrations to the database in appsettings/factory
  dotnet ef database update --project FinRiskLensAI.Data --startup-project FinRiskLensAI

  # check which migrations are applied vs pending
  dotnet ef migrations list --project FinRiskLensAI.Data --startup-project FinRiskLensAI

  # undo the last (unapplied) migration
  dotnet ef migrations remove --project FinRiskLensAI.Data --startup-project FinRiskLensAI
  ```

  `--output-dir Migrations/SqlServer` matters — keeps SQL Server migrations separate
  from the (unused) PostgreSQL folder. Design-time commands use the connection string
  in `ApplicationDbContextFactory` (Data project), runtime uses `appsettings.json` —
  both currently point at the same remote server; keep them in sync when it changes.
  Note: `database update` runs against the REMOTE shared DB — teammates share it.

### 2. ML.NET scoring engine — `FinRiskLensAI.ML` project (see Doc/08_ML_ENGINE.md)
- Feature extractors (Newtonsoft `JObject`, tolerant probing) for Udyam, GST, ITR
  (handles ITR-1 and ITR-3 shapes across up to 3 years), AA (dedupes txns by
  `txnId` — same txns appear under multiple FIPs in real responses).
- GST covers six return types: taxpayer profile, monthly GSTR-3B (turnover),
  GSTR-1 summary (B2B share/counterparties), GSTR-1 B2B/e-invoices, GSTR-1 CDNR
  (credit-note revenue reversals), GSTR-1 HSN (product-mix diversity), GSTR-2A
  (purchase-to-sales trade cycle). **Real GST API responses are wrapped in a
  `response.message.data` envelope** — `GstFeatureExtractor.Unwrap()` handles
  wrapped and bare payloads.
- Six dimensions with doc weights (25/20/15/15/15/10), missing-source weight
  redistribution, NTC neutral default on Debt Serviceability (never zero).
  Missing sources feed neutral values into the ML vector too, not zeros.
- ML.NET: LightGBM calibration on a 15-feature vector (lazy-trained on 3000
  synthetic profiles; final score = 0.6 heuristic + 0.4 model), per-instance
  permutation importance → fixed template explanations, SSA cashflow trend,
  RandomizedPca 3-way income anomaly check (GST vs ITR vs bank credits —
  data-integrity signal, separate from score).
- Verified twice: demo payloads **748/1000 Good**; real 6-month GST data for
  UDYAM-MH-20-0067394 (UCN Fibrenet) **719/1000 Good** (₹2.99 cr/mo turnover,
  Compliance 150/150). Known gap: that folder's aa.json is demo data mismatched
  to the business; anomaly score 0.426 sat just under the 0.5 flag threshold —
  tune thresholds when real paired AA data exists.
- **Clean layering (2026-07-04):** all interfaces/models/enums moved to Core —
  `Core/Interfaces` (IRiskScoringService, IBlobAnalysisService, IMsmeDataStore),
  `Core/Models/Scoring` (RiskAnalysisResult, MsmeFeatureSet, LendingAssessment),
  `Core/Models/Storage` (MsmeDataManifest, MsmeDataFiles). ML project holds
  implementations only (ScoreFeatureVector stays in ML — it carries an ML.NET
  attribute).
- **Lending assessment (stage 8, `LendingCalculator`):** 7 bank ratios (DSCR,
  FOIR, banking penetration, gross margin, days-cash, inflow CV, credit-note
  ratio) with benchmarks + Strong/Adequate/Weak status, and indicative
  eligibility — WC via turnover/Nayak method (20% of turnover × band factor),
  term loan via EMI-headroom annuity (5y @ 11%). Formulas in
  `Doc/08_ML_ENGINE.md` Stage 8.
- **Underwriting deep-dives (stage 9, 2026-07-04):** four sections on every
  result — `BankStatementAnalysis` (AMB/peak from running balances, channel
  split, cash/salary/customer/supplier classification, cheque vs ECS-NACH
  returns, OD + min-balance breaches), `GstDeepDive` (GSTR-1 vs 3B consistency,
  cash-vs-ITC tax discipline, net ITC/mo, top-5 customer/vendor concentration
  with masked GSTINs), `FinancialRatios` (EBITDA/net margin from ITR-3 P&L,
  debtor days/asset turnover from ITR balance sheet, N/A when absent),
  `IndustryRiskInfo` (NIC-2 → static sector weight table, feeds 10% of Business
  Stability). Appraisal table now 12 ratios. Calculation formulas:
  `Doc/08_ML_ENGINE.md` Stages 8-9.
- **Financial Health Card dashboard built:**
  `/Dashboard/FinancialHealthCard?uan=…` (DashboardController + Chart.js) —
  score gauge with needle, dimension radar/bars, strengths/risks, anomaly box,
  Bank Lending Assessment section (eligibility tiles, ratio table, cashflow/EMI
  chart, methodology notes), run-analysis button for folders without a result.

### 3. Two API entry paths
- `POST /api/scoring/analyze` (`ScoringController`) — payloads in the body. Kept
  deliberately as the stateless dev/test/what-if harness; NOT the production path.
- **Blob flow (primary)** — `api/msme-data/{uan}`: `PUT /manifest` (declare expected
  files), `PUT /files/{fileName}` (upload one raw payload at a time), `GET /status`,
  `POST /analyze?force=` (409 + missing list until manifest satisfied; force analyzes
  partial data), `GET /result`. Azure Blob container `msme-data`, folder per UAN,
  file conventions in `MsmeDataFiles` (`udyam.json`, `itr.json`, `aa.json`,
  `gst_taxpayer.json`, `epfo.json`, monthly GST files `gstr3b_MMyyyy.json`,
  `gstr1_summary_MMyyyy.json`, `gstr1_b2b_Invoice_MMyyyy.json`,
  `gstr1_cdnr_MMyyyy.json`, `gstr1_hsn_summary_MMyyyy.json`,
  `gstr2a_b2b_MMyyyy.json`, plus `_manifest.json` and `result.json`).
  Engine writes `result.json` back; manifest lifecycle Collecting→Processing→Completed/Failed.
- Azure Storage connection string in `appsettings.json` under `AzureBlob:*`
  (account `tflgspblobstorage`). Verified end-to-end against the real account;
  test folder `UDYAM-MH-20-0033382` may still exist there (contains real personal
  data — deletable).

### 4. UI
- Home page (`Views/Home/Index.cshtml`) rewritten to match the docs: six-dimension
  section with real weights, Udyam-first onboarding CTAs → `/Onboarding/CustOnboarding`.
- All icons are Bootstrap Icons (Material Symbols removed, including the font link).
- `.brandbar` header is sticky (both `Index.css` and `CustomerOnboarding.css`),
  `section[id] { scroll-margin-top: 90px }` keeps anchor targets clear of it.

## Decisions & conventions (IMPORTANT)

- **Never add a Claude co-author line to commits.** Author is Abhinav only.
- Commits go **directly to main** (no PR flow); a teammate also pushes — pull
  `--rebase` before pushing.
- `Json files/` at repo root is **git-ignored on purpose**: it holds REAL personal
  data (PAN, Aadhaar, ITR, bank statements). Never commit it; sanitized samples
  would go elsewhere (e.g. `Doc/sample-payloads/`).
- Credentials (SQL `sa`, Azure storage key, JWT key) live in `appsettings.json` —
  accepted hackathon trade-off; rotate/move to a secret store before anything public.
- Follow repo ground rules in `Doc/00_README.md` (strict layering, `*Repository`/
  `*Service` naming for Autofac auto-registration, decimals 18,4) — EXCEPT the
  entity-base rule, which changed on 2026-07-03: entities derive from
  `AuditableEntity` (audit fields only) and declare their own int identity PK
  named `<EntityName>ID`. `BaseEntity` no longer exists.

## Gotchas discovered (save yourself the debugging)

- PowerShell 5.1: `Get-Content -Raw` + `ConvertTo-Json` mangles JSON strings into
  objects (ETS metadata) → use `[IO.File]::ReadAllText(...)`. Also `$home` is a
  reserved read-only variable.
- The remote SQL server needs SQL auth (`user id=sa`) — `Trusted_Connection=True`
  fails with "untrusted domain".
- `dotnet ef` needed `Microsoft.EntityFrameworkCore.Design` in the WEB csproj
  (startup project), tool version 10.x works against EF Core 8.
- First call to a scoring endpoint pays ~1–2s of lazy LightGBM/PCA training; all
  later calls are instant.
- Real GST API responses are enveloped (`response.message.data`) — never parse
  GST payloads from the root without going through `GstFeatureExtractor.Unwrap()`.
- Missing data sources must feed NEUTRAL values into the ML feature vector, not
  zeros — a zero reads as worst-case behavior (this bit us: "no ITR" produced a
  bogus "ITR filed late" explanation until fixed).

## Likely next steps (not started)

1. Wire the data-pull services (teammate added AA/HttpClientHelper code in Core) to
   save API responses straight to blob via `IMsmeDataStore.UploadAsync` — Option A
   discussed; blob side is ready.
2. Admin login controller/service/UI against `ADM_Login` (+ JWT bearer registration —
   gap #2 in `Doc/07_SETUP_GAPS.md` still open).
3. `UdyamLookupService` with the t_MsmeEnquiry cache-or-API flow.
4. EPFO extractor when the data source arrives (slot exists: `EpfoJson`,
   `epfo.json`, `HasEpfo`).
5. ~~Financial Health Card dashboard~~ — DONE (see ML engine section above);
   remaining UI work: score trend line once historical computations exist.
6. Map `RiskAnalysisResult` onto `ScoreComputation`/`ScoreExplanation`/
   `CreditProductRecommendation` entities (not created yet) per `Doc/01_DOMAIN_MODEL.md`.
7. Swap synthetic training data for IDBI sandbox data when it opens **July 22**.
8. Anomaly-detector thresholds need tuning with realistic data (current test mixed
   one company's GST with personal ITR/AA, so cross-source figures were incoherent).
