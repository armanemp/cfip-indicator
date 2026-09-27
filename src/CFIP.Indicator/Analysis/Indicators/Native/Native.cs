using System;
using System.Collections.Generic;
using System.Linq;
using cAlgo.API;
using cAlgo.API.Indicators;
using cAlgo.API.Internals;

namespace cAlgo
{
    internal sealed class Native
                    {
                        public Bars Bars;
                        public ExponentialMovingAverage Fast;
                        public ExponentialMovingAverage Slow;
                        public AverageTrueRange Atr;
                        public RelativeStrengthIndex Rsi;
                        public DirectionalMovementSystem Dms;
                        public ExponentialMovingAverage MacdFast;
                        public ExponentialMovingAverage MacdSlow;
                    }
}
