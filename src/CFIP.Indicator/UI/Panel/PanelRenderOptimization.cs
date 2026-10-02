using System;
using cAlgo.API;

namespace cAlgo
{
    public partial class CFIPIndicator : Indicator
    {
        private bool ShouldRenderFullPanel(
            SignalVisualSnapshot snapshot)
        {
            if (!_initializationReady)
                return true;

            string key =
                BuildPanelPresentationKey(
                    snapshot);

            if (string.Equals(
                    key,
                    _lastPanelPresentationKey,
                    StringComparison.Ordinal))
                return false;

            _lastPanelPresentationKey = key;
            return true;
        }

        private string BuildPanelPresentationKey(
            SignalVisualSnapshot snapshot)
        {
            // Presentation-key construction is read-only. G5 invalidates the
            // canonical state snapshot only when an authoritative input changes.
            EnsurePanelExecutionProtectionStateCache();

            if (snapshot == null)
            {
                return
                    "NULL|" +
                    (_panelHidden ? "1" : "0") +
                    "|" +
                    (ShowUnifiedPanel ? "1" : "0");
            }

            return string.Join(
                "|",
                ShowUnifiedPanel ? "1" : "0",
                _panelHidden ? "1" : "0",
                snapshot.Stage ?? "",
                snapshot.AuthoritativeDirection,
                snapshot.DecisionDirection,
                snapshot.TopDownStage ?? "",
                snapshot.TopDownEligible ? "1" : "0",
                snapshot.PlanActive ? "1" : "0",
                snapshot.PendingOrder ? "1" : "0",
                snapshot.LivePosition ? "1" : "0",
                FramePresentationKey(_m1Frame),
                FramePresentationKey(_m5Frame),
                FramePresentationKey(_m15Frame),
                FramePresentationKey(_m30Frame),
                FramePresentationKey(_h1Frame),
                FramePresentationKey(_h4Frame),
                FramePresentationKey(_d1Frame),
                FramePresentationKey(_w1Frame),
                snapshot.Confidence,
                snapshot.SmartQuality,
                snapshot.TimeframeAgreement,
                snapshot.TriggerRuntimeReady ? "1" : "0",
                snapshot.TriggerRuntimeScore,
                snapshot.TriggerRuntimeRequired,
                PriceKey(snapshot.Entry),
                PriceKey(snapshot.IdealEntry),
                PriceKey(snapshot.Trigger),
                PriceKey(snapshot.Stop),
                PriceKey(snapshot.Tp1),
                PriceKey(snapshot.Tp2),
                PriceKey(snapshot.Tp3),
                PriceKey(snapshot.Tp4),
                PriceKey(snapshot.BrokerStop),
                PriceKey(snapshot.BrokerTarget),
                _lifecycleState.ToString(),
                _autoTradingState ?? "",
                _autoTradingReason ?? "",
                _autoExecutionBlockReason ?? "",
                _autoOrdersBlockReason ?? "",
                ExecutionPanelPresentationIdentityRule.Compose(
                    _autoTradingState,
                    _autoTradingReason,
                    _lastExecutionTelemetryPath,
                    _lastExecutionTelemetryState,
                    _activeExecutionScenarioId,
                    _marketSuitabilityScore,
                    _marketSuitabilityState,
                    _marketSuitabilityReason,
                    _lastBreakEvenDiagnostic),
                GetAutoTradingPanelState(),
                GetAutoOrdersPanelState(),
                GetAutoProtectionPanelState(),
                _brokerProtectionRecoveryRequired ? "1" : "0");
        }

        private string FramePresentationKey(
            Frame frame)
        {
            if (frame == null)
                return "NULL";

            return string.Join(
                ":",
                frame.Index,
                frame.Direction,
                frame.Quality,
                frame.Evidence);
        }

        private string PriceKey(
            double price)
        {
            if (!IsFinitePositive(price))
                return "0";

            return NormalizePrice(price)
                .ToString(
                    "G17",
                    System.Globalization.CultureInfo.InvariantCulture);
        }
    }
}