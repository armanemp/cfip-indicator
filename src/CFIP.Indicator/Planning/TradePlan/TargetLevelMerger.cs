// CFIP Indicator — TargetLevelMerger.cs
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
private List<Level> MergeLevels(
                                            List<Level> input,
                                            double atr)
                                        {
                                            List<Level> result =
                                                new List<Level>();
                                
                                            if (input == null ||
                                                input.Count == 0)
                                                return result;
                                
                                            double tolerance =
                                                Math.Max(
                                                    Symbol.PipSize * 2,
                                                    atr *
                                                    Math.Max(
                                                        0.02,
                                                        SmartLevelClusterAtr));
                                
                                            for (int i = 0;
                                                 i < input.Count;
                                                 i++)
                                            {
                                                Level current =
                                                    input[i];
                                
                                                Level match =
                                                    result.FirstOrDefault(
                                                        x =>
                                                            Math.Abs(
                                                                x.Price -
                                                                current.Price) <=
                                                            tolerance);
                                
                                                if (match == null)
                                                {
                                                    result.Add(current);
                                                    continue;
                                                }
                                
                                                int matchHits =
                                                    Math.Max(
                                                        1,
                                                        match.Hits);
                                
                                                int currentHits =
                                                    Math.Max(
                                                        1,
                                                        current.Hits);
                                
                                                // The first source is the authoritative representative because
                                                // BuildTargetLevels orders inputs by descending source score.
                                                // Confluence updates score/hits only; it must never fabricate a
                                                // price or overwrite source provenance.
                                                match.Score =
                                                    Clamp(
                                                        Math.Max(
                                                            match.Score,
                                                            current.Score) +
                                                        Math.Min(
                                                            20,
                                                            Math.Min(
                                                                match.Score,
                                                                current.Score) *
                                                            0.25),
                                                        0,
                                                        100);
                                
                                                match.Hits =
                                                    matchHits +
                                                    currentHits;
                                
                                                // Preserve match.Price, Kind, Timeframe, Age and
                                                // SourceAgeMinutes from the authoritative source.
                                            }
                                
                                            return
                                                result
                                                .OrderByDescending(
                                                    x => x.Score)
                                                .ToList();
                                        }
    }
}
