using System;
using cAlgo.API;

namespace cAlgo
{
    public partial class CFIPIndicator : Indicator
    {
        private void RenderPanelSignalPipelineRows(
            ref int slot,
            int contentWidth)
        {
            Decision decision =
                _decision;

            string direction =
                decision == null || decision.Direction == 0
                    ? "WAIT"
                    : decision.Direction == 1
                        ? "BUY"
                        : "SELL";

            bool decisionAllowed =
                decision != null &&
                decision.EntryAllowed;

            bool triggerReady =
                decision != null &&
                decision.TriggerReady;

            bool actionable =
                decision != null &&
                decision.ActionableNow;

            bool planActive =
                _plan != null;

            string reason =
                decision == null
                    ? "NO DECISION"
                    : actionable
                        ? "ACTIONABLE"
                        : !decisionAllowed
                            ? CompactText(
                                decision.BlockReason,
                                36)
                            : !triggerReady
                                ? "WAITING TRIGGER"
                                : CompactText(
                                    decision.ActionabilityReason,
                                    36);

            string pipeline =
                "PIPELINE " +
                direction +
                "  • D " +
                SignalPipelineYesNo(decisionAllowed) +
                "  • T " +
                SignalPipelineYesNo(triggerReady) +
                "  • A " +
                SignalPipelineYesNo(actionable) +
                "  • P " +
                SignalPipelineYesNo(planActive);

            AddPanelRow(
                ref slot,
                pipeline,
                actionable || planActive
                    ? TpLineColor
                    : PanelMutedTextColor,
                true,
                contentWidth);

            if (decision != null)
            {
                AddPanelRow(
                    ref slot,
                    "QUALITY  CONF " +
                    decision.Confidence +
                    "  • SMART " +
                    decision.SmartQuality +
                    "  • MTF " +
                    decision.TimeframeAgreement +
                    "  • EVID " +
                    decision.IndependentEvidence +
                    "  • STRUCT " +
                    decision.StructuralConfirmations +
                    "  • LOC " +
                    decision.EntryLocationQuality,
                    decision.Confidence >= MinimumConfidence &&
                    decision.SmartQuality >=
                        Math.Max(
                            MinimumSmartQuality,
                            SmartQualityThreshold)
                        ? PanelSecondaryTextColor
                        : PanelWarningColor,
                    false,
                    contentWidth);
            }

            AddPanelRow(
                ref slot,
                "PIPELINE REASON  " +
                reason,
                actionable
                    ? PanelSecondaryTextColor
                    : PanelWarningColor,
                false,
                contentWidth);
        }

        private string SignalPipelineYesNo(bool value)
        {
            return value ? "Y" : "N";
        }
    }
}
