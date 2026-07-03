# ML Engine — FinRiskLensAI.ML

AI/ML analysis project for the MSME Financial Health Score, built with **ML.NET on
ASP.NET Core 8**. It consumes the raw API responses from Udyam, GST, ITR and the
Account Aggregator (EPFO slot ready), engineers features from them, and produces the
complete Financial Health Card payload: a 0–1000 score, six dimension sub-scores,
plain-language explanations, credit product recommendations, and a cross-source
fraud/anomaly signal — implementing `03_SCORING_ENGINE.md`.

```
FinRiskLensAI.ML
├── Features/            ← one extractor per data source (JSON → numbers)
│   ├── UdyamFeatureExtractor.cs
│   ├── GstFeatureExtractor.cs
│   ├── ItrFeatureExtractor.cs
│   ├── AaFeatureExtractor.cs
│   └── TrendMath.cs
├── MachineLearning/     ← the ML.NET components
│   ├── ScoreCalibrationModel.cs      (LightGBM + permutation importance)
│   ├── CashflowTrendAnalyzer.cs      (SSA time-series)
│   ├── IncomeAnomalyDetector.cs      (RandomizedPca)
│   ├── SyntheticProfileGenerator.cs  (training data until IDBI sandbox opens)
│   └── ScoreFeatureVector.cs         (the 12-feature model input row)
├── Models/              ← MsmeFeatureSet, RiskAnalysisResult
├── Services/            ← RiskScoringService (scoring orchestrator)
│   └── BlobAnalysisService.cs        (blob-folder collection + analysis flow)
├── Storage/             ← Azure Blob Storage integration
│   ├── IMsmeDataStore.cs             (storage abstraction)
│   ├── AzureBlobDataStore.cs         (container client, UAN-named folders)
│   ├── MsmeDataFiles.cs              (file-naming convention)
│   └── MsmeDataManifest.cs           (_manifest.json lifecycle)
└── DI/MLModule.cs       ← Autofac registration
```

NuGet: `Microsoft.ML`, `Microsoft.ML.LightGbm`, `Microsoft.ML.TimeSeries`,
`Azure.Storage.Blobs`, `Newtonsoft.Json`. References only `FinRiskLensAI.Core`
(layering rules respected); the web project references it and exposes the API.

---

## 1. Use cases

| # | Use case | Who | What happens |
|---|----------|-----|--------------|
| 1 | **First score after onboarding** | MSME finishing Step 4 of the customer flow | All collected payloads are posted to the engine; the Financial Health Card is rendered from the result |
| 2 | **NTC (New-to-Credit) scoring** | MSME with no bureau record | Debt Serviceability defaults to a neutral midpoint (never zero) and the result carries a transparency explanation |
| 3 | **Partial-consent scoring** | MSME who rejected AA consent | Cash Flow Health + Transaction Trustworthiness are excluded, their weights redistributed, and `excludedDimensions` says so — a credit officer can't misread a partial score as complete |
| 4 | **Score refresh** | Background job / dashboard refresh button | Same call with fresh payloads; new `ScoreComputation` row, old ones keep the trend history |
| 5 | **Credit officer decisioning (IDBI LOS)** | LOS webhook (Step 9) | Overall score + band + explanations returned in seconds, including the anomaly flag for manual review |
| 6 | **Fraud screening** | Risk team | Three-way GST-vs-ITR-vs-bank-credit check flags data-integrity mismatches separately from creditworthiness |
| 7 | **Product pre-selection** | Lending journey | Band-mapped recommendations (Standard / CGTMSE / MUDRA) with indicative ticket sizes anchored to observed turnover |
| 8 | **Large / staged data collection** | Data-pull services, AA callbacks | Payloads too big or too asynchronous for one request are dropped file-by-file into the MSME's blob folder; analysis auto-gates on the manifest being satisfied (§2b) |

---

## 2. The API — two ways in

There are two entry paths. The direct endpoint is fine for demos and small
payloads; the **blob-storage flow is the primary path** because real GST invoice
dumps and 6–12 months of AA transactions can exceed what one HTTP request should
carry (Kestrel's default body limit is 30 MB, and the whole request would sit in
memory).

### 2a. Direct endpoint (small payloads / demo)

