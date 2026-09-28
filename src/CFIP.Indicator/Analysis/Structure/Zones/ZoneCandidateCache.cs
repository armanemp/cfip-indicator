using System.Collections.Generic;
using cAlgo.API;

namespace cAlgo
{
    internal sealed class ZoneCandidateCache
    {
        public Bars Bars;
        public int Index;
        public double Atr;

        public readonly List<Zone> BullFvgs =
            new List<Zone>();

        public readonly List<Zone> BearFvgs =
            new List<Zone>();

        public readonly List<Zone> BullOrderBlocks =
            new List<Zone>();

        public readonly List<Zone> BearOrderBlocks =
            new List<Zone>();
    }
}
