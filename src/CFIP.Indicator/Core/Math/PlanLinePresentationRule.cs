namespace cAlgo
{
    internal static class PlanLinePresentationRule
    {
        internal const int MinimumThickness = 1;
        internal const int MaximumThickness = 3;

        public static int ResolveThickness(
            int configuredThickness)
        {
            if (configuredThickness < MinimumThickness)
                return MinimumThickness;

            if (configuredThickness > MaximumThickness)
                return MaximumThickness;

            return configuredThickness;
        }
    }
}
