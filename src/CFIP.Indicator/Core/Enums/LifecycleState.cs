namespace cAlgo
{
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
