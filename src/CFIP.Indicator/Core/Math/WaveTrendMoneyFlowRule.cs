namespace cAlgo
{
    internal static class WaveTrendMoneyFlowRule
    {
        internal static bool TryCalculateContribution(
            double typical,
            double previousTypical,
            double tickVolume,
            out double positiveFlow,
            out double negativeFlow)
        {
            positiveFlow = 0;
            negativeFlow = 0;

            if (!IsFiniteNonNegative(typical) ||
                !IsFiniteNonNegative(previousTypical) ||
                !IsFiniteNonNegative(tickVolume))
                return false;

            if (tickVolume <= 0 ||
                typical == previousTypical)
                return true;

            double money = typical * tickVolume;
            if (!IsFiniteNonNegative(money))
                return false;

            if (typical > previousTypical)
                positiveFlow = money;
            else
                negativeFlow = money;

            return true;
        }

        private static bool IsFiniteNonNegative(double value)
        {
            return !double.IsNaN(value) &&
                   !double.IsInfinity(value) &&
                   value >= 0;
        }
    }
}
