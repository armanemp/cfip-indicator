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
                    string.Equals(position.Label, executionLabel, StringComparison.Ordinal))
                    count++;
            }
            return count;
        }

        public static int CountManagedPending(
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

        public static int CountManagedScenarioObjects(
            Robot robot,
            string managedInstanceLabel)
        {
            if (robot == null ||
                string.IsNullOrWhiteSpace(managedInstanceLabel))
                return 0;

            int count = 0;

            foreach (Position position in robot.Positions)
            {
                if (position != null &&
                    string.Equals(
                        position.SymbolName,
                        robot.SymbolName,
                        StringComparison.Ordinal) &&
                    IsScenarioLabel(
                        position.Label,
                        managedInstanceLabel))
                {
                    count++;
                }
            }

            foreach (PendingOrder order in robot.PendingOrders)
            {
                if (order != null &&
                    string.Equals(
                        order.SymbolName,
                        robot.SymbolName,
                        StringComparison.Ordinal) &&
                    IsScenarioPendingLabel(
                        order.Label,
                        managedInstanceLabel))
                {
                    count++;
                }
            }

            return count;
        }

        public static bool IsScenarioLabel(
            string actualLabel,
            string managedInstanceLabel)
        {
            if (string.IsNullOrWhiteSpace(actualLabel) ||
                string.IsNullOrWhiteSpace(managedInstanceLabel))
                return false;

            string root =
                managedInstanceLabel.Trim();

            return
                string.Equals(
                    actualLabel,
                    root,
                    StringComparison.Ordinal) ||
                actualLabel.StartsWith(
                    root + "|CFIP-S:",
                    StringComparison.Ordinal);
        }

        public static bool IsScenarioPendingLabel(
            string actualLabel,
            string managedInstanceLabel)
        {
            if (string.IsNullOrWhiteSpace(actualLabel) ||
                string.IsNullOrWhiteSpace(managedInstanceLabel))
                return false;

            const string pendingSuffix = "-PENDING";

            if (!actualLabel.EndsWith(
                    pendingSuffix,
                    StringComparison.Ordinal))
                return false;

            string root =
                actualLabel.Substring(
                    0,
                    actualLabel.Length -
                    pendingSuffix.Length);

            return IsScenarioLabel(
                root,
                managedInstanceLabel);
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