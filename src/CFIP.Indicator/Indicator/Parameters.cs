        [Parameter("Minimum Confidence", Group = "01 · Decision", DefaultValue = 72, MinValue = 50, MaxValue = 99)]
        public int MinimumConfidence { get; set; }

        [Parameter("Minimum Edge", Group = "01 · Decision", DefaultValue = 15, MinValue = 0, MaxValue = 50)]
        public int MinimumEdge { get; set; }

        [Parameter("Minimum Smart Quality", Group = "01 · Decision", DefaultValue = 70, MinValue = 40, MaxValue = 95)]
        public int MinimumSmartQuality { get; set; }

        [Parameter("Minimum Structural Confirmations", Group = "01 · Decision", DefaultValue = 4, MinValue = 1, MaxValue = 8)]
        public int MinimumStructuralConfirmations { get; set; }

        [Parameter("Minimum Independent Evidence", Group = "01 · Decision", DefaultValue = 4, MinValue = 2, MaxValue = 8)]
        public int MinimumIndependentEvidence { get; set; }

        [Parameter("Minimum Timeframe Agreement", Group = "01 · Decision", DefaultValue = 72, MinValue = 50, MaxValue = 100)]
        public int MinimumTimeframeAgreement { get; set; }

        [Parameter("Require Higher TF Agreement", Group = "01 · Decision", DefaultValue = true)]
        public bool RequireHigherTfAgreement { get; set; }

        [Parameter("Require Core Agreement", Group = "01 · Decision", DefaultValue = true)]
        public bool RequireCoreAgreement { get; set; }

        [Parameter("Require Structural Confirmation", Group = "01 · Decision", DefaultValue = true)]
        public bool RequireStructuralConfirmation { get; set; }

        [Parameter("Use Advanced Confluence", Group = "01 · Decision", DefaultValue = true)]
        public bool UseAdvancedConfluence { get; set; }

        [Parameter("Allow Strong Trigger Override", Group = "01 · Decision", DefaultValue = true)]
        public bool AllowStrongTriggerOverride { get; set; }

        [Parameter("M1 Trigger", Group = "02 · MTF", DefaultValue = false)]
        public bool UseM1Trigger { get; set; }

        [Parameter("M5 Confirmation", Group = "02 · MTF", DefaultValue = true)]
        public bool UseM5Confirmation { get; set; }

        [Parameter("M5 Weight", Group = "02 · MTF", DefaultValue = 7, MinValue = 1, MaxValue = 20)]
        public int M5Weight { get; set; }

        [Parameter("M15 Weight", Group = "02 · MTF", DefaultValue = 8, MinValue = 1, MaxValue = 20)]
        public int M15Weight { get; set; }

        [Parameter("M30 Weight", Group = "02 · MTF", DefaultValue = 5, MinValue = 0, MaxValue = 20)]
        public int M30Weight { get; set; }

        [Parameter("H1 Weight", Group = "02 · MTF", DefaultValue = 3, MinValue = 0, MaxValue = 20)]
        public int H1Weight { get; set; }

        [Parameter("H4 Weight", Group = "02 · MTF", DefaultValue = 2, MinValue = 0, MaxValue = 20)]
        public int H4Weight { get; set; }

        [Parameter("D1 Weight", Group = "02 · MTF", DefaultValue = 2, MinValue = 0, MaxValue = 20)]
        public int D1Weight { get; set; }

        [Parameter("W1 Weight", Group = "02 · MTF", DefaultValue = 3, MinValue = 0, MaxValue = 20)]
        public int W1Weight { get; set; }

        [Parameter("Structure Lookback", Group = "03 · Structure", DefaultValue = 40, MinValue = 15, MaxValue = 150)]
        public int StructureLookback { get; set; }

        [Parameter("Swing Strength", Group = "03 · Structure", DefaultValue = 2, MinValue = 1, MaxValue = 5)]
        public int SwingStrength { get; set; }

        [Parameter("Structure Break ATR", Group = "03 · Structure", DefaultValue = 0.05, MinValue = 0, MaxValue = 0.5)]
        public double StructureBreakAtr { get; set; }

        [Parameter("Use Internal Structure", Group = "03 · Structure", DefaultValue = true)]
        public bool UseInternalStructure { get; set; }

        [Parameter("Use MSS / CHOCH", Group = "03 · Structure", DefaultValue = true)]
        public bool UseMssChoch { get; set; }

        [Parameter("Use FVG", Group = "04 · Zones", DefaultValue = true)]
        public bool UseFvg { get; set; }

        [Parameter("FVG Lookback", Group = "04 · Zones", DefaultValue = 24, MinValue = 5, MaxValue = 80)]
        public int FvgLookback { get; set; }

        [Parameter("Minimum FVG ATR", Group = "04 · Zones", DefaultValue = 0.08, MinValue = 0.01, MaxValue = 1)]
        public double MinimumFvgAtr { get; set; }

        [Parameter("Use Order Block", Group = "04 · Zones", DefaultValue = true)]
        public bool UseOrderBlock { get; set; }

        [Parameter("OB Lookback", Group = "04 · Zones", DefaultValue = 30, MinValue = 5, MaxValue = 100)]
        public int ObLookback { get; set; }

        [Parameter("OB Displacement ATR", Group = "04 · Zones", DefaultValue = 0.60, MinValue = 0.1, MaxValue = 3)]
        public double ObDisplacementAtr { get; set; }

        [Parameter("OB Use Body For Zone", Group = "04 · Zones", DefaultValue = false)]
        public bool ObUseBodyForZone { get; set; }

        [Parameter("OB Break By Wicks", Group = "04 · Zones", DefaultValue = false)]
        public bool ObBreakByWicks { get; set; }

        [Parameter("OB Structure Lookback", Group = "04 · Zones", DefaultValue = 8, MinValue = 3, MaxValue = 40)]
        public int ObStructureLookback { get; set; }

        [Parameter("OB Impulse Bars", Group = "04 · Zones", DefaultValue = 4, MinValue = 2, MaxValue = 8)]
        public int ObImpulseBars { get; set; }

        [Parameter("OB Minimum Quality", Group = "04 · Zones", DefaultValue = 68, MinValue = 50, MaxValue = 100)]
        public int ObMinimumQuality { get; set; }

        [Parameter("Require FVG Retest", Group = "04 · Zones", DefaultValue = true)]
        public bool RequireFvgRetest { get; set; }

        [Parameter("Use 2-Bar Imbalance FVG", Group = "04 · Zones", DefaultValue = true)]
        public bool UseTwoBarImbalanceFvg { get; set; }

        [Parameter("Require OB Displacement", Group = "04 · Zones", DefaultValue = true)]
        public bool RequireObDisplacement { get; set; }

        [Parameter("Zone Proximity ATR", Group = "04 · Zones", DefaultValue = 0.25, MinValue = 0.05, MaxValue = 1)]
        public double ZoneProximityAtr { get; set; }

        [Parameter("Maximum Zone Age", Group = "04 · Zones", DefaultValue = 40, MinValue = 5, MaxValue = 200)]
        public int MaximumZoneAgeBars { get; set; }

        [Parameter("Liquidity Lookback", Group = "05 · Liquidity", DefaultValue = 40, MinValue = 10, MaxValue = 150)]
        public int LiquidityLookback { get; set; }

        [Parameter("Use Equal High / Low", Group = "05 · Liquidity", DefaultValue = true)]
        public bool UseEqualHighLow { get; set; }

        [Parameter("Equal Level Tolerance ATR", Group = "05 · Liquidity", DefaultValue = 0.12, MinValue = 0.02, MaxValue = 0.5)]
        public double EqualLevelToleranceAtr { get; set; }

        [Parameter("Use Liquidity Sweep", Group = "05 · Liquidity", DefaultValue = true)]
        public bool UseLiquiditySweep { get; set; }

        [Parameter("Liquidity Sweep Minimum Depth ATR", Group = "05 · Liquidity", DefaultValue = 0.05, MinValue = 0, MaxValue = 1.0, Step = 0.01)]
        public double LiquiditySweepMinimumDepthAtr { get; set; }

        [Parameter("Use Displacement", Group = "05 · Liquidity", DefaultValue = true)]
        public bool UseDisplacement { get; set; }

        [Parameter("Displacement ATR", Group = "05 · Liquidity", DefaultValue = 0.80, MinValue = 0.2, MaxValue = 3)]
        public double DisplacementAtr { get; set; }

        [Parameter("Use Premium / Discount", Group = "05 · Liquidity", DefaultValue = true)]
        public bool UsePremiumDiscount { get; set; }

        [Parameter("Use Daily Weekly Liquidity", Group = "05 · Liquidity", DefaultValue = true)]
        public bool UseDailyWeeklyLiquidity { get; set; }

        [Parameter("Use Daily Pivots", Group = "05 · Liquidity", DefaultValue = true)]
        public bool UseDailyPivots { get; set; }

        [Parameter("Daily Pivot Weight", Group = "05 · Liquidity", DefaultValue = 18, MinValue = 1, MaxValue = 50)]
        public int DailyPivotWeight { get; set; }

        [Parameter("Fast EMA", Group = "06 · Indicators", DefaultValue = 9, MinValue = 2, MaxValue = 50)]
        public int FastEma { get; set; }

        [Parameter("Slow EMA", Group = "06 · Indicators", DefaultValue = 21, MinValue = 3, MaxValue = 100)]
        public int SlowEma { get; set; }

        [Parameter("RSI Period", Group = "06 · Indicators", DefaultValue = 14, MinValue = 2, MaxValue = 50)]
        public int RsiPeriod { get; set; }

        [Parameter("ADX Period", Group = "06 · Indicators", DefaultValue = 14, MinValue = 3, MaxValue = 50)]
        public int AdxPeriod { get; set; }

        [Parameter("ADX Minimum", Group = "06 · Indicators", DefaultValue = 16, MinValue = 5, MaxValue = 50)]
        public double AdxMinimum { get; set; }

        [Parameter("ATR Period", Group = "06 · Indicators", DefaultValue = 14, MinValue = 3, MaxValue = 50)]
        public int AtrPeriod { get; set; }

        [Parameter("Use EMA Slope", Group = "06 · Indicators", DefaultValue = true)]
        public bool UseEmaSlope { get; set; }

        [Parameter("Avoid RSI Exhaustion", Group = "06 · Indicators", DefaultValue = true)]
        public bool AvoidRsiExhaustion { get; set; }

        [Parameter("Minimum Trigger Body ATR", Group = "07 · Entry Precision", DefaultValue = 0.12, MinValue = 0.02, MaxValue = 0.8)]
        public double MinimumTriggerBodyAtr { get; set; }

        [Parameter("Minimum Close Location", Group = "07 · Entry Precision", DefaultValue = 0.65, MinValue = 0.5, MaxValue = 0.95)]
        public double MinimumCloseLocation { get; set; }

        [Parameter("Maximum Trigger Range ATR", Group = "07 · Entry Precision", DefaultValue = 2.5, MinValue = 0.8, MaxValue = 5)]
        public double MaximumTriggerRangeAtr { get; set; }

        [Parameter("Require Stable M5 Direction", Group = "07 · Entry Precision", DefaultValue = true)]
        public bool RequireStableM5Direction { get; set; }

        [Parameter("Stable M5 Bars", Group = "07 · Entry Precision", DefaultValue = 2, MinValue = 1, MaxValue = 5)]
        public int StableM5Bars { get; set; }

        [Parameter("Require Stable M15 Direction", Group = "07 · Entry Precision", DefaultValue = true)]
        public bool RequireStableM15Direction { get; set; }

        [Parameter("Stable M15 Bars", Group = "07 · Entry Precision", DefaultValue = 2, MinValue = 1, MaxValue = 5)]
        public int StableM15Bars { get; set; }

        [Parameter("Entry Buffer ATR", Group = "07 · Entry Precision", DefaultValue = 0.05, MinValue = 0, MaxValue = 0.5)]
        public double EntryBufferAtr { get; set; }

        [Parameter("Maximum Entry Extension ATR", Group = "07 · Entry Precision", DefaultValue = 0.75, MinValue = 0.1, MaxValue = 2.5)]
        public double MaximumEntryExtensionAtr { get; set; }

        [Parameter("Minimum Retest Quality", Group = "07 · Entry Precision", DefaultValue = 76, MinValue = 40, MaxValue = 100)]
        public int MinimumRetestQuality { get; set; }

        [Parameter("Require Retest Quality", Group = "07 · Entry Precision", DefaultValue = true)]
        public bool RequireRetestQuality { get; set; }

        [Parameter("Supply / Demand Weight", Group = "08 · Smart Weights", DefaultValue = 125, MinValue = 50, MaxValue = 200)]
        public int SupplyDemandWeight { get; set; }

        [Parameter("FVG Weight", Group = "08 · Smart Weights", DefaultValue = 120, MinValue = 50, MaxValue = 200)]
        public int FvgWeight { get; set; }

        [Parameter("Order Block Weight", Group = "08 · Smart Weights", DefaultValue = 125, MinValue = 50, MaxValue = 200)]
        public int OrderBlockWeight { get; set; }

        [Parameter("Liquidity Pool Weight", Group = "08 · Smart Weights", DefaultValue = 120, MinValue = 50, MaxValue = 200)]
        public int LiquidityPoolWeight { get; set; }

        [Parameter("Equal High / Low Weight", Group = "08 · Smart Weights", DefaultValue = 115, MinValue = 50, MaxValue = 200)]
        public int EqualHighLowWeight { get; set; }

        [Parameter("Swing Structure Weight", Group = "08 · Smart Weights", DefaultValue = 105, MinValue = 50, MaxValue = 200)]
        public int SwingStructureWeight { get; set; }

        [Parameter("MTF Cluster Weight", Group = "08 · Smart Weights", DefaultValue = 130, MinValue = 50, MaxValue = 200)]
        public int MtfClusterWeight { get; set; }

        [Parameter("Previous Day Weight", Group = "08 · Smart Weights", DefaultValue = 115, MinValue = 50, MaxValue = 200)]
        public int PreviousDayWeight { get; set; }

        [Parameter("Previous Week Weight", Group = "08 · Smart Weights", DefaultValue = 130, MinValue = 50, MaxValue = 200)]
        public int PreviousWeekWeight { get; set; }

        [Parameter("Session Weight", Group = "08 · Smart Weights", DefaultValue = 110, MinValue = 50, MaxValue = 200)]
        public int SessionWeight { get; set; }

        [Parameter("HTF Structure Weight", Group = "08 · Smart Weights", DefaultValue = 118, MinValue = 50, MaxValue = 200)]
        public int HtfStructureWeight { get; set; }

        [Parameter("Minimum SL ATR", Group = "09 · Risk & Targets", DefaultValue = 0.55, MinValue = 0.1, MaxValue = 5)]
        public double MinimumSlAtr { get; set; }

        [Parameter("Maximum SL ATR", Group = "09 · Risk & Targets", DefaultValue = 1.80, MinValue = 0.5, MaxValue = 10)]
        public double MaximumSlAtr { get; set; }

        [Parameter("Fallback SL ATR Multiplier", Group = "09 · Risk & Targets", DefaultValue = 1.00, MinValue = 0.1, MaxValue = 5)]
        public double FallbackSlAtr { get; set; }

        [Parameter("TP1 Minimum RR", Group = "09 · Risk & Targets", DefaultValue = 2.00, MinValue = 0.5, MaxValue = 10)]
        public double Tp1MinimumRR { get; set; }

        [Parameter("TP2 Minimum RR", Group = "09 · Risk & Targets", DefaultValue = 3.20, MinValue = 0.8, MaxValue = 20)]
        public double Tp2MinimumRR { get; set; }

        [Parameter("TP3 Minimum RR", Group = "09 · Risk & Targets", DefaultValue = 4.80, MinValue = 1, MaxValue = 30)]
        public double Tp3MinimumRR { get; set; }

        [Parameter("TP4 Minimum RR", Group = "09 · Risk & Targets", DefaultValue = 6.50, MinValue = 1.5, MaxValue = 40)]
        public double Tp4MinimumRR { get; set; }

        [Parameter("Minimum TP Spacing ATR", Group = "09 · Risk & Targets", DefaultValue = 0.40, MinValue = 0.05, MaxValue = 3)]
        public double MinimumTpSpacingAtr { get; set; }

        [Parameter("Target Clearance ATR", Group = "09 · Risk & Targets", DefaultValue = 0.10, MinValue = 0.02, MaxValue = 1)]
        public double TargetClearanceAtr { get; set; }

        [Parameter("Reject Target Obstacle", Group = "09 · Risk & Targets", DefaultValue = true)]
        public bool RejectTargetObstacle { get; set; }

        [Parameter("Maximum Target Extension ATR", Group = "09 · Risk & Targets", DefaultValue = 4.0, MinValue = 1, MaxValue = 20)]
        public double MaximumTargetExtensionAtr { get; set; }

        [Parameter("Use HTF Structure For Stop", Group = "09 · Risk & Targets", DefaultValue = true)]
        public bool UseHtfStructureForStop { get; set; }

        [Parameter("Require Structural Stop", Group = "09 · Risk & Targets", DefaultValue = true)]
        public bool RequireStructuralStop { get; set; }

        [Parameter("Adaptive Structural RR", Group = "09 · Risk & Targets", DefaultValue = true)]
        public bool AdaptiveStructuralRR { get; set; }

        [Parameter("Enable Live Exit Management", Group = "10 · Live Management", DefaultValue = true)]
        public bool EnableLiveExitManagement { get; set; }

        [Parameter("Move SL To Break Even", Group = "10 · Live Management", DefaultValue = true)]
        public bool MoveSlToBreakEven { get; set; }

        [Parameter("Break Even Trigger RR", Group = "10 · Live Management", DefaultValue = 0.90, MinValue = 0.2, MaxValue = 10)]
        public double BreakEvenTriggerRR { get; set; }

        [Parameter("Break Even Buffer Pips", Group = "10 · Live Management", DefaultValue = 0.5, MinValue = 0, MaxValue = 20)]
        public double BreakEvenBufferPips { get; set; }

        [Parameter("Use Spread Aware Break Even", Group = "10 · Live Management", DefaultValue = true)]
        public bool UseSpreadAwareBreakEven { get; set; }

        [Parameter("Risk Free Lock Pips", Group = "10 · Live Management", DefaultValue = 0.5, MinValue = 0, MaxValue = 10)]
        public double RiskFreeLockPips { get; set; }

        [Parameter("Smart Trail Momentum Bonus ATR", Group = "10 · Live Management", DefaultValue = 0.10, MinValue = 0, MaxValue = 0.50, Step = 0.01)]
        public double SmartTrailMomentumBonusAtr { get; set; }

        [Parameter("Smart Trail Tighten At RR", Group = "10 · Live Management", DefaultValue = 1.80, MinValue = 0.5, MaxValue = 10)]
        public double SmartTrailTightenAtRR { get; set; }

        [Parameter("Enable Structural SL Repricing", Group = "10 · Live Management", DefaultValue = true)]
        public bool EnableStructuralSlRepricing { get; set; }

        [Parameter("SL Reprice Start RR", Group = "10 · Live Management", DefaultValue = 1.00, MinValue = 0.5, MaxValue = 10)]
        public double SlRepriceStartRR { get; set; }

        [Parameter("SL Reprice Breathing ATR", Group = "10 · Live Management", DefaultValue = 0.85, MinValue = 0.2, MaxValue = 5)]
        public double SlRepriceBreathingAtr { get; set; }

        [Parameter("SL Reprice Step ATR", Group = "10 · Live Management", DefaultValue = 0.08, MinValue = 0.01, MaxValue = 1)]
        public double SlRepriceStepAtr { get; set; }

        [Parameter("Update Unhit Targets", Group = "10 · Live Management", DefaultValue = true)]
        public bool UpdateUnhitTargets { get; set; }

        [Parameter("Target Update Trigger RR", Group = "10 · Live Management", DefaultValue = 1.20, MinValue = 0.5, MaxValue = 10)]
        public double TargetUpdateTriggerRR { get; set; }

        [Parameter("Enable Profit Exhaustion Protection", Group = "10 · Live Management", DefaultValue = true)]
        public bool EnableProfitExhaustionProtection { get; set; }

        [Parameter("Exhaustion Minimum Peak RR", Group = "10 · Live Management", DefaultValue = 1.50, MinValue = 0.8, MaxValue = 10)]
        public double ExhaustionMinimumPeakRR { get; set; }

        [Parameter("Exhaustion Retracement Percent", Group = "10 · Live Management", DefaultValue = 35, MinValue = 15, MaxValue = 80)]
        public double ExhaustionRetracementPercent { get; set; }

        [Parameter("Exhaustion Pressure Threshold", Group = "10 · Live Management", DefaultValue = 78, MinValue = 55, MaxValue = 100)]
        public int ExhaustionPressureThreshold { get; set; }

        [Parameter("Exhaustion Minimum Opposite Evidence", Group = "10 · Live Management", DefaultValue = 3, MinValue = 1, MaxValue = 8)]
        public int ExhaustionMinimumOppositeEvidence { get; set; }

        [Parameter("Use Session Filter", Group = "11 · Filters", DefaultValue = false)]
        public bool UseSessionFilter { get; set; }

        [Parameter("Session Start UTC", Group = "11 · Filters", DefaultValue = 6, MinValue = 0, MaxValue = 23)]
        public int SessionStartUtc { get; set; }

        [Parameter("Session End UTC", Group = "11 · Filters", DefaultValue = 20, MinValue = 0, MaxValue = 23)]
        public int SessionEndUtc { get; set; }

        [Parameter("Enable End Of Day Alert", Group = "11 · Filters", DefaultValue = true)]
        public bool EnableEndOfDayAlert { get; set; }

        [Parameter("End Of Day Alert Minutes Before", Group = "11 · Filters", DefaultValue = 30, MinValue = 5, MaxValue = 180)]
        public int EndOfDayAlertMinutesBefore { get; set; }

        [Parameter("Enable End Of Day Auto Close", Group = "11 · Filters", DefaultValue = true)]
        public bool EnableEndOfDayAutoClose { get; set; }

        [Parameter("Use Spread Filter", Group = "11 · Filters", DefaultValue = true)]
        public bool UseSpreadFilter { get; set; }

        [Parameter("Maximum Spread ATR", Group = "11 · Filters", DefaultValue = 0.15, MinValue = 0.01, MaxValue = 1)]
        public double MaximumSpreadAtr { get; set; }

        [Parameter("Cooldown M5 Bars", Group = "11 · Filters", DefaultValue = 3, MinValue = 0, MaxValue = 50)]
        public int CooldownM5Bars { get; set; }

        [Parameter("Avoid Friday Late Entry", Group = "11 · Filters", DefaultValue = false)]
        public bool AvoidFridayLateEntry { get; set; }

        [Parameter("Friday Cutoff UTC", Group = "11 · Filters", DefaultValue = 18, MinValue = 0, MaxValue = 23)]
        public int FridayCutoffUtc { get; set; }

        [Parameter("Use Volatility Guard", Group = "11 · Filters", DefaultValue = true)]
        public bool UseVolatilityGuard { get; set; }

        [Parameter("Event Shock Range ATR", Group = "11 · Filters", DefaultValue = 2.20, MinValue = 1.2, MaxValue = 5)]
        public double EventShockRangeAtr { get; set; }

        [Parameter("Event Shock ATR Expansion", Group = "11 · Filters", DefaultValue = 1.55, MinValue = 1.1, MaxValue = 3)]
        public double EventShockAtrExpansion { get; set; }

        [Parameter("News Blackout UTC", Group = "11 · Filters", DefaultValue = "")]
        public string NewsBlackoutUtc { get; set; }

        [Parameter("Enable Sound Alerts", Group = "12 · ALERTS — CORE", DefaultValue = true)]
        public bool EnableSoundAlerts { get; set; }

        [Parameter("Show Popup Alerts", Group = "12 · ALERTS — CORE", DefaultValue = false)]
        public bool ShowPopupAlerts { get; set; }

        [Parameter("Popup Critical Only", Group = "12 · ALERTS — CORE", DefaultValue = true)]
        public bool PopupCriticalOnly { get; set; }

        [Parameter("Popup Duration Seconds", Group = "12 · ALERTS — CORE", DefaultValue = 6, MinValue = 1, MaxValue = 60)]
        public int PopupDurationSeconds { get; set; }

        [Parameter("Popup Font Size", Group = "12 · ALERTS — CORE", DefaultValue = 11, MinValue = 8, MaxValue = 22)]
        public int PopupFontSize { get; set; }

        [Parameter("Show Entry Restriction Popup", Group = "12 · ALERTS — CORE", DefaultValue = true)]
        public bool ShowEntryRestrictionPopup { get; set; }

        [Parameter("Alert On News / Event Guard", Group = "12 · ALERTS — CORE", DefaultValue = true)]
        public bool AlertOnNewsEventGuard { get; set; }

        [Parameter("Popup Margin", Group = "12 · ALERTS — CORE", DefaultValue = 10, MinValue = 0, MaxValue = 50)]
        public int PopupMargin { get; set; }

        [Parameter("Popup Border Alpha", Group = "12 · ALERTS — CORE", DefaultValue = 235, MinValue = 0, MaxValue = 255)]
        public int PopupBorderAlpha { get; set; }

        [Parameter("Popup Bold", Group = "12 · ALERTS — CORE", DefaultValue = false)]
        public bool PopupBold { get; set; }

        [Parameter("Popup Font Family", Group = "12 · ALERTS — CORE", DefaultValue = "Arial")]
        public string PopupFontFamily { get; set; }

        [Parameter("Alert On Confirmed Signal", Group = "12 · ALERTS — CORE", DefaultValue = true)]
        public bool AlertOnConfirmedSignal { get; set; }

        [Parameter("Alert On Reaction", Group = "12 · ALERTS — CORE", DefaultValue = true)]
        public bool AlertOnReaction { get; set; }

        [Parameter("Alert On Early Watch", Group = "12 · ALERTS — CORE", DefaultValue = true)]
        public bool AlertOnEarlyWatch { get; set; }

        [Parameter("Alert On Level Hit", Group = "12 · ALERTS — CORE", DefaultValue = true)]
        public bool AlertOnLevelHit { get; set; }

        [Parameter("Alert On Invalidated", Group = "12 · ALERTS — CORE", DefaultValue = true)]
        public bool AlertOnInvalidated { get; set; }

        [Parameter("Alert Cooldown Seconds", Group = "12 · ALERTS — CORE", DefaultValue = 8, MinValue = 1, MaxValue = 60)]
        public int AlertCooldownSeconds { get; set; }

        [Parameter("Suppress Duplicate Alerts", Group = "12 · ALERTS — CORE", DefaultValue = true)]
        public bool SuppressDuplicateAlerts { get; set; }

        [Parameter("Enable Email Alerts", Group = "12 · ALERTS — CORE", DefaultValue = false)]
        public bool EnableEmailAlerts { get; set; }

        [Parameter("Sender Email", Group = "12 · ALERTS — CORE", DefaultValue = "")]
        public string SenderEmail { get; set; }

        [Parameter("Receiver Email", Group = "12 · ALERTS — CORE", DefaultValue = "")]
        public string ReceiverEmail { get; set; }

        [Parameter("Enable Auto Trading", Group = "13 · AUTO TRADING", DefaultValue = false)]
        public bool EnableAutoTrading { get; set; }

        [Parameter("Enable Automatic Orders", Group = "13 · AUTO TRADING", DefaultValue = false)]
        public bool EnableAutomaticOrders { get; set; }

        [Parameter("Pending Order Mode", Group = "13 · AUTO TRADING", DefaultValue = PendingOrderMode.Adaptive)]
        public PendingOrderMode PendingOrderMode { get; set; }

        [Parameter("Pending Order Expiry Minutes", Group = "13 · AUTO TRADING", DefaultValue = 120, MinValue = 15, MaxValue = 1440)]
        public int PendingOrderExpiryMinutes { get; set; }

        [Parameter("Pending Entry Buffer ATR", Group = "13 · AUTO TRADING", DefaultValue = 0.10, MinValue = 0.02, MaxValue = 1.00, Step = 0.01)]
        public double PendingEntryBufferAtr { get; set; }

        [Parameter("Pending Minimum Confidence", Group = "13 · AUTO TRADING", DefaultValue = 84, MinValue = 50, MaxValue = 99)]
        public int PendingMinimumConfidence { get; set; }

        [Parameter("Pending Minimum Smart Quality", Group = "13 · AUTO TRADING", DefaultValue = 78, MinValue = 50, MaxValue = 95)]
        public int PendingMinimumSmartQuality { get; set; }

        [Parameter("Pending Minimum Trend Quality", Group = "13 · AUTO TRADING", DefaultValue = 72, MinValue = 40, MaxValue = 100)]
        public int PendingMinimumTrendQuality { get; set; }

        [Parameter("Pending Auto Cleanup", Group = "13 · AUTO TRADING", DefaultValue = true)]
        public bool PendingAutoCleanup { get; set; }

        [Parameter("Auto Trading Reminder", Group = "13 · AUTO TRADING", DefaultValue = true)]
        public bool AutoTradingReminder { get; set; }

        [Parameter("Reversal Close Minimum Evidence", Group = "13 · AUTO TRADING", DefaultValue = 5, MinValue = 2, MaxValue = 10)]
        public int ReversalCloseMinimumEvidence { get; set; }

        [Parameter("Reversal Close Minimum MTF", Group = "13 · AUTO TRADING", DefaultValue = 70, MinValue = 50, MaxValue = 100)]
        public int ReversalCloseMinimumMtf { get; set; }

        [Parameter("Confirmed Signals Only", Group = "13 · AUTO TRADING", DefaultValue = true)]
        public bool ConfirmedSignalsOnly { get; set; }

        [Parameter("Sizing Mode", Group = "13 · AUTO TRADING", DefaultValue = SizingMode.RiskPercentEquity)]
        public SizingMode SizingMode { get; set; }

        [Parameter("Risk % Equity", Group = "13 · AUTO TRADING", DefaultValue = 0.50, MinValue = 0.05, MaxValue = 5)]
        public double RiskPercentEquity { get; set; }

        [Parameter("Fixed Lots", Group = "13 · AUTO TRADING", DefaultValue = 0.01, MinValue = 0.001, MaxValue = 100, Step = 0.001)]
        public double FixedLots { get; set; }

        [Parameter("Minimum Auto Confidence", Group = "13 · AUTO TRADING", DefaultValue = 86, MinValue = 50, MaxValue = 99)]
        public int MinimumAutoConfidence { get; set; }

        [Parameter("Minimum Auto Smart Quality", Group = "13 · AUTO TRADING", DefaultValue = 80, MinValue = 50, MaxValue = 95)]
        public int MinimumAutoSmartQuality { get; set; }

        [Parameter("Minimum Auto Level Quality", Group = "13 · AUTO TRADING", DefaultValue = 72, MinValue = 40, MaxValue = 100)]
        public int MinimumAutoLevelQuality { get; set; }

        [Parameter("Auto TP Stage", Group = "13 · AUTO TRADING", DefaultValue = TargetStage.TP1)]
        public TargetStage AutoTpStage { get; set; }

        [Parameter("Enable Dynamic TP Advance", Group = "13 · AUTO TRADING", DefaultValue = true)]
        public bool EnableDynamicTpAdvance { get; set; }

        [Parameter("TP Advance Proximity Percent", Group = "13 · AUTO TRADING", DefaultValue = 72, MinValue = 50, MaxValue = 98)]
        public double TpAdvanceProximityPercent { get; set; }

        [Parameter("Enable Partial Take Profit", Group = "13 · AUTO TRADING", DefaultValue = false)]
        public bool EnablePartialTakeProfit { get; set; }

        [Parameter("Partial Close At TP1 Percent", Group = "13 · AUTO TRADING", DefaultValue = 33, MinValue = 0, MaxValue = 90, Step = 1)]
        public double PartialCloseTp1Percent { get; set; }

        [Parameter("Partial Close At TP2 Percent", Group = "13 · AUTO TRADING", DefaultValue = 33, MinValue = 0, MaxValue = 90, Step = 1)]
        public double PartialCloseTp2Percent { get; set; }

        [Parameter("Move To Break Even After Partial", Group = "13 · AUTO TRADING", DefaultValue = true)]
        public bool MoveToBreakEvenAfterPartial { get; set; }

        [Parameter("Enable Reversal Protection Close", Group = "13 · AUTO TRADING", DefaultValue = true)]
        public bool EnableReversalProtectionClose { get; set; }

        [Parameter("Reversal Protection Minimum Quality", Group = "13 · AUTO TRADING", DefaultValue = 82, MinValue = 50, MaxValue = 100)]
        public int ReversalProtectionMinimumQuality { get; set; }

        [Parameter("Maximum Open Positions", Group = "13 · AUTO TRADING", DefaultValue = 1, MinValue = 1, MaxValue = 20)]
        public int MaximumOpenPositions { get; set; }

        [Parameter("Enable Daily Loss Limit", Group = "13 · AUTO TRADING", DefaultValue = true)]
        public bool EnableDailyLossLimit { get; set; }

        [Parameter("Maximum Daily Loss Percent", Group = "13 · AUTO TRADING", DefaultValue = 3.0, MinValue = 0.5, MaxValue = 20, Step = 0.5)]
        public double MaximumDailyLossPercent { get; set; }

        [Parameter("Use Market Hours Guard", Group = "13 · AUTO TRADING", DefaultValue = true)]
        public bool UseMarketHoursGuard { get; set; }

        [Parameter("Use Auto Margin Guard", Group = "13 · AUTO TRADING", DefaultValue = true)]
        public bool UseAutoMarginGuard { get; set; }

        [Parameter("Max Auto Margin Usage %", Group = "13 · AUTO TRADING", DefaultValue = 80, MinValue = 10, MaxValue = 100)]
        public double MaxAutoMarginUsagePercent { get; set; }

        [Parameter("Margin Buffer %", Group = "13 · AUTO TRADING", DefaultValue = 10, MinValue = 0, MaxValue = 40)]
        public double MarginBufferPercent { get; set; }

        [Parameter("Auto Trade Label", Group = "13 · AUTO TRADING", DefaultValue = "CFIP-SMART66")]
        public string AutoTradeLabel { get; set; }

        [Parameter("Smart Broker Protection", Group = "13 · AUTO TRADING", DefaultValue = true)]
        public bool AutoBrokerProtection { get; set; }

        [Parameter("One Order Per Signal", Group = "13 · AUTO TRADING", DefaultValue = true)]
        public bool OneOrderPerSignal { get; set; }

        [Parameter("Enable Market Suitability Guard", Group = "24 · SMART EXECUTION", DefaultValue = true)]
        public bool EnableMarketSuitabilityGuard { get; set; }

        [Parameter("Minimum Market Suitability", Group = "24 · SMART EXECUTION", DefaultValue = 68, MinValue = 50, MaxValue = 95)]
        public int MinimumMarketSuitability { get; set; }

        [Parameter("Hard Market Suitability Gate", Group = "24 · SMART EXECUTION", DefaultValue = true)]
        public bool HardMarketSuitabilityGate { get; set; }

        [Parameter("Require Session Suitability", Group = "24 · SMART EXECUTION", DefaultValue = false)]
        public bool RequireSessionSuitability { get; set; }

        [Parameter("Use Smart Risk Scaling", Group = "24 · SMART EXECUTION", DefaultValue = true)]
        public bool UseSmartRiskScaling { get; set; }

        [Parameter("Minimum Smart Risk Multiplier", Group = "24 · SMART EXECUTION", DefaultValue = 0.55, MinValue = 0.25, MaxValue = 1.0, Step = 0.05)]
        public double MinimumSmartRiskMultiplier { get; set; }

        [Parameter("Full Risk Confidence Threshold", Group = "24 · SMART EXECUTION", DefaultValue = 94, MinValue = 70, MaxValue = 99)]
        public int FullRiskConfidenceThreshold { get; set; }

        [Parameter("Full Risk Suitability Threshold", Group = "24 · SMART EXECUTION", DefaultValue = 88, MinValue = 60, MaxValue = 100)]
        public int FullRiskSuitabilityThreshold { get; set; }

        [Parameter("Penalize Choppy Regime Risk", Group = "24 · SMART EXECUTION", DefaultValue = true)]
        public bool PenalizeChoppyRegimeRisk { get; set; }

        [Parameter("Suitability Recalculation Seconds", Group = "24 · SMART EXECUTION", DefaultValue = 2, MinValue = 1, MaxValue = 10)]
        public int SuitabilityRecalculationSeconds { get; set; }

        [Parameter("Show Level Lines", Group = "14 · DISPLAY — CORE", DefaultValue = true)]
        public bool ShowLevelLines { get; set; }

        [Parameter("Full Width Level Lines", Group = "14 · DISPLAY — CORE", DefaultValue = true)]
        public bool FullWidthLevelLines { get; set; }

        [Parameter("Level Line Thickness", Group = "14 · DISPLAY — CORE", DefaultValue = 1, MinValue = 1, MaxValue = 3)]
        public int LevelLineThickness { get; set; }

        [Parameter("Show Entry", Group = "14 · DISPLAY — CORE", DefaultValue = true)]
        public bool ShowEntry { get; set; }

        [Parameter("Show Trigger", Group = "14 · DISPLAY — CORE", DefaultValue = true)]
        public bool ShowTrigger { get; set; }

        [Parameter("Show SL", Group = "14 · DISPLAY — CORE", DefaultValue = true)]
        public bool ShowSL { get; set; }

        [Parameter("Show TP1", Group = "14 · DISPLAY — CORE", DefaultValue = true)]
        public bool ShowTP1 { get; set; }

        [Parameter("Show TP2", Group = "14 · DISPLAY — CORE", DefaultValue = true)]
        public bool ShowTP2 { get; set; }

        [Parameter("Show TP3", Group = "14 · DISPLAY — CORE", DefaultValue = true)]
        public bool ShowTP3 { get; set; }

        [Parameter("Show TP4", Group = "14 · DISPLAY — CORE", DefaultValue = true)]
        public bool ShowTP4 { get; set; }

        [Parameter("Show Signal Arrow", Group = "14 · DISPLAY — CORE", DefaultValue = true)]
        public bool ShowSignalArrow { get; set; }

        [Parameter("Show Early Watch", Group = "14 · DISPLAY — CORE", DefaultValue = true)]
        public bool ShowEarlyWatch { get; set; }

        [Parameter("Show Historical Signals", Group = "14 · DISPLAY — CORE", DefaultValue = false)]
        public bool ShowHistoricalSignals { get; set; }

        [Parameter("Historical Signal Limit", Group = "14 · DISPLAY — CORE", DefaultValue = 10, MinValue = 1, MaxValue = 50)]
        public int HistoricalSignalLimit { get; set; }

        [Parameter("Show Unified Panel", Group = "14 · DISPLAY — CORE", DefaultValue = true)]
        public bool ShowUnifiedPanel { get; set; }

        [Parameter("Show Panel Background", Group = "14 · DISPLAY — CORE", DefaultValue = true)]
        public bool ShowPanelBackground { get; set; }

        [Parameter("Panel Position", Group = "14 · DISPLAY — CORE", DefaultValue = PanelCorner.BottomLeft)]
        public PanelCorner PanelPosition { get; set; }

        [Parameter("Panel Width", Group = "14 · DISPLAY — CORE", DefaultValue = 430, MinValue = 220, MaxValue = 700)]
        public int PanelWidth { get; set; }

        [Parameter("Panel Font Size", Group = "14 · DISPLAY — CORE", DefaultValue = 11, MinValue = 8, MaxValue = 20)]
        public int PanelFontSize { get; set; }

        [Parameter("Panel Font Family", Group = "14 · DISPLAY — CORE", DefaultValue = "Arial")]
        public string PanelFontFamily { get; set; }

        [Parameter("Panel Bold", Group = "14 · DISPLAY — CORE", DefaultValue = false)]
        public bool PanelBold { get; set; }

        [Parameter("Panel Background", Group = "14 · DISPLAY — CORE", DefaultValue = "Black")]
        public Color PanelBackground { get; set; }

        [Parameter("Panel Background Alpha", Group = "14 · DISPLAY — CORE", DefaultValue = 145, MinValue = 0, MaxValue = 255)]
        public int PanelBackgroundAlpha { get; set; }

        [Parameter("Panel Border", Group = "14 · DISPLAY — CORE", DefaultValue = "#3A4656")]
        public Color PanelBorder { get; set; }

        [Parameter("Panel Border Alpha", Group = "14 · DISPLAY — CORE", DefaultValue = 230, MinValue = 0, MaxValue = 255)]
        public int PanelBorderAlpha { get; set; }

        [Parameter("Panel Border Thickness", Group = "14 · DISPLAY — CORE", DefaultValue = 1, MinValue = 0, MaxValue = 4)]
        public int PanelBorderThickness { get; set; }

        [Parameter("Panel Corner Radius", Group = "14 · DISPLAY — CORE", DefaultValue = 5, MinValue = 0, MaxValue = 20)]
        public int PanelCornerRadius { get; set; }

        [Parameter("Panel Padding", Group = "14 · DISPLAY — CORE", DefaultValue = 9, MinValue = 0, MaxValue = 30)]
        public int PanelPadding { get; set; }

        [Parameter("Panel Margin", Group = "14 · DISPLAY — CORE", DefaultValue = 8, MinValue = 0, MaxValue = 30)]
        public int PanelMargin { get; set; }

        [Parameter("Panel Row Gap", Group = "14 · DISPLAY — CORE", DefaultValue = 1, MinValue = 0, MaxValue = 6)]
        public int PanelRowGap { get; set; }

        [Parameter("Panel Max Height", Group = "14 · DISPLAY — CORE", DefaultValue = 650, MinValue = 260, MaxValue = 1200)]
        public int PanelMaxHeight { get; set; }

        [Parameter("Panel Row Padding", Group = "14 · DISPLAY — CORE", DefaultValue = 3, MinValue = 0, MaxValue = 12)]
        public int PanelRowPadding { get; set; }

        [Parameter("Panel Button Gap", Group = "14 · DISPLAY — CORE", DefaultValue = 4, MinValue = 0, MaxValue = 16)]
        public int PanelButtonGap { get; set; }

        [Parameter("Panel Accent Color", Group = "14 · DISPLAY — CORE", DefaultValue = "#4A90E2")]
        public Color PanelAccentColor { get; set; }

        [Parameter("Panel Section Color", Group = "14 · DISPLAY — CORE", DefaultValue = "#8FA3B8")]
        public Color PanelSectionColor { get; set; }

        [Parameter("Panel Secondary Text Color", Group = "14 · DISPLAY — CORE", DefaultValue = "#C5CBD3")]
        public Color PanelSecondaryTextColor { get; set; }

        [Parameter("Panel Muted Text Color", Group = "14 · DISPLAY — CORE", DefaultValue = "#8A95A5")]
        public Color PanelMutedTextColor { get; set; }

        [Parameter("Panel Warning Color", Group = "14 · DISPLAY — CORE", DefaultValue = "Orange")]
        public Color PanelWarningColor { get; set; }

        [Parameter("Panel Text Color", Group = "14 · DISPLAY — CORE", DefaultValue = "White")]
        public Color PanelTextColor { get; set; }

        [Parameter("Entry Line Color", Group = "14 · DISPLAY — CORE", DefaultValue = "White")]
        public Color EntryLineColor { get; set; }

        [Parameter("Trigger Line Color", Group = "14 · DISPLAY — CORE", DefaultValue = "Orange")]
        public Color TriggerLineColor { get; set; }

        [Parameter("SL Line Color", Group = "14 · DISPLAY — CORE", DefaultValue = "Red")]
        public Color SlLineColor { get; set; }

        [Parameter("TP1 Line Color", Group = "14 · DISPLAY — CORE", DefaultValue = "Lime")]
        public Color TpLineColor { get; set; }

        [Parameter("TP2 Line Color", Group = "14 · DISPLAY — CORE", DefaultValue = "SpringGreen")]
        public Color Tp2LineColor { get; set; }

        [Parameter("TP3 Line Color", Group = "14 · DISPLAY — CORE", DefaultValue = "Turquoise")]
        public Color Tp3LineColor { get; set; }

        [Parameter("TP4 Line Color", Group = "14 · DISPLAY — CORE", DefaultValue = "Gold")]
        public Color Tp4LineColor { get; set; }

        [Parameter("BUY Arrow Color", Group = "14 · DISPLAY — CORE", DefaultValue = "Lime")]
        public Color BuyArrowColor { get; set; }

        [Parameter("SELL Arrow Color", Group = "14 · DISPLAY — CORE", DefaultValue = "Red")]
        public Color SellArrowColor { get; set; }

        [Parameter("Live Trigger Score", Group = "15 · CONTROL — ADVANCED", DefaultValue = 4, MinValue = 1, MaxValue = 6)]
        public int LiveTriggerScore { get; set; }

        [Parameter("Precision Trigger Score", Group = "15 · CONTROL — ADVANCED", DefaultValue = 5, MinValue = 2, MaxValue = 6)]
        public int PrecisionTriggerScore { get; set; }

        [Parameter("Allow Strong M5 Trigger Override", Group = "15 · CONTROL — ADVANCED", DefaultValue = true)]
        public bool AllowStrongM5TriggerOverride { get; set; }

        [Parameter("M5 Only Confirmed Trigger", Group = "15 · CONTROL — ADVANCED", DefaultValue = true)]
        public bool M5OnlyConfirmedTrigger { get; set; }

        [Parameter("Allow M15 Neutral Pullback", Group = "15 · CONTROL — ADVANCED", DefaultValue = true)]
        public bool AllowM15NeutralPullback { get; set; }

        [Parameter("Higher TF Penalty", Group = "15 · CONTROL — ADVANCED", DefaultValue = 7, MinValue = 0, MaxValue = 20)]
        public int HigherTfPenalty { get; set; }

        [Parameter("Use Zone Confluence", Group = "15 · CONTROL — ADVANCED", DefaultValue = true)]
        public bool UseZoneConfluence { get; set; }

        [Parameter("Use Higher TF Liquidity Targets", Group = "15 · CONTROL — ADVANCED", DefaultValue = true)]
        public bool UseHigherTfLiquidityTargets { get; set; }

        [Parameter("Minimum HTF Target RR", Group = "15 · CONTROL — ADVANCED", DefaultValue = 2.50, MinValue = 1, MaxValue = 20)]
        public double MinimumHtfTargetRR { get; set; }

        [Parameter("Structural TP RR Step", Group = "15 · CONTROL — ADVANCED", DefaultValue = 0.50, MinValue = 0.10, MaxValue = 2.0, Step = 0.05)]
        public double StructuralTpRrStep { get; set; }

        [Parameter("Minimum Trade RR", Group = "15 · CONTROL — ADVANCED", DefaultValue = 2.00, MinValue = 0.5, MaxValue = 20)]
        public double MinimumTradeRR { get; set; }

        [Parameter("Use RR Filter", Group = "15 · CONTROL — ADVANCED", DefaultValue = true)]
        public bool UseRRFilter { get; set; }

        [Parameter("Avoid Late Entry", Group = "15 · CONTROL — ADVANCED", DefaultValue = true)]
        public bool AvoidLateEntry { get; set; }

        [Parameter("Use Precision Execution Model", Group = "15 · CONTROL — ADVANCED", DefaultValue = true)]
        public bool UsePrecisionExecutionModel { get; set; }

        [Parameter("Stop Buffer ATR", Group = "15 · CONTROL — ADVANCED", DefaultValue = 0.10, MinValue = 0.01, MaxValue = 1.0)]
        public double StopBufferAtr { get; set; }

        [Parameter("Require HTF Targets", Group = "15 · CONTROL — ADVANCED", DefaultValue = false)]
        public bool RequireHtfTargets { get; set; }

        [Parameter("Maximum Structural Stop ATR", Group = "15 · CONTROL — ADVANCED", DefaultValue = 2.25, MinValue = 0.5, MaxValue = 10)]
        public double MaximumStructuralStopAtr { get; set; }

        [Parameter("Target Obstacle Lookback Bars", Group = "15 · CONTROL — ADVANCED", DefaultValue = 8, MinValue = 3, MaxValue = 50)]
        public int TargetObstacleLookbackBars { get; set; }

        [Parameter("Allow Direct Displacement Override", Group = "15 · CONTROL — ADVANCED", DefaultValue = true)]
        public bool AllowDirectDisplacementOverride { get; set; }

        [Parameter("Direct Displacement Override Score", Group = "15 · CONTROL — ADVANCED", DefaultValue = 6, MinValue = 3, MaxValue = 6)]
        public int DirectDisplacementOverrideScore { get; set; }

        [Parameter("Maximum Setup Age Bars", Group = "15 · CONTROL — ADVANCED", DefaultValue = 8, MinValue = 1, MaxValue = 50)]
        public int MaximumSetupAgeBars { get; set; }

        [Parameter("Allow Synthetic Target Fallback", Group = "15 · CONTROL — ADVANCED", DefaultValue = true)]
        public bool AllowSyntheticTargetFallback { get; set; }

        [Parameter("HTF Stop Buffer ATR", Group = "15 · CONTROL — ADVANCED", DefaultValue = 0.15, MinValue = 0.01, MaxValue = 1.0)]
        public double HtfStopBufferAtr { get; set; }

        [Parameter("Block New Signal While Active", Group = "15 · CONTROL — ADVANCED", DefaultValue = true)]
        public bool BlockNewSignalWhileActive { get; set; }

        [Parameter("Cooldown Bars", Group = "15 · CONTROL — ADVANCED", DefaultValue = 3, MinValue = 0, MaxValue = 50)]
        public int CooldownBars { get; set; }

        [Parameter("Use News Event Guard", Group = "15 · CONTROL — ADVANCED", DefaultValue = true)]
        public bool UseNewsEventGuard { get; set; }

        [Parameter("Use Volatility Event Guard", Group = "15 · CONTROL — ADVANCED", DefaultValue = true)]
        public bool UseVolatilityEventGuard { get; set; }

        [Parameter("Event Guard Cooldown Bars", Group = "15 · CONTROL — ADVANCED", DefaultValue = 3, MinValue = 0, MaxValue = 50)]
        public int EventGuardCooldownBars { get; set; }

        [Parameter("Use Regime No-Trade Guard", Group = "15 · CONTROL — ADVANCED", DefaultValue = true)]
        public bool UseRegimeNoTradeGuard { get; set; }

        [Parameter("Show Reaction Arrow", Group = "15 · CONTROL — ADVANCED", DefaultValue = true)]
        public bool ShowReactionArrow { get; set; }

        [Parameter("Show Historical Arrows", Group = "15 · CONTROL — ADVANCED", DefaultValue = true)]
        public bool ShowHistoricalArrows { get; set; }

        [Parameter("Enable Dynamic Structural Stop Alias", Group = "15 · CONTROL — ADVANCED", DefaultValue = true)]
        public bool EnableDynamicSlTrail { get; set; }

        [Parameter("Structural Stop Breathing ATR", Group = "15 · CONTROL — ADVANCED", DefaultValue = 0.85, MinValue = 0.20, MaxValue = 5)]
        public double TrailDistanceAtr { get; set; }

        [Parameter("Structural Stop Step ATR", Group = "15 · CONTROL — ADVANCED", DefaultValue = 0.08, MinValue = 0.01, MaxValue = 1)]
        public double TrailStepAtr { get; set; }

        [Parameter("Target Update Step ATR", Group = "15 · CONTROL — ADVANCED", DefaultValue = 0.20, MinValue = 0.02, MaxValue = 2)]
        public double TargetUpdateStepAtr { get; set; }

        [Parameter("Use Swing Structure In Structural Stop", Group = "15 · CONTROL — ADVANCED", DefaultValue = true)]
        public bool UseSwingStructureInTrail { get; set; }

        [Parameter("Smart Minimum Independent Evidence", Group = "15 · CONTROL — ADVANCED", DefaultValue = 4, MinValue = 2, MaxValue = 8)]
        public int SmartMinimumIndependentEvidence { get; set; }

        [Parameter("Smart Stop Zone Bonus", Group = "15 · CONTROL — ADVANCED", DefaultValue = 10, MinValue = 0, MaxValue = 30)]
        public int SmartStopZoneBonus { get; set; }

        [Parameter("Smart Liquidity Pool Bonus", Group = "15 · CONTROL — ADVANCED", DefaultValue = 12, MinValue = 0, MaxValue = 30)]
        public int SmartLiquidityPoolBonus { get; set; }

        [Parameter("Smart Trail Minimum RR", Group = "15 · CONTROL — ADVANCED", DefaultValue = 1.00, MinValue = 0.5, MaxValue = 10)]
        public double SmartTrailMinimumRR { get; set; }

        [Parameter("Smart Use Closed-Bar Decision (Safety-Enforced)", Group = "15 · CONTROL — ADVANCED", DefaultValue = true)]
        public bool SmartUseClosedBarDecision { get; set; }

        [Parameter("Smart Target Nearest Bias", Group = "15 · CONTROL — ADVANCED", DefaultValue = 0.65, MinValue = 0.20, MaxValue = 1.0, Step = 0.05)]
        public double SmartTargetNearestBias { get; set; }

        [Parameter("Require Smart Consensus", Group = "15 · CONTROL — ADVANCED", DefaultValue = true)]
        public bool RequireSmartConsensus { get; set; }

        [Parameter("Smart Strong Setup Quality", Group = "15 · CONTROL — ADVANCED", DefaultValue = 82, MinValue = 60, MaxValue = 99)]
        public int SmartStrongSetupQuality { get; set; }

        [Parameter("Smart Strong Setup Edge", Group = "15 · CONTROL — ADVANCED", DefaultValue = 10, MinValue = 4, MaxValue = 30)]
        public int SmartStrongSetupEdge { get; set; }

        [Parameter("Smart Flip Confirmation Bars", Group = "15 · CONTROL — ADVANCED", DefaultValue = 2, MinValue = 1, MaxValue = 5)]
        public int SmartFlipConfirmationBars { get; set; }

        [Parameter("Allow Smart Soft Gate", Group = "15 · CONTROL — ADVANCED", DefaultValue = true)]
        public bool AllowSmartSoftGate { get; set; }

        [Parameter("Enable Fast Reversal Intelligence", Group = "15 · CONTROL — ADVANCED", DefaultValue = true)]
        public bool EnableFastReversalIntelligence { get; set; }

        [Parameter("Fast Reversal Minimum Quality", Group = "15 · CONTROL — ADVANCED", DefaultValue = 74, MinValue = 50, MaxValue = 95)]
        public int FastReversalMinimumQuality { get; set; }

        [Parameter("Fast Reversal Lookback Bars", Group = "15 · CONTROL — ADVANCED", DefaultValue = 6, MinValue = 3, MaxValue = 15)]
        public int FastReversalLookbackBars { get; set; }

        [Parameter("Fast Reversal Minimum Zone Quality", Group = "15 · CONTROL — ADVANCED", DefaultValue = 60, MinValue = 40, MaxValue = 90)]
        public int FastReversalMinimumZoneQuality { get; set; }

        [Parameter("Allow Fast M5 Reversal Before M15", Group = "15 · CONTROL — ADVANCED", DefaultValue = true)]
        public bool AllowFastM5ReversalBeforeM15 { get; set; }

        [Parameter("Retest Lookback Bars", Group = "15 · CONTROL — ADVANCED", DefaultValue = 8, MinValue = 3, MaxValue = 30)]
        public int RetestLookbackBars { get; set; }

        [Parameter("Retest Max Bars After Displacement", Group = "15 · CONTROL — ADVANCED", DefaultValue = 6, MinValue = 1, MaxValue = 20)]
        public int RetestMaxBarsAfterDisplacement { get; set; }

        [Parameter("Retest Zone Tolerance ATR", Group = "15 · CONTROL — ADVANCED", DefaultValue = 0.25, MinValue = 0.05, MaxValue = 1)]
        public double RetestZoneToleranceAtr { get; set; }

        [Parameter("Retest Rejection Body ATR", Group = "15 · CONTROL — ADVANCED", DefaultValue = 0.12, MinValue = 0.02, MaxValue = 1)]
        public double RetestRejectionBodyAtr { get; set; }

        [Parameter("Require Retest Close Confirmation", Group = "15 · CONTROL — ADVANCED", DefaultValue = true)]
        public bool RequireRetestCloseConfirmation { get; set; }

        [Parameter("Use Extended Liquidity Map", Group = "15 · CONTROL — ADVANCED", DefaultValue = true)]
        public bool UseExtendedLiquidityMap { get; set; }

        [Parameter("Use Session Liquidity Targets", Group = "15 · CONTROL — ADVANCED", DefaultValue = true)]
        public bool UseSessionLiquidityTargets { get; set; }

        [Parameter("Liquidity Target Minimum Score", Group = "15 · CONTROL — ADVANCED", DefaultValue = 72, MinValue = 40, MaxValue = 100)]
        public int LiquidityTargetMinimumScore { get; set; }

        [Parameter("Target Obstacle Buffer ATR", Group = "15 · CONTROL — ADVANCED", DefaultValue = 0.10, MinValue = 0.01, MaxValue = 1)]
        public double TargetObstacleBufferAtr { get; set; }

        [Parameter("Require Obstacle Free TP1", Group = "15 · CONTROL — ADVANCED", DefaultValue = true)]
        public bool RequireObstacleFreeTp1 { get; set; }

        [Parameter("Use Volume Expansion", Group = "20 · Confluence Extensions", DefaultValue = false)]
        public bool UseVolumeExpansion { get; set; }

        [Parameter("Volume Expansion Ratio", Group = "20 · Confluence Extensions", DefaultValue = 1.15, MinValue = 1.0, MaxValue = 3.0, Step = 0.05)]
        public double VolumeExpansionRatio { get; set; }

        [Parameter("Use MACD Bias", Group = "20 · Confluence Extensions", DefaultValue = false)]
        public bool UseMacdBias { get; set; }

        [Parameter("MACD Fast Period", Group = "20 · Confluence Extensions", DefaultValue = 12, MinValue = 2, MaxValue = 50)]
        public int MacdFastPeriod { get; set; }

        [Parameter("MACD Slow Period", Group = "20 · Confluence Extensions", DefaultValue = 26, MinValue = 3, MaxValue = 100)]
        public int MacdSlowPeriod { get; set; }

        [Parameter("Use VWAP Bias", Group = "20 · Confluence Extensions", DefaultValue = false)]
        public bool UseVwapBias { get; set; }

        [Parameter("VWAP Lookback Bars", Group = "20 · Confluence Extensions", DefaultValue = 48, MinValue = 10, MaxValue = 200)]
        public int VwapLookbackBars { get; set; }

        [Parameter("Use Healthy Volatility", Group = "20 · Confluence Extensions", DefaultValue = false)]
        public bool UseHealthyVolatility { get; set; }

        [Parameter("Healthy ATR Minimum Ratio", Group = "20 · Confluence Extensions", DefaultValue = 0.85, MinValue = 0.50, MaxValue = 1.50, Step = 0.05)]
        public double HealthyAtrMinimumRatio { get; set; }

        [Parameter("Healthy ATR Maximum Ratio", Group = "20 · Confluence Extensions", DefaultValue = 1.80, MinValue = 1.0, MaxValue = 3.0, Step = 0.05)]
        public double HealthyAtrMaximumRatio { get; set; }

        [Parameter("Early Setup Confidence", Group = "21 · Complete Intelligence", DefaultValue = 52, MinValue = 40, MaxValue = 95)]
        public int EarlySetupConfidence { get; set; }

        [Parameter("Enable Live Reaction", Group = "21 · Complete Intelligence", DefaultValue = true)]
        public bool EnableLiveReaction { get; set; }

        [Parameter("Live Reaction Watch Threshold", Group = "21 · Complete Intelligence", DefaultValue = 56, MinValue = 40, MaxValue = 95)]
        public int LiveReactionWatchThreshold { get; set; }

        [Parameter("Live Reaction Threshold", Group = "21 · Complete Intelligence", DefaultValue = 68, MinValue = 40, MaxValue = 95)]
        public int LiveReactionThreshold { get; set; }

        [Parameter("Live Reaction Strong Threshold", Group = "21 · Complete Intelligence", DefaultValue = 82, MinValue = 50, MaxValue = 99)]
        public int LiveReactionStrongThreshold { get; set; }

        [Parameter("Minimum Live Reaction Evidence", Group = "21 · Complete Intelligence", DefaultValue = 3, MinValue = 1, MaxValue = 8)]
        public int MinimumLiveReactionEvidence { get; set; }

        [Parameter("Use Volume Expansion Evidence", Group = "21 · Complete Intelligence", DefaultValue = true)]
        public bool UseVolumeExpansionEvidence { get; set; }

        [Parameter("Use MACD Evidence", Group = "21 · Complete Intelligence", DefaultValue = true)]
        public bool UseMacdEvidence { get; set; }

        [Parameter("Use VWAP Evidence", Group = "21 · Complete Intelligence", DefaultValue = true)]
        public bool UseVwapEvidence { get; set; }

        [Parameter("Use Healthy Volatility Evidence", Group = "21 · Complete Intelligence", DefaultValue = true)]
        public bool UseHealthyVolatilityEvidence { get; set; }

        [Parameter("Minimum Smart Direction Share", Group = "21 · Complete Intelligence", DefaultValue = 57, MinValue = 50, MaxValue = 95)]
        public int MinimumSmartDirectionShare { get; set; }

        [Parameter("Adaptive Smart Thresholds", Group = "21 · Complete Intelligence", DefaultValue = true)]
        public bool AdaptiveSmartThresholds { get; set; }

        [Parameter("Smart Regime Buffer", Group = "21 · Complete Intelligence", DefaultValue = 6, MinValue = 0, MaxValue = 15)]
        public int SmartRegimeBuffer { get; set; }

        [Parameter("Smart Score Temperature", Group = "21 · Complete Intelligence", DefaultValue = 12.0, MinValue = 1.0, MaxValue = 50.0, Step = 0.5)]
        public double SmartScoreTemperature { get; set; }

        [Parameter("Smart Consensus Threshold", Group = "21 · Complete Intelligence", DefaultValue = 57, MinValue = 50, MaxValue = 95)]
        public int SmartConsensusThreshold { get; set; }

        [Parameter("Adaptive Regime Weighting", Group = "21 · Complete Intelligence", DefaultValue = true)]
        public bool AdaptiveRegimeWeighting { get; set; }

        [Parameter("Use Multi TF Level Map", Group = "21 · Complete Intelligence", DefaultValue = true)]
        public bool UseMultiTfLevelMap { get; set; }

        [Parameter("Smart Level Cluster ATR", Group = "21 · Complete Intelligence", DefaultValue = 0.10, MinValue = 0.02, MaxValue = 1.0, Step = 0.01)]
        public double SmartLevelClusterAtr { get; set; }

        [Parameter("Smart Target Quality", Group = "21 · Complete Intelligence", DefaultValue = 58, MinValue = 40, MaxValue = 100)]
        public int SmartTargetQuality { get; set; }

        [Parameter("Smart Stop Quality", Group = "21 · Complete Intelligence", DefaultValue = 55, MinValue = 40, MaxValue = 100)]
        public int SmartStopQuality { get; set; }

        [Parameter("Smart Exit Pressure Threshold", Group = "21 · Complete Intelligence", DefaultValue = 65, MinValue = 40, MaxValue = 95)]
        public int SmartExitPressureThreshold { get; set; }

        [Parameter("Smart Alert Cooldown Seconds", Group = "21 · Complete Intelligence", DefaultValue = 8, MinValue = 1, MaxValue = 60)]
        public int SmartAlertCooldownSeconds { get; set; }

        [Parameter("Smart Weekly Context", Group = "21 · Complete Intelligence", DefaultValue = true)]
        public bool SmartWeeklyContext { get; set; }

        [Parameter("Block Same-Bar Reentry After Exit", Group = "21 · Complete Intelligence", DefaultValue = true)]
        public bool BlockSameBarReentryAfterExit { get; set; }

        [Parameter("Allow Execution Frame Stop Fallback", Group = "21 · Complete Intelligence", DefaultValue = true)]
        public bool AllowExecutionFrameStopFallback { get; set; }

        [Parameter("Use Historical Choppiness Guard", Group = "21 · Complete Intelligence", DefaultValue = true)]
        public bool UseHistoricalChoppinessGuard { get; set; }

        [Parameter("Alert On News Event", Group = "21 · Complete Intelligence", DefaultValue = true)]
        public bool AlertOnNewsEvent { get; set; }

        [Parameter("Alert On Session Block", Group = "21 · Complete Intelligence", DefaultValue = false)]
        public bool AlertOnSessionBlock { get; set; }

        [Parameter("Alert On Spread Block", Group = "21 · Complete Intelligence", DefaultValue = false)]
        public bool AlertOnSpreadBlock { get; set; }

        [Parameter("Alert On Friday Block", Group = "21 · Complete Intelligence", DefaultValue = false)]
        public bool AlertOnFridayBlock { get; set; }

        [Parameter("Alert On Regime No Trade", Group = "21 · Complete Intelligence", DefaultValue = false)]
        public bool AlertOnRegimeNoTrade { get; set; }

        [Parameter("Alert On Cooldown Block", Group = "21 · Complete Intelligence", DefaultValue = false)]
        public bool AlertOnCooldownBlock { get; set; }

        [Parameter("Alert On Exit Plan Update", Group = "21 · Complete Intelligence", DefaultValue = true)]
        public bool AlertOnExitPlanUpdate { get; set; }

        [Parameter("Show Engine Status", Group = "21 · Complete Intelligence", DefaultValue = true)]
        public bool ShowEngineStatus { get; set; }

        [Parameter("Panel State Hold Seconds", Group = "21 · Complete Intelligence", DefaultValue = 2, MinValue = 0, MaxValue = 10)]
        public int PanelStateHoldSeconds { get; set; }

        [Parameter("Show Level Prices In Unified Panel", Group = "21 · Complete Intelligence", DefaultValue = true)]
        public bool ShowLevelPricesInUnifiedPanel { get; set; }

        [Parameter("Show Trade Plan Panel", Group = "21 · Complete Intelligence", DefaultValue = true)]
        public bool ShowTradePlanPanel { get; set; }

        [Parameter("Show Trade Action Buttons", Group = "13 · AUTO TRADING", DefaultValue = true)]
        public bool ShowTradeActionButtons { get; set; }

        [Parameter("Always Show Safety Buttons", Group = "13 · AUTO TRADING", DefaultValue = true)]
        public bool AlwaysShowSafetyButtons { get; set; }

        [Parameter("Action Button Margin", Group = "13 · AUTO TRADING", DefaultValue = 2, MinValue = 0, MaxValue = 20)]
        public int ActionButtonMargin { get; set; }

        [Parameter("Use Authoritative Signal State", Group = "21 · Complete Intelligence", DefaultValue = true)]
        public bool UseAuthoritativeSignalState { get; set; }

        [Parameter("Require Plan Integrity", Group = "21 · Complete Intelligence", DefaultValue = true)]
        public bool RequirePlanIntegrity { get; set; }

        [Parameter("Maximum Spread / Stop Risk Ratio", Group = "21 · Complete Intelligence", DefaultValue = 0.18, MinValue = 0.02, MaxValue = 0.50, Step = 0.01)]
        public double MaximumSpreadToStopRiskRatio { get; set; }

        [Parameter("Minimum Smart Target Quality For TP1", Group = "21 · Complete Intelligence", DefaultValue = 55, MinValue = 40, MaxValue = 95)]
        public int MinimumSmartTargetQualityForTp1 { get; set; }

        [Parameter("Prediction Color", Group = "21 · Complete Intelligence", DefaultValue = "#4A90E2")]
        public Color PredictionColor { get; set; }

        [Parameter("Strong BUY Arrow Color", Group = "21 · Complete Intelligence", DefaultValue = "Lime")]
        public Color StrongBuyArrowColor { get; set; }

        [Parameter("Strong SELL Arrow Color", Group = "21 · Complete Intelligence", DefaultValue = "Red")]
        public Color StrongSellArrowColor { get; set; }

        [Parameter("Confirmed BUY Arrow Color", Group = "21 · Complete Intelligence", DefaultValue = "Lime")]
        public Color ConfirmedBuyArrowColor { get; set; }

        [Parameter("Confirmed SELL Arrow Color", Group = "21 · Complete Intelligence", DefaultValue = "Red")]
        public Color ConfirmedSellArrowColor { get; set; }

        [Parameter("Caution BUY Arrow Color", Group = "21 · Complete Intelligence", DefaultValue = "#9AA7B4")]
        public Color CautionBuyArrowColor { get; set; }

        [Parameter("Caution SELL Arrow Color", Group = "21 · Complete Intelligence", DefaultValue = "#9AA7B4")]
        public Color CautionSellArrowColor { get; set; }

        [Parameter("Blocked / Reaction Arrow Color", Group = "21 · Complete Intelligence", DefaultValue = "#9AA7B4")]
        public Color BlockedReactionArrowColor { get; set; }

        [Parameter("Fallback TP1 RR", Group = "21 · Complete Intelligence", DefaultValue = 2.0, MinValue = 0.5, MaxValue = 10, Step = 0.05)]
        public double FallbackTp1RR { get; set; }

        [Parameter("Fallback TP2 RR", Group = "21 · Complete Intelligence", DefaultValue = 3.2, MinValue = 0.8, MaxValue = 20, Step = 0.05)]
        public double FallbackTp2RR { get; set; }

        [Parameter("Fallback TP3 RR", Group = "21 · Complete Intelligence", DefaultValue = 4.8, MinValue = 1.0, MaxValue = 30, Step = 0.05)]
        public double FallbackTp3RR { get; set; }

        [Parameter("Fallback TP4 RR", Group = "21 · Complete Intelligence", DefaultValue = 6.5, MinValue = 1.5, MaxValue = 40, Step = 0.05)]
        public double FallbackTp4RR { get; set; }

        [Parameter("Require Precision Entry", Group = "23 · Structural Execution", DefaultValue = true)]
        public bool RequirePrecisionEntry { get; set; }

        [Parameter("Execution Zone ATR", Group = "23 · Structural Execution", DefaultValue = 0.35, MinValue = 0.05, MaxValue = 1.50, Step = 0.01)]
        public double ExecutionZoneAtr { get; set; }

        [Parameter("Minimum Entry Quality", Group = "23 · Structural Execution", DefaultValue = 72, MinValue = 40, MaxValue = 100)]
        public int MinimumEntryQuality { get; set; }

        [Parameter("Maximum Entry Distance ATR", Group = "23 · Structural Execution", DefaultValue = 0.45, MinValue = 0.05, MaxValue = 2.0, Step = 0.01)]
        public double MaximumEntryDistanceAtr { get; set; }

        [Parameter("Allow Precision Breakout Entry", Group = "23 · Structural Execution", DefaultValue = true)]
        public bool AllowPrecisionBreakoutEntry { get; set; }

        [Parameter("Precision Breakout Buffer ATR", Group = "23 · Structural Execution", DefaultValue = 0.08, MinValue = 0.01, MaxValue = 0.50, Step = 0.01)]
        public double PrecisionBreakoutBufferAtr { get; set; }

        [Parameter("Minimum Targets For Plan", Group = "23 · Structural Execution", DefaultValue = 1, MinValue = 1, MaxValue = 4)]
        public int MinimumTargetsForPlan { get; set; }

        [Parameter("Require HTF Reward For TP1", Group = "23 · Structural Execution", DefaultValue = false)]
        public bool RequireHtfRewardForTp1 { get; set; }

        [Parameter("Require HTF Reward For TP2+", Group = "23 · Structural Execution", DefaultValue = true)]
        public bool RequireHtfRewardForTp2Plus { get; set; }

        [Parameter("Minimum HTF Reward Quality", Group = "23 · Structural Execution", DefaultValue = 68, MinValue = 40, MaxValue = 100)]
        public int MinimumHtfRewardQuality { get; set; }

        [Parameter("Maximum Reward RR", Group = "23 · Structural Execution", DefaultValue = 12.0, MinValue = 2.0, MaxValue = 40, Step = 0.10)]
        public double MaximumRewardRR { get; set; }

        [Parameter("HTF Reward Bonus", Group = "23 · Structural Execution", DefaultValue = 22, MinValue = 0, MaxValue = 40)]
        public int HtfRewardBonus { get; set; }

        [Parameter("Liquidity Reward Bonus", Group = "23 · Structural Execution", DefaultValue = 14, MinValue = 0, MaxValue = 40)]
        public int LiquidityRewardBonus { get; set; }

        [Parameter("Zone Reward Bonus", Group = "23 · Structural Execution", DefaultValue = 10, MinValue = 0, MaxValue = 30)]
        public int ZoneRewardBonus { get; set; }

        [Parameter("Preferred Stop Risk ATR", Group = "23 · Structural Execution", DefaultValue = 1.00, MinValue = 0.25, MaxValue = 3.0, Step = 0.05)]
        public double PreferredStopRiskAtr { get; set; }

        [Parameter("Stop Risk Balance Weight", Group = "23 · Structural Execution", DefaultValue = 18, MinValue = 0, MaxValue = 40)]
        public int StopRiskBalanceWeight { get; set; }

        [Parameter("Minimum Structural Stop Quality", Group = "23 · Structural Execution", DefaultValue = 65, MinValue = 40, MaxValue = 100)]
        public int MinimumStructuralStopQuality { get; set; }

        [Parameter("Structural Stop Management Only", Group = "23 · Structural Execution", DefaultValue = true)]
        public bool StructuralStopManagementOnly { get; set; }

        [Parameter("Structural Target Updates Only", Group = "23 · Structural Execution", DefaultValue = true)]
        public bool StructuralTargetUpdatesOnly { get; set; }

        [Parameter("Use M5 Structure For Initial Stop", Group = "22 · Safety & Precision", DefaultValue = true)]
        public bool UseM5StructureForStop { get; set; }

        [Parameter("Use Zone Mitigation Guard", Group = "22 · Safety & Precision", DefaultValue = true)]
        public bool UseZoneMitigationGuard { get; set; }

        [Parameter("Require Order Block Retest", Group = "22 · Safety & Precision", DefaultValue = false)]
        public bool RequireObRetest { get; set; }

[Parameter("FVG Invalidate On Full Fill (Safety-Enforced)", Group = "22 · Safety & Precision", DefaultValue = true)]
        public bool FvgInvalidateOnFullFill { get; set; }

        [Parameter("FVG Partial Mitigation", Group = "22 · Safety & Precision", DefaultValue = true)]
        public bool EnableFvgPartialMitigation { get; set; }

        [Parameter("FVG Break By Wicks", Group = "22 · Safety & Precision", DefaultValue = false)]
        public bool FvgBreakByWicks { get; set; }

        [Parameter("Include Spread In Risk Sizing", Group = "13 · AUTO TRADING", DefaultValue = true)]
        public bool IncludeSpreadInRiskSizing { get; set; }

        [Parameter("Use Semantic Alert Sounds", Group = "22 · Safety & Precision", DefaultValue = true)]
        public bool UseSemanticAlertSounds { get; set; }

        [Parameter("Managed Actions Only", Group = "13 · AUTO TRADING", DefaultValue = true)]
        public bool ManagedActionsOnly { get; set; }

        [Parameter("Show Spread Diagnostics", Group = "22 · Safety & Precision", DefaultValue = true)]
        public bool ShowSpreadDiagnostics { get; set; }

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