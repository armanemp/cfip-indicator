using System;
using cAlgo.API;

namespace cAlgo
{
    public partial class CFIPIndicator : Indicator
    {
        private void AddM1MicroTargetContext(
            System.Collections.Generic.List<Level> levels,
            int closedM5,
            int direction,
            double entry,
            double atr)
        {
            if (!UseM1Trigger ||
                _m1Bars == null ||
                levels == null ||
                closedM5 < 0 ||
                !IsFinitePositive(entry) ||
                !IsFinitePositive(atr))
                return;

            DateTime reference =
                _m5Bars.OpenTimes[closedM5];

            int m1Index =
                ClosedIndex(
                    _m1Bars,
                    reference);

            if (m1Index < 10)
                return;

            double m1Atr =
                Atr(
                    _m1Bars,
                    m1Index);

            if (!IsFinitePositive(m1Atr))
                m1Atr = atr;

            double microSwing =
                direction == 1
                    ? FindSwingHighAbove(
                        _m1Bars,
                        m1Index,
                        entry)
                    : FindSwingLowBelow(
                        _m1Bars,
                        m1Index,
                        entry);

            AddLevel(
                levels,
                microSwing,
                "M1_MICRO_SWING_TARGET",
                "M1",
                0,
                Math.Max(
                    1.0,
                    SwingStructureWeight * 0.45),
                TargetAgeSemanticsRule.ElapsedMinutes(
                    _m1Bars.OpenTimes[m1Index],
                    reference));

            Zone microFvg =
                FindNearestFvg(
                    _m1Bars,
                    m1Index,
                    -direction,
                    m1Atr,
                    false,
                    entry,
                    true);

            if (microFvg != null)
            {
                AddLevel(
                    levels,
                    direction == 1
                        ? microFvg.Low
                        : microFvg.High,
                    "M1_MICRO_FVG_TARGET",
                    "M1",
                    microFvg.Age,
                    Math.Max(
                        1.0,
                        (FvgWeight + ZoneRewardBonus) * 0.45),
                    TargetAgeSemanticsRule.ElapsedMinutes(
                        _m1Bars.OpenTimes[m1Index],
                        reference));
            }

            Zone microOb =
                FindNearestOrderBlock(
                    _m1Bars,
                    m1Index,
                    -direction,
                    m1Atr);

            if (microOb != null)
            {
                AddLevel(
                    levels,
                    direction == 1
                        ? microOb.Low
                        : microOb.High,
                    "M1_MICRO_ORDER_BLOCK_TARGET",
                    "M1",
                    microOb.Age,
                    Math.Max(
                        1.0,
                        (OrderBlockWeight + ZoneRewardBonus) * 0.45),
                    TargetAgeSemanticsRule.ElapsedMinutes(
                        _m1Bars.OpenTimes[m1Index],
                        reference));
            }
        }
    }
}
