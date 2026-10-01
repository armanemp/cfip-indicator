// CFIP Indicator — SmartThresholdPolicy.cs
// Adaptive smart-decision threshold policy.

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
            SmartThresholdResolution thresholds =
                SmartThresholdPolicyRule.ResolveSmartThresholds(
                    regime,
                    AdaptiveSmartThresholds,
                    MinimumSmartQuality,
                    MinimumSmartDirectionShare,
                    MinimumEdge,
                    SmartRegimeBuffer);

            qualityThreshold =
                thresholds.QualityThreshold;
            shareThreshold =
                thresholds.ShareThreshold;
            edgeThreshold =
                thresholds.EdgeThreshold;
        }

        private int SmartMinimumConsensusFloor()
        {
            return System.Math.Max(
                40,
                SmartConsensusThreshold - 12);
        }
    }
}
