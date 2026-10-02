using System;
using CFIP.Contracts;

namespace CFIP.cBot.Execution
{
    public static class MarketExecutionIntentRule
    {
        public static bool Validate(
            ExecutionIntent intent,
            out string reason)
        {
            reason = "OK";

            if (intent == null ||
                intent.Identity == null)
            {
                reason = "MISSING MARKET INTENT";
                return false;
            }

            if (intent.Action != ExecutionAction.Market &&
                intent.Action != ExecutionAction.Aggressive)
            {
                reason = "UNSUPPORTED MARKET ACTION";
                return false;
            }

            if (intent.Identity.Direction != TradeDirection.Buy &&
                intent.Identity.Direction != TradeDirection.Sell)
            {
                reason = "INVALID MARKET DIRECTION";
                return false;
            }

            if (string.IsNullOrWhiteSpace(intent.ExecutionLabel))
            {
                reason = "MISSING EXECUTION LABEL";
                return false;
            }

            if (!IsFinitePositive(intent.RequestedEntry) ||
                !IsFinitePositive(intent.Stop) ||
                !IsFinitePositive(intent.InitialTarget))
            {
                reason = "INVALID MARKET GEOMETRY";
                return false;
            }

            if (!intent.RequestedVolume.HasValue ||
                !IsFinitePositive(intent.RequestedVolume.Value))
            {
                reason = "INVALID MARKET VOLUME";
                return false;
            }

            MarketExecutionProfile profile = intent.MarketProfile;
            if (profile == null)
            {
                reason = "MISSING MARKET PROFILE";
                return false;
            }

            if (!IsFiniteNonNegative(profile.MarketRangePips) ||
                !IsFinitePositive(profile.StopPips) ||
                !IsFinitePositive(profile.TargetPips))
            {
                reason = "INVALID MARKET PROFILE";
                return false;
            }

            if (profile.UseServerTakeProfitLadder)
            {
                if (!IsFinitePositive(profile.Tp1Pips) ||
                    !IsFinitePositive(profile.Tp2Pips) ||
                    !IsFinitePositive(profile.FinalTpPips) ||
                    !IsFinitePositive(profile.Tp1Volume) ||
                    !IsFinitePositive(profile.Tp2Volume) ||
                    profile.Tp2Pips <= profile.Tp1Pips ||
                    profile.FinalTpPips <= profile.Tp2Pips)
                {
                    reason = "INVALID MARKET TP LADDER";
                    return false;
                }

                if (profile.BreakEvenTriggerPips.HasValue &&
                    !IsFiniteNonNegative(profile.BreakEvenTriggerPips.Value))
                {
                    reason = "INVALID BREAK-EVEN TRIGGER";
                    return false;
                }

                if (profile.BreakEvenOffsetPips.HasValue &&
                    !IsFiniteNonNegative(profile.BreakEvenOffsetPips.Value))
                {
                    reason = "INVALID BREAK-EVEN OFFSET";
                    return false;
                }
            }

            return true;
        }

        private static bool IsFinitePositive(double value)
        {
            return
                !double.IsNaN(value) &&
                !double.IsInfinity(value) &&
                value > 0;
        }

        private static bool IsFiniteNonNegative(double value)
        {
            return
                !double.IsNaN(value) &&
                !double.IsInfinity(value) &&
                value >= 0;
        }
    }
}
