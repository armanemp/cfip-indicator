using System;
using cAlgo.API;

namespace cAlgo
{
    public partial class CFIPIndicator : Indicator
    {
        private bool M1TriggerReady(
            Bars m1Bars,
            Bars m5Bars,
            int m1Index,
            int closedM5,
            DateTime reference,
            int direction)
        {
            if (m1Bars == null ||
                m5Bars == null ||
                m1Index < 20 ||
                m1Index >= m1Bars.Count - 1 ||
                closedM5 < 20 ||
                closedM5 >= m5Bars.Count - 1 ||
                _m1Frame == null ||
                _m1Frame.Index != m1Index)
                return false;

            DateTime m1Open =
                m1Bars.OpenTimes[m1Index];

            DateTime m1NextOpen =
                m1Bars.OpenTimes[m1Index + 1];

            DateTime m5Open =
                m5Bars.OpenTimes[closedM5];

            DateTime m5NextOpen =
                m5Bars.OpenTimes[closedM5 + 1];

            if (!M1TriggerRule.IsClosedInsideM5Window(
                m1Open,
                m1NextOpen,
                m5Open,
                m5NextOpen,
                reference))
                return false;

            double atr =
                Atr(
                    m1Bars,
                    m1Index);

            if (atr <= 0)
                return false;

            int trigger =
                direction == 1
                    ? BullTriggerScore(
                        m1Bars,
                        m1Index)
                    : direction == -1
                        ? BearTriggerScore(
                            m1Bars,
                            m1Index)
                        : 0;

            int requiredTrigger =
                TriggerThresholdRule.ResolveRequiredScore(
                    UsePrecisionExecutionModel,
                    LiveTriggerScore,
                    PrecisionTriggerScore);

            int microLookback =
                Math.Max(
                    3,
                    Math.Min(
                        8,
                        SwingStrength * 2));

            int microStart =
                Math.Max(
                    0,
                    m1Index - microLookback);

            double priorMicroHigh =
                Highest(
                    m1Bars,
                    microStart,
                    m1Index - 1);

            double priorMicroLow =
                Lowest(
                    m1Bars,
                    microStart,
                    m1Index - 1);

            return M1TriggerRule.IsReady(
                direction,
                _m1Frame.Direction,
                m1Bars.OpenPrices[m1Index],
                m1Bars.HighPrices[m1Index],
                m1Bars.LowPrices[m1Index],
                m1Bars.ClosePrices[m1Index],
                atr,
                MinimumTriggerBodyAtr,
                MinimumCloseLocation,
                MaximumTriggerRangeAtr,
                trigger,
                requiredTrigger,
                priorMicroHigh,
                priorMicroLow,
                StructureBreakAtr,
                UseDisplacement,
                DisplacementAtr);
        }
    }
}
