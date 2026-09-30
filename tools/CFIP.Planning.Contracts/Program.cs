using System.Collections.Generic;
using System;

namespace cAlgo
{
    internal static class Program
    {
        private static void Main()
        {
            VerifyTargetSymmetry();
            VerifyTargetInvalidDirections();
            VerifyRiskPercentBounds();
            VerifyRiskAmount();
            VerifyMarginUsage();

            VerifyStopTargetProtection();
            VerifyTp1DirectionalDefence();
            VerifyLifecycleTransitions();
            VerifyTradePlanRegistryOrdering();
            VerifyPlanRewardRiskQuality();
            VerifyTargetPipelineD7();
            VerifyLiveReversalD9();
            Console.WriteLine("Planning contracts OK");
        }

        private static void VerifyTargetSymmetry()
        {
            Assert(
                TargetProgressionRule.IsValid(1, 100, 110),
                "BUY progression");

            Assert(
                TargetProgressionRule.IsValid(-1, 100, 90),
                "SELL progression");

            Assert(
                TargetProgressionRule.IsValid(1, 90, 100) ==
                TargetProgressionRule.IsValid(-1, 90, 80),
                "directional progression symmetry");
        }

        private static void VerifyTargetInvalidDirections()
        {
            Assert(
                !TargetProgressionRule.IsValid(0, 100, 110),
                "neutral progression blocked");

            Assert(
                !TargetProgressionRule.IsValid(1, 100, 100),
                "equal target blocked");
        }

        private static void VerifyRiskPercentBounds()
        {
            Assert(
                RiskPercentPolicy.Calculate(2, false, 0.5) == 2,
                "base risk");

            Assert(
                RiskPercentPolicy.Calculate(10, true, 0.5) == 2.5,
                "smart risk scaling");

            Assert(
                RiskPercentPolicy.Calculate(0, true, 1) >= 0.05,
                "minimum risk");

            Assert(
                RiskPercentPolicy.Calculate(10, true, 2) == 5,
                "maximum multiplier clamp");

            Assert(
                RiskPercentPolicy.Calculate(10, true, 0.1) == 1.25,
                "minimum multiplier clamp");
        }

        private static void VerifyRiskAmount()
        {
            Assert(
                RiskAmountCalculator.Calculate(10000, 1.5) == 150,
                "risk amount");

            Assert(
                RiskAmountCalculator.Calculate(0, 1) == 0,
                "zero equity");

            Assert(
                RiskAmountCalculator.Calculate(10000, 0) == 0,
                "zero risk");
        }

        private static void VerifyMarginUsage()
        {
            Assert(
                MarginUsagePolicy.CalculateAllowedPercent(80, 20) == 60,
                "margin buffer");

            Assert(
                MarginUsagePolicy.CalculateAllowedPercent(5, 0) == 10,
                "margin floor");

            Assert(
                MarginUsagePolicy.CalculateAllowedPercent(120, 0) == 100,
                "margin ceiling");
        }        private static void VerifyStopTargetProtection()
        {
            Assert(
                PriceProtectionRule.ValidateStop(1, 100, 98, 1),
                "BUY stop invariant");

            Assert(
                PriceProtectionRule.ValidateStop(-1, 100, 102, 1),
                "SELL stop invariant");

            Assert(
                PriceProtectionRule.ValidateTarget(1, 100, 102, 1),
                "BUY target invariant");

            Assert(
                PriceProtectionRule.ValidateTarget(-1, 100, 98, 1),
                "SELL target invariant");

            Assert(
                !PriceProtectionRule.ValidateStop(1, 100, 101, 1),
                "BUY stop wrong side");

            Assert(
                !PriceProtectionRule.ValidateStop(-1, 100, 99, 1),
                "SELL stop wrong side");

            Assert(
                !PriceProtectionRule.ValidateTarget(1, 100, 99, 1),
                "BUY target wrong side");

            Assert(
                !PriceProtectionRule.ValidateTarget(-1, 100, 101, 1),
                "SELL target wrong side");

            Assert(
                PriceProtectionRule.ValidateStop(1, 100, 98, 1) ==
                PriceProtectionRule.ValidateTarget(-1, 100, 98, 1),
                "SELL target mirrors BUY stop");

            Assert(
                PriceProtectionRule.ValidateStop(-1, 100, 102, 1) ==
                PriceProtectionRule.ValidateTarget(1, 100, 102, 1),
                "BUY target mirrors SELL stop");
        }

