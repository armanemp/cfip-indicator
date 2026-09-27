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
                                                        "AUTO ORDER  •  " +
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
                                    
                                                if (AutoTradingEnabled &&
                                                    (AutoBrokerProtection ||
                                                     AutoProtectBrokerPositions))
                                                {
                                                    AddPanelRow(
                                                        ref slot,
                                                        "AUTO PROTECTION  •  " +
                                                        GetAutoProtectionPanelState(),
                                                        TpLineColor,
                                                        false,
                                                        contentWidth);
                                                }
                                    
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
