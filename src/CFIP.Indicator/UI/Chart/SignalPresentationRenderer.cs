using System;
using cAlgo.API;

namespace cAlgo
{
    public partial class CFIPIndicator : Indicator
    {
        private void RenderNonActionableWatchState(
            SignalVisualSnapshot snapshot,
            int visualDirection,
            int hostBar)
        {
            if (!ShowEarlyWatch)
            {
                Chart.RemoveObject(
                    P + "REACTION_ARROW");
                RemoveStackedSignalArrows();
                return;
            }

            Chart.RemoveObject(
                P + "REACTION_ARROW");

            // Canonical MTF trend arrows are independent of decision
            // actionability. The calculation lifecycle owns their rendering.
            RemoveStackedSignalArrows();
        }

        private bool IsStrongWatchSnapshot(
            SignalVisualSnapshot snapshot)
        {
            if (snapshot == null)
                return false;

            return WatchReactionAlertRule.IsStrongWatch(
                snapshot.AuthoritativeDirection,
                snapshot.Confidence,
                snapshot.SmartQuality,
                snapshot.TimeframeAgreement,
                snapshot.IndependentEvidence,
                snapshot.StructuralConfirmations,
                MinimumConfidence,
                MinimumSmartQuality,
                SmartQualityThreshold,
                MinimumTimeframeAgreement,
                SmartMinimumTimeframeAgreement,
                MinimumIndependentEvidence,
                MinimumStructuralConfirmations);
        }

    }
}
