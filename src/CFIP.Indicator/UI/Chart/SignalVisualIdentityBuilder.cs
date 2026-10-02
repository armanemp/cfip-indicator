using System;

namespace cAlgo
{
    public partial class CFIPIndicator
    {
        private void PopulateCanonicalVisualIdentity(
                            SignalVisualSnapshot snapshot,
                            int closedM5,
                            int direction)
                        {
                            if (snapshot == null)
                                return;

                            string signalId =
                                ResolveProviderSignalId(
                                    closedM5);

                            OpportunityLane lane =
                                ResolveProviderLane();

                            string scenarioId =
                                ResolveProviderScenarioId(
                                    signalId,
                                    lane,
                                    direction);

                            string planId =
                                ResolveProviderPlanId(
                                    signalId);

                            TradeOpportunityCandidate scenario = null;
                            _tradePlanRegistry.TryGetCandidate(
                                scenarioId,
                                out scenario);

                            snapshot.SignalId =
                                signalId ?? "";

                            snapshot.ScenarioId =
                                scenarioId ?? "";

                            snapshot.PlanId =
                                planId ?? "";

                            snapshot.SourceTimeframe =
                                ProviderScenarioIdentityRule.ResolveSourceTimeframe(
                                    scenario,
                                    ProviderScenarioIdentityRule.CanonicalM5);

                            snapshot.Revision =
                                Math.Max(
                                    1,
                                    _cfipProviderRevision);
                        }

    }
}
