using System;

namespace cAlgo
{
    internal static class EntryActionabilityPolicy
    {
        public const double LongExtremeRangePosition = 0.85;
        public const double ShortExtremeRangePosition = 0.15;
        public const double LongNearExtremeRangePosition = 0.75;
        public const double ShortNearExtremeRangePosition = 0.25;

        public const int ExtremeLocationRisk = 45;
        public const int NearExtremeLocationRisk = 25;

        public const double M5AdverseRiskAtrWeight = 60;
        public const int M5AdverseRiskCap = 28;
        public const double M1AdverseRiskAtrWeight = 45;
        public const int M1AdverseRiskCap = 18;

        public const int DivergenceLowQuality = 70;
        public const int DivergenceMediumQuality = 78;
        public const int DivergenceHighQuality = 86;
        public const int DivergenceLowRisk = 16;
        public const int DivergenceMediumRisk = 25;
        public const int DivergenceHighRisk = 32;
        public const int SupportiveHiddenDivergenceRiskAdjustment = 10;

        public const double StrongAdverseM5Atr = 0.45;
        public const double StrongAdverseM1Atr = 0.40;
        public const int StrongAdverseRiskFloor = 75;

        public const double AdverseM5BlockAtr = 0.30;
        public const double AdverseM1BlockAtr = 0.45;

        public const int ExecutionZoneQualityFloor = 40;
        public const int ContinuationStructuralConfirmationsFloor = 3;

        public const double TriggerPipToleranceFraction = 0.10;
        public const double BreakoutLateExtensionFloorAtr = 0.10;
        public const double RetestLateDistanceFloorAtr = 0.05;
        public const double MicroConflictAdverseM1Atr = 0.25;
        public const double MicroConflictEntryDistanceAtr = 0.10;

        public static bool IsRetestReady(bool insideZone, bool triggerReached)
        {
            return insideZone && !triggerReached;
        }

        public static bool ShouldBlockTrapRisk(ExecutionMode mode)
        {
            return mode != ExecutionMode.BreakoutMarket;
        }

        public static double ResolveAnchor(
            ExecutionMode mode,
            double trigger,
            double ideal,
            double fallback)
        {
            double anchor =
                mode == ExecutionMode.BreakoutMarket
                    ? trigger
                    : ideal;

            return IsFinitePositive(anchor)
                ? anchor
                : fallback;
        }

        public static double ResolveActualEntry(
            ExecutionMode mode,
            double market,
            double previewEntry)
        {
            return mode == ExecutionMode.WaitingForTrigger
                ? previewEntry
                : market;
        }

        public static bool IsLate(
            ExecutionMode mode,
            double triggerExtensionAtr,
            double entryDistanceAtr,
            double maximumExtensionAtr,
            double maximumDistanceAtr)
        {
            if (mode == ExecutionMode.BreakoutMarket)
                return triggerExtensionAtr >
                    Math.Max(
                        BreakoutLateExtensionFloorAtr,
                        maximumExtensionAtr);

            return entryDistanceAtr >
                Math.Max(
                    RetestLateDistanceFloorAtr,
                    maximumDistanceAtr);
        }

        public static bool IsMicroConflict(
            bool m1DirectionConflict,
            double adverseM1Atr,
            double entryDistanceAtr)
        {
            return
                m1DirectionConflict &&
                (adverseM1Atr >= MicroConflictAdverseM1Atr ||
                 entryDistanceAtr >= MicroConflictEntryDistanceAtr);
        }

        public static double ResolveTriggerTolerance(
            double tickSize,
            double pipSize)
        {
            double safeTick = IsFiniteNonNegative(tickSize) ? tickSize : 0;
            double safePip = IsFiniteNonNegative(pipSize) ? pipSize : 0;

            return Math.Max(
                safeTick,
                safePip * TriggerPipToleranceFraction);
        }

        public static int ResolveContinuationStructuralMinimum(
            int configuredMinimum)
        {
            return Math.Max(
                ContinuationStructuralConfirmationsFloor,
                configuredMinimum);
        }

        private static bool IsFinitePositive(double value)
        {
            return
                !double.IsNaN(value) &&
                !double.IsInfinity(value) &&
                value > 0;
        }

        private static bool IsFiniteNonNegative(double value)
        {
            return
                !double.IsNaN(value) &&
                !double.IsInfinity(value) &&
                value >= 0;
        }
    }
}