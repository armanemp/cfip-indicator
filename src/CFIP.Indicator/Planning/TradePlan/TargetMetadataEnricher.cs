// CFIP Indicator — TargetMetadataEnricher.cs
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
private void ApplyTargetMeta(
                                            List<Level> candidates,
                                            double target,
                                            double atr,
                                            out string source,
                                            out int quality)
                                        {
                                            source =
                                                target > 0
                                                    ? "RR"
                                                    : "";
                                
                                            quality =
                                                target > 0
                                                    ? 55
                                                    : 0;
                                
                                            if (target <= 0)
                                                return;
                                
                                            Level best = null;
                                            double bestDistance = double.MaxValue;
                                
                                            for (int i = 0; i < candidates.Count; i++)
                                            {
                                                double distance =
                                                    Math.Abs(
                                                        candidates[i].Price -
                                                        target);
                                
                                                if (distance <= atr * 0.15 &&
                                                    distance < bestDistance)
                                                {
                                                    best =
                                                        candidates[i];
                                
                                                    bestDistance =
                                                        distance;
                                                }
                                            }
                                
                                            if (best != null)
                                            {
                                                source =
                                                    best.Kind +
                                                    "@" +
                                                    best.Timeframe;
                                
                                                quality =
                                                    ClampInt(
                                                        (int)Math.Round(
                                                            best.Score),
                                                        0,
                                                        100);
                                
                                                if (best.Age <= 5)
                                                    quality =
                                                        Math.Min(
                                                            100,
                                                            quality + 5);
                                            }
                                        }
    }
}
