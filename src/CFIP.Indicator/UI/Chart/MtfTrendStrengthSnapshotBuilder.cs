using System;
using cAlgo.API;

namespace cAlgo
{
    public partial class CFIPIndicator : Indicator
    {
        private void ApplyMtfTrendStrength(
            SignalVisualSnapshot snapshot)
        {
            if (snapshot == null)
                return;

            MtfTrendStrengthResult trendStrength =
                MtfTrendStrengthRule.Evaluate(
                    new[]
                    {
                        _m1Frame,
                        _m5Frame,
                        _m15Frame,
                        _m30Frame,
                        _h1Frame,
                        _h4Frame,
                        _d1Frame,
                        _w1Frame
                    },
                    new[]
                    {
                        0.0,
                        Math.Max(0, M5Weight),
                        Math.Max(0, M15Weight),
                        Math.Max(0, M30Weight),
                        Math.Max(0, H1Weight),
                        Math.Max(0, H4Weight),
                        Math.Max(0, D1Weight),
                        SmartWeeklyContext
                            ? Math.Max(0, W1Weight)
                            : 0
                    },
                    Symbol.Ask > 0 &&
                    Symbol.Bid > 0
                        ? (Symbol.Ask + Symbol.Bid) * 0.5
                        : Symbol.Ask);

            snapshot.MtfTrendDirection =
                trendStrength.Direction;
            snapshot.MtfTrendStrengthLevel =
                trendStrength.Level;
            snapshot.MtfTrendStrengthScore =
                trendStrength.Score;
            snapshot.HtfTrendDirection =
                trendStrength.HigherTimeframeDirection;
            snapshot.HtfTrendStrengthScore =
                trendStrength.HigherTimeframeScore;
        }
    }
}
