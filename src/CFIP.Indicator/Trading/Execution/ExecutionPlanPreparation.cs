// CFIP Indicator — ExecutionPlanPreparation.cs
// Executable plan preparation; no broker mutation is allowed here.

using System;
using System.Collections.Generic;
using System.Linq;
using cAlgo.API;

namespace cAlgo
{
    public partial class CFIPIndicator : Indicator
    {
        private double SelectStructuralAutoTarget(
                            int closedM5,
                            int direction,
                            double entry,
                            double stop,
                            double atr,
                            TargetStage stage)
                        {
                            if (atr <= 0 ||
                                !IsFinitePositive(entry) ||
                                !IsFinitePositive(stop))
                                return 0;
                
                            double risk =
                                RiskRewardMathRule.RiskFromLevels(
                                    entry,
                                    stop,
                                    Symbol.PipSize);
                
                            if (risk <= 0)
                                return 0;
                
                            List<Level> levels =
                                BuildTargetLevels(
                                    closedM5,
                                    direction,
                                    entry,
                                    atr);
                
                            OpportunityLane lane =
                                ResolvePlanTargetSelectionLane();

                            List<Level> selected =
                                SelectTargets(
                                    levels,
                                    closedM5,
                                    entry,
                                    risk,
                                    direction,
                                    atr,
                                    lane);
                
                            int stageIndex =
                                ClampInt(
                                    (int)stage,
                                    0,
                                    3);
                
                            // All execution paths use the same progressive fallback policy:
                            // requested stage -> nearest available earlier stage -> synthetic
                            // RR target (when explicitly permitted). This keeps pending,
                            // aggressive and market execution behavior identical.
                            for (int i = stageIndex;
                                 i >= 0;
                                 i--)
                            {
                                if (i < selected.Count &&
                                    selected[i] != null &&
                                    IsFinitePositive(
                                        selected[i].Price))
                                    return NormalizePrice(
                                        selected[i].Price);
                            }
                
                            double rrStep =
                                Math.Max(
                                    0.10,
                                    StructuralTpRrStep);

                            double[] requiredRR =
                                BuildTargetSelectionRequiredRR(
                                    rrStep,
                                    lane);

                            double fallbackRR =
                                stageIndex == 0
                                    ? Math.Max(
                                        FallbackTp1RR,
                                        requiredRR[0])
                                    : stageIndex == 1
                                        ? Math.Max(
                                            FallbackTp2RR,
                                            requiredRR[1])
                                        : stageIndex == 2
                                            ? Math.Max(
                                                FallbackTp3RR,
                                                requiredRR[2])
                                            : Math.Max(
                                                FallbackTp4RR,
                                                requiredRR[3]);
                
                            bool requiresHtf =
                                RequiresHtfRewardForTargetStage(
                                    stageIndex,
                                    lane);
                
                            if (AllowSyntheticTargetFallback &&
                                !requiresHtf &&
                                fallbackRR > 0)
                            {
                                double synthetic =
                                    RiskRewardMathRule.TargetFromRR(
                                        direction,
                                        entry,
                                        risk,
                                        fallbackRR);

                                return IsFinitePositive(synthetic)
                                    ? NormalizePrice(synthetic)
                                    : 0;
                            }
                
                            return 0;
                        }

        private bool IsExecutionPlanConsistent(
                            int direction,
                            double entry,
                            double stop,
                            double target)
                        {
                            return direction != 0 &&
                                   IsFinitePositive(entry) &&
                                   IsFinitePositive(stop) &&
                                   IsFinitePositive(target) &&
                                   IsValidStop(direction, entry, stop) &&
                                   IsValidTarget(direction, entry, target);
                        }

