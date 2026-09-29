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
            int requiredTrigger)
        {
            if (direction != 1 && direction != -1)
                return false;

            if (m1Direction != direction ||
                atr <= 0 ||
                high <= low)
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

            return triggerScore >= requiredTrigger;
        }
    }
}
