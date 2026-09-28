using System;
using cAlgo.API;

namespace cAlgo
{
    public partial class CFIPIndicator : Indicator
    {
        private bool TryAcquireNormalSubmission(
            int closedM5,
            int direction,
            out string reason)
        {
            return _normalSubmissionGate.TryAcquire(
                Server.TimeInUtc,
                "NORMAL|" + closedM5 + "|" + direction,
                out reason);
        }

        private void RecordNormalSubmission(
            TradeResult result)
        {
            _normalSubmissionGate.Record(
                Server.TimeInUtc,
                result != null &&
                result.IsSuccessful &&
                result.Position != null);
        }

        private bool TryAcquireAggressiveSubmission(
            int closedM5,
            int direction,
            out string reason)
        {
            return _aggressiveSubmissionGate.TryAcquire(
                Server.TimeInUtc,
                "AGGRESSIVE|" + closedM5 + "|" + direction,
                out reason);
        }

        private void RecordAggressiveSubmission(
            TradeResult result)
        {
            _aggressiveSubmissionGate.Record(
                Server.TimeInUtc,
                result != null &&
                result.IsSuccessful &&
                result.Position != null);
        }

        private bool TryAcquirePendingSubmission(
            int closedM5,
            int direction,
            string kind,
            out string reason)
        {
            return _pendingSubmissionGate.TryAcquire(
                Server.TimeInUtc,
                kind + "|" + closedM5 + "|" + direction,
                out reason);
        }

        private void RecordPendingSubmission(
            TradeResult result)
        {
            _pendingSubmissionGate.Record(
                Server.TimeInUtc,
                result != null &&
                result.IsSuccessful &&
                result.PendingOrder != null);
        }

        private void RecordNormalSubmissionFailure()
        {
            _normalSubmissionGate.Record(
                Server.TimeInUtc,
                false);
        }

        private void RecordAggressiveSubmissionFailure()
        {
            _aggressiveSubmissionGate.Record(
                Server.TimeInUtc,
                false);
        }

        private void RecordPendingSubmissionFailure()
        {
            _pendingSubmissionGate.Record(
                Server.TimeInUtc,
                false);
        }
    }
}
