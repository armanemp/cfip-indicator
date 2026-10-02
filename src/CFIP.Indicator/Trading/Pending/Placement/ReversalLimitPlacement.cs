using System;
using cAlgo.API;

namespace cAlgo
{
    public partial class CFIPIndicator : Indicator
    {
        private bool PrepareReversalLimitForCbot(
            int closedM5)
        {
            int direction;
            double atr;
            double targetEntry;
            double stop;
            double target;
            double stopPips;
            double targetPips;
            double volume;
            ExecutionIntent pendingIntent;

            if (!TryPrepareReversalLimit(
                    closedM5,
                    out direction,
                    out atr,
                    out targetEntry,
                    out stop,
                    out target,
                    out stopPips,
                    out targetPips,
                    out volume,
                    out pendingIntent))
            {
                return false;
            }

            if (pendingIntent == null)
            {
                _autoOrdersBlockReason =
                    "PENDING LIMIT • EXECUTION INTENT UNAVAILABLE";
                return false;
            }

            Plan pendingSnapshot =
                CapturePendingOrderPlanSnapshot(
                    pendingIntent,
                    closedM5,
                    atr);

            if (pendingSnapshot == null)
            {
                _autoOrdersBlockReason =
                    "PENDING LIMIT • ABSOLUTE PLAN SNAPSHOT UNAVAILABLE";
                return false;
            }

            string executionScenarioId =
                ResolveDirectionExecutionScenarioId(
                    direction);

            if (string.IsNullOrWhiteSpace(
                    executionScenarioId))
            {
                _autoOrdersBlockReason =
                    "PENDING LIMIT • SCENARIO ID UNAVAILABLE";
                return false;
            }

            _activeExecutionScenarioId =
                executionScenarioId;

            _pendingOrderPlanSnapshot =
                pendingSnapshot;

            _autoOrdersBlockReason =
                "PENDING LIMIT • READY FOR CBOT | " +
                (direction == 1 ? "BUY" : "SELL") +
                " | ENTRY " +
                targetEntry.ToString(
                    "F" + Symbol.Digits) +
                " | SL " +
                stopPips.ToString("F1") +
                "p | TP " +
                targetPips.ToString("F1") +
                "p";

            return true;
        }
    }
}
