using System;
using cAlgo.API;

namespace cAlgo
{
    public partial class CFIPIndicator : Indicator
    {
        private void RenderPanelTradePlanLiveRows(
            ref int slot,
            int contentWidth)
        {
            double liveRR =
                _plan.Risk > 0
                    ? (_plan.Direction == 1
                        ? _lastMarket -
                          _plan.Entry
                        : _plan.Entry -
                          _lastMarket) /
                      _plan.Risk
                    : 0;

            int exitPressure =
                CalculateSmartExitPressure(
                    _lastMarket,
                    liveRR);

            AddPanelRow(
                ref slot,
                "LIVE  RR " +
                liveRR.ToString("F2") +
                "  •  TP HIT " +
                _tp1Hit +
                "/" +
                _tp2Hit +
                "/" +
                _tp3Hit +
                "/" +
                _tp4Hit,
                liveRR >= 0
                    ? TpLineColor
                    : SlLineColor,
                true,
                contentWidth);

            Position managedPosition =
                GetManagedPosition();

            if (managedPosition != null)
            {
                AddPanelRow(
                    ref slot,
                    "POSITION  •  " +
                    (managedPosition.TradeType ==
                         TradeType.Buy
                        ? "BUY"
                        : "SELL") +
                    "  #" +
                    managedPosition.Id +
                    "  •  VOL " +
                    managedPosition.VolumeInUnits.ToString("F0") +
                    "  •  P/L " +
                    managedPosition.NetProfit.ToString("F2") +
                    "  •  SL " +
                    (managedPosition.StopLoss.HasValue
                        ? Price(
                            managedPosition.StopLoss.Value)
                        : "-") +
                    "  •  TP " +
                    (managedPosition.TakeProfit.HasValue
                        ? Price(
                            managedPosition.TakeProfit.Value)
                        : "-") +
                    "  •  " +
                    BrokerTargetStageText(
                        managedPosition.TakeProfit.HasValue
                            ? managedPosition.TakeProfit.Value
                            : 0),
                    managedPosition.NetProfit >= 0
                        ? TpLineColor
                        : SlLineColor,
                    true,
                    contentWidth);
            }

            AddPanelRow(
                ref slot,
                "SMART EXIT  " +
                GetSmartExitMode() +
                "  •  PRESSURE " +
                exitPressure,
                exitPressure >=
                    SmartExitPressureThreshold
                    ? SlLineColor
                    : exitPressure >=
                      LiveReactionWatchThreshold
                        ? PanelWarningColor
                        : TpLineColor,
                true,
                contentWidth);
        }
    }
}
