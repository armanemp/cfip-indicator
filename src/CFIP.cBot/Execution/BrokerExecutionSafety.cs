using System;
using cAlgo.API;

namespace CFIP.cBot.Execution
{
    internal static class BrokerExecutionSafety
    {
        public static int CountManagedPositions(
            Robot robot,
            string executionLabel)
        {
            int count = 0;
            foreach (Position position in robot.Positions)
            {
                if (position != null &&
                    string.Equals(position.SymbolName, robot.SymbolName, StringComparison.Ordinal) &&
                    CbotManagedObjectIdentityRule.MatchesManagedLabel(
                        position.Label,
                        executionLabel))
                    count++;
            }
            return count;
        }

        public static int CountManagedPending(
            Robot robot,
            string executionLabel)
        {
            int count = 0;
            foreach (PendingOrder order in robot.PendingOrders)
            {
                if (order != null &&
                    string.Equals(order.SymbolName, robot.SymbolName, StringComparison.Ordinal) &&
                    CbotManagedObjectIdentityRule.MatchesManagedPendingLabel(
                        order.Label,
                        executionLabel))
                    count++;
            }
            return count;
        }

        public static bool TryConstrainVolumeForMargin(
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

            if (robot == null ||
                !IsFinitePositiveBrokerSafety(requestedVolume))
                return false;

            double allowedMargin =
                Risk.ExecutionMarginBudgetRule.AllowedMargin(
                    robot.Account.FreeMargin,
                    maximumMarginUsagePercent,
                    marginBufferPercent);

            if (!IsFinitePositiveBrokerSafety(allowedMargin))
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

            if (!IsFinitePositiveBrokerSafety(estimatedMargin))
            {
                reason = "EXECUTION MARGIN ESTIMATE INVALID";
                return false;
            }

            double candidate =
                Risk.ExecutionMarginBudgetRule.ScaleVolumeToBudget(
                    requestedVolume,
                    estimatedMargin,
                    allowedMargin);

            if (!IsFinitePositiveBrokerSafety(candidate))
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

            if (!IsFinitePositiveBrokerSafety(finalEstimatedMargin) ||
                finalEstimatedMargin > allowedMargin)
            {
                reason = "FINAL EXECUTION MARGIN BUDGET EXCEEDED";
                return false;
            }

            return true;
        }

        private static bool IsFinitePositiveBrokerSafety(double value)
        {
            return !double.IsNaN(value) &&
                   !double.IsInfinity(value) &&
                   value > 0;
        }
    }
}