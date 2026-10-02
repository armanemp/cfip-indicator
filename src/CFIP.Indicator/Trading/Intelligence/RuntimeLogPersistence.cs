using System;
using System.Globalization;
using System.IO;
using System.Text;
using cAlgo.API.Internals;

namespace cAlgo
{
    public partial class CFIPIndicator
    {
        private const string RuntimeLogSchema =
            "CFIP-RUNTIME-LOG,2";

        private string _runtimeLogPrefixCache;
        private string _runtimeLogPrefixIdentityCache;

        private const string RuntimeLogHeader =
            "ObservedUtcTicks,EventType,M5,Path,State,Reason,ScenarioId,SourceTimeframe," +
            "Direction,Entry,Stop,Tp1,Tp2,Tp3,Tp4,Confidence,SmartQuality,ActionableNow," +
            "PositionId,PendingOrderId,ScenarioEvidence,LocationQuality,WaveTrendQuality," +
            "PolicyAllowed,PolicyReason,ForecastHorizonBars";

        private string RuntimeLogPrefix()
        {
            string symbol =
                SanitizeArchivePart(
                    string.IsNullOrWhiteSpace(SymbolName)
                        ? "UNKNOWN"
                        : SymbolName);

            string timeframe =
                SanitizeArchivePart(
                    ExecutionTimeframePolicy.PrimaryExecution);

            string accountScope =
                MemoryAccountScopeToken();

            string identity =
                symbol +
                "|" +
                timeframe +
                "|" +
                accountScope +
                "|" +
                MemoryConfigurationFingerprint();

            if (!string.IsNullOrWhiteSpace(
                    _runtimeLogPrefixCache) &&
                string.Equals(
                    _runtimeLogPrefixIdentityCache,
                    identity,
                    StringComparison.Ordinal))
                return _runtimeLogPrefixCache;

            _runtimeLogPrefixIdentityCache =
                identity;

            _runtimeLogPrefixCache =
                "CFIP_RuntimeLog_v2_" +
                symbol +
                "_" +
                timeframe +
                "_" +
                accountScope +
                "_" +
                MemoryConfigurationFingerprint();

            return _runtimeLogPrefixCache;
        }

        private string RuntimeLogFilePath(
            DateTime observedUtc)
        {
            DateTime start =
                OutcomeArchivePeriodStart(
                    observedUtc);

            DateTime endExclusive =
                start.AddDays(
                    CanonicalTimeRule.OutcomeArchivePeriodDays);

            return
                OutcomeArchiveDirectory +
                Path.DirectorySeparatorChar +
                RuntimeLogPrefix() +
                "_" +
                start.ToString(
                    "yyyyMMdd",
                    CultureInfo.InvariantCulture) +
                "_" +
                endExclusive.AddDays(-1).ToString(
                    "yyyyMMdd",
                    CultureInfo.InvariantCulture) +
                ".csv";
        }

        private string RuntimeNumber(
            double value)
        {
            if (double.IsNaN(value) ||
                double.IsInfinity(value))
                return "";

            return value.ToString(
                "R",
                CultureInfo.InvariantCulture);
        }

