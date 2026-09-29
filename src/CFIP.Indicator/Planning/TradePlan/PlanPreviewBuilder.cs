using System;
using System.Collections.Generic;
using cAlgo.API;

namespace cAlgo
{
    public partial class CFIPIndicator : Indicator
    {
        private TradeSetupPreview BuildTradeSetupPreview(
            int closedM5,
            ExecutionModel execution)
        {
            if (_m5Bars == null ||
                execution == null ||
                (execution.Direction != 1 &&
                 execution.Direction != -1) ||
                closedM5 < 30)
                return null;

            double atr = Atr(_m5Bars, closedM5);
            if (!IsFinitePositive(atr))
                return null;

            double entry =
                IsFinitePositive(execution.ActualEntry)
                    ? execution.ActualEntry
                    : execution.IdealEntry;
            if (!IsFinitePositive(entry))
                return null;

            entry = NormalizePrice(entry);

            double stop =
                BuildStructuralStop(
                    closedM5,
                    execution.Direction,
                    entry,
                    atr,
                    out _,
                    out _);

            if (!IsValidStop(execution.Direction, entry, stop))
            {
                stop =
                    IsValidStop(
                        execution.Direction,
                        entry,
                        execution.Invalidation)
                        ? execution.Invalidation
                        : execution.Direction == 1
                            ? entry - atr * FallbackSlAtr
                            : entry + atr * FallbackSlAtr;
                stop = NormalizePrice(stop);
            }

            if (!IsValidStop(execution.Direction, entry, stop))
                return null;

            double risk = Math.Abs(entry - stop);
            if (!IsFinitePositive(risk))
                return null;

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
                    atr);

            TradeSetupPreview preview =
                new TradeSetupPreview
                {
                    Direction = execution.Direction,
                    EntryMode = execution.Mode,
                    CreatedM5 = closedM5,
                    Entry = entry,
                    IdealEntry = NormalizePrice(execution.IdealEntry),
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
                BuildTargetSelectionRequiredRR(rrStep);

            preview.Tp1 =
                SelectTarget(
                    selected,
                    0,
                    entry,
                    risk,
                    execution.Direction,
                    requiredRR[0]);

            preview.Tp2 =
                SelectTarget(
                    selected,
                    1,
                    entry,
                    risk,
                    execution.Direction,
                    requiredRR[1]);

            preview.Tp3 =
                SelectTarget(
                    selected,
                    2,
                    entry,
                    risk,
                    execution.Direction,
                    requiredRR[2]);

            preview.Tp4 =
                SelectTarget(
                    selected,
                    3,
                    entry,
                    risk,
                    execution.Direction,
                    requiredRR[3]);

            return preview;
        }
    }
}