using System;
using cAlgo.API;

namespace cAlgo
{
    public partial class CFIPIndicator : Indicator
    {
        private bool TryPreparePlanInputs(
            int closedM5,
            int direction,
            out double atr,
            out ExecutionModel execution,
            out double entry,
            out double stop,
            out string stopSource,
            out int stopQuality,
            out double risk)
        {
            atr = 0;
            execution = null;
            entry = 0;
            stop = 0;
            stopSource = "";
            stopQuality = 0;
            risk = 0;

            if (_m5Bars == null ||
                closedM5 < 30 ||
                (direction != 1 && direction != -1))
                return false;

            atr =
                Atr(
                    _m5Bars,
                    closedM5);

            if (atr <= 0)
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

            Plan executionProbe =
                new Plan
                {
                    Direction = direction,
                    EntryMode = execution.Mode,
                    EntryTrigger = execution.Trigger,
                    EntryZoneLow = execution.ZoneLow,
                    EntryZoneHigh = execution.ZoneHigh
                };

            string executionReason;

            if (!IsExecutableMarketEntry(
                    executionProbe,
                    entry,
                    out executionReason))
                return false;

            stop =
                BuildStructuralStop(
                    closedM5,
                    direction,
                    entry,
                    atr,
                    out stopSource,
                    out stopQuality);

            if (!IsFinitePositive(stop))
            {
                if (RequireStructuralStop)
                    return false;

                stop =
                    direction == 1
                        ? entry -
                          atr *
                          FallbackSlAtr
                        : entry +
                          atr *
                          FallbackSlAtr;

                stopSource =
                    "ATR FALLBACK";
                stopQuality = 50;
            }

            if (AvoidLateEntry &&
                Math.Abs(
                    entry -
                    _m5Bars.ClosePrices[
                        closedM5]) >
                atr *
                MaximumEntryExtensionAtr)
                return false;

            risk =
                Math.Abs(
                    entry -
                    stop);

            double spread =
                Math.Max(
                    0,
                    Symbol.Ask -
                    Symbol.Bid);

            double minimumRisk =
                Math.Max(
                    Math.Max(
                        0.05,
                        MinimumSlAtr) *
                    atr,
                    spread *
                    Math.Max(
                        1.0,
                        MaximumSpreadToStopRiskRatio));

            double maximumRisk =
                Math.Min(
                    Math.Max(
                        MinimumSlAtr,
                        MaximumSlAtr),
                    Math.Max(
                        MinimumSlAtr,
                        MaximumStructuralStopAtr)) *
                atr;

            return
                risk >= minimumRisk &&
                risk <= maximumRisk;
        }
    }
}
