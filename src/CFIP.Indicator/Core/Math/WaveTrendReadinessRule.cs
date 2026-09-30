
    internal static class WaveTrendReadinessRule
    {
        internal static int ResolveComponentReadyIndex(
            int length,
            int momentumLength)
        {
            int safeLength =
                Math.Max(
                    2,
                    length);

            int safeMomentum =
                Math.Max(
                    1,
                    momentumLength);

            return
                Math.Max(
                    safeLength,
                    safeMomentum +
                    safeLength -
                    1);
        }

        internal static int ResolveSmoothReadyIndex(
            int componentReadyIndex,
            int smoothType,
            int smoothLength)
        {
            return
                componentReadyIndex +
                WaveTrendMovingAverageCalculator.RequiredSourceBars(
                    smoothType,
                    smoothLength) -
                1;
        }

        internal static int ResolveSignalReadyIndex(
            int smoothReadyIndex,
            int signalType,
            int signalLength)
        {
            return
                smoothReadyIndex +
                WaveTrendMovingAverageCalculator.RequiredSourceBars(
                    signalType,
                    signalLength) -
                1;
        }

        internal static bool IsSnapshotReady(
            int index,
            int signalReadyIndex)
        {
            return
                index >
                signalReadyIndex;
        }
    }
}

