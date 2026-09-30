namespace cAlgo
{
    internal sealed class DailyLossEvaluation
    {
        internal bool DataReady { get; }
        internal bool LimitHit { get; }
        internal bool Locked { get; }
        internal double LossAmount { get; }
        internal double LossPercent { get; }
        internal double DailyNetPnl { get; }
        internal double RealizedNetProfit { get; }
        internal double FloatingPnlChange { get; }
        internal double NetCashFlow { get; }
        internal bool UsedEquityFallback { get; }
        internal string Reason { get; }

        internal DailyLossEvaluation(
            bool dataReady,
            bool limitHit,
            bool locked,
            double lossAmount,
            double lossPercent,
            double dailyNetPnl,
            double realizedNetProfit,
            double floatingPnlChange,
            double netCashFlow,
            bool usedEquityFallback,
            string reason)
        {
            DataReady = dataReady;
            LimitHit = limitHit;
            Locked = locked;
            LossAmount = lossAmount;
            LossPercent = lossPercent;
            DailyNetPnl = dailyNetPnl;
            RealizedNetProfit = realizedNetProfit;
            FloatingPnlChange = floatingPnlChange;
            NetCashFlow = netCashFlow;
            UsedEquityFallback = usedEquityFallback;
            Reason = reason ?? "";
        }

        internal static DailyLossEvaluation Unavailable(
            string reason)
        {
            return new DailyLossEvaluation(
                false,
                false,
                false,
                0,
                0,
                0,
                0,
                0,
                0,
                false,
                reason);
        }
    }
    }
}
