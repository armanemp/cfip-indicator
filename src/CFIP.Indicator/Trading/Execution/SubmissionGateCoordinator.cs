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
            bool confirmed =
                result != null &&
                result.IsSuccessful &&
                HasConfirmedSubmissionEntity(
                    identity.Path,
                    result);

            _submissionGate.Record(
                identity,
                Server.TimeInUtc,
                confirmed);

            RecordExecutionTelemetry(
                identity,
                result,
                confirmed);
        }

        private void RecordSubmissionFailure(
            SubmissionAttemptIdentity identity)
        {
            _submissionGate.Record(
                identity,
                Server.TimeInUtc,
                false);

            RecordExecutionTelemetryFailure(
                identity,
                "EXCEPTION / SUBMISSION FAILED");
        }

        private void RecordExecutionTelemetry(
            SubmissionAttemptIdentity identity,
            TradeResult result,
            bool confirmed)
        {
            _lastExecutionTelemetryPath =
                identity.Path.ToString();
            _lastExecutionTelemetryM5 =
                identity.AttemptKey.IndexOf("|", StringComparison.Ordinal) >= 0
                    ? ParseTelemetryM5(identity.AttemptKey)
                    : -1;
            _lastExecutionTelemetryUtc =
                Server.TimeInUtc;

            if (result == null)
            {
                _lastExecutionTelemetryState =
                    "NULL RESULT";
                _lastExecutionTelemetryReason =
                    "BROKER RETURNED NULL";
                return;
            }

            _lastExecutionTelemetryState =
                confirmed
                    ? "CONFIRMED"
                    : result.IsSuccessful
                        ? "UNCONFIRMED"
                        : "REJECTED";

            _lastExecutionTelemetryReason =
                result.Error.HasValue
                    ? result.Error.Value.ToString()
                    : result.IsSuccessful
                        ? "BROKER ACCEPTED"
                        : "BROKER REJECTED";
        }

        private void RecordExecutionTelemetryFailure(
            SubmissionAttemptIdentity identity,
            string reason)
        {
            _lastExecutionTelemetryPath =
                identity.Path.ToString();
            _lastExecutionTelemetryM5 =
                ParseTelemetryM5(identity.AttemptKey);
            _lastExecutionTelemetryUtc =
                Server.TimeInUtc;
            _lastExecutionTelemetryState =
                "FAILED";
            _lastExecutionTelemetryReason =
                string.IsNullOrWhiteSpace(reason)
                    ? "SUBMISSION FAILED"
                    : reason;
        }

        private int ParseTelemetryM5(
            string attemptKey)
        {
            if (string.IsNullOrWhiteSpace(attemptKey))
                return -1;

            string[] parts =
                attemptKey.Split('|');

            if (parts.Length < 2)
                return -1;

            int value;
            return
                int.TryParse(
                    parts[1],
                    System.Globalization.NumberStyles.Integer,
                    System.Globalization.CultureInfo.InvariantCulture,
                    out value)
                    ? value
                    : -1;
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