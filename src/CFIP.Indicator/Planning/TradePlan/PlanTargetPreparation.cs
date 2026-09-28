using System;
using System.Collections.Generic;
using System.Linq;
using cAlgo.API;

namespace cAlgo
{
    public partial class CFIPIndicator : Indicator
    {
        private bool TryBuildPlanTargets(
            List<Level> candidates,
            List<Level> selected,
            int closedM5,
            double entry,
            double risk,
            int direction,
            double atr,
            out double tp1,
            out double tp2,
            out double tp3,
            out double tp4)
        {
            tp1 = 0;
            tp2 = 0;
            tp3 = 0;
            tp4 = 0;

            if (candidates == null ||
                selected == null)
                return false;

            int filledTargetSlots =
                selected.Count(
                    x => x != null);

            if (filledTargetSlots <
                Math.Max(
                    1,
                    MinimumTargetsForPlan))
                return false;

            tp1 =
                SelectTarget(
                    selected,
                    0,
                    entry,
                    risk,
                    direction,
                    Math.Max(
                        FallbackTp1RR,
                        MinimumRequiredRR()));

            tp2 =
                SelectTarget(
                    selected,
                    1,
                    entry,
                    risk,
                    direction,
                    Math.Max(
                        FallbackTp2RR,
                        Tp2MinimumRR));

            tp3 =
                SelectTarget(
                    selected,
                    2,
                    entry,
                    risk,
                    direction,
                    Math.Max(
                        FallbackTp3RR,
                        Tp3MinimumRR));

            tp4 =
                SelectTarget(
                    selected,
                    3,
                    entry,
                    risk,
                    direction,
                    Math.Max(
                        FallbackTp4RR,
                        Tp4MinimumRR));

            if (!IsValidTarget(
                    direction,
                    entry,
                    tp1))
                return false;

            if (UseRRFilter &&
                Math.Abs(
                    tp1 -
                    entry) /
                Math.Max(
                    Symbol.PipSize,
                    risk) <
                Math.Max(
                    MinimumTradeRR,
                    MinimumRequiredRR()))
                return false;

            if (RequireHtfTargets &&
                !HasAnyHtfTargetLevel(
                    candidates))
                return false;

            if (RequireHtfRewardForTp2Plus)
            {
                if (tp2 > 0 &&
                    !IsHtfSourceForReward(
                        selected,
                        tp2))
                    return false;

                if (tp3 > 0 &&
                    !IsHtfSourceForReward(
                        selected,
                        tp3))
                    return false;

                if (tp4 > 0 &&
                    !IsHtfSourceForReward(
                        selected,
                        tp4))
                    return false;
            }

            if (RequireHtfRewardForTp1 &&
                !IsHtfSourceForReward(
                    selected,
                    tp1))
                return false;

            if (RejectTargetObstacle &&
                RequireObstacleFreeTp1 &&
                HasTargetObstacle(
                    _m5Bars,
                    closedM5,
                    direction,
                    entry,
                    tp1,
                    atr))
                return false;

            return true;
        }
    }
}
