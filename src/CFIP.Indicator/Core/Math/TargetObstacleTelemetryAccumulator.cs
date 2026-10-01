using System;
using System.Globalization;

namespace cAlgo
{
    internal sealed class TargetObstacleTelemetryAccumulator
    {
        public int Count { get; private set; }
        public double MinTargetDistanceAtr { get; private set; } = double.PositiveInfinity;
        public double MaxTargetDistanceAtr { get; private set; }
        public double MinTargetExtensionPercent { get; private set; } = double.PositiveInfinity;
        public double MaxTargetExtensionPercent { get; private set; }
        public double MinObstacleDistanceAtr { get; private set; } = double.PositiveInfinity;
        public double MaxObstacleDistanceAtr { get; private set; }

        public void Observe(
            double targetDistanceAtr,
            double obstacleDistanceAtr,
            double maximumTargetExtensionAtr)
        {
            if (!IsFinitePositiveDistance(targetDistanceAtr))
                return;

            double extension =
                Math.Max(
                    1.0,
                    IsFinitePositiveDistance(maximumTargetExtensionAtr)
                        ? maximumTargetExtensionAtr
                        : 1.0);

            double targetExtensionPercent =
                Math.Max(
                    0,
                    (targetDistanceAtr / extension) * 100.0);

            Count++;

            MinTargetDistanceAtr =
                Math.Min(
                    MinTargetDistanceAtr,
                    targetDistanceAtr);

            MaxTargetDistanceAtr =
                Math.Max(
                    MaxTargetDistanceAtr,
                    targetDistanceAtr);

            MinTargetExtensionPercent =
                Math.Min(
                    MinTargetExtensionPercent,
                    targetExtensionPercent);

            MaxTargetExtensionPercent =
                Math.Max(
                    MaxTargetExtensionPercent,
                    targetExtensionPercent);

            if (IsFinitePositiveOrZero(obstacleDistanceAtr))
            {
                MinObstacleDistanceAtr =
                    Math.Min(
                        MinObstacleDistanceAtr,
                        obstacleDistanceAtr);

                MaxObstacleDistanceAtr =
                    Math.Max(
                        MaxObstacleDistanceAtr,
                        obstacleDistanceAtr);
            }
        }

        public string FormatSummary()
        {
            if (Count <= 0)
                return string.Empty;

            string summary =
                "TARGET_ATR_MIN=" +
                MinTargetDistanceAtr.ToString(
                    "0.00",
                    CultureInfo.InvariantCulture) +
                " • TARGET_ATR_MAX=" +
                MaxTargetDistanceAtr.ToString(
                    "0.00",
                    CultureInfo.InvariantCulture) +
                " • EXT_PCT_MIN=" +
                MinTargetExtensionPercent.ToString(
                    "0.0",
                    CultureInfo.InvariantCulture) +
                " • EXT_PCT_MAX=" +
                MaxTargetExtensionPercent.ToString(
                    "0.0",
                    CultureInfo.InvariantCulture);

            if (!double.IsPositiveInfinity(
                    MinObstacleDistanceAtr))
            {
                summary +=
                    " • OBSTACLE_ATR_MIN=" +
                    MinObstacleDistanceAtr.ToString(
                        "0.00",
                        CultureInfo.InvariantCulture) +
                    " • OBSTACLE_ATR_MAX=" +
                    MaxObstacleDistanceAtr.ToString(
                        "0.00",
                        CultureInfo.InvariantCulture);
            }

            return summary;
        }

        private static bool IsFinitePositiveDistance(double value)
        {
            return
                !double.IsNaN(value) &&
                !double.IsInfinity(value) &&
                value > 0;
        }

        private static bool IsFinitePositiveOrZero(double value)
        {
            return
                !double.IsNaN(value) &&
                !double.IsInfinity(value) &&
                value >= 0;
        }
    }
}
