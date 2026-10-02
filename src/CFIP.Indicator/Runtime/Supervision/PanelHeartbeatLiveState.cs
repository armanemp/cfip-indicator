using System;
using cAlgo.API;
using cAlgo.API.Internals;

namespace cAlgo
{
    public partial class CFIPIndicator : Indicator
    {
        private int PanelContentWidth()
        {
            return Math.Max(
                200,
                PanelWidth -
                2 * Math.Max(0, PanelPadding) -
                2 * Math.Max(0, PanelBorderThickness));
        }

        private void UpdatePanelHeartbeatLiveRows()
        {
            if (!ShowUnifiedPanel ||
                _panel == null ||
                _panelRows.Count == 0 ||
                _plan == null)
                return;

            int width = PanelContentWidth();

            double liveMarket =
                _plan.Direction == 1
                    ? Symbol.Bid
                    : Symbol.Ask;

            if (IsFinitePositive(liveMarket))
                _lastMarket = liveMarket;

            double liveRR =
                RiskRewardMathRule.DirectionalProgressRR(
                    _plan.Direction,
                    _plan.Entry,
                    _lastMarket,
                    _plan.Risk,
                    Symbol.PipSize);

            if (_panelLiveRow >= 0)
            {
                SetPanelRow(
                    _panelLiveRow,
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
                    width);
            }

            Position position =
                GetManagedPosition();

            if (_panelPositionRow >= 0)
            {
                string text =
                    position == null
                        ? ""
                        : "POSITION  •  " +
                          (position.TradeType == TradeType.Buy
                              ? "BUY"
                              : "SELL") +
                          "  #" +
                          position.Id +
                          "  •  VOL " +
                          position.VolumeInUnits.ToString("F0") +
                          "  •  P/L " +
                          position.NetProfit.ToString("F2") +
                          "  •  SL " +
                          (position.StopLoss.HasValue
                              ? Price(position.StopLoss.Value)
                              : "-") +
                          "  •  TP " +
                          (position.TakeProfit.HasValue
                              ? Price(position.TakeProfit.Value)
                              : "-") +
                          "  •  " +
                          BrokerTargetStageText(
                              position.TakeProfit.HasValue
                                  ? position.TakeProfit.Value
                                  : 0);

                SetPanelRow(
                    _panelPositionRow,
                    text,
                    position == null
                        ? PanelSecondaryTextColor
                        : position.NetProfit >= 0
                            ? TpLineColor
                            : SlLineColor,
                    true,
                    width);
            }

            if (_panelExitRow >= 0)
            {
                int pressure =
                    CalculateSmartExitPressure(
                        _lastMarket,
                        liveRR);

                SetPanelRow(
                    _panelExitRow,
                    "SMART EXIT  " +
                    GetSmartExitMode() +
                    "  •  PRESSURE " +
                    pressure,
                    pressure >= SmartExitPressureThreshold
                        ? SlLineColor
                        : pressure >= LiveReactionWatchThreshold
                            ? PanelWarningColor
                            : TpLineColor,
                    true,
                    width);
            }
        }
    }
}