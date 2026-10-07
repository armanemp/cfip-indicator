using System;
using cAlgo.API;
using cAlgo.API.Internals;

namespace cAlgo
{
    internal sealed class RangeManipulationSnapshot
    {
        public static RangeManipulationSnapshot Empty =>
            new RangeManipulationSnapshot
            {
                State = "NONE",
                Direction = 0,
                Score = 0,
                High = 0,
                Low = 0,
                WidthAtr = 0,
                StartIndex = -1,
                EndIndex = -1,
                SweepIndex = -1,
                BreakoutIndex = -1,
                RetestIndex = -1,
                Reason = "NO RANGE"
            };

        public string State { get; set; }
        public int Direction { get; set; }
        public int Score { get; set; }
        public double High { get; set; }
        public double Low { get; set; }
        public double WidthAtr { get; set; }
        public int StartIndex { get; set; }
        public int EndIndex { get; set; }
        public int SweepIndex { get; set; }
        public int BreakoutIndex { get; set; }
        public int RetestIndex { get; set; }
        public bool IsRange { get; set; }
        public bool IsManipulation { get; set; }
        public bool IsConfirmedBreakout { get; set; }
        public bool IsRetest { get; set; }
        public bool IsFailedBreakout { get; set; }
        public string Reason { get; set; }
    }
}

namespace cAlgo
{
    internal sealed class RangeManipulationAnalyzer
    {
        public RangeManipulationSnapshot Evaluate(
            Bars bars,
            int index,
            int lookback,
            int minTouches,
            double maxWidthAtr,
            double minWidthAtr,
            double sweepAtr,
            double pipSize,
            double breakoutAtr,
            double retestAtr,
            int minimumScore)
        {
            RangeManipulationSnapshot empty =
                RangeManipulationSnapshot.Empty;

            if (bars == null ||
                index < 30 ||
                index >= bars.Count)
                return empty;

            double atr = Atr(bars, index, 14);
            if (!IsFinitePositive(atr))
                return empty;

            int span = Math.Max(12, Math.Min(80, lookback));
            int first = Math.Max(2, index - span + 1);

            double high = bars.HighPrices[first];
            double low = bars.LowPrices[first];

            for (int i = first + 1; i <= index; i++)
            {
                high = Math.Max(high, bars.HighPrices[i]);
                low = Math.Min(low, bars.LowPrices[i]);
            }

            double width = high - low;
            double widthAtr = width / atr;

            if (width <= 0 ||
                widthAtr < minWidthAtr ||
                widthAtr > maxWidthAtr)
                return empty;

            int upperTouches = 0;
            int lowerTouches = 0;

            double tolerance = Math.Max(
                pipSize * 2,
                atr * 0.10);

            for (int i = first; i <= index; i++)
            {
                if (Math.Abs(bars.HighPrices[i] - high) <= tolerance)
                    upperTouches++;

                if (Math.Abs(bars.LowPrices[i] - low) <= tolerance)
                    lowerTouches++;
            }

            if (upperTouches < minTouches ||
                lowerTouches < minTouches)
                return empty;

            double efficiency =
                RangeEfficiency(
                    bars,
                    first,
                    index);

            if (efficiency > 0.62)
                return empty;

            int score = 40;
            score += Math.Min(20, (upperTouches + lowerTouches) * 3);
            score += efficiency < 0.38 ? 15 : 7;
            score += widthAtr <= 2.5 ? 10 : 4;

            int sweepDirection = 0;
            int sweepIndex = -1;

            double sweepDistance =
                Math.Max(
                    pipSize * 2,
                    atr * sweepAtr);

            for (int i = Math.Max(first + 2, index - 12);
                 i <= index;
                 i++)
            {
                double open = bars.OpenPrices[i];
                double close = bars.ClosePrices[i];
                double highBar = bars.HighPrices[i];
                double lowBar = bars.LowPrices[i];
                double barRange =
                    Math.Max(
                        pipSize,
                        highBar - lowBar);

                double lowerWick =
                    Math.Max(
                        0,
                        Math.Min(open, close) - lowBar);

                double upperWick =
                    Math.Max(
                        0,
                        highBar - Math.Max(open, close));

                bool bullSweep =
                    lowBar < low - sweepDistance &&
                    close > low &&
                    close > open &&
                    lowerWick / barRange >= 0.25;

                bool bearSweep =
                    highBar > high + sweepDistance &&
                    close < high &&
                    close < open &&
                    upperWick / barRange >= 0.25;

                if (bullSweep == bearSweep)
                    continue;

                sweepDirection = bullSweep ? 1 : -1;
                sweepIndex = i;
            }

            bool bullishBreakout =
                bars.ClosePrices[index] >
                high + atr * breakoutAtr &&
                bars.ClosePrices[index] >
                bars.OpenPrices[index] &&
                Math.Abs(
                    bars.ClosePrices[index] -
                    bars.OpenPrices[index]) >=
                atr * 0.35;

            bool bearishBreakout =
                bars.ClosePrices[index] <
                low - atr * breakoutAtr &&
                bars.ClosePrices[index] <
                bars.OpenPrices[index] &&
                Math.Abs(
                    bars.ClosePrices[index] -
                    bars.OpenPrices[index]) >=
                atr * 0.35;

            bool confirmedBreakout =
                bullishBreakout || bearishBreakout;

            int direction =
                confirmedBreakout
                    ? bullishBreakout ? 1 : -1
                    : sweepDirection;

            if (direction != 0)
                score += 15;

            if (confirmedBreakout)
                score += 20;

            if (sweepDirection != 0)
                score += 15;

            if (direction == 0)
                score = Math.Min(score, 60);

            score = Math.Max(0, Math.Min(100, score));

            if (score < minimumScore &&
                !confirmedBreakout)
                return BuildRangeOnly(
                    high,
                    low,
                    widthAtr,
                    first,
                    index,
                    score,
                    upperTouches,
                    lowerTouches);

            bool retest =
                false;
            int retestIndex = -1;

            if (confirmedBreakout)
            {
                double toleranceRetest =
                    Math.Max(
                        pipSize * 2,
                        atr * retestAtr);

                if (direction > 0)
                {
                    retest =
                        bars.LowPrices[index] <=
                        high + toleranceRetest &&
                        bars.ClosePrices[index] > high;
                }
                else
                {
                    retest =
                        bars.HighPrices[index] >=
                        low - toleranceRetest &&
                        bars.ClosePrices[index] < low;
                }

                if (retest)
                    retestIndex = index;
            }

            bool failed =
                !confirmedBreakout &&
                ((sweepDirection > 0 &&
                  bars.ClosePrices[index] <
                  low - atr * 0.05) ||
                 (sweepDirection < 0 &&
                  bars.ClosePrices[index] >
                  high + atr * 0.05));

            string state;

            if (retest)
                state = direction > 0
                    ? "BULL_RETEST"
                    : "BEAR_RETEST";
            else if (confirmedBreakout)
                state = direction > 0
                    ? "BULL_BREAKOUT"
                    : "BEAR_BREAKOUT";
            else if (sweepDirection != 0)
                state = sweepDirection > 0
                    ? "BULL_MANIPULATION"
                    : "BEAR_MANIPULATION";
            else
                state = "RANGE";

            return new RangeManipulationSnapshot
            {
                State = state,
                Direction = direction,
                Score = score,
                High = high,
                Low = low,
                WidthAtr = widthAtr,
                StartIndex = first,
                EndIndex = index,
                SweepIndex = sweepIndex,
                BreakoutIndex = confirmedBreakout ? index : -1,
                RetestIndex = retestIndex,
                IsRange = true,
                IsManipulation = sweepDirection != 0,
                IsConfirmedBreakout = confirmedBreakout,
                IsRetest = retest,
                IsFailedBreakout = failed,
                Reason =
                    "TOUCHES " +
                    upperTouches +
                    "/" +
                    lowerTouches +
                    " | EFF " +
                    Math.Round(efficiency, 2) +
                    " | ATR " +
                    Math.Round(widthAtr, 2) +
                    " | " +
                    state
            };
        }

