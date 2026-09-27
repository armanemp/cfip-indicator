using System;
using System.Collections.Generic;
using System.Linq;
using cAlgo.API;
using cAlgo.API.Indicators;
using cAlgo.API.Internals;

namespace cAlgo
{
    public partial class CFIPIndicator : Indicator
    {
        private sealed class Frame
                {
                    public Bars Bars;
                    public int Index;
                    public double Atr;
                    public double Rsi;
                    public double Adx;
                    public double EmaFast;
                    public double EmaSlow;
                    public int Direction;
                    public int BullScore;
                    public int BearScore;
                    public int Evidence;
                    public int Quality;
                    public bool StructureBull;
                    public bool StructureBear;
                    public bool MssBull;
                    public bool MssBear;
                    public bool ChochBull;
                    public bool ChochBear;
                    public bool DisplacementBull;
                    public bool DisplacementBear;
                    public bool LiquidityBull;
                    public bool LiquidityBear;
                    public bool FvgBull;
                    public bool FvgBear;
                    public bool ObBull;
                    public bool ObBear;
                    public bool TrendBull;
                    public bool TrendBear;
                    public bool MomentumBull;
                    public bool MomentumBear;
                    public bool RejectionBull;
                    public bool RejectionBear;
                    public bool EqualHigh;
                    public bool EqualLow;
                    public bool VolumeBull;
                    public bool VolumeBear;
                    public bool MacdBull;
                    public bool MacdBear;
                    public bool VwapBull;
                    public bool VwapBear;
                    public bool VolatilityBull;
                    public bool VolatilityBear;
                    public bool Choppy;
                }
        
                private sealed class Level
                {
                    public double Price;
                    public double Score;
                    public string Kind;
                    public string Timeframe;
                    public int Age;
                    public int Hits;
                }
        
                private sealed class Zone
                {
                    public double Low;
                    public double High;
                    public int Direction;
                    public string Kind;
                    public int Age;
                    public int Quality;
                }
        
                [Parameter("Enable Early Prediction", Group = "15 · INTELLIGENCE — EARLY", DefaultValue = true)]
                public bool EnableEarlyPrediction { get; set; }
        
                [Parameter("Minimum Early Confidence", Group = "15 · INTELLIGENCE — EARLY", DefaultValue = 56, MinValue = 40, MaxValue = 95)]
                public int MinimumEarlyConfidence { get; set; }
        
                [Parameter("Prediction Lookahead Bars", Group = "15 · INTELLIGENCE — EARLY", DefaultValue = 10, MinValue = 2, MaxValue = 30)]
                public int PredictionLookaheadBars { get; set; }
        
                [Parameter("Show Prediction Zone", Group = "15 · INTELLIGENCE — EARLY", DefaultValue = true)]
                public bool ShowPredictionZone { get; set; }
        
                [Parameter("Show Prediction Targets", Group = "15 · INTELLIGENCE — EARLY", DefaultValue = true)]
                public bool ShowPredictionTargets { get; set; }
        
                [Parameter("Use Liquidity Forecast", Group = "15 · INTELLIGENCE — EARLY", DefaultValue = true)]
                public bool UseLiquidityForecast { get; set; }
        
                [Parameter("Alert On Early Setup", Group = "15 · INTELLIGENCE — EARLY", DefaultValue = true)]
                public bool AlertOnEarlySetup { get; set; }
        
                [Parameter("Alert On BOS", Group = "15 · INTELLIGENCE — EARLY", DefaultValue = true)]
                public bool AlertOnBos { get; set; }
        
                [Parameter("Alert On MSS / CHOCH", Group = "15 · INTELLIGENCE — EARLY", DefaultValue = true)]
                public bool AlertOnMssChoch { get; set; }
        
                [Parameter("Alert On Liquidity Sweep", Group = "15 · INTELLIGENCE — EARLY", DefaultValue = true)]
                public bool AlertOnLiquiditySweep { get; set; }
        
                [Parameter("Enable Outcome Telemetry", Group = "15 · INTELLIGENCE — EARLY", DefaultValue = true)]
                public bool EnableOutcomeTelemetry { get; set; }
        
