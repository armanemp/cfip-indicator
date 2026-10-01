using System;

namespace cAlgo
{
    internal readonly struct TopDownCalibrationSnapshot
    {
        public int HtfDirection { get; }
        public int HtfAlignment { get; }
        public int HtfAbsoluteStrength { get; }
        public int MidDirection { get; }
        public int MidAlignment { get; }
        public int MidAbsoluteStrength { get; }
        public int EntryDirection { get; }
        public int EntryAlignment { get; }
        public int EntryAbsoluteStrength { get; }
        public bool HtfStrong { get; }
        public bool Eligible { get; }
        public string Stage { get; }

        public TopDownCalibrationSnapshot(
            int htfDirection,
            int htfAlignment,
            int htfAbsoluteStrength,
            int midDirection,
            int midAlignment,
            int midAbsoluteStrength,
            int entryDirection,
            int entryAlignment,
            int entryAbsoluteStrength,
            bool htfStrong,
            bool eligible,
            string stage)
        {
            HtfDirection = htfDirection;
            HtfAlignment = Math.Max(0, Math.Min(100, htfAlignment));
            HtfAbsoluteStrength = Math.Max(0, Math.Min(100, htfAbsoluteStrength));
            MidDirection = midDirection;
            MidAlignment = Math.Max(0, Math.Min(100, midAlignment));
            MidAbsoluteStrength = Math.Max(0, Math.Min(100, midAbsoluteStrength));
            EntryDirection = entryDirection;
            EntryAlignment = Math.Max(0, Math.Min(100, entryAlignment));
            EntryAbsoluteStrength = Math.Max(0, Math.Min(100, entryAbsoluteStrength));
            HtfStrong = htfStrong;
            Eligible = eligible;
            Stage = string.IsNullOrWhiteSpace(stage) ? "HTF SEARCH" : stage;
        }
    }

    internal static class TopDownCalibrationRule
    {
        internal static TopDownCalibrationSnapshot Evaluate(
            int[] htfDirections,
            int[] htfQualities,
            double[] htfWeights,
            int[] midDirections,
            int[] midQualities,
            double[] midWeights,
            int entryDirection,
            int entryQuality,
            int minimumStrongAlignment,
            int selectedDirection)
        {
            TopDownCalibrationGroupResult htf = EvaluateGroup(
                htfDirections, htfQualities, htfWeights);

            TopDownCalibrationGroupResult mid = EvaluateGroup(
                midDirections, midQualities, midWeights);

            int safeEntryDirection =
                entryQuality > 0 &&
                (entryDirection == 1 || entryDirection == -1)
                    ? entryDirection
                    : 0;

            int entryAlignment =
                safeEntryDirection == 0 || htf.Direction == 0
                    ? 50
                    : safeEntryDirection == htf.Direction ? 100 : 0;

            int entryAbsoluteStrength =
                safeEntryDirection == 0
                    ? 0
                    : Math.Max(0, Math.Min(100, entryQuality));

            int strongThreshold =
                Math.Max(60, Math.Min(90, minimumStrongAlignment));

            bool htfStrong =
                htf.Direction != 0 &&
                htf.Alignment >= strongThreshold &&
                htf.AbsoluteStrength >= strongThreshold;

            int middleConflictThreshold =
                Math.Max(60, strongThreshold - 10);

            bool midStrong =
                mid.Direction != 0 &&
                mid.Alignment >= middleConflictThreshold &&
                mid.AbsoluteStrength >= middleConflictThreshold;

            bool eligible = true;

            if (selectedDirection != 0)
            {
                if (htfStrong && selectedDirection != htf.Direction)
                    eligible = false;

                if (eligible &&
                    htfStrong &&
                    midStrong &&
                    mid.Direction != htf.Direction)
                    eligible = false;

                if (eligible &&
                    htfStrong &&
                    entryAbsoluteStrength >= middleConflictThreshold &&
                    entryAlignment == 0 &&
                    safeEntryDirection != 0)
                    eligible = false;
            }

            string stage;

            if (htf.Direction == 0)
                stage = "HTF SEARCH";
            else if (!htfStrong)
                stage = "HTF MIXED";
            else if (!eligible)
                stage = midStrong && mid.Direction != htf.Direction
                    ? "MIDFRAME CONFLICT"
                    : "ENTRY CONFLICT";
            else if (mid.Direction == htf.Direction &&
                     mid.Alignment >= Math.Max(65, strongThreshold - 5) &&
                     safeEntryDirection == htf.Direction)
                stage = "ENTRY CALIBRATED";
            else if (mid.Direction == htf.Direction)
                stage = "LTF CALIBRATION";
            else
                stage = "MIDFRAME CALIBRATION";

            return new TopDownCalibrationSnapshot(
                htf.Direction, htf.Alignment, htf.AbsoluteStrength,
                mid.Direction, mid.Alignment, mid.AbsoluteStrength,
                safeEntryDirection, entryAlignment, entryAbsoluteStrength,
                htfStrong, eligible, stage);
        }

        private static TopDownCalibrationGroupResult EvaluateGroup(
            int[] directions,
            int[] qualities,
            double[] weights)
        {
            double bull = 0;
            double bear = 0;
            double bullWeight = 0;
            double bearWeight = 0;

            int count = Math.Min(
                directions == null ? 0 : directions.Length,
                Math.Min(
                    qualities == null ? 0 : qualities.Length,
                    weights == null ? 0 : weights.Length));

            for (int i = 0; i < count; i++)
            {
                if (directions[i] != 1 && directions[i] != -1)
                    continue;

                if (qualities[i] <= 0 || weights[i] <= 0)
                    continue;

                double qualityFactor =
                    Math.Max(0.0, Math.Min(1.0, qualities[i] / 100.0));

                double contribution = weights[i] * qualityFactor;

                if (directions[i] == 1)
                {
                    bull += contribution;
                    bullWeight += weights[i];
                }
                else
                {
                    bear += contribution;
                    bearWeight += weights[i];
                }
            }

            double total = bull + bear;
            if (total <= 0)
                return new TopDownCalibrationGroupResult(0, 0, 0);

            int direction = bull == bear ? 0 : bull > bear ? 1 : -1;
            double dominantWeight = direction == 1 ? bullWeight : bearWeight;

            int alignment =
                (int)Math.Round(100.0 * Math.Max(bull, bear) / total);

            int absoluteStrength =
                dominantWeight <= 0
                    ? 0
                    : (int)Math.Round(
                        100.0 * Math.Max(bull, bear) / dominantWeight);

            return new TopDownCalibrationGroupResult(
                direction,
                Math.Max(0, Math.Min(100, alignment)),
                Math.Max(0, Math.Min(100, absoluteStrength)));
        }
    }
}
