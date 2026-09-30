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

                                    if (!LiveInvalidationRule.ShouldEvaluateClosedBar(
                                            closedM5,
                                            _lastInvalidationEvaluationM5))
                                        return;

                                    double invalidationMarket;
                                    if (!TryGetClosedM5InvalidationMarket(
                                            closedM5,
                                            out invalidationMarket))
                                        return;

                                    double invalidationMove;
                                    if (!LiveInvalidationRule.TryCalculateDirectionalMove(
                                            _plan.Direction,
                                            _plan.Entry,
                                            invalidationMarket,
                                            out invalidationMove))
                                        return;

                                    bool structuralExitAccepted =
                                        CheckStructuralSetupInvalidation(
                                            closedM5,
                                            invalidationMarket);

                                    _lastInvalidationEvaluationM5 =
                                        closedM5;

                                    if (structuralExitAccepted ||
                                        _plan == null ||
                                        !_plan.IsLivePosition)
                                        return;

                                    ProcessActivePlanFalseSignalRisk(
                                        closedM5,
                                        invalidationMove);
                                }
    }
}
