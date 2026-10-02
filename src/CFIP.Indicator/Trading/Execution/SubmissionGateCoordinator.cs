using System;
using cAlgo.API;

namespace cAlgo
{
    public partial class CFIPIndicator : Indicator
    {
        private SubmissionAttemptIdentity BuildSubmissionAttemptIdentity(
            int closedM5,
            int direction,
            ExecutionSubmissionPath path,
            string scenarioId)
        {
            string signalKey =
                SymbolName + "|" +
                closedM5 + "|" +
                direction;

            string attemptKey =
                path.ToString() + "|" +
                closedM5 + "|" +
                direction + "|" +
                (string.IsNullOrWhiteSpace(scenarioId)
                    ? "UNSCOPED"
                    : scenarioId.Trim().Replace("|", "/"));

            return new SubmissionAttemptIdentity(
                signalKey,
                attemptKey,
                path,
                scenarioId);
        }

        private SubmissionAttemptIdentity BuildSubmissionAttemptIdentity(
            int closedM5,
            int direction,
            ExecutionSubmissionPath path)
        {
            return BuildSubmissionAttemptIdentity(
                closedM5,
                direction,
                path,
                "UNSCOPED");
        }

        private bool TryAcquireSubmission(
            int closedM5,
            int direction,
            ExecutionSubmissionPath path,
            out SubmissionAttemptIdentity identity,
            out string reason)
        {
            return TryAcquireSubmission(
                closedM5,
                direction,
                path,
                "UNSCOPED",
                out identity,
                out reason);
        }

        private bool TryAcquireSubmission(
            int closedM5,
            int direction,
            ExecutionSubmissionPath path,
            string scenarioId,
            out SubmissionAttemptIdentity identity,
            out string reason)
        {
            identity = BuildSubmissionAttemptIdentity(
                closedM5,
                direction,
                path,
                scenarioId);

            return _submissionGate.TryAcquire(
                identity,
                Server.TimeInUtc,
                out reason);
        }

        private string ResolvePlanExecutionScenarioId(
            int closedM5)
        {
            if (_plan == null)
                return "CANONICAL-NONE";

            TradeOpportunityCandidate selected;
            string reason;

            if (ScenarioExecutionPolicyRule.TryResolvePlanScenario(
                    _tradePlanRegistry.Snapshot(),
                    _plan,
                    _decision,
                    Math.Max(
                        Symbol.TickSize * 2,
                        Symbol.PipSize * 0.10),
                    M5OnlyConfirmedTrigger,
                    out selected,
                    out reason) &&
                selected != null &&
                !string.IsNullOrWhiteSpace(
                    selected.ScenarioId))
            {
                return selected.ScenarioId;
            }

            return
                ScenarioExecutionPolicyRule.CanonicalScenarioId(
                    _plan);
        }

        private string ResolveDirectionExecutionScenarioId(
            int direction)
        {
            TradeOpportunityCandidate selected;

            if (ScenarioExecutionPolicyRule.TryResolveDirectionScenario(
                    _tradePlanRegistry.Snapshot(),
                    _decision,
                    direction,
                    M5OnlyConfirmedTrigger,
                    out selected) &&
                selected != null &&
                !string.IsNullOrWhiteSpace(
                    selected.ScenarioId))
            {
                return selected.ScenarioId;
            }

            return
                "CANONICAL-DIRECTION-" +
                (direction == 1 ? "BUY" : "SELL");
        }

        private void RecordSubmission(
            SubmissionAttemptIdentity identity,
            TradeResult result,
            ExecutionIntent intent = null)
        {
            string intentTrace =
                FormatExecutionIntentTrace(intent);
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
                confirmed,
                intent);

            RecordExecutionTelemetryHistory(
                identity.Path.ToString(),
                ParseTelemetryM5(identity.AttemptKey),
                confirmed
                    ? "CONFIRMED"
                    : result == null
                        ? "NULL RESULT"
                        : result.IsSuccessful
                            ? "UNCONFIRMED"
                            : "REJECTED",
                "INSTANCE=" +
                InstanceId +
                " • SCENARIO=" +
                identity.ScenarioId +
                " • " +
                intentTrace +
                " • " +
                (result == null
                    ? "BROKER RETURNED NULL"
                    : result.Error.HasValue
                        ? result.Error.Value.ToString()
                        : result.IsSuccessful
                            ? "BROKER ACCEPTED"
                            : "BROKER REJECTED"));
        }

        private string FormatExecutionIntentTrace(
            ExecutionIntent intent)
        {
            if (intent == null)
                return "INTENT=NONE";

            return
                "INTENT ENTRY=" +
                Price(intent.RequestedEntry) +
                " TRIGGER=" +
                Price(intent.Trigger) +
                " SL=" +
                Price(intent.Stop) +
                " TP=" +
                Price(intent.Target) +
                " SL_PIPS=" +
                intent.StopPips.ToString("F4") +
                " TP_PIPS=" +
                intent.TargetPips.ToString("F4") +
                " VOL=" +
                intent.Volume.ToString("F0") +
                " M5=" +
                intent.CreatedM5;

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

            RecordExecutionTelemetryHistory(
                identity.Path.ToString(),
                ParseTelemetryM5(identity.AttemptKey),
                "FAILED",
                "INSTANCE=" +
                InstanceId +
                " • SCENARIO=" +
                identity.ScenarioId +
                " • EXCEPTION / SUBMISSION FAILED");
        }

        private void RecordExecutionTelemetry(
            SubmissionAttemptIdentity identity,
            TradeResult result,
            bool confirmed,
            ExecutionIntent intent)
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
                    "SCENARIO " +
                    identity.ScenarioId +
                    " • " +
                    FormatExecutionIntentTrace(intent) +
                    " • BROKER RETURNED NULL";
                return;
            }

            _lastExecutionTelemetryState =
                confirmed
                    ? "CONFIRMED"
                    : result.IsSuccessful
                        ? "UNCONFIRMED"
                        : "REJECTED";

            string outcomeReason =
                result.Error.HasValue
                    ? result.Error.Value.ToString()
                    : result.IsSuccessful
                        ? "BROKER ACCEPTED"
                        : "BROKER REJECTED";

            _lastExecutionTelemetryReason =
                "SCENARIO " +
                identity.ScenarioId +
                " • " +
                FormatExecutionIntentTrace(intent) +
                " • " +
                outcomeReason;
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
                "SCENARIO " +
                identity.ScenarioId +
                " • " +
                (string.IsNullOrWhiteSpace(reason)
                    ? "SUBMISSION FAILED"
                    : reason);
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