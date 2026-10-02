namespace cAlgo
{
    /// <summary>
    /// Converts independent evidence-family coverage into a small bounded
    /// quality bonus. This does not add directional votes and does not lower
    /// any downstream safety/actionability gate.
    /// </summary>
    internal static class IndependentEvidenceDiversityRule
    {
        public static int QualityBonus(
            int groupCount)
        {
            switch (groupCount)
            {
                case 4:
                    return 6;
                case 3:
                    return 4;
                case 2:
                    return 2;
                default:
                    return 0;
            }
        }
    }
}