                [Parameter("Outcome Maximum M5 Bars", Group = "15 · INTELLIGENCE — EARLY", DefaultValue = 72, MinValue = 10, MaxValue = 500)]
                public int OutcomeMaximumM5Bars { get; set; }
        
                [Parameter("Enable Confidence Calibration", Group = "15 · INTELLIGENCE — EARLY", DefaultValue = true)]
                public bool EnableConfidenceCalibration { get; set; }
        
                [Parameter("Use Empirical Calibration", Group = "15 · INTELLIGENCE — EARLY", DefaultValue = true)]
                public bool UseEmpiricalCalibration { get; set; }
        
                [Parameter("Calibration Directional Minimum Samples", Group = "15 · INTELLIGENCE — EARLY", DefaultValue = 6, MinValue = 2, MaxValue = 250)]
                public int CalibrationDirectionalMinimumSamples { get; set; }
        
                [Parameter("Calibration Minimum Samples", Group = "15 · INTELLIGENCE — EARLY", DefaultValue = 5, MinValue = 1, MaxValue = 100)]
                public int CalibrationMinimumSamples { get; set; }
        
                [Parameter("Calibration Max Confidence Adjustment", Group = "15 · INTELLIGENCE — EARLY", DefaultValue = 8, MinValue = 0, MaxValue = 20)]
                public int CalibrationMaxConfidenceAdjustment { get; set; }
        
                [Parameter("Show Outcome Diagnostics", Group = "15 · INTELLIGENCE — EARLY", DefaultValue = true)]
                public bool ShowOutcomeDiagnostics { get; set; }
        
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
        
                [Parameter("Use Proxy Expected Value Gate", Group = "16 · Accuracy", DefaultValue = true)]
                public bool UseProxyExpectedValueGate { get; set; }
        
                [Parameter("Minimum Proxy Expected Value", Group = "16 · Accuracy", DefaultValue = 0.20, MinValue = -1, MaxValue = 2, Step = 0.05)]
                public double MinimumProxyExpectedValue { get; set; }
        
                [Parameter("Enable Smart Decision Engine", Group = "17 · Smart Engine", DefaultValue = true)]
                public bool EnableSmartDecisionEngine { get; set; }
        
                [Parameter("Smart Minimum Timeframe Agreement", Group = "17 · Smart Engine", DefaultValue = 72, MinValue = 50, MaxValue = 95)]
                public int SmartMinimumTimeframeAgreement { get; set; }
        
                [Parameter("Smart Target Minimum RR", Group = "17 · Smart Engine", DefaultValue = 1.50, MinValue = 0.5, MaxValue = 8, Step = 0.05)]
                public double SmartTargetMinimumRR { get; set; }
        
                [Parameter("Smart Target Max Candidates", Group = "17 · Smart Engine", DefaultValue = 32, MinValue = 8, MaxValue = 64)]
                public int SmartTargetMaxCandidates { get; set; }
        
                [Parameter("Smart Regime Quality Floor", Group = "17 · Smart Engine", DefaultValue = 55, MinValue = 30, MaxValue = 90)]
                public int SmartRegimeQualityFloor { get; set; }
        
                [Parameter("No Trade Minimum Smart Quality", Group = "17 · Smart Engine", DefaultValue = 55, MinValue = 30, MaxValue = 90)]
                public int NoTradeMinimumSmartQuality { get; set; }
        
                [Parameter("Block Compression Regime", Group = "17 · Smart Engine", DefaultValue = true)]
                public bool BlockCompressionRegime { get; set; }
        
                [Parameter("Block Weak Range Transition", Group = "17 · Smart Engine", DefaultValue = true)]
                public bool BlockWeakRangeTransition { get; set; }
        
                [Parameter("Alert On Live Reaction", Group = "12 · ALERTS — ADVANCED", DefaultValue = true)]
                public bool AlertOnLiveReaction { get; set; }
        
                [Parameter("Alert On Smart Decision", Group = "12 · ALERTS — ADVANCED", DefaultValue = true)]
                public bool AlertOnSmartDecision { get; set; }
        
                [Parameter("Enable Level Hit Alerts", Group = "12 · ALERTS — ADVANCED", DefaultValue = true)]
                public bool EnableLevelHitAlerts { get; set; }
        
