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

        private Frame AnalyzeFrameCached(
            Frame current,
            Bars bars,
            int index)
        {
            if (bars == null ||
                index < 0 ||
                index >= bars.Count)
                return null;

            if (current != null &&
                ReferenceEquals(
                    current.Bars,
                    bars) &&
                current.Index == index)
                return current;

            return AnalyzeFrame(
                bars,
                index);
        }
    }
}
