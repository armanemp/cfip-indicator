using cAlgo.API;

namespace cAlgo
{
    public partial class CFIPIndicator : Indicator
    {
        private Frame AnalyzeFrame(
            Bars bars,
            int index)
        {
            Frame frame =
                BuildMarketFrameEvidence(
                    bars,
                    index);

            return ScoreMarketFrame(
                frame);
        }
    }
}
