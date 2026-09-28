namespace cAlgo
{
    internal static class LifecycleTransitionPolicy
    {
        public static bool IsAllowed(
            LifecycleState current,
            LifecycleState next)
        {
            if (current == next)
                return true;

            switch (current)
            {
                case LifecycleState.Flat:
                    return next == LifecycleState.Signal ||
                           next == LifecycleState.PlanReady ||
                           next == LifecycleState.Closed ||
                           next == LifecycleState.Error;

                case LifecycleState.Signal:
                    return next == LifecycleState.PlanReady ||
                           next == LifecycleState.ExecutionReady ||
                           next == LifecycleState.Flat ||
                           next == LifecycleState.Rejected ||
                           next == LifecycleState.Error;

                case LifecycleState.PlanReady:
                    return next == LifecycleState.ExecutionReady ||
                           next == LifecycleState.Flat ||
                           next == LifecycleState.Rejected ||
                           next == LifecycleState.Error;

                case LifecycleState.ExecutionReady:
                    return next == LifecycleState.PendingOrder ||
                           next == LifecycleState.LivePosition ||
                           next == LifecycleState.ExitRequested ||
                           next == LifecycleState.Rejected ||
                           next == LifecycleState.Flat ||
                           next == LifecycleState.Error;

                case LifecycleState.PendingOrder:
                    return next == LifecycleState.LivePosition ||
                           next == LifecycleState.ExitRequested ||
                           next == LifecycleState.RecoveryRequired ||
                           next == LifecycleState.Closed ||
                           next == LifecycleState.Error;

                case LifecycleState.LivePosition:
                    return next == LifecycleState.ExitRequested ||
                           next == LifecycleState.Closed ||
                           next == LifecycleState.RecoveryRequired ||
                           next == LifecycleState.Error;

                case LifecycleState.ExitRequested:
                    return next == LifecycleState.LivePosition ||
                           next == LifecycleState.Closed ||
                           next == LifecycleState.RecoveryRequired ||
                           next == LifecycleState.Error;

                case LifecycleState.RecoveryRequired:
                    return next == LifecycleState.PendingOrder ||
                           next == LifecycleState.LivePosition ||
                           next == LifecycleState.ExitRequested ||
                           next == LifecycleState.Closed ||
                           next == LifecycleState.Error;

                case LifecycleState.Closed:
                    return next == LifecycleState.Flat ||
                           next == LifecycleState.Signal ||
                           next == LifecycleState.Error;

                case LifecycleState.Rejected:
                    return next == LifecycleState.Flat ||
                           next == LifecycleState.Signal ||
                           next == LifecycleState.Error;

                case LifecycleState.Error:
                    return next == LifecycleState.RecoveryRequired ||
                           next == LifecycleState.Flat ||
                           next == LifecycleState.Closed;

                default:
                    return false;
            }
        }
    }
}
