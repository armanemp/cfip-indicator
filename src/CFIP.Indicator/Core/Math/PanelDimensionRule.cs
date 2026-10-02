namespace cAlgo
{
    // Pure panel geometry owner. UI modules consume these bounded values rather
    // than repeating width/padding/border arithmetic in multiple render paths.
    internal static class PanelDimensionRule
    {
        internal static int EffectiveWidth(int configuredWidth)
        {
            return ClampPanelDimension(
                configuredWidth,
                220,
                700);
        }

        internal static int EffectiveContentWidth(
            int configuredWidth,
            int padding,
            int borderThickness)
        {
            return ClampPanelDimension(
                EffectiveWidth(configuredWidth) -
                2 * ClampNonNegative(padding) -
                2 * ClampNonNegative(borderThickness),
                200,
                700);
        }

        private static int ClampPanelDimension(
            int value,
            int minimum,
            int maximum)
        {
            return value < minimum
                ? minimum
                : value > maximum
                    ? maximum
                    : value;
        }

        private static int ClampNonNegative(int value)
        {
            return value < 0 ? 0 : value;
        }
    }
}