        private void ArchiveRuntimeEvent(
            string eventType,
            int m5,
            string path,
            string state,
            string reason,
            string scenarioId,
            string sourceTimeframe,
            int direction,
            double entry,
            double stop,
            double tp1,
            double tp2,
            double tp3,
            double tp4,
            int confidence,
            int smartQuality,
            bool actionableNow,
            long positionId,
            long pendingOrderId,
            DateTime? observedUtc = null,
            int scenarioEvidence = 0,
            int locationQuality = 0,
            int waveTrendQuality = 0,
            bool policyAllowed = false,
            string policyReason = "",
            int forecastHorizonBars = 0)
        {
            try
            {
                DateTime observed =
                    observedUtc.HasValue
                        ? CanonicalTimeRule.EnsureUtc(
                            observedUtc.Value)
                        : Server.TimeInUtc;

                StringBuilder row =
                    new StringBuilder();

                row.Append(
                    observed.Ticks.ToString(
                        CultureInfo.InvariantCulture));
                row.Append(',');
                row.Append(Encode(eventType ?? ""));
                row.Append(',');
                row.Append(m5.ToString(
                    CultureInfo.InvariantCulture));
                row.Append(',');
                row.Append(Encode(path ?? ""));
                row.Append(',');
                row.Append(Encode(state ?? ""));
                row.Append(',');
                row.Append(Encode(reason ?? ""));
                row.Append(',');
                row.Append(Encode(scenarioId ?? ""));
                row.Append(',');
                row.Append(Encode(sourceTimeframe ?? ""));
                row.Append(',');
                row.Append(direction);
                row.Append(',');
                row.Append(RuntimeNumber(entry));
                row.Append(',');
                row.Append(RuntimeNumber(stop));
                row.Append(',');
                row.Append(RuntimeNumber(tp1));
                row.Append(',');
                row.Append(RuntimeNumber(tp2));
                row.Append(',');
                row.Append(RuntimeNumber(tp3));
                row.Append(',');
                row.Append(RuntimeNumber(tp4));
                row.Append(',');
                row.Append(confidence);
                row.Append(',');
                row.Append(smartQuality);
                row.Append(',');
                row.Append(actionableNow ? "1" : "0");
                row.Append(',');
                row.Append(positionId);
                row.Append(',');
                row.Append(pendingOrderId);
                row.Append(',');
                row.Append(scenarioEvidence);
                row.Append(',');
                row.Append(locationQuality);
                row.Append(',');
                row.Append(waveTrendQuality);
                row.Append(',');
                row.Append(policyAllowed ? "1" : "0");
                row.Append(',');
                row.Append(Encode(policyReason ?? ""));
                row.Append(',');
                row.Append(forecastHorizonBars);

                _bufferedArchivePersistence.Enqueue(
                    RuntimeLogFilePath(
                        observed),
                    RuntimeLogSchema +
                    Environment.NewLine +
                    RuntimeLogHeader,
                    row.ToString());
            }
            catch (Exception ex)
            {
                // Diagnostics must never become a trading failure path.
                Print(
                    "CFIP runtime log queue failed: {0}",
                    ex.Message);
            }
        }

        private void ArchiveRuntimeDecisionTrace(
            SignalEvaluationTrace trace)
        {
            if (trace == null)
                return;

            ArchiveRuntimeEvent(
                "DECISION",
                trace.ClosedM5,
                "SIGNAL",
                trace.TraceGate,
                string.IsNullOrWhiteSpace(
                    trace.DecisionReason)
                    ? trace.ActionabilityReason
                    : trace.DecisionReason,
                "MAIN",
                "MTF",
                trace.Direction,
                trace.Entry,
                trace.Stop,
                trace.Tp1,
                trace.Tp2,
                trace.Tp3,
                trace.Tp4,
                trace.Confidence,
                trace.SmartQuality,
                trace.ActionableNow != 0,
                0,
                0,
                trace.ObservedUtcTicks > 0
                    ? new DateTime(
                        trace.ObservedUtcTicks,
                        DateTimeKind.Utc)
                    : (DateTime?)null);
        }

        private void ArchiveRuntimeEntrySignalTiming(
            int m5,
            int direction,
            EntrySignalTiming timing,
            string traceId)
        {
            if (!timing.Measured)
                return;

            ArchiveRuntimeEvent(
                "ACTIONABILITY_TIMING",
                m5,
                "SIGNAL",
                "MEASURED",
                "CAUSAL_UTC_TICKS=" +
                timing.CausalEventUtcTicks.ToString(
                    CultureInfo.InvariantCulture) +
                ";ACTIONABLE_UTC_TICKS=" +
                timing.ActionableUtcTicks.ToString(
                    CultureInfo.InvariantCulture) +
                ";LATENCY_MS=" +
                timing.LatencyMilliseconds.ToString(
                    CultureInfo.InvariantCulture),
                string.IsNullOrWhiteSpace(traceId)
                    ? "MAIN"
                    : traceId,
                "ENTRY",
                direction,
                0,
                0,
                0,
                0,
                0,
                0,
                _decision == null ? 0 : _decision.Confidence,
                _decision == null ? 0 : _decision.SmartQuality,
                true,
                0,
                0,
                timing.Measured
                    ? new DateTime(
                        timing.ActionableUtcTicks,
                        DateTimeKind.Utc)
                    : (DateTime?)null);
        }

