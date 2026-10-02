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
            if (!ValidateSingleExecutionCapacity(
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

            string dailyLossReason;

            if (DailyLossLimitHit(
                    TimeInUtc,
                    out dailyLossReason))
            {
                _autoExecutionBlockReason =
                    dailyLossReason;

                SetAutoTradingState(
                    "BLOCKED",
                    dailyLossReason);
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

            // Re-evaluate the final market-entry state immediately before
            // any market execution gate. This prevents a stale closed-bar
            // ActionableNow value from authorizing a moved quote/plan.
            RefreshLiveDecisionActionability(
                closedM5);

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

            if (_m5Frame != null)
            {
                IndicatorQualityGateResult indicatorGate =
                    IndicatorExecutionQualityRule.EvaluateIndicatorExecutionQuality(
                        IndicatorQualityGateStage.AutomaticMarket,
                        _m5Frame.IndicatorConfluenceQuality,
                        _m5Frame.IndicatorConflict);

                if (!indicatorGate.Allowed)
                {
                    _autoExecutionBlockReason =
                        indicatorGate.Reason;

                    SetAutoTradingState(
                        "BLOCKED",
                        indicatorGate.Reason);
                    return false;
                }
            }

            string suitabilityReason;

            if (!PassesMarketSuitability(
                    closedM5,
                    _plan.Direction,
                    out suitabilityReason,
                    true))
            {
                _autoExecutionBlockReason =
                    "SUITABILITY • " +
                    suitabilityReason;
                SetAutoTradingState(
                    "BLOCKED",
                    suitabilityReason);
                return false;
            }

            if (_m5Bars != null &&
                _plan != null)
            {
                CanonicalPriceSnapshot priceSnapshot =
                    GetCanonicalPriceSnapshot();

                if (priceSnapshot == null ||
                    !priceSnapshot.IsQuoteValid)
                {
                    _autoExecutionBlockReason =
                        "INVALID CANONICAL MARKET QUOTE";
                    SetAutoTradingState(
                        "BLOCKED",
                        _autoExecutionBlockReason);
                    return false;
                }

                double liveEntry =
                    NormalizePrice(
                        priceSnapshot.GetExecutablePrice(
                            _plan.Direction));

                double stopRisk =
                    Math.Abs(
                        liveEntry -
                        _plan.Stop);

                double spread =
                    Math.Max(
                        0,
                        priceSnapshot.Spread);

                double spreadRiskRatio =
                    stopRisk > 0
                        ? spread / stopRisk
                        : double.MaxValue;

                if (spreadRiskRatio >
                    ExecutionThresholdPolicy.NormalizeMaximumSpreadToStopRiskRatio(
                        MaximumSpreadToStopRiskRatio))
                {
                    _autoExecutionBlockReason =
                        "SPREAD / STOP RISK";

                    SetAutoTradingState(
                        "BLOCKED",
                        _autoExecutionBlockReason);
                    return false;
                }
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

            if (_decision == null ||
                !_decision.ActionableNow)
            {
                string reason =
                    _decision == null
                        ? "NO DECISION"
                        : string.IsNullOrWhiteSpace(
                            _decision.ActionabilityReason)
                            ? "ENTRY NOT ACTIONABLE"
                            : _decision.ActionabilityReason;

                _autoExecutionBlockReason =
                    reason;

                SetAutoTradingState(
                    "BLOCKED",
                    reason);

                return false;
            }

            if (_plan != null)
            {
                CanonicalPriceSnapshot priceSnapshot =
                    GetCanonicalPriceSnapshot();

                if (priceSnapshot == null ||
                    !priceSnapshot.IsQuoteValid)
                {
                    _autoExecutionBlockReason =
                        "INVALID CANONICAL MARKET QUOTE";
                    SetAutoTradingState(
                        "BLOCKED",
                        _autoExecutionBlockReason);
                    return false;
                }

                double liveEntry =
                    NormalizePrice(
                        priceSnapshot.GetExecutablePrice(
                            _plan.Direction));

                ExecutionPlanGeometryResult geometry =
                    ExecutionPlanGeometryRule.Evaluate(
                        _plan.Direction,
                        liveEntry,
                        _plan.Stop,
                        _plan.Tp1,
                        priceSnapshot.Spread,
                        MinimumRequiredRRForRegime(
                            _decision.Regime),
                        MaximumRewardRR,
                        Symbol.PipSize);

                if (!geometry.Allowed)
                {
                    _autoExecutionBlockReason =
                        "PLAN GEOMETRY • " +
                        geometry.Reason;

                    SetAutoTradingState(
                        "BLOCKED",
                        _autoExecutionBlockReason);

                    return false;
                }
            }

            return true;
        }
    }
}
