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
                
                            bool structuralBull =
                                _m5Frame.StructureBull ||
                                _m5Frame.MssBull ||
                                _m5Frame.ChochBull;

                            bool structuralBear =
                                _m5Frame.StructureBear ||
                                _m5Frame.MssBear ||
                                _m5Frame.ChochBear;

                            int structuralDirection =
                                structuralBull && !structuralBear
                                    ? 1
                                    : structuralBear && !structuralBull
                                        ? -1
                                        : 0;

                            if (structuralDirection != 0 &&
                                (StructuralEvidenceRule.HasCanonicalStructuralEvent(
                                     structuralDirection == 1
                                         ? _m5Frame.StructureBull
                                         : _m5Frame.StructureBear,
                                     structuralDirection == 1
                                         ? _m5Frame.MssBull
                                         : _m5Frame.MssBear,
                                     structuralDirection == 1
                                         ? _m5Frame.ChochBull
                                         : _m5Frame.ChochBear)))
                            {
                                bool sendBos =
                                    AlertOnBos &&
                                    (structuralDirection == 1
                                        ? _m5Frame.StructureBull
                                        : _m5Frame.StructureBear);

                                bool sendMssChoch =
                                    AlertOnMssChoch &&
                                    (structuralDirection == 1
                                        ? _m5Frame.MssBull ||
                                          _m5Frame.ChochBull
                                        : _m5Frame.MssBear ||
                                          _m5Frame.ChochBear);

                                // Structure/MSS/CHOCH are labels of one causal
                                // break. Emit one user-facing event even when
                                // several labels are simultaneously true.
                                if (sendBos || sendMssChoch)
                                {
                                    if (ShowContextEventMarker &&
                                        sendBos)
                                    {
                                        int bar =
                                            MapM5ToChart(
                                                closedM5,
                                                Bars.Count - 1);

                                        DrawIcon(
                                            P + "BOS_MARKER",
                                            structuralDirection == 1
                                                ? ChartIconType.UpArrow
                                                : ChartIconType.DownArrow,
                                            bar,
                                            structuralDirection == 1
                                                ? Bars.LowPrices[bar]
                                                : Bars.HighPrices[bar],
                                            structuralDirection == 1
                                                ? BuyArrowColor
                                                : SellArrowColor);
                                    }

                                    string alertType =
                                        sendBos
                                            ? "BOS"
                                            : "MSS";

                                    SendUnifiedAlert(
                                        alertType +
                                        "|" +
                                        closedM5 +
                                        "|" +
                                        structuralDirection,
                                        "CFIP " +
                                        (sendBos
                                            ? "BOS"
                                            : "MSS/CHOCH") +
                                        " | " +
                                        (structuralDirection == 1
                                            ? "BUY"
                                            : "SELL"),
                                        structuralDirection,
                                        false);
                                }
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
                                if (SendUnifiedAlert(
                                        "EARLY|" +
                                        closedM5 +
                                        "|" +
                                        _prediction.Direction,
                                        _prediction.Reason,
                                        _prediction.Direction,
                                        false))
                                {
                                    _lastEarlyAlertM5 =
                                        closedM5;
                                }
                            }
                        }
    }
}