                        private bool RebuildSmartExecutionLevels(
                            int closedM5,
                            int direction,
                            double executionEntry,
                            double atr)
                        {
                            if (_plan == null ||
                                _m5Bars == null ||
                                direction == 0 ||
                                !IsFinitePositive(executionEntry) ||
                                atr <= 0)
                                return false;
                
                            executionEntry = NormalizePrice(executionEntry);
                
                            string stopSource;
                            int stopQuality;
                
                            double stop = BuildStructuralStop(
                                closedM5,
                                direction,
                                executionEntry,
                                atr,
                                out stopSource,
                                out stopQuality);
                
                            if (!IsFinitePositive(stop) ||
                                !IsValidStop(direction, executionEntry, stop))
                                return false;
                
                            double risk =
                                RiskRewardMathRule.RiskFromLevels(
                                    executionEntry,
                                    stop,
                                    Symbol.PipSize);
                
                            List<Level> levels = BuildTargetLevels(
                                closedM5,
                                direction,
                                executionEntry,
                                atr);                
                            OpportunityLane lane =
                                ResolvePlanTargetSelectionLane();

                            List<Level> selected =
                                SelectTargets(
                                    levels,
                                    closedM5,
                                    executionEntry,
                                    risk,
                                    direction,
                                    atr,
                                    lane);

                            double[] requiredRR =
                                BuildTargetSelectionRequiredRR(
                                    Math.Max(
                                        0.10,
                                        StructuralTpRrStep),
                                    lane);

                            double tp1 =
                                SelectTarget(
                                    selected,
                                    0,
                                    executionEntry,
                                    risk,
                                    direction,
                                    Math.Max(
                                        FallbackTp1RR,
                                        requiredRR[0]),
                                    lane);

                            double tp2 =
                                SelectTarget(
                                    selected,
                                    1,
                                    executionEntry,
                                    risk,
                                    direction,
                                    Math.Max(
                                        FallbackTp2RR,
                                        requiredRR[1]),
                                    lane);

                            double tp3 =
                                SelectTarget(
                                    selected,
                                    2,
                                    executionEntry,
                                    risk,
                                    direction,
                                    Math.Max(
                                        FallbackTp3RR,
                                        requiredRR[2]),
                                    lane);

                            double tp4 =
                                SelectTarget(
                                    selected,
                                    3,
                                    executionEntry,
                                    risk,
                                    direction,
                                    Math.Max(
                                        FallbackTp4RR,
                                        requiredRR[3]),
                                    lane);
                
                            if (!IsValidTarget(direction, executionEntry, tp1))
                                return false;
                
                            _plan.Entry = executionEntry;
                            _plan.Stop = NormalizePrice(stop);
                            _plan.Risk = risk;
                            _plan.StopSource = stopSource;
                            _plan.StopQuality = stopQuality;
                
                            _plan.Tp1 = IsValidTarget(direction, executionEntry, tp1)
                                ? NormalizePrice(tp1) : 0;
                            _plan.Tp2 = IsValidTarget(direction, executionEntry, tp2)
                                ? NormalizePrice(tp2) : 0;
                            _plan.Tp3 = IsValidTarget(direction, executionEntry, tp3)
                                ? NormalizePrice(tp3) : 0;
                            _plan.Tp4 = IsValidTarget(direction, executionEntry, tp4)
                                ? NormalizePrice(tp4) : 0;
                
                            ApplySelectedTargetMeta(
                                selected,
                                0,
                                _plan.Tp1,
                                out _plan.Tp1Source,
                                out _plan.Tp1Quality);

                            ApplySelectedTargetMeta(
                                selected,
                                1,
                                _plan.Tp2,
                                out _plan.Tp2Source,
                                out _plan.Tp2Quality);

                            ApplySelectedTargetMeta(
                                selected,
                                2,
                                _plan.Tp3,
                                out _plan.Tp3Source,
                                out _plan.Tp3Quality);

                            ApplySelectedTargetMeta(
                                selected,
                                3,
                                _plan.Tp4,
                                out _plan.Tp4Source,
                                out _plan.Tp4Quality);

                            _plan.HtfTargetCount = CountHtfTargetsInPlan(_plan);
                            _runtimeTpStageIndex = -1;
                            _runtimeTpStagePlanCreatedM5 = -1;
                            RecalculatePlanRR();
                
                            double effectiveTarget =
                                AutoTarget(_plan, EffectiveAutoTpStage());
                
                            return IsExecutionPlanConsistent(
                                direction, executionEntry, _plan.Stop, effectiveTarget);
                        }

        private bool TryPrepareExecutablePlan(
                            int closedM5,
                            double executionEntry,
                            out double stopPips,
                            out double targetPips,
                            out double target)
                        {
                            stopPips =
                                0;
                
                            targetPips =
                                0;
                
                            target =
                                0;
                
                            if (_plan == null || _m5Bars == null || closedM5 < 20)
                                return false;
                
                            double atr = Atr(_m5Bars, closedM5);
                            if (atr <= 0)
                                return false;
                
                            if (!RebuildSmartExecutionLevels(
                                    closedM5, _plan.Direction, executionEntry, atr))
                                return false;
                
                            double stop =
                                _plan.Stop;
                
                            target =
                                AutoTarget(
                                    _plan,
                                    EffectiveAutoTpStage());
                
                            if (!IsExecutionPlanConsistent(
                                    _plan.Direction,
                                    executionEntry,
                                    stop,
                                    target))
                                return false;
                
                            target =
                                NormalizePrice(
                                    target);
                
                            stopPips =
                                Math.Abs(
                                    executionEntry -
                                    stop) /
                                Symbol.PipSize;
                
                            targetPips =
                                Math.Abs(
                                    target -
                                    executionEntry) /
                                Symbol.PipSize;
                
                            return stopPips > 0 &&
                                   targetPips > 0 &&
                                   IsExecutionPlanConsistent(
                                       _plan.Direction,
                                       executionEntry,
                                       stop,
                                       target);
                        }
    }
}
