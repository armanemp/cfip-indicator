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
            _panelLiveRow = slot;
            _panelPositionRow = -1;
            _panelExitRow = -1;

            double liveRR =
                RiskRewardMathRule.DirectionalProgressRR(
                    _plan.Direction,
                    _plan.Entry,
                    _lastMarket,
                    _plan.Risk,
                    Symbol.PipSize);

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
                _panelPositionRow = slot;
                AddPanelRow(
                    ref slot,
                    "POSITION  •  " +
                    DirectionText(
                        managedPosition.TradeType) +
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

            _panelExitRow = slot;

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
