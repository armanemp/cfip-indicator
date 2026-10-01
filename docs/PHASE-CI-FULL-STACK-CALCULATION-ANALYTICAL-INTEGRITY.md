# CFIP Indicator — Full-Stack Calculation & Analytical Integrity Track

## Purpose

This track is a **blocking correctness program** to be completed before the next
ordinary refinement phase is resumed.

The objective is to make the complete causal chain mathematically consistent,
directionally symmetric, time-consistent, provenance-aware and observable:

`Market Data → Primitive Indicators → Indicator Fusion → Structure/Zones/Liquidity → MTF/Regime/Reaction → Decision → Trigger → Entry Geometry → SL → TP/RR → Execution Intent → Broker Acceptance → Trade`

This is not a threshold-tuning program. Numeric thresholds and strategy weights
must not be changed merely because a test fails. First establish what the
system means and whether the current implementation actually computes that
meaning.

The track is complete only when:

- every critical calculation has one authoritative definition;
- BUY and SELL are mirror-symmetric where symmetry is expected;
- closed-bar and live/intrabar inputs are explicit;
- indicator outputs are proven numerically correct and warm-up safe;
- structure, FVG, OB, liquidity, divergence and regime calculations have
  deterministic semantics;
- correlated evidence is not counted as independent evidence without an
  explicit rule;
- Trigger, Entry, SL, TP and RR consume the same price/quote/ATR semantics;
- signal latency is measured rather than guessed;
- the exact same validated geometry reaches the execution boundary;
- deterministic replay and target-terminal validation cover the complete chain.

## Authoritative audit map

### Primitive indicator layer

Current production/native indicator and numerical inputs include:

- ATR;
- ADX / DMI;
- EMA;
- RSI;
- MACD;
- volume-expansion;
- VWAP;
- healthy-volatility calculations;
- Choppiness;
- Range Efficiency.

Current production OSS adapter families include:

- Aroon;
- Bollinger Bands;
- CCI;
- MACD;
- MFI;
- OBV;
- Parabolic SAR;
- RSI;
- Stochastic;
- SuperTrend.

WaveTrend is an explicit composite momentum engine with its own calculation,
readiness and evidence owners and must be audited separately rather than being
treated as several independent votes.

### Structure and price-location layer

The audit covers:

- swing points;
- canonical swing highs/lows;
- equality/plateau semantics;
- structural sequence;
- liquidity pools;
- liquidity sweeps;
- supply/demand;
- premium/discount;
- FVG detection, lifecycle and mitigation;
- Order Block detection, displacement evidence, mitigation and confluence;
- zone overlap and zone quality;
- target/stop source provenance.

### Market-intelligence layer

The audit covers:

- MTF closed context;
- timeframe alignment;
- absolute strength;
- regime classification;
- regime transition/stability;
- trend/range/compression/expansion/high-volatility semantics;
- divergence;
- hidden vs regular divergence;
- live bias;
- reaction analysis;
- early prediction/watch state;
- parallel opportunities/scenarios;
- indicator confluence;
- evidence grouping and correlation control.

### Decision layer

The audit covers:

- directional score;
- consensus;
- edge;
- quality;
- confidence;
- adaptive/regime thresholds;
- top-down calibration;
- independent evidence counts;
- structural confirmations;
- decision gates;
- EntryAllowed vs TriggerReady vs ActionableNow lifecycle.

### Trade-plan / execution layer

The audit covers:

- execution zone;
- ideal entry;
- trigger;
- actual executable entry;
- entry invalidation;
- late/extension semantics;
- structural stop;
- risk;
- TP1..TP4;
- target source provenance;
- target obstacles;
- RR ladder;
- synthetic fallback;
- execution-plan geometry;
- market/pending/aggressive intent;
- broker-side direction and distance constraints.

## Phase sequence

### CI-00 — Canonical data, price and time contract

Define and test one contract for:

