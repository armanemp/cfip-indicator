namespace cAlgo
{
    internal static class PlanLinePresentationRule
    {
        internal const int MinimumThickness = 1;
        internal const int MaximumThickness = 3;
        internal const int SignalLineAlpha = 255;

        public static int ResolveThickness(
            int configuredThickness)
        {
            // One visual contract for every signal/plan level.
            return MinimumThickness;
        }
    }
}
