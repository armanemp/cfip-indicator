namespace cAlgo
{
    public enum PanelCorner
        {
            TopLeft,
            TopRight,
            BottomLeft,
            BottomRight
        }
    
        public enum SizingMode
        {
            RiskPercentEquity = 0,
            FixedLots = 1
        }
    
        public enum TargetStage
        {
            TP1 = 0,
            TP2 = 1,
            TP3 = 2,
            TP4 = 3
        }
    
        public enum PendingOrderMode
        {
            Adaptive = 0,
            ContinuationStop = 1,
            ReversalLimit = 2,
            Both = 3
        }
    
        public enum ExecutionMode
        {
            None = 0,
            WaitingForTrigger = 1,
            RetestMarket = 2,
            BreakoutMarket = 3,
            ContinuationStop = 4,
            ReversalLimit = 5
        }
    
        public enum DecisionPolicyMode
        {
            Confirmed = 0,
            Soft = 1,
            Aggressive = 2,
            Pending = 3
        }
    
        public enum ExecutionIntentKind
        {
            Market = 0,
            Stop = 1,
            Limit = 2
        }
    
        public enum LifecycleState
        {
            Flat = 0,
            Signal = 1,
            PlanReady = 2,
            ExecutionReady = 3,
            PendingOrder = 4,
            LivePosition = 5,
            ExitRequested = 6,
            RecoveryRequired = 7,
            Closed = 8,
            Rejected = 9,
            Error = 10
        }
}
