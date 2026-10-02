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
                    public double MarketRangePips;
                    public bool UseServerTakeProfitLadder;
                    public double Tp1Pips;
                    public double Tp1Volume;
                    public double Tp2Pips;
                    public double Tp2Volume;
                    public double FinalTpPips;
                    public double? BreakEvenTriggerPips;
                    public double? BreakEvenOffsetPips;
                }
}
