// ============================================================================
// CFIP Indicator — RegimeFilter.cs
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
        // ============================================================
                
                        private int RetestQuality(
                            Bars bars,
                            int index,
                            int direction)
                        {
                            if (bars == null ||
                                index < 15)
                                return 0;
                
                            double atr =
                                Atr(
                                    bars,
                                    index);
                
                            if (atr <= 0)
                                return 0;
                
                            int quality = 35;
                
                            Zone zone =
                                FindNearestOpposingZone(
                                    bars,
                                    index,
                                    direction,
                                    atr);
                
                            if (zone != null)
                            {
                                double price =
                                    bars.ClosePrices[index];
                
                                double tolerance =
                                    atr *
                                    Math.Max(
                                        ZoneProximityAtr,
                                        RetestZoneToleranceAtr);
                
                                bool near =
                                    price >= zone.Low - tolerance &&
                                    price <= zone.High + tolerance;
                
                                if (near)
                                    quality += 25;
                
                                if (zone.Quality >=
                                    FastReversalMinimumZoneQuality)
                                    quality += 10;
                
                                int first =
                                    Math.Max(
                                        2,
                                        index -
                                        RetestLookbackBars);
                
                                bool touched = false;
                
                                for (int i = first;
                                     i <= index;
                                     i++)
                                {
                                    if (bars.HighPrices[i] >= zone.Low &&
                                        bars.LowPrices[i] <= zone.High)
                                    {
                                        touched = true;
                                        break;
                                    }
                                }
                
                                if (touched)
                                    quality += 15;
                
                                if (RequireRetestCloseConfirmation &&
                                    !near)
                                    quality -= 15;
                            }
                
                            double body =
                                Math.Abs(
                                    bars.ClosePrices[index] -
                                    bars.OpenPrices[index]);
                
                            if (body >=
                                atr *
                                Math.Max(
                                    RetestRejectionBodyAtr,
                                    MinimumTriggerBodyAtr))
                                quality += 10;
                
                            bool directional =
                                direction == 1
                                    ? bars.ClosePrices[index] >
                                      bars.OpenPrices[index]
                                    : bars.ClosePrices[index] <
                                      bars.OpenPrices[index];
                
                            if (directional)
                                quality += 5;
                
                            int firstRecent =
                                Math.Max(
                                    2,
                                    index -
                                    RetestMaxBarsAfterDisplacement);
                
                            for (int i = firstRecent;
                                 i <= index;
                                 i++)
                            {
                                double bodySize =
                                    Math.Abs(
                                        bars.ClosePrices[i] -
                                        bars.OpenPrices[i]);
                
                                if (bodySize >=
                                        atr * DisplacementAtr &&
                                    (direction == 1
                                        ? bars.ClosePrices[i] >
                                          bars.OpenPrices[i]
                                        : bars.ClosePrices[i] <
                                          bars.OpenPrices[i]))
                                {
                                    quality += 10;
                                    break;
                                }
                            }
                
                            return ClampInt(
                                quality,
                                0,
                                100);
                        }
        
        private string DetectRegime(
                            Bars bars,
                            int index)
                        {
                            double atr =
                                Atr(
                                    bars,
                                    index);
                
                            double oldAtr =
                                Atr(
                                    bars,
                                    Math.Max(
                                        20,
                                        index - 10));
                
                            if (atr <= 0 ||
                                oldAtr <= 0)
                                return "UNKNOWN";
                
                            double ratio =
                                atr /
                                oldAtr;
                
                            double adx =
                                Adx(
                                    bars,
                                    index);
                
                            if (ratio >= 1.30)
                                return "EXPANSION";
                
                            if (ratio <= 0.80)
                                return "COMPRESSION";
                
                            if (adx < AdxMinimum)
                                return "RANGE";
                
                            if (Math.Abs(
                                    Ema(
                                        bars,
                                        index,
                                        true) -
                                    Ema(
                                        bars,
                                        index,
                                        false)) <=
                                atr * 0.10)
                                return "TRANSITION";
                
                            return "TREND";
                        }
        
        private int RegimeQuality(
                            string regime,
                            Bars bars,
                            int index)
                        {
                            double adx =
                                Adx(
                                    bars,
                                    index);
                
                            if (regime == "TREND")
                                return ClampInt(
                                    (int)Math.Round(
                                        60 +
                                        Math.Min(
                                            35,
                                            adx)),
                                    0,
                                    100);
                
                            if (regime == "EXPANSION")
                                return ClampInt(
                                    (int)Math.Round(
                                        65 +
                                        Math.Min(
                                            30,
                                            adx * 0.5)),
                                    0,
                                    100);
                
                            if (regime == "RANGE")
                                return 52;
                
                            if (regime == "COMPRESSION")
                                return 45;
                
                            if (regime == "TRANSITION")
                                return 48;
                
                            return 35;
                        }
        
        private bool CooldownBlocked(
                            int currentM5)
                        {
                            if (CooldownM5Bars <= 0 ||
                                _lastSignalM5 < 0)
                                return false;
                
                            return currentM5 -
                                   _lastSignalM5 <
                                   CooldownM5Bars;
                        }
    }
}