- Bid;
- Ask;
- executable BUY price;
- executable SELL price;
- midpoint usage;
- pip size;
- tick size;
- broker minimum stop distance;
- broker minimum TP distance;
- ATR source index;
- closed-bar index;
- live/open-bar index;
- MTF reference timestamp;
- quote timestamp;
- signal timestamp.

Acceptance:

- every downstream calculation can identify its exact reference price,
  reference bar and reference time;
- no implicit use of a different quote/index remains.

#### CI-00 implementation record

CI-00 establishes `CanonicalPriceSnapshot` as the calculation-level owner for
current quote geometry. `CalculationMarketContext` composes this snapshot with
the existing `MtfClosedContext`, so MTF ownership is not duplicated.

The cTrader `Symbol` adapter is isolated in
`CanonicalMarketContextBuilder.cs`. It converts the broker's minimum-distance
unit into an explicit platform-neutral representation and records the terminal
observation time as `QuoteObservedUtc`.

High-risk planning/execution consumers now use this context for executable
BUY/SELL price, spread, pip conversion, tick/digits normalization and retest
market tolerance.

Deterministic Runtime Acceptance contracts and the phase-specific static audit
are included. Target-terminal broker semantics are intentionally deferred to the
later CI-15 through CI-17 phases.

### CI-01 — Primitive indicator mathematical audit

Audit each native indicator implementation and every direct consumer for:

- formula correctness;
- smoothing method;
- seed/warm-up;
- index alignment;
- missing/zero handling;
- NaN/Infinity;
- direction/mirror symmetry;
- boundary behavior.

Deterministic fixtures must include:

- monotonic rise;
- monotonic fall;
- flat market;
- alternating market;
- large gap;
- zero-volume/constant-volume cases where applicable.

Acceptance:

- expected values are independently reproducible;
- every consumer knows whether it uses the current closed value, prior closed
  value or live value.

#### CI-01 implementation record

Status: **implemented on branch phase/ci-01-primitive-indicator-integrity; final gate pending.**

Completed correctness work:

- retained cTrader-native ATR, ADX/DMI, EMA and RSI as the only production
  implementations of those standard indicators;
- aligned DMI +DI/-DI readiness with the same configured warm-up contract used
  by the other native frame inputs;
- made the existing two-EMA MACD feature explicit as MACD-line bias instead of
  calling that value a histogram;
- centralized DMI, MACD-line bias, RangeEfficiency, Choppiness, VWAP and Volume
  Expansion arithmetic in dedicated pure mathematical owners;
- corrected RangeEfficiency so numerator and denominator use the same number of
  close-change intervals and the configured period is never silently shortened;
- corrected Choppiness to require the full configured bar window;
- corrected VWAP to use exactly the configured number of bars and zero weight for
  zero-volume samples;
- corrected Volume Expansion to use actual bar range rather than a pip-size
  floor;
- added deterministic contract coverage for formulas, boundaries and BUY/SELL
  symmetry;
- added a CI-01 static audit to the accumulated Source/Architecture workflow.

Explicit non-changes:

- no score/weight/threshold tuning;
- no new MACD signal-period parameter;
- no replacement of native cTrader indicator mathematics by a second live engine;
- no FVG/OB/structure/decision/execution changes.

#### CI-02 implementation record

Status: **VERIFIED COMPLETE — implementation head `c3720853edbcf5c04bb1f5cbf1e9533f39e87a4e`; PR #156.**

Completed correctness and architecture work:

- kept `OssIndicatorSettings` as the fixed-setting authority and
  `OssIndicatorParameters` as the configured-period/minimum-history authority;
- added canonical `OssQuoteWindowRule` for bounded window geometry and
  deterministic rebuild decisions;
- added canonical `OssQuoteProjectionRule` for finite/non-negative quote-volume
  normalization;
- preserved the 768-bar stable window for recursive/path-dependent Skender
  adapters and the existing 161-bar rolling window for window-local adapters;
- retained first/last stable-window fingerprints and HistoryLoaded/Reloaded
  invalidation;
