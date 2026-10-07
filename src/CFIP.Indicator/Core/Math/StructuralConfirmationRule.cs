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

            return frameDirection == -requestedDirection
                ? 0
                : 1;
        }
    }
}
