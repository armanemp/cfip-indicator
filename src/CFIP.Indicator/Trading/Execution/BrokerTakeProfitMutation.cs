// CFIP Indicator — BrokerTakeProfitMutation.cs
// Single-responsibility broker mutation module.

using System;
using cAlgo.API;

namespace cAlgo
{
    public partial class CFIPIndicator : Indicator
    {
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
                                MarkBrokerStateDirty();

                                TradeResult result =
                                    position.ModifyTakeProfitPrice(normalized);
                
                                if (!BrokerConfirmationPolicy.IsSuccessfulMutation(
                                        result != null,
                                        result != null &&
                                        result.IsSuccessful))
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

        private bool TryModifyTakeProfitLadder(
                            Position position,
                            RelativeTakeProfitProtections protections,
                            string context)
                        {
                            if (position == null ||
                                protections == null)
                                return false;

                            try
                            {
                                MarkBrokerStateDirty();

                                TradeResult result =
                                    position.ModifyTakeProfit(
                                        protections);

                                if (!BrokerConfirmationPolicy.IsSuccessfulMutation(
                                        result != null,
                                        result != null &&
                                        result.IsSuccessful))
                                {
                                    Print(
                                        "CFIP advanced TP ladder mutation rejected ({0}).",
                                        context);
                                    return false;
                                }

                                return true;
                            }
                            catch (Exception ex)
                            {
                                Print(
                                    "CFIP advanced TP ladder mutation failed ({0}): {1}",
                                    context,
                                    ex.Message);
                                return false;
                            }
                        }

        private bool TryModifyTakeProfitPips(
                            Position position,
                            double targetPips,
                            string context)
                        {
                            if (position == null ||
                                double.IsNaN(targetPips) ||
                                double.IsInfinity(targetPips) ||
                                targetPips <= 0)
                                return false;

                            try
                            {
                                MarkBrokerStateDirty();

                                TradeResult result =
                                    position.ModifyTakeProfitPips(
                                        targetPips);

                                if (!BrokerConfirmationPolicy.IsSuccessfulMutation(
                                        result != null,
                                        result != null &&
                                        result.IsSuccessful))
                                {
                                    Print(
                                        "CFIP single TP mutation rejected ({0}).",
                                        context);
                                    return false;
                                }

                                return true;
                            }
                            catch (Exception ex)
                            {
                                Print(
                                    "CFIP single TP mutation failed ({0}): {1}",
                                    context,
                                    ex.Message);
                                return false;
                            }
                        }
    }
}
