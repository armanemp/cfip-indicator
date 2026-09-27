// ============================================================================
// CFIP Indicator — DecisionEvidence.cs
// One responsibility per module. Behavioral parity with v73 is preserved.
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
        private int TimeframeAgreement(
                                    int direction,
                                    DateTime reference)
                                {
                                    if (direction == 0)
                                        return 0;
                        
                                    Frame[] frames =
                                    {
                                        _m5Frame,
                                        _m15Frame,
                                        _m30Frame,
                                        _h1Frame,
                                        _h4Frame,
                                        _d1Frame,
                                        _w1Frame
                                    };
                        
                                    Bars[] bars =
                                    {
                                        _m5Bars,
                                        _m15Bars,
                                        _m30Bars,
                                        _h1Bars,
                                        _h4Bars,
                                        _d1Bars,
                                        _w1Bars
                                    };
                        
                                    double[] weights =
                                    {
                                        Math.Max(0, M5Weight),
                                        Math.Max(0, M15Weight),
                                        Math.Max(0, M30Weight),
                                        Math.Max(0, H1Weight),
                                        Math.Max(0, H4Weight),
                                        Math.Max(0, D1Weight),
                                        Math.Max(0, W1Weight)
                                    };
                        
                                    bool[] enabled =
                                    {
                                        true,
                                        true,
                                        M30Weight > 0,
                                        H1Weight > 0,
                                        H4Weight > 0,
                                        D1Weight > 0,
                                        SmartWeeklyContext &&
                                        W1Weight > 0
                                    };
                        
                                    double totalWeight = 0;
                                    double alignedWeight = 0;
                        
                                    for (int i = 0;
                                         i < frames.Length;
                                         i++)
                                    {
                                        if (!enabled[i] ||
                                            weights[i] <= 0 ||
                                            frames[i] == null ||
                                            bars[i] == null ||
                                            frames[i].Quality <= 0)
                                            continue;
                        
                                        int closedIndex =
                                            ClosedIndex(
                                                bars[i],
                                                reference);
                        
                                        if (closedIndex < 0 ||
                                            frames[i].Index != closedIndex)
                                            continue;
                        
                                        // A neutral timeframe is not evidence against the selected
                                        // direction; it contributes no alignment weight.
                                        if (frames[i].Direction == 0)
                                            continue;
                        
                                        totalWeight +=
                                            weights[i];
                        
                                        if (frames[i].Direction == direction)
                                            alignedWeight +=
                                                weights[i];
                                    }
                        
                                    return
                                        totalWeight <= 0
                                            ? 0
                                            : ClampInt(
                                                (int)Math.Round(
                                                    100.0 *
                                                    alignedWeight /
                                                    totalWeight),
                                                0,
                                                100);
                                }
        
        private int IndependentEvidence(
                                    int direction)
                                {
                                    if (_m5Frame == null ||
                                        direction == 0)
                                        return 0;
                        
                                    int count = 0;
                        
                                    if (direction == 1)
                                    {
                                        if (_m5Frame.StructureBull) count++;
                                        if (_m5Frame.LiquidityBull) count++;
                                        if (_m5Frame.FvgBull) count++;
                                        if (_m5Frame.ObBull) count++;
                                        if (_m5Frame.DisplacementBull) count++;
                                        if (_m5Frame.MomentumBull) count++;
                                        if (_m5Frame.VolumeBull) count++;
                                        if (_m5Frame.MacdBull) count++;
                                        if (_m5Frame.VwapBull) count++;
                                        if (_m5Frame.VolatilityBull) count++;
                                    }
                                    else
                                    {
                                        if (_m5Frame.StructureBear) count++;
                                        if (_m5Frame.LiquidityBear) count++;
                                        if (_m5Frame.FvgBear) count++;
                                        if (_m5Frame.ObBear) count++;
                                        if (_m5Frame.DisplacementBear) count++;
                                        if (_m5Frame.MomentumBear) count++;
                                        if (_m5Frame.VolumeBear) count++;
                                        if (_m5Frame.MacdBear) count++;
                                        if (_m5Frame.VwapBear) count++;
                                        if (_m5Frame.VolatilityBear) count++;
                                    }
                        
                                    return count;
                                }
        
        private int StructuralConfirmations(
                                    int direction)
                                {
                                    if (_m5Frame == null ||
                                        direction == 0)
                                        return 0;
                        
                                    int count = 0;
                        
                                    if (direction == 1)
                                    {
                                        if (_m5Frame.StructureBull) count++;
                                        if (_m5Frame.MssBull ||
                                            _m5Frame.ChochBull) count++;
                                        if (_m5Frame.DisplacementBull) count++;
                                        if (_m15Frame != null &&
                                            _m15Frame.StructureBull) count++;
                                        if (_h1Frame != null &&
                                            _h1Frame.StructureBull) count++;
                                        if (_h4Frame != null &&
                                            _h4Frame.StructureBull) count++;
                                    }
                                    else
                                    {
                                        if (_m5Frame.StructureBear) count++;
                                        if (_m5Frame.MssBear ||
                                            _m5Frame.ChochBear) count++;
                                        if (_m5Frame.DisplacementBear) count++;
                                        if (_m15Frame != null &&
                                            _m15Frame.StructureBear) count++;
                                        if (_h1Frame != null &&
                                            _h1Frame.StructureBear) count++;
                                        if (_h4Frame != null &&
                                            _h4Frame.StructureBear) count++;
                                    }
                        
                                    return count;
                                }
        
        private bool CanAcceptConfirmedDirection(
                                    int direction,
                                    int closedM5)
                                {
                                    if (direction == 0)
                                        return false;
                        
                                    if (!PreventRapidDirectionFlip)
                                        return true;
                        
                                    if (_lastConfirmedDirection == 0 ||
                                        _lastConfirmedM5 < 0)
                                        return true;
                        
                                    int elapsed =
                                        closedM5 -
                                        _lastConfirmedM5;
                        
                                    if (elapsed < 0)
                                        return false;
                        
                                    if (direction ==
                                        _lastConfirmedDirection)
                                    {
                                        if (_plan != null)
                                            return false;
                        
                                        if (_lastExitM5 >= 0 &&
                                            closedM5 -
                                            _lastExitM5 <
                                            Math.Max(
                                                0,
                                                ExitReentryCooldownM5))
                                            return false;
                        
                                        return true;
                                    }
                        
                                    if (elapsed <
                                        Math.Max(
                                            1,
                                            OppositeSignalCooldownM5))
                                        return false;
                        
                                    if (_plan != null &&
                                        !AllowOppositeWhileActive)
                                        return false;
                        
                                    if (RequireM15ReversalForOpposite)
                                    {
                                        if (_m15Frame == null ||
                                            _m15Frame.Direction !=
                                            direction)
                                            return false;
                        
                                        bool structural =
                                            direction == 1
                                                ? (_m15Frame.MssBull ||
                                                   _m15Frame.ChochBull)
                                                : (_m15Frame.MssBear ||
                                                   _m15Frame.ChochBear);
                        
                                        bool force =
                                            direction == 1
                                                ? (_m15Frame.DisplacementBull &&
                                                   _m15Frame.LiquidityBull)
                                                : (_m15Frame.DisplacementBear &&
                                                   _m15Frame.LiquidityBear);
                        
                                        if (!(RequireReversalForce
                                                ? structural && force
                                                : structural || force))
                                            return false;
                        
                                        int m15Index =
                                            ClosedIndex(
                                                _m15Bars,
                                                _m5Bars.OpenTimes[closedM5]);
                        
                                        if (m15Index >= 0 &&
                                            !StableDirection(
                                                _m15Bars,
                                                m15Index,
                                                direction,
                                                Math.Max(
                                                    1,
                                                    SmartFlipConfirmationBars)))
                                            return false;
                                    }
                        
                                    int m5Evidence = 0;
                        
                                    if (_m5Frame != null)
                                    {
                                        if (direction == 1)
                                        {
                                            if (_m5Frame.MssBull) m5Evidence++;
                                            if (_m5Frame.ChochBull) m5Evidence++;
                                            if (_m5Frame.DisplacementBull) m5Evidence++;
                                            if (_m5Frame.LiquidityBull) m5Evidence++;
                                        }
                                        else
                                        {
                                            if (_m5Frame.MssBear) m5Evidence++;
                                            if (_m5Frame.ChochBear) m5Evidence++;
                                            if (_m5Frame.DisplacementBear) m5Evidence++;
                                            if (_m5Frame.LiquidityBear) m5Evidence++;
                                        }
                                    }
                        
                                    return
                                        m5Evidence >=
                                        Math.Max(
                                            1,
                                            MinimumOppositeM5Structure);
                                }
    }
}
