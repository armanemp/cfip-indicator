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
        private bool ProcessActivePlanReactionExits(
                                    int closedM5,
                                    double market,
                                    double peakRR)
                                {
                                    if (CheckLiveReversalAgainstPlan(
                                            closedM5))
                                        return false;

                                    if (CheckProfitExhaustionExit(
                                            closedM5,
                                            market,
                                            peakRR))
                                        return false;

                                    if (CheckStructuralSetupInvalidation(
                                            closedM5,
                                            market))
                                        return false;

                                    return true;
                                }
    }
}