Payloads go in exactly as the upstream APIs returned them — no pre-processing
needed by the caller.

```
POST /api/scoring/analyze
Content-Type: application/json
```

```jsonc
{
  "udyamJson":         "<raw Udyam Aadhaar API response>",
  "itrJson":           "<raw ITR API response — up to 3 assessment years>",
  "aaJson":            "<raw AA FI data response — 6–12 months>",
  "gstTaxpayerJson":   "<raw Search Taxpayer response>",
  "gstr3bJsons":       [ "<one GSTR-3B details response per month, up to 12>" ],
  "gstr1SummaryJsons": [ "<one GSTR-1 summary response per month>" ],
  "gstr1B2bJsons":     [ "<GSTR-1 B2B invoice responses>" ],
  "epfoJson":          null   // slot wired, source arrives later
}
```

Every field is optional — whatever is missing triggers the weight-redistribution
path instead of failing. Response (abridged, from a real run):

```jsonc
{
  "overallScore": 748,
  "scoreBand": "Good",
  "heuristicScore": 776.5,          // rules-engine component
  "mlCalibratedScore": 705,         // LightGBM component
  "cashflowTrendSlope": 0.933,      // SSA trend on monthly bank inflows
  "dimensions": [
    { "dimension": "Revenue Vitality", "score": 197.1, "maxPoints": 250,
      "effectiveWeight": 0.25, "dataAvailable": true, "usedNeutralDefault": false },
    { "dimension": "Debt Serviceability", "score": 80, "maxPoints": 100,
      "effectiveWeight": 0.10, "dataAvailable": true, "usedNeutralDefault": true }
    // ... 4 more
  ],
  "explanations": [
    { "dimension": "Revenue Vitality", "impact": "Positive", "relativeWeight": 1.0,
      "explanationText": "GST turnover is growing month over month" },
    { "dimension": "Debt Serviceability", "impact": "Positive", "relativeWeight": 0.1,
      "explanationText": "No credit bureau record found (New-to-Credit) — this dimension used a neutral default rather than penalizing missing history" }
  ],
  "recommendations": [
    { "productName": "Standard Working Capital Loan", "schemeCode": "Standard",
      "indicativeAmountMin": 1900000, "indicativeAmountMax": 3800000,
      "recommendationReason": "Good financial health score supports a standard working capital facility" }
  ],
  "anomaly": { "isAnomalous": false, "anomalyScore": 0.398,
               "gstMonthlyTurnover": 1055609, "itrMonthlyIncome": 22614, "bankMonthlyCredits": 27000 },
  "excludedDimensions": [],
  "modelVersion": "frl-scoring-v1.0"
}
```

### 2b. Blob-storage flow (the primary path — handles any payload size)

Source files are uploaded **one at a time** into an Azure Blob Storage folder named
by the MSME's Udyam number; analysis starts only when everything the folder is
expected to contain has arrived. No single request carries the full data set, and
during analysis only one file is in memory at a time.

```
Container: msme-data                     (config: AzureBlob:ConnectionString / :Container)
└── UDYAM-MH-20-0067394/                 ← folder per MSME, named by UAN
    ├── _manifest.json                   ← control file: what to expect + status
    ├── udyam.json  itr.json  aa.json  gst_taxpayer.json  epfo.json
    ├── gstr3b_MMyyyy.json               ← monthly outward supplies (turnover)
    ├── gstr1_summary_MMyyyy.json        ← section totals (B2B share, counterparties)
    ├── gstr1_b2b_Invoice_MMyyyy.json    ← B2B / e-invoices (counterparty diversity)
    ├── gstr1_cdnr_MMyyyy.json           ← credit/debit notes (revenue reversals)
    ├── gstr1_hsn_summary_MMyyyy.json    ← HSN summaries (product-mix diversity)
    ├── gstr2a_b2b_MMyyyy.json           ← inward purchases (trade-cycle sanity)
    └── result.json                      ← written by the engine after analysis
```

> **Envelope note:** real GST API responses arrive wrapped as
> `{ "response_code": 1, "response": { "message": { "data": { …payload… } } } }`.
> `GstFeatureExtractor.Unwrap()` handles both wrapped and bare payloads, so
> fixtures and real pulls work interchangeably.

