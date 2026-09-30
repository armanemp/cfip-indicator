// ============================================================================
// CFIP Indicator — AlertEngine.cs
// ============================================================================

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
        private void EmitContextAlerts(
                            int closedM5)
                        {
                            if (_lastContextM5 ==
                                closedM5 ||
                                _m5Frame == null)
                                return;
                
                            _lastContextM5 =
                                closedM5;
                
                            if (AlertOnBos &&
                                (_m5Frame.StructureBull ||
                                 _m5Frame.StructureBear))
                            {
                                if (ShowContextEventMarker)
                                {
                                    int bar =
                                        MapM5ToChart(
                                            closedM5,
                                            Bars.Count - 1);
                
                                    DrawIcon(
                                        P + "BOS_MARKER",
                                        _m5Frame.StructureBull
                                            ? ChartIconType.UpArrow
                                            : ChartIconType.DownArrow,
                                        bar,
                                        _m5Frame.StructureBull
                                            ? Bars.LowPrices[bar]
                                            : Bars.HighPrices[bar],
                                        _m5Frame.StructureBull
                                            ? BuyArrowColor
                                            : SellArrowColor);
                                }
                
                                int direction =
                                    _m5Frame.StructureBull
                                        ? 1
                                        : -1;
                
                                SendUnifiedAlert(
                                    "BOS|" +
                                    closedM5 +
                                    "|" +
                                    direction,
                                    "CFIP BOS | " +
                                    (direction == 1
                                        ? "BUY"
                                        : "SELL"),
                                    direction,
                                    false);
                            }
                
                            if (AlertOnMssChoch &&
                                (_m5Frame.MssBull ||
                                 _m5Frame.MssBear ||
                                 _m5Frame.ChochBull ||
                                 _m5Frame.ChochBear))
                            {
                                int direction =
                                    _m5Frame.MssBull ||
                                    _m5Frame.ChochBull
                                        ? 1
                                        : -1;
                
                                SendUnifiedAlert(
                                    "MSS|" +
                                    closedM5 +
                                    "|" +
                                    direction,
                                    "CFIP MSS/CHOCH | " +
                                    (direction == 1
                                        ? "BUY"
                                        : "SELL"),
                                    direction,
                                    false);
                            }
                
                            if (AlertOnLiquiditySweep &&
                                (_m5Frame.LiquidityBull ||
                                 _m5Frame.LiquidityBear))
                            {
                                int direction =
                                    _m5Frame.LiquidityBull
                                        ? 1
                                        : -1;
                
                                SendUnifiedAlert(
                                    "SWEEP|" +
                                    closedM5 +
                                    "|" +
                                    direction,
                                    "CFIP LIQUIDITY SWEEP | " +
                                    (direction == 1
                                        ? "BUY"
                                        : "SELL"),
                                    direction,
                                    false);
                            }
                
                            if (AlertOnEarlySetup &&
                                _prediction != null &&
                                _prediction.DirectionalShare >=
                                MinimumEarlyConfidence &&
                                _prediction.AbsoluteStrength >=
                                Math.Max(1, MinimumEarlyConfidence) &&
                                _prediction.DirectionalShare <
                                MinimumConfidence &&
                                _lastEarlyAlertM5 !=
                                closedM5)
                            {
                                SendUnifiedAlert(
                                    "EARLY|" +
                                    closedM5 +
                                    "|" +
                                    _prediction.Direction,
                                    _prediction.Reason,
                                    _prediction.Direction,
                                    false);
                
                                _lastEarlyAlertM5 =
                                    closedM5;
                            }
                        }
    }
}
