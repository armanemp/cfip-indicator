using System;

namespace cAlgo
{
    /// <summary>
    /// Platform-neutral structural evidence de-duplication.
    /// Structure, MSS and CHOCH may be different labels for the same causal
    /// break on one timeframe; they must not stack as independent events.
    /// </summary>
    internal static class StructuralEvidenceRule
    {
        public static bool HasCanonicalStructuralEvent(
            bool structure,
            bool mss,
            bool choch)
        {
            return structure || mss || choch;
        }

        public static bool IsIndependentTransition(
            bool structure,
            bool mss,
            bool choch)
        {
            return !structure && (mss || choch);
        }

        public static int CanonicalEventCount(
            bool structure,
            bool mss,
            bool choch)
        {
            return HasCanonicalStructuralEvent(
                    structure,
                    mss,
                    choch)
                ? 1
                : 0;
        }
    }
}
