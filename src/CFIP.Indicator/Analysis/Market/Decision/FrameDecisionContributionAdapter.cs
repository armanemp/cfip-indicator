namespace cAlgo
{
    internal sealed class FrameDecisionContributionAdapter
    {
        private readonly DecisionFrameContributionCalculator _calculator =
            new DecisionFrameContributionCalculator();

        public DecisionFrameContribution Calculate(
            Frame frame,
            double weight)
        {
            if (frame == null)
                return new DecisionFrameContribution(0, 0, 0);

            DecisionFrameContribution result =
                _calculator.Calculate(
                    frame.BullScore,
                    frame.BearScore,
                    frame.Quality,
                    weight);

            return new DecisionFrameContribution(
                result.Bull,
                result.Bear,
                frame.Direction == 0 ? 0 : 1);
        }
    }
}
