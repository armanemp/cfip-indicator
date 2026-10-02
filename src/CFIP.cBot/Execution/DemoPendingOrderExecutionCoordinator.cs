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

        public bool TryExecuteStop(
            Robot robot,
            SignalEnvelope envelope,
            string executionLabel,
            DateTime nowUtc,
            double maximumMarginUsagePercent,
            double marginBufferPercent,
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

            if (envelope.Intent.Action != ExecutionAction.PendingStop)
            {
                reason = "PENDING STOP ACTION REQUIRED";
                return false;
            }

            string key = envelope.Identity.IdempotencyKey;
            if (string.IsNullOrWhiteSpace(key))
            {
                reason = "MISSING IDEMPOTENCY KEY";
                return false;
            }

            if (_rememberedKeys.Contains(key))
            {
                reason = "DUPLICATE IDEMPOTENCY KEY";
                return false;
            }

            if (string.IsNullOrWhiteSpace(executionLabel))
            {
                reason = "MISSING EXECUTION LABEL";
                return false;
            }

            if (!IsFinitePositive(envelope.Intent.RequestedEntry) ||
                !IsFinitePositive(envelope.Intent.Stop) ||
                !IsFinitePositive(envelope.Intent.InitialTarget) ||
                !envelope.Intent.RequestedVolume.HasValue ||
                !IsFinitePositive(envelope.Intent.RequestedVolume.Value))
            {
                reason = "INVALID PENDING STOP GEOMETRY OR VOLUME";
                return false;
            }

            TradeType tradeType =
                envelope.Identity.Direction == TradeDirection.Buy
                    ? TradeType.Buy
                    : envelope.Identity.Direction == TradeDirection.Sell
                        ? TradeType.Sell
                        : (TradeType)(-1);

            if (envelope.Identity.Direction == TradeDirection.None)
            {
                reason = "INVALID PENDING STOP DIRECTION";
                return false;
            }

            if (!IsFinitePositive(robot.Symbol.Bid) ||
                !IsFinitePositive(robot.Symbol.Ask) ||
                !IsFinitePositive(robot.Symbol.PipSize) ||
                robot.Symbol.Ask < robot.Symbol.Bid)
            {
                reason = "INVALID LIVE QUOTE";
                return false;
            }

            double liveExecutableReference =
                envelope.Identity.Direction == TradeDirection.Buy
                    ? robot.Symbol.Ask
                    : robot.Symbol.Bid;

            double minimumOffset =
                Math.Max(
                    robot.Symbol.TickSize,
                    robot.Symbol.PipSize * 0.10);

            bool correctSide =
                envelope.Identity.Direction == TradeDirection.Buy
                    ? envelope.Intent.RequestedEntry >
                      liveExecutableReference + minimumOffset
                    : envelope.Intent.RequestedEntry <
                      liveExecutableReference - minimumOffset;

            if (!correctSide)
            {
                reason = "PENDING STOP TRIGGER NOT AHEAD OF EXECUTABLE QUOTE";
                return false;
            }

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

            if (!IsFinitePositive(stopPips) ||
                !IsFinitePositive(targetPips))
            {
                reason = "INVALID PENDING STOP DISTANCES";
                return false;
            }

            if (envelope.Identity.ExpiryUtc.HasValue &&
                envelope.Identity.ExpiryUtc.Value <= nowUtc)
            {
                reason = "PENDING STOP EXPIRED";
                return false;
            }

            if (envelope.Intent.ExpiryUtc.HasValue &&
                envelope.Intent.ExpiryUtc.Value <= nowUtc)
            {
                reason = "PENDING STOP INTENT EXPIRED";
                return false;
            }

            if (!TryConstrainVolumeForMargin(
                    robot,
                    tradeType,
                    envelope.Intent.RequestedVolume.Value,
                    maximumMarginUsagePercent,
                    marginBufferPercent,
                    out double volume,
                    out reason))
                return false;

            int managedPositions = CountManagedPositions(robot, executionLabel);
            int managedPending = CountManagedPending(robot, executionLabel);

            if (managedPositions + managedPending >= 1)
            {
                reason = "SINGLE-PLAN CAPACITY BLOCKED";
                return false;
            }

            string label = executionLabel + "-PENDING";
            DateTime? expiration =
                envelope.Intent.ExpiryUtc ?? envelope.Identity.ExpiryUtc;

            TradeResult result;
            try
            {
                result = robot.PlaceStopOrder(
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
                    false);
            }
            catch (Exception ex)
            {
                Remember(key);
                reason = "PENDING STOP SUBMISSION EXCEPTION";
                report = BuildReport(
                    envelope,
                    BrokerReportStatus.RecoveryRequired,
                    nowUtc,
                    null,
                    ex.Message);
                return false;
            }

            Remember(key);

            if (result == null)
            {
                reason = "NULL PENDING STOP RESULT";
                report = BuildReport(
                    envelope,
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
                        : "BROKER REJECTED PENDING STOP";
                report = BuildReport(
                    envelope,
                    BrokerReportStatus.Rejected,
                    nowUtc,
                    result,
                    reason);
                return false;
            }

            reason = "PENDING ORDER #" + result.PendingOrder.Id;
            report = BuildReport(
                envelope,
                BrokerReportStatus.Confirmed,
                nowUtc,
                result,
                reason);
            return true;
        }

        private static int CountManagedPositions(
            Robot robot,
            string executionLabel)
        {
            int count = 0;
            foreach (Position position in robot.Positions)
            {
                if (position != null &&
                    string.Equals(position.SymbolName, robot.SymbolName, StringComparison.Ordinal) &&
                    string.Equals(position.Label, executionLabel, StringComparison.Ordinal))
                    count++;
            }
            return count;
        }

        private static int CountManagedPending(
            Robot robot,
            string executionLabel)
        {
            int count = 0;
            string label = executionLabel + "-PENDING";
            foreach (PendingOrder order in robot.PendingOrders)
            {
                if (order != null &&
                    string.Equals(order.SymbolName, robot.SymbolName, StringComparison.Ordinal) &&
                    string.Equals(order.Label, label, StringComparison.Ordinal))
                    count++;
            }
            return count;
        }

        private static bool TryConstrainVolumeForMargin(
            Robot robot,
            TradeType tradeType,
            double requestedVolume,
            double maximumMarginUsagePercent,
            double marginBufferPercent,
            out double constrainedVolume,
            out string reason)
        {
            constrainedVolume = 0;
            reason = "MARGIN SIZING UNAVAILABLE";

            if (!IsFinitePositive(requestedVolume))
                return false;

            double allowedMargin =
                Risk.ExecutionMarginBudgetRule.AllowedMargin(
                    robot.Account.FreeMargin,
                    maximumMarginUsagePercent,
                    marginBufferPercent);

            if (!IsFinitePositive(allowedMargin))
            {
                reason = "NO EXECUTION MARGIN BUDGET";
                return false;
            }

            double estimatedMargin;
            try
            {
                estimatedMargin =
                    robot.Symbol.GetEstimatedMargin(
                        tradeType,
                        requestedVolume);
            }
            catch
            {
                reason = "EXECUTION MARGIN ESTIMATE FAILED";
                return false;
            }

            if (!IsFinitePositive(estimatedMargin))
            {
                reason = "EXECUTION MARGIN ESTIMATE INVALID";
                return false;
            }

            double candidate =
                Risk.ExecutionMarginBudgetRule.ScaleVolumeToBudget(
                    requestedVolume,
                    estimatedMargin,
                    allowedMargin);

            if (!IsFinitePositive(candidate))
            {
                reason = "EXECUTION MARGIN VOLUME INVALID";
                return false;
            }

            constrainedVolume =
                robot.Symbol.NormalizeVolumeInUnits(
                    candidate,
                    RoundingMode.Down);

            if (robot.Symbol.VolumeInUnitsMin > 0 &&
                constrainedVolume < robot.Symbol.VolumeInUnitsMin)
            {
                reason = "MARGIN CAP BELOW BROKER MINIMUM";
                return false;
            }

            double finalEstimatedMargin =
                robot.Symbol.GetEstimatedMargin(
                    tradeType,
                    constrainedVolume);

            if (!IsFinitePositive(finalEstimatedMargin) ||
                finalEstimatedMargin > allowedMargin)
            {
                reason = "FINAL EXECUTION MARGIN BUDGET EXCEEDED";
                return false;
            }

            return true;
        }

        private static BrokerExecutionReport BuildReport(
            SignalEnvelope envelope,
            BrokerReportStatus status,
            DateTime nowUtc,
            TradeResult result,
            string reason)
        {
            long? pendingId =
                result != null && result.PendingOrder != null
                    ? result.PendingOrder.Id
                    : (long?)null;

            return new BrokerExecutionReport(
                envelope.Identity,
                BrokerAction.SubmitPendingStop,
                status,
                nowUtc,
                nowUtc,
                status == BrokerReportStatus.Confirmed
                    ? nowUtc
                    : (DateTime?)null,
                result != null && result.Position != null
                    ? result.Position.Id
                    : (long?)null,
                pendingId,
                null,
                null,
                null,
                pendingId.HasValue ? pendingId.Value.ToString() : "",
                result != null && result.Error.HasValue
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
            while (_rememberedOrder.Count > MaxRememberedKeys)
                _rememberedKeys.Remove(_rememberedOrder.Dequeue());
        }

        private static bool IsFinitePositive(double value)
        {
            return !double.IsNaN(value) &&
                   !double.IsInfinity(value) &&
                   value > 0;
        }
    }
}