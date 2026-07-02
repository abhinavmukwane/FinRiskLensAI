# Scoring Engine

> **Implementation status (2026-07-02): IMPLEMENTED** in the `FinRiskLensAI.ML`
> project — six weighted dimensions, missing-dimension weight redistribution,
> NTC neutral default, LightGBM + PFI explanations, SSA trend, RandomizedPca
> three-way income anomaly check, band → product mapping, synthetic training
> data pending the July 22 sandbox. See `08_ML_ENGINE.md` for the API contract
> and pipeline internals. Differences from this spec worth knowing: features are
> currently extracted from raw payloads (blob folder / request body) rather than
> `DataSourceSnapshot` rows (those entities don't exist yet), and sub-score
> calculators are heuristic formulas blended 60/40 with the LightGBM model
> rather than per-dimension trained models — revisit both once sandbox data lands.

The Financial Health Score is 0-1000, composed of six weighted
dimensions. This document defines what each dimension measures, its
weight, its primary data source, and how to handle missing data —
missing-data handling matters more than usual here because credit-
invisible MSMEs are the whole point of the product.

## The six dimensions

| Dimension | Weight | Points | Primary source |
|---|---|---|---|
| Revenue Vitality | 25% | 250 | GST + ITR |
| Cash Flow Health | 20% | 200 | AA bank statements |
| Transaction Trustworthiness | 15% | 150 | AA bank statements (UPI credits) |
| Compliance Quotient | 15% | 150 | GST + ITR + EPFO |
| Business Stability | 15% | 150 | EPFO + Udyam |
| Debt Serviceability | 10% | 100 | AA + bureau (if available) |

`OverallScore = sum of the six weighted sub-scores`, each sub-score
itself computed on a 0-1 scale before weighting, so the sub-scores
stored on `ScoreComputation` (per `01_DOMAIN_MODEL.md`) should be stored
as the 0-(dimension max) value, not the raw 0-1 model output — keep the
UI simple by storing display-ready numbers.

### Revenue Vitality (GST + ITR derived)

What it's trying to capture: is the business generating real, growing,
consistent revenue.

Candidate features from GST: month-over-month GST turnover trend (slope
over trailing 12 months), GSTR-1 vs GSTR-3B consistency (large
persistent gaps are a red flag), ITC claim pattern, B2B-to-B2C ratio,
inter-state trade spread.

Candidate features from ITR: declared gross/net income trend across
available assessment years, income stability year-over-year, and — as a
cross-check rather than a standalone feature — the gap between
ITR-declared income and GST-declared turnover. A large unexplained gap
here is a second, independent read on the same fraud/misrepresentation
signal described below, distinct from the GST-vs-bank-credit check.
ITR is also the dimension's fallback for MSMEs below the GST
registration threshold (very small businesses that file ITR but aren't
GST-registered) — see the missing-data handling below.

### Cash Flow Health (AA-derived)

What it's trying to capture: does the business have a cash cushion and
predictable inflows, independent of what GST says about invoiced
revenue (the two can diverge — see the fraud signal note below).

Candidate features: average monthly closing balance relative to
turnover, coefficient of variation of monthly inflows (volatility),
days-cash-on-hand, cheque/payment bounce rate.

### Transaction Trustworthiness (UPI-derived, from AA bank data)

What it's trying to capture: the quality and stability of the
customer/counterparty base, not just the volume.

Candidate features: transaction velocity, counterparty diversity
(concentration in 1-2 payers is a risk even at high volume),
business-hours proportion of transactions, repeat-payer ratio.

### Compliance Quotient (GST + ITR + EPFO)

What it's trying to capture: organizational maturity and regulatory
discipline, which correlates with repayment discipline.

Candidate features: GST filing regularity over trailing 24 months, ITR
filing regularity across available assessment years (filed on time vs.
late vs. not filed), EPFO contribution consistency, any recorded
penalty/demand notices.

### Business Stability (EPFO + Udyam)

What it's trying to capture: is this a durable, ongoing concern.

Candidate features: business vintage (`Msme.BusinessVintageMonths`,
computed from Udyam's `DateOfIncorporation`), employee count trend from
EPFO contribution headcount, sector risk index (a static lookup table
by `SectorCode`, not a live model), Udyam registration class (Micro/
Small/Medium).

### Debt Serviceability (AA + bureau)

What it's trying to capture: existing obligations relative to
capacity.

Candidate features: existing EMI-to-inflow ratio from AA statement
patterns, projected DSCR, bureau-reported credit utilization if a
bureau record exists.

**Missing-data handling:** for a true NTC borrower, bureau data will be
absent. Default this dimension to a neutral midpoint (not zero, not the
max) rather than penalizing the absence of a credit history — a zero
here would defeat the product's purpose. Flag on `ScoreExplanation` that
this dimension used a neutral default so it's transparent, not hidden.

## Missing dimension handling (general rule)

If a whole data source is unavailable (e.g. AA consent was rejected, so
Cash Flow Health and Transaction Trustworthiness have no input):

1. Do not silently zero the dimension — this would make refusal of AA
   consent look like bad credit behavior, which it isn't.
2. Redistribute that dimension's weight proportionally across the
   dimensions that *do* have data, and record which dimensions were
   excluded on the `ScoreComputation` (add a field or explanation entry
   noting reduced-input scoring).
3. Surface this clearly on the Financial Health Card — "Score computed
   from GST + ITR + EPFO + Udyam only; AA consent not granted" — so a
   credit officer isn't misreading a partial score as a complete one.

## Fraud / anomaly signal: cross-source income validation

Independent of the six dimensions, run a cross-check across three
independent income signals: GST-declared turnover, ITR-declared
income, and actual bank credits from the AA statements over the same
period. Having three sources instead of two makes this materially
stronger — a two-way mismatch could be a timing artifact (e.g. invoiced
but uncollected revenue), but a consistent pattern where two sources
agree and the third diverges sharply is a much clearer signal of which
source is misrepresenting. A large, unexplained gap is a red flag worth
surfacing as its own explanation entry, separate from the dimension
scores — it's a data-integrity signal, not a creditworthiness signal,
and conflating the two would confuse the officer.

This is a good candidate for the ML.NET RandomizedPca anomaly detector
mentioned in the tech stack — treat (GST turnover, ITR income, actual
bank credit) as a three-way feature set, plus their pairwise ratios, as
the anomaly detection input.

## Trend overlay (SSA)

The 12-month trend line shown on the Financial Health Card isn't a
separate score — it's the same `OverallScore` (and optionally each
sub-score) computed at each historical `ScoreComputation` for that
Msme, plotted over time. The ML.NET SSA (time-series) component is used
during *feature engineering* to derive trend-based features (e.g. "is
revenue accelerating or decelerating") that feed into Revenue Vitality
and Cash Flow Health — it is not a second, competing score.

## Explainability (PFI)

After the LightGBM sub-scores are computed, run Permutation Feature
Importance to identify which features moved each sub-score the most for
this specific MSME. Translate the top 2-3 features per dimension into
short, human-readable strings for `ScoreExplanation` — e.g. "3 of last 6
GSTR-3B filings were late" rather than exposing a raw feature name and
importance float. This translation step (feature name → sentence) is
worth building as a small lookup/template dictionary rather than
generating free text, so the wording stays consistent and audit-
reviewable.

## Score bands

Suggested default thresholds (tune once you have synthetic data
distributions, don't treat these as final):

| Band | Range |
|---|---|
| Excellent | 800-1000 |
| Good | 650-799 |
| Fair | 500-649 |
| At Risk | 350-499 |
| High Risk | 0-349 |

## Credit product recommendation mapping

A simple score-band → scheme lookup is sufficient for the hackathon
build — this doesn't need to be a model:

- Excellent/Good → standard working capital products, full indicative
  ticket size range
- Fair → CGTMSE-backed working capital (guarantee reduces lender risk
  on a moderate-risk borrower)
- At Risk → MUDRA (Shishu/Kishor tier depending on requested amount),
  smaller indicative ticket size
- High Risk → no product recommendation; surface the specific weak
  dimensions instead so the MSME has something actionable

## Synthetic data note

IDBI's sandbox (with real synthetic banking datasets) opens July 22.
Until then, build a synthetic data generator that produces MSME
profiles spanning the `BorrowerType` spectrum — NTC with strong GST/ITR
but no AA history, NTB with full data, thin-file, and a couple of
deliberately anomalous profiles (mismatches across GST, ITR, and bank
credit) to exercise the fraud signal. Swap the generator for real
sandbox calls behind the same connector interfaces described in
`05_INTEGRATIONS.md`
— the scoring engine shouldn't need to change when the data source
changes.
