using System;
using System.Collections.Generic;
using cAlgo.API;
using CFIP.Contracts;

namespace CFIP.cBot.Execution
{
    internal sealed class DemoPendingOrderExecutionCoordinator
    {
        private const int MaxRememberedKeys = 128;

        private readonly HashSet<string> _rememberedKeys =
            new HashSet<string>(StringComparer.Ordinal);

        private readonly Queue<string> _rememberedOrder =
            new Queue<string>();

        public bool TryExecute(
            Robot robot,
            SignalEnvelope envelope,
            DateTime nowUtc,
            double maximumMarginUsagePercent,
            double marginBufferPercent,
            int maximumConcurrentScenarios,
            CbotExecutionIdempotencyStore idempotencyStore,
            out BrokerExecutionReport report,
            out string reason)
        {
            report = null;
            reason = "NOT APPLICABLE";

            if (robot == null ||
                envelope == null ||
                envelope.Identity == null ||
                envelope.Intent == null)
            {
                reason = "NO PENDING EXECUTION ENVELOPE";
                return false;
            }

            ExecutionAction action =
                envelope.Intent.Action;

            if (action != ExecutionAction.PendingStop &&
                action != ExecutionAction.PendingLimit)
            {
                reason =
                    "PENDING STOP OR LIMIT ACTION REQUIRED";
                return false;
            }

            string key =
                envelope.Identity.IdempotencyKey;

            if (string.IsNullOrWhiteSpace(key))
            {
                reason =
                    "MISSING IDEMPOTENCY KEY";
                return false;
            }

            if (string.IsNullOrWhiteSpace(
                    envelope.Identity.ScenarioId))
            {
                reason =
                    "MISSING SCENARIO ID";
                return false;
            }

            if (_rememberedKeys.Contains(key))
            {
                reason = "DUPLICATE IDEMPOTENCY KEY";
                return false;
            }

            if (idempotencyStore != null &&
                !idempotencyStore.CanAttempt(
                    key,
                    nowUtc,
                    out reason))
            {
                return false;
            }

            string executionLabel =
                envelope.Intent.ExecutionLabel ?? "";

            if (string.IsNullOrWhiteSpace(
                    executionLabel))
            {
                reason =
                    "MISSING EXECUTION LABEL";
                return false;
            }

            if (!IsFinitePositivePendingExecution(
                    envelope.Intent.RequestedEntry) ||
                !IsFinitePositivePendingExecution(
                    envelope.Intent.Stop) ||
                !IsFinitePositivePendingExecution(
                    envelope.Intent.InitialTarget) ||
                !envelope.Intent.RequestedVolume.HasValue ||
                !IsFinitePositivePendingExecution(
                    envelope.Intent.RequestedVolume.Value))
            {
                reason =
                    "INVALID PENDING GEOMETRY OR VOLUME";
                return false;
            }

            if (envelope.Identity.Direction !=
                    TradeDirection.Buy &&
                envelope.Identity.Direction !=
                    TradeDirection.Sell)
            {
                reason =
                    "INVALID PENDING DIRECTION";
                return false;
            }

            if (!IsFinitePositivePendingExecution(
                    robot.Symbol.Bid) ||
                !IsFinitePositivePendingExecution(
                    robot.Symbol.Ask) ||
                !IsFinitePositivePendingExecution(
                    robot.Symbol.PipSize) ||
                robot.Symbol.Ask <
                    robot.Symbol.Bid)
            {
                reason =
                    "INVALID LIVE QUOTE";
                return false;
            }

            double executableReference =
                envelope.Identity.Direction ==
                    TradeDirection.Buy
                    ? robot.Symbol.Ask
                    : robot.Symbol.Bid;

            double minimumOffset =
                Math.Max(
                    robot.Symbol.TickSize,
                    robot.Symbol.PipSize * 0.10);

            bool correctSide;

            if (action ==
                ExecutionAction.PendingStop)
            {
                correctSide =
                    envelope.Identity.Direction ==
                        TradeDirection.Buy
                        ? envelope.Intent.RequestedEntry >
                          executableReference +
                          minimumOffset
                        : envelope.Intent.RequestedEntry <
                          executableReference -
                          minimumOffset;
            }
            else
            {
                correctSide =
                    envelope.Identity.Direction ==
                        TradeDirection.Buy
                        ? envelope.Intent.RequestedEntry <
                          executableReference -
                          minimumOffset
                        : envelope.Intent.RequestedEntry >
                          executableReference +
                          minimumOffset;
            }

            if (!correctSide)
            {
                reason =
                    action ==
                        ExecutionAction.PendingStop
                        ? "PENDING STOP TRIGGER NOT AHEAD OF EXECUTABLE QUOTE"
                        : "PENDING LIMIT ENTRY NOT BEYOND EXECUTABLE QUOTE";
                return false;
            }

            TradeType tradeType =
                envelope.Identity.Direction ==
                    TradeDirection.Buy
                    ? TradeType.Buy
                    : TradeType.Sell;

            double stopPips =
                Math.Abs(
                    envelope.Intent.RequestedEntry -
                    envelope.Intent.Stop) /
                robot.Symbol.PipSize;

            double targetPips =
                Math.Abs(
                    envelope.Intent.InitialTarget -
                    envelope.Intent.RequestedEntry) /
                robot.Symbol.PipSize;

            if (!IsFinitePositivePendingExecution(
                    stopPips) ||
                !IsFinitePositivePendingExecution(
                    targetPips))
            {
                reason =
                    "INVALID PENDING STOP/TARGET DISTANCES";
                return false;
            }

            DateTime? expiration =
                envelope.Intent.ExpiryUtc ??
                envelope.Identity.ExpiryUtc;

            if (expiration.HasValue &&
                expiration.Value <= nowUtc)
            {
                reason =
                    "PENDING INTENT EXPIRED";
                return false;
            }

            double volume;
            string marginReason;

            if (!BrokerExecutionSafety.TryConstrainVolumeForMargin(
                    robot,
                    tradeType,
                    envelope.Intent.RequestedVolume.Value,
                    maximumMarginUsagePercent,
                    marginBufferPercent,
                    out volume,
                    out marginReason))
            {
                reason =
                    marginReason;
                return false;
            }

            int managedPositions =
                BrokerExecutionSafety.CountManagedPositions(
                    robot,
                    executionLabel);

            int managedPending =
                BrokerExecutionSafety.CountManagedPending(
                    robot,
                    executionLabel);

            if (managedPositions +
                managedPending >= 1)
            {
                reason =
                    "SCENARIO CAPACITY BLOCKED • SCENARIO ALREADY ACTIVE";
                return false;
            }

            string managedRoot =
                executionLabel;
            int scenarioMarker =
                managedRoot.IndexOf(
                    "|CFIP-S:",
                    StringComparison.Ordinal);

            if (scenarioMarker > 0)
                managedRoot =
                    managedRoot.Substring(
                        0,
                        scenarioMarker);

            if (maximumConcurrentScenarios < 1 ||
                BrokerExecutionSafety.CountManagedScenarioObjects(
                    robot,
                    managedRoot) >= maximumConcurrentScenarios)
            {
                reason =
                    "CONCURRENT SCENARIO CAPACITY BLOCKED • " +
                    Math.Max(1, maximumConcurrentScenarios);
                return false;
            }

            string label =
                executionLabel + "-PENDING";

            TradeResult result;

            try
            {
                result =
                    action ==
                        ExecutionAction.PendingStop
                        ? robot.PlaceStopOrder(
                            tradeType,
                            robot.SymbolName,
                            volume,
                            envelope.Intent.RequestedEntry,
                            label,
                            stopPips,
                            targetPips,
                            ProtectionType.Relative,
                            expiration,
                            "CFIP DEMO PENDING STOP",
                            false)
                        : robot.PlaceLimitOrder(
                            tradeType,
                            robot.SymbolName,
                            volume,
                            envelope.Intent.RequestedEntry,
                            label,
                            stopPips,
                            targetPips,
                            ProtectionType.Relative,
                            expiration,
                            "CFIP DEMO PENDING LIMIT",
                            false);
            }
            catch (Exception ex)
            {
                Remember(key);
                if (idempotencyStore != null)
                    idempotencyStore.RecordAttempt(
                        robot,
                        key,
                        false,
                        nowUtc);

                reason =
                    action ==
                        ExecutionAction.PendingStop
                        ? "PENDING STOP SUBMISSION EXCEPTION"
                        : "PENDING LIMIT SUBMISSION EXCEPTION";

                report =
                    BuildReport(
                        envelope,
                        action,
                        BrokerReportStatus.RecoveryRequired,
                        nowUtc,
                        null,
                        ex.Message);

                return false;
            }

            Remember(key);

            if (result == null)
            {
                if (idempotencyStore != null)
                    idempotencyStore.RecordAttempt(
                        robot,
                        key,
                        false,
                        nowUtc);

                reason =
                    action ==
                        ExecutionAction.PendingStop
                        ? "NULL PENDING STOP RESULT"
                        : "NULL PENDING LIMIT RESULT";

                report =
                    BuildReport(
                        envelope,
                        action,
                        BrokerReportStatus.RecoveryRequired,
                        nowUtc,
                        null,
                        reason);

                return false;
            }

            if (!result.IsSuccessful ||
                result.PendingOrder == null)
            {
                reason =
                    result.Error.HasValue
                        ? result.Error.Value.ToString()
                        : action ==
                            ExecutionAction.PendingStop
                            ? "BROKER REJECTED PENDING STOP"
                            : "BROKER REJECTED PENDING LIMIT";

                report =
                    BuildReport(
                        envelope,
                        action,
                        BrokerReportStatus.Rejected,
                        nowUtc,
                        result,
                        reason);

                if (idempotencyStore != null)
                    idempotencyStore.RecordAttempt(
                        robot,
                        key,
                        false,
                        nowUtc);

                return false;
            }

            reason =
                action ==
                    ExecutionAction.PendingStop
                    ? "PENDING STOP #" +
                      result.PendingOrder.Id
                    : "PENDING LIMIT #" +
                      result.PendingOrder.Id;

            report =
                BuildReport(
                    envelope,
                    action,
                    BrokerReportStatus.Confirmed,
                    nowUtc,
                    result,
                    reason);

            if (idempotencyStore != null)
                idempotencyStore.RecordAttempt(
                    robot,
                    key,
                    true,
                    nowUtc);

            return true;
        }

