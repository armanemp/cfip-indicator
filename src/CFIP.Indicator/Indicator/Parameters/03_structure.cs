using cAlgo.API;

namespace cAlgo
{
    public partial class CFIPIndicator : Indicator
    {
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
    }
}
