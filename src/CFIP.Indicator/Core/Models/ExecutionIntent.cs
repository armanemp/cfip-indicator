using System;
using System.Collections.Generic;
using System.Linq;
using cAlgo.API;
using cAlgo.API.Indicators;
using cAlgo.API.Internals;

namespace cAlgo
{
    internal sealed class ExecutionIntent
                {
                    public int Direction;
                    public DecisionPolicyMode Policy;
                    public ExecutionIntentKind Kind;
                    public double RequestedEntry;
                    public double Trigger;
                    public double ZoneLow;
                    public double ZoneHigh;
                    public double Stop;
                    public double Target;
                    public double StopPips;
                    public double TargetPips;
                    public double Volume;
                    public int CreatedM5;
                    public string Source;
                }
}
