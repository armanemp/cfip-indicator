// CFIP Indicator — StructuralStopCandidateSelector.cs
// Structural stop candidate selection orchestration.

using System;
using System.Collections.Generic;
using cAlgo.API;

namespace cAlgo
{
    public partial class CFIPIndicator : Indicator
    {
        private double SelectStructuralStopCandidate(
            List<Level> candidates,
            int closedM5,
            int direction,
            double entry,
            double atr,
            out string source,
            out int quality)
        {
            source = "NONE";
            quality = 0;

            if (candidates.Count == 0)
                return 0;

            GetStructuralStopRiskBounds(
                atr,
                out double minRiskAtr,
                out double maxRiskAtr);

            Level best = null;
            double bestScore = double.MinValue;
            int bestQuality = 0;
            string bestSource = "NONE";

            int minimumQuality =
                Math.Max(
                    Math.Max(40, MinimumStructuralStopQuality),
                    SmartStopQuality);

            for (int i = 0; i < candidates.Count; i++)
            {
                Level candidate = candidates[i];

                if (!TryEvaluateStructuralStopCandidate(
                        candidate,
                        closedM5,
                        direction,
                        entry,
                        atr,
                        minRiskAtr,
                        maxRiskAtr,
                        out double stop,
                        out double score))
                    continue;

                if (score > bestScore)
                {
                    bestScore = score;
                    best = candidate;
                    bestQuality =
                        ClampInt(
                            (int)Math.Round(score),
                            0,
                            100);
                    bestSource =
                        candidate.Kind +
                        "@" +
                        candidate.Timeframe;
                }
            }

            if (best == null ||
                bestQuality < minimumQuality)
                return 0;

            double selectedStop =
                ResolveStructuralStopPrice(
                    best,
                    closedM5,
                    atr,
                    direction);

            source = bestSource;
            quality = bestQuality;

            return NormalizePrice(selectedStop);
        }
    }
}
