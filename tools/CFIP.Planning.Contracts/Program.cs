using System.Collections.Generic;
using System;

namespace cAlgo
{
    internal static class Program
    {
        private static void Main()
        {
            VerifyTargetSymmetry();
            VerifyPendingEntrySpread();
            VerifyExecutionTimeframeRoles();
            VerifyTargetInvalidDirections();
            VerifyRiskPercentBounds();
            VerifyRiskAmount();
            VerifyMarginUsage();
            VerifyOssIndicatorSettings();

            VerifyStopTargetProtection();
            VerifyTp1DirectionalDefence();
            VerifyLifecycleTransitions();
            VerifyTradePlanRegistryOrdering();
            VerifyPlanRewardRiskQuality();
            VerifyCanonicalRiskRewardMath();
            VerifyExecutionIntentGeometry();
            VerifyTargetPipelineD7();
            VerifyTargetObstacleCachePolicyF9();
            VerifyTargetSelectionConsistency();
            VerifyTargetLadderSelection();
            VerifyTargetCandidateRewardScoring();
            VerifyLiveReversalD9();
            Console.WriteLine("Planning contracts OK");
        }

        private static void VerifyPendingEntrySpread()
        {
            Assert(
                Math.Abs(
                    PendingEntryPriceRule.ForExecutableStop(
                        1,
                        100.0,
                        0.20,
                        0.10) -
                    100.20) < 1e-12,
                "BUY pending trigger must include spread");

            Assert(
                Math.Abs(
                    PendingEntryPriceRule.ForExecutableStop(
                        -1,
                        100.0,
                        0.20,
                        0.10) -
                    99.80) < 1e-12,
                "SELL pending trigger must include spread");

            Assert(
                Math.Abs(
                    PendingEntryPriceRule.ForExecutableStop(
                        1,
                        100.0,
                        0,
                        0.10) -
                    100.0) < 1e-12,
                "zero-spread pending trigger preserves structural price");

            Assert(
                PendingEntryPriceRule.ForExecutableStop(
                    0,
                    100.0,
                    0.20,
                    0.10) == 0,
                "neutral pending direction must fail closed");
        }

        private static void VerifyTargetSymmetry()
        {
            Assert(
                TargetProgressionRule.IsValid(1, 100, 110),
                "BUY progression");