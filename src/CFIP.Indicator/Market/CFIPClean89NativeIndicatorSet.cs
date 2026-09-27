// Migrated from CFIP-PRO v89.
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using cAlgo.API;
using cAlgo.API.Indicators;
using cAlgo.API.Internals;

namespace cAlgo
{
        public sealed class CFIPClean89NativeIndicatorSet
        {
            public Bars Bars { get; private set; }
            public ExponentialMovingAverage Fast { get; private set; }
            public ExponentialMovingAverage Slow { get; private set; }
            public AverageTrueRange Atr { get; private set; }
            public RelativeStrengthIndex Rsi { get; private set; }
            public DirectionalMovementSystem Dms { get; private set; }
            public ExponentialMovingAverage MacdFast { get; private set; }
            public ExponentialMovingAverage MacdSlow { get; private set; }
    
            public CFIPClean89NativeIndicatorSet(
                Bars bars,
                ExponentialMovingAverage fast,
                ExponentialMovingAverage slow,
                AverageTrueRange atr,
                RelativeStrengthIndex rsi,
                DirectionalMovementSystem dms,
                ExponentialMovingAverage macdFast,
                ExponentialMovingAverage macdSlow)
            {
                Bars = bars;
                Fast = fast;
                Slow = slow;
                Atr = atr;
                Rsi = rsi;
                Dms = dms;
                MacdFast = macdFast;
                MacdSlow = macdSlow;
            }
        }
}
