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

            ProcessCanonicalActionableEntryAlert(
                closedM5);
        }

        private void ProcessCanonicalActionableEntryAlert(
            int closedM5)
        {
            if (_lastActionableEntryAlertM5 ==
                closedM5 ||
                !_decision.EntryAllowed ||
                !_decision.ActionableNow ||
                GetManagedPendingOrder() != null ||
                _plan != null ||
                _decision.Direction == 0 ||
                _plan == null ||
                _plan.IsLivePosition)
                return;

            int direction =
                _plan.Direction;

            string source = "CONFIRMED";

            if (AlertOnSmartDecision &&
                _decision.SmartQuality >=
                    SmartStrongSetupQuality &&
                _decision.Edge >=
                    SmartStrongSetupEdge)
            {
                source = "SMART";
            }
            else if (AlertOnHighConfidenceEntry &&
                     _decision.Confidence >=
                     HighConfidenceThreshold)
            {
                source = "HIGH";
            }
            else if (!AlertOnConfirmedSignal)
            {
                return;
            }

            string directionText =
                direction == 1
                    ? "BUY"
                    : "SELL";

            string message =
                "CFIP ACTIONABLE " +
                directionText +
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
                " | CONF " +
                _decision.Confidence +
                " | LOC " +
                _decision.EntryLocationQuality +
                " | TIMING " +
                _decision.EntryTimingQuality +
                " | POS " +
                _decision.EntryPositionQuality +
                " | DIV " +
                (_decision.DivergenceType ?? "NONE") +
                " Q" +
                _decision.DivergenceQuality +
                " | " +
                (_decision.ActionabilityReason ?? "ACTIONABLE");

            SendUnifiedAlert(
                "ACTION|" +
                closedM5 +
                "|" +
                _decision.Direction,
                message,
                direction,
                true);

            _lastActionableEntryAlertM5 =
                closedM5;
        }

        private void ProcessRestrictionAlert(
            int closedM5)
        {
            bool actionabilityBlocked =
                _decision.EntryAllowed &&
                !_decision.ActionableNow &&
                !string.IsNullOrWhiteSpace(
                    _decision.ActionabilityReason);

            string effectiveRestriction =
                actionabilityBlocked
                    ? "ACTIONABILITY • " +
                      _decision.ActionabilityReason
                    : _decision.BlockReason;

            if ((!_decision.EntryAllowed ||
                 actionabilityBlocked) &&
                RestrictionAlertEnabled(
                    effectiveRestriction) &&
                !string.IsNullOrWhiteSpace(
                    effectiveRestriction))
            {
                string restrictionMessage =
                    effectiveRestriction.Trim();

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

            if (_decision.EntryAllowed &&
                _decision.ActionableNow)
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
