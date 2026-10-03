from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]


def read(relative):
    return (ROOT / relative).read_text(encoding="utf-8")


def require(condition, message):
    if not condition:
        raise SystemExit(message)


contracts = read("src/CFIP.Contracts/AlertEnvelope.cs")
delivery = read("src/CFIP.Indicator/Core/Runtime/AlertDelivery.cs")
queue = read("src/CFIP.Indicator/Core/Runtime/AlertDeliveryQueue.cs")
alerts = read("src/CFIP.Indicator/Trading/Alerts/AlertEngine.cs")
alert_identity = read("src/CFIP.Indicator/Trading/Alerts/CanonicalAlertEnvelopeBuilder.cs")
processor = read("src/CFIP.Indicator/UI/Panel/AlertDeliveryProcessor.cs")
snapshot = read("src/CFIP.Indicator/UI/Chart/SignalVisualSnapshot.cs")
snapshot_builder = read("src/CFIP.Indicator/UI/Chart/SignalVisualSnapshotBuilder.cs")
visual_identity = read("src/CFIP.Indicator/UI/Chart/SignalVisualIdentityBuilder.cs")
watch_renderer = read("src/CFIP.Indicator/UI/Chart/SignalPresentationRenderer.cs")
closed = read("src/CFIP.Indicator/Runtime/Calculation/CalculationClosedBar.cs")
cycle = read("src/CFIP.Indicator/Runtime/Calculation/CalculationCycle.cs")
stages = read("src/CFIP.Indicator/Runtime/Calculation/CalculationStageIsolation.cs")
live = read("src/CFIP.Indicator/Runtime/Calculation/CalculationLiveCycle.cs")
lines = read("src/CFIP.Indicator/UI/Chart/PlanLineRenderer.cs")
labels = read("src/CFIP.Indicator/UI/Chart/PlanLabelFormatting.cs")
label_anchor = read("src/CFIP.Indicator/UI/Chart/PlanLabelAnchorCalculator.cs")
label_renderer = read("src/CFIP.Indicator/UI/Chart/PlanLabelRenderer.cs")
alert_renderer = read("src/CFIP.Indicator/UI/Chart/AlertSignalRenderer.cs")
provider = read("src/CFIP.Indicator/Runtime/Provider/CFIPReadOnlyProviderRefresh.cs")
cbot = read("src/CFIP.cBot/CFIPExecutionBot.cs")
workflow = read(".github/workflows/source-check.yml")

require(
    "public sealed record AlertEnvelope(" in contracts and
    "ContractIdentity Identity" in contracts and
    "SignalStage Stage" in contracts and
    "VisualMarkAllowed" in contracts,
    "M3: canonical immutable AlertEnvelope contract is incomplete",
)

require(
    "AlertEnvelope envelope" in delivery and
    "public AlertEnvelope Envelope" in delivery and
    "AlertEnvelope envelope" in delivery,
    "M3: AlertDelivery must carry the canonical envelope",
)

require(
    "BuildCanonicalAlertEnvelope(" in alerts and
    "new AlertDelivery(" in alerts and
    "envelope," in alerts,
    "M3: AlertEngine does not enqueue the canonical alert envelope",
)

require(
    "BuildCanonicalAlertEnvelope(" in alert_identity and
    "IsBlockedCandidateAlert(" in alert_identity and
    "ResolveAlertSignalStage(" in alert_identity,
    "M3: canonical alert identity/blocked-stage owner is incomplete",
)

require(
    "Notifications.PlaySound" not in alerts and
    (
        "ProcessQueuedAlertDelivery();" in cycle or
        "ProcessQueuedAlertSoundDelivery();" in cycle
    ),
    "M3: sound must stay transport-owned by the queued delivery processor",
)

require(
    "bool blockedCandidateAlert" in alerts and
    "EnableSoundAlerts &&" in alerts and
    "!blockedCandidateAlert" in alerts and
    "!blockedCandidate &&" in alert_identity,
    "M3: blocked candidate alerts must not create audible/visual signal side effects",
)

require(
    "RecordPanelAlertDelivery(next)" in processor and
    "Notifications.PlaySound(" in processor and
    "next.Message" not in processor,
    "M3: panel rail and sound must consume the same queued event",
)

