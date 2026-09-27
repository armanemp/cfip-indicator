// CFIP Indicator — BrokerPositionMutation.cs
// The sole broker position mutation boundary.

using System;
using cAlgo.API;

namespace cAlgo
{
    public partial class CFIPIndicator : Indicator
    {
        private bool TryModifyStopLoss(
                            Position position,
                            double price,
                            string context)
                        {
                            if (position == null ||
                                !IsFinitePositive(price))
                                return false;
                
                            double normalized =
                                NormalizePrice(price);
                
                            if (!IsFinitePositive(normalized))
                                return false;
                
                            try
                            {
                                TradeResult result =
                                    position.ModifyStopLossPrice(normalized);
                
                                if (result == null ||
                                    !result.IsSuccessful)
                                {
                                    Print(
                                        "CFIP SL mutation rejected ({0}).",
                                        context);
                                    return false;
                                }
                
                                return true;
                            }
                            catch (Exception ex)
                            {
                                Print(
                                    "CFIP SL mutation failed ({0}): {1}",
                                    context,
                                    ex.Message);
                                return false;
                            }
                        }

        private bool TryModifyTakeProfit(
                            Position position,
                            double price,
                            string context)
                        {
                            if (position == null ||
                                !IsFinitePositive(price))
                                return false;
                
                            double normalized =
                                NormalizePrice(price);
                
                            if (!IsFinitePositive(normalized))
                                return false;
                
                            try
                            {
                                TradeResult result =
                                    position.ModifyTakeProfitPrice(normalized);
                
                                if (result == null ||
                                    !result.IsSuccessful)
                                {
                                    Print(
                                        "CFIP TP mutation rejected ({0}).",
                                        context);
                                    return false;
                                }
                
                                return true;
                            }
                            catch (Exception ex)
                            {
                                Print(
                                    "CFIP TP mutation failed ({0}): {1}",
                                    context,
                                    ex.Message);
                                return false;
                            }
                        }

        private bool TryClosePosition(
                            Position position,
                            string context,
                            double? volumeInUnits = null)
                        {
                            if (position == null)
                                return false;
                
                            try
                            {
                                TradeResult result =
                                    volumeInUnits.HasValue
                                        ? ClosePosition(
                                            position,
                                            volumeInUnits.Value)
                                        : ClosePosition(position);
                
                                if (result == null ||
                                    !result.IsSuccessful)
                                {
                                    Print(
                                        "CFIP close rejected ({0}).",
                                        context);
                                    return false;
                                }
                
                                return true;
                            }
                            catch (Exception ex)
                            {
                                Print(
                                    "CFIP close failed ({0}): {1}",
                                    context,
                                    ex.Message);
                                return false;
                            }
                        }

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