                [Parameter("Alert On TP1", Group = "12 · ALERTS — ADVANCED", DefaultValue = true)]
                public bool AlertOnTp1 { get; set; }
        
                [Parameter("Alert On TP2", Group = "12 · ALERTS — ADVANCED", DefaultValue = true)]
                public bool AlertOnTp2 { get; set; }
        
                [Parameter("Alert On TP3", Group = "12 · ALERTS — ADVANCED", DefaultValue = true)]
                public bool AlertOnTp3 { get; set; }
        
                [Parameter("Alert On TP4", Group = "12 · ALERTS — ADVANCED", DefaultValue = true)]
                public bool AlertOnTp4 { get; set; }
        
                [Parameter("Alert On SL", Group = "12 · ALERTS — ADVANCED", DefaultValue = true)]
                public bool AlertOnSl { get; set; }
        
                [Parameter("Alert On False Signal Risk", Group = "12 · ALERTS — ADVANCED", DefaultValue = true)]
                public bool AlertOnFalseSignalRisk { get; set; }
        
                [Parameter("Alert On Entry Restriction", Group = "12 · ALERTS — ADVANCED", DefaultValue = false)]
                public bool AlertOnEntryRestriction { get; set; }
        
                [Parameter("Alert On High Confidence Entry", Group = "12 · ALERTS — ADVANCED", DefaultValue = true)]
                public bool AlertOnHighConfidenceEntry { get; set; }
        
                [Parameter("High Confidence Threshold", Group = "12 · ALERTS — ADVANCED", DefaultValue = 82, MinValue = 70, MaxValue = 99)]
                public int HighConfidenceThreshold { get; set; }
        
                [Parameter("Alert Sound Type", Group = "12 · ALERTS — ADVANCED", DefaultValue = SoundType.PositiveNotification)]
                public cAlgo.API.SoundType AlertSoundType { get; set; }
        
                [Parameter("Sound File Path", Group = "12 · ALERTS — ADVANCED", DefaultValue = "")]
                public string SoundFilePath { get; set; }
        
                [Parameter("Popup Position", Group = "12 · ALERTS — ADVANCED", DefaultValue = PanelCorner.TopRight)]
                public PanelCorner PopupPosition { get; set; }
        
                [Parameter("Popup Width", Group = "12 · ALERTS — ADVANCED", DefaultValue = 430, MinValue = 220, MaxValue = 700)]
                public int PopupWidth { get; set; }
        
                [Parameter("Keep Popup Until Next Alert", Group = "12 · ALERTS — ADVANCED", DefaultValue = false)]
                public bool KeepPopupUntilNextAlert { get; set; }
        
                [Parameter("Show Popup Close Button", Group = "12 · ALERTS — ADVANCED", DefaultValue = true)]
                public bool ShowPopupCloseButton { get; set; }
        
                [Parameter("Popup Background", Group = "12 · ALERTS — ADVANCED", DefaultValue = "Black")]
                public Color PopupBackgroundColor { get; set; }
        
                [Parameter("Popup Background Alpha", Group = "12 · ALERTS — ADVANCED", DefaultValue = 235, MinValue = 0, MaxValue = 255)]
                public int PopupBackgroundAlpha { get; set; }
        
                [Parameter("Popup Border", Group = "12 · ALERTS — ADVANCED", DefaultValue = "#3A4656")]
                public Color PopupBorderColor { get; set; }
        
                [Parameter("Popup Border Thickness", Group = "12 · ALERTS — ADVANCED", DefaultValue = 1, MinValue = 0, MaxValue = 4)]
                public int PopupBorderThickness { get; set; }
        
                [Parameter("Popup Corner Radius", Group = "12 · ALERTS — ADVANCED", DefaultValue = 5, MinValue = 0, MaxValue = 20)]
                public int PopupCornerRadius { get; set; }
        
                [Parameter("Popup Padding", Group = "12 · ALERTS — ADVANCED", DefaultValue = 8, MinValue = 0, MaxValue = 30)]
                public int PopupPadding { get; set; }
        
                [Parameter("Popup Text Color", Group = "12 · ALERTS — ADVANCED", DefaultValue = "White")]
                public Color PopupTextColor { get; set; }
        
