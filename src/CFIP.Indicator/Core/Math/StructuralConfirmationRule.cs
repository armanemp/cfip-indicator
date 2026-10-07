namespace cAlgo
{
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

            // A structural event on a neutral frame is context, not confirmation.
            // Only same-direction structure earns a directional confirmation.
            return frameDirection == requestedDirection
                ? 1
                : 0;
        }
    }
}
