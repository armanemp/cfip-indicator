using System;
using cAlgo.API;

namespace cAlgo
{
    public partial class CFIPIndicator : Indicator
    {
        private bool TryResolveAutomaticPostFillTarget(
            int closedM5,
            Position position,
            ref double target)
        {
            double structuralTarget =
                AutoTarget(
                    _plan,
                    EffectiveAutoTpStage());

            if (IsValidTarget(
                    _plan.Direction,
                    position.EntryPrice,
                    structuralTarget))
            {
                target =
                    NormalizePrice(
                        structuralTarget);
                return true;
            }

            if (position.TakeProfit.HasValue &&
                IsValidTarget(
                    _plan.Direction,
                    position.EntryPrice,
                    position.TakeProfit.Value))
            {
                target =
                    NormalizePrice(
                        position.TakeProfit.Value);
                return true;
            }

            if (IsValidTarget(
                    _plan.Direction,
                    position.EntryPrice,
                    _plan.Tp1))
            {
                target =
                    NormalizePrice(
                        _plan.Tp1);
                return true;
            }

            _autoExecutionBlockReason =
                "POST-FILL TARGET INVALID";

            SetAutoTradingState(
                "ERROR",
                "POSITION OPENED • NO VALID TARGET");

            if (!RequestLivePlanExit(
                    closedM5,
                    "POST-FILL TARGET INVALID"))
            {
                SetLifecycleState(
                    LifecycleState.RecoveryRequired,
                    "POST-FILL TARGET INVALID • CLOSE REJECTED");
            }

            return false;
        }
    }
}
