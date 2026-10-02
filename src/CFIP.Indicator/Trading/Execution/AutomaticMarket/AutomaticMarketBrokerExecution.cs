using System;
using cAlgo.API;

namespace cAlgo
{
    public partial class CFIPIndicator : Indicator
    {
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
                string submissionReason;
                ExecutionIntent validatedIntent;

                if (!TryValidateAutomaticMarketSubmission(
                        closedM5,
                        type,
                        entry,
                        target,
                        volume,
                        out validatedIntent,
                        out submissionReason))
                {
                    _autoExecutionBlockReason =
                        submissionReason;
                    SetAutoTradingState(
                        "BLOCKED",
                        submissionReason);
                    RefreshReadOnlyProvider(closedM5);
                    return;
                }

                if (validatedIntent == null)
                {
                    _autoExecutionBlockReason =
                        "VALIDATED EXECUTION INTENT UNAVAILABLE";
                    SetAutoTradingState(
                        "BLOCKED",
                        _autoExecutionBlockReason);
                    RefreshReadOnlyProvider(closedM5);
                    return;
                }

                double marketRangePips =
                    CalculateAutomaticMarketRangePips(
                        closedM5,
                        validatedIntent.RequestedEntry);

                if (!IsFinitePositive(marketRangePips))
                {
                    _autoExecutionBlockReason =
                        "MARKET RANGE UNAVAILABLE";
                    SetAutoTradingState(
                        "BLOCKED",
                        _autoExecutionBlockReason);
                    RefreshReadOnlyProvider(closedM5);
                    return;
                }

                RelativeTakeProfitProtections serverTakeProfits;
                StopLossBreakEven serverBreakEven;

                // Build the same server-protection profile as the former
                // broker path, but transport it as immutable intent data only.
                TryBuildServerSideTakeProfitLadder(
                    validatedIntent.RequestedEntry,
                    validatedIntent.Target,
                    validatedIntent.Volume,
                    out serverTakeProfits,
                    out serverBreakEven);

                // CBOT cBot handoff — Indicator validates and publishes the
                // exact immutable intent/profile; it never mutates the broker.
                SetAutoTradingState(
                    "CBOT HANDOFF",
                    validatedIntent.Kind == ExecutionIntentKind.Market
                        ? "MARKET INTENT READY"
                        : "EXECUTION INTENT READY");

                RefreshReadOnlyProvider(closedM5);
            }
            catch (Exception ex)
            {
                _autoExecutionBlockReason =
                    ex.Message;
                SetAutoTradingState(
                    "BLOCKED",
                    "CBOT HANDOFF • " + ex.Message);
                RefreshReadOnlyProvider(closedM5);
            }
        }
    }
}
