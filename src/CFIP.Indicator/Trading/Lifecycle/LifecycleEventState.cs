namespace cAlgo
{
    public partial class CFIPIndicator : cAlgo.API.Indicator
    {
        private readonly LifecycleEventIdempotencyGuard _lifecycleEventGuard =
            new LifecycleEventIdempotencyGuard();
    }
}
