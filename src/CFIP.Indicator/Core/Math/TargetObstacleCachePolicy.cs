using System;

namespace cAlgo
{
    internal readonly struct TargetObstacleCacheKey : IEquatable<TargetObstacleCacheKey>
    {
        public int BarCount { get; }
        public int Index { get; }
        public long OpenTimeTicks { get; }
        public int Direction { get; }
        public int SwingStrength { get; }
        public int TargetLookbackBars { get; }
        public int LiquidityLookback { get; }
        public bool UseEqualHighLow { get; }
        public double EqualityTolerance { get; }
        public double PipSize { get; }

        public TargetObstacleCacheKey(
            int barCount,
            int index,
            long openTimeTicks,
            int direction,
            int swingStrength,
            int targetLookbackBars,
            int liquidityLookback,
            bool useEqualHighLow,
            double equalityTolerance,
            double pipSize)
        {
            BarCount = barCount;
            Index = index;
            OpenTimeTicks = openTimeTicks;
            Direction = direction;
            SwingStrength = swingStrength;
            TargetLookbackBars = targetLookbackBars;
            LiquidityLookback = liquidityLookback;
            UseEqualHighLow = useEqualHighLow;
            EqualityTolerance = equalityTolerance;
            PipSize = pipSize;
        }

        public bool Equals(TargetObstacleCacheKey other)
        {
            return BarCount == other.BarCount &&
                   Index == other.Index &&
                   OpenTimeTicks == other.OpenTimeTicks &&
                   Direction == other.Direction &&
                   SwingStrength == other.SwingStrength &&
                   TargetLookbackBars == other.TargetLookbackBars &&
                   LiquidityLookback == other.LiquidityLookback &&
                   UseEqualHighLow == other.UseEqualHighLow &&
                   EqualityTolerance == other.EqualityTolerance &&
                   PipSize == other.PipSize;
        }

        public override bool Equals(object obj)
        {
            return obj is TargetObstacleCacheKey &&
                   Equals((TargetObstacleCacheKey)obj);
        }
    }

    internal static class TargetObstacleCachePolicy
    {
        public const int MaximumEntries = 16;

        public static bool IsSupportedDirection(int direction)
        {
            return direction == 1 || direction == -1;
        }

        public static bool IsObsoleteSameBars(
            TargetObstacleCacheKey cached,
            TargetObstacleCacheKey current)
        {
            return cached.BarCount != current.BarCount ||
                   cached.Index != current.Index ||
                   cached.OpenTimeTicks != current.OpenTimeTicks;
        }
    }
}