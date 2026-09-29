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
