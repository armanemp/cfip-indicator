using System;
using cAlgo.API;

namespace cAlgo
{
    public partial class CFIPIndicator : Indicator
    {
        private SubmissionAttemptIdentity BuildSubmissionAttemptIdentity(
            int closedM5,
            int direction,
            ExecutionSubmissionPath path)
        {
            string signalKey =
                SymbolName + "|" +
                closedM5 + "|" +
                direction;

            string attemptKey =
                path.ToString() + "|" +
                closedM5 + "|" +
                direction;

            return new SubmissionAttemptIdentity(
                signalKey,
                attemptKey,
                path);
        }

        private bool TryAcquireSubmission(
            int closedM5,
            int direction,
            ExecutionSubmissionPath path,
            out SubmissionAttemptIdentity identity,
            out string reason)
        {
            identity = BuildSubmissionAttemptIdentity(
                closedM5,
                direction,
                path);

            return _submissionGate.TryAcquire(
                identity,
                Server.TimeInUtc,
                out reason);
        }

        private void RecordSubmission(
            SubmissionAttemptIdentity identity,
            TradeResult result)
        {
            _submissionGate.Record(
                identity,
                Server.TimeInUtc,
                result != null &&
                result.IsSuccessful &&
                HasConfirmedSubmissionEntity(
                    identity.Path,
                    result));
        }

        private void RecordSubmissionFailure(
            SubmissionAttemptIdentity identity)
        {
            _submissionGate.Record(
                identity,
                Server.TimeInUtc,
                false);
        }

        private bool HasConfirmedSubmissionEntity(
            ExecutionSubmissionPath path,
            TradeResult result)
        {
            if (result == null || !result.IsSuccessful)
                return false;

            switch (path)
            {
                case ExecutionSubmissionPath.AutomaticMarket:
                case ExecutionSubmissionPath.AggressiveMarket:
                    return result.Position != null;

                case ExecutionSubmissionPath.PendingStop:
                case ExecutionSubmissionPath.PendingLimit:
                    return result.PendingOrder != null;

                default:
                    return false;
            }
        }
    }
}