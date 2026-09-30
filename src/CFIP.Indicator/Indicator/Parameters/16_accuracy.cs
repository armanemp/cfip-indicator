using cAlgo.API;

namespace cAlgo
{
    public partial class CFIPIndicator : Indicator
    {
        [Parameter("Use Smart Entry Quality Filter", Group = "16 · Accuracy", DefaultValue = true)]
        public bool UseSmartEntryQualityFilter { get; set; }

        [Parameter("Smart Quality Threshold", Group = "16 · Accuracy", DefaultValue = 70, MinValue = 40, MaxValue = 95)]
        public int SmartQualityThreshold { get; set; }

        [Parameter("Require Fresh M5 Trigger", Group = "16 · Accuracy", DefaultValue = true)]
        public bool RequireFreshM5Trigger { get; set; }

        [Parameter("Minimum Fresh Trigger Evidence", Group = "16 · Accuracy", DefaultValue = 3, MinValue = 1, MaxValue = 8)]
        public int MinimumFreshTriggerEvidence { get; set; }

        [Parameter("Use False Signal Guard", Group = "16 · Accuracy", DefaultValue = true)]
        public bool UseFalseSignalGuard { get; set; }

        [Parameter("False Signal Adverse R", Group = "16 · Accuracy", DefaultValue = 1.10, MinValue = 0.25, MaxValue = 5)]
        public double FalseSignalAdverseR { get; set; }

        [Parameter("Enable Soft Adverse-R Invalidation", Group = "16 · Accuracy", DefaultValue = true)]
        public bool EnableSoftAdverseRInvalidation { get; set; }

        [Parameter("False Signal Watch Bars", Group = "16 · Accuracy", DefaultValue = 3, MinValue = 1, MaxValue = 12)]
        public int FalseSignalWatchBars { get; set; }

        [Parameter("Invalidate On False Signal", Group = "16 · Accuracy", DefaultValue = true)]
        public bool InvalidateOnFalseSignal { get; set; }

        [Parameter("Enable Setup Invalidation", Group = "16 · Accuracy", DefaultValue = true)]
        public bool EnableSetupInvalidation { get; set; }

        [Parameter("Invalidation Structure ATR", Group = "16 · Accuracy", DefaultValue = 0.10, MinValue = 0.02, MaxValue = 0.50)]
        public double InvalidationStructureAtr { get; set; }

        [Parameter("Invalidation Zone Close ATR", Group = "16 · Accuracy", DefaultValue = 0.10, MinValue = 0.02, MaxValue = 0.75)]
        public double InvalidationZoneCloseAtr { get; set; }

        [Parameter("Invalidation Max Adverse R", Group = "16 · Accuracy", DefaultValue = 0.75, MinValue = 0.30, MaxValue = 2.00, Step = 0.05)]
        public double InvalidationMaxAdverseR { get; set; }

        [Parameter("Require MTF Flip For Invalidation", Group = "16 · Accuracy", DefaultValue = true)]
        public bool RequireMtfFlipForInvalidation { get; set; }

        [Parameter("Allow Reversal Against Stale HTF", Group = "16 · Accuracy", DefaultValue = true)]
        public bool AllowReversalAgainstStaleHtf { get; set; }

        [Parameter("Enable Live Structural Reversal", Group = "16 · Accuracy", DefaultValue = true)]
        public bool EnableLiveStructuralReversal { get; set; }

        [Parameter("Live Reversal Minimum Confidence", Group = "16 · Accuracy", DefaultValue = 68, MinValue = 50, MaxValue = 95)]
        public int LiveReversalMinimumConfidence { get; set; }

        [Parameter("Live Reversal Minimum Evidence", Group = "16 · Accuracy", DefaultValue = 3, MinValue = 2, MaxValue = 8)]
        public int LiveReversalMinimumEvidence { get; set; }

        [Parameter("Live Reversal Structural Score", Group = "16 · Accuracy", DefaultValue = 72, MinValue = 50, MaxValue = 100)]
        public int LiveReversalStructuralScore { get; set; }

        [Parameter("Require Reversal Force", Group = "16 · Accuracy", DefaultValue = true)]
        public bool RequireReversalForce { get; set; }

        [Parameter("Opposite Signal Cooldown M5", Group = "16 · Accuracy", DefaultValue = 5, MinValue = 0, MaxValue = 50)]
        public int OppositeSignalCooldownM5 { get; set; }

        [Parameter("Prevent Rapid Direction Flip", Group = "16 · Accuracy", DefaultValue = true)]
        public bool PreventRapidDirectionFlip { get; set; }

        [Parameter("Require M15 Reversal For Opposite", Group = "16 · Accuracy", DefaultValue = true)]
        public bool RequireM15ReversalForOpposite { get; set; }

        [Parameter("Allow Opposite While Active", Group = "16 · Accuracy", DefaultValue = false)]
        public bool AllowOppositeWhileActive { get; set; }

        [Parameter("Minimum Opposite M5 Structure", Group = "16 · Accuracy", DefaultValue = 2, MinValue = 1, MaxValue = 6)]
        public int MinimumOppositeM5Structure { get; set; }

        [Parameter("Exit Reentry Cooldown M5", Group = "16 · Accuracy", DefaultValue = 3, MinValue = 0, MaxValue = 50)]
        public int ExitReentryCooldownM5 { get; set; }

        [Parameter("Use Structural Sequence Gate", Group = "16 · Accuracy", DefaultValue = true)]
        public bool UseStructuralSequenceGate { get; set; }

        [Parameter("Minimum Structural Sequence", Group = "16 · Accuracy", DefaultValue = 2, MinValue = 1, MaxValue = 5)]
        public int MinimumStructuralSequence { get; set; }

        [Parameter("Require Entry Location Confluence", Group = "16 · Accuracy", DefaultValue = true)]
        public bool RequireEntryLocationConfluence { get; set; }

        [Parameter("Minimum Entry Location Quality", Group = "16 · Accuracy", DefaultValue = 64, MinValue = 40, MaxValue = 95)]
        public int MinimumEntryLocationQuality { get; set; }

        [Parameter("Use Reward Quality Gate", Group = "16 · Accuracy", DefaultValue = true)]
        public bool UseProxyExpectedValueGate { get; set; }

        [Parameter("Minimum Reward Quality Floor", Group = "16 · Accuracy", DefaultValue = 0.20, MinValue = -1, MaxValue = 2, Step = 0.05)]
        public double MinimumProxyExpectedValue { get; set; }
    }
}
