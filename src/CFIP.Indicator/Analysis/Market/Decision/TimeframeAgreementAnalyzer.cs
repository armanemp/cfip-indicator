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
                                        SmartWeeklyContext &&
                                        W1Weight > 0
                                    };
                        
                                    double totalWeight = 0;
                                    double alignedWeight = 0;
                        
                                    for (int i = 0;
                                         i < frames.Length;
                                         i++)
                                    {
                                        if (!enabled[i] ||
                                            weights[i] <= 0 ||
                                            frames[i] == null ||
                                            bars[i] == null ||
                                            frames[i].Quality <= 0)
                                            continue;
                        
                                        int closedIndex =
                                            closedIndices[i];
                        
                                        if (closedIndex < 0 ||
                                            frames[i].Index != closedIndex)
                                            continue;
                        
                                        // A neutral timeframe is not evidence against the selected
                                        // direction; it contributes no alignment weight.
                                        if (frames[i].Direction == 0)
                                            continue;
                        
                                        totalWeight +=
                                            weights[i];
                        
                                        if (frames[i].Direction == direction)
                                            alignedWeight +=
                                                weights[i];
                                    }
                        
                                    return
                                        totalWeight <= 0
                                            ? 0
                                            : ClampInt(
                                                (int)Math.Round(
                                                    100.0 *
                                                    alignedWeight /
                                                    totalWeight),
                                                0,
                                                100);
                                }
    }
}
