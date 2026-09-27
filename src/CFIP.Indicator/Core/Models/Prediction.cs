using System;
using System.Collections.Generic;
using System.Linq;
using cAlgo.API;
using cAlgo.API.Indicators;
using cAlgo.API.Internals;

namespace cAlgo
{
    internal sealed class Prediction
                    {
                        public int Direction;
                        public ExecutionMode Mode;
                        public int Confidence;
                        public double Entry;
                        public double StopLoss;
                        public double ZoneLow;
                        public double ZoneHigh;
                        public double Trigger;
                        public double Target;
                        public double Target1;
                        public double Target2;
                        public double Target3;
                        public double Target4;
                        public string Reason;
                    }
}
