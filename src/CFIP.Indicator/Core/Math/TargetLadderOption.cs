using System;
using System.Collections.Generic;

namespace cAlgo
{
    /// <summary>
    /// Platform-neutral candidate used by the coherent TP ladder selector.
    /// CandidateIndex points back to the authoritative Level collection.
    /// </summary>
    internal readonly struct TargetLadderOption
    {
        public int CandidateIndex { get; }
        public double Price { get; }
        public double Score { get; }

        public TargetLadderOption(
            int candidateIndex,
            double price,
            double score)
        {
            CandidateIndex = candidateIndex;
            Price = price;
            Score = score;
        }
    }
}
