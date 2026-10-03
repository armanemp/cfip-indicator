using System;
using System.Collections.Generic;
using cAlgo.API;
using CFIP.Contracts;

namespace CFIP.cBot.Execution
{
    internal sealed class DemoMarketExecutionCoordinator
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
                reason = "NO EXECUTION ENVELOPE";
                return false;
            }

            bool marketAction =
                envelope.Intent.Action == ExecutionAction.Market;

            bool aggressiveAction =
                envelope.Intent.Action == ExecutionAction.Aggressive;

            if (!marketAction && !aggressiveAction)
            {
                reason = "DEMO BRIDGE SUPPORTS MARKET AND AGGRESSIVE ACTIONS ONLY";
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
                reason = "MISSING EXECUTION LABEL";
                return false;
            }

            if (!IsFinitePositive(envelope.Intent.RequestedEntry) ||
                !IsFinitePositive(envelope.Intent.Stop) ||
                !IsFinitePositive(envelope.Intent.InitialTarget) ||
                !envelope.Intent.RequestedVolume.HasValue ||
                !IsFinitePositive(envelope.Intent.RequestedVolume.Value))
            {
                reason = "INVALID EXECUTION GEOMETRY OR VOLUME";
                return false;
            }

            if (envelope.Identity.Direction == TradeDirection.Buy &&
                (envelope.Intent.Stop >= envelope.Intent.RequestedEntry ||
                 envelope.Intent.InitialTarget <= envelope.Intent.RequestedEntry))
            {
                reason = "BUY EXECUTION GEOMETRY WRONG SIDE";
                return false;
            }

            if (envelope.Identity.Direction == TradeDirection.Sell &&
                (envelope.Intent.Stop <= envelope.Intent.RequestedEntry ||
                 envelope.Intent.InitialTarget >= envelope.Intent.RequestedEntry))
            {
                reason = "SELL EXECUTION GEOMETRY WRONG SIDE";
                return false;
            }

            double volume = envelope.Intent.RequestedVolume.Value;

            if (robot.Symbol.VolumeInUnitsMin > 0 &&
                volume < robot.Symbol.VolumeInUnitsMin)
            {
                reason = "REQUESTED VOLUME BELOW BROKER MINIMUM";
                return false;
            }

            if (robot.Symbol.VolumeInUnitsMax > 0 &&
                volume > robot.Symbol.VolumeInUnitsMax)
            {
                reason = "REQUESTED VOLUME ABOVE BROKER MAXIMUM";
                return false;
            }

            string marginReason;
            if (!BrokerExecutionSafety.TryConstrainVolumeForMargin(
                    robot,
                    envelope.Identity.Direction == TradeDirection.Buy
                        ? TradeType.Buy
                        : TradeType.Sell,
                    volume,
                    maximumMarginUsagePercent,
                    marginBufferPercent,
                    out volume,
                    out marginReason))
            {
                reason = marginReason;
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

            if (managedPositions + managedPending >= 1)
            {
                reason = "SCENARIO CAPACITY BLOCKED • SCENARIO ALREADY ACTIVE";
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

            if (!IsFinitePositive(robot.Symbol.Bid) ||
                !IsFinitePositive(robot.Symbol.Ask) ||
                !IsFinitePositive(robot.Symbol.PipSize))
            {
                reason = "INVALID LIVE QUOTE";
                return false;
            }

            double pipSize = robot.Symbol.PipSize;
            double stopPips =
                Math.Abs(
                    envelope.Intent.RequestedEntry -
                    envelope.Intent.Stop) /
                pipSize;
            double targetPips =
                Math.Abs(
                    envelope.Intent.InitialTarget -
                    envelope.Intent.RequestedEntry) /
                pipSize;

            if (envelope.Intent.MarketProfile != null &&
                IsFinitePositive(
                    envelope.Intent.MarketProfile.StopPips))
                stopPips =
                    envelope.Intent.MarketProfile.StopPips;

            if (envelope.Intent.MarketProfile != null &&
                IsFinitePositive(
                    envelope.Intent.MarketProfile.TargetPips))
                targetPips =
                    envelope.Intent.MarketProfile.TargetPips;

            if (!IsFinitePositive(stopPips) ||
                !IsFinitePositive(targetPips))
            {
                reason = "INVALID STOP/TARGET DISTANCE";
                return false;
            }

            double marketRangePips =
                envelope.Intent.MarketProfile == null
                    ? 0
                    : envelope.Intent.MarketProfile.MarketRangePips;

            if (marketRangePips < 0 ||
                double.IsNaN(marketRangePips) ||
                double.IsInfinity(marketRangePips))
            {
                reason = "INVALID MARKET RANGE";
                return false;
            }

            if (marketRangePips > 0)
            {
                double reference =
                    envelope.Intent.RequestedEntry;

                double liveQuote =
                    envelope.Identity.Direction == TradeDirection.Buy
                        ? robot.Symbol.Ask
                        : robot.Symbol.Bid;

                if (!IsFinitePositive(liveQuote) ||
                    Math.Abs(liveQuote - reference) >
                    marketRangePips * pipSize)
                {
                    reason = "LIVE QUOTE OUTSIDE MARKET RANGE";
                    return false;
                }
            }

            TradeType tradeType =
                envelope.Identity.Direction == TradeDirection.Buy
                    ? TradeType.Buy
                    : TradeType.Sell;

            TradeResult result;
            try
            {
                bool useMarketRange =
                    marketAction &&
                    marketRangePips > 0;

                result =
                    useMarketRange
                        ? robot.ExecuteMarketRangeOrder(
                            tradeType,
                            robot.SymbolName,
                            volume,
                            marketRangePips,
                            envelope.Intent.RequestedEntry,
                            executionLabel,
                            stopPips,
                            targetPips,
                            "CFIP DEMO",
                            false)
                        : robot.ExecuteMarketOrder(
                            tradeType,
                            robot.SymbolName,
                            volume,
                            executionLabel,
                            stopPips,
                            targetPips,
                            aggressiveAction
                                ? "CFIP DEMO AGGRESSIVE"
                                : "CFIP DEMO",
                            false);
            }
            catch (Exception ex)
            {
                if (idempotencyStore != null)
                    idempotencyStore.RecordAttempt(
                        robot,
                        key,
                        false,
                        nowUtc);
                reason = "BROKER SUBMISSION EXCEPTION";
                report = BuildReport(
                    envelope,
                    BrokerReportStatus.RecoveryRequired,
                    nowUtc,
                    null,
                    ex.Message);
                return false;
            }

            if (result == null)
            {
                if (idempotencyStore != null)
                    idempotencyStore.RecordAttempt(
                        robot,
                        key,
                        false,
                        nowUtc);

                reason = "NULL TRADE RESULT";
                report = BuildReport(
                    envelope,
                    BrokerReportStatus.RecoveryRequired,
                    nowUtc,
                    null,
                    reason);
                return false;
            }

            if (!result.IsSuccessful ||
                result.Position == null)
            {
                reason =
                    result.Error.HasValue
                        ? result.Error.Value.ToString()
                        : "BROKER REJECTED";

                report = BuildReport(
                    envelope,
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

            bool protectionConfirmed =
                result.Position.StopLoss.HasValue &&
                result.Position.TakeProfit.HasValue &&
                IsFinitePositive(
                    result.Position.StopLoss.Value) &&
                IsFinitePositive(
                    result.Position.TakeProfit.Value);

            BrokerReportStatus confirmationStatus =
                protectionConfirmed
                    ? BrokerReportStatus.Confirmed
                    : BrokerReportStatus.RecoveryRequired;

            reason =
                protectionConfirmed
                    ? "POSITION #" + result.Position.Id
                    : "POSITION #" + result.Position.Id +
                      " • BROKER PROTECTION INCOMPLETE";

            Remember(key);

            report = BuildReport(
                envelope,
                confirmationStatus,
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
            BrokerReportStatus status,
            DateTime nowUtc,
            TradeResult result,
            string reason)
        {
            long? positionId =
                result != null && result.Position != null
                    ? result.Position.Id
                    : (long?)null;

            double? entryPrice =
                result != null && result.Position != null
                    ? result.Position.EntryPrice
                    : (double?)null;

            double? confirmedStop =
                result != null &&
                result.Position != null &&
                result.Position.StopLoss.HasValue
                    ? result.Position.StopLoss.Value
                    : (double?)null;

            double? confirmedTarget =
                result != null &&
                result.Position != null &&
                result.Position.TakeProfit.HasValue
                    ? result.Position.TakeProfit.Value
                    : (double?)null;

            string reference =
                positionId.HasValue
                    ? positionId.Value.ToString()
                    : "";

            string errorCode =
                result != null && result.Error.HasValue
                    ? result.Error.Value.ToString()
                    : "";

            return new BrokerExecutionReport(
                envelope.Identity,
                envelope.Intent != null &&
                envelope.Intent.Action == ExecutionAction.Aggressive
                    ? BrokerAction.SubmitAggressive
                    : envelope.Intent.MarketProfile != null &&
                      envelope.Intent.MarketProfile.MarketRangePips > 0
                        ? BrokerAction.SubmitMarketRange
                        : BrokerAction.SubmitMarket,
                status,
                nowUtc,
                nowUtc,
                status == BrokerReportStatus.Confirmed
                    ? nowUtc
                    : (DateTime?)null,
                positionId,
                null,
                entryPrice,
                confirmedStop,
                confirmedTarget,
                reference,
                errorCode,
                reason,
                envelope.Identity.Revision);
        }

        private void Remember(string key)
        {
            if (!_rememberedKeys.Add(key))
                return;

            _rememberedOrder.Enqueue(key);

            while (_rememberedOrder.Count > MaxRememberedKeys)
            {
                _rememberedKeys.Remove(_rememberedOrder.Dequeue());
            }
        }

        private static bool IsFinitePositive(double value)
        {
            return !double.IsNaN(value) &&
                   !double.IsInfinity(value) &&
                   value > 0;
        }
    }
}