Endpoints (all under `api/msme-data/{uan}`, see `MsmeDataController`):

| Verb + route | Purpose |
|---|---|
| `PUT /manifest` | Declare expected files: `{ "udyam": true, "itr": true, "aa": true, "gstTaxpayer": true, "gstMonths": 12, "epfo": false }` |
| `PUT /files/{fileName}` | Upload one raw payload (body = the JSON as-is; 500 MB per-file limit). Response reports what's still missing |
| `GET /status` | Files present/missing, `readyForAnalysis`, manifest status |
| `POST /analyze?force=` | Run the pipeline. Returns `409` + the missing-file list while incomplete; `force=true` analyzes partial data via weight redistribution |
| `GET /result` | Last persisted `result.json` without recomputing |

The manifest is the completeness contract — the engine never guesses whether
"9 GSTR-3B files" means done or still uploading:

```
Status lifecycle:  Collecting → Processing → Completed
                                     └──────→ Failed (error recorded in manifest)
```

`BlobAnalysisService.AnalyzeAsync` verifies the manifest, downloads each blob
sequentially into the matching extractor slot (routing by file-name convention),
runs the same seven-stage pipeline as the direct endpoint, writes `result.json`
back into the folder, and stamps the manifest:

```csharp
foreach (var file in status.FilesPresent.OrderBy(f => f))
{
    if (file.StartsWith(MsmeDataFiles.Gstr3bPrefix, ...))
        request.Gstr3bJsons.Add(await _store.DownloadAsync(uan, file, ct) ?? "");
    // gstr1_summary_* / gstr1_b2b_* route the same way
}
var result = _scoring.Analyze(request);
await _store.UploadAsync(uan, MsmeDataFiles.Result, JsonConvert.SerializeObject(result, ...));
```

Re-running `/analyze` after new files land simply recomputes and overwrites
`result.json` — that is the Step-5 manual refresh from `02_CUSTOMER_FLOW.md`.
Storage is behind `IMsmeDataStore`, so swapping Azure for local disk (tests) is a
DI change, not a code change.

**Who consumes `result.json` (added 2026-07-03):** besides `GET /result`, the
dashboard now renders it directly — `GET /Dashboard/FinancialHealthCard?uan={UAN}`
(`DashboardController`) calls `GetStatusAsync`/`GetResultAsync` on
`IBlobAnalysisService` and draws the full card (gauge, six-dimension bars,
radar/bar charts, explanations, anomaly box, product recommendations). When no
result exists yet, the page surfaces the manifest status and a "Run AI Analysis
Now" button that fires `POST /api/msme-data/{uan}/analyze?force=true` — the
use-case-4 "dashboard refresh button" is now real.

---

## 3. How it works — the pipeline

`RiskScoringService.Analyze()` runs seven stages:

```
raw JSON payloads
      │
      ▼
[1] Feature extraction        4 extractors → MsmeFeatureSet (~30 features + flags)
      │
      ▼
[2] Six sub-scores (0..1)     rules engine per 03_SCORING_ENGINE.md
      │
      ▼
[3] Weight redistribution     missing sources never zero a dimension
      │
      ▼
[4] LightGBM calibration      final = 0.6 × rules + 0.4 × model
      │
      ▼
[5] Anomaly check             RandomizedPca over GST / ITR / bank credits
      │
      ▼
[6] Explanations              permutation importance → template dictionary
      │
      ▼
[7] Recommendations           band → scheme lookup
```

### Stage 1 — Feature extraction

Each extractor parses tolerantly with `JObject` (real payload shapes vary — the two
sample ITRs use ITR-1 and ITR-3 forms). Example from `ItrFeatureExtractor` probing
multiple income paths:

```csharp
var income = FirstNumber(year,
    "$..ITR1_IncomeDeductions.GrossTotIncome",   // ITR-1
    "$..['PartB-TI'].GrossTotalIncome",          // ITR-3
    "$..['PartB-TI'].TotalIncome",
    "$..ITR1_IncomeDeductions.TotalIncome");
```

The AA extractor dedupes transactions by `txnId` — the same consent session can
return identical transactions under multiple FIPs (observed in the sample data):

