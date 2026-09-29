using System;
using System.Collections.Generic;
using System.Linq;
using cAlgo.API;
using cAlgo.API.Internals;

namespace cAlgo
{
    public partial class CFIPIndicator : Indicator
    {
        private void AddPredictivePointCandidate(
            List<PredictivePendingCandidate> candidates,
            double level,
            int baseQuality,
            string source,
            int direction,
            double market,
            double atr,
            double minimumFutureDistance,
            double maximumFutureDistance)
        {
            if (!IsFinitePositive(level) ||
                atr <= 0)
                return;

            double distance =
                Math.Abs(
                    level - market);

            bool future =
                direction == 1
                    ? level <=
                      market - minimumFutureDistance
                    : level >=
                      market + minimumFutureDistance;

            if (!future ||
                distance >
                maximumFutureDistance)
                return;

            int contextQuality =
                PredictivePendingContextQuality(
                    direction,
                    "M5");

            double score =
                baseQuality *
                0.65 +
                contextQuality -
                10.0 *
                Math.Min(
                    1.0,
                    distance /
                    Math.Max(
                        Symbol.TickSize,
                        maximumFutureDistance));

            candidates.Add(
                new PredictivePendingCandidate
                {
                    Price = NormalizePrice(level),
                    Score = Math.Max(0, score),
                    Quality = baseQuality,
                    DistanceAtr =
                        distance /
                        Math.Max(
                            Symbol.TickSize,
                            atr),
                    Source = source,
                    ConfluenceCount = 1
                });
        }

        private int PredictivePendingContextQuality(
            int direction,
            string timeframe)
        {
            int score = 0;

            Frame frame =
                timeframe == "M15"
                    ? _m15Frame
                    : _m5Frame;

            if (frame != null)
            {
                if (frame.Direction == direction)
                    score += 6;

                if (frame.StructureBull && direction == 1 ||
                    frame.StructureBear && direction == -1)
                    score += 5;

                if (direction == 1)
                {
                    if (frame.LiquidityBull) score += 5;
                    if (frame.FvgBull) score += 4;
                    if (frame.ObBull) score += 4;
                    if (frame.FvgObBullConfluence) score += 5;
                    if (frame.VolumeBull) score += 2;
                    if (frame.MacdBull) score += 2;
                    if (frame.VwapBull) score += 2;
                    if (frame.VolatilityBull) score += 1;
                    if (frame.EqualLow) score += 3;
                    if (frame.MssBull || frame.ChochBull) score += 3;
                }
                else
                {
                    if (frame.LiquidityBear) score += 5;
                    if (frame.FvgBear) score += 4;
                    if (frame.ObBear) score += 4;
                    if (frame.FvgObBearConfluence) score += 5;
                    if (frame.VolumeBear) score += 2;
                    if (frame.MacdBear) score += 2;
                    if (frame.VwapBear) score += 2;
                    if (frame.VolatilityBear) score += 1;
                    if (frame.EqualHigh) score += 3;
                    if (frame.MssBear || frame.ChochBear) score += 3;
                }

                if (frame.OssBull && direction == 1)
                    score += Math.Min(
                        5,
                        frame.OssBullVotes);

                if (frame.OssBear && direction == -1)
                    score += Math.Min(
                        5,
                        frame.OssBearVotes);
            }

            if (_m15Frame != null &&
                _m15Frame.Direction == direction &&
                timeframe != "M15")
                score += 5;

            if (_m30Frame != null &&
                _m30Frame.Direction == direction)
                score += 3;

            if (_decision != null &&
                _decision.Direction == direction)
                score += Math.Min(
                    8,
                    (int)Math.Round(
                        _decision.Confidence * 0.08));

            if (_reaction != null &&
                _reaction.Direction == direction)
                score += Math.Min(
                    8,
                    (int)Math.Round(
                        _reaction.Confidence * 0.08));

            return Math.Min(
                35,
                score);
        }
    }
}