        private void ArchiveRuntimePrediction(
            Prediction prediction,
            int closedM5)
        {
            if (!EnableEarlyPrediction)
                return;

            int direction =
                prediction == null
                    ? 0
                    : prediction.Direction;

            ArchiveRuntimeEvent(
                "PREDICTION",
                closedM5,
                "EARLY",
                prediction == null ||
                direction == 0
                    ? "WATCH"
                    : "FORECAST",
                prediction == null
                    ? "NO PREDICTION"
                    : prediction.Reason,
                "PREDICTION-M5-M15",
                "M5/M15",
                direction,
                prediction == null ? 0 : prediction.Entry,
                prediction == null ? 0 : prediction.StopLoss,
                prediction == null ? 0 : prediction.Target1,
                prediction == null ? 0 : prediction.Target2,
                prediction == null ? 0 : prediction.Target3,
                prediction == null ? 0 : prediction.Target4,
                prediction == null ? 0 : prediction.Confidence,
                prediction == null ? 0 : prediction.Confidence,
                prediction != null &&
                direction != 0,
                0,
                0,
                null,
                0,
                0,
                0,
                false,
                "",
                Math.Max(
                    0,
                    PredictionLookaheadBars));
        }

        private void ArchiveRuntimeScenario(
            TradeOpportunityCandidate candidate)
        {
            if (candidate == null)
                return;

            ArchiveRuntimeEvent(
                "SCENARIO",
                candidate.CreatedM5,
                "PARALLEL",
                candidate.Stage,
                candidate.ActionabilityReason,
                candidate.ScenarioId,
                candidate.SourceTimeframe,
                candidate.Direction,
                candidate.Entry,
                candidate.Stop,
                candidate.Tp1,
                candidate.Tp2,
                candidate.Tp3,
                candidate.Tp4,
                candidate.Quality,
                candidate.Quality,
                candidate.ActionableNow,
                0,
                0,
                null,
                candidate.IndependentEvidenceScore,
                candidate.LocationConfluenceScore,
                candidate.WaveTrendQuality,
                candidate.ExecutionPolicyAllowed,
                candidate.ExecutionPolicyReason,
                0);
        }

        private void ArchiveRuntimeExecution(
            string path,
            int m5,
            string state,
            string reason)
        {
            string sourceTimeframe =
                "MTF";

            string scenarioId =
                "MAIN";

            int direction = 0;
            double entry = 0;
            double stop = 0;
            double tp1 = 0;
            double tp2 = 0;
            double tp3 = 0;
            double tp4 = 0;
            long positionId = 0;
            long pendingOrderId = 0;
            int confidence = 0;
            int smartQuality = 0;
            bool actionable = false;

            if (_plan != null)
            {
                direction =
                    _plan.Direction;
                entry =
                    _plan.Entry;
                stop =
                    _plan.Stop;
                tp1 =
                    _plan.Tp1;
                tp2 =
                    _plan.Tp2;
                tp3 =
                    _plan.Tp3;
                tp4 =
                    _plan.Tp4;
                positionId =
                    _plan.PositionId;
            }

            if (_decision != null)
            {
                confidence =
                    _decision.Confidence;
                smartQuality =
                    _decision.SmartQuality;
                actionable =
                    _decision.ActionableNow;
            }

            var pending =
                GetManagedPendingOrder();

            if (pending != null)
                pendingOrderId =
                    pending.Id;

            ArchiveRuntimeEvent(
                "EXECUTION",
                m5,
                path,
                state,
                reason,
                scenarioId,
                sourceTimeframe,
                direction,
                entry,
                stop,
                tp1,
                tp2,
                tp3,
                tp4,
                confidence,
                smartQuality,
                actionable,
                positionId,
                pendingOrderId);
        }
    }
}
