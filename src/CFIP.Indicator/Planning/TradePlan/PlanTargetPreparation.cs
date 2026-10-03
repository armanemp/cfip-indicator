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
            OpportunityLane lane,
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
                selected == null ||
                !IsFinitePositive(entry) ||
                !IsFinitePositive(risk) ||
                !IsFinitePositive(atr) ||
                (direction != 1 &&
                 direction != -1))
                return false;

            int filledTargetSlots =
                selected.Count(
                    x => x != null);

            if (filledTargetSlots <
                Math.Max(
                    1,
                    MinimumTargetsForPlan))
                return false;

            double[] requiredRR =
                BuildTargetSelectionRequiredRR(
                    risk,
                    atr,
                    lane);

            tp1 =
                SelectTarget(
                    selected,
                    0,
                    entry,
                    risk,
                    direction,
                    requiredRR[0],
                    lane);

            tp2 =
                SelectTarget(
                    selected,
                    1,
                    entry,
                    risk,
                    direction,
                    requiredRR[1],
                    lane);

            tp3 =
                SelectTarget(
                    selected,
                    2,
                    entry,
                    risk,
                    direction,
                    requiredRR[2],
                    lane);

            tp4 =
                SelectTarget(
                    selected,
                    3,
                    entry,
                    risk,
                    direction,
                    requiredRR[3],
                    lane);

            if (!IsValidTarget(
                    direction,
                    entry,
                    tp1))
                return false;

            if (UseRRFilter)
            {
                RiskRewardMathResult tp1Geometry =
                    RiskRewardMathRule.EvaluateFromRisk(
                        direction,
                        entry,
                        risk,
                        tp1,
                        0,
                        requiredRR[0],
                        MaximumRewardRR,
                        Symbol.PipSize);

                if (!tp1Geometry.Valid)
                    return false;
            }

            if (RequireHtfTargets &&
                !HasAnyHtfTargetLevel(
                    candidates))
                return false;

            bool requireHtfTp2Plus =
                RequiresHtfRewardForTargetStage(
                    1,
                    lane);

            bool requireHtfTp1 =
                RequiresHtfRewardForTargetStage(
                    0,
                    lane);

            if (requireHtfTp2Plus)
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

            if (requireHtfTp1 &&
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
