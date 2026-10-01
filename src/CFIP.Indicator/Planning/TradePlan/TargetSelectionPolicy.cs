using System;
using System.Collections.Generic;
using cAlgo.API;

namespace cAlgo
{
    public partial class CFIPIndicator : Indicator
    {
        private OpportunityLane ResolvePlanTargetSelectionLane()
        {
            if (_plan != null)
                return _plan.Lane;

            bool topDownCalibrated =
                _decision != null &&
                _decision.TopDownEligible &&
                string.Equals(
                    _decision.TopDownStage,
                    "ENTRY CALIBRATED",
                    StringComparison.OrdinalIgnoreCase);

            return topDownCalibrated
                ? OpportunityLane.Strategic
                : OpportunityLane.Tactical;
        }

        private double[] BuildTargetSelectionRequiredRR(
            double rrStep,
            OpportunityLane lane)
        {
            return TargetSelectionRequiredRrRule.BuildRequiredRrLadder(
                rrStep,
                lane,
                Tp1MinimumRR,
                Tp2MinimumRR,
                Tp3MinimumRR,
                Tp4MinimumRR,
                MinimumRequiredRR(),
                MinimumTradeRR,
                TacticalOpportunityMinimumRR);
        }

        private bool RequiresHtfRewardForTargetStage(
            int stage,
            OpportunityLane lane)
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
