using System;
using cAlgo.API;

namespace cAlgo
{
    public partial class CFIPIndicator : Indicator
    {
        private double CalculateAutomaticMarketRangePips(
            int closedM5,
            double basePrice)
        {
            if (_m5Bars == null ||
                closedM5 < 1 ||
                !IsFinitePositive(basePrice) ||
                !IsFinitePositive(Symbol.PipSize))
                return 0;

            double atr =
                Atr(
                    _m5Bars,
                    closedM5);

            if (!IsFinitePositive(atr))
                return 0;

            double spread =
                Math.Max(
                    0,
                    Symbol.Ask - Symbol.Bid);

            double spreadPips =
                spread /
                Math.Max(
                    Symbol.PipSize,
                    1e-9);

            double atrPips =
                atr /
                Math.Max(
                    Symbol.PipSize,
                    1e-9);

            // Market-range is a bounded execution envelope, not permission to
            // chase price. Keep it close to the live spread while allowing a
            // small ATR-scaled tolerance for normal quote movement.
            double maximum =
                atrPips *
                Math.Max(
                    0.08,
                    Math.Min(
                        0.20,
                        Math.Max(
                            0.08,
                            MaximumEntryExtensionAtr * 0.50)));

            if (maximum <= 0)
                return 0;

            double minimumUsableRange =
                Math.Max(
                    0.10,
                    spreadPips * 1.10);

            double desired =
                Math.Max(
                    minimumUsableRange,
                    atrPips * 0.02);

            // The market-range envelope is always capped by the ATR-derived
            // maximum; it may never silently exceed the execution extension gate.
            return Math.Max(
                0.10,
                Math.Min(
                    maximum,
                    desired));
        }

        private void ExecutePreparedAutomaticMarketTrade(
            int closedM5,
            TradeType type,
            double entry,
            double stopPips,
            double targetPips,
            double target,
            double volume)
        {
            try
            {
                if (!CanRunAutomaticEntry())
                {
                    ApplyRuntimeEntryGate();
                    return;
                }

                string submissionReason;
                if (!TryValidateAutomaticMarketSubmission(
                        closedM5,
                        type,
                        entry,
                        target,
                        volume,
                        out submissionReason))
                {
                    _autoExecutionBlockReason =
                        submissionReason;

                    SetAutoTradingState(
                        "BLOCKED",
                        submissionReason);
                    return;
                }

                string submissionGateReason;
                SubmissionAttemptIdentity submissionIdentity;

                if (!TryAcquireSubmission(
                        closedM5,
                        _plan == null
                            ? 0
                            : _plan.Direction,
                        ExecutionSubmissionPath.AutomaticMarket,
                        out submissionIdentity,
                        out submissionGateReason))
                {
                    _autoExecutionBlockReason =
                        submissionGateReason;

                    SetAutoTradingState(
                        "BLOCKED",
                        submissionGateReason);
                    return;
                }

                double marketRangePips =
                    CalculateAutomaticMarketRangePips(
                        closedM5,
                        entry);

                if (marketRangePips <= 0)
                {
                    _autoExecutionBlockReason =
                        "MARKET RANGE UNAVAILABLE";

                    SetAutoTradingState(
                        "BLOCKED",
                        _autoExecutionBlockReason);
                    return;
                }

                TradeResult result;

                try
                {
                    result =
                        TryExecuteMarketRangeOrder(
                            type,
                            SymbolName,
                            volume,
                            marketRangePips,
                            entry,
                            NormalizeLabel(),
                            stopPips,
                            targetPips,
                            TradeExecutionMetadata.DefaultExecutionComment,
                            false,
                            "AUTOMATIC MARKET RANGE");
                }
                catch
                {
                    RecordSubmissionFailure(submissionIdentity);
                    throw;
                }

                RecordSubmission(submissionIdentity, result);

                if (result == null)
                {
                    _autoExecutionBlockReason =
                        "NULL TRADE RESULT";
                    SetAutoTradingState(
                        "ERROR",
                        "NULL TRADE RESULT");
                    return;
                }

                if (!BrokerConfirmationPolicy.CanAdoptPosition(
                        true,
                        result.IsSuccessful,
                        result.Position != null))
                {
                    _autoExecutionBlockReason =
                        result.Error.HasValue
                            ? result.Error.Value.ToString()
                            : "TRADE REJECTED";

                    SetAutoTradingState(
                        "ERROR",
                        _autoExecutionBlockReason);
                    return;
                }

                if (!TryAcceptAutomaticMarketFill(
                        closedM5,
                        result))
                    return;

                if (!TryResolveAutomaticPostFillTarget(
                        closedM5,
                        result.Position,
                        ref target))
                    return;

                bool protectionOk = true;

                if (AutoBrokerProtection)
                {
                    protectionOk =
                        EnsureBrokerProtectionForPosition(
                            result.Position,
                            _plan.Stop,
                            target,
                            "NEW MARKET ENTRY",
                            _plan.Direction);
                }

                SetAutoTradingState(
                    protectionOk
                        ? "EXECUTED"
                        : "RECOVERY",
                    protectionOk
                        ? "POSITION #" +
                          result.Position.Id
                        : "POSITION #" +
                          result.Position.Id +
                          " • BROKER PROTECTION RECOVERY");

                double confirmedStop =
                    GetActiveBrokerStopPrice();

                double confirmedTarget =
                    GetActiveBrokerTargetPrice();

                SendUnifiedAlert(
                    "AUTO|" +
                    closedM5,
                    "CFIP AUTO " +
                    (_plan.Direction == 1
                        ? "BUY"
                        : "SELL") +
                    " EXECUTED | #" +
                    result.Position.Id +
                    " | ENTRY " +
                    Price(
                        result.Position.EntryPrice) +
                    " | BROKER SL " +
                    (IsFinitePositive(confirmedStop)
                        ? Price(confirmedStop)
                        : "RECOVERY") +
                    " | BROKER TP " +
                    (IsFinitePositive(confirmedTarget)
                        ? Price(confirmedTarget)
                        : "RECOVERY"),
                    _plan.Direction,
                    true);
            }
            catch (Exception ex)
            {
                SetAutoTradingState(
                    "ERROR",
                    ex.Message);

                Print(
                    "CFIP auto trade failed: {0}",
                    ex.Message);
            }
        }
    }
}
