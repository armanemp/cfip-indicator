// CFIP Indicator — TimeframeAgreementAnalyzer.cs
// Single-responsibility decision evidence module.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using cAlgo.API;
using cAlgo.API.Indicators;
using cAlgo.API.Internals;

namespace cAlgo
{
    public partial class CFIPIndicator : Indicator
    {
        private int TimeframeAgreement(
            int direction,
            MtfClosedContext context)
        {
            if (direction == 0 ||
                context == null)
                return 0;

            int[] closedIndices =
            {
                context.M1,
                context.M5,
                context.M15,
                context.M30,
                context.H1,
                context.H4,
                context.D1,
                context.W1
            };

            Frame[] frames =
            {
                _m1Frame,
                _m5Frame,
                _m15Frame,
                _m30Frame,
                _h1Frame,
                _h4Frame,
                _d1Frame,
                _w1Frame
            };

            Bars[] bars =
            {
                _m1Bars,
                _m5Bars,
                _m15Bars,
                _m30Bars,
                _h1Bars,
                _h4Bars,
                _d1Bars,
                _w1Bars
            };

            int[] directions =
            {
                frames[0] == null ? 0 : frames[0].Direction,
                frames[1] == null ? 0 : frames[1].Direction,
                frames[2] == null ? 0 : frames[2].Direction,
                frames[3] == null ? 0 : frames[3].Direction,
                frames[4] == null ? 0 : frames[4].Direction,
                frames[5] == null ? 0 : frames[5].Direction,
                frames[6] == null ? 0 : frames[6].Direction,
                frames[7] == null ? 0 : frames[7].Direction
            };

            int[] qualities =
            {
                frames[0] == null || bars[0] == null ? 0 : frames[0].Quality,
                frames[1] == null || bars[1] == null ? 0 : frames[1].Quality,
                frames[2] == null || bars[2] == null ? 0 : frames[2].Quality,
                frames[3] == null || bars[3] == null ? 0 : frames[3].Quality,
                frames[4] == null || bars[4] == null ? 0 : frames[4].Quality,
                frames[5] == null || bars[5] == null ? 0 : frames[5].Quality,
                frames[6] == null || bars[6] == null ? 0 : frames[6].Quality,
                frames[7] == null || bars[7] == null ? 0 : frames[7].Quality
            };

            int[] actualIndices =
            {
                frames[0] == null || bars[0] == null ? -1 : frames[0].Index,
                frames[1] == null || bars[1] == null ? -1 : frames[1].Index,
                frames[2] == null || bars[2] == null ? -1 : frames[2].Index,
                frames[3] == null || bars[3] == null ? -1 : frames[3].Index,
                frames[4] == null || bars[4] == null ? -1 : frames[4].Index,
                frames[5] == null || bars[5] == null ? -1 : frames[5].Index,
                frames[6] == null || bars[6] == null ? -1 : frames[6].Index,
                frames[7] == null || bars[7] == null ? -1 : frames[7].Index
            };

            double[] weights =
            {
                UseM1Trigger ? Math.Max(1.0, M5Weight * 0.35) : 0,
                Math.Max(0, M5Weight),
                Math.Max(0, M15Weight),
                Math.Max(0, M30Weight),
                Math.Max(0, H1Weight),
                Math.Max(0, H4Weight),
                Math.Max(0, D1Weight),
                Math.Max(0, W1Weight)
            };

            bool[] enabled =
            {
                UseM1Trigger,
                true,
                true,
                M30Weight > 0,
                H1Weight > 0,
                H4Weight > 0,
                D1Weight > 0,
                SmartWeeklyContext && W1Weight > 0
            };

            return TimeframeAgreementRule.Calculate(
                direction,
                directions,
                qualities,
                actualIndices,
                closedIndices,
                weights,
                enabled);
        }
    }
}
