using cAlgo.API;

namespace cAlgo
{
    public partial class CFIPIndicator : Indicator
    {
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
    }
}
