using System;

namespace cAlgo
{
    /// <summary>
    /// Immutable sample owned by the aggressive-flow analyzer.
    /// Direction is a quote-midpoint movement proxy: +1 buy, -1 sell, 0 neutral.
    /// It is not executed trade volume.
    /// </summary>
    internal readonly struct AggressiveFlowSample
    {
        internal AggressiveFlowSample(DateTime utc, int direction)
        {
            Utc = utc;
            Direction = direction;
        }

        internal DateTime Utc { get; }
        internal int Direction { get; }
    }
}
