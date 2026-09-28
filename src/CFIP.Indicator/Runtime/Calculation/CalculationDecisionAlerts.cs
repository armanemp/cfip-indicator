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

            ProcessHighConfidenceAlert(
                closedM5);

            ProcessRestrictionAlert(
                closedM5);

            ProcessSmartDecisionAlert(
                closedM5);
        }

        private void ProcessHighConfidenceAlert(
            int closedM5)
        {
            if (!AlertOnHighConfidenceEntry ||
                _decision.Confidence <
                HighConfidenceThreshold ||
                _lastHighConfidenceM5 ==
                closedM5)
                return;

            SendUnifiedAlert(
                "HIGH|" +
                closedM5,
                "CFIP HIGH CONFIDENCE | " +
                (_decision.Direction == 1
                    ? "BUY"
                    : "SELL") +
                " | CONF " +
                _decision.Confidence,
                _decision.Direction,
                true);

            _lastHighConfidenceM5 =
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

        private void ProcessSmartDecisionAlert(
            int closedM5)
        {
            if (!AlertOnSmartDecision ||
                !_decision.EntryAllowed ||
                _decision.SmartQuality <
                SmartStrongSetupQuality ||
                _decision.Edge <
                SmartStrongSetupEdge ||
                _lastSmartDecisionAlertM5 ==
                closedM5)
                return;

            SendUnifiedAlert(
                "SMART|" +
                closedM5,
                "CFIP SMART DECISION | " +
                (_decision.Direction == 1
                    ? "BUY"
                    : "SELL") +
                " | Q " +
                _decision.SmartQuality +
                " | CONF " +
                _decision.Confidence,
                _decision.Direction,
                true);

            _lastSmartDecisionAlertM5 =
                closedM5;
        }
    }
}
