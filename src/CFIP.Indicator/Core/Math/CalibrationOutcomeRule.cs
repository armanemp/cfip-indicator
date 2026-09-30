namespace cAlgo
{
    internal static class CalibrationOutcomeRule
    {
        public static bool IsPositiveRealizedR(double realizedR)
        {
            return !double.IsNaN(realizedR) &&
                   !double.IsInfinity(realizedR) &&
                   realizedR > 0;
        }
    }
}
