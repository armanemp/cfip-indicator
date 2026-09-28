using System.Collections.Generic;
using cAlgo.API;

namespace cAlgo
{
    public partial class CFIPIndicator : Indicator
    {
        private bool HasAnyHtfTargetLevel(
            List<Level> candidates)
        {
            if (candidates == null)
                return false;

            for (int i = 0;
                 i < candidates.Count;
                 i++)
            {
                string tf =
                    candidates[i].Timeframe;

                if (tf == "H1" ||
                    tf == "H4" ||
                    tf == "D1" ||
                    tf == "W1")
                    return true;
            }

            return false;
        }
    }
}