        private static void VerifyTp1DirectionalDefence()
        {
            Assert(
                PriceProtectionRule.ValidateTarget(
                    1,
                    100,
                    102,
                    0),
                "BUY TP1 direction accepted");

            Assert(
                PriceProtectionRule.ValidateTarget(
                    -1,
                    100,
                    98,
                    0),
                "SELL TP1 direction accepted");

            Assert(
                !PriceProtectionRule.ValidateTarget(
                    1,
                    100,
                    98,
                    0),
                "BUY wrong-side TP1 rejected");

            Assert(
                !PriceProtectionRule.ValidateTarget(
                    -1,
                    100,
                    102,
                    0),
                "SELL wrong-side TP1 rejected");

            Assert(
                PriceProtectionRule.ValidateTarget(
                    1,
                    100,
                    102,
                    0) ==
                PriceProtectionRule.ValidateStop(
                    -1,
                    100,
                    102,
                    0),
                "BUY TP1 mirrors SELL stop");

            Assert(
                PriceProtectionRule.ValidateTarget(
                    -1,
                    100,
                    98,
                    0) ==
                PriceProtectionRule.ValidateStop(
                    1,
                    100,
                    98,
                    0),
                "SELL TP1 mirrors BUY stop");

            TargetCandidateConstraintResult buyWrongSide =
                TargetCandidateConstraintRule.Evaluate(
                    0,
                    1,
                    100,
                    1,
                    98,
                    2,
                    0.01,
                    2.0,
                    12.0,
                    4.0,
                    0.40,
                    100,
                    false,
                    false,
                    false,
                    0,
                    0);

            Assert(
                !buyWrongSide.Allowed &&
                buyWrongSide.Reason ==
                TargetCandidateRejectionReasons.TargetSideInvalid,
                "BUY wrong-side TP1 candidate rejected canonically");

            TargetCandidateConstraintResult sellWrongSide =
                TargetCandidateConstraintRule.Evaluate(
                    0,
                    -1,
                    100,
                    1,
                    102,
                    2,
                    0.01,
                    2.0,
                    12.0,
                    4.0,
                    0.40,
                    100,
                    false,
                    false,
                    false,
                    0,
                    0);

            Assert(
                !sellWrongSide.Allowed &&
                sellWrongSide.Reason ==
                TargetCandidateRejectionReasons.TargetSideInvalid,
                "SELL wrong-side TP1 candidate rejected canonically");

            Assert(
                TargetProgressionRule.IsValid(1, 102, 104) ==
                TargetProgressionRule.IsValid(-1, 98, 96),
                "TP progression BUY/SELL symmetry remains intact");

            Console.WriteLine(
                "D8 TP1 directional defence: BUY/SELL valid and wrong-side fixtures passed");
        }

