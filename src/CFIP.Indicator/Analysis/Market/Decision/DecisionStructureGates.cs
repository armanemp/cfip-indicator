using System;
using cAlgo.API;

namespace cAlgo
{
    public partial class CFIPIndicator : Indicator
    {
        private DecisionFilterResult EvaluateDecisionStructureGates(
            DateTime reference,
            int closedM5,
            Decision decision)
        {
            if (UseStructuralSequenceGate &&
                StructuralSequence(
                    _m5Bars,
                    closedM5,
                    decision.Direction) <
                MinimumStructuralSequence)
                return new DecisionFilterResult(false, "STRUCTURAL SEQUENCE");

            if (UseZoneConfluence &&
                RequireEntryLocationConfluence &&
                EntryLocationQuality(
                    _m5Bars,
                    closedM5,
                    decision.Direction) <
                MinimumEntryLocationQuality)
                return new DecisionFilterResult(false, "ENTRY LOCATION");

            if (RequireStableM5Direction &&
                !StableDirection(
                    _m5Bars,
                    closedM5,
                    decision.Direction,
                    StableM5Bars))
                return new DecisionFilterResult(false, "M5 STABILITY");

            int m15Closed =
                ClosedIndex(
                    _m15Bars,
                    reference);

            if (RequireStableM15Direction &&
                m15Closed >= StableM15Bars + 5 &&
                !StableDirection(
                    _m15Bars,
                    m15Closed,
                    decision.Direction,
                    StableM15Bars))
                return new DecisionFilterResult(false, "M15 STABILITY");

            if (RequireRetestQuality &&
                decision.RetestQuality <
                MinimumRetestQuality)
            {
                bool retestOverride =
                    AllowStrongTriggerOverride &&
                    decision.Confidence >= 85 &&
                    decision.Edge >= 20 &&
                    decision.IndependentEvidence >=
                    MinimumIndependentEvidence + 1;

                if (!retestOverride)
                    return new DecisionFilterResult(false, "RETEST QUALITY");
            }
            return new DecisionFilterResult(true, string.Empty);
        }
    }
}