- removed artificial unit volume from zero-volume source observations;
- added runtime contracts for window movement, bounds and quote-volume
  normalization;
- consolidated the existing H3-B deterministic benchmark into one Track 19
  OSS parity owner covering all production Skender indicator families,
  stable/rolling boundaries, OBV direction and zero-volume fixtures;
- removed the duplicate CI-02 benchmark module so numerical parity has one
  benchmark execution authority;
- added full-prefix versus bounded runtime/allocation measurement;
- accumulated `audit_phase_ci_02.py` after CI-01 in Source/Architecture CI.

Acceptance boundary:

- stable recursive indicators must remain finite and directionally equivalent
  to full-prefix references while max/mean/RMS error is measured;
- window-local indicators must match full-prefix terminal output within the
  deterministic 1e-12 gate;
- FacioQuo remains research-only and production package ownership is unchanged.

No parameter, confidence, score, RR, risk, SL/TP, decision or execution-policy
tuning is part of CI-02.

Verification on the final CI-02 implementation head:
- Source / Architecture: PASS — workflow run 36909965454;
- Runtime Acceptance Contracts: PASS — workflow run 36909965513;
- cTrader Compile / Build: PASS — workflow run 36909965368;
- OSS benchmark: PASS — workflow run 36909965470;
- deterministic OSS parity: 384 compared points with zero direction/non-finite/
  rolling-exact mismatches.

### CI-02 — OSS numerical parity, warm-up and cache audit

Audit Skender adapters and caches, including:

- fixed settings ownership;
- parameter-driven settings;
- warm-up windows;
- bounded windows;
- first/last boundary invalidation;
- output alignment;
- finite-value coverage;
- parity against deterministic full-prefix references;
- cache invalidation after new data;
- allocation and runtime cost.

The existing production/research OSS boundary remains unchanged unless a
specific correctness defect is proven.

Acceptance:

- no adapter changes numerical meaning silently;
- no bounded cache produces a different answer from the canonical full-prefix
  result beyond documented warm-up semantics.

### CI-03 — Indicator fusion and correlation audit

Trace every indicator into:

- raw signal;
- normalized evidence;
- frame score;
- decision contribution;
- final quality/confidence;
- trigger/actionability.

Identify cases where several indicators are measuring substantially the same
phenomenon and are counted as separate independent evidence.

Required distinction:

`measurement` ≠ `independent evidence`

Acceptance:

- each evidence contribution has an explicit role;
- duplicate/correlated evidence cannot inflate confidence accidentally;
- disabling an optional indicator removes only its intended contribution.

### CI-04 — Structure, swing and liquidity semantics audit

Audit:

- swing equality;
- plateau grouping;
- confirmation lag;
- future-bar leakage;
- equal-high/low tolerance;
- liquidity candidate validity;
- sweep confirmation;
- active/unbroken state;
- structure sequence;
- structural direction.

BUY/SELL symmetry must be proven with mirrored deterministic fixtures.

Acceptance:

- identical mirrored inputs produce mirrored structural decisions;
- no candidate uses future information.

### CI-05 — FVG full lifecycle audit

Audit:

`detection → zone geometry → age → mitigation → partial fill → full fill → invalidation → lookup → execution/target usage`

Cover:

- three-candle definition;
- wick/body boundary policy;
- partial mitigation;
- full-fill invalidation;
- break-by-wick option;
- exact boundary prices;
- source age;
- MTF mapping.

Acceptance:

- one canonical FVG definition is used everywhere;
- the same zone cannot be considered valid by one module and invalid by another
  under identical state.

### CI-06 — Order Block full lifecycle audit

Audit:

- candidate candle selection;
- impulse/displacement relation;
- directional validity;
- zone boundaries;
- quality;
- mitigation;
- retest;
- confluence;
- stale/invalid state;
- MTF propagation.

Special requirement:

- OB must not become merely a cosmetic zone;
- OB evidence used for Entry/SL/TP must be traceable back to the same source
  object.

