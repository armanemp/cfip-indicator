using System;
using cAlgo.API;

namespace cAlgo
{
    public partial class CFIPIndicator : Indicator
    {
        private void RenderPanelOverviewExecutionRows(
            ref int slot,
            int contentWidth)
        {
            AddPanelRow(
                ref slot,
                "SMART RISK  " +
                EffectiveAutoRiskPercent().ToString("F2") +
                "%  •  SUIT " +
                SuitabilityRiskMultiplier().ToString("F2") +
                "x  •  OUTCOME " +
                OutcomeRiskMultiplier().ToString("F2") +
                "x",
                PanelSecondaryTextColor,
                false,
                contentWidth);

            AddPanelRow(
                ref slot,
                "AUTO EXEC  " +
                GetAutoTradingPanelState(),
                GetAutoTradingPanelColor(),
                true,
                contentWidth);

            AddPanelRow(
                ref slot,
                "CBOT LINK  " +
                CbotExecutionStatePanelText() +
                "  •  " +
                CbotExecutionScenarioPanelText(),
                CbotExecutionStatePanelColor(),
                false,
                contentWidth);

            AddPanelRow(
                ref slot,
                "BREAK-EVEN  " +
                CompactText(
                    string.IsNullOrWhiteSpace(
                        _lastBreakEvenDiagnostic)
                        ? "NOT EVALUATED"
                        : _lastBreakEvenDiagnostic,
                    88),
                string.IsNullOrWhiteSpace(
                    _lastBreakEvenDiagnostic) ||
                _lastBreakEvenDiagnostic.StartsWith(
                    "APPLIED",
                    StringComparison.OrdinalIgnoreCase)
                    ? PanelSecondaryTextColor
                    : PanelWarningColor,
                false,
                contentWidth);

            AddPanelRow(
                ref slot,
                OutcomeHistoryPanelText(),
                PanelSecondaryTextColor,
                false,
                contentWidth);

            AddPanelRow(
                ref slot,
                ExecutionTelemetryPanelText(),
                PanelSecondaryTextColor,
                false,
                contentWidth);

            AddPanelRow(
                ref slot,
                "EXEC MODE  " +
                (_executionModel == null
                    ? "NONE"
                    : ExecutionModeText(
                        _executionModel.Mode)),
                _executionModel != null &&
                (_executionModel.Mode ==
                    ExecutionMode.BreakoutMarket ||
                 _executionModel.Mode ==
                    ExecutionMode.RetestMarket)
                    ? EntryLineColor
                    : TriggerLineColor,
                true,
                contentWidth);

            AddPanelRow(
                ref slot,
                ExecutionLevelSemanticsText(
                    _executionModel),
                PanelSecondaryTextColor,
                false,
                contentWidth);

            if (_executionModel != null &&
                _executionModel.Direction != 0)
            {
                AddPanelRow(
                    ref slot,
                    "ENTRY RELATION  " +
                    GetExecutionRelationText(
                        _executionModel,
                        _executionModel.ActualEntry),
                    PanelSecondaryTextColor,
                    false,
                    contentWidth);
            }

            AddPanelRow(
                ref slot,
                "AUTO ORDERS  " +
                GetAutoOrdersPanelState(),
                GetAutoOrdersPanelColor(),
                false,
                contentWidth);

            if (UseDailyPivots)
            {
                AddPanelRow(
                    ref slot,
                    "PIVOT  " +
                    DailyPivotPanelText(),
                    PanelAccentColor,
                    false,
                    contentWidth);
            }
        }
    }
}
