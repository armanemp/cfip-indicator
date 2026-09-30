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
                                
                                                match.Price =
                                                    NormalizePrice(
                                                        (match.Price *
                                                         matchHits +
                                                         current.Price *
                                                         currentHits) /
                                                        (matchHits +
                                                         currentHits));
                                
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
                                
                                                match.Age =
                                                    Math.Min(
                                                        match.Age,
                                                        current.Age);
                                
                                                if (current.Timeframe == "M15" ||
                                                    current.Timeframe == "M30" ||
                                                    current.Timeframe == "H1" ||
                                                    current.Timeframe == "H4" ||
                                                    current.Timeframe == "D1" ||
                                                    current.Timeframe == "W1")
                                                {
                                                    match.Timeframe =
                                                        current.Timeframe;
                                                    match.SourceAgeMinutes =
                                                        current.SourceAgeMinutes;
                                                }
                                
                                                if (match.Kind == "SWING" &&
                                                    current.Kind != "SWING")
                                                    match.Kind =
                                                        current.Kind;
                                            }
                                
                                            return
                                                result
                                                .OrderByDescending(
                                                    x => x.Score)
                                                .ToList();
                                        }
    }
}