Acceptance:

- deterministic fixtures cover bullish/bearish OBs, weak displacement, strong
  displacement, mitigation and invalidation;
- no duplicated OB definition exists.

### CI-07 — Market regime, MTF and context audit

Audit:

- M1/M5/M15/M30/H1/H4/D1/W1 mapping;
- closed-bar alignment;
- timeframe stability;
- top-down absolute strength;
- regime classification;
- regime transitions;
- trend/range/compression/expansion/high-volatility;
- premium/discount;
- session context.

Acceptance:

- one market-state snapshot drives every consumer for the same reference
  instant;
- no higher-timeframe value can accidentally come from a newer/future bar.

### CI-08 — Divergence, WaveTrend, reaction and early-signal audit

Audit:

- regular divergence;
- hidden divergence;
- divergence quality thresholds;
- WaveTrend base calculation;
- WaveTrend smoothing;
- WaveTrend cross/turn/zone semantics;
- live reaction timing;
- early prediction;
- watch/reaction/confirmed-state separation.

Acceptance:

- these features cannot silently veto or boost unrelated stages through
  undocumented side effects;
- live reaction never rewrites the closed-bar decision snapshot.

### CI-09 — Decision engine mathematical audit

Audit end-to-end:

`frame contribution → score → consensus → edge → quality → confidence → gates → EntryAllowed`

Check:

- score normalization;
- weighting;
- clipping;
- adaptive thresholds;
- regime-specific changes;
- higher-timeframe penalties;
- calibration adjustment;
- evidence duplication;
- directional symmetry;
- deterministic repeatability.

Acceptance:

- same input snapshot always produces the same decision;
- every score component has a traceable contribution;
- confidence is never treated as a calibrated probability unless empirical
  calibration has actually been completed.

### CI-10 — Trigger and trigger-lifecycle audit

Audit:

- M5 trigger score;
- M1 trigger;
- microstructure break;
- displacement;
- fresh-trigger evidence;
- breakout buffer;
- exact threshold behavior;
- M5/M1 window relation;
- latch/reset semantics;
- trigger expiry;
- direct displacement override;
- trigger readiness propagation.

Required lifecycle:

`Decision eligible → Trigger candidate → Trigger confirmed → Actionable state`

Acceptance:

- a newly confirmed M1 trigger can become actionable inside the correct M5
  window without waiting for an unrelated later M5 cycle;
- no trigger can be accepted after its causal window has expired;
- early and confirmed trigger states are explicitly distinct.

### CI-11 — Entry geometry and signal-timing audit

Canonicalize:

- execution zone;
- ideal entry;
- trigger price;
- actual executable quote;
- entry distance;
- extension;
- late state;
- retest state;
- breakout state;
- continuation state.

Audit current issue:

- live Actionability and Plan Preparation must use the same late-entry and
  distance contract.

Acceptance:

- one mathematical entry geometry is shared by actionability, plan,
  pending, market execution and presentation;
- signal latency is measured from the causal event, not inferred from the bar
  that happens to display it.

### CI-12 — Structural SL audit

Audit:

- M5 swing;
- M5 FVG;
- M5 OB;
- HTF structure;
- HTF FVG/OB;
- buffer;
- minimum risk;
- maximum risk;
- spread relation;
- broker distance;
- fallback;
- final normalization;
- BUY/SELL symmetry.

Priority rule:

`Structural validity → protection validity → risk feasibility → reward optimization`

Reward feasibility must not silently redefine structural stop meaning.

Acceptance:

- one authoritative stop geometry;
- no risk expansion during planning;
- no incorrect-side or passed stop.

### CI-13 — TP source, target obstacle and ladder audit

Audit:

- all target source families;
- source provenance;
- source age;
- directional validity;
- obstacle path;
- opposing zones;
- HTF path;
- spacing;
- target extension;
- target progression;
- synthetic fallback;
- TP1..TP4 selection.

