using cAlgo.API;

namespace cAlgo
{
    public partial class CFIPIndicator : Indicator
    {
        private readonly IndependentEvidenceFusionCalculator
            _independentEvidenceFusionCalculator =
                new IndependentEvidenceFusionCalculator();

        private int IndependentEvidence(
            int direction)
        {
            return _independentEvidenceFusionCalculator.Calculate(
                _m5Frame,
                direction);
        }
    }
}
