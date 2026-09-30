// CFIP Indicator — SmartThresholdPolicy.cs
// Adaptive smart-decision threshold policy.

using System;
using cAlgo.API;

namespace cAlgo
{
    public partial class CFIPIndicator : Indicator
    {
        private void GetAdaptiveSmartThresholds(
            string regime,
            out int qualityThreshold,
            out int shareThreshold,
            out int edgeThreshold)
        {
            qualityThreshold =
                Math.Max(
                    40,
                    Math.Min(
                        95,
                        MinimumSmartQuality));

            shareThreshold =
                ExecutionThresholdPolicy.NormalizeDirectionShare(
                    MinimumSmartDirectionShare);

            edgeThreshold =
                Math.Max(
                    4,
                    Math.Min(
                        30,
                        MinimumEdge));

            if (!AdaptiveSmartThresholds)
                return;

            int b =
                Math.Max(
                    0,
                    SmartRegimeBuffer);

            switch (
                regime ??
                "UNKNOWN")
            {
                case "TREND":
                case "EXPANSION":
                    qualityThreshold -= b;
                    shareThreshold -= Math.Max(1, b / 3);
                    edgeThreshold -= Math.Max(1, b / 3);
                    break;

                case "REVERSAL":
                    qualityThreshold -= Math.Max(1, b / 2);
                    break;

                case "RANGE":
                    qualityThreshold += Math.Max(1, b / 2);
                    shareThreshold += Math.Max(1, b / 3);
                    edgeThreshold += Math.Max(1, b / 3);
                    break;

                case "COMPRESSION":
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
        }

        private int SmartMinimumConsensusFloor()
        {
            return Math.Max(
                40,
                SmartConsensusThreshold - 12);
        }
    }
}
