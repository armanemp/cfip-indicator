// ============================================================================
// CFIP Indicator — ActivePlanEvaluation.cs
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
        private void EvaluateActivePlan(
                                    int closedM5)
                                {
                                    double market;
                                    double currentMove;
                                    double peakRR;

                                    if (!TryPrepareActivePlanEvaluation(
                                            closedM5,
                                            out market,
                                            out currentMove,
                                            out peakRR))
                                        return;

                                    UpdateActivePlanLiveManagement(
                                        closedM5,
                                        market);

                                    if (!ValidateActivePlanRuntimeIntegrity(
                                            closedM5))
                                        return;

                                    if (!ProcessActivePlanLevelHits(
                                            closedM5,
                                            market))
                                        return;

                                    if (!ProcessActivePlanReactionExits(
                                            closedM5,
                                            market,
                                            peakRR))
                                        return;

                                    ProcessActivePlanFalseSignalRisk(
                                        closedM5,
                                        currentMove);
                                }
    }
}
