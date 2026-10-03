using cAlgo.API;

namespace cAlgo
{
    internal static class PlanLinePresentationRule
    {
        internal const int MinimumThickness = 1;
        internal const int MaximumThickness = 3;
        internal const int SignalLineAlpha = 220;

        public static int ResolveThickness(
            int configuredThickness)
        {
            // One visual contract for every signal/plan level.
            return MinimumThickness;
        }

        public static Color ResolveColor(
            Color semanticColor)
        {
            // Keep semantic identity while using a restrained alpha for a
            // cleaner modern chart surface. Every line uses the same treatment.
            return Color.FromArgb(
                SignalLineAlpha,
                semanticColor);
        }
    }
}
