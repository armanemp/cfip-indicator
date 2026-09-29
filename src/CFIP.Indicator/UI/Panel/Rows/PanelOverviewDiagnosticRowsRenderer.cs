using System;
using cAlgo.API;

namespace cAlgo
{
    public partial class CFIPIndicator : Indicator
    {
        private void RenderPanelOverviewDiagnosticRows(
            ref int slot,
            int contentWidth)
        {
            bool tradingPermission =
                HasTradingPermission();

            AddPanelRow(
                ref slot,
                "PERMISSION  •  " +
                (tradingPermission
                    ? "TRADING ALLOWED"
                    : "TRADING NOT GRANTED"),
                tradingPermission
                    ? TpLineColor
                    : PanelWarningColor,
                true,
                contentWidth);

            if (ShowSpreadDiagnostics)
            {
                double spreadPips =
                    Math.Max(
                        0,
                        (Symbol.Ask - Symbol.Bid) /
                        Math.Max(
                            Symbol.PipSize,
                            1e-9));

                double spreadAtrRatio = 0;

                if (_m5Frame != null &&
                    _m5Frame.Atr > 0)
                {
                    spreadAtrRatio =
                        (Symbol.Ask - Symbol.Bid) /
                        _m5Frame.Atr;
                }

                AddPanelRow(
                    ref slot,
                    "SPREAD  " +
                    spreadPips.ToString("F1") +
                    " pips  •  ATR " +
                    spreadAtrRatio.ToString("F3"),
                    spreadPips > 0 &&
                    UseSpreadFilter &&
                    _m5Frame != null &&
                    _m5Frame.Atr > 0 &&
                    (Symbol.Ask - Symbol.Bid) /
                    _m5Frame.Atr >
                    MaximumSpreadAtr
                        ? PanelWarningColor
                        : PanelMutedTextColor,
                    false,
                    contentWidth);
            }

            if (ShowEngineStatus)
            {
                AddPanelRow(
                    ref slot,
                    "ENGINE  " +
                    _status +
                    "  •  " +
                    CalculationAgeText(),
                    CalculationAgeIsStale()
                        ? PanelWarningColor
                        : PanelMutedTextColor,
                    false,
                    contentWidth);
            }
        }

        private bool CalculationAgeIsStale()
        {
            if (_lastCalculationCompletedUtc == DateTime.MinValue)
                return true;

            return
                (Server.TimeInUtc -
                 _lastCalculationCompletedUtc).TotalSeconds > 3;
        }

        private string CalculationAgeText()
        {
            if (_lastCalculationCompletedUtc == DateTime.MinValue)
                return "CALC NO CYCLE";

            double age =
                Math.Max(
                    0,
                    (Server.TimeInUtc -
                     _lastCalculationCompletedUtc).TotalSeconds);

            return
                "CALC " +
                age.ToString("F1") +
                "s AGO";
        }
        }
    }
}
