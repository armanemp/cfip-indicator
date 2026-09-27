// ============================================================================
// CFIP Indicator — DecisionFeatures.cs
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
        private int FreshTriggerEvidence(
                            Bars bars,
                            int index,
                            int direction)
                        {
                            if (bars == null || index < 5)
                                return 0;
                
                            int evidence = 0;
                
                            double atr =
                                Atr(
                                    bars,
                                    index);
                
                            double body =
                                Math.Abs(
                                    bars.ClosePrices[index] -
                                    bars.OpenPrices[index]);
                
                            if (direction == 1 &&
                                bars.ClosePrices[index] >
                                bars.OpenPrices[index])
                                evidence++;
                
                            if (direction == -1 &&
                                bars.ClosePrices[index] <
                                bars.OpenPrices[index])
                                evidence++;
                
                            if (atr > 0 &&
                                body >=
                                atr *
                                MinimumTriggerBodyAtr)
                                evidence++;
                
                            if (direction == 1 &&
                                bars.ClosePrices[index] >
                                Highest(
                                    bars,
                                    Math.Max(
                                        0,
                                        index - 5),
                                    index - 1))
                                evidence++;
                
                            if (direction == -1 &&
                                bars.ClosePrices[index] <
                                Lowest(
                                    bars,
                                    Math.Max(
                                        0,
                                        index - 5),
                                    index - 1))
                                evidence++;
                
                            return evidence;
                        }
        
        private int StructuralSequence(
                            Bars bars,
                            int index,
                            int direction)
                        {
                            if (bars == null ||
                                index < 8)
                                return 0;
                
                            double atr =
                                Atr(
                                    bars,
                                    index);
                
                            int result = 0;
                
                            if (direction == 1)
                            {
                                if (BullStructure(
                                        bars,
                                        index,
                                        atr))
                                    result++;
                
                                if (BullMss(
                                        bars,
                                        index,
                                        atr))
                                    result++;
                
                                if (BullDisplacement(
                                        bars,
                                        index,
                                        atr))
                                    result++;
                            }
                            else
                            {
                                if (BearStructure(
                                        bars,
                                        index,
                                        atr))
                                    result++;
                
                                if (BearMss(
                                        bars,
                                        index,
                                        atr))
                                    result++;
                
                                if (BearDisplacement(
                                        bars,
                                        index,
                                        atr))
                                    result++;
                            }
                
                            return result;
                        }
        
        private int EntryLocationQuality(
                            Bars bars,
                            int index,
                            int direction)
                        {
                            if (bars == null ||
                                index < 10)
                                return 0;
                
                            double atr =
                                Atr(
                                    bars,
                                    index);
                
                            if (atr <= 0)
                                return 0;
                
                            int quality = 40;
                
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
                
                                if (price >=
                                        zone.Low -
                                        atr *
                                        ZoneProximityAtr &&
                                    price <=
                                        zone.High +
                                        atr *
                                        ZoneProximityAtr)
                                    quality += 30;
                
                                if (zone.Quality >= 80)
                                    quality += 15;
                            }
                
                            if (PremiumDiscountBias(
                                    bars,
                                    index) ==
                                direction)
                                quality += 10;
                
                            return ClampInt(
                                quality,
                                0,
                                100);
                        }
        
        private double ProxyExpectedValue(
                            int quality,
                            double rr)
                        {
                            double winRate =
                                Clamp(
                                    quality / 100.0,
                                    0.05,
                                    0.95);
                
                            return
                                winRate * rr -
                                (1.0 - winRate);
                        }
        
        private bool NoTradeRegimeBlocked(
                            string regime,
                            int quality)
                        {
                            if (quality <
                                NoTradeMinimumSmartQuality)
                                return true;
                
                            if (BlockCompressionRegime &&
                                regime == "COMPRESSION")
                                return true;
                
                            if (BlockWeakRangeTransition &&
                                (regime == "RANGE" ||
                                 regime == "TRANSITION") &&
                                quality <
                                SmartRegimeQualityFloor)
                                return true;
                
                            return false;
                        }
        
        private int CalibratedConfidence(
                            int baseConfidence,
                            int direction)
                        {
                            if (!UseEmpiricalCalibration ||
                                !EnableConfidenceCalibration ||
                                !EnableOutcomeTelemetry ||
                                !_directionSamples.ContainsKey(
                                    direction))
                                return baseConfidence;
                
                            int totalSamples =
                                _directionSamples.Values.Sum();
                
                            if (totalSamples <
                                Math.Max(
                                    1,
                                    CalibrationMinimumSamples))
                                return baseConfidence;
                
                            int samples =
                                _directionSamples[direction];
                
                            if (samples <
                                CalibrationDirectionalMinimumSamples)
                                return baseConfidence;
                
                            int wins =
                                _directionWins.ContainsKey(
                                    direction)
                                    ? _directionWins[direction]
                                    : 0;
                
                            double rate =
                                samples <= 0
                                    ? 0.5
                                    : (double)wins /
                                      samples;
                
                            int adjustment =
                                ClampInt(
                                    (int)Math.Round(
                                        (rate - 0.5) *
                                        2.0 *
                                        CalibrationMaxConfidenceAdjustment),
                                    -CalibrationMaxConfidenceAdjustment,
                                    CalibrationMaxConfidenceAdjustment);
                
                            return ClampInt(
                                baseConfidence +
                                adjustment,
                                0,
                                100);
                        }
    }
}
