from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
IND = ROOT / "src" / "CFIP.Indicator"

def read(rel: str) -> str:
    return (IND / rel).read_text(encoding="utf-8")

def root_read(rel: str) -> str:
    return (ROOT / rel).read_text(encoding="utf-8")

def check(condition: bool, message: str) -> None:
    if not condition:
        raise SystemExit(message)

lamp = read("UI/Panel/PanelTrendTimeframeLampRow.cs")
panel = read("UI/Panel/PanelMainRenderer.cs")
surface = read("UI/Panel/Theme/PanelSurfaceAndHeaderLayout.cs")
queue = read("Core/Runtime/AlertDeliveryQueue.cs")
alerts = read("Trading/Alerts/AlertEngine.cs")
processor = read("UI/Panel/AlertDeliveryProcessor.cs")
contracts = root_read("tools/CFIP.Runtime.Contracts/Program.cs")
workflow = root_read(".github/workflows/source-check.yml")

check(
    "new StackPanel" in lamp and
    "_panelTrendTimeframeLampIndicators" in lamp and
    "_panelTrendTimeframeLampLabels" in lamp and
    'indicator.Text = "●"' in lamp and
    'label.Text = labels[i]' in lamp,
    "MTF lamps must render indicator and timeframe label as separate vertical elements",
)

check(
    "PanelTrendTimeframeLampTopSpacing" in lamp and
    "PanelTrendTimeframeLampBottomSpacing" in lamp and
    "PanelTrendTimeframeLampTopSpacing" in panel and
    "PanelTrendTimeframeLampBottomSpacing" in panel and
    "PanelTrendTimeframeLampTopSpacing" in surface,
    "MTF lamp spacing must be part of deterministic panel geometry",
)

check(
    "int alertRailHeight" in panel and
    "GetPanelAlertMessageRailHeight()" in panel and
    "ResolvePanelFooterAreaHeight(" in panel and
    "PanelTrendTimeframeLampRowHeight" in panel and
    "PanelTrendTimeframeLampTopSpacing" in panel and
    "PanelTrendTimeframeLampBottomSpacing" in panel,
    "panel height budget must reserve the real alert footer and two-line MTF rail",
)

check(
    "ContractIdentity identity" in queue and
    "identity.CreatedClosedM5" in queue and
    "delivery.Envelope.AlertKey" in queue and
    "ContractIdentity identity" in queue and
    "identity.CreatedClosedM5" in queue and
    "delivery.Envelope.AlertKey" in queue and
    "_pendingAlertIds" in queue and
    "_pendingAlertIds.Contains(alertId)" in queue and
    "_pendingAlertIds.Add(alertId)" in queue and
    "RemovePendingAlertId(delivery)" in queue and
    "_pendingAlertIds.Clear()" in queue,
    "alert delivery queue must provide pending canonical-event idempotency",
)

check(
    'queue.Enqueue(normal1) &&\n                !queue.Enqueue(normal1)' in contracts and
    "same alert may be re-armed after it is actually delivered" in contracts and
    "AlertDeliveryQueueContracts.Run();" in contracts,
    "runtime contracts must cover queue duplicate suppression and post-delivery rearming",
)

check(
    "RewardDistanceAtr =" in read("Analysis/Market/ParallelOpportunityCandidateBuilder.cs") and
    "MinimumRequiredRewardDistanceAtr =" in read("Analysis/Market/ParallelOpportunityCandidateBuilder.cs") and
    "RegimeAdaptiveRewardFloorRule.ResolveAdaptiveRewardFloor(" in read("Analysis/Market/ParallelOpportunityCandidateBuilder.cs"),
    "TradeOpportunityCandidate reward-distance fields must be assigned by the canonical parallel-candidate builder",
)

check(
    "Notifications.PlaySound(" not in alerts and
    processor.count("Notifications.PlaySound(") == 3 and
    "RecordPanelAlertDelivery(next)" in processor,
    "Indicator signal sound must keep one delivery owner and one queued event boundary",
)

check(
    "tools/audit_phase_panel_footer_alert_dedup_2026_10_03.py" in workflow,
    "new footer/alert dedup audit must run in Source/Architecture CI",
)

# Second-pass footer/alert coherence checks.
layout = read("UI/Panel/PanelLayoutManager.cs")
visual = read("UI/Panel/Theme/PanelVisualSettings.cs")
heartbeat = read("UI/Panel/ProcessingHeartbeatLamp.cs")
decision_alerts = read("Runtime/Calculation/CalculationDecisionAlerts.cs")
context_alerts = read("Trading/Alerts/ContextAlertEmitter.cs")
constants = read("UI/Panel/PanelConstants.cs")
canonical_arrows = read("UI/Chart/SignalStackedArrowRenderer.cs")

check(
    "PanelFooterMinHeight = 34" in constants and
    "PanelFooterButtonInternalMargin = 2" in constants and
    "ResolvePanelFooterAreaHeight(" in layout and
    "ResolvePanelFooterAreaHeight(" in panel and
    "ResolvePanelFooterAreaHeight(" in visual and
    "minimumRenderableHeight" in layout and
    "PanelTrendTimeframeLampRowHeight" in panel and
    "PanelTrendTimeframeLampTopSpacing" in panel,
    "footer height must have one shared minimum and constrained-height safety",
)

