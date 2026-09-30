// ============================================================================
// CFIP Indicator — LiveReversalEpisodeState.cs
// Single-responsibility live reversal episode state.
// ============================================================================

using System;

namespace cAlgo
{
    public partial class CFIPIndicator
    {
        private long _reversalEpisodePositionId = -1;
        private int _reversalEpisodeDirection;
        private int _reversalEpisodeLastQualifiedM5 = -1;
        private bool _reversalEpisodeAlerted;

        private void ResetReversalEpisode()
        {
            _reversalEpisodePositionId = -1;
            _reversalEpisodeDirection = 0;
            _reversalEpisodeLastQualifiedM5 = -1;
            _reversalEpisodeAlerted = false;
        }

        private void MaintainReversalEpisode(
            long positionId,
            int oppositeDirection,
            int closedM5)
        {
            if (positionId <= 0 ||
                oppositeDirection == 0)
                return;

            if (_reversalEpisodePositionId != positionId ||
                _reversalEpisodeDirection != oppositeDirection ||
                (_reversalEpisodeLastQualifiedM5 >= 0 &&
                 closedM5 - _reversalEpisodeLastQualifiedM5 > 1))
            {
                _reversalEpisodePositionId = positionId;
                _reversalEpisodeDirection = oppositeDirection;
                _reversalEpisodeLastQualifiedM5 = closedM5;
                _reversalEpisodeAlerted = false;
                return;
            }

            _reversalEpisodeLastQualifiedM5 =
                Math.Max(
                    _reversalEpisodeLastQualifiedM5,
                    closedM5);
        }

        private bool TryMarkReversalAlertEmitted(
            long positionId,
            int oppositeDirection,
            int closedM5)
        {
            MaintainReversalEpisode(
                positionId,
                oppositeDirection,
                closedM5);

            if (_reversalEpisodeAlerted)
                return false;

            _reversalEpisodeAlerted = true;
            return true;
        }

        private void ResetReversalEpisodeIfPositionChanged(
            long positionId)
        {
            if (positionId <= 0 ||
                _reversalEpisodePositionId == positionId)
                return;

            ResetReversalEpisode();
        }

        private void ResetReversalEpisodeOnClosedPosition(
            long positionId)
        {
            if (positionId <= 0 ||
                _reversalEpisodePositionId != positionId)
                return;

            ResetReversalEpisode();
        }
    }
}
