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
                        
                                    int[] directions =
                                    {
                                        _m1Frame == null ? 0 : _m1Frame.Direction,
                                        _m5Frame == null ? 0 : _m5Frame.Direction,
                                        _m15Frame == null ? 0 : _m15Frame.Direction,
                                        _m30Frame == null ? 0 : _m30Frame.Direction,
                                        _h1Frame == null ? 0 : _h1Frame.Direction,
                                        _h4Frame == null ? 0 : _h4Frame.Direction,
                                        _d1Frame == null ? 0 : _d1Frame.Direction,
                                        _w1Frame == null ? 0 : _w1Frame.Direction
                                    };

                                    int[] qualities =
                                    {
                                        _m1Frame == null ? 0 : _m1Frame.Quality,
                                        _m5Frame == null ? 0 : _m5Frame.Quality,
                                        _m15Frame == null ? 0 : _m15Frame.Quality,
                                        _m30Frame == null ? 0 : _m30Frame.Quality,
                                        _h1Frame == null ? 0 : _h1Frame.Quality,
                                        _h4Frame == null ? 0 : _h4Frame.Quality,
                                        _d1Frame == null ? 0 : _d1Frame.Quality,
                                        _w1Frame == null ? 0 : _w1Frame.Quality
                                    };

                                    int[] actualIndices =
                                    {
                                        _m1Frame == null ? -1 : _m1Frame.Index,
                                        _m5Frame == null ? -1 : _m5Frame.Index,
                                        _m15Frame == null ? -1 : _m15Frame.Index,
                                        _m30Frame == null ? -1 : _m30Frame.Index,
                                        _h1Frame == null ? -1 : _h1Frame.Index,
                                        _h4Frame == null ? -1 : _h4Frame.Index,
                                        _d1Frame == null ? -1 : _d1Frame.Index,
                                        _w1Frame == null ? -1 : _w1Frame.Index
                                    };

                                    return
                                        TimeframeAgreementRule.Calculate(
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
