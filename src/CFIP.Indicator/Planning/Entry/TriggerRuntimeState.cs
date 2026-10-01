using System;

namespace cAlgo
{
    internal sealed class TriggerRuntimeState
    {
        public int DecisionM5 = -1;
        public int LiveM5Window = -1;
        public int ClosedM1 = -1;
        public int Direction;
        public int Score;
        public int RequiredScore;
        public bool Ready;
        public bool Latched;
        public int ConfirmedM1 = -1;
        public long ConfirmationRevision;
        public string Reason = "WAITING";
        public DateTime UpdatedUtc = DateTime.MinValue;

        public void Reset(
            int decisionM5,
            int direction)
        {
            DecisionM5 = decisionM5;
            LiveM5Window = -1;
            ClosedM1 = -1;
            Direction = direction;
            Score = 0;
            RequiredScore = 0;
            Ready = false;
            Latched = false;
            ConfirmedM1 = -1;
            Reason = "WAITING";
            UpdatedUtc = DateTime.MinValue;
        }
    }
}