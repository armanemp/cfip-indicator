// CFIP Indicator — TargetLevelCandidateMerger.cs
// Single-responsibility planning module.

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
private void AddLevel(
                                            List<Level> levels,
                                            double price,
                                            string kind,
                                            string timeframe,
                                            int age,
                                            double baseScore)
                                        {
                                            if (!IsFinitePositive(price))
                                                return;
                                
                                            double score =
                                                Math.Max(
                                                    0,
                                                    baseScore) * 0.60;
                                
                                            if (age <= 5)
                                                score += 15;
                                
                                            if (timeframe == "H1" ||
                                                timeframe == "H4" ||
                                                timeframe == "D1" ||
                                                timeframe == "W1")
                                                score += 8;
                                
                                            if (kind.IndexOf(
                                                    "LIQUIDITY",
                                                    StringComparison.OrdinalIgnoreCase) >= 0)
                                                score += 5;
                                
                                            if (kind.IndexOf(
                                                    "FVG",
                                                    StringComparison.OrdinalIgnoreCase) >= 0 ||
                                                kind.IndexOf(
                                                    "ORDER_BLOCK",
                                                    StringComparison.OrdinalIgnoreCase) >= 0)
                                                score += 4;
                                
                                            Level level =
                                                new Level
                                                {
                                                    Price =
                                                        NormalizePrice(price),
                                                    Score =
                                                        Clamp(
                                                            score,
                                                            0,
                                                            100),
                                                    Kind = kind,
                                                    Timeframe = timeframe,
                                                    Age =
                                                        Math.Max(
                                                            0,
                                                            age),
                                                    Hits = 1
                                                };
                                
                                            levels.Add(
                                                level);
                                        }
    }
}
