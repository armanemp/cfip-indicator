namespace cAlgo
{
    /// <summary>
    /// Canonical directional structural-confirmation owner.
    /// </summary>
    internal static class StructuralConfirmationRule
    {
        internal static int CountDirectionalStructureContribution(
            int requestedDirection,
            int frameDirection,
            bool structure)
        {
            if (!structure ||
                (requestedDirection != 1 &&
                 requestedDirection != -1))
                return 0;

            // Opposite-frame structure is contradictory evidence, not support.
            // Neutral structure remains valid transition/recovery evidence.
            return frameDirection == -requestedDirection
                ? 0
                : 1;
        }
    }
}
