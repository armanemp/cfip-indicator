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
            double risk,
            double atr,
            OpportunityLane lane)
        {
            double riskAtr =
                risk > 0 &&
                atr > 0
                    ? risk / Math.Max(Symbol.PipSize, atr)
                    : 0;

            int confidence =
                _decision == null
                    ? 0
                    : _decision.Confidence;

            int smartQuality =
                _decision == null
                    ? 0
                    : _decision.SmartQuality;

            int targetQuality =
                Math.Max(
                    0,
                    SmartTargetQuality);

            int structuralQuality =
                _plan != null
                    ? Math.Max(0, _plan.StopQuality)
                    : 60;

            string regime =
                _decision == null
                    ? "UNKNOWN"
                    : _decision.Regime;

            return TargetSelectionRequiredRrRule.BuildRequiredRrLadder(
                regime,
                lane,
                riskAtr,
                confidence,
                smartQuality,
                targetQuality,
                structuralQuality,
                Math.Max(1.0, MaximumTargetExtensionAtr));
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
