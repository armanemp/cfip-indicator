using System;
using cAlgo.API;

namespace cAlgo
{
    public partial class CFIPIndicator : Indicator
    {
        private void ProcessDecisionAlerts(
            int closedM5)
        {
            if (_decision == null)
                return;

            ProcessRestrictionAlert(
                closedM5);

            ProcessCanonicalActionableSignalAlert(
                closedM5);
        }

        private void ProcessLiveActionableSignalAlert(
            int closedM5)
        {
            if (_decision == null)
                return;

            ProcessCanonicalActionableSignalAlert(
                closedM5);
        }

        private void ProcessCanonicalActionableSignalAlert(
            int closedM5)
        {
            if (_lastActionableAlertM5 ==
                closedM5)
                return;

            bool confirmedRequested =
                AlertOnConfirmedSignal;

            bool highRequested =
                AlertOnHighConfidenceEntry &&
                _decision.Confidence >=
                    HighConfidenceThreshold;

            bool smartRequested =
                AlertOnSmartDecision &&
                _decision.EntryAllowed &&
                _decision.SmartQuality >=
                    SmartStrongSetupQuality &&
                _decision.Edge >=
                    SmartStrongSetupEdge;

            if (!confirmedRequested &&
                !highRequested &&
                !smartRequested)
                return;

            string reason;
            if (!IsCurrentSignalActionable(
                    closedM5,
                    out reason))
                return;

            string source =
                smartRequested
                    ? "SMART"
                    : highRequested
                        ? "HIGH"
                        : "CONFIRMED";

            int locationQuality =
                EntryLocationQuality(
                    _m5Bars,
                    closedM5,
                    _plan.Direction);

            DivergenceResult divergence =
                AnalyzeDivergence(
                    _m5Bars,
                    closedM5);

            string message =
                "CFIP ACTIONABLE " +
                (_plan.Direction == 1
                    ? "BUY"
                    : "SELL") +
                " | " +
                source +
                " | ENTRY " +
                Price(_plan.Entry) +
                " | IDEAL " +
                Price(_plan.IdealEntry) +
                " | TRIGGER " +
                Price(_plan.EntryTrigger) +
                " | SL " +
                Price(_plan.Stop) +
                " | TP1 " +
                Price(_plan.Tp1) +
                " | RR " +
                _plan.Tp1RR.ToString("F2") +
                " | LOC " +
                locationQuality +
                " | DIV " +
                (divergence.Type ?? "NONE") +
                " Q" +
                divergence.Quality;

            SendUnifiedAlert(
                "ACTION|" +
                closedM5 +
                "|" +
                _plan.Direction,
                message,
                _plan.Direction,
                true);

            _lastActionableAlertM5 =
                closedM5;
        }

        private void ProcessRestrictionAlert(
            int closedM5)
        {
            if (!_decision.EntryAllowed &&
                RestrictionAlertEnabled(
                    _decision.BlockReason) &&
                !string.IsNullOrWhiteSpace(
                    _decision.BlockReason))
            {
                string restrictionMessage =
                    _decision.BlockReason.Trim();

                bool restrictionChanged =
                    !string.Equals(
                        _lastRestrictionMessage,
                        restrictionMessage,
                        StringComparison.OrdinalIgnoreCase);

                if (!restrictionChanged)
                    return;

                SendUnifiedAlert(
                    "RESTRICT|" +
                    restrictionMessage,
                    "CFIP ENTRY BLOCKED | " +
                    restrictionMessage,
                    _decision.Direction,
                    false);

                _lastRestrictionMessage =
                    restrictionMessage;

                _lastRestrictionAlertUtc =
                    TimeInUtc;

                _lastRestrictionM5 =
                    closedM5;

                return;
            }

            if (_decision.EntryAllowed)
            {
                _lastRestrictionMessage =
                    "";
                _lastRestrictionAlertUtc =
                    DateTime.MinValue;
                _lastRestrictionM5 =
                    -1;
            }
        }
    }
}