namespace CFIP.cBot.Risk
{
    internal static class ExecutionMarginBudgetRule
    {
        public static double AllowedMargin(
            double freeMargin,
            double maximumUsagePercent,
            double bufferPercent)
        {
            if (double.IsNaN(freeMargin) ||
                double.IsInfinity(freeMargin) ||
                freeMargin <= 0)
                return 0;

            double usage =
                Math.Max(
                    10,
                    Math.Min(
                        100,
                        maximumUsagePercent -
                        Math.Max(
                            0,
                            Math.Min(40, bufferPercent))));

            return freeMargin * usage / 100.0;
        }

        public static double ScaleVolumeToBudget(
            double requestedVolume,
            double estimatedMargin,
            double allowedMargin)
        {
            if (double.IsNaN(requestedVolume) ||
                double.IsInfinity(requestedVolume) ||
                requestedVolume <= 0 ||
                double.IsNaN(estimatedMargin) ||
                double.IsInfinity(estimatedMargin) ||
                estimatedMargin <= 0 ||
                double.IsNaN(allowedMargin) ||
                double.IsInfinity(allowedMargin) ||
                allowedMargin <= 0)
                return 0;

            if (estimatedMargin <= allowedMargin)
                return requestedVolume;

            return requestedVolume *
                   allowedMargin /
                   estimatedMargin;
        }
    }
}