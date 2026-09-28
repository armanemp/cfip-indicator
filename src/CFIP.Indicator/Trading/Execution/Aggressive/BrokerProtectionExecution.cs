// ============================================================================
// CFIP Indicator — BrokerProtectionExecution.cs
// ============================================================================

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using cAlgo.API;
using cAlgo.API.Indicators;
using cAlgo.API.Internals;

namespace cAlgo
{
    public partial class CFIPIndicator : Indicator
    {
        private void ProtectBrokerPositions(
                                    int closedM5)
                                {
                                    if (!AutoBrokerProtection &&
                                        !AutoProtectBrokerPositions)
                                        return;

                                    if (_plan == null ||
                                        !_plan.IsLivePosition ||
                                        _lifecycleState ==
                                            LifecycleState.ExitRequested)
                                        return;

                                    if ((TimeInUtc -
                                         _lastBrokerModifyUtc).TotalMilliseconds <
                                        Math.Max(
                                            100,
                                            BrokerModifyCooldownMs))
                                        return;

                                    Position planPosition = null;

                                    if (_plan.PositionId > 0)
                                    {
                                        foreach (Position position in Positions)
                                        {
                                            if (position != null &&
                                                position.SymbolName == SymbolName &&
                                                position.Id == _plan.PositionId &&
                                                IsManagedPosition(position))
                                            {
                                                planPosition = position;
                                                break;
                                            }
                                        }
                                    }

                                    // The active plan may manage only its bound broker position.
                                    // Never apply its stop/target to another position just because
                                    // that position happens to share the managed label.
                                    if (planPosition != null)
                                    {
                                        ProtectBoundPlanPosition(
                                            planPosition);

                                        _lastBrokerModifyUtc =
                                            TimeInUtc;
                                        return;
                                    }

                                    if (!AutoProtectBrokerPositions)
                                        return;

                                    string label =
                                        string.IsNullOrWhiteSpace(
                                            ManagedPositionLabel)
                                            ? NormalizeLabel()
                                            : ManagedPositionLabel.Trim();

                                    foreach (Position position in Positions)
                                    {
                                        if (position == null ||
                                            position.SymbolName != SymbolName ||
                                            position.Label != label)
                                            continue;

                                        if (!ProtectOrphanManagedPosition(
                                                position,
                                                closedM5))
                                            return;

                                        _lastBrokerModifyUtc =
                                            TimeInUtc;
                                    }
                                }
                                }
    }
}
