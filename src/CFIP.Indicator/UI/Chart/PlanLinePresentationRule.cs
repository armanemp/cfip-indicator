using cAlgo.API;

namespace cAlgo
{
    internal static class PlanLinePresentationRule
    {
        internal const int MinimumThickness = 1;
        internal const int MaximumThickness = 3;
        internal const int SignalLineAlpha = 255;

        public static int ResolveThickness(int configuredThickness)
        {
            return PlanLineThicknessRule.ResolveThickness(configuredThickness);
        }

        public static Color ResolveColor(Color semanticColor)
        {
            return Color.FromArgb(
                SignalLineAlpha,
                semanticColor);
        }
    }
}
