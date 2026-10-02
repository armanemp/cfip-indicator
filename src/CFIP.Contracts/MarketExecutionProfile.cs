namespace CFIP.Contracts
{
    public sealed record MarketExecutionProfile(
        double MarketRangePips,
        double StopPips,
        double TargetPips,
        bool UseServerTakeProfitLadder,
        double Tp1Pips,
        double Tp1Volume,
        double Tp2Pips,
        double Tp2Volume,
        double FinalTpPips,
        double? BreakEvenTriggerPips,
        double? BreakEvenOffsetPips);
}
