// ============================================================================
// CFIP Indicator — ExecutionModelBuilder.cs
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
        private bool TryBuildExecutionZone(
                                    int closedM5,
                                    int direction,
                                    out double atr,
                                    out double market,
                                    out double low,
                                    out double high,
                                    out double ideal,
                                    out double tolerance,
                                    out double triggerBuffer,
                                    out string source,
                                    out int quality)
                                {
                                    atr = 0;
                                    market = 0;
                                    low = 0;
                                    high = 0;
                                    ideal = 0;
                                    tolerance = 0;
                                    triggerBuffer = 0;
                                    source = "NONE";
                                    quality = 0;

                                    if (_m5Bars == null ||
                                        closedM5 < 20 ||
                                        (direction != 1 && direction != -1))
                                        return false;
                        
                                    atr =
                                        Atr(
                                            _m5Bars,
                                            closedM5);
                        
                                    if (atr <= 0)
                                        return false;
                        
                                    market =
                                        NormalizePrice(
                                            direction == 1
                                                ? Symbol.Ask
                                                : Symbol.Bid);
                        
                                    Zone m5Fvg =
                                        FindNearestFvgForExecution(
                                            _m5Bars,
                                            closedM5,
                                            direction,
                                            atr,
                                            market);
                        
                                    Zone m5Ob =
                                        FindNearestOrderBlockForExecution(
                                            _m5Bars,
                                            closedM5,
                                            direction,
                                            atr,
                                            market);
                        
                                    Zone m15Fvg = null;
                                    Zone m15Ob = null;
                        
                                    int m15Index =
                                        ClosedIndex(
                                            _m15Bars,
                                            _m5Bars.OpenTimes[closedM5]);
                        
                                    double m15Atr =
                                        m15Index >= 10
                                            ? Atr(_m15Bars, m15Index)
                                            : 0;
                        
                                    if (m15Index >= 10 && m15Atr > 0)
                                    {
                                        m15Fvg =
                                            FindNearestFvgForExecution(
                                                _m15Bars,
                                                m15Index,
                                                direction,
                                                m15Atr,
                                                market);
                        
                                        m15Ob =
                                            FindNearestOrderBlockForExecution(
                                                _m15Bars,
                                                m15Index,
                                                direction,
                                                m15Atr,
                                                market);
                                    }
                        
                                                                        if (m5Fvg != null && m5Ob != null)
                                    {
                                        double overlapLow =
                                            Math.Max(m5Fvg.Low, m5Ob.Low);
                        
                                        double overlapHigh =
                                            Math.Min(m5Fvg.High, m5Ob.High);
                        
                                        if (overlapHigh >= overlapLow)
                                        {
                                            low = overlapLow;
                                            high = overlapHigh;
                                            source = "M5 FVG+OB";
                                            quality = 92;
                                        }
                                    }
                        
                                    if (quality == 0 && m5Fvg != null)
                                    {
                                        low = m5Fvg.Low;
                                        high = m5Fvg.High;
                                        source = "M5 FVG";
                                        quality = 84;
                                    }
                        
                                    if (quality == 0 && m5Ob != null)
                                    {
                                        low = m5Ob.Low;
                                        high = m5Ob.High;
                                        source = "M5 ORDER_BLOCK";
                                        quality = 86;
                                    }
                        
                                    if (quality == 0 &&
                                        m15Fvg != null &&
                                        m15Ob != null)
                                    {
                                        double overlapLow =
                                            Math.Max(m15Fvg.Low, m15Ob.Low);
                        
                                        double overlapHigh =
                                            Math.Min(m15Fvg.High, m15Ob.High);
                        
                                        if (overlapHigh >= overlapLow)
                                        {
                                            low = overlapLow;
                                            high = overlapHigh;
                                            source = "M15 FVG+OB";
                                            quality = 86;
                                        }
                                    }
                        
                                    if (quality == 0 && m15Fvg != null)
                                    {
                                        low = m15Fvg.Low;
                                        high = m15Fvg.High;
                                        source = "M15 FVG";
                                        quality = 78;
                                    }
                        
                                    if (quality == 0 && m15Ob != null)
                                    {
                                        low = m15Ob.Low;
                                        high = m15Ob.High;
                                        source = "M15 ORDER_BLOCK";
                                        quality = 80;
                                    }
                        
                                    if (quality > 0 &&
                                        m15Fvg != null &&
                                        m15Ob != null)
                                    {
                                        double overlapLow =
                                            Math.Max(
                                                low,
                                                Math.Max(m15Fvg.Low, m15Ob.Low));
                        
                                        double overlapHigh =
                                            Math.Min(
                                                high,
                                                Math.Min(m15Fvg.High, m15Ob.High));
                        
                                        if (overlapHigh > overlapLow)
                                        {
                                            low = overlapLow;
                                            high = overlapHigh;
                                            quality += 5;
                                            source += "+MTF";
                                        }
                                    }
                        
                                    if (quality == 0)
                                    {
                                        double swing =
                                            direction == 1
                                                ? FindSwingLowBelow(
                                                    _m5Bars,
                                                    closedM5,
                                                    market)
                                                : FindSwingHighAbove(
                                                    _m5Bars,
                                                    closedM5,
                                                    market);
                        
                                        if (IsFinitePositive(swing))
                                        {
                                            if (direction == 1)
                                            {
                                                low = swing;
                                                high =
                                                    swing +
                                                    atr *
                                                    Math.Max(
                                                        0.10,
                                                        ExecutionZoneAtr);
                                            }
                                            else
                                            {
                                                low =
                                                    swing -
                                                    atr *
                                                    Math.Max(
                                                        0.10,
                                                        ExecutionZoneAtr);
                                                high = swing;
                                            }
                        
                                            source = "M5 SWING";
                                            quality = 70;
                                        }
                                    }
                        
                                    if (!IsFinitePositive(low) ||
                                        !IsFinitePositive(high) ||
                                        high <= low)
                                        return false;
                        
                                    ideal =
                                        low +
                                        (high - low) * 0.50;
                        
                                    int retest =
                                        RetestQuality(
                                            _m5Bars,
                                            closedM5,
                                            direction);
                        
                                    if ((direction == 1 &&
                                         PremiumDiscountBias(
                                             _m5Bars,
                                             closedM5) == 1) ||
                                        (direction == -1 &&
                                         PremiumDiscountBias(
                                             _m5Bars,
                                             closedM5) == -1))
                                        quality += 5;
                        
                                    if (_m15Frame != null &&
                                        _m15Frame.Direction == direction)
                                        quality += 5;
                        
                                    if (_m30Frame != null &&
                                        _m30Frame.Direction == direction)
                                        quality += 3;
                        
                                    if (retest >= MinimumRetestQuality)
                                        quality += 5;
                        
                                    quality =
                                        ClampInt(
                                            quality,
                                            0,
                                            100);
                        
                                    tolerance =
                                        atr *
                                        Math.Max(
                                            0.02,
                                            ExecutionZoneAtr);
                        
                                    triggerBuffer =
                                        atr *
                                        Math.Max(
                                            0.01,
                                            PrecisionBreakoutBufferAtr);
                        
                                    ideal =
                                        low +
                                        (high - low) * 0.50;

                                    tolerance =
                                        atr *
                                        Math.Max(
                                            0.02,
                                            ExecutionZoneAtr);

                                    triggerBuffer =
                                        atr *
                                        Math.Max(
                                            0.01,
                                            PrecisionBreakoutBufferAtr);

                                    return true;
                                }
    }
}
