// CFIP Indicator — BrokerProtectionCoordinator.cs
// Single-responsibility broker mutation module.

using System;
using cAlgo.API;

namespace cAlgo
{
    public partial class CFIPIndicator : Indicator
    {
        private bool EnsureBrokerProtectionForPosition(
                            Position position,
                            double stop,
                            double target,
                            string context,
                            int direction)
                        {
                            bool stopOk =
                                TryModifyStopLoss(
                                    position,
                                    stop,
                                    context + " • SL");
                
                            bool targetOk =
                                TryModifyTakeProfit(
                                    position,
                                    target,
                                    context + " • TP");
                
                            bool protectedOk =
                                stopOk &&
                                targetOk;
                
                            _brokerProtectionRecoveryRequired =
                                !protectedOk;
                
                            if (!protectedOk)
                            {
                                SetLifecycleState(
                                    LifecycleState.RecoveryRequired,
                                    context +
                                    " • BROKER PROTECTION REJECTED");
                
                                SendUnifiedAlert(
                                    "PROTECTION-REJECTED|" +
                                    position.Id,
                                    "CFIP BROKER PROTECTION REJECTED | #" +
                                    position.Id,
                                    direction,
                                    true);
                            }
                
                            return protectedOk;
                        }
    }
}