require(
    "public string SignalId;" in snapshot and
    "public string ScenarioId;" in snapshot and
    "public string PlanId;" in snapshot and
    "public string SourceTimeframe;" in snapshot and
    "public long Revision;" in snapshot and
    "PopulateCanonicalVisualIdentity(" in snapshot_builder and
    "ResolveProviderSignalId(" in visual_identity and
    "ProviderScenarioIdentityRule.ResolveSourceTimeframe(" in visual_identity,
    "M3: visual snapshot is missing canonical trade identity",
)

require(
    "ResolveProviderSignalId(" in alert_identity and
    "ResolveProviderScenarioId(" in alert_identity and
    "ResolveProviderPlanId(" in alert_identity and
    "ProviderScenarioIdentityRule.ResolveSourceTimeframe(" in alert_identity,
    "M3: alert identity must reuse the canonical provider identity resolvers",
)

require(
    "snapshot.DecisionEntryAllowed" in watch_renderer and
    "!snapshot.PendingOrder" in watch_renderer and
    "!snapshot.LivePosition" in watch_renderer,
    "M3: blocked/pending/live states must not draw directional watch marks",
)

require(
    "BuildDecision(" in closed and
    "BuildEarlyPrediction(" in closed and
    "TryEnsureAutomaticPlan(" in closed and
    "ProcessDecisionAlerts(" in closed,
    "M3: closed-bar decision chain is missing a canonical stage",
)

require(
    closed.index("BuildDecision(") <
    closed.index("BuildEarlyPrediction(") <
    closed.index("TryEnsureAutomaticPlan(") <
    closed.index("ProcessDecisionAlerts("),
    "M3: decision -> prediction -> plan -> alert ordering is broken",
)

require(
    "ProcessDecisionOwnedWatchReactionAlerts(" in stages and
    "RenderCalculationState(" in stages and
    "ProcessQueuedAlertSoundDelivery();" in cycle,
    "M3: calculation cycle must connect analysis, presentation and alert transport",
)

require(
    live.index("RefreshLiveDecisionActionability(") <
    live.index("BuildSignalVisualSnapshot(") <
    live.index("RenderLatestAlertSignalMarker(") <
    live.index("RenderPanel();"),
    "M3: live presentation must consume the refreshed decision before provider/panel state is published",
)

require(
    "private const int CompactPlanLineLengthBars = 40;" in lines and
    "return LineStyle.Solid;" in lines and
    "PlanLinePresentationRule.ResolveThickness(" in lines and
    "line.ExtendToInfinity =" in lines and
    "line.IsInteractive =" in lines,
    "M3: signal-line geometry contract regressed",
)

require(
    "private string PlanTimeframeTag()" in labels and
    "ResolveCanonicalPlanSourceTimeframe()" in labels and
    "ResolveCanonicalPlanSourceTimeframe()" in labels,
    "M3: label source-timeframe resolver missing",
)

require(
    "return\n                \"(\" +" in labels and
    "ResolveCanonicalPlanSourceTimeframe()" in labels,
    "M3: main plan label still needs real source timeframe",
)

require(
    "RenderLatestAlertSignalMarker(" in alert_renderer and
    "Chart.DrawIcon(" not in alert_renderer,
    "M3: legacy alert mirror must not create a duplicate chart marker",
)

require(
    "lineLeft - offset" in label_anchor and
    "Chart.RemoveObject(" in label_renderer and
    "return semanticColor;" in label_renderer and
    "GetReadableLabelTextColor(" in label_renderer,
    "M3: compact signal labels must stay left of the line, background-free and line-colored",
)

require(
    "ContractIdentity identity" in provider and
    "new SignalEnvelope(" in provider and
    "_cfipProviderEnvelope" in provider and
    "_lastSignalEnvelope" in cbot and
    "SignalEnvelope" in cbot,
    "M3: Indicator -> provider -> cBot identity chain is incomplete",
)

require(
    "audit_phase_m3_trade_truth.py" in workflow,
    "M3: phase audit is not wired into Source/Architecture CI",
)

print("=" * 72)
print("M3 Single Trade Truth / Alert-Chart Coherence audit: PASS")
print("=" * 72)
