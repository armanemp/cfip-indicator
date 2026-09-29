// CFIP Indicator — LiveFillReconciliation.cs
// Single-responsibility lifecycle module.

using System;
using System.Collections.Generic;
using System.Linq;
using cAlgo.API;
using cAlgo.API.Indicators;
using cAlgo.API.Internals;

namespace cAlgo
{
    public partial class CFIPIndicator : Indicator
    {
        private void ReconcileLivePlanToActualFill(
                                    Position position,
                                    int closedM5)
                                {
                                    if (_plan == null ||
                                        position == null ||
                                        _m5Bars == null ||
                                        closedM5 < 20)
                                        return;
                        
                                    int direction =
                                        position.TradeType == TradeType.Buy
                                            ? 1
                                            : -1;
                        
                                    double actualEntry =
                                        NormalizePrice(
                                            position.EntryPrice);
                        
                                    if (!IsFinitePositive(actualEntry))
                                        return;
                        
                                    double atr =
                                        Atr(
                                            _m5Bars,
                                            closedM5);
                        
                                    if (atr <= 0)
                                        return;
                        
                                    _plan.Entry =
                                        actualEntry;
                        
                                    // The broker may fill at a slightly different price than the
                                    // executable quote. Reconcile the live exit geometry from the actual
                                    // fill without allowing an old/passed TP or a less-protective SL to
                                    // overwrite broker-safe state.
                                    if (ReconcileLiveFillExitGeometry(
                                            closedM5,
                                            direction,
                                            actualEntry,
                                            atr,
                                            position))
                                        return;

                                    _brokerProtectionRecoveryRequired = true;
                                    SetLifecycleState(
                                        LifecycleState.RecoveryRequired,
                                        "LIVE FILL • EXIT GEOMETRY RECONCILIATION FAILED");

                                    _autoExecutionBlockReason =
                                        "LIVE FILL • EXIT GEOMETRY RECONCILIATION FAILED";

                                    return;

                                }
    }
}
