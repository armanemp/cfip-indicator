using System.Collections.Generic;

namespace cAlgo
{
    public partial class CFIPIndicator
    {
        private const int MaxSignalEvaluationTraceHistory = 256;

        private readonly List<SignalEvaluationTrace>
            _signalEvaluationTraces =
                new List<SignalEvaluationTrace>();

        private readonly HashSet<long>
            _signalTraceMemoryKeys =
                new HashSet<long>();
    }
}
