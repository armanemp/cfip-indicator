namespace cAlgo
{
    internal static class PlanLineThicknessRule
    {
        internal const int MinimumThickness = 1;
        internal const int MaximumThickness = 3;

        public static int ResolveThickness(int configuredThickness)
        {
            return MinimumThickness;
        }
    }
}
