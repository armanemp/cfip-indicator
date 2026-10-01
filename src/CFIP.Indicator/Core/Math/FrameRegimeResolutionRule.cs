namespace cAlgo
{
    internal static class FrameRegimeResolutionRule
    {
        public const string Unknown = MarketRegimeIdentity.Unknown;

        public static string ResolveFrameSnapshotRegime(
            MarketRegimeSnapshot snapshot)
        {
            return NormalizeFrameRegimeValue(
                snapshot == null
                    ? null
                    : snapshot.Regime);
        }

        public static string NormalizeFrameRegimeValue(
            string regime)
        {
            return MarketRegimeIdentity.Normalize(regime);
        }

        public static bool IsNeutral(
            string regime)
        {
            return NormalizeFrameRegimeValue(regime) == Unknown;
        }
    }
}
