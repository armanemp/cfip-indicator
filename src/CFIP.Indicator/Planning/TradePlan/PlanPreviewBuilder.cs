using System;
using System.Collections.Generic;
using cAlgo.API;

namespace cAlgo
{
    public partial class CFIPIndicator : Indicator
    {
        private bool TryGetParallelScenarioPreview(
            int closedM5,
            ExecutionModel execution,
            OpportunityLane lane,
            ParallelScenarioGeometry geometry,
            out TradeSetupPreview preview)
        {
            preview = null;

            if (execution == null ||
                geometry == null)
                return false;

            int cacheKey =
                ((int)lane * 2) +
                (execution.Direction == 1 ? 1 : 0);

            if (_parallelPreviewCache.TryGetValue(
                    cacheKey,
                    out preview))
                return preview != null;

            preview =
                BuildTradeSetupPreviewFromGeometry(
                    closedM5,
                    execution,
                    lane,
                    geometry);

            if (preview == null)
                return false;

            _parallelPreviewCache[cacheKey] = preview;
            return true;
        }

        private TradeSetupPreview BuildTradeSetupPreview(
            int closedM5,
            ExecutionModel execution,
            OpportunityLane lane = OpportunityLane.Strategic)
        {
            if (_m5Bars == null ||
                execution == null ||
                (execution.Direction != 1 &&
                 execution.Direction != -1) ||
                closedM5 < 30)
                return null;

            ParallelScenarioGeometry geometry;

            if (!TryBuildParallelScenarioGeometry(
                    closedM5,
                    execution.Direction,
                    out geometry))
                return null;

            return BuildTradeSetupPreviewFromGeometry(
                closedM5,
                execution,
                lane,
                geometry);
        }

        private TradeSetupPreview BuildTradeSetupPreviewFromGeometry(
            int closedM5,
            ExecutionModel execution,
            OpportunityLane lane,
            ParallelScenarioGeometry geometry)
        {
            if (geometry == null ||
                execution == null)
                return null;

            double atr =
                geometry.Atr;
            double entry =
                geometry.Entry;
            double stop =
                geometry.Stop;
            double risk =
                geometry.Risk;

            List<Level> candidates =
                BuildTargetLevels(
                    closedM5,
                    execution.Direction,
                    entry,
                    atr);
            List<Level> selected =
                SelectTargets(
                    candidates,
                    closedM5,
                    entry,
                    risk,
                    execution.Direction,
                    atr,
                    lane);

            TradeSetupPreview preview =
                new TradeSetupPreview
                {
                    Direction = execution.Direction,
                    EntryMode = execution.Mode,
                    CreatedM5 = closedM5,
                    Entry = entry,
                    IdealEntry = NormalizePrice(execution.IdealEntry),
                    ZoneLow = NormalizePrice(execution.ZoneLow),
                    ZoneHigh = NormalizePrice(execution.ZoneHigh),
                    ZoneTolerance = Math.Max(0, execution.ZoneTolerance),
                    Trigger = NormalizePrice(execution.Trigger),
                    Invalidation = NormalizePrice(execution.Invalidation),
                    Stop = stop,
                    Risk = risk
                };

            double rrStep =
                Math.Max(
                    0.10,
                    StructuralTpRrStep);

            double[] requiredRR =
                BuildTargetSelectionRequiredRR(
                    rrStep,
                    lane);

            preview.Tp1 =
                SelectTarget(
                    selected,
                    0,
                    entry,
                    risk,
                    execution.Direction,
                    requiredRR[0],
                    lane);

            preview.Tp2 =
                SelectTarget(
                    selected,
                    1,
                    entry,
                    risk,
                    execution.Direction,
                    requiredRR[1],
                    lane);

            preview.Tp3 =
                SelectTarget(
                    selected,
                    2,
                    entry,
                    risk,
                    execution.Direction,
                    requiredRR[2],
                    lane);

            preview.Tp4 =
                SelectTarget(
                    selected,
                    3,
                    entry,
                    risk,
                    execution.Direction,
                    requiredRR[3],
                    lane);

            return preview;
        }
    }
}