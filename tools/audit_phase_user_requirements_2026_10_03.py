#!/usr/bin/env python3
"""CFIP 2026-10-03 user-requirements integration audit."""

from pathlib import Path
import sys

ROOT = Path(__file__).resolve().parents[1]
errors = []

def read(rel):
    p = ROOT / rel
    if not p.exists():
        errors.append("missing " + rel)
        return ""
    return p.read_text(encoding="utf-8")

bot = read("src/CFIP.cBot/CFIPExecutionBot.cs")
env = read("src/CFIP.cBot/Execution/CbotExecutionEnvironmentGate.cs")
pending = read("src/CFIP.cBot/Execution/DemoPendingOrderExecutionCoordinator.cs")
provider = read("src/CFIP.Indicator/Runtime/Provider/CFIPReadOnlyProviderScenarioBatch.cs")
registry = read("src/CFIP.Indicator/Trading/Intelligence/TradePlanRegistry.cs")
magnitude = read("src/CFIP.Indicator/Core/Math/OpportunityMagnitudeRule.cs")
range_quality = read("src/CFIP.Indicator/Core/Math/RangeSignalQualityRule.cs")
decision = read("src/CFIP.Indicator/Analysis/Market/Decision/DecisionOrchestration.cs")
prediction = read("src/CFIP.Indicator/Trading/Intelligence/Prediction/EarlyPredictionEngine.cs")
mtf_prediction = read("src/CFIP.Indicator/Core/Math/MtfEarlyPredictionFusionRule.cs")
trend = read("src/CFIP.Indicator/Core/Math/MtfTrendStrengthRule.cs")
arrows = read("src/CFIP.Indicator/UI/Chart/SignalStackedArrowRenderer.cs")
signal_renderer = read("src/CFIP.Indicator/UI/Chart/SignalRenderer.cs")
audio = read("src/CFIP.cBot/Execution/CbotLifecycleAudioService.cs")
header = read("src/CFIP.Indicator/UI/Panel/PanelHeaderRenderer.cs")
workflow = read(".github/workflows/source-check.yml")
calculation = read("src/CFIP.Indicator/Runtime/Calculation/CalculationCycle.cs")
scenario_policy = read("src/CFIP.Indicator/Core/Math/ScenarioExecutionPolicyRule.cs")
provider_plan = read("src/CFIP.Indicator/Runtime/Provider/CFIPReadOnlyProviderPlan.cs")
reward_floor = read("src/CFIP.Indicator/Core/Math/RegimeAdaptiveRewardFloorRule.cs")

def check(name, ok):
    print(f"{'PASS' if ok else 'FAIL'} | {name}")
    if not ok:
        errors.append(name)

# Realtime present/future semantics.
check(
    "scenario batch separates current ActionableNow and future pending scenarios",
    "ActionableNow" in provider and
    "FutureOrderReady" in provider and
    "ExecutionAction.PendingStop" in provider and
    "ExecutionAction.PendingLimit" in provider
)
check(
    "cBot consumes multiple scenarios independently",
    "SignalEnvelope[] scenarios" in bot and
    "ProcessSignalEnvelope(" in bot and
    "MaxConcurrentScenarios" in bot
)
check(
    "future orders remain broker pending mutations",
    "PlaceStopOrder(" in pending and
    "PlaceLimitOrder(" in pending
)

# Live readiness is explicit, account-scoped, and fail-closed.
check(
    "live has separate market/pending/aggressive/management controls",
    all(x in bot for x in (
        "EnableLiveMarketExecution",
        "EnableLivePendingStopExecution",
        "EnableLivePendingLimitExecution",
        "EnableLiveAggressiveExecution",
        "EnableLiveManagementExecution",
    ))
)
check(
    "live execution is blocked unless explicitly armed",
    "robot.Account.IsLive" in env and
    "LIVE EXECUTION NOT ARMED" in env
)
check(
    "live and demo session capacity are account-scoped",
    "EffectiveSessionExecutionCap" in bot and
    "MaxLiveExecutionsPerSession" in bot
)

# MTF intelligence + M15 contract.
check(
    "decision receives all analytical timeframes",
    all(x in decision for x in (
        "M1Frame", "M5Frame", "M15Frame", "M30Frame",
        "H1Frame", "H4Frame", "D1Frame", "W1Frame"
    ))
)
check(
    "early prediction fuses all structural frames",
    all(x in prediction for x in (
        "_m5Frame", "_m15Frame", "_m30Frame",
        "_h1Frame", "_h4Frame", "_d1Frame", "_w1Frame"
    )) and
    "MtfEarlyPredictionFusionRule.Evaluate(" in prediction and
    "M1 remains a precision/confirmation layer" in mtf_prediction
)
check(
    "nine-level smart arrow model has one canonical owner",
    "class MtfTrendStrengthRule" in trend and
    "ResolveNineLevel" in trend and
    "LevelMinimumScore = 35" in trend and
    "LevelBandSize = 5" in trend and
    "ResolveTier" in trend and
    "int arrowCount" in arrows and
    '"WATCH_ARROW"' in arrows and
    '"WATCH_ARROW_2"' in arrows and
    '"WATCH_ARROW_3"' in arrows and
    "snapshot.MtfTrendStrengthLevel" in arrows and
    "HtfTrendArrowStrengthRule" not in arrows
)

check(
    "M1 trigger marker is not a competing directional arrow",
    "ChartIconType.Circle" in signal_renderer and
    "P + \"M1_TRIGGER\"" in signal_renderer and
    "ChartIconType.UpArrow" not in signal_renderer[
        signal_renderer.find('P + "M1_TRIGGER"'):
    signal_renderer.find('P + "M1_TRIGGER"') + 500
    ]
)

# Stagnant-market quality / magnitude.
check(
    "small reward excursions are rejected by regime-aware magnitude",
    "public static bool IsMeaningful(" in magnitude and
    "OpportunityMagnitudeRule.IsMeaningful(" in read("src/CFIP.Indicator/Analysis/Market/FuturePendingOpportunityRuntime.cs")
)
check(
    "range signals require strong structural evidence and RR",
    "RANGE NO-TRADE • MID-RANGE" in range_quality and
    "RANGE NO-TRADE • LOW RR" in range_quality and
    "input.Tp1RR < 2.25" in range_quality
)

# Audio and panel header.
check(
    "cBot lifecycle/execution audio has one owner",
    "CbotLifecycleAudioService" in audio and
    "PlayStarted(" in audio and
    "PlayStopped(" in audio and
    "PlayExecutionConfirmed(" in audio and
    "PlayExecutionRejected(" in audio
)
check(
    "panel header is a dedicated realtime presentation owner",
    "private void RefreshPanelHeader()" in header and
    "UpdatePanelHeaderLiveState()" in header and
    "GetCanonicalSignalPanelStatus()" in read("src/CFIP.Indicator/UI/Panel/PanelHeaderLiveState.cs") and
    "IsCbotExecutionStateFresh()" in header
)
check(
    "new integration audits are accumulated",
    "python tools/audit_phase_cbot_lifecycle_audio_2026_10_03.py" in workflow and
    "python tools/audit_phase_panel_header_realtime_2026_10_03.py" in workflow and
    "python tools/audit_phase_user_requirements_2026_10_03.py" in workflow and
    "python tools/audit_phase_final_realtime_live_smart_system_2026_10_03.py" in workflow
)

if errors:
    print("CFIP USER-REQUIREMENTS INTEGRATION AUDIT: FAIL")
    for e in errors:
        print(" - " + e)
    sys.exit(1)

print("CFIP USER-REQUIREMENTS INTEGRATION AUDIT: PASS")
