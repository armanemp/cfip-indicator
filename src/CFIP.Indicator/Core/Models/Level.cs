using System;
using System.Collections.Generic;
using System.Linq;
using cAlgo.API;
using cAlgo.API.Indicators;
using cAlgo.API.Internals;

namespace cAlgo
{
    internal sealed class Level
                    {
                        public double Price;
                        public double Score;
                        public string Kind;
                        public string Timeframe;
                        public int Age;
                        public int Hits;
                    }
}