                [Parameter("Line Length Bars", Group = "14 · DISPLAY — ADVANCED", DefaultValue = 40, MinValue = 5, MaxValue = 300)]
                public int LineLengthBars { get; set; }
        
                [Parameter("Line Forward Bars", Group = "14 · DISPLAY — ADVANCED", DefaultValue = 10, MinValue = 1, MaxValue = 100)]
                public int LineForwardBars { get; set; }
        
                [Parameter("Show Signal Labels", Group = "14 · DISPLAY — ADVANCED", DefaultValue = true)]
                public bool ShowSignalLabels { get; set; }
        
                [Parameter("Show Level Price Labels", Group = "14 · DISPLAY — ADVANCED", DefaultValue = true)]
                public bool ShowLevelPriceLabels { get; set; }
        
                [Parameter("Show Context Event Marker", Group = "14 · DISPLAY — ADVANCED", DefaultValue = true)]
                public bool ShowContextEventMarker { get; set; }
        
                [Parameter("Label Left Offset Bars", Group = "14 · DISPLAY — ADVANCED", DefaultValue = 2, MinValue = 1, MaxValue = 10)]
                public int LabelLeftOffsetBars { get; set; }
        
                [Parameter("Show Prediction Objects", Group = "14 · DISPLAY — ADVANCED", DefaultValue = true)]
                public bool ShowPredictionObjects { get; set; }
        
                [Parameter("Arrow Offset ATR", Group = "14 · DISPLAY — ADVANCED", DefaultValue = 0.18, MinValue = 0.02, MaxValue = 1)]
                public double ArrowOffsetAtr { get; set; }
        
                [Parameter("Minimum Arrow Offset Pips", Group = "14 · DISPLAY — ADVANCED", DefaultValue = 2.0, MinValue = 0.5, MaxValue = 20)]
                public double MinimumArrowOffsetPips { get; set; }
        
                [Parameter("Show Early Arrow", Group = "14 · DISPLAY — ADVANCED", DefaultValue = true)]
                public bool ShowEarlyArrow { get; set; }
        
                [Parameter("Show Panel Toggle Button", Group = "14 · DISPLAY — PANEL", DefaultValue = true)]
                public bool ShowPanelToggleButton { get; set; }
        
                [Parameter("Panel Toggle Width", Group = "14 · DISPLAY — PANEL", DefaultValue = 26, MinValue = 22, MaxValue = 40)]
                public int PanelToggleWidth { get; set; }
        
                [Parameter("Panel Toggle Height", Group = "14 · DISPLAY — PANEL", DefaultValue = 26, MinValue = 22, MaxValue = 40)]
                public int PanelToggleHeight { get; set; }
        
                [Parameter("Action Button Width", Group = "13 · AUTO TRADING", DefaultValue = 150, MinValue = 100, MaxValue = 240)]
                public int ActionButtonWidth { get; set; }
        
                [Parameter("Action Button Height", Group = "13 · AUTO TRADING", DefaultValue = 25, MinValue = 20, MaxValue = 50)]
                public int ActionButtonHeight { get; set; }
        
                [Parameter("Auto Protect Broker Positions", Group = "13 · AUTO TRADING", DefaultValue = false)]
                public bool AutoProtectBrokerPositions { get; set; }
        
                [Parameter("Managed Position Label", Group = "13 · AUTO TRADING", DefaultValue = "")]
                public string ManagedPositionLabel { get; set; }
        
                [Parameter("Sync Smart Broker Take Profit", Group = "13 · AUTO TRADING", DefaultValue = true)]
                public bool SyncBrokerTakeProfit { get; set; }
        
                [Parameter("Prevent Broker TP Backward Move", Group = "13 · AUTO TRADING", DefaultValue = true)]
                public bool PreventBrokerTpBackwardMove { get; set; }
        
                [Parameter("Broker Modify Cooldown ms", Group = "13 · AUTO TRADING", DefaultValue = 750, MinValue = 100, MaxValue = 5000, Step = 50)]
                public int BrokerModifyCooldownMs { get; set; }
        
                [Parameter("Enable Aggressive Auto Entry", Group = "13 · AUTO TRADING", DefaultValue = false)]
                public bool EnableAggressiveAutoEntry { get; set; }
        
