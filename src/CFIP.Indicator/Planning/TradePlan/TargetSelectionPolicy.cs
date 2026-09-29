using System;
using System.Collections.Generic;
using cAlgo.API;

namespace cAlgo
{
    public partial class CFIPIndicator : Indicator
    {
        private double[] BuildTargetSelectionRequiredRR(
            double rrStep)
        {
            double adaptiveTp1RR =
                Math.Max(
                    Tp1MinimumRR,
                    MinimumRequiredRR());

            return new[]
            {
                adaptiveTp1RR,
                Math.Max(
                    Tp2MinimumRR,
                    adaptiveTp1RR + rrStep),
                Math.Max(
                    Tp3MinimumRR,
                    Tp2MinimumRR + rrStep),
                Math.Max(
                    Tp4MinimumRR,
                    Tp3MinimumRR + rrStep)
            };
        }

        private bool RequiresHtfRewardForTargetStage(
            int stage,
            OpportunityLane lane = OpportunityLane.Strategic)
        {
            bool topDownCalibrated =
                _decision != null &&
                _decision.TopDownEligible &&
                string.Equals(
                    _decision.TopDownStage,
                    "ENTRY CALIBRATED",
                    StringComparison.OrdinalIgnoreCase);

            if (lane == OpportunityLane.Tactical ||
                lane == OpportunityLane.CounterHtfTactical ||
                lane == OpportunityLane.MicroReaction)
            {
                return stage == 0
                    ? RequireHtfRewardForTp1
                    : RequireHtfRewardForTp2Plus;
            }

            return stage == 0
                ? RequireHtfRewardForTp1 ||
                  topDownCalibrated
                : RequireHtfRewardForTp2Plus;
        }

        private double FindPreviousSelectedTargetPrice(
            List<Level> selected,
            int stage,
            double entry)
        {
            double previous = entry;

            for (int p = stage - 1;
                 p >= 0;
                 p--)
            {
                if (selected[p] != null)
                {
                    previous =
                        selected[p].Price;
                    break;
                }
            }

            return previous;
        }
    }
}
