#!/usr/bin/env python3
"""Deep analyzer, indicator and vote-aggregation integrity audit.

The audit intentionally favors a small number of canonical evidence owners over
adding more correlated indicators. It protects numerical readiness, symmetry,
closed/live semantics and the Indicator -> Decision -> Contract -> cBot chain.
"""
from pathlib import Path
import sys

ROOT = Path(__file__).resolve().parents[1]
errors = []


def read(rel: str) -> str:
    p = ROOT / rel
    if not p.exists():
        errors.append("missing: " + rel)
        return ""
    return p.read_text(encoding="utf-8")


def check(name: str, ok: bool) -> None:
    print(f"{'PASS' if ok else 'FAIL'} | {name}")
    if not ok:
        errors.append(name)


# Inventory coverage: every production analyzer family remains represented.
expected_analyzers = [
    "src/CFIP.Indicator/Analysis/Indicators/AverageTrueRange.cs",
    "src/CFIP.Indicator/Analysis/Indicators/AverageDirectionalIndex.cs",
    "src/CFIP.Indicator/Analysis/Indicators/DirectionalMovementIndex.cs",
    "src/CFIP.Indicator/Analysis/Indicators/ExponentialMovingAverage.cs",
    "src/CFIP.Indicator/Analysis/Indicators/RelativeStrengthIndex.cs",
    "src/CFIP.Indicator/Analysis/Indicators/MacdIndicator.cs",
    "src/CFIP.Indicator/Analysis/Indicators/External/OssIndicatorConfluenceAnalyzer.cs",
    "src/CFIP.Indicator/Analysis/Indicators/External/SkenderRsi.cs",
    "src/CFIP.Indicator/Analysis/Indicators/External/SkenderMacd.cs",
    "src/CFIP.Indicator/Analysis/Indicators/External/SkenderMfi.cs",
    "src/CFIP.Indicator/Analysis/Indicators/External/SkenderStoch.cs",
    "src/CFIP.Indicator/Analysis/Indicators/External/SkenderSuperTrend.cs",
    "src/CFIP.Indicator/Analysis/Indicators/External/SkenderAroon.cs",
    "src/CFIP.Indicator/Analysis/Indicators/External/SkenderCci.cs",
    "src/CFIP.Indicator/Analysis/Indicators/External/SkenderObv.cs",
    "src/CFIP.Indicator/Analysis/Indicators/External/SkenderBollingerBands.cs",
    "src/CFIP.Indicator/Analysis/Indicators/External/SkenderParabolicSar.cs",
    "src/CFIP.Indicator/Analysis/Market/ChoppinessIndexAnalyzer.cs",
    "src/CFIP.Indicator/Analysis/Market/DivergenceAnalyzer.cs",
    "src/CFIP.Indicator/Analysis/Market/HealthyVolatilityAnalyzer.cs",
    "src/CFIP.Indicator/Analysis/Market/LiveBiasAnalyzer.cs",
    "src/CFIP.Indicator/Analysis/Market/MacdBiasAnalyzer.cs",
    "src/CFIP.Indicator/Analysis/Market/PremiumDiscountAnalyzer.cs",
    "src/CFIP.Indicator/Analysis/Market/RangeEfficiencyAnalyzer.cs",
    "src/CFIP.Indicator/Analysis/Market/VolumeExpansionAnalyzer.cs",
    "src/CFIP.Indicator/Analysis/Market/VolumeProfileAnalyzer.cs",
    "src/CFIP.Indicator/Analysis/Market/VwapBiasAnalyzer.cs",
    "src/CFIP.Indicator/Analysis/Market/WaveTrendEngine.cs",
    "src/CFIP.Indicator/Analysis/Market/WaveTrendEvidenceAnalyzer.cs",
    "src/CFIP.Indicator/Analysis/Structure/StructureAnalyzer.cs",
    "src/CFIP.Indicator/Analysis/Structure/SwingPointAnalyzer.cs",
    "src/CFIP.Indicator/Analysis/Structure/LiquiditySweepAnalyzer.cs",
    "src/CFIP.Indicator/Analysis/Structure/EqualLevelAnalyzer.cs",
    "src/CFIP.Indicator/Analysis/Structure/Zones/FvgDetectionAnalyzer.cs",
    "src/CFIP.Indicator/Analysis/Structure/Zones/FvgLifecycleAnalyzer.cs",
    "src/CFIP.Indicator/Analysis/Structure/Zones/FvgMitigationEvaluator.cs",
    "src/CFIP.Indicator/Analysis/Structure/Zones/FvgZoneQualityCalculator.cs",
    "src/CFIP.Indicator/Analysis/Structure/Zones/OrderBlockAnalyzer.cs",
    "src/CFIP.Indicator/Analysis/Structure/Zones/OrderBlockMitigationGuard.cs",
    "src/CFIP.Indicator/Analysis/Structure/Zones/OrderBlockEvidenceBuilder.cs",
    "src/CFIP.Indicator/Analysis/Structure/Zones/OrderBlockCandidateBuilder.cs",
    "src/CFIP.Indicator/Analysis/Structure/Zones/OrderBlockQualityCalculator.cs",
]
check(
    "full analyzer inventory remains present",
    all((ROOT / rel).exists() for rel in expected_analyzers),
)


