using System;
using System.Collections.Generic;

namespace cAlgo
{
    /// <summary>
    /// Canonical adaptive target ladder.
    ///
    /// RR is derived from the current stop risk in ATR and the live setup
    /// quality. It is not a fixed strategy target.
    /// </summary>
    internal static class TargetSelectionRequiredRrRule
    {
        public static double[] BuildRequiredRrLadder(
            string regime,
            OpportunityLane lane,
            double riskAtr,
            int confidence,
            int smartQuality,
            int targetQuality,
            int structuralQuality,
            double maximumTargetExtensionAtr)
        {
            AdaptiveRewardRiskProfile profile =
                AdaptiveRewardRiskProfileRule.Resolve(
                    regime,
                    lane,
                    riskAtr,
                    confidence,
                    smartQuality,
                    targetQuality,
                    structuralQuality,
                    maximumTargetExtensionAtr);

            return new[]
            {
                profile.RequiredTp1RR,
                profile.RequiredTp2RR,
                profile.RequiredTp3RR,
                profile.RequiredTp4RR
            };
        }

        public static bool IsMonotonicNonDecreasing(
            IReadOnlyList<double> requiredRR)
        {
            if (requiredRR == null ||
                requiredRR.Count != 4)
                return false;

            double previous = 0;

            for (int i = 0;
                 i < requiredRR.Count;
                 i++)
            {
                double current =
                    requiredRR[i];

                if (!IsFiniteNonNegativeRequiredRr(current) ||
                    (i > 0 &&
                     current < previous))
                    return false;

                previous = current;
            }

            return true;
        }

        private static bool IsFiniteNonNegativeRequiredRr(
            double value)
        {
            return
                !double.IsNaN(value) &&
                !double.IsInfinity(value) &&
                value >= 0;
        }
    }
}