        private static void VerifyLiveReversalD9()
        {
            Assert(
                LiveReversalDecisionRule.OppositeDirection(1) == -1,
                "BUY position reverses to SELL");

            Assert(
                LiveReversalDecisionRule.OppositeDirection(-1) == 1,
                "SELL position reverses to BUY");

            Assert(
                LiveReversalDecisionRule.OppositeDirection(0) == 0,
                "neutral position has no reversal direction");

            Assert(
                LiveReversalDecisionRule.IsOppositeEvidenceDirection(
                    1,
                    -1),
                "BUY position accepts SELL evidence");

            Assert(
                !LiveReversalDecisionRule.IsOppositeEvidenceDirection(
                    1,
                    1),
                "BUY position rejects same-direction evidence");

            Assert(
                LiveReversalDecisionRule.ResolveDirectionalConfidence(
                    1,
                    -1,
                    84,
                    -1,
                    79) == 84,
                "reaction confidence is used only for opposite direction");

            Assert(
                LiveReversalDecisionRule.ResolveDirectionalConfidence(
                    1,
                    1,
                    94,
                    -1,
                    79) == 79,
                "same-direction reaction confidence is ignored");

            Assert(
                LiveReversalDecisionRule.ResolveDirectionalConfidence(
                    -1,
                    1,
                    77,
                    1,
                    88) == 88,
                "SELL reversal uses opposite BUY evidence");

            Assert(
                LiveReversalDecisionRule.ResolveDirectionalConfidence(
                    -1,
                    -1,
                    96,
                    -1,
                    96) == 0,
                "same-direction SELL evidence cannot qualify a SELL reversal");

            Assert(
                LiveReversalDecisionRule.ResolveAction(
                    true,
                    false,
                    true,
                    false) ==
                LiveReversalAction.DetectedRetain,
                "reversal detection without close permission retains position");

            Assert(
                LiveReversalDecisionRule.ResolveAction(
                    true,
                    true,
                    false,
                    false) ==
                LiveReversalAction.DetectedRetain,
                "non-profitable reversal retains position");

            Assert(
                LiveReversalDecisionRule.ResolveAction(
                    true,
                    true,
                    true,
                    false) ==
                LiveReversalAction.ExitRequested,
                "profitable reversal requests exit");

            Assert(
                LiveReversalDecisionRule.ResolveAction(
                    true,
                    true,
                    true,
                    true) ==
                LiveReversalAction.AwaitBrokerConfirmation,
                "accepted exit waits for broker confirmation");

            Assert(
                LiveReversalDecisionRule.ResolveAction(
                    false,
                    true,
                    true,
                    false) ==
                LiveReversalAction.ReconcileBrokerState,
                "missing broker position requires reconciliation");

            Assert(
                ExecutionThresholdPolicy.NormalizeLiveReversalConfidence(49) == 50 &&
                ExecutionThresholdPolicy.NormalizeLiveReversalConfidence(68) == 68 &&
                ExecutionThresholdPolicy.NormalizeLiveReversalConfidence(96) == 95,
                "live reversal confidence boundary preserves public range");

            Assert(
                ExecutionThresholdPolicy.NormalizeLiveReversalStructuralScore(49) == 50 &&
                ExecutionThresholdPolicy.NormalizeLiveReversalStructuralScore(72) == 72 &&
                ExecutionThresholdPolicy.NormalizeLiveReversalStructuralScore(101) == 100,
                "live reversal structural-score boundary preserves public range");

            Assert(
                LiveReversalEpisodeRule.IsSameEpisode(
                    42,
                    -1,
                    42,
                    -1),
                "same position/direction remains one reversal episode");

            Assert(
                !LiveReversalEpisodeRule.IsSameEpisode(
                    42,
                    -1,
                    43,
                    -1),
                "position change starts a new reversal episode");

            Assert(
                !LiveReversalEpisodeRule.IsSameEpisode(
                    42,
                    1,
                    42,
                    -1),
                "opposite-direction change starts a new reversal episode");

            Assert(
                LiveReversalEpisodeRule.ShouldEmitDetectionAlert(false) &&
                !LiveReversalEpisodeRule.ShouldEmitDetectionAlert(true),
                "reversal detection alert is emitted once per episode");

            Console.WriteLine(
                "D9 live reversal: directional evidence, action semantics, episode semantics and parameter boundaries passed");
        }

        private static void VerifyLifecycleTransitions()
        {
            Assert(
                LifecycleTransitionPolicy.IsAllowed(
                    LifecycleState.Flat,
                    LifecycleState.Signal),
                "flat to signal");

            Assert(
                LifecycleTransitionPolicy.IsAllowed(
                    LifecycleState.Signal,
                    LifecycleState.PlanReady),
                "signal to plan");

            Assert(
                LifecycleTransitionPolicy.IsAllowed(
                    LifecycleState.ExecutionReady,
                    LifecycleState.PendingOrder),
                "execution to pending");

            Assert(
                LifecycleTransitionPolicy.IsAllowed(
                    LifecycleState.PendingOrder,
                    LifecycleState.LivePosition),
                "pending to live");

            Assert(
                LifecycleTransitionPolicy.IsAllowed(
                    LifecycleState.LivePosition,
                    LifecycleState.ExitRequested),
                "live to exit");

            Assert(
                LifecycleTransitionPolicy.IsAllowed(
                    LifecycleState.ExitRequested,
                    LifecycleState.Closed),
                "exit to closed");

            Assert(
                LifecycleTransitionPolicy.IsAllowed(
                    LifecycleState.Closed,
                    LifecycleState.Closed),
                "closed state idempotency");

            Assert(
                !LifecycleTransitionPolicy.IsAllowed(
                    LifecycleState.Closed,
                    LifecycleState.PendingOrder),
                "closed to pending blocked");
        }



