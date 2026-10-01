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

            if (target <= 0 ||
                candidates == null ||
                atr <= 0)
                return;

            Level best = null;
            double bestDistance = double.MaxValue;

            for (int i = 0; i < candidates.Count; i++)
            {
                Level candidate =
                    candidates[i];

                if (candidate == null)
                    continue;

                double distance =
                    Math.Abs(
                        candidate.Price -
                        target);

                if (distance <= atr * 0.15 &&
                    distance < bestDistance)
                {
                    best =
                        candidate;

                    bestDistance =
                        distance;
                }
            }

            if (best != null)
            {
                source =
                    BuildTargetSourceIdentity(best);

                quality =
                    CalculateTargetQuality(best);
            }
        }

        private void ApplySelectedTargetMeta(
            List<Level> selected,
            int stage,
            double target,
            out string source,
            out int quality)
        {
            source = "";
            quality = 0;

            if (!IsFinitePositive(target))
                return;

            Level selectedSource =
                selected != null &&
                stage >= 0 &&
                stage < selected.Count
                    ? selected[stage]
                    : null;

            if (selectedSource != null &&
                IsFinitePositive(selectedSource.Price) &&
                Math.Abs(
                    selectedSource.Price -
                    target) <=
                Math.Max(
                    Symbol.TickSize,
                    Symbol.PipSize * 0.25))
            {
                source =
                    BuildTargetSourceIdentity(
                        selectedSource);

                quality =
                    CalculateTargetQuality(
                        selectedSource);

                return;
            }

            // A target without a selected source is the deliberate RR fallback.
            // Never infer a real source merely because another level is nearby.
            source = "SYNTHETIC_RR";
            quality = 55;
        }

        private string BuildTargetSourceIdentity(
            Level sourceLevel)
        {
            if (sourceLevel == null)
                return "";

            string kind =
                string.IsNullOrWhiteSpace(sourceLevel.Kind)
                    ? "TARGET"
                    : sourceLevel.Kind.Trim();

            string timeframe =
                string.IsNullOrWhiteSpace(sourceLevel.Timeframe)
                    ? "UNKNOWN"
                    : sourceLevel.Timeframe.Trim();

            return
                kind +
                "@" +
                timeframe;
        }

        private int CalculateTargetQuality(
            Level sourceLevel)
        {
            if (sourceLevel == null)
                return 0;

            int quality =
                ClampInt(
                    (int)Math.Round(
                        sourceLevel.Score),
                    0,
                    100);

            if (sourceLevel.Age <= 5)
                quality =
                    Math.Min(
                        100,
                        quality + 5);

            return quality;
        }
    }
}
