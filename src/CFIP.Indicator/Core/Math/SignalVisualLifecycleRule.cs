namespace cAlgo
{
    internal readonly struct SignalVisualLifecycleInput
    {
        public bool LivePosition { get; }
        public bool PendingOrder { get; }
        public bool PreTradePlanPresent { get; }
        public int CreatedM5 { get; }
        public int CurrentM5 { get; }
        public int PlanDirection { get; }
        public int DecisionDirection { get; }
        public bool DecisionEntryAllowed { get; }
        public bool DecisionActionableNow { get; }
        public bool DecisionTriggerReady { get; }

        public SignalVisualLifecycleInput(
            bool livePosition,
            bool pendingOrder,
            bool preTradePlanPresent,
            int createdM5,
            int currentM5,
            int planDirection,
            int decisionDirection,
            bool decisionEntryAllowed,
            bool decisionActionableNow,
            bool decisionTriggerReady)
        {
            LivePosition = livePosition;
            PendingOrder = pendingOrder;
            PreTradePlanPresent = preTradePlanPresent;
            CreatedM5 = createdM5;
            CurrentM5 = currentM5;
            PlanDirection = planDirection;
            DecisionDirection = decisionDirection;
            DecisionEntryAllowed = decisionEntryAllowed;
            DecisionActionableNow = decisionActionableNow;
            DecisionTriggerReady = decisionTriggerReady;
        }
    }

    internal static class SignalVisualLifecycleRule
    {
        private const int PreTradeVisualExpiryBars = 2;

        public static bool IsSetupPreviewVisible(
            bool present,
            int createdM5,
            int currentM5,
            int direction,
            int decisionDirection,
            bool decisionEntryAllowed)
        {
            if (!present ||
                createdM5 < 0 ||
                currentM5 < createdM5 ||
                currentM5 - createdM5 >
                    PreTradeVisualExpiryBars)
                return false;

            if (direction != 1 &&
                direction != -1)
                return false;

            if (decisionDirection != 0 &&
                direction != decisionDirection)
                return false;

            // A setup preview is still pre-trigger, but it must already have
            // passed the canonical decision gates. Blocked/low-quality decisions
            // must never create a trade-looking visual on the chart.
            return decisionEntryAllowed;
        }

        public static bool IsSetupPreviewWithinPracticalDistance(
            double market,
            double proposedEntry,
            double atr,
            double configuredMaximumEntryDistanceAtr)
        {
            if (!NumericGuards.IsFinitePositive(market) ||
                !NumericGuards.IsFinitePositive(proposedEntry) ||
                !NumericGuards.IsFinitePositive(atr))
                return false;

            double configured =
                NumericGuards.IsFiniteValue(
                    configuredMaximumEntryDistanceAtr)
                    ? System.Math.Max(
                        0.05,
                        configuredMaximumEntryDistanceAtr)
                    : 0.45;

            // A preview is a nearby structural setup, not a multi-session forecast.
            // Keep it bounded by the existing entry-distance policy and cap the
            // display envelope at 1.5 ATR of the canonical M5 volatility.
            double maximumDistanceAtr =
                System.Math.Max(
                    0.80,
                    System.Math.Min(
                        1.50,
                        configured * 2.50));

            return
                System.Math.Abs(
                    proposedEntry -
                    market) /
                atr <=
                maximumDistanceAtr;
        }

        public static bool IsPreTradePlanVisible(
            SignalVisualLifecycleInput input)
        {
            if (input.LivePosition ||
                input.PendingOrder ||
                !input.PreTradePlanPresent)
                return false;

            if (input.CreatedM5 < 0 ||
                input.CurrentM5 < input.CreatedM5)
                return false;

            if (input.CurrentM5 - input.CreatedM5 >
                PreTradeVisualExpiryBars)
                return false;

            if (input.PlanDirection != 1 &&
                input.PlanDirection != -1)
                return false;

            if (input.DecisionDirection !=
                input.PlanDirection)
                return false;

            return
                input.DecisionEntryAllowed &&
                input.DecisionActionableNow &&
                input.DecisionTriggerReady;
        }
    }
}