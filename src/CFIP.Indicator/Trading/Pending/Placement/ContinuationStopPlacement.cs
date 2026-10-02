using System;
using cAlgo.API;

namespace cAlgo
{
    public partial class CFIPIndicator : Indicator
    {
        private bool PrepareContinuationStopForCbot(
            int closedM5)
        {
            if (_decision == null ||
                _decision.Direction == 0)
            {
                _autoOrdersBlockReason =
                    "PENDING STOP • NO M15 DECISION";
                return false;
            }

            int direction;
            double atr;
            double trigger;
            double stop;
            double target;
            double stopPips;
            double targetPips;
            double volume;
            ExecutionIntent pendingIntent;

            if (!TryPrepareContinuationStop(
                    closedM5,
                    out direction,
                    out atr,
                    out trigger,
                    out stop,
                    out target,
                    out stopPips,
                    out targetPips,
                    out volume,
                    out pendingIntent))
                return false;

            if (pendingIntent == null)
            {
                _autoOrdersBlockReason =
                    "PENDING STOP • EXECUTION INTENT UNAVAILABLE";
                return false;
            }

            // Keep one immutable absolute lifecycle snapshot at the same
            // analysis/intention boundary. This preserves the original pending
            // plan for broker-confirmed fill reconciliation without restoring any
            // broker mutation authority to the Indicator.
            Plan pendingSnapshot =
                CapturePendingOrderPlanSnapshot(
                    pendingIntent,
                    closedM5,
                    atr);

            if (pendingSnapshot == null)
            {
                _autoOrdersBlockReason =
                    "PENDING STOP • ABSOLUTE PLAN SNAPSHOT UNAVAILABLE";
                return false;
            }

            _pendingOrderPlanSnapshot =
                pendingSnapshot;

            // Analysis-owned state only. The cBot receives the captured immutable
            // intent through the provider; this method never mutates the broker.
            _activeExecutionScenarioId =
                ResolveDirectionExecutionScenarioId(
                    direction);

            _autoOrdersBlockReason =
                "PENDING STOP • READY FOR CBOT | " +
                (direction == 1 ? "BUY" : "SELL") +
                " | ENTRY " +
                trigger.ToString("F" + Symbol.Digits) +
                " | SL " +
                stopPips.ToString("F1") +
                "p | TP " +
                targetPips.ToString("F1") +
                "p";

            return true;
        }
    }
}
