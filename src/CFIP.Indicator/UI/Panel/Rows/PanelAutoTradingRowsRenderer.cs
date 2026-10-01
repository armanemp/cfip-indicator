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
        private void RenderPanelAutoTradingRows(
            ref int slot,
            int contentWidth)
        {
            AddPanelRow(
                ref slot,
                "AUTO TRADING  •  " +
                GetAutoTradingPanelState(),
                GetAutoTradingPanelColor(),
                true,
                contentWidth);

            if (AutoTradingEnabled &&
                !string.IsNullOrWhiteSpace(_autoExecutionBlockReason) &&
                !string.Equals(
                    _autoExecutionBlockReason,
                    "NOT EVALUATED",
                    StringComparison.OrdinalIgnoreCase))
            {
                AddPanelRow(
                    ref slot,
                    "AUTO TRADE DIAGNOSTIC  •  " +
                    CompactText(
                        _autoExecutionBlockReason,
                        100),
                    IsPanelExecutionRecoveryRequired(
                        _autoTradingState,
                        _autoExecutionBlockReason)
                        ? SlLineColor
                        : PanelWarningColor,
                    true,
                    contentWidth);
            }

            AddPanelRow(
                ref slot,
                "AUTO ORDERS  •  " +
                GetAutoOrdersPanelState(),
                GetAutoOrdersPanelColor(),
                true,
                contentWidth);

            PendingOrder managedPending =
                GetManagedPendingOrder();

            if (managedPending != null)
            {
                string pendingType =
                    managedPending.OrderType ==
                        PendingOrderType.Stop
                        ? "STOP"
                        : managedPending.OrderType ==
                          PendingOrderType.Limit
                            ? "LIMIT"
                            : "PENDING";

                AddPanelRow(
                    ref slot,
                    "AUTO ORDER DETAIL  •  " +
                    pendingType +
                    "  •  ENTRY " +
                    Price(
                        managedPending.TargetPrice) +
                    (managedPending.StopLoss.HasValue
                        ? "  •  SL " +
                          Price(
                              managedPending.StopLoss.Value)
                        : "") +
                    (managedPending.TakeProfit.HasValue
                        ? "  •  TP " +
                          Price(
                              managedPending.TakeProfit.Value)
                        : ""),
                    TriggerLineColor,
                    true,
                    contentWidth);
            }

            AddPanelRow(
                ref slot,
                "AUTO CONFIG  •  " +
                (ConfirmedSignalsOnly
                    ? "CONFIRMED ONLY"
                    : "PLAN ELIGIBLE") +
                "  •  " +
                (SizingMode ==
                    SizingMode.FixedLots
                    ? "FIXED " +
                      FixedLots.ToString("F2") +
                      " LOT"
                    : "RISK " +
                      RiskPercentEquity.ToString("F2") +
                      "%"),
                PanelSecondaryTextColor,
                false,
                contentWidth);

            AddPanelRow(
                ref slot,
                "AUTO PROTECTION  •  " +
                GetAutoProtectionPanelState(),
                GetAutoProtectionPanelColor(),
                false,
                contentWidth);

            if (!string.IsNullOrWhiteSpace(
                    _lastAlertMessage))
            {
                AddPanelRow(
                    ref slot,
                    "LAST ALERT  •  " +
                    CompactText(
                        _lastAlertMessage,
                        120),
                    _lastAlertCritical
                        ? (_lastAlertDirection == 1
                            ? BuyArrowColor
                            : _lastAlertDirection == -1
                                ? SellArrowColor
                                : PanelWarningColor)
                        : PanelSecondaryTextColor,
                    false,
                    contentWidth);
            }
        }
    }
}