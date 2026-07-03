# Domain Model

> **Implementation status (2026-07-02):** the core entities below (`Msme`,
> `ConsentRecord`, `DataSourceSnapshot`, `ScoreComputation`, `ScoreExplanation`,
> `CreditProductRecommendation`) are **not yet implemented**. What exists so far
> (in `Core/Models/`, configured in `Data/Configurations/`, applied to the DB):
> - `AdmLogin` → table `ADM_Login` — admin login (username + password hash),
>   seeded admin user. Supporting table, not part of this domain model.
> - `MsmeEnquiry` → `t_MsmeEnquiry` — Udyam API response cache, one row per UAN
>   (unique), full raw JSON in `Payload`; serves repeat lookups without an API
>   hit. This is the natural feeder for `Msme` when Step 2 (Register) is built.
> - `MsmeLocation` / `MsmeNicCode` → `t_MsmeLocations` / `t_MsmeNicCodes` —
>   children of `MsmeEnquiry` for `location_of_plant_details` and `nic_code`.
> The scoring output shape (`ScoreComputation` etc.) currently exists only as
> the in-memory `RiskAnalysisResult` in `FinRiskLensAI.ML` (see `08_ML_ENGINE.md`)
> plus `result.json` in the MSME's blob folder — mapping it onto these entities
> is a pending step.
>
> **Entity conventions changed (2026-07-03):** this doc's references to
> `BaseEntity` are historical. Entities derive from `AuditableEntity` (audit
> fields only) and declare their own **int identity PK named `<EntityName>ID`**
> (e.g. `MsmeEnquiryID`); Guid keys and `IsDeleted` were removed. When building
> the entities below, follow the new convention (`MsmeID`, `ConsentRecordID`,
> `ScoreComputationID`, …).

This describes the core entities the rest of the system is built around.
All entities live in `FinRiskLensAI.Core/Models` and derive from
`BaseEntity` or `AuditableEntity` (see `ARCHITECTURE.md` §5).

Field lists below are the fields that matter for behavior — treat them as
a minimum, not an exhaustive schema. Add supporting fields as needed
during implementation (e.g. display names, soft foreign keys) as long as
they don't violate the layering rules.

## Msme (AuditableEntity)

The core borrower entity. One row per MSME enterprise, not per loan
application — an MSME can have multiple score computations over time.

Onboarding is Udyam-first (see `02_CUSTOMER_FLOW.md` Step 1-2), so most
identity fields are populated directly from the Udyam API response at
registration, not typed in by the MSME. Field mapping from the Udyam
response:

- `UdyamRegistrationNumber` — the lookup key for Step 1; primary
  business key for this entity (GSTIN is secondary, added in Step 4)
- `EnterpriseName` ← `enterpriseName`
- `OwnerName` ← `ownerName`
- `PanNumber` ← `panNumber`
- `GstinFromUdyam` ← `gstinStatus` (the Udyam response's linked GSTIN
  info) — nullable; used to pre-fill Step 4's GSTIN prompt when
  available, distinct from `Gstin` below which is the confirmed value
  actually used for GST data pulls
- `EmailId` ← `emailId` — the registration/login OTP channel; treat as
  the primary contact channel, not `MobileNumber`
- `MobileNumber` ← `mobileNumber` — retained for AA consent flow and as
  a secondary contact channel, but not used for registration/login OTP
- `EnterpriseType` ← `enterpriseType`
- `MajorActivity` ← `majorActivity` — also a candidate input for
  `SectorCode` classification below
- `OfficialAddress` ← `officialAddress`
- `DateOfIncorporation` ← `dateOfIncorporation`
- `DateOfCommencement` ← `dateOfCommencement`
- `OrganizationType` ← `organizationType`
- `BankName` ← `bankName`
- `BankAccountNumber` ← `bankAccountNumber` — treat as sensitive;
  don't surface in logs or in any API response beyond what's strictly
  needed
- `IfscCode` ← `ifscCode`

Fields not sourced from Udyam:

- `Gstin` — confirmed/entered in Step 4, may start pre-filled from
  `GstinFromUdyam` but is the field GST pulls actually key off once
  set
- `TradeName` — nullable; Udyam gives `enterpriseName` as the legal/
  registered name, trade name (if different) can be captured
  separately if needed, not a hard requirement
