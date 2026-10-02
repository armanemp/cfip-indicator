using System;

namespace cAlgo
{
    internal readonly struct ExecutionZoneSelectionInput
    {
        public int Quality { get; }
        public double DistanceAtr { get; }
        public int Age { get; }
        public bool IsPrimaryTimeframe { get; }
        public bool IsFvg { get; }
        public bool IsOrderBlock { get; }
        public bool IsObFvgConfluence { get; }
        public bool HasMtfOverlap { get; }

        public ExecutionZoneSelectionInput(
            int quality,
            double distanceAtr,
            int age,
            bool isPrimaryTimeframe,
            bool isFvg,
            bool isOrderBlock,
            bool isObFvgConfluence,
            bool hasMtfOverlap)
        {
            Quality = NumericGuards.ClampInt(quality, 0, 100);
            DistanceAtr = NumericGuards.IsFiniteValue(distanceAtr)
                ? Math.Max(0, distanceAtr)
                : double.PositiveInfinity;
            Age = Math.Max(0, age);
            IsPrimaryTimeframe = isPrimaryTimeframe;
            IsFvg = isFvg;
            IsOrderBlock = isOrderBlock;
            IsObFvgConfluence = isObFvgConfluence;
            HasMtfOverlap = hasMtfOverlap;
        }
    }

    internal readonly struct ExecutionZoneSelectionResult
    {
        public bool Valid { get; }
        public double Score { get; }

        public ExecutionZoneSelectionResult(
            bool valid,
            double score)
        {
            Valid = valid;
            Score = NumericGuards.IsFiniteValue(score)
                ? score
                : double.NegativeInfinity;
        }
    }

    internal static class ExecutionZoneSelectionRule
    {
        internal static ExecutionZoneSelectionResult Evaluate(
            ExecutionZoneSelectionInput input,
            double maximumZoneAgeBars,
            double proximityScaleAtr)
        {
            if (input.Quality <= 0 ||
                !NumericGuards.IsFiniteValue(input.DistanceAtr) ||
                !NumericGuards.IsFiniteValue(maximumZoneAgeBars) ||
                maximumZoneAgeBars <= 0 ||
                !NumericGuards.IsFiniteValue(proximityScaleAtr) ||
                proximityScaleAtr <= 0)
                return new ExecutionZoneSelectionResult(false, 0);

            double proximityPenalty =
                22.0 *
                Math.Min(
                    1.0,
                    input.DistanceAtr /
                    proximityScaleAtr);

            double agePenalty =
                12.0 *
                Math.Min(
                    1.0,
                    input.Age /
                    Math.Max(
                        1.0,
                        maximumZoneAgeBars));

            double score =
                input.Quality -
                proximityPenalty -
                agePenalty;

            if (input.IsPrimaryTimeframe)
                score += 5.0;

            if (input.IsObFvgConfluence)
                score += 15.0;

            if (input.HasMtfOverlap)
                score += 8.0;

            // Preserve a small structural preference for explicit zone provenance.
            if (input.IsOrderBlock)
                score += 2.0;

            if (input.IsFvg)
                score += 1.0;

            return new ExecutionZoneSelectionResult(
                true,
                score);
        }
    }
}
