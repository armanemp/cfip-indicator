using System;

namespace cAlgo
{
    internal static class M5PanelContracts
    {
        internal static void Run()
        {
            Assert(
                PanelDimensionRule.EffectiveWidth(180) == 220 &&
                PanelDimensionRule.EffectiveWidth(220) == 220 &&
                PanelDimensionRule.EffectiveWidth(430) == 430 &&
                PanelDimensionRule.EffectiveWidth(700) == 700 &&
                PanelDimensionRule.EffectiveWidth(900) == 700,
                "panel width clamps deterministically to 220..700");

            Assert(
                PanelDimensionRule.EffectiveContentWidth(
                    220,
                    10,
                    1) == 200,
                "220px panel content width preserves padding and border math");

            Assert(
                PanelDimensionRule.EffectiveContentWidth(
                    430,
                    12,
                    2) == 402 &&
                PanelDimensionRule.EffectiveContentWidth(
                    700,
                    20,
                    2) == 656,
                "430px and 700px panel content widths remain deterministic");

            Assert(
                PanelDimensionRule.EffectiveContentWidth(
                    220,
                    1000,
                    1000) == 200 &&
                PanelDimensionRule.EffectiveContentWidth(
                    430,
                    -10,
                    -5) == 430,
                "panel content width remains bounded for pathological padding/border inputs");

            Console.WriteLine(
                "M5 panel dimension contracts PASS");
        }

        private static void Assert(
            bool condition,
            string reason)
        {
            if (!condition)
                throw new InvalidOperationException(
                    "M5 panel contract failed: " +
                    reason);
        }
    }
}
