using System;

namespace cAlgo
{
    internal sealed class AggressiveEntryPolicy
    {
        private const int RequiredQualifyingSamples = 2;

        private int _reactionM5 = -1;
        private int _direction;
        private DateTime _lastSampleUtc = DateTime.MinValue;
        private int _qualifyingSamples;

        public bool IsQualified =>
            _qualifyingSamples >= RequiredQualifyingSamples;

        public int QualifyingSamples =>
            _qualifyingSamples;

        public int ReactionM5 =>
            _reactionM5;

        public int Direction =>
            _direction;

        public bool ObserveReactionSample(
            int reactionM5,
            int direction,
            bool entryAllowed,
            DateTime sampleUtc)
        {
            if (reactionM5 < 0 ||
                direction == 0 ||
                !entryAllowed ||
                sampleUtc == DateTime.MinValue)
            {
                ResetQualification();
                return false;
            }

            if (_reactionM5 != reactionM5 ||
                _direction != direction)
            {
                _reactionM5 = reactionM5;
                _direction = direction;
                _lastSampleUtc = DateTime.MinValue;
                _qualifyingSamples = 0;
            }

            if (sampleUtc <= _lastSampleUtc)
                return IsQualified;

            _lastSampleUtc = sampleUtc;
            _qualifyingSamples =
                Math.Min(
                    RequiredQualifyingSamples,
                    _qualifyingSamples + 1);

            return IsQualified;
        }

        public void ResetQualification()
        {
            _reactionM5 = -1;
            _direction = 0;
            _lastSampleUtc = DateTime.MinValue;
            _qualifyingSamples = 0;
        }

        public string GetQualificationStateText()
        {
            if (IsQualified)
                return "ARMED";

            if (_qualifyingSamples > 0)
                return "CONFIRMING " +
                       _qualifyingSamples +
                       "/" +
                       RequiredQualifyingSamples;

            return "WAITING";
        }
    }
}
