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
            if (frame == null ||
                frame.Direction == 0)
                return new DecisionFrameContribution(0, 0, 0);

            DecisionFrameContribution result =
                _calculator.Calculate(
                    frame.BullScore,
                    frame.BearScore,
                    frame.Quality,
                    weight,
                    frame.Evidence);

            return new DecisionFrameContribution(
                result.Bull,
                result.Bear,
                frame.Direction == 0 ? 0 : 1);
        }
    }
}
