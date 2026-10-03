using cAlgo.API;

namespace cAlgo
{
    public partial class CFIPIndicator : Indicator
    {
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

        [Parameter("Reversal Close Minimum Net Profit", Group = "13 · AUTO TRADING", DefaultValue = 0.0, MinValue = 0, MaxValue = 100000, Step = 0.01)]
        public double ReversalCloseMinimumNetProfit { get; set; }

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

        [Parameter("Maximum Open Positions", Group = "13 · AUTO TRADING", DefaultValue = 3, MinValue = 1, MaxValue = 10)]
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

        [Parameter("Auto Trade Label", Group = "13 · AUTO TRADING", DefaultValue = "CFIP-SMART")]
        public string AutoTradeLabel { get; set; }

        [Parameter("Smart Broker Protection", Group = "13 · AUTO TRADING", DefaultValue = true)]
        public bool AutoBrokerProtection { get; set; }

        [Parameter("One Order Per Signal", Group = "13 · AUTO TRADING", DefaultValue = true)]
        public bool OneOrderPerSignal { get; set; }

        [Parameter("Show Trade Action Buttons", Group = "13 · AUTO TRADING", DefaultValue = true)]
        public bool ShowTradeActionButtons { get; set; }

        [Parameter("Always Show Safety Buttons", Group = "13 · AUTO TRADING", DefaultValue = true)]
        public bool AlwaysShowSafetyButtons { get; set; }

        [Parameter("Action Button Margin", Group = "13 · AUTO TRADING", DefaultValue = 2, MinValue = 0, MaxValue = 20)]
        public int ActionButtonMargin { get; set; }

        [Parameter("Include Spread In Risk Sizing", Group = "13 · AUTO TRADING", DefaultValue = true)]
        public bool IncludeSpreadInRiskSizing { get; set; }

        [Parameter("Managed Actions Only", Group = "13 · AUTO TRADING", DefaultValue = true)]
        public bool ManagedActionsOnly { get; set; }

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
    }
}
