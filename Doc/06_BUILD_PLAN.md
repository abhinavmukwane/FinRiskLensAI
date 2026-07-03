# Build Plan

> **Progress status (2026-07-02):**
> - **Phase 0** — half done: DbContext registered (SQL Server chosen over
>   PostgreSQL — see `07_SETUP_GAPS.md` status note); JWT bearer still open.
> - **Phase 1** — partially done, different entities than planned: `ADM_Login`
>   + the `t_MsmeEnquiry`/`t_MsmeLocations`/`t_MsmeNicCodes` Udyam cache are
>   live on the remote SQL Server; the core domain entities (`Msme`,
>   `ScoreComputation`, …) are still pending.
> - **Phase 4 (the July 9 target) — substantially done, ahead of order:** the
>   `FinRiskLensAI.ML` scoring engine implements all seven items below
>   (LightGBM, redistribution, RandomizedPca anomaly, SSA, PFI explanations,
>   recommendations) and is verified with real sample payloads — see
>   `08_ML_ENGINE.md`. It reads raw payloads (Azure Blob folder per UAN, or
>   request body) instead of `DataSourceSnapshot` rows for now.
> - Also built (not in the original plan): the blob-storage collection flow
>   with manifest gating (`api/msme-data/{uan}`), which handles payloads too
>   large for a single request.
> - **Phase 5 — substantially done (2026-07-03):** the Financial Health Card
>   renders at `GET /Dashboard/FinancialHealthCard?uan={UAN}` from the blob
>   `result.json` — score gauge + band, six-dimension bars with excluded/
>   neutral-default markers, Chart.js radar + earned-vs-max charts,
>   strengths/risks, anomaly box, product recommendation cards, and a
>   "Run AI Analysis Now" action when no result exists yet. Still missing
>   from the Phase 5 scope: the score trend line (needs `ScoreComputation`
>   history) and PDF export (Phase 9).
> - **Phases 2, 3, 6, 7** — not started. Home page UI aligned to the
>   product story. A teammate has started AA connector code
>   (`HttpClientHelper` in Core).

Suggested build order, sequenced so there's always something
demonstrable, and mapped loosely to the IDBI Innovate 2026 timeline
(initial submission July 9, sandbox access July 22-31, final prototype
July 31).

This is a suggested order, not a rigid schedule — adjust based on how
much of each phase Claude Code completes in a session.

## Phase 0 — Close the scaffold gaps

Before building anything new, resolve the two gaps `ARCHITECTURE.md`
itself flags:

1. Register `ApplicationDbContext` with a provider in `Program.cs`,
   driven by the existing `Database:DbType` config switch, targeting
   PostgreSQL (Npgsql) as primary.
2. Register the JWT bearer handler (`AddAuthentication().AddJwtBearer`)
   so the already-configured `Jwt` settings actually activate.

See `07_SETUP_GAPS.md` for specifics on both.

## Phase 1 — Domain + persistence

Implement the entities from `01_DOMAIN_MODEL.md` in `Core`, their EF
Core configurations in `Data`, and confirm migrations apply cleanly
against PostgreSQL. No business logic yet — just get the schema real
and queryable.

**Demonstrable at end of phase:** can create/read an `Msme` row and its
related entities through the generic repository, verified via a
throwaway test or a minimal controller action.

## Phase 2 — Synthetic data generator + connector interfaces

Build the five connector interfaces (`05_INTEGRATIONS.md`) and the
synthetic data generator implementing all of them. This unblocks
everything downstream without waiting for sandbox access.

**Demonstrable:** calling any connector interface for a fixture Udyam
number (and its linked GSTIN, for the GST connector) returns realistic,
varied data across the five profile types described in
`05_INTEGRATIONS.md`.

## Phase 3 — Onboarding, registration, and consent flow (Steps 1-6)

`UdyamLookupService`, `MsmeOnboardingService`, `ConsentOrchestrationService`,
and the controllers/views for: Udyam number entry + confirmation screen
(Step 1), Register + email OTP dispatch (Step 2), OTP verification into
the dashboard (Step 3), the existing-user branch at Register (Step 3b),
GSTIN confirmation/entry plus AA consent disclosure inside the
dashboard (Step 4), the dashboard's "show current score or resume
onboarding" branch (Step 5), and returning-user login (Step 6). Build
against the synthetic Udyam and AA connectors for now.

**Demonstrable:** an MSME can go from entering only a Udyam number,
through Register and email OTP, to a dashboard that either prompts for
GSTIN (first-time) or shows an existing score (returning user) —
resulting in a persisted `Msme` populated from the synthetic Udyam
response, and a `ConsentRecord` once AA consent is requested.

## Phase 4 — Feature engineering + scoring engine (Step 7)

This is the core IP of the product — give it the most iteration time.

1. Feature extraction per dimension, from `DataSourceSnapshot` rows.
2. ML.NET LightGBM sub-score models — can be trained on the synthetic
   generator's output initially.
3. Missing-dimension weight redistribution logic
   (`03_SCORING_ENGINE.md`).
4. GST-vs-bank-credit anomaly check (RandomizedPca).
5. SSA trend features feeding into Revenue Vitality / Cash Flow Health.
6. PFI-based explanation generation, translated to
   `ScoreExplanation` rows via the template/lookup approach.
7. `RecommendationService` score-band → product mapping.

**Demonstrable:** given any synthetic MSME profile, running the scoring
pipeline produces a plausible `ScoreComputation` with sensible
sub-scores, explanations that make sense for that profile, and an
appropriate product recommendation — including the NTC profile scoring
reasonably despite having no bureau data, and the anomalous profile
surfacing the fraud flag.

**This phase is the initial-submission (July 9) target** — you don't
need sandbox data or a polished UI yet, but you do need a working,
explainable scoring pipeline to describe and screenshot for the
submission.

## Phase 5 — Financial Health Card UI (Step 8)

Razor views + Chart.js: radar chart, trend line, strengths/risks list,
recommendation cards, freshness indicator. Build against Phase 4's
output.

**Demonstrable:** the full Health Card renders correctly for each
synthetic profile type, including the reduced-dimension case (AA
consent not granted) showing its caveat clearly.

## Phase 6 — External API surface (Steps 9, 11)

The ULI DSP-compatible score API and the IDBI LOS webhook API from
`04_API_CONTRACTS.md`. JWT auth (from Phase 0) protects these. mTLS/
OAuth2 gateway concerns are Azure API Management configuration, not
application code — note that in the demo materials rather than trying
to fully simulate it locally.

**Demonstrable:** an authenticated external call retrieves a score for
a known GSTIN, and a clear "not assessed yet" response for an unknown
one.

## Phase 7 — Background refresh (Step 10)

Azure Functions: queue-triggered pull-on-consent, timer-triggered
staleness sweep, HTTP-triggered AA callback receiver. Wire these against
whichever connectors are live at this point (synthetic or sandbox,
depending on timing).

## Phase 8 — Sandbox integration (July 22-31)

Swap synthetic connector implementations for real sandbox calls behind
the same interfaces from Phase 2. Re-run Phase 4's scoring pipeline
against real sandbox data, sanity-check the outputs, retrain/recalibrate
if the real data distribution looks meaningfully different from the
synthetic fixtures.

## Phase 9 — Polish for final submission (through July 31)

PDF export of the Health Card, officer override notes on the LOS
integration, demo video recording, README/setup instructions for the
GitHub repo, and any UI polish. Don't start this phase early at the
expense of Phase 4/5 — a rough-but-correct scoring engine beats a
polished UI around a shallow one, for this problem statement
specifically.