                [Parameter("Aggressive Minimum Confidence", Group = "13 · AUTO TRADING", DefaultValue = 88, MinValue = 50, MaxValue = 99)]
                public int AggressiveMinimumConfidence { get; set; }
        
                [Parameter("Aggressive Minimum Evidence", Group = "13 · AUTO TRADING", DefaultValue = 4, MinValue = 1, MaxValue = 8)]
                public int AggressiveMinimumEvidence { get; set; }
        
                [Parameter("Aggressive Minimum Smart Quality", Group = "13 · AUTO TRADING", DefaultValue = 78, MinValue = 50, MaxValue = 95)]
                public int AggressiveMinimumSmartQuality { get; set; }
        
                [Parameter("Aggressive Risk % Equity", Group = "13 · AUTO TRADING", DefaultValue = 0.25, MinValue = 0.05, MaxValue = 5)]
                public double AggressiveRiskPercentEquity { get; set; }
        
                [Parameter("Aggressive TP Stage", Group = "13 · AUTO TRADING", DefaultValue = TargetStage.TP1)]
                public TargetStage AggressiveTpStage { get; set; }
        
                [Parameter("Aggressive Require Smart Agreement", Group = "13 · AUTO TRADING", DefaultValue = true)]
                public bool AggressiveRequireSmartAgreement { get; set; }
        
                private sealed class ExecutionIntent
            {
                public int Direction;
                public DecisionPolicyMode Policy;
                public ExecutionIntentKind Kind;
                public double RequestedEntry;
                public double Trigger;
                public double ZoneLow;
                public double ZoneHigh;
                public double Stop;
                public double Target;
                public double StopPips;
                public double TargetPips;
                public double Volume;
                public int CreatedM5;
                public string Source;
            }
        
            private sealed class ExecutionModel
                {
                    public int Direction;
                    public ExecutionMode Mode;
                    public double IdealEntry;
                    public double ActualEntry;
                    public double ZoneLow;
                    public double ZoneHigh;
                    public double Trigger;
                    public double Invalidation;
                    public int Quality;
                    public bool Ready;
                    public string Source;
                }
        
                    private ExecutionIntent BuildExecutionIntent(
                    int direction,
                    DecisionPolicyMode policy,
                    ExecutionIntentKind kind,
                    double entry,
                    double trigger,
                    double zoneLow,
                    double zoneHigh,
                    double stop,
                    double target,
                    double volume,
                    int closedM5,
                    string source)
                {
                    entry = NormalizePrice(entry);
                    stop = NormalizePrice(stop);
                    target = NormalizePrice(target);
        
                    return new ExecutionIntent
                    {
                        Direction = direction,
                        Policy = policy,
                        Kind = kind,
                        RequestedEntry = entry,
                        Trigger = NormalizePrice(trigger),
                        ZoneLow = NormalizePrice(zoneLow),
                        ZoneHigh = NormalizePrice(zoneHigh),
                        Stop = stop,
                        Target = target,
                        StopPips =
                            IsFinitePositive(entry) &&
                            IsFinitePositive(stop)
                                ? Math.Abs(entry - stop) /
                                  Symbol.PipSize
                                : 0,
                        TargetPips =
                            IsFinitePositive(entry) &&
                            IsFinitePositive(target)
                                ? Math.Abs(target - entry) /
                                  Symbol.PipSize
                                : 0,
                        Volume = volume,
                        CreatedM5 = closedM5,
                        Source = source
                    };
                }
        
