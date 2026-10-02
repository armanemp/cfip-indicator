using System;
using cAlgo.API;

namespace cAlgo
{
    public partial class CFIPIndicator : Indicator
    {
        private bool TryPreparePlanInputs(
            int closedM5,
            int direction,
            OpportunityLane lane,
            out double atr,
            out ExecutionModel execution,
            out double entry,
            out double stop,
            out string stopSource,
            out int stopQuality,
            out double risk,
            out CanonicalTradePathGeometry canonicalPath)
        {
            atr = 0;
            execution = null;
            entry = 0;
            stop = 0;
            stopSource = "";
            stopQuality = 0;
            risk = 0;
            canonicalPath = null;

            if (_m5Bars == null ||
                closedM5 < 30 ||
                (direction != 1 && direction != -1))
                return false;

            atr =
                Atr(
                    _m5Bars,
                    closedM5);

            if (!IsFinitePositive(atr))
                return false;

            execution =
                BuildExecutionModel(
                    closedM5,
                    direction);

            if (execution == null ||
                !execution.Ready ||
                !IsFinitePositive(
                    execution.ActualEntry))
                return false;

            if (execution.Mode !=
                    ExecutionMode.BreakoutMarket &&
                execution.Mode !=
                    ExecutionMode.RetestMarket)
                return false;

            entry =
                NormalizePrice(
                    execution.ActualEntry);

            if (!IsFinitePositive(entry))
                return false;

            if (RequirePrecisionEntry &&
                execution.Quality <
                Math.Max(
                    40,
                    MinimumEntryQuality))
                return false;

            // Keep the existing execution-window contract in the same
            // preparation boundary before the canonical reward path is built.
            Plan executionProbe =
                new Plan
                {
                    Direction = direction,
                    EntryMode = execution.Mode,
                    EntryTrigger = execution.Trigger,
                    EntryZoneLow = execution.ZoneLow,
                    EntryZoneHigh = execution.ZoneHigh,
                    EntryZoneTolerance = execution.ZoneTolerance
                };

            if (!IsExecutableMarketEntry(
                    executionProbe,
                    entry,
                    out _))
                return false;

            if (!TryBuildCanonicalTradePathGeometry(
                    closedM5,
                    direction,
                    execution,
                    lane,
                    out canonicalPath,
                    out _))
                return false;

            if (canonicalPath == null ||
                !canonicalPath.IsValid ||
                Math.Abs(
                    canonicalPath.Entry -
                    entry) >
                Math.Max(
                    Symbol.TickSize * 2,
                    Symbol.PipSize * 0.10))
                return false;

            // From this point onward every executable plan value is taken from
            // the exact same canonical actual-entry geometry used by live
            // actionability. PlanBuilder must not rebuild SL/TP independently.
            entry =
                canonicalPath.Entry;
            stop =
                canonicalPath.Stop;
            stopSource =
                canonicalPath.StopSource;
            stopQuality =
                canonicalPath.StopQuality;
            risk =
                canonicalPath.Risk;

            return
                IsFinitePositive(entry) &&
                IsFinitePositive(stop) &&
                IsFinitePositive(risk);
        }
    }
}
