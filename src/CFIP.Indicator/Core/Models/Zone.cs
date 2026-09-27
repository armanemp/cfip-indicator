using System;
using System.Collections.Generic;
using System.Linq;
using cAlgo.API;
using cAlgo.API.Indicators;
using cAlgo.API.Internals;

namespace cAlgo
{
    internal sealed class Zone
                    {
                        public double Low;
                        public double High;
                        public int Direction;
                        public string Kind;
                        public int Age;
                        public int Quality;
                    }
}
