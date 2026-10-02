using cAlgo.API;

namespace cAlgo
{
    public partial class CFIPIndicator : Indicator
    {
        private TradeActionabilityResult EvaluateTradeActionability(
            int closedM5,
            int direction,
            OpportunityLane lane,
            string regime,
            ExecutionModel execution,
            TradeSetupPreview preview)
        {
            TradeActionabilityEvaluationState state;
            TradeActionabilityResult failure;

            if (!TryPrepareTradeActionability(
                    closedM5,
                    direction,
                    lane,
                    regime,
                    execution,
                    preview,
                    out state,
                    out failure))
                return failure;

            return EvaluatePreparedTradeActionability(
                state);
        }
    }
}
