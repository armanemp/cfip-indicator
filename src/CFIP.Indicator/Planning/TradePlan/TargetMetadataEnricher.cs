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
        private void ApplyExactTargetMeta(
            Level selectedSource,
            double target,
            out string source,
            out int quality)
        {
            source = "";
            quality = 0;

            if (!IsFinitePositive(target))
                return;

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

            source = "SYNTHETIC_RR";
            quality = 55;
        }

        private void ApplyResolvedTargetMeta(
            List<Level> selected,
            int stage,
            double target,
            double previousTarget,
            string previousSource,
            int previousQuality,
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
                IsFinitePositive(selectedSource.Price))
            {
                ApplyExactTargetMeta(
                    selectedSource,
                    target,
                    out source,
                    out quality);

                if (!string.IsNullOrWhiteSpace(source) &&
                    source != "SYNTHETIC_RR")
                    return;
            }

            if (IsFinitePositive(previousTarget) &&
                Math.Abs(
                    previousTarget -
                    target) <=
                Math.Max(
                    Symbol.TickSize,
                    Symbol.PipSize * 0.25) &&
                !string.IsNullOrWhiteSpace(previousSource))
            {
                source = previousSource;
                quality = ClampInt(previousQuality, 0, 100);
                return;
            }

            // A resolved target without an authoritative selected source is
            // explicitly synthetic. Never infer a real source from proximity.
            source = "SYNTHETIC_RR";
            quality = 55;
        }

        private void ApplySelectedTargetMeta(
            List<Level> selected,
            int stage,
            double target,
            out string source,
            out int quality)
        {
            ApplyResolvedTargetMeta(
                selected,
                stage,
                target,
                0,
                "",
                0,
                out source,
                out quality);
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
