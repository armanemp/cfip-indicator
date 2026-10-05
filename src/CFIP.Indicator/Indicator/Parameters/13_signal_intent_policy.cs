using cAlgo.API;

namespace cAlgo
{
    public partial class CFIPIndicator : Indicator
    {
        [Parameter("Pending Order Mode", Group = "13 · SIGNAL / INTENT POLICY", DefaultValue = PendingOrderMode.Adaptive)]
        public PendingOrderMode PendingOrderMode { get; set; }[Parameter("Pending Entry Buffer ATR", Group = "13 · SIGNAL / INTENT POLICY", DefaultValue = 0.10, MinValue = 0.02, MaxValue = 1.00, Step = 0.01)]
        public double PendingEntryBufferAtr { get; set; }

        [Parameter("Pending Minimum Confidence", Group = "13 · SIGNAL / INTENT POLICY", DefaultValue = 84, MinValue = 50, MaxValue = 99)]
        public int PendingMinimumConfidence { get; set; }

        [Parameter("Pending Minimum Smart Quality", Group = "13 · SIGNAL / INTENT POLICY", DefaultValue = 78, MinValue = 50, MaxValue = 95)]
        public int PendingMinimumSmartQuality { get; set; }

        [Parameter("Pending Minimum Trend Quality", Group = "13 · SIGNAL / INTENT POLICY", DefaultValue = 72, MinValue = 40, MaxValue = 100)]
        public int PendingMinimumTrendQuality { get; set; }

        [Parameter("Pending Auto Cleanup", Group = "13 · SIGNAL / INTENT POLICY", DefaultValue = true)]
        public bool PendingAutoCleanup { get; set; }

        [Parameter("Reversal Close Minimum Evidence", Group = "13 · SIGNAL / INTENT POLICY", DefaultValue = 5, MinValue = 2, MaxValue = 10)]
        public int ReversalCloseMinimumEvidence { get; set; }

        [Parameter("Reversal Close Minimum MTF", Group = "13 · SIGNAL / INTENT POLICY", DefaultValue = 70, MinValue = 50, MaxValue = 100)]
        public int ReversalCloseMinimumMtf { get; set; }

        [Parameter("Reversal Close Minimum Net Profit", Group = "13 · SIGNAL / INTENT POLICY", DefaultValue = 0.0, MinValue = 0, MaxValue = 100000, Step = 0.01)]
        public double ReversalCloseMinimumNetProfit { get; set; }

        [Parameter("Sizing Mode", Group = "13 · SIGNAL / INTENT POLICY", DefaultValue = SizingMode.RiskPercentEquity)]
        public SizingMode SizingMode { get; set; }

        [Parameter("Risk % Equity", Group = "13 · SIGNAL / INTENT POLICY", DefaultValue = 0.50, MinValue = 0.05, MaxValue = 5)]
        public double RiskPercentEquity { get; set; }

        [Parameter("Fixed Lots", Group = "13 · SIGNAL / INTENT POLICY", DefaultValue = 0.01, MinValue = 0.001, MaxValue = 100, Step = 0.001)]
        public double FixedLots { get; set; }

        [Parameter("Auto TP Stage", Group = "13 · SIGNAL / INTENT POLICY", DefaultValue = TargetStage.TP1)]
        public TargetStage AutoTpStage { get; set; }

        [Parameter("Enable Dynamic TP Advance", Group = "13 · SIGNAL / INTENT POLICY", DefaultValue = true)]
        public bool EnableDynamicTpAdvance { get; set; }

        [Parameter("TP Advance Proximity Percent", Group = "13 · SIGNAL / INTENT POLICY", DefaultValue = 72, MinValue = 50, MaxValue = 98)]
        public double TpAdvanceProximityPercent { get; set; }

        [Parameter("Enable Partial Take Profit", Group = "13 · SIGNAL / INTENT POLICY", DefaultValue = false)]
        public bool EnablePartialTakeProfit { get; set; }

        [Parameter("Partial Close At TP1 Percent", Group = "13 · SIGNAL / INTENT POLICY", DefaultValue = 33, MinValue = 0, MaxValue = 90, Step = 1)]
        public double PartialCloseTp1Percent { get; set; }

        [Parameter("Partial Close At TP2 Percent", Group = "13 · SIGNAL / INTENT POLICY", DefaultValue = 33, MinValue = 0, MaxValue = 90, Step = 1)]
        public double PartialCloseTp2Percent { get; set; }

        [Parameter("Move To Break Even After Partial", Group = "13 · SIGNAL / INTENT POLICY", DefaultValue = true)]
        public bool MoveToBreakEvenAfterPartial { get; set; }

        [Parameter("Enable Reversal Protection Close", Group = "13 · SIGNAL / INTENT POLICY", DefaultValue = true)]
        public bool EnableReversalProtectionClose { get; set; }

        [Parameter("Reversal Protection Minimum Quality", Group = "13 · SIGNAL / INTENT POLICY", DefaultValue = 82, MinValue = 50, MaxValue = 100)]
        public int ReversalProtectionMinimumQuality { get; set; }

        [Parameter("Use Auto Margin Guard", Group = "13 · SIGNAL / INTENT POLICY", DefaultValue = true)]
        public bool UseAutoMarginGuard { get; set; }

        [Parameter("Max Auto Margin Usage %", Group = "13 · SIGNAL / INTENT POLICY", DefaultValue = 80, MinValue = 10, MaxValue = 100)]
        public double MaxAutoMarginUsagePercent { get; set; }

        [Parameter("Margin Buffer %", Group = "13 · SIGNAL / INTENT POLICY", DefaultValue = 10, MinValue = 0, MaxValue = 40)]
        public double MarginBufferPercent { get; set; }

        [Parameter("Include Spread In Risk Sizing", Group = "13 · SIGNAL / INTENT POLICY", DefaultValue = true)]
        public bool IncludeSpreadInRiskSizing { get; set; }

        [Parameter("Aggressive Risk % Equity", Group = "13 · SIGNAL / INTENT POLICY", DefaultValue = 0.25, MinValue = 0.05, MaxValue = 5)]
        public double AggressiveRiskPercentEquity { get; set; }
    }
}
