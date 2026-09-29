using System;
using cAlgo.API;

namespace cAlgo
{
    public partial class CFIPIndicator : Indicator
    {
        private bool TryAssessCurrentPlanActionability(
            int closedM5,
            out SignalActionabilityResult result)
        {
            result =
                new SignalActionabilityResult(
                    false,
                    "NOT EVALUATED");

            if (_decision == null ||
                !_decision.EntryAllowed ||
                (_decision.Direction != 1 &&
                 _decision.Direction != -1))
                return false;

            if (_plan == null ||
                _plan.IsLivePosition)
            {
                result =
                    new SignalActionabilityResult(
                        false,
                        "NO PRE-TRADE PLAN");
                return false;
            }

            int ageBars =
                Math.Max(
                    0,
                    closedM5 -
                    _plan.CreatedM5);

            double atr =
                _m5Bars != null &&
                closedM5 >= 1
                    ? Atr(
                        _m5Bars,
                        closedM5)
                    : 0;

            if (!IsFinitePositive(atr))
            {
                result =
                    new SignalActionabilityResult(
                        false,
                        "ATR INVALID");
                return false;
            }

            double market =
                _plan.Direction == 1
                    ? Symbol.Ask
                    : Symbol.Bid;

            if (!IsFinitePositive(market))
            {
                result =
                    new SignalActionabilityResult(
                        false,
                        "MARKET INVALID");
                return false;
            }

            bool executionReady =
                _executionModel != null &&
                _executionModel.Direction ==
                    _plan.Direction &&
                _executionModel.Ready &&
                (_executionModel.Mode ==
                    ExecutionMode.RetestMarket ||
                 _executionModel.Mode ==
                    ExecutionMode.BreakoutMarket);

            if (!executionReady &&
                _plan.EntryMode !=
                    ExecutionMode.RetestMarket &&
                _plan.EntryMode !=
                    ExecutionMode.BreakoutMarket)
            {
                result =
                    new SignalActionabilityResult(
                        false,
                        "ENTRY NOT READY");
                return false;
            }

            DivergenceResult divergence =
                AnalyzeDivergence(
                    _m5Bars,
                    closedM5);

            int locationQuality =
                EntryLocationQuality(
                    _m5Bars,
                    closedM5,
                    _plan.Direction);

            bool opposingRegular =
                divergence.IsRegular &&
                divergence.Direction ==
                    -_plan.Direction;

            double minimumRR =
                Math.Max(
                    Tp1MinimumRR,
                    MinimumRequiredRR());

            result =
                SignalActionabilityRule.Evaluate(
                    _plan.Direction,
                    _plan.Tp1RR,
                    minimumRR,
                    locationQuality,
                    45,
                    divergence.Direction,
                    divergence.Quality,
                    opposingRegular,
                    divergence.AgeBars,
                    ageBars,
                    executionReady,
                    GetManagedPendingOrder() != null,
                    market,
                    _plan.EntryZoneLow,
                    _plan.EntryZoneHigh,
                    atr);

            return result.Allowed;
        }

        private bool IsCurrentSignalActionable(
            int closedM5,
            out string reason)
        {
            SignalActionabilityResult result;

            bool allowed =
                TryAssessCurrentPlanActionability(
                    closedM5,
                    out result);

            reason =
                result.Reason;

            return allowed;
        }
    }
}