```csharp
var txnId = txn.Value<string>("txnId") ?? Guid.NewGuid().ToString();
if (!seenTxnIds.Add(txnId)) continue;   // don't inflate monthly aggregates
```

Key engineered features:

| Source | Features |
|--------|----------|
| Udyam | business vintage (months), enterprise class (Micro/Small/Medium), plant + NIC footprint |
| GST — GSTR-3B | monthly turnover series (`osup_det.txval`), turnover trend slope, filing regularity |
| GST — GSTR-1 summary / B2B invoices | B2B share, counterparty count, registration status (taxpayer profile) |
| GST — GSTR-1 CDNR | credit/debit-note value vs turnover (revenue-reversal ratio → Revenue Vitality penalty) |
| GST — GSTR-1 HSN | distinct HSN/SAC codes sold (product-mix diversity → Business Stability footprint) |
| GST — GSTR-2A | inward purchase value, purchase-to-sales ratio (healthy trade-cycle band → Business Stability) |
| ITR | per-year income series, income trend slope, filed-on-time ratio, years filed |
| AA | monthly credit/debit series, inflow volatility (CV), days-cash-on-hand, bounce count, UPI share, counterparty diversity, repeat-payer ratio, EMI-to-inflow ratio |

Missing sources feed **neutral values** into the model vector, never zeros — a
missing ITR must not read as "filed late" (same fairness rule as the dimension
weight redistribution).

### Stage 2 — Six dimensions (weights from the scoring doc)

| Dimension | Weight | Inputs |
|---|---|---|
| Revenue Vitality | 25% | GST turnover trend + consistency, ITR income trend |
| Cash Flow Health | 20% | cash cushion, inflow steadiness, SSA trend, bounce penalty |
| Transaction Trustworthiness | 15% | velocity, counterparty diversity, repeat payers, UPI adoption |
| Compliance Quotient | 15% | GST filing regularity, registration status, ITR timeliness |
| Business Stability | 15% | vintage, enterprise class, footprint (EPFO headcount joins later) |
| Debt Serviceability | 10% | EMI-to-inflow from AA; bureau when available |

Each is a small readable function, e.g.:

```csharp
private static double CashFlowHealth(MsmeFeatureSet f)
{
    if (!f.HasAa) return 0;
    var cushion       = Math.Min(1, f.AaDaysCashOnHand / 90.0);   // 3 months cash = full marks
    var steadiness    = 1 - Math.Min(1, f.AaInflowVolatility);
    var bouncePenalty = Math.Min(0.4, f.AaBounceCount * 0.1);
    var trendBonus    = 0.5 + 0.5 * f.AaCashflowTrendSlope;       // from the SSA analyzer
    return Math.Clamp(0.35*cushion + 0.30*steadiness + 0.20*trendBonus - bouncePenalty + 0.15, 0, 1);
}
```

### Stage 3 — Missing-source handling

Refusing AA consent must not look like bad credit. Weights of unavailable dimensions
are redistributed proportionally and the exclusion is reported:

```csharp
var availableWeight = DimensionDefs.Where(d => d.HasData(features)).Sum(d => d.Weight);
var effectiveWeight = available ? weight / availableWeight : 0;
if (!available) result.ExcludedDimensions.Add(name);
```

NTC borrowers get a neutral midpoint on Debt Serviceability — with a flag:

```csharp
if (!f.HasBureau && !f.HasAa)
{
    usedNeutral = true;
    return 0.5;   // absence of credit history is not bad credit
}
```

### Stage 4 — LightGBM calibration (ML.NET)

