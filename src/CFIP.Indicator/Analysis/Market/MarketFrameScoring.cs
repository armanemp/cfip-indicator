// CFIP Indicator — MarketFrameScoring.cs
// Single-responsibility analysis module.

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
        private void AddScore(
                            bool condition,
                            int score,
                            ref int total,
                            ref int evidence)
                        {
                            if (!condition)
                                return;
                
                            total += score;
                            evidence++;
                        }

        private void AddScore(
                            bool condition,
                            int score,
                            ref int total,
                            ref int evidence,
                            bool countAsEvidence)
                        {
                            if (!condition)
                                return;
                
                            total += score;
                
                            if (countAsEvidence)
                                evidence++;
                        }

        private void AddFrame(
                            Frame frame,
                            double weight,
                            ref double buy,
                            ref double sell,
                            ref int evidence)
                        {
                            if (frame == null || frame.Quality <= 0 || weight <= 0)
                                return;
                
                            double scale =
                                weight / 10.0;
                
                            buy += frame.BullScore * scale;
                            sell += frame.BearScore * scale;
                
                            if (frame.Direction != 0)
                                evidence++;
                        }
    }
}
