# External Integrations

Each of these is a connector the `Services` layer calls out to. Build
each behind an interface (`IGstConnector`, `IAaConnector`,
`IEpfoConnector`, `IUdyamConnector`) so the synthetic-data generator and
the eventual real sandbox/production call can be swapped without
touching the scoring engine or the customer flow logic.

## Account Aggregator (AA) — FIP-AA-FIU framework

**What it is:** RBI's consent-based data-sharing framework. We act as an
FIU (Financial Information User). The MSME's bank is the FIP (Financial
Information Provider). An AA (a licensed intermediary) brokers consent
and data transfer between the two — we never talk to the bank directly.

**Flow shape:**
1. We submit a consent request to the AA, specifying FIP, data type
   (bank transactions), purpose, and time window.
2. The AA returns a consent handle; the actual approval happens on the
   MSME's AA app/interface, outside our system.
3. We receive a status callback (webhook) when the MSME approves or
   rejects.
4. Once approved, we submit a data request against the consent
   artifact; the AA returns the financial information (in the AA
   framework's standard bank-statement schema) either synchronously or
   via a second callback, depending on the AA's implementation.

**What we store:** the consent handle and lifecycle in `ConsentRecord`;
the raw statement payload in `DataSourceSnapshot` with `SourceType =
AaBankStatement`.

**For the hackathon:** the sandbox environment (opening July 22) should
provide either a sandbox AA or synthetic bank statement data in the AA
schema. Until then, build `IAaConnector` against a local fixture that
returns data in the same shape.

## GST System API

**What it is:** access to a business's filed GST returns — primarily
GSTR-1 (outward supplies / sales) and GSTR-3B (summary return with tax
liability), plus ITC (input tax credit) claim data.

**Flow shape:** typically accessed either directly via GSTN's API
(requires GSP — GST Suvidha Provider — empanelment) or via a licensed
GSP/account aggregator-adjacent intermediary. For the hackathon build,
treat this as a black-box HTTP call behind `IGstConnector` — the
important part is what we do with the response, not how the GSP
relationship is commercially structured (that's a partnership detail
outside the scope of the prototype).

**Authorization:** unlike AA data, GST data access is typically
authorized once at onboarding (the MSME grants access to their GSTIN's
return data as part of the onboarding terms), not through a separate
per-pull consent artifact. Model this as a boolean/timestamp on `Msme`
rather than a full `ConsentRecord`, unless you want to unify everything
under `ConsentRecord` for audit consistency — either is defensible, just
be consistent.

**What we store:** `DataSourceSnapshot` with `SourceType = Gst`,
containing the trailing 12-24 months of return summaries needed for the
Revenue Vitality and Compliance Quotient features.

## EPFO REST API

**What it is:** access to a registered establishment's PF contribution
history — used as a proxy for payroll stability and headcount trend.

**Note:** many MSMEs, especially very small ones, will not have EPFO
registration at all (it's mandatory only above a headcount threshold).
This is expected, not an error — `IEpfoConnector` should return a clear
"not registered" result distinct from a fetch failure, and the scoring
engine should treat "not registered" as a valid input to Business
Stability (small, informal-sector businesses aren't penalized for being
small), not as missing data requiring weight redistribution.

**What we store:** `DataSourceSnapshot` with `SourceType = Epfo`.

## Udyam Registry — primary onboarding source

**What it is:** the government MSME registration database. This is now
the *entry point* for onboarding (`02_CUSTOMER_FLOW.md` Step 1), not a
secondary lookup — the MSME provides only their Udyam Registration
Number, and the API response supplies enough verified identity data to
populate the `Msme` record without further manual entry.

**Flow shape:** a lookup by Udyam Registration Number, no consent flow
needed for the lookup itself (it's public registry data) — though
displaying the result and asking the MSME to confirm/Register is itself
a consent moment, captured per Step 2.

**Response contract:**

| Field | Type | Notes |
|---|---|---|
| `udyamRegistrationNumber` | string | echo of the input key |
| `enterpriseName` | string | maps to `Msme.EnterpriseName` |
| `ownerName` | string | maps to `Msme.OwnerName` |
| `panNumber` | string | maps to `Msme.PanNumber` |
| `gstinStatus` | string | linked GSTIN info; used to pre-fill Step 4's GSTIN prompt |
| `mobileNumber` | string | maps to `Msme.MobileNumber`; secondary contact, not the OTP channel |
| `emailId` | string | maps to `Msme.EmailId`; **this is the registration/login OTP channel** |
| `enterpriseType` | string | maps to `Msme.EnterpriseType` |
| `majorActivity` | string | maps to `Msme.MajorActivity`; candidate input for `SectorCode` |
| `officialAddress` | string | maps to `Msme.OfficialAddress` |
| `dateOfIncorporation` | string | maps to `Msme.DateOfIncorporation`; preferred source for `BusinessVintageMonths` |
| `dateOfCommencement` | string | maps to `Msme.DateOfCommencement` |
| `organizationType` | string | maps to `Msme.OrganizationType` |
| `bankName` | string | maps to `Msme.BankName` |
| `bankAccountNumber` | string | maps to `Msme.BankAccountNumber`; treat as sensitive |
| `ifscCode` | string | maps to `Msme.IfscCode` |

**What we store:** the full response is written to
`DataSourceSnapshot` with `SourceType = Udyam` (as before, for audit/
explainability purposes), and the mapped fields above are written
directly onto `Msme` at Step 2 registration — this is the one source
that populates the entity itself rather than only feeding the scoring
pipeline, since it's identity data, not financial history.

**Sensitive fields:** `bankAccountNumber` and `panNumber` should be
handled with the same care as any PII/financial identifier — avoid
logging them, mask in any UI display beyond what's needed, and don't
include them in the ULI/OCEN external API responses described in
`04_API_CONTRACTS.md` (those responses are score-focused; there's no
reason a third-party lender query needs bank account details).

## ULI (Unified Lending Interface) and OCEN 2.0

These aren't inbound data sources — they're the outbound side described
in `04_API_CONTRACTS.md` §3-4. Documented here only to note the
distinction clearly: AA/GST/EPFO/Udyam are things we *call*; ULI/OCEN
are the standards our *own* API should conform to so other lenders can
call *us*. Don't build an "ULI connector" — build a compliant DSP-facing
endpoint instead.

## Connector interface shape (suggested)

Keep each connector interface narrow and focused on one responsibility
— fetch the data, return it in a normalized shape, don't do feature
engineering inside the connector. `IUdyamConnector` is the one called
first and synchronously (the MSME is waiting on the confirmation
screen in `02_CUSTOMER_FLOW.md` Step 1); the other three are called
later, during Step 4, and can tolerate the async/partial-completeness
handling described there:

```
Task<UdyamLookupResult> FetchAsync(string udyamRegistrationNumber, CancellationToken ct);
Task<GstDataResult> FetchAsync(string gstin, DateRange window, CancellationToken ct);
```

Where each result type carries either the successful payload or a
typed failure reason (not found, consent required, upstream timeout,
upstream error) — the calling service needs to distinguish these to
implement the partial-completeness handling described in
`02_CUSTOMER_FLOW.md` Step 4.

## Synthetic data generator (build this first)

Before any real connector, build a generator that implements all four
connector interfaces against fixture data. Since every MSME now enters
through Udyam lookup, every fixture profile needs a valid Udyam
response — vary the *other* three sources to cover:

- A clean NTB MSME: full Udyam, GST, AA, EPFO data, healthy across all
  dimensions
- A clean NTC MSME: valid Udyam, strong GST and EPFO history, no
  bureau record, AA consent granted
- A thin-file MSME: valid Udyam with a linked GSTIN, but AA consent
  not granted and no EPFO (below headcount threshold) — this is now
  the right way to model "thin file", since Udyam itself is no longer
  optional
- An anomalous MSME: GST-declared turnover significantly exceeds AA
  bank credits (to exercise the fraud/anomaly signal)
- A declining MSME: negative revenue trend over the trailing 6 months,
  to exercise the trend overlay and produce a realistic "at risk" band

This generator is what the scoring engine and UI get built and
demoed against until sandbox access opens July 22 — treat it as a
first-class part of the build, not a throwaway script.