# Canonical primitive indicator owners.
native = read("src/CFIP.Indicator/Analysis/Indicators/Native/Native.cs")
native_registry = read("src/CFIP.Indicator/Analysis/Indicators/NativeIndicatorRegistry.cs")
atr = read("src/CFIP.Indicator/Analysis/Indicators/AverageTrueRange.cs")
adx = read("src/CFIP.Indicator/Analysis/Indicators/AverageDirectionalIndex.cs")
dmi = read("src/CFIP.Indicator/Analysis/Indicators/DirectionalMovementIndex.cs")
ema = read("src/CFIP.Indicator/Analysis/Indicators/ExponentialMovingAverage.cs")
rsi = read("src/CFIP.Indicator/Analysis/Indicators/RelativeStrengthIndex.cs")
macd = read("src/CFIP.Indicator/Analysis/Indicators/MacdIndicator.cs")

check(
    "native indicator registry covers the eight supported MTF frames and no M2",
    all(x in native_registry for x in ("M1", "M5", "M15", "M30", "H1", "H4", "D1", "W1")) and
    "M2" not in native_registry,
)
check(
    "ATR/ADX/DMI/EMA/RSI use one native indicator owner path",
    all(x in native for x in ("AverageTrueRange", "DirectionalMovementSystem")) and
    "Indicators.ExponentialMovingAverage" in ema and
    "Indicators.RelativeStrengthIndex" in rsi and
    "GetNative(" in (atr + adx + dmi + rsi),
)
macd_bias = read("src/CFIP.Indicator/Analysis/Market/MacdBiasAnalyzer.cs")
check(
    "MACD bias is explicitly a fast/slow MACD-line bias and not a second signal engine",
    "two-EMA MACD-line bias" in macd_bias and
    "set.MacdFast.Result[index] -\n                                set.MacdSlow.Result[index]" in macd_bias,
)

# OSS numerical adapters: missing values must fail closed, not become numeric zero.
stoch = read("src/CFIP.Indicator/Analysis/Indicators/External/SkenderStoch.cs")
quote_cache = read("src/CFIP.Indicator/Analysis/Indicators/External/OssQuoteSeriesCache.cs")
oss = read("src/CFIP.Indicator/Analysis/Indicators/External/OssIndicatorConfluenceAnalyzer.cs")
check(
    "Stochastic missing K/D cannot become a fabricated bearish/bullish vote",
    "last.K.HasValue" in stoch and
    "last.D.HasValue" in stoch and
    "k = double.NaN" in stoch,
)
check(
    "OSS quote projection rejects invalid floating-point inputs before decimal conversion",
    "double.IsNaN(open)" in quote_cache and
    "double.IsInfinity(close)" in quote_cache and
    "catch (OverflowException)" in quote_cache,
)
check(
    "aggregate OSS vote does not count OBV as an independent directional vote",
    "ObvBias" in oss and
    "BullVotes++" not in oss and
    "BearVotes++" not in oss,
)

# Specialized analytics.
wave = read("src/CFIP.Indicator/Analysis/Market/WaveTrendEngine.cs")
divergence = read("src/CFIP.Indicator/Analysis/Market/DivergenceAnalyzer.cs")
volume_profile = read("src/CFIP.Indicator/Core/Math/VolumeProfileEvidenceRule.cs")
check(
    "WaveTrend OS/OB threshold orientation is symmetric",
    "_os1 = Math.Max(os1, os2)" in wave and
    "_ob1 = Math.Min(ob1, ob2)" in wave,
)
check(
    "hidden bullish and bearish divergence score oscillator movement with the correct sign",
    "if (chosenHidden)" in divergence and
    "HiddenRsiDelta" in divergence and
    "HiddenWaveDelta" in divergence,
)
check(
    "volume-profile value-edge evidence cannot match a price outside the edge",
    "price >= profile.VAL" in volume_profile and
    "price <= profile.VAH" in volume_profile,
)

# MTF agreement: neutral ready frames must not disappear from the denominator.
timeframe = read("src/CFIP.Indicator/Analysis/Market/Decision/TimeframeAgreementAnalyzer.cs")
check(
    "MTF agreement keeps neutral ready frames in denominator",
    "Every ready/enabled timeframe remains part of the" in timeframe and
    "totalWeight +=" in timeframe and
    "frames[i].Direction == 0" not in timeframe,
)

