# Customer Flow — End to End

This is the MSME's journey from first landing on the platform to a credit
officer seeing their Financial Health Card in the IDBI LOS. Each step
names the service/controller responsible and what decision or state
change happens — implementation detail (exact routes, exact view models)
is left to you.

Onboarding is Udyam-first: the Udyam Registration Number is the initial
key, and the Udyam API response supplies enough verified identity data
(name, PAN, mobile, email, bank details, incorporation date, etc.) that
the MSME doesn't have to type in most of it. GSTIN and consent-based
financial data collection happen after registration, inside the
dashboard — not as a gate before account creation.

## Step 1 — Udyam lookup

**Actor:** MSME owner, via IDBI portal or branch-assisted app.

MSME enters their Udyam Registration Number. Nothing else is required
at this stage.

- `UdyamLookupService` calls `IUdyamConnector` with the registration
  number and gets back the full Udyam profile (see field list in
  `05_INTEGRATIONS.md` — enterprise name, owner name, PAN, GSTIN status,
  mobile, email, enterprise type, major activity, official address,
  dates of incorporation/commencement, organization type, and bank
  details).
- This response is held in a short-lived pending state (session, or a
  `PendingRegistration` holding record) — nothing is written to `Msme`
  yet, because the person hasn't consented or confirmed anything.
- The UI shows the fetched details back to the MSME owner as a
  read-only confirmation screen: "here's what we found for this Udyam
  number — is this you?" This is also the natural place to surface the
  consent language for what happens next (data collection, scoring).

**Decision point:** if the Udyam number doesn't resolve (invalid number,
lookup failure, or the `gstinStatus` field indicates no valid linked
GSTIN), stop here with a clear message. Don't fall back to manual GSTIN
entry at this stage — Udyam resolution is the entry gate for this flow.

## Step 2 — Consent + Register

MSME reviews the confirmation screen from Step 1 and clicks **Register**.

- On click, this is the actual consent moment for account creation —
  capture it explicitly (timestamp + what was consented to), since it's
  distinct from the AA financial-data consent that comes later in the
  dashboard.
- `MsmeOnboardingService` checks whether an `Msme` already exists for
  this Udyam number (or its linked GSTIN/PAN). If yes, this isn't a new
  registration — route to Step 3b (existing user) instead of creating a
  duplicate.
- If no existing `Msme`, create one now, populated from the Udyam
  response fields (see `01_DOMAIN_MODEL.md` for the field mapping).
  `BorrowerType` isn't determined yet at this point — leave it
  unset/pending until bureau/AA data is available later.
- Trigger an OTP sent to `emailId` from the Udyam response — not a
  mobile OTP. The email address is Udyam-verified, which is why it's
  trustworthy enough to use as the registration channel here.

## Step 3 — OTP verification → dashboard login

- MSME enters the OTP received by email. On success,
  `MsmeOnboardingService` finalizes the `Msme` record (moves it from
  pending/newly-created to active) and issues the authenticated session
  for dashboard access.
- This is also the point where login credentials are effectively
  established for return visits — decide during implementation whether
  that's email+OTP-on-every-login, or OTP-to-set-a-password-once; either
  is reasonable, but pick one and keep Step 6 (returning user login)
  consistent with it.
- On success, land the MSME on the dashboard. What they see next
  depends on whether this is their first time (Step 4) or a returning
  visit with a score already on file (Step 5).

## Step 3b — Existing user reaches Register

If Step 2 detected an existing `Msme` for this Udyam number, skip
account creation entirely — send the email OTP for *login* (same
mechanism, different intent) and take them straight to Step 5
(dashboard, existing score) once verified. Don't re-run Step 2's
create-account logic against an existing record.

## Step 4 — New user: GSTIN + data collection

First dashboard visit for a newly registered MSME with no score yet.

- The dashboard prompts for GSTIN. Since Udyam's `gstinStatus` field
  may already indicate a linked GSTIN, check that first — if the Udyam
  response already gives us a usable GSTIN, pre-fill it and let the
  MSME confirm rather than retype it. Only fall back to manual entry if
  Udyam didn't supply one.
- Once GSTIN is confirmed, proceed into AA consent request (data window
  6-12 months — see below) and the rest of the multi-source data pull,
  same as previously scoped: GST, ITR, AA bank statements, EPFO, and
  anything Udyam didn't already cover. ITR is keyed off `Msme.PanNumber`
  (already populated from the Udyam response) and, like GST, doesn't
  need a separate consent artifact.
- This stage is where `BorrowerType` (NTC / NTB / ExistingBorrower) gets
  determined, based on whether a bureau record is found during the AA/
  bureau pull.