                private bool ValidateExecutionIntent(
                    ExecutionIntent intent,
                    double liveQuote,
                    out string reason)
                {
                    reason = "OK";
        
                    if (intent == null ||
                        (intent.Direction != 1 &&
                         intent.Direction != -1))
                    {
                        reason = "INVALID EXECUTION INTENT";
                        return false;
                    }
        
                    if (!IsExecutionPlanConsistent(
                            intent.Direction,
                            intent.RequestedEntry,
                            intent.Stop,
                            intent.Target))
                    {
                        reason = "INCONSISTENT EXECUTION INTENT";
                        return false;
                    }
        
                    if (!IsFinitePositive(intent.Volume) ||
                        intent.StopPips <= 0 ||
                        intent.TargetPips <= 0)
                    {
                        reason = "INCOMPLETE EXECUTION INTENT";
                        return false;
                    }
        
                    if (intent.Kind ==
                        ExecutionIntentKind.Market)
                    {
                        if (!IsFinitePositive(liveQuote))
                        {
                            reason = "INVALID MARKET QUOTE";
                            return false;
                        }
                    }
                    else if (intent.Kind ==
                                ExecutionIntentKind.Stop)
                    {
                        if (!SamePrice(
                                intent.RequestedEntry,
                                intent.Trigger) ||
                            !IsValidPendingEntry(
                                intent.Direction,
                                intent.RequestedEntry,
                                true))
                        {
                            reason = "INVALID STOP INTENT";
                            return false;
                        }
                    }
                    else if (intent.Kind ==
                                ExecutionIntentKind.Limit)
                    {
                        if (!IsValidPendingEntry(
                                intent.Direction,
                                intent.RequestedEntry,
                                false))
                        {
                            reason = "INVALID LIMIT INTENT";
                            return false;
                        }
                    }
        
                    return true;
                }
        
                private bool ValidateActualMarketFill(
                    ExecutionIntent intent,
                    double actualFill,
                    double atr,
                    out string reason)
                {
                    reason = "OK";
        
                    if (intent == null ||
                        !IsFinitePositive(actualFill) ||
                        atr <= 0)
                    {
                        reason = "INVALID ACTUAL FILL";
                        return false;
                    }
        
                    if (Math.Abs(
                            actualFill -
                            intent.RequestedEntry) >
                        atr *
                        Math.Max(
                            0.10,
                            MaximumEntryExtensionAtr))
                    {
                        reason = "BROKER FILL FAR FROM INTENT";
                        return false;
                    }
        
                    return true;
                }
        
            private sealed class Prediction
                {
                    public int Direction;
                    public ExecutionMode Mode;
                    public int Confidence;
                    public double Entry;
                    public double StopLoss;
                    public double ZoneLow;
                    public double ZoneHigh;
                    public double Trigger;
                    public double Target;
                    public double Target1;
                    public double Target2;
                    public double Target3;
                    public double Target4;
                    public string Reason;
                }
        
                private sealed class Decision
                {
                    public int Direction;
                    public int Confidence;
                    public int Edge;
                    public int SmartQuality;
                    public int TimeframeAgreement;
                    public int IndependentEvidence;
                    public int StructuralConfirmations;
                    public int RetestQuality;
                    public int BuyShare;
                    public int SellShare;
                    public string Regime;
                    public int RegimeQuality;
                    public bool TriggerReady;
                    public bool EntryAllowed;
                    public string BlockReason;
                    public string Reason;
                }
        
                private sealed class Plan
                {
                    public int Direction;
                    public ExecutionMode EntryMode;
                    public double Entry;
                    public double IdealEntry;
                    public double EntryZoneLow;
                    public double EntryZoneHigh;
                    public double EntryTrigger;
                    public double EntryInvalidation;
                    public int EntryQuality;
                    public string EntrySource;
                    public double Stop;
                    public double Tp1;
                    public double Tp2;
                    public double Tp3;
                    public double Tp4;
                    public double Risk;
                    public double Tp1RR;
                    public double Tp2RR;
                    public double Tp3RR;
                    public double Tp4RR;
                    public int StopQuality;
                    public int Tp1Quality;
                    public int Tp2Quality;
                    public int Tp3Quality;
                    public int Tp4Quality;
                    public string StopSource;
                    public string Tp1Source;
                    public string Tp2Source;
                    public string Tp3Source;
                    public string Tp4Source;
                    public int HtfTargetCount;
                    public int CreatedM5;
                    public double OriginalVolume;
                    public bool IsLivePosition;
                    public long PositionId;
                }
        
                private sealed class Native
                {
                    public Bars Bars;
                    public ExponentialMovingAverage Fast;
                    public ExponentialMovingAverage Slow;
                    public AverageTrueRange Atr;
                    public RelativeStrengthIndex Rsi;
                    public DirectionalMovementSystem Dms;
                    public ExponentialMovingAverage MacdFast;
                    public ExponentialMovingAverage MacdSlow;
                }
    }
}
