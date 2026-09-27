// Migrated from CFIP-PRO v89.
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using cAlgo.API;
using cAlgo.API.Indicators;
using cAlgo.API.Internals;

namespace cAlgo
{
        public sealed class CFIPClean89NativeIndicatorCatalog
        {
            private readonly IIndicatorsAccessor _indicators;
            private readonly List<CFIPClean89NativeIndicatorSet> _sets =
                new List<CFIPClean89NativeIndicatorSet>();
    
            public CFIPClean89NativeIndicatorCatalog(
                IIndicatorsAccessor indicators)
            {
                _indicators =
                    indicators ??
                    throw new ArgumentNullException("indicators");
            }
    
            public CFIPClean89NativeIndicatorSet GetOrCreate(
                Bars bars,
                CFIPClean89ConfigSnapshot configuration)
            {
                if (bars == null)
                    return null;
    
                for (int i = 0; i < _sets.Count; i++)
                {
                    if (ReferenceEquals(_sets[i].Bars, bars))
                        return _sets[i];
                }
    
                int fastPeriod = Math.Max(2, configuration.Get("FastEma", 21));
                int slowPeriod = Math.Max(fastPeriod + 1, configuration.Get("SlowEma", 55));
                int atrPeriod = Math.Max(2, configuration.Get("AtrPeriod", 14));
                int rsiPeriod = Math.Max(2, configuration.Get("RsiPeriod", 14));
                int adxPeriod = Math.Max(2, configuration.Get("AdxPeriod", 14));
                int macdFastPeriod = Math.Max(2, configuration.Get("MacdFastPeriod", 12));
                int macdSlowPeriod = Math.Max(macdFastPeriod + 1, configuration.Get("MacdSlowPeriod", 26));
    
                try
                {
                    var set = new CFIPClean89NativeIndicatorSet(
                        bars,
                        _indicators.ExponentialMovingAverage(
                            bars.ClosePrices,
                            fastPeriod),
                        _indicators.ExponentialMovingAverage(
                            bars.ClosePrices,
                            slowPeriod),
                        _indicators.AverageTrueRange(
                            bars,
                            atrPeriod,
                            MovingAverageType.WilderSmoothing),
                        _indicators.RelativeStrengthIndex(
                            bars.ClosePrices,
                            rsiPeriod),
                        _indicators.DirectionalMovementSystem(
                            bars,
                            adxPeriod,
                            MovingAverageType.WilderSmoothing),
                        _indicators.ExponentialMovingAverage(
                            bars.ClosePrices,
                            macdFastPeriod),
                        _indicators.ExponentialMovingAverage(
                            bars.ClosePrices,
                            macdSlowPeriod));
    
                    _sets.Add(set);
                    return set;
                }
                catch
                {
                    return null;
                }
            }
        }
}