        private static RangeManipulationSnapshot BuildRangeOnly(
            double high,
            double low,
            double widthAtr,
            int first,
            int index,
            int score,
            int upperTouches,
            int lowerTouches)
        {
            return new RangeManipulationSnapshot
            {
                State = "RANGE",
                Direction = 0,
                Score = score,
                High = high,
                Low = low,
                WidthAtr = widthAtr,
                StartIndex = first,
                EndIndex = index,
                SweepIndex = -1,
                BreakoutIndex = -1,
                RetestIndex = -1,
                IsRange = true,
                Reason =
                    "RANGE " +
                    upperTouches +
                    "/" +
                    lowerTouches +
                    " TOUCHES"
            };
        }

        private static double RangeEfficiency(
            Bars bars,
            int first,
            int last)
        {
            if (last <= first)
                return 1;

            double path = 0;
            for (int i = first + 1; i <= last; i++)
                path += Math.Abs(
                    bars.ClosePrices[i] -
                    bars.ClosePrices[i - 1]);

            double net = Math.Abs(
                bars.ClosePrices[last] -
                bars.ClosePrices[first]);

            return path <= 0
                ? 0
                : net / path;
        }

        private static double Atr(
            Bars bars,
            int index,
            int period)
        {
            int first =
                Math.Max(
                    1,
                    index - period + 1);

            double sum = 0;
            int count = 0;

            for (int i = first; i <= index; i++)
            {
                double previous =
                    bars.ClosePrices[i - 1];

                double tr =
                    Math.Max(
                        bars.HighPrices[i] -
                        bars.LowPrices[i],
                        Math.Max(
                            Math.Abs(
                                bars.HighPrices[i] -
                                previous),
                            Math.Abs(
                                bars.LowPrices[i] -
                                previous)));

                if (tr > 0 &&
                    !double.IsNaN(tr) &&
                    !double.IsInfinity(tr))
                {
                    sum += tr;
                    count++;
                }
            }

            return count == 0 ? 0 : sum / count;
        }

        private static bool IsFinitePositive(double value)
        {
            return value > 0 &&
                   !double.IsNaN(value) &&
                   !double.IsInfinity(value);
        }
    }
}