        private static void VerifyTradePlanRegistryOrdering()
        {
            TradePlanRegistry registry =
                new TradePlanRegistry();

            registry.Upsert(
                new TradeOpportunityCandidate
                {
                    Id = "BUY_TACTICAL",
                    Lane = OpportunityLane.Tactical,
                    Direction = 1,
                    Quality = 80,
                    Tp1RR = 2.0,
                    ActionableNow = false,
                    CreatedM5 = 100
                });

            registry.Upsert(
                new TradeOpportunityCandidate
                {
                    Id = "SELL_STRATEGIC",
                    Lane = OpportunityLane.Strategic,
                    Direction = -1,
                    Quality = 74,
                    Tp1RR = 2.6,
                    ActionableNow = true,
                    CreatedM5 = 101
                });

            registry.Upsert(
                new TradeOpportunityCandidate
                {
                    Id = "BUY_STRONG",
                    Lane = OpportunityLane.Tactical,
                    Direction = 1,
                    Quality = 91,
                    Tp1RR = 2.2,
                    ActionableNow = true,
                    CreatedM5 = 101
                });

            IReadOnlyList<TradeOpportunityCandidate> snapshot =
                registry.Snapshot();

            Assert(
                snapshot.Count == 3,
                "registry snapshot count");

            Assert(
                snapshot[0].Id == "BUY_STRONG",
                "actionable quality ordering");

            TradeOpportunityCandidate best;
            Assert(
                registry.TryGetBest(out best) &&
                best != null &&
                best.Id == "BUY_STRONG",
                "registry best candidate");

            registry.Upsert(
                new TradeOpportunityCandidate
                {
                    Id = "BUY_STRONG",
                    Lane = OpportunityLane.Tactical,
                    Direction = 1,
                    Quality = 95,
                    Tp1RR = 1.8,
                    ActionableNow = true,
                    CreatedM5 = 102
                });

            Assert(
                registry.Count == 3 &&
                registry.TryGetBest(out best) &&
                best.Quality == 95,
                "stable identity replacement");
        }


        private static void VerifyTargetPipelineD7()
        {
            Assert(
                TargetAgeSemanticsRule.IsAllowed(
                    "M5",
                    10,
                    0,
                    10,
                    40),
                "M5 setup age uses setup-bar semantics");

            double htfMaxMinutes =
                TargetAgeSemanticsRule.GetMaximumHtfAgeMinutes(
                    "H1",
                    20,
                    40);

            Assert(
                htfMaxMinutes == 1200,
                "H1 target age uses elapsed-time equivalent");

            Assert(
                TargetAgeSemanticsRule.IsAllowed(
                    "H1",
                    60,
                    1199.0,
                    20,
                    40) &&
                !TargetAgeSemanticsRule.IsAllowed(
                    "H1",
                    60,
                    1201.0,
                    20,
                    40),
                "HTF age boundary is deterministic");

            Assert(
                TargetAgeSemanticsRule.ElapsedMinutes(
                    new DateTime(2026, 1, 1, 10, 0, 0),
                    new DateTime(2026, 1, 1, 11, 30, 0)) == 90,
                "elapsed target age is deterministic");

            double[] requiredRR =
            {
                2.00,
                3.20,
                4.80,
                6.50
            };

            int accepted = 0;
            int belowRejected = 0;
            int sellAccepted = 0;

            for (int stage = 0; stage < 4; stage++)
            {
                double previous =
                    stage == 0
                        ? 100
                        : 100 +
                          requiredRR[stage - 1] * 1.0 +
                          0.4;

                TargetCandidateConstraintResult valid =
                    TargetCandidateConstraintRule.Evaluate(
                        stage,
                        1,
                        100,
                        1,
                        100 + requiredRR[stage] * 1.0 + 0.2,
                        2,
                        0.01,
                        requiredRR[stage],
                        12.0,
                        4.0,
                        0.40,
                        previous,
                        false,
                        stage >= 1,
                        stage >= 1,
                        75,
                        68);

                Assert(
                    valid.Allowed,
                    "TP" +
                    (stage + 1).ToString() +
                    " valid fixture accepted");

                accepted++;

                TargetCandidateConstraintResult below =
                    TargetCandidateConstraintRule.Evaluate(
                        stage,
                        1,
                        100,
                        1,
                        100 + Math.Max(
                            0.5,
                            requiredRR[stage] - 0.2) * 1.0,
                        2,
                        0.01,
                        requiredRR[stage],
                        12.0,
                        4.0,
                        0.40,
                        previous,
                        false,
                        stage >= 1,
                        stage >= 1,
                        75,
                        68);

                Assert(
                    !below.Allowed &&
                    below.Reason ==
                    TargetCandidateRejectionReasons.RewardRiskBelowMinimum,
                    "TP" +
                    (stage + 1).ToString() +
                    " below-minimum RR rejection");

                belowRejected++;

                TargetCandidateConstraintResult sell =
                    TargetCandidateConstraintRule.Evaluate(
                        stage,
                        -1,
                        100,
                        1,
                        100 - requiredRR[stage] * 1.0 - 0.2,
                        2,
                        0.01,
                        requiredRR[stage],
                        12.0,
                        4.0,
                        0.40,
                        stage == 0
                            ? 100
                            : 100 -
                              requiredRR[stage - 1] * 1.0 -
                              0.4,
                        false,
                        stage >= 1,
                        stage >= 1,
                        75,
                        68);

                Assert(
                    sell.Allowed,
                    "TP" +
                    (stage + 1).ToString() +
                    " BUY/SELL feasibility symmetry");

                sellAccepted++;
            }

            Assert(
                accepted == 4 &&
                belowRejected == 4 &&
                sellAccepted == 4,
                "D7 deterministic fixture matrix");

            double[] riskAtrFixtures =
            {
                0.55,
                0.75,
                1.00,
                1.80
            };

            int[] tp4Reachable =
            {
                1, 0, 0, 0
            };

            int observedTp4Reachable = 0;

            for (int i = 0;
                 i < riskAtrFixtures.Length;
                 i++)
            {
                double risk =
                    riskAtrFixtures[i];

                double maximumReachableRR =
                    TargetRewardEnvelopeRule.MaximumReachableRR(
                        risk,
                        1.0,
                        4.0,
                        12.0);

                bool tp4 =
                    TargetRewardEnvelopeRule.CanReachStage(
                        6.50,
                        risk,
                        1.0,
                        4.0,
                        12.0);

                if (tp4)
                    observedTp4Reachable++;

                Assert(
                    tp4 ==
                    (tp4Reachable[i] == 1),
                    "TP4 extension feasibility at risk ATR " +
                    risk.ToString("F2"));

                Console.WriteLine(
                    "D7 envelope riskATR=" +
                    risk.ToString("F2") +
                    " maxReachableRR=" +
                    maximumReachableRR.ToString("F2") +
                    " TP4=" +
                    (tp4 ? "YES" : "NO"));
            }

            Assert(
                observedTp4Reachable == 1,
                "D7 TP4 feasibility matrix");

            Console.WriteLine(
                "D7 fixture matrix: " +
                "primary=" + accepted.ToString() + "/4, " +
                "belowMin=" + belowRejected.ToString() + "/4, " +
                "sellMirror=" + sellAccepted.ToString() + "/4, " +
                "tp4Reachable=" + observedTp4Reachable.ToString() + "/4");
        }


