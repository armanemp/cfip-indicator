using System;
using cAlgo.API;
using CFIP.Contracts;

namespace CFIP.cBot.Execution
{
    internal sealed class CbotExecutionEnvironmentGate
    {
        public bool Evaluate(
            Robot robot,
            SignalEnvelope envelope,
            double maximumMarginUsagePercent,
            double marginBufferPercent,
            out string reason)
        {
            reason = "OK";

            if (robot == null ||
                envelope == null ||
                envelope.Identity == null ||
                envelope.Intent == null)
            {
                reason = "EXECUTION ENVIRONMENT INPUT INVALID";
                return false;
            }

            if (robot.Account == null ||
                robot.Account.IsLive)
            {
                reason = "LIVE ACCOUNT BLOCKED";
                return false;
            }

            try
            {
                if (!Permissions.TradingPermission.IsAllowed)
                {
                    reason = "TRADING PERMISSION NOT GRANTED";
                    return false;
                }
            }
            catch
            {
                reason = "TRADING PERMISSION STATE UNAVAILABLE";
                return false;
            }

            if (!string.Equals(
                    envelope.Identity.Symbol,
                    robot.SymbolName,
                    StringComparison.Ordinal))
            {
                reason = "EXECUTION SYMBOL MISMATCH";
                return false;
            }

            if (envelope.Identity.Direction != TradeDirection.Buy &&
                envelope.Identity.Direction != TradeDirection.Sell)
            {
                reason = "EXECUTION DIRECTION INVALID";
                return false;
            }

            if (!FinitePositive(envelope.Intent.RequestedEntry) ||
                !FinitePositive(envelope.Intent.Stop) ||
                !FinitePositive(envelope.Intent.InitialTarget) ||
                !envelope.Intent.RequestedVolume.HasValue ||
                !FinitePositive(envelope.Intent.RequestedVolume.Value))
            {
                reason = "EXECUTION GEOMETRY INVALID";
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

            if (!FinitePositive(robot.Symbol.Bid) ||
                !FinitePositive(robot.Symbol.Ask) ||
                !FinitePositive(robot.Symbol.PipSize))
            {
                reason = "LIVE QUOTE INVALID";
                return false;
            }

            int positions =
                BrokerExecutionSafety.CountManagedPositions(
                    robot,
                    envelope.Intent.ExecutionLabel ?? string.Empty);

            int pending =
                BrokerExecutionSafety.CountManagedPending(
                    robot,
                    envelope.Intent.ExecutionLabel ?? string.Empty);

            if (positions + pending >= 1)
            {
                reason = "SINGLE-PLAN CAPACITY BLOCKED";
                return false;
            }

            if (!FinitePositive(robot.Account.FreeMargin))
            {
                reason = "FREE MARGIN INVALID";
                return false;
            }

            if (robot.Account.MarginLevel.HasValue &&
                (!FinitePositive(robot.Account.MarginLevel.Value) ||
                 (robot.Account.StopOutLevel > 0 &&
                  robot.Account.MarginLevel.Value <=
                  robot.Account.StopOutLevel)))
            {
                reason = "ACCOUNT MARGIN LEVEL UNSAFE";
                return false;
            }

            if (envelope.Plan == null ||
                !FinitePositive(envelope.Plan.PlanRisk))
            {
                reason = "PLAN RISK UNAVAILABLE";
                return false;
            }

            double spread =
                robot.Symbol.Ask -
                robot.Symbol.Bid;

            if (!FinitePositive(spread))
            {
                reason = "SPREAD INVALID";
                return false;
            }

            double spreadLimit =
                envelope.Intent.MaxSpreadToStopRiskRatio;

            if (!FinitePositive(spreadLimit))
            {
                reason = "SPREAD RISK LIMIT UNAVAILABLE";
                return false;
            }

            if (spread / envelope.Plan.PlanRisk >
                Math.Max(0.02, spreadLimit))
            {
                reason = "LIVE SPREAD EXCEEDS PLAN RISK LIMIT";
                return false;
            }

            if (!FinitePositive(maximumMarginUsagePercent) ||
                !FinitePositive(marginBufferPercent) ||
                marginBufferPercent >=
                maximumMarginUsagePercent)
            {
                reason = "MARGIN SAFETY CONFIGURATION INVALID";
                return false;
            }

            return true;
        }

        private static bool FinitePositive(double value)
        {
            return !double.IsNaN(value) &&
                   !double.IsInfinity(value) &&
                   value > 0;
        }
    }
}
