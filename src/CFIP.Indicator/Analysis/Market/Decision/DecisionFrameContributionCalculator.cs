namespace cAlgo
{
    internal sealed class DecisionFrameContributionCalculator
    {
        public DecisionFrameContribution Calculate(
            Frame frame,
            double weight)
        {
            if (frame == null ||
                frame.Quality <= 0 ||
                weight <= 0)
            {
                return new DecisionFrameContribution(
                    0,
                    0,
                    0);
            }

            double scale = weight / 10.0;

            return new DecisionFrameContribution(
                frame.BullScore * scale,
                frame.BearScore * scale,
                frame.Direction == 0 ? 0 : 1);
        }
    }
}
