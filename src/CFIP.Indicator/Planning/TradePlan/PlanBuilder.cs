using System;
using cAlgo.API;

namespace cAlgo
{
    public partial class CFIPIndicator : Indicator
    {
        private Plan BuildPlan(
            int closedM5,
            int direction)
        {
            OpportunityLane lane =
                ResolvePlanTargetSelectionLane();

            if (!TryPreparePlanInputs(
                    closedM5,
                    direction,
                    lane,
                    out double atr,
                    out ExecutionModel execution,
                    out double entry,
                    out double stop,
                    out string stopSource,
                    out int stopQuality,
                    out double risk,
                    out CanonicalTradePathGeometry canonicalPath))
                return null;

            // The canonical path is the only source for executable Entry/SL/TP.
            // Target provenance is also carried forward from the exact selected
            // ladder so Plan metadata cannot drift from the geometry that was
            // actually evaluated.
            Plan p =
                CreatePlanFromInputs(
                    execution,
                    direction,
                    closedM5,
                    entry,
                    stop,
                    stopSource,
                    stopQuality,
                    canonicalPath.Tp1,
                    canonicalPath.Tp2,
                    canonicalPath.Tp3,
                    canonicalPath.Tp4);

            if (p == null)
                return null;

            p.Lane = lane;

            p.Tp1Source = canonicalPath.Tp1Source;
            p.Tp1Quality = canonicalPath.Tp1Quality;
            p.Tp2Source = canonicalPath.Tp2Source;
            p.Tp2Quality = canonicalPath.Tp2Quality;
            p.Tp3Source = canonicalPath.Tp3Source;
            p.Tp3Quality = canonicalPath.Tp3Quality;
            p.Tp4Source = canonicalPath.Tp4Source;
            p.Tp4Quality = canonicalPath.Tp4Quality;
            p.HtfTargetCount =
                canonicalPath.HtfTargetCount;

            p.SignalBarOpenTimeUtcTicks =
                GetSignalBarOpenTimeUtcTicks(closedM5);
            p.SignalTraceId =
                BuildSignalTraceId(closedM5);

            BindPlanCalibrationContext(
                p,
                _decision,
                direction,
                lane);

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