Replace/repair greedy assumptions where deterministic fixtures prove that
stage-by-stage selection produces a globally inferior or inconsistent ladder.

Acceptance:

- TP ladder is evaluated as a coherent sequence;
- each target is traceable to a real source or explicitly marked synthetic;
- no target is based on a level already invalidated/consumed by the current
  reference state.

### CI-14 — Canonical risk/reward and protection mathematics

Create one canonical calculation contract for:

`Risk`
`Reward`
`Nominal RR`
`Spread-adjusted/effective RR`
`Minimum RR`
`Maximum RR`

Audit every consumer for formula drift, especially the current difference
between candidate-stage and final-stage RR normalization.

Acceptance:

- candidate filtering, plan validation, actionability and execution use the
  same RR semantics;
- BUY/SELL mirror fixtures pass;
- boundary cases around pip/tick/risk floors are deterministic.

### CI-15 — End-to-end execution-geometry and broker-boundary audit

Trace the exact same validated values through:

`Decision → Trigger → Entry → SL → TP → Intent → Validation → Broker submission → Broker-confirmed state`

Audit:

- market;
- aggressive;
- pending Stop;
- pending Limit;
- actual fill;
- favorable/adverse fill envelope;
- slippage;
- broker minimum-distance transformations;
- post-fill adoption.

Acceptance:

- no execution path reconstructs Entry/SL/TP independently;
- the broker-confirmed state can be compared against the exact submitted
  intent.

### CI-16 — Deterministic replay, latency and counterexample suite

Build a unified replay suite that records at minimum:

- reference time;
- causal event;
- indicator snapshot;
- structure snapshot;
- decision;
- trigger;
- entry;
- SL;
- TP1..TP4;
- RR;
- first-actionable timestamp;
- alert timestamp;
- execution-attempt timestamp;
- fill timestamp.

Required counterexamples:

- fast breakout;
- slow breakout;
- fast reversal;
- retest;
- range market;
- expansion;
- compression;
- strong OB+FVG confluence;
- weak single-zone setup;
- high spread;
- large displacement;
- M1 confirmation late in M5;
- M1 confirmation early in M5;
- target obstruction;
- opposite divergence;
- mirrored BUY/SELL.

Acceptance:

- expected output is deterministic;
- no hidden path changes Entry/SL/TP after the authoritative plan is created;
- latency bottlenecks are measurable.

### CI-17 — Target-terminal cTrader validation

Validate on the actual target environment:

- indicator initialization;
- data readiness;
- M1/M5 timing;
- Ask/Bid behavior;
- broker distance rules;
- fill timing;
- pending fill;
- slippage;
- reconnect/reload;
- chart/panel timing;
- signal and plan synchronization.

Acceptance:

- runtime behavior agrees with deterministic replay at the semantic level;
- deviations are documented as platform/broker realities rather than hidden
  inside the mathematical model.

### CI-FINAL — Full-stack Calculation Integrity Certification

The track closes only when:

- Source/Architecture gates are green;
- Runtime Acceptance Contracts are green;
- cTrader compile is green;
- deterministic replay is green;
- all critical counterexamples are accounted for;
- no duplicate mathematical owner remains;
- latency measurements are available;
- the complete Decision → Trade chain consumes canonical geometry.

Only after CI-FINAL is the next previously planned roadmap phase resumed.

## Non-goals

This track does **not** authorize:

- blanket lowering of thresholds;
- arbitrary score/weight increases;
- claims of higher win rate without outcome evidence;
- adding more indicators merely to increase evidence counts;
- replacing existing engines with external engines without numerical proof;
- weakening safety gates to obtain more signals.

## Mandatory documentation

Every CI phase must leave:

- deterministic contract/fixture results;
- exact formulas/semantics changed;
- caller migration record;
- duplicate-owner audit result;
- verification status;
- manual cTrader boundary, where applicable.

The final continuity record must link this track from the master roadmap and state
which prior phase is resumed after CI-FINAL.
