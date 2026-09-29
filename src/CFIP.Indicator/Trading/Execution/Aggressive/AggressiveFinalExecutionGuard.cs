using System;
using cAlgo.API;

namespace cAlgo
{
    public partial class CFIPIndicator : Indicator
    {
        private bool TryValidateAggressiveFinalExecution(
            int closedM5,
            TradeType tradeType,
            double entry,
            double volume,
            out string reason)
        {
            reason = string.Empty;

            if (!CanRunAutomaticEntry())
            {
                ApplyRuntimeEntryGate();
                reason = "AGGRESSIVE • RUNTIME ENTRY BLOCKED";
                return false;
            }

            RefreshLiveDecisionActionability(
                closedM5);

            if (_reaction == null ||
                _reaction.Direction == 0)
            {
                reason = "AGGRESSIVE • NO REACTION";
                return false;
            }

            if (_decision == null ||
                !_decision.ActionableNow)
            {
                reason =
                    "AGGRESSIVE • " +
                    (_decision == null
                        ? "NO DECISION"
                        : string.IsNullOrWhiteSpace(
                            _decision.ActionabilityReason)
                            ? "ENTRY NOT ACTIONABLE"
                            : _decision.ActionabilityReason);
                return false;
            }

            if (!EnsureTradingPermission())
            {
                reason =
                    "AGGRESSIVE • TRADING PERMISSION NOT GRANTED";
                return false;
            }

            string suitabilityReason;

            if (!PassesMarketSuitability(
                    closedM5,
                    _reaction.Direction,
                    out suitabilityReason,
                    true))
            {
                reason =
                    "AGGRESSIVE • SUITABILITY • " +
                    suitabilityReason;
                return false;
            }

            if (!IsFinitePositive(entry))
            {
                reason =
                    "AGGRESSIVE • INVALID LIVE ENTRY";
                return false;
            }

            return true;
        }
    }
}
