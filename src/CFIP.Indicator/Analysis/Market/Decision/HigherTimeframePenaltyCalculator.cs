using cAlgo.API;

namespace cAlgo
{
    public partial class CFIPIndicator : Indicator
    {
        private int HigherTimeframeConfidencePenalty(int direction)
        {
            if (direction == 0 ||
                HigherTfPenalty <= 0)
                return 0;

            bool h1Against =
                _h1Frame != null &&
                _h1Frame.Direction != 0 &&
                _h1Frame.Direction != direction;

            bool h4Against =
                _h4Frame != null &&
                _h4Frame.Direction != 0 &&
                _h4Frame.Direction != direction;

            bool d1Against =
                _d1Frame != null &&
                _d1Frame.Direction != 0 &&
                _d1Frame.Direction != direction;

            if (!h1Against &&
                !h4Against &&
                !d1Against)
                return 0;

            return
                HigherTfPenalty +
                (d1Against
                    ? HigherTfPenalty / 2
                    : 0);
        }
    }
}