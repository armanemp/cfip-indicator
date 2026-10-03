namespace cAlgo
{
    internal static class PlanLinePresentationRule
    {
        internal const int MinimumThickness = 1;
        internal const int MaximumThickness = 3;

        public static int ResolveThickness(
            int configuredThickness)
        {
            // Signal/plan level geometry has one visual contract: one-pixel
            // solid lines. The parameter remains readable for compatibility,
            // but it cannot create a second visual language.
            return MinimumThickness;
        }
    }
}
