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

            // Once a plan has passed the canonical decision gates, its Entry /
            // Trigger / SL / TP geometry belongs to that plan lifecycle. A
            // transient change in execution actionability must not erase the
            // geometry while the plan is still current. Replacement, direction
            // change, pending conversion, live activation, or expiry owns removal.
            return true;
        }
    }
}
