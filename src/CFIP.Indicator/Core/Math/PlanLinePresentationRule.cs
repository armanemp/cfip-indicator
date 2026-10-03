namespace cAlgo
{
    internal static class PlanLinePresentationRule
    {
        internal const int MinimumThickness = 1;
        internal const int MaximumThickness = 3;

        public static int ResolveThickness(
            int configuredThickness)
        {
            // The public parameter remains backward-compatible, but the
            // canonical chart presentation is intentionally fixed at one
            // pixel for all signal/plan level lines.
            return MinimumThickness;
        }
    }
}
