using System;

namespace cAlgo
{
    internal readonly struct SmartThresholdResolution
    {
        public int QualityThreshold { get; }
        public int ShareThreshold { get; }
        public int EdgeThreshold { get; }

        public SmartThresholdResolution(
            int qualityThreshold,
            int shareThreshold,
            int edgeThreshold)
        {
            QualityThreshold = qualityThreshold;
            ShareThreshold = shareThreshold;
            EdgeThreshold = edgeThreshold;
        }
    }

    internal static class SmartThresholdPolicyRule
    {
        public static SmartThresholdResolution Resolve(
            string regime,
            bool adaptiveSmartThresholds,
            int minimumSmartQuality,
            int minimumSmartDirectionShare,
            int minimumEdge,
            int smartRegimeBuffer)
        {
            int qualityThreshold =
                Math.Max(
                    40,
                    Math.Min(
                        95,
                        minimumSmartQuality));

            int shareThreshold =
                ExecutionThresholdPolicy.NormalizeDirectionShare(
                    minimumSmartDirectionShare);

            int edgeThreshold =
                Math.Max(
                    4,
                    Math.Min(
                        30,
                        minimumEdge));

            if (!adaptiveSmartThresholds)
            {
                return new SmartThresholdResolution(
                    qualityThreshold,
                    shareThreshold,
                    edgeThreshold);
            }

            int b =
                Math.Max(
                    0,
                    smartRegimeBuffer);

            switch (
                regime ??
                MarketRegimeIdentity.Unknown)
            {
                case MarketRegimeIdentity.Trend:
                case MarketRegimeIdentity.Expansion:
                    qualityThreshold -= b;
                    shareThreshold -= Math.Max(1, b / 3);
                    edgeThreshold -= Math.Max(1, b / 3);
                    break;

                case MarketRegimeIdentity.Range:
                    qualityThreshold += Math.Max(1, b / 2);
                    shareThreshold += Math.Max(1, b / 3);
                    edgeThreshold += Math.Max(1, b / 3);
                    break;

                case MarketRegimeIdentity.Compression:
                    qualityThreshold += b;
                    shareThreshold += Math.Max(1, b / 2);
                    edgeThreshold += Math.Max(1, b / 2);
                    break;
            }

            qualityThreshold =
                Math.Max(
                    40,
                    Math.Min(
                        95,
                        qualityThreshold));

            shareThreshold =
                ExecutionThresholdPolicy.NormalizeDirectionShare(
                    shareThreshold);

            edgeThreshold =
                Math.Max(
                    4,
                    Math.Min(
                        30,
                        edgeThreshold));

            return new SmartThresholdResolution(
                qualityThreshold,
                shareThreshold,
                edgeThreshold);
        }
    }
}