        private static void VerifyPlanRewardRiskQuality()
        {
            PlanRewardRiskQualityResult weakReward =
                PlanRewardRiskQualityRule.Evaluate(
                    1,
                    100,
                    98,
                    101.5,
                    2.0,
                    0.05,
                    2.0,
                    1.0,
                    2.0);

            Assert(
                !weakReward.Allowed &&
                weakReward.Reason.Contains(
                    "REWARD TOO LOW FOR STOP"),
                "weak TP1 reward blocked");

            PlanRewardRiskQualityResult wideStop =
                PlanRewardRiskQualityRule.Evaluate(
                    1,
                    100,
                    94,
                    113,
                    3.0,
                    0.05,
                    2.0,
                    1.0,
                    1.8);

            Assert(
                !wideStop.Allowed &&
                wideStop.Reason.Contains(
                    "STOP RISK TOO HIGH"),
                "oversized stop blocked");

            PlanRewardRiskQualityResult spreadHeavy =
                PlanRewardRiskQualityRule.Evaluate(
                    1,
                    100,
                    98,
                    104,
                    2.0,
                    0.30,
                    2.0,
                    1.0,
                    2.0);

            Assert(
                !spreadHeavy.Allowed &&
                spreadHeavy.Reason.Contains(
                    "RR TOO LOW AFTER SPREAD"),
                "spread-adjusted RR blocked");

            PlanRewardRiskQualityResult valid =
                PlanRewardRiskQualityRule.Evaluate(
                    1,
                    100,
                    98.5,
                    105,
                    3.0,
                    0.05,
                    2.0,
                    1.0,
                    2.0);

            Assert(
                valid.Allowed &&
                valid.NominalRR > 3 &&
                valid.EffectiveRR > 2,
                "healthy reward-risk accepted");

            PlanRewardRiskQualityResult sell =
                PlanRewardRiskQualityRule.Evaluate(
                    -1,
                    100,
                    102,
                    95,
                    3.0,
                    0.05,
                    2.0,
                    1.0,
                    2.0);

            Assert(
                sell.Allowed &&
                sell.NominalRR > 2.4,
                "SELL reward-risk symmetry");
        }

        private static void Assert(bool condition, string name)
        {
            if (!condition)
                throw new InvalidOperationException(
                    "Planning contract failed: " + name);
        }
    }
}
