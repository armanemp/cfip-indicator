namespace cAlgo
{
    internal static class TargetCandidateRejectionReasons
    {
        internal const string InvalidGeometry = "TARGET_GEOMETRY_INVALID";
        internal const string TargetSideInvalid = "TARGET_SIDE_INVALID";
        internal const string M5SetupTooOld = "M5_SETUP_TOO_OLD";
        internal const string HtfTargetTooOld = "HTF_TARGET_TOO_OLD";
        internal const string HtfSourceRequired = "HTF_SOURCE_REQUIRED";
        internal const string HtfQualityTooLow = "HTF_QUALITY_TOO_LOW";
        internal const string RewardRiskInvalid = "RR_INVALID";
        internal const string RewardRiskBelowMinimum = "RR_BELOW_MIN";
        internal const string RewardRiskAboveMaximum = "RR_ABOVE_MAX";
        internal const string TargetTooFar = "TARGET_TOO_FAR";
        internal const string TargetSpacingConflict = "TARGET_SPACING_CONFLICT";
        internal const string TargetProgressionInvalid = "TARGET_PROGRESSION_INVALID";
        internal const string M5Obstacle = "OBSTACLE_SWING";
        internal const string EqualHighLowObstacle = "OBSTACLE_EQ";
        internal const string OpposingZoneObstacle = "OBSTACLE_OPPOSING_ZONE";
        internal const string HtfZoneObstacle = "OBSTACLE_HTF_ZONE";
        internal const string StageRequiredAboveMaximum = "STAGE_REQUIRED_RR_ABOVE_MAX";
        internal const string StageUnreachableByExtension = "STAGE_UNREACHABLE_BY_EXTENSION";
    }
}
