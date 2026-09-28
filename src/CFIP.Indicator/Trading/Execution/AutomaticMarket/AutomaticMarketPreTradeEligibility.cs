using System;
using cAlgo.API;

namespace cAlgo
{
    public partial class CFIPIndicator : Indicator
    {
        private bool PassAutomaticMarketPreTradeEligibility(
            int closedM5)
        {
            if (!AutoTradingEnabled)
            {
                _lastAutoPlanAttemptM5 =
                    -1;

                SetAutoTradingState(
                    "OFF",
                    "DISABLED");
                return false;
            }

            string capacityReason;
            if (!ValidateConfiguredPositionCapacity(
                    out capacityReason))
            {
                _autoExecutionBlockReason =
                    capacityReason;
                SetAutoTradingState(
                    "BLOCKED",
                    capacityReason);
                return false;
            }

            PendingOrder existingPending =
                GetManagedPendingOrder();

            if (existingPending != null)
            {
                _autoExecutionBlockReason =
                    "PENDING ORDER EXISTS";
                SetAutoTradingState(
                    "ARMED",
                    "WAITING FOR PENDING ORDER");
                return false;
            }

            if (DailyLossLimitHit(
                    TimeInUtc))
            {
                _autoExecutionBlockReason =
                    "DAILY LOSS LIMIT";
                SetAutoTradingState(
                    "BLOCKED",
                    "DAILY LOSS LIMIT REACHED");
                return false;
            }

            if (_plan == null)
            {
                EnsureSignalPlan(
                    closedM5,
                    ConfirmedSignalsOnly
                        ? DecisionPolicyMode.Confirmed
                        : DecisionPolicyMode.Soft);

                if (_plan == null)
                {
                    if (_executionModel != null &&
                        _executionModel.Mode ==
                            ExecutionMode.WaitingForTrigger)
                    {
                        _autoExecutionBlockReason =
                            "WAITING FOR TRIGGER";

                        SetAutoTradingState(
                            "ARMED",
                            "WAITING FOR TRIGGER");
                    }
                    else
                    {
                        _autoExecutionBlockReason =
                            "NO ELIGIBLE PLAN";

                        SetAutoTradingState(
                            "ARMED",
                            ConfirmedSignalsOnly
                                ? "WAITING FOR CONFIRMED PLAN"
                                : "WAITING FOR SMART-ELIGIBLE PLAN");
                    }

                    return false;
                }
            }

            if (_decision == null ||
                _decision.Direction == 0)
            {
                _autoExecutionBlockReason =
                    "NO DECISION";
                SetAutoTradingState(
                    "ARMED",
                    "WAITING FOR DECISION");
                return false;
            }

            if (!_decision.EntryAllowed)
            {
                bool liveGate =
                    !ConfirmedSignalsOnly &&
                    IsLiveExecutionGateReason(
                        _decision.BlockReason);

                if (!liveGate)
                {
                    string reason =
                        string.IsNullOrWhiteSpace(
                            _decision.BlockReason)
                            ? "WAITING FOR CONFIRMATION"
                            : _decision.BlockReason;

                    _autoExecutionBlockReason =
                        reason;

                    SetAutoTradingState(
                        "ARMED",
                        reason);
                    return false;
                }
            }

            if (OneOrderPerSignal &&
                _lastAutoM5 == closedM5)
            {
                _autoExecutionBlockReason =
                    "ALREADY TRADED THIS M5";
                SetAutoTradingState(
                    "ARMED",
                    "ALREADY TRADED THIS M5");
                return false;
            }

            if (_decision.Confidence <
                MinimumAutoConfidence)
            {
                string reason =
                    "CONF " +
                    _decision.Confidence +
                    " < " +
                    MinimumAutoConfidence;

                _autoExecutionBlockReason =
                    reason;
                SetAutoTradingState(
                    "BLOCKED",
                    reason);
                return false;
            }

            if (_decision.SmartQuality <
                MinimumAutoSmartQuality)
            {
                string reason =
                    "SMART Q " +
                    _decision.SmartQuality +
                    " < " +
                    MinimumAutoSmartQuality;

                _autoExecutionBlockReason =
                    reason;
                SetAutoTradingState(
                    "BLOCKED",
                    reason);
                return false;
            }

            string suitabilityReason;

            if (!PassesMarketSuitability(
                    closedM5,
                    _plan.Direction,
                    out suitabilityReason))
            {
                _autoExecutionBlockReason =
                    "SUITABILITY • " +
                    suitabilityReason;
                SetAutoTradingState(
                    "BLOCKED",
                    suitabilityReason);
                return false;
            }

            int levelQuality =
                Math.Min(
                    _plan.StopQuality,
                    _plan.Tp1Quality);

            if (levelQuality <
                MinimumAutoLevelQuality)
            {
                string reason =
                    "LEVEL Q " +
                    levelQuality +
                    " < " +
                    MinimumAutoLevelQuality;

                _autoExecutionBlockReason =
                    reason;
                SetAutoTradingState(
                    "BLOCKED",
                    reason);
                return false;
            }

            if (ManagedPositionCount() >=
                Math.Max(
                    1,
                    MaximumOpenPositions))
            {
                _autoExecutionBlockReason =
                    "MAX OPEN POSITIONS";
                SetAutoTradingState(
                    "BLOCKED",
                    "MAX OPEN POSITIONS");
                return false;
            }

            return true;
        }
    }
}
