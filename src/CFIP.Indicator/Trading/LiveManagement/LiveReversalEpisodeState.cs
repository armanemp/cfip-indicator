// ============================================================================
// CFIP Indicator — LiveReversalEpisodeState.cs
// Single-responsibility live reversal episode state.
// ============================================================================

namespace cAlgo
{
    public partial class CFIPIndicator
    {
        private long _reversalEpisodePositionId = -1;
        private int _reversalEpisodeDirection;
        private bool _reversalEpisodeAlerted;

        private void ResetReversalEpisode()
        {
            _reversalEpisodePositionId = -1;
            _reversalEpisodeDirection = 0;
            _reversalEpisodeAlerted = false;
        }

        private void MaintainReversalEpisode(
            long positionId,
            int oppositeDirection)
        {
            if (positionId <= 0 ||
                oppositeDirection == 0)
                return;

            bool sameEpisode =
                LiveReversalEpisodeRule.IsSameEpisode(
                    positionId,
                    oppositeDirection,
                    _reversalEpisodePositionId,
                    _reversalEpisodeDirection);

            if (!sameEpisode)
            {
                _reversalEpisodePositionId = positionId;
                _reversalEpisodeDirection = oppositeDirection;
                _reversalEpisodeAlerted = false;
            }
        }

        private bool TryMarkReversalAlertEmitted(
            long positionId,
            int oppositeDirection)
        {
            MaintainReversalEpisode(
                positionId,
                oppositeDirection);

            if (!LiveReversalEpisodeRule.ShouldEmitDetectionAlert(
                    _reversalEpisodeAlerted))
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
