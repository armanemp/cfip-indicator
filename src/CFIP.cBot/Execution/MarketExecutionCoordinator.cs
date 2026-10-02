using System;
using System.Collections.Generic;
using cAlgo.API;
using CFIP.Contracts;

namespace CFIP.cBot.Execution
{
    public sealed class MarketExecutionCoordinator
    {
        private const int MaxRememberedKeys = 128;
        private readonly HashSet<string> _rememberedKeys =
            new HashSet<string>(StringComparer.Ordinal);
        private readonly Queue<string> _rememberedOrder =
            new Queue<string>();

        public bool TryExecute(
            Robot robot,
            SignalEnvelope envelope,
            bool enableMarketRange,
            DateTime nowUtc,
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
                reason = "NO EXECUTION ENVELOPE";
                return false;
            }

            string key =
                envelope.Intent.Identity.IdempotencyKey;

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

            if (!MarketExecutionIntentRule.Validate(
                    envelope.Intent,
                    out reason))
                return false;

            if (!HasLiveCapacity(
                    robot,
                    envelope.Intent.ExecutionLabel,
                    out reason))
                return false;

            TradeResult result;
            string mutationReason;

            try
            {
                if (!MarketBrokerMutation.TrySubmit(
                        robot,
                        envelope.Intent,
                        enableMarketRange,
                        out result,
                        out mutationReason))
                {
                    bool uncertain =
                        string.Equals(
                            mutationReason,
                            "NULL TRADE RESULT",
                            StringComparison.Ordinal) ||
                        string.Equals(
                            mutationReason,
                            "BROKER ACCEPTED WITHOUT POSITION",
                            StringComparison.Ordinal);

                    Remember(key);

                    report =
                        BuildReport(
                            envelope,
                            uncertain
                                ? BrokerReportStatus.RecoveryRequired
                                : BrokerReportStatus.Rejected,
                            uncertain
                                ? BrokerAction.SubmitMarket
                                : ResolveBrokerAction(
                                    envelope.Intent,
                                    enableMarketRange),
                            nowUtc,
                            null,
                            mutationReason);

                    reason = mutationReason;
                    return false;
                }
            }
            catch (Exception ex)
            {
                Remember(key);

                report =
                    BuildReport(
                        envelope,
                        BrokerReportStatus.RecoveryRequired,
                        ResolveBrokerAction(
                            envelope.Intent,
                            enableMarketRange),
                        nowUtc,
                        null,
                        ex.Message);

                reason = "BROKER SUBMISSION EXCEPTION";
                return false;
            }

            Remember(key);

            report =
                BuildReport(
                    envelope,
                    BrokerReportStatus.Confirmed,
                    ResolveBrokerAction(
                        envelope.Intent,
                        enableMarketRange),
                    nowUtc,
                    result,
                    "POSITION CONFIRMED");

            reason = "POSITION #" +
                result.Position.Id;

            return true;
        }

        private static BrokerExecutionReport BuildReport(
            SignalEnvelope envelope,
            BrokerReportStatus status,
            BrokerAction action,
            DateTime nowUtc,
            TradeResult result,
            string reason)
        {
            long? positionId =
                result != null &&
                result.Position != null
                    ? result.Position.Id
                    : (long?)null;

            string brokerReference =
                positionId.HasValue
                    ? positionId.Value.ToString()
                    : "";

            string errorCode =
                result != null &&
                result.Error.HasValue
                    ? result.Error.Value.ToString()
                    : "";

            return new BrokerExecutionReport(
                envelope.Identity,
                action,
                status,
                nowUtc,
                nowUtc,
                status == BrokerReportStatus.Confirmed
                    ? nowUtc
                    : (DateTime?)null,
                positionId,
                null,
                result != null &&
                result.Position != null
                    ? result.Position.EntryPrice
                    : (double?)null,
                null,
                null,
                brokerReference,
                errorCode,
                reason,
                envelope.Identity.Revision);
        }

        private static BrokerAction ResolveBrokerAction(
            ExecutionIntent intent,
            bool enableMarketRange)
        {
            if (intent.Action == ExecutionAction.Aggressive)
                return BrokerAction.SubmitAggressive;

            return enableMarketRange &&
                intent.MarketProfile != null &&
                intent.MarketProfile.MarketRangePips > 0
                ? BrokerAction.SubmitMarketRange
                : BrokerAction.SubmitMarket;
        }

        private static bool HasLiveCapacity(
            Robot robot,
            string executionLabel,
            out string reason)
        {
            reason = "OK";

            if (string.IsNullOrWhiteSpace(executionLabel))
            {
                reason = "MISSING EXECUTION LABEL";
                return false;
            }

            if (!IsFinitePositive(robot.Symbol.Bid) ||
                !IsFinitePositive(robot.Symbol.Ask) ||
                !IsFinitePositive(robot.Symbol.PipSize))
            {
                reason = "INVALID LIVE QUOTE";
                return false;
            }

            int positions = 0;
            foreach (Position position in robot.Positions)
            {
                if (position != null &&
                    string.Equals(
                        position.SymbolName,
                        robot.SymbolName,
                        StringComparison.Ordinal) &&
                    string.Equals(
                        position.Label,
                        executionLabel,
                        StringComparison.Ordinal))
                {
                    positions++;
                }
            }

            int pendingOrders = 0;
            foreach (PendingOrder order in robot.PendingOrders)
            {
                if (order != null &&
                    string.Equals(
                        order.SymbolName,
                        robot.SymbolName,
                        StringComparison.Ordinal) &&
                    string.Equals(
                        order.Label,
                        executionLabel,
                        StringComparison.Ordinal))
                {
                    pendingOrders++;
                }
            }

            if (positions + pendingOrders >= 1)
            {
                reason = "SINGLE-PLAN CAPACITY BLOCKED";
                return false;
            }

            return true;
        }

        private void Remember(string key)
        {
            if (!_rememberedKeys.Add(key))
                return;

            _rememberedOrder.Enqueue(key);

            while (_rememberedOrder.Count > MaxRememberedKeys)
            {
                _rememberedKeys.Remove(
                    _rememberedOrder.Dequeue());
            }
        }

        private static bool IsFinitePositive(double value)
        {
            return
                !double.IsNaN(value) &&
                !double.IsInfinity(value) &&
                value > 0;
        }
    }
}