A LightGBM regressor maps the 15-feature vector (see
`ScoreFeatureVector.FeatureNames`) to a 0–1000 score. It trains
**lazily, once per process**, on 3,000 synthetic profiles spanning the borrower
spectrum (the doc's synthetic-data strategy until the IDBI sandbox opens July 22):

```csharp
var pipeline = ml.Regression.Trainers.LightGbm(
    labelColumnName: nameof(ScoreFeatureVector.Label),
    featureColumnName: nameof(ScoreFeatureVector.Features),
    numberOfLeaves: 31, numberOfIterations: 150, minimumExampleCountPerLeaf: 20);
```

The final score blends both engines — rules keep it explainable, the model adds
non-linear judgment:

```csharp
result.OverallScore = Math.Round(0.6 * result.HeuristicScore + 0.4 * result.MlCalibratedScore);
```

### Stage 5 — Cross-source anomaly check (ML.NET RandomizedPca)

Three independent income reads — GST-declared turnover, ITR-declared income, actual
bank credits — plus their pairwise ratios go through a PCA-based anomaly detector
trained on consistent profiles. Two agreeing sources + one diverging sharply is the
misrepresentation signature:

```csharp
var pipeline = ml.Transforms.NormalizeMinMax("Features")
    .Append(ml.AnomalyDetection.Trainers.RandomizedPca("Features", rank: 3));
```

The result is a **data-integrity signal, kept separate from the score** — it never
deducts points, it flags the case for officer review.

### Stage 6 — Explanations (PFI → templates)

For this specific MSME, each feature is neutralized one at a time and the score
delta measured — per-instance permutation importance:

```csharp
var perturbed = new ScoreFeatureVector { Features = (float[])input.Features.Clone() };
perturbed.Features[i] = neutral;                       // trends→0, ratios→0.5
impacts.Add((FeatureNames[i], baseline - engine.Predict(perturbed).Score));
```

Top movers are translated through a fixed template dictionary (consistent,
audit-reviewable wording — no free text):

```csharp
["GstFilingRegularity"] = ("Compliance Quotient",
    "GST returns filed for every expected period",      // positive
    "Gaps found in GST return filing history"),         // negative
```

### Stage 7 — Recommendations

Straight band→scheme lookup per the doc; ticket sizes anchor to observed scale
(annualized GST turnover or bank inflows). High Risk gets no product — the weak
dimensions are the actionable output instead.

| Band | Products |
|---|---|
| Excellent / Good | Standard Working Capital, Business Term Loan |
| Fair | CGTMSE-backed Working Capital |
| At Risk | MUDRA (Shishu/Kishor) |
| High Risk | none — surface weak dimensions |

### SSA trend (used inside Stage 2)

Monthly bank inflows run through ML.NET's SSA forecaster; the forecast-vs-recent
delta becomes the trend feature feeding Cash Flow Health:

```csharp
var pipeline = ml.Forecasting.ForecastBySsa(
    outputColumnName: "Forecast", inputColumnName: "Value",
    windowSize: monthlySeries.Count / 3,
    seriesLength: monthlySeries.Count, trainSize: monthlySeries.Count, horizon: 3);
```

Series shorter than 8 months fall back to an analytic least-squares slope — the
thin-file case must still score.

---

## 4. Wiring (DI + web)

`MLModule` (Autofac) registers extractors and models as singletons — the ML models
train once, lazily, on first request:

```csharp
// Program.cs
container.RegisterModule(new MLModule());
```

Model classes are `Lazy<T>`-trained singletons, so the first `/analyze` call pays
~1–2s of training and every call after is instant. To consume from another service:

```csharp
public class ScoreComputationService
{
    private readonly IRiskScoringService _scoring;
    public ScoreComputationService(IRiskScoringService scoring) => _scoring = scoring;

    public RiskAnalysisResult Compute(RiskAnalysisRequest payloads) => _scoring.Analyze(payloads);
}
```

---

## 5. Testing it

### Blob flow (primary)

With the app running, drive the collection + analysis cycle:

```powershell
$api = "http://localhost:5199/api/msme-data/UDYAM-MH-20-0033382"

# 1. Declare what the folder will contain
$manifest = @{ udyam=$true; itr=$true; aa=$true; gstTaxpayer=$true; gstMonths=12; epfo=$false } | ConvertTo-Json
Invoke-RestMethod -Uri "$api/manifest" -Method Put -Body $manifest -ContentType "application/json"

# 2. Upload payloads one file at a time (body = the raw JSON)
Invoke-RestMethod -Uri "$api/files/udyam.json" -Method Put -ContentType "application/json" `
    -Body ([IO.File]::ReadAllText("$base\UdyamAdharResponce.json"))
# ... itr.json, aa.json, gst_taxpayer.json, gstr3b_MMyyyy.json × 12

# 3. Check progress, then analyze
Invoke-RestMethod -Uri "$api/status"
Invoke-RestMethod -Uri "$api/analyze" -Method Post      # 409 + missing list if incomplete
Invoke-RestMethod -Uri "$api/result"                    # persisted result, no recompute
```

**From Postman:** set the method to `POST`, URL
`http://localhost:5199/api/msme-data/{uan}/analyze`, body **none** (everything
comes from the blob folder), Send. Incomplete folders return `409` with the
missing-file list; `GET .../result` returns the saved result without recomputing.