# Canonical decision/vote aggregation.
frame_vote = read("src/CFIP.Indicator/Analysis/Market/Decision/DecisionFrameContribution.cs")
score = read("src/CFIP.Indicator/Analysis/Market/Decision/DecisionScoreCalculator.cs")
score_snapshot = read("src/CFIP.Indicator/Analysis/Market/Decision/DecisionScoreSnapshot.cs")
consensus = read("src/CFIP.Indicator/Analysis/Market/Decision/DecisionConsensusCalculator.cs")
consensus_snapshot = read("src/CFIP.Indicator/Analysis/Market/Decision/DecisionConsensusSnapshot.cs")
evaluator = read("src/CFIP.Indicator/Analysis/Market/Decision/DecisionEvaluator.cs")
decision = read("src/CFIP.Indicator/Core/Models/Decision.cs")

check(
    "required decision frames cannot enter scoring without native readiness",
    "frame.NativeIndicatorsReady" in read("src/CFIP.Indicator/Analysis/Market/Decision/DecisionInputSnapshotFactory.cs") and
    "is not ready for decision scoring" in read("src/CFIP.Indicator/Analysis/Market/Decision/DecisionInputSnapshotFactory.cs"),
)
check(
    "restriction alerts accept the canonical NEWS reason",
    'case "NEWS":' in read("src/CFIP.Indicator/Analysis/Market/Decision/DecisionRestrictionAlertPolicy.cs"),
)

check(
    "reaction quality is clamped to a finite 0..100 domain",
    "quality =\n                ClampInt(" in read("src/CFIP.Indicator/Analysis/Reaction/ReactionAnalyzer.cs") and
    "0,\n                    100" in read("src/CFIP.Indicator/Analysis/Reaction/ReactionAnalyzer.cs"),
)

check(
    "non-M5 regime stability is derived from recent regime cores",
    "int stability = 1;" in read("src/CFIP.Indicator/Analysis/Market/MarketRegimeAnalyzer.cs") and
    "beforePrevious" in read("src/CFIP.Indicator/Analysis/Market/MarketRegimeAnalyzer.cs"),
)

check(
    "parallel preview reuses the canonical scenario geometry owner",
    "TryBuildParallelScenarioGeometry(" in read("src/CFIP.Indicator/Planning/TradePlan/PlanPreviewBuilder.cs") and
    "private bool TryBuildScenarioGeometry(" not in read("src/CFIP.Indicator/Planning/TradePlan/PlanPreviewBuilder.cs"),
)

check(
    "frame vote contract carries eligibility, direction and configured weight",
    all(x in frame_vote for x in ("Weight", "Direction", "Eligible")),
)
check(
    "score aggregation derives eligible/directional/neutral frame coverage from the same frame votes",
    all(x in score for x in ("AccumulateVoteCoverage(", "eligibleFrameWeight", "directionalFrameWeight", "neutralFrameWeight")),
)
check(
    "consensus is the single final vote aggregation owner",
    "class DecisionConsensusCalculator" in consensus and
    "DirectionalCoveragePercent" in consensus_snapshot and
    "VoteConfidence" in consensus_snapshot and
    "Calculate(" in consensus,
)
check(
    "decision consumes the exact consensus diagnostics without recomputation",
    all(x in evaluator for x in (
        "decision.VoteBuyScore = consensus.BuyScore;",
        "decision.VoteSellScore = consensus.SellScore;",
        "decision.VoteNetScore = consensus.NetScore;",
        "decision.VoteCoverage = consensus.DirectionalCoveragePercent;",
        "decision.VoteConfidence = consensus.VoteConfidence;",
    )) and
    "VoteCoverage" in decision,
)
check(
    "direction remains exactly symmetric for BUY and SELL",
    "buyShare > sellShare" in consensus and
    "sellShare > buyShare" in consensus and
    "buyShare == sellShare" not in consensus,
)

# Research-backed architecture guard: do not create an independent indicator vote
# for every correlated measurement. The canonical fusion rule owns grouping.
fusion = read("src/CFIP.Indicator/Core/Math/IndicatorEvidenceFusionRule.cs")
independence = read("src/CFIP.Indicator/Core/Math/IndicatorEvidenceIndependenceRule.cs")
check(
    "correlated indicators remain grouped instead of becoming one vote each",
    "Trend, momentum and context" in independence and
    "trendBull" in fusion and
    "momentumBull" in fusion and
    "contextBull" in fusion,
)

# Cross-boundary safety.
provider = "\n".join(
    read(p.as_posix())
    for p in sorted((ROOT / "src/CFIP.Indicator/Runtime/Provider").glob("CFIPReadOnlyProvider*.cs"))
)
bot = read("src/CFIP.cBot/CFIPExecutionBot.cs")
check(
    "Indicator remains broker-mutation-free and cBot remains execution owner",
    "ExecuteMarketOrder(" not in provider and
    "PlaceStopOrder(" not in provider and
    "PlaceLimitOrder(" not in provider and
    "ProcessSignalEnvelope(" in bot and
    "CbotSignalPreflight.TryValidate(" in bot,
)

if errors:
    print("=" * 72)
    print("DEEP ANALYZER / VOTE AUDIT: FAIL")
    for error in errors:
        print("- " + error)
    sys.exit(1)

print("=" * 72)
print("DEEP ANALYZER / VOTE AUDIT: PASS")
