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

            AggressiveFlowSnapshot flow =
                GetAggressiveFlowSnapshot();

            // Flow is realtime modulation only. If the provider has gone
            // quiet, do not let an old flow snapshot keep influencing the
            // current signal/arrow strength.
            if (!TryGetFreshAggressiveFlowSnapshot(
                    out flow))
            {
                flow =
                    new AggressiveFlowSnapshot(
                        0,
                        0,
                        0,
                        0,
                        0,
                        DateTime.MinValue);
            }

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
                    0.0,
                    flow.DirectionalBias,
                    flow.DirectionalConfidence);

            snapshot.MtfTrendDirection =
                trendStrength.Direction;
            snapshot.MtfTrendStrengthLevel =
                trendStrength.Level;
            snapshot.MtfTrendStrengthScore =
                trendStrength.Score;
            snapshot.MtfTrendStrengthTier =
                trendStrength.Tier;
            snapshot.HtfTrendDirection =
                trendStrength.HigherTimeframeDirection;
            snapshot.HtfTrendStrengthScore =
                trendStrength.HigherTimeframeScore;
        }
    }
}
