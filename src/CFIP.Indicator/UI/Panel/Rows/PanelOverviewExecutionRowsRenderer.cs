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
                "%  •  " +
                SuitabilityRiskMultiplier().ToString("F2") +
                "x BASE",
                PanelSecondaryTextColor,
                false,
                contentWidth);

            AddPanelRow(
                ref slot,
                "AUTO EXEC  " +
                (AutoTradingEnabled
                    ? "ON"
                    : "OFF") +
                "  •  " +
                CompactText(
                    _autoExecutionBlockReason,
                    72),
                AutoTradingEnabled &&
                string.Equals(
                    _autoExecutionBlockReason,
                    "READY TO SUBMIT",
                    StringComparison.OrdinalIgnoreCase)
                    ? TpLineColor
                    : PanelSecondaryTextColor,
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
                (AutomaticOrdersEnabled
                    ? "ON"
                    : "OFF") +
                "  •  " +
                CompactText(
                    _autoOrdersBlockReason,
                    72),
                AutomaticOrdersEnabled &&
                string.Equals(
                    _autoOrdersBlockReason,
                    "ORDER PLACED",
                    StringComparison.OrdinalIgnoreCase)
                    ? TpLineColor
                    : PanelSecondaryTextColor,
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
