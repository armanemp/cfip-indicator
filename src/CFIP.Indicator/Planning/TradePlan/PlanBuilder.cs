using System;
using System.Collections.Generic;
using cAlgo.API;

namespace cAlgo
{
    public partial class CFIPIndicator : Indicator
    {
        private Plan BuildPlan(
            int closedM5,
            int direction)
        {
            if (!TryPreparePlanInputs(
                closedM5,
                direction,
                out double atr,
                out ExecutionModel execution,
                out double entry,
                out double stop,
                out string stopSource,
                out int stopQuality,
                out double risk))
                return null;

            List<Level> candidates =
                BuildTargetLevels(
                    closedM5,
                    direction,
                    entry,
                    atr);

            OpportunityLane lane =
                ResolvePlanTargetSelectionLane();

            List<Level> selected =
                SelectTargets(
                    candidates,
                    closedM5,
                    entry,
                    risk,
                    direction,
                    atr,
                    lane);

            if (!TryBuildPlanTargets(
                candidates,
                selected,
                closedM5,
                entry,
                risk,
                direction,
                atr,
                lane,
                out double tp1,
                out double tp2,
                out double tp3,
                out double tp4))
                return null;

            Plan p =
                CreatePlanFromInputs(
                    execution,
                    direction,
                    closedM5,
                    entry,
                    stop,
                    stopSource,
                    stopQuality,
                    tp1,
                    tp2,
                    tp3,
                    tp4);

            if (p == null)
                return null;

            p.Lane = lane;

            p.SignalBarOpenTimeUtcTicks =
                GetSignalBarOpenTimeUtcTicks(closedM5);
            p.SignalTraceId =
                BuildSignalTraceId(closedM5);

            BindPlanCalibrationContext(
                p,
                _decision,
                direction,
                lane);

            EnrichPlanTargetMetadata(
                p,
                selected);

            if (RequirePlanIntegrity &&
                !ValidatePlanIntegrity(
                    p,
                    direction,
                    entry,
                    atr,
                    true))
                return null;

            return p;
        }
    }
}