Verified end-to-end against the real storage account twice:
- Demo folder (`UDYAM-MH-20-0033382`, synthetic GSTR-3B): early `/analyze` was
  refused with `Missing: gstr3b_* (0 of 12 months uploaded)`; after all 16 files
  landed it returned **748/1000, band Good**.
- **Real GST data** (`UDYAM-MH-20-0067394`, UCN Fibrenet — 6 months of actual
  GSTR-3B/1/2A/CDNR/HSN pulls, no ITR): **719/1000, band Good** — ₹2.99 cr/month
  average turnover extracted through the response envelope, Compliance Quotient
  150/150, ITR excluded cleanly via neutral defaults. Caveat: the folder's
  `aa.json` was the demo AA file (₹27k/month credits vs ₹2.99 cr GST turnover);
  the three-way anomaly check scored 0.426 — just under the 0.5 flag threshold —
  which is the concrete case to tune thresholds against once real paired AA
  data exists.

### Direct endpoint (small payloads)

```powershell
$request = @{
    udyamJson   = [IO.File]::ReadAllText("$base\UdyamAdharResponce.json")
    itrJson     = [IO.File]::ReadAllText("$base\ITR_Respopnce.json")
    aaJson      = [IO.File]::ReadAllText("$base\AA.json")
    gstr3bJsons = @( <one JSON string per month> )
} | ConvertTo-Json -Depth 4

Invoke-RestMethod -Uri "http://localhost:5199/api/scoring/analyze" `
    -Method Post -Body $request -ContentType "application/json"
```

> PowerShell 5.1 note: use `[IO.File]::ReadAllText(...)`, not `Get-Content -Raw` —
> `Get-Content` attaches provider metadata that makes `ConvertTo-Json` serialize the
> string as an object and the request fails model binding with a 400.

Both paths produce identical results with the same inputs (both were verified at
**748/1000, band Good**: all six dimensions populated, NTC neutral-default flagged
on Debt Serviceability, cashflow trend +0.93).

---

## 6. Extension points

- **EPFO** — `RiskAnalysisRequest.EpfoJson` slot exists; add an `EpfoFeatureExtractor`,
  set `HasEpfo`, and feed headcount trend into Business Stability + contribution
  consistency into Compliance Quotient. Nothing else changes.
- **IDBI sandbox (July 22)** — replace `SyntheticProfileGenerator` output with real
  labeled sandbox rows; the LightGBM pipeline, feature vector, and PFI explanations
  are unchanged.
- **Bureau data** — set `HasBureau` and extend `DebtServiceability()` with utilization;
  the neutral-default path already distinguishes NTC from bureau-backed borrowers.
- **Score bands / weights** — thresholds and weights are constants in
  `RiskScoringService` (`Band()`, `DimensionDefs`); tune once synthetic-data
  distributions are validated, per the scoring doc's warning.
- **Persistence** — map `RiskAnalysisResult` onto the `ScoreComputation` /
  `ScoreExplanation` / `CreditProductRecommendation` entities from
  `01_DOMAIN_MODEL.md` when those tables are added. The blob folder already
  satisfies the `DataSourceSnapshot.RawPayload` guidance ("store large dumps in
  Azure Blob Storage, keep only the URI") — the snapshot rows just need the blob
  paths.
- **Auto-trigger** — today analysis starts via `POST /analyze` (explicit,
  debuggable). A background poller that scans for complete-but-unprocessed
  manifests, or an Event Grid blob-created subscription, can be layered on top of
  `IBlobAnalysisService` without touching the pipeline — that also covers the
  Step-10 background refresh job.
- **Secrets** — the blob connection string and account key live in
  `appsettings.json` (`AzureBlob:*`) for the hackathon build, same as the SQL
  connection string; production moves both to a secret store, per the standing
  warning in `07_SETUP_GAPS.md`.
