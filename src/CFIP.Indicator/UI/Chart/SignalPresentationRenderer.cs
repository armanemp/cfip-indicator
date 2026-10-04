using System;
using cAlgo.API;

namespace cAlgo
{
    public partial class CFIPIndicator : Indicator
    {
        private bool ShouldRenderCanonicalMtfTrendArrows(
            SignalVisualSnapshot snapshot)
        {
            if (snapshot == null ||
                !ShowSignalArrow)
                return false;

            bool tradeStateOwnsArrows =
                snapshot.PlanActive ||
                snapshot.ActionableNow ||
                snapshot.DecisionEntryAllowed;

            if (tradeStateOwnsArrows)
                return true;

            if (!ShowEarlyWatch)
                return false;

            // Early arrows are intentionally subordinate to the confirmed
            // trade-state visuals. ShowEarlyArrow controls only the weak/early
            // MTF tier; stronger MTF states remain visible when ShowSignalArrow
            // is enabled.
            bool weakOrEarly =
                string.Equals(
                    snapshot.MtfTrendStrengthTier,
                    "WEAK",
                    StringComparison.OrdinalIgnoreCase) ||
                snapshot.MtfTrendStrengthLevel <= 3;

            return !weakOrEarly || ShowEarlyArrow;
        }

        private void RenderNonActionableWatchState(
            SignalVisualSnapshot snapshot,
            int visualDirection,
            int hostBar)
        {
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
