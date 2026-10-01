using System;

namespace cAlgo
{
    internal static class M1TriggerRule
    {
        internal static bool IsClosedInsideM5Window(
            DateTime m1Open,
            DateTime m1NextOpen,
            DateTime m5Open,
            DateTime m5NextOpen,
            DateTime reference)
        {
            return
                m1Open < m1NextOpen &&
                m5Open < m5NextOpen &&
                m1Open >= m5Open &&
                m1Open < m5NextOpen &&
                m1NextOpen <= reference &&
                m5NextOpen <= reference;
        }

        internal static bool IsClosedM1InsideM5Window(
            DateTime m1Open,
            DateTime m1NextOpen,
            DateTime m5Open,
            DateTime m5NextOpen,
            DateTime reference)
        {
            return
                m1Open < m1NextOpen &&
                m5Open < m5NextOpen &&
                m1Open >= m5Open &&
                m1Open < m5NextOpen &&
                m1NextOpen <= m5NextOpen &&
                m1NextOpen <= reference;
        }

        internal static bool IsReady(
            int direction,
            int m1Direction,
            double open,
            double high,
            double low,
            double close,
            double atr,
            double minimumBodyAtr,
            double minimumCloseLocation,
            double maximumRangeAtr,
            int triggerScore,
            int requiredTrigger,
            double priorMicroHigh,
            double priorMicroLow,
            double structureBreakBuffer,
            bool useDisplacement,
            double displacementAtr)
        {
            if (direction != 1 && direction != -1)
                return false;

            if (m1Direction != direction ||
                atr <= 0 ||
                high <= low ||
                priorMicroHigh <= 0 ||
                priorMicroLow <= 0)
                return false;

            double range = high - low;
            double body = Math.Abs(close - open);

            if (body < atr * minimumBodyAtr ||
                range > atr * maximumRangeAtr)
                return false;

            bool candleAligned =
                direction == 1
                    ? close > open
                    : close < open;

            if (!candleAligned)
                return false;

            double location =
                direction == 1
                    ? (close - low) / range
                    : (high - close) / range;

            if (location < minimumCloseLocation)
                return false;

            double buffer =
                Math.Max(
                    0,
                    atr * Math.Max(0, structureBreakBuffer));

            bool microStructureBreak =
                direction == 1
                    ? close > priorMicroHigh + buffer
                    : close < priorMicroLow - buffer;

            bool displacement =
                useDisplacement &&
                displacementAtr > 0 &&
                body >= atr * displacementAtr;

            // Technical agreement alone is not a causal trigger. The closed
            // M1 bar must either break recent micro-structure or displace with
            // the configured displacement threshold.
            if (!microStructureBreak &&
                !displacement)
                return false;

            return TriggerThresholdRule.IsScoreReady(
                triggerScore,
                requiredTrigger);
        }
    }
}
