using System;
using System.Collections.Generic;
using System.Linq;
using cAlgo.API;
using cAlgo.API.Indicators;
using cAlgo.API.Internals;

namespace cAlgo
{
    internal sealed class ExecutionModel
                    {
                        public int Direction;
                        public ExecutionMode Mode;
                        public double IdealEntry;
                        public double ActualEntry;
                        public double ZoneLow;
                        public double ZoneHigh;
                        public double Trigger;
                        public double Invalidation;
                        public int Quality;
                        public bool Ready;
                        public string Source;
                    }
}
