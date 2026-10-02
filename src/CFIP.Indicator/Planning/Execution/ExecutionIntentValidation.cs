using System;
using System.Collections.Generic;
using System.Linq;
using cAlgo.API;
using cAlgo.API.Indicators;
using cAlgo.API.Internals;

namespace cAlgo
{
    public partial class CFIPIndicator : Indicator
    {
        private bool ValidateExecutionIntent(
            ExecutionIntent intent,
            double liveQuote,
            out string reason)
        {
            reason = "OK";

            if (intent == null ||
                (intent.Direction != 1 &&
                 intent.Direction != -1))
            {
                reason = "INVALID EXECUTION INTENT";
                return false;
            }

            ExecutionIntentGeometryResult geometry =
                ExecutionIntentGeometryRule.Evaluate(
                    intent.Direction,
                    intent.RequestedEntry,
                    intent.Stop,
                    intent.Target,
                    Symbol.PipSize);

            if (!geometry.Valid ||
                !IsExecutionPlanConsistent(
                    intent.Direction,
                    intent.RequestedEntry,
                    intent.Stop,
                    intent.Target))
            {
                reason =
                    geometry.Valid
                        ? "INCONSISTENT EXECUTION INTENT"
                        : geometry.Reason;
                return false;
            }

            if (!IsFinitePositive(intent.Volume) ||
                intent.StopPips <= 0 ||
                intent.TargetPips <= 0)
            {
                reason = "INCOMPLETE EXECUTION INTENT";
                return false;
            }

            if (Math.Abs(intent.StopPips - geometry.StopPips) > 1e-12 ||
                Math.Abs(intent.TargetPips - geometry.TargetPips) > 1e-12)
            {
                reason = "EXECUTION INTENT PIP GEOMETRY MISMATCH";
                return false;
            }

            if (intent.Kind ==
                ExecutionIntentKind.Market)
            {
                if (!IsFinitePositive(liveQuote))
                {
                    reason = "INVALID MARKET QUOTE";
                    return false;
                }
            }
            else if (intent.Kind ==
                     ExecutionIntentKind.Stop)
            {
                if (!SamePrice(
                    intent.RequestedEntry,
                    intent.Trigger) ||
                    !IsValidPendingEntry(
                        intent.Direction,
                        intent.RequestedEntry,
                        true))
                {
                    reason = "INVALID STOP INTENT";
                    return false;
                }
            }
            else if (intent.Kind ==
                     ExecutionIntentKind.Limit)
            {
                if (!IsValidPendingEntry(
                    intent.Direction,
                    intent.RequestedEntry,
                    false))
                {
                    reason = "INVALID LIMIT INTENT";
                    return false;
                }
            }

            return true;
        }

        private bool ValidateActualMarketFill(
            ExecutionIntent intent,
            double actualFill,
            double atr,
            out string reason)
        {
            reason = "OK";

            if (intent == null)
            {
                reason = "INVALID ACTUAL FILL";
                return false;
            }

            if (!ExecutionFillAcceptanceRule.IsAcceptable(
                intent.Direction,
                intent.RequestedEntry,
                actualFill,
                atr,
                MaximumEntryExtensionAtr,
                true))
            {
                reason =
                    !IsFinitePositive(actualFill) ||
                    !IsFinitePositive(atr)
                        ? "INVALID ACTUAL FILL"
                        : "BROKER FILL FAR FROM INTENT";
                return false;
            }

            return true;
        }
    }
}