        private static BrokerExecutionReport BuildReport(
            SignalEnvelope envelope,
            ExecutionAction action,
            BrokerReportStatus status,
            DateTime nowUtc,
            TradeResult result,
            string reason)
        {
            long? pendingId =
                result != null &&
                result.PendingOrder != null
                    ? result.PendingOrder.Id
                    : (long?)null;

            return new BrokerExecutionReport(
                envelope.Identity,
                action ==
                    ExecutionAction.PendingLimit
                    ? BrokerAction.SubmitPendingLimit
                    : BrokerAction.SubmitPendingStop,
                status,
                nowUtc,
                nowUtc,
                status ==
                    BrokerReportStatus.Confirmed
                    ? nowUtc
                    : (DateTime?)null,
                result != null &&
                result.Position != null
                    ? result.Position.Id
                    : (long?)null,
                pendingId,
                null,
                null,
                null,
                pendingId.HasValue
                    ? pendingId.Value.ToString()
                    : "",
                result != null &&
                result.Error.HasValue
                    ? result.Error.Value.ToString()
                    : "",
                reason,
                envelope.Identity.Revision);
        }

        private void Remember(string key)
        {
            if (!_rememberedKeys.Add(key))
                return;

            _rememberedOrder.Enqueue(key);

            while (_rememberedOrder.Count >
                   MaxRememberedKeys)
            {
                _rememberedKeys.Remove(
                    _rememberedOrder.Dequeue());
            }
        }

        private static bool IsFinitePositivePendingExecution(
            double value)
        {
            return
                !double.IsNaN(value) &&
                !double.IsInfinity(value) &&
                value > 0;
        }
    }
}