- `BusinessVintageMonths` — computed at Step 4 from
  `DateOfIncorporation` (preferred, since it's now Udyam-verified) or
  GST registration date if incorporation date is unavailable
- `SectorCode` — NIC code or simplified sector classification; can be
  derived from `MajorActivity` or set independently, used for
  sector-adjusted benchmarking in scoring
- `BorrowerType` — enum: `NTC` (New-to-Credit), `NTB` (New-to-Bank),
  `ExistingBorrower`. Not known at registration — set during Step 4
  based on bureau pull result (or absence of one)
- `RegistrationStatus` — enum: `PendingOtp`, `Active`. Distinguishes a
  record created at Step 2 (Register clicked, OTP not yet verified)
  from one that's completed Step 3. Needed so Step 5's "resume
  incomplete onboarding" logic has something concrete to check beyond
  just "does a score exist"

## ConsentRecord (AuditableEntity)

One row per AA consent artifact. An MSME may have multiple consent
records over time (renewed, revoked, expired).

- `MsmeId` — FK to Msme
- `ConsentHandle` — AA framework consent artifact identifier
- `FipId` — which Financial Information Provider the consent targets
- `ConsentStatus` — enum: `Requested`, `Active`, `Revoked`, `Expired`,
  `Rejected`
- `ConsentValidFrom`, `ConsentValidTo`
- `DataRangeFrom`, `DataRangeTo` — the historical window the MSME
  approved (6-12 months per `02_CUSTOMER_FLOW.md` Step 4; take what's
  available if less than 6 months of history exists for a young
  business)
- `PurposeCode` — AA framework purpose code (should map to "credit
  assessment" purpose category)

This table is also the audit trail the architecture notes call for —
never delete rows, only transition `ConsentStatus`.

## DataSourceSnapshot (AuditableEntity)

One row per raw data pull from a single source, before feature
engineering. Keeping raw pulls separate from computed features means a
score can always be explained by walking back to the exact data it was
computed from.

- `MsmeId` — FK to Msme
- `SourceType` — enum: `Gst`, `Itr`, `AaBankStatement`, `Epfo`, `Udyam`
- `ConsentRecordId` — FK to ConsentRecord, nullable (GST/Udyam/EPFO pulls
  may not require AA consent depending on integration approach — see
  `05_INTEGRATIONS.md`)
- `RawPayload` — the source system's response, stored as-is (JSON column
  or blob reference — for large statement dumps, store in Azure Blob
  Storage and keep only the blob URI here)
- `FetchedAt`
- `IsStale` — flag set by the refresh job when a newer snapshot
  supersedes this one; never delete old snapshots, mark them stale

## ScoreComputation (AuditableEntity)

One row per score run. This is the entity the Financial Health Card is
rendered from.

- `MsmeId` — FK to Msme
- `OverallScore` — 0-1000
- `ScoreBand` — enum: `Excellent`, `Good`, `Fair`, `AtRisk`, `HighRisk`
  (derive thresholds in the scoring service, don't hardcode in the UI)
- `RevenueVitalityScore`, `CashFlowHealthScore`,
  `TransactionTrustworthinessScore`, `ComplianceQuotientScore`,
  `BusinessStabilityScore`, `DebtServiceabilityScore` — each the raw
  sub-score before weighting, so the radar chart can show them directly
- `ModelVersion` — string tag identifying which trained model produced
  this score, for reproducibility
- `ComputedAt`
- `DataFreshnessAt` — the most recent `DataSourceSnapshot.FetchedAt`
  across all sources used, so the UI can show "last updated" honestly
- `IsCurrent` — flag; only one `ScoreComputation` per Msme should be
  current at a time, older ones remain for the trend chart

## ScoreExplanation (AuditableEntity)

One row per (ScoreComputation, dimension) pair — the human-readable
reasons behind a sub-score, sourced from the PFI/feature-importance
output.

- `ScoreComputationId` — FK
- `Dimension` — matches one of the six sub-score fields above
- `ExplanationText` — short, e.g. "3 of last 6 GSTR-3B filings were late"
- `ImpactDirection` — enum: `Positive`, `Negative`
- `RelativeWeight` — how much this factor contributed, for sorting
  strengths/risks by importance

## CreditProductRecommendation (AuditableEntity)

- `ScoreComputationId` — FK
- `ProductName` — e.g. "CGTMSE-backed Working Capital"
- `SchemeCode` — e.g. `CGTMSE`, `MUDRA`, `Standard`
- `IndicativeAmountMin`, `IndicativeAmountMax`
- `RecommendationReason` — short text

## Entity relationship summary

```
Msme 1---* ConsentRecord
Msme 1---* DataSourceSnapshot
Msme 1---* ScoreComputation
ConsentRecord 1---* DataSourceSnapshot   (nullable FK)
ScoreComputation 1---* ScoreExplanation
ScoreComputation 1---* CreditProductRecommendation
```

## Layer ownership

- **Core** — the entity classes above, `BorrowerType`/`ConsentStatus`/
  `SourceType`/`ScoreBand`/`ImpactDirection` enums, and any repository
  *interfaces* specific to these entities if you need queries beyond
  generic `IRepository<T>` (e.g. `IMsmeRepository.GetByGstinAsync`).
- **Data** — `IEntityTypeConfiguration<T>` for each entity, concrete
  repositories only where a specific query justifies one (most entities
  can go through the generic repository untouched).
- **Services** — `MsmeOnboardingService`, `ConsentOrchestrationService`,
  `ScoringService`, `ScoreExplanationService`, `RecommendationService` —
  see `02_CUSTOMER_FLOW.md` and `03_SCORING_ENGINE.md` for what each
  owns.
- **Web** — controllers and views only; no entity should be constructed
  or queried directly from a controller.