check(
    "Math.Max(1, _panelTrendTimeframeLampRow.Width)" in lamp and
    "rowWidth / cellCount" in lamp and
    "FontSize = 10" in lamp and
    "Math.Max(224" not in lamp and
    "Math.Max(28" not in lamp,
    "MTF lamp cells must divide the real panel width without narrow-panel overflow",
)

check(
    "PanelStatusLampFontSize = 20" in constants and
    "PanelStatusLampWidth = 30" in constants and
    "PanelTrendTimeframeLampRowHeight" in visual and
    "PanelTrendTimeframeLampTopSpacing" in visual and
    "PanelStatusLampHeight = 30" in constants and
    "PanelStatusLampFontSize" in lamp and
    "PanelStatusLampFontSize" in heartbeat and
    "PanelAlertMessageVisibleCapacity = 2" in read("UI/Panel/PanelAlertMessageRenderer.cs") and
    "TextWrapping = TextWrapping.NoWrap" in read("UI/Panel/PanelAlertMessageRenderer.cs") and
    "TextTrimming = TextTrimming.None" in read("UI/Panel/PanelAlertMessageRenderer.cs"),
    "header and MTF lamps must share the enlarged status-lamp geometry",
)

check(
    "string canonicalScenarioId = string.Empty;" in decision_alerts and
    "candidateScenarioId" in decision_alerts and
    "ResolveProviderScenarioId(" in decision_alerts,
    "the primary canonical scenario must not be emitted by both alert owners",
)

check(
    "private bool SendUnifiedAlert(" in alerts and
    "return delivered;" in alerts and
    "retryable=true" in alerts and
    "if (queued &&" in alerts and
    "Notifications.PlaySound(" not in alerts,
    "alert acknowledgement must occur at queue acceptance and playback must stay centralized",
)

check(
    "StructuralEvidenceRule.HasCanonicalStructuralEvent(" in context_alerts,
    "structural alerts must retain one canonical structural event owner",
)

sound_policy = read("Trading/Alerts/AlertSoundPolicy.cs")
check(
    "MaxRememberedSignalSoundGroups = 256" in processor and
    "_rememberedSignalSoundGroups" in processor and
    "_rememberedSignalSoundGroupOrder" in processor and
    "Contains(groupKey)" in processor and
    "delivery.SoundGroupKey" in processor and
    "public static AlertSoundDecision ResolveDecision(" in sound_policy and
    "BuildGroupKey(" in sound_policy and
    "CreatedClosedM5" in sound_policy and
    'identity.Direction.ToString() + "|" + key' in sound_policy,
    "signal sound dedup must consume the canonical policy group and preserve distinct semantic alert stages",
)

sound_map = root_read("src/CFIP.Indicator/Trading/Alerts/AlertSoundPolicy.cs")
check(
    "SoundType.Doorbell" in sound_map and
    "SoundType.PositiveNotification" in sound_map and
    "SoundType.NegativeNotification" in sound_map and
    "SoundType.Confirmation" in sound_map and
    "SoundType.Announcement" in sound_map and
    '"WATCH|"' in sound_map and
    '"REACTION|"' in sound_map and
    '"ACTION|"' in sound_map and
    '"TP"' in sound_map and
    '"SL|"' in sound_map and
    '"REVERSAL|"' in sound_map,
    "canonical alert sound mapping must use the single AlertSoundPolicy owner",
)

check(
    '"WATCH_ARROW"' in canonical_arrows and
    '"WATCH_ARROW_2"' in canonical_arrows and
    '"WATCH_ARROW_3"' in canonical_arrows and
    'ChartIconType.UpArrow' in canonical_arrows and
    'ChartIconType.DownArrow' in canonical_arrows and
    "snapshot.MtfTrendStrengthLevel" in canonical_arrows,
    "canonical signal arrows must use the canonical smart-strength stack",
)

check(
    "PanelFooterMinHeight = 34" in constants and
    "return contentHeight;" in layout and
    "PanelTrendTimeframeLampRowHeight = 38" in lamp and
    "PanelTrendTimeframeLampIndicatorHeight = 22" in lamp and
    "PanelAlertMessageRowHeight = 18" in read("UI/Panel/PanelAlertMessageRenderer.cs"),
    "footer geometry must be compact at the actual content boundary",
)

check(
    "_lastRenderedPanelAlertRevision" in read("UI/Panel/PanelAlertMessageRenderer.cs") and
    "if (_lastRenderedPanelAlertRevision ==" in read("UI/Panel/PanelAlertMessageRenderer.cs") and
    "Visibility follows current panel lifecycle" in read("UI/Panel/PanelAlertMessageRenderer.cs") and
    "RefreshPanelAlertFooterGeometry();" in read("UI/Panel/PanelAlertMessageRenderer.cs") and
    "ResolvePanelFooterAreaHeight(" in read("UI/Panel/PanelAlertMessageRenderer.cs") and
    "_buttonStack.Height" in read("UI/Panel/PanelAlertMessageRenderer.cs"),
    "panel alert delivery must resize the live footer without waiting for a full panel render",
)


print("Panel footer / MTF lamp / alert dedup audit PASS")
