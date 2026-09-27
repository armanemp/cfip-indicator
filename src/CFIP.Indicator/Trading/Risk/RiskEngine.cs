// ============================================================================
// CFIP Indicator — RiskEngine.cs
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
        private bool DailyLossLimitHit(
                            DateTime nowUtc)
                        {
                            if (!EnableDailyLossLimit)
                                return false;
                
                            if (_dailyLossBaselineDate.Date !=
                                nowUtc.Date)
                            {
                                _dailyLossBaselineDate =
                                    nowUtc.Date;
                
                                _dailyStartEquity =
                                    Account.Equity;
                
                                _dailyLossLimitAlerted =
                                    false;
                            }
                
                            if (_dailyStartEquity <= 0)
                                return false;
                
                            double lossPercent =
                                (_dailyStartEquity -
                                 Account.Equity) /
                                _dailyStartEquity *
                                100.0;
                
                            if (lossPercent <
                                Math.Max(
                                    0.5,
                                    MaximumDailyLossPercent))
                                return false;
                
                            if (!_dailyLossLimitAlerted)
                            {
                                _dailyLossLimitAlerted =
                                    true;
                
                                SendUnifiedAlert(
                                    "DAILYLOSS|" +
                                    nowUtc.Date.ToString(
                                        "yyyyMMdd"),
                                    "Daily loss limit reached (" +
                                    lossPercent.ToString("F2") +
                                    "% >= " +
                                    MaximumDailyLossPercent.ToString(
                                        "F2") +
                                    "%) - new auto-trade entries are blocked for the " +
                                    "rest of the day. Open positions are left untouched.",
                                    0,
                                    true);
                            }
                
                            return true;
                        }
        
        private double EffectiveAutoRiskPercent()
                        {
                            double baseRisk =
                                Math.Max(
                                    0.05,
                                    RiskPercentEquity);
                
                            if (!UseSmartRiskScaling)
                                return baseRisk;
                
                            return ClampDouble(
                                baseRisk * SuitabilityRiskMultiplier(),
                                0.05,
                                baseRisk);
                        }
        
        private double EffectiveAggressiveRiskPercent()
                        {
                            double baseRisk =
                                Math.Max(
                                    0.05,
                                    AggressiveRiskPercentEquity);
                
                            if (!UseSmartRiskScaling)
                                return baseRisk;
                
                            return ClampDouble(
                                baseRisk * SuitabilityRiskMultiplier(),
                                0.05,
                                baseRisk);
                        }
        
        private double AdjustVolumeForMargin(
                            TradeType tradeType,
                            double volume)
                        {
                            if (!IsFinitePositive(volume) ||
                                !UseAutoMarginGuard)
                                return volume;
                
                            try
                            {
                                double freeMargin =
                                    Math.Max(
                                        0,
                                        Account.FreeMargin);
                
                                double usage =
                                    Math.Max(
                                        10,
                                        Math.Min(
                                            100,
                                            MaxAutoMarginUsagePercent -
                                            Math.Max(
                                                0,
                                                Math.Min(
                                                    40,
                                                    MarginBufferPercent))));
                
                                double allowed =
                                    freeMargin *
                                    usage /
                                    100.0;
                
                                if (allowed <= 0)
                                    return 0;
                
                                double estimated =
                                    Symbol.GetEstimatedMargin(
                                        tradeType,
                                        volume);
                
                                if (!IsFinitePositive(estimated) ||
                                    estimated <= allowed)
                                    return Symbol.NormalizeVolumeInUnits(
                                        volume,
                                        RoundingMode.Down);
                
                                double reduced =
                                    Symbol.NormalizeVolumeInUnits(
                                        volume *
                                        allowed /
                                        estimated,
                                        RoundingMode.Down);
                
                                while (reduced >= Symbol.VolumeInUnitsMin)
                                {
                                    double check =
                                        Symbol.GetEstimatedMargin(
                                            tradeType,
                                            reduced);
                
                                    if (!IsFinitePositive(check) ||
                                        check <= allowed)
                                        break;
                
                                    reduced =
                                        Symbol.NormalizeVolumeInUnits(
                                            reduced -
                                            Symbol.VolumeInUnitsStep,
                                            RoundingMode.Down);
                                }
                
                                return reduced >= Symbol.VolumeInUnitsMin
                                    ? reduced
                                    : 0;
                            }
                            catch (Exception ex)
                            {
                                Print(
                                    "CFIP margin sizing failed: {0}",
                                    ex.Message);
                                return 0;
                            }
                        }
        
        private double CalculateVolume(
                            double stopPips)
                        {
                            try
                            {
                                double volume;
                
                                if (SizingMode ==
                                    SizingMode.FixedLots)
                                {
                                    volume =
                                        Symbol.QuantityToVolumeInUnits(
                                            Math.Max(
                                                0.001,
                                                FixedLots));
                                }
                                else
                                {
                                    double riskAmount =
                                        Math.Max(
                                            0,
                                            Account.Equity) *
                                        EffectiveAutoRiskPercent() /
                                        100.0;
                
                                    if (riskAmount <= 0)
                                        return 0;
                
                                    volume =
                                        Symbol.VolumeForFixedRisk(
                                            riskAmount,
                                            stopPips,
                                            RoundingMode.Down);
                                }
                
                                if (!IsFinitePositive(
                                        volume))
                                    return 0;
                
                                volume =
                                    Symbol.NormalizeVolumeInUnits(
                                        volume,
                                        RoundingMode.Down);
                
                                if (volume <
                                    Symbol.VolumeInUnitsMin)
                                    return 0;
                
                                if (volume >
                                    Symbol.VolumeInUnitsMax)
                                {
                                    volume =
                                        Symbol.NormalizeVolumeInUnits(
                                            Symbol.VolumeInUnitsMax,
                                            RoundingMode.Down);
                                }
                
                                return volume;
                            }
                            catch (Exception ex)
                            {
                                Print(
                                    "CFIP volume calculation failed: {0}",
                                    ex.Message);
                
                                return 0;
                            }
                        }
        
        private int ManagedPositionCount()
                        {
                            int count = 0;
                
                            foreach (Position position in Positions)
                            {
                                if (IsManagedPosition(position))
                                    count++;
                            }
                
                            return count;
                        }
        
        private bool IsAutoPlanValid(
                            int direction,
                            double entry,
                            double stop,
                            double target)
                        {
                            return
                                (direction == 1 ||
                                 direction == -1) &&
                                IsFinitePositive(entry) &&
                                IsValidStop(
                                    direction,
                                    entry,
                                    stop) &&
                                IsValidTarget(
                                    direction,
                                    entry,
                                    target);
                        }
        
        private double CalculateAggressiveVolume(
                            double stopPips)
                        {
                            try
                            {
                                if (stopPips <= 0)
                                    return 0;
                
                                double amount =
                                    Math.Max(
                                        0,
                                        Account.Equity) *
                                    EffectiveAggressiveRiskPercent() /
                                    100.0;
                
                                if (amount <= 0)
                                    return 0;
                
                                double volume =
                                    Symbol.VolumeForFixedRisk(
                                        amount,
                                        stopPips,
                                        RoundingMode.Down);
                
                                return
                                    Symbol.NormalizeVolumeInUnits(
                                        volume,
                                        RoundingMode.Down);
                            }
                            catch
                            {
                                return 0;
                            }
                        }
    }
}
