using System;

namespace cAlgo
{
    internal static class M2PrecisionRule
    {
        public static M2PrecisionSnapshot Evaluate(
            Frame m2,
            int closedIndex,
            int m5Direction)
        {
            if (m2 == null ||
                closedIndex < 0 ||
                m2.Index != closedIndex)
                return M2PrecisionSnapshot.Unavailable;

            int direction =
                m2.Direction == 1 || m2.Direction == -1
                    ? m2.Direction
                    : 0;

            int quality =
                Math.Max(
                    0,
                    Math.Min(
                        100,
                        m2.Quality));

            bool usable =
                direction != 0 &&
                quality > 0;

            bool alignsWithM5 =
                usable &&
                (m5Direction == 0 ||
                 direction == m5Direction);

            return new M2PrecisionSnapshot(
                closedIndex,
                direction,
                quality,
                usable,
                alignsWithM5);
        }
    }
}