- **Data window:** the AA consent request, the GST return pull, and the
  ITR pull should all target a 6-12 month / most-recent-available-
  assessment-year history respectively. Where a source has less than
  6 months of history available (a genuinely young business), take
  what's available rather than blocking — this is exactly the
  thin-file case `03_SCORING_ENGINE.md` already handles via
  missing-dimension weight redistribution.
- Once the pull is complete (or reasonably timed out with partial
  data), proceed into the scoring pipeline (Step 7 below).

**Decision point:** same as the previous AA-consent decision point — if
AA consent is rejected or times out, still produce a partial score from
GST + ITR + EPFO + Udyam alone, don't hard-block.

## Step 5 — Existing user: dashboard shows current score

Returning MSME, or a newly-onboarded MSME on any visit after their
first score has been computed.

- Dashboard checks for a current `ScoreComputation` for this `Msme`.
- If one exists: render the Financial Health Card directly (Step 8) —
  no re-collection of data, no repeated consent flow.
- If none exists yet (registered but data collection/scoring hasn't
  completed — e.g. they registered, then dropped off before finishing
  Step 4): resume Step 4 rather than showing an empty dashboard.
- The MSME should also be able to explicitly trigger a refresh
  (re-consent AA, re-pull data) from the dashboard rather than waiting
  for the background refresh job — surface this as a visible action,
  not just an automatic background process.

## Step 6 — Returning login (subsequent visits, any time later)

- MSME enters their Udyam number (or whatever identifier the chosen
  login mechanism from Step 3 uses) and receives an email OTP.
- On verification, route based on score presence exactly as in Step 5 —
  this step and Step 5 are really the same dashboard-entry logic,
  called out separately here only because "returning after weeks" and
  "continuing right after registration" are different user moments
  worth designing for, even though the underlying check is identical.

## Step 7 — Feature engineering + scoring

Runs as part of Step 4 for a new user's first score, and again whenever
a refresh is triggered (Step 5's manual refresh action, or the
background job in Step 10).

- Reads all non-stale `DataSourceSnapshot` rows for the Msme.
- Extracts features per source (see `03_SCORING_ENGINE.md`).
- Runs the ML.NET pipeline (LightGBM sub-scores + SSA trend overlay).
- Writes one new `ScoreComputation`, marks it `IsCurrent = true`, flips
  the previous current computation to `IsCurrent = false` — never
  delete, this preserves trend history.
- Runs the PFI-based explanation step, writes `ScoreExplanation` rows.
- `RecommendationService` maps the resulting score band to
  `CreditProductRecommendation` rows.

**Target:** well under 5 seconds for a demo-realistic data volume.

## Step 8 — Financial Health Card display

- Controller loads the current `ScoreComputation` + its
  `ScoreExplanation` and `CreditProductRecommendation` children, plus
  the last N `ScoreComputation` rows for the trend line.
- Razor view renders: overall score + band, Chart.js radar chart across
  the 6 dimensions, Chart.js line chart for the trend, top strengths/
  risks sorted by `RelativeWeight`, recommended product cards, and a
  freshness indicator sourced from `DataFreshnessAt`.
- PDF export (if built) renders from the same view model.

## Step 9 — IDBI LOS integration

For the credit officer, not the MSME.

- Credit officer opens a loan application in the (external, IDBI-owned)
  LOS. The LOS calls our webhook/API with the MSME's GSTIN, Udyam
  number, or internal ID.
- Our controller resolves the current `ScoreComputation` and returns it
  in the shape the LOS expects (see `04_API_CONTRACTS.md`).
- If no current score exists yet, the API should say so explicitly —
  the LOS can trigger onboarding, or the officer proceeds with
  traditional underwriting.
- Credit officer can add a manual override note, stored against the
  `ScoreComputation` it applies to — never mutates the computed score
  itself.

## Step 10 — Score refresh (background)

- Timer-triggered Azure Function periodically checks for MSMEs whose
  most recent `ScoreComputation.ComputedAt` is older than a threshold
  (e.g. 30 days), or whose `ConsentRecord.DataRangeTo` is approaching
  expiry, and re-triggers the data pull + scoring.
- Also fireable on-demand: a new GST filing detected, a fresh AA
  consent grant, or the MSME's manual refresh action from Step 5.

## Step 11 — ULI/OCEN publication (parallel to Step 9)

Not gated on Step 9 — any OCEN-compliant lender can query a score once
it exists, subject to their own consent/authorization with the MSME.
See `04_API_CONTRACTS.md` for the DSP-facing contract.
