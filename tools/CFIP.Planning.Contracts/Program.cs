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
        }        private static void VerifyOssIndicatorSettings()
        {
            OssIndicatorSettings settings =
                OssIndicatorSettings.Default;

            Assert(
                settings.MacdSignalPeriod == 9,
                "H3-A MACD signal period default");

            Assert(
                settings.BollingerPeriod == 20 &&
                Math.Abs(settings.BollingerStandardDeviations - 2.0) < 1e-12,
                "H3-A Bollinger defaults");

            Assert(
                settings.MfiPeriod == 14,
                "H3-A MFI period default");

            Assert(
                settings.StochLookbackPeriod == 14 &&
                settings.StochSignalPeriod == 3 &&
                settings.StochSmoothPeriod == 3,
                "H3-A Stochastic defaults");

            Assert(
                settings.SuperTrendPeriod == 10 &&
                Math.Abs(settings.SuperTrendMultiplier - 3.0) < 1e-12,
                "H3-A SuperTrend defaults");

            Assert(
                settings.AroonPeriod == 25 &&
                settings.CciPeriod == 20,
                "H3-A Aroon/CCI defaults");

            Assert(
                Math.Abs(settings.ParabolicSarAccelerationFactor - 0.02) < 1e-12 &&
                Math.Abs(settings.ParabolicSarMaximumAccelerationFactor - 0.20) < 1e-12,
                "H3-A Parabolic SAR defaults");

            Console.WriteLine(
                "H3-A Skender settings defaults: all legacy values preserved");
        }

        private static void VerifyStopTargetProtection()
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

        private static void VerifyTargetSelectionConsistency()
        {
            Assert(
                Math.Abs(
                    PlanRewardRiskQualityRule.BaseMinimumRrFloor -
                    0.50) < 1e-12,
                "E8 base RR floor constant");

            Assert(
                Math.Abs(
                    PlanRewardRiskQualityRule.PreferredStopRiskAtrFloor -
                    0.25) < 1e-12,
                "E8 preferred stop ATR floor constant");

            Assert(
                Math.Abs(
                    PlanRewardRiskQualityRule.MaximumStopRiskAtrFloor -
                    0.50) < 1e-12,
                "E8 maximum stop ATR floor constant");

            Assert(
                Math.Abs(
                    PlanRewardRiskQualityRule.AdaptiveStopExcessRrCap -
                    0.50) < 1e-12,
                "E8 adaptive RR cap constant");

            Assert(
                Math.Abs(
                    PlanRewardRiskQualityRule.AdaptiveStopExcessRrMultiplier -
                    0.25) < 1e-12,
                "E8 adaptive RR multiplier constant");

            Assert(
                Math.Abs(
                    PlanRewardRiskQualityRule.EffectiveRrBaseFactor -
                    0.90) < 1e-12,
                "E8 effective RR factor constant");

            Assert(
                Math.Abs(
                    PlanRewardRiskQualityRule.EffectiveRrAbsoluteReduction -
                    0.15) < 1e-12,
                "E8 effective RR reduction constant");

            OpportunityLane[] lanes =
            {
                OpportunityLane.Strategic,
                OpportunityLane.Tactical,
                OpportunityLane.CounterHtfTactical,
                OpportunityLane.MicroReaction
            };

            double[][] fixtures =
            {
                new[]
                {
                    0.10, 2.00, 3.20, 4.80, 6.50, 2.00, 2.00, 2.40
                },
                new[]
                {
                    0.25, 1.00, 1.00, 1.00, 1.00, 2.50, 2.00, 1.50
                },
                new[]
                {
                    0.10, 2.00, 1.20, 1.30, 1.40, 2.10, 2.00, 3.00
                },
                new[]
                {
                    0.40, 4.00, 2.00, 2.00, 2.00, 3.50, 3.50, 2.25
                }
            };

            int checkedLadders = 0;

            for (int fixture = 0;
                 fixture < fixtures.Length;
                 fixture++)
            {
                double[] f = fixtures[fixture];

                for (int laneIndex = 0;
                     laneIndex < lanes.Length;
                     laneIndex++)
                {
                    double[] requiredRR =
                        TargetSelectionRequiredRrRule.BuildRequiredRrLadder(
                            f[0],
                            lanes[laneIndex],
                            f[1],
                            f[2],
                            f[3],
                            f[4],
                            f[5],
                            f[6],
                            f[7]);

                    Assert(
                        TargetSelectionRequiredRrRule.IsMonotonicNonDecreasing(
                            requiredRR),
                        "E8 required RR monotonicity lane " +
                        lanes[laneIndex].ToString() +
                        " fixture " +
                        fixture.ToString());

                    for (int stage = 1;
                         stage < 4;
                         stage++)
                    {
                        Assert(
                            requiredRR[stage] >=
                            requiredRR[stage - 1],
                            "E8 TP" +
                            (stage + 1).ToString() +
                            " does not invert previous stage");
                    }

                    for (int stage = 0;
                         stage < 4;
                         stage++)
                    {
                        double rr = requiredRR[stage];

                        Assert(
                            TargetProgressionRule.IsValid(
                                1,
                                100,
                                100 + rr) ==
                            TargetProgressionRule.IsValid(
                                -1,
                                100,
                                100 - rr),
                            "E8 BUY/SELL target symmetry lane " +
                            lanes[laneIndex].ToString() +
                            " stage " +
                            (stage + 1).ToString());
                    }

                    checkedLadders++;
                }
            }

            double[] tacticalInversion =
                TargetSelectionRequiredRrRule.BuildRequiredRrLadder(
                    0.10,
                    OpportunityLane.Tactical,
                    2.00,
                    1.00,
                    1.00,
                    1.00,
                    2.00,
                    1.50,
                    1.00);

            Assert(
                Math.Abs(tacticalInversion[0] - 2.00) < 1e-12 &&
                Math.Abs(tacticalInversion[1] - 2.10) < 1e-12 &&
                Math.Abs(tacticalInversion[2] - 2.20) < 1e-12 &&
                Math.Abs(tacticalInversion[3] - 2.30) < 1e-12,
                "E8 tactical RR ordering cannot invert");

            double[] strategicTradeFloor =
                TargetSelectionRequiredRrRule.BuildRequiredRrLadder(
                    0.10,
                    OpportunityLane.Strategic,
                    2.00,
                    3.20,
                    4.80,
                    6.50,
                    2.00,
                    3.25,
                    2.00);

            Assert(
                strategicTradeFloor[0] == 3.25 &&
                strategicTradeFloor[1] == 3.35 &&
                strategicTradeFloor[2] == 4.80 &&
                strategicTradeFloor[3] == 6.50,
                "E8 MinimumTradeRR remains an active strategic floor");

            Console.WriteLine(
                "E8 target-selection contracts: " +
                checkedLadders.ToString() +
                " lane/fixture ladders, named constants, BUY/SELL symmetry passed");
        }



        private static void VerifyTargetLadderSelection()
        {
            IReadOnlyList<TargetLadderOption>[] buyStages =
            {
                new List<TargetLadderOption>
                {
                    new TargetLadderOption(0, 103.0, 100.0),
                    new TargetLadderOption(1, 102.0, 95.0)
                },
                new List<TargetLadderOption>
                {
                    new TargetLadderOption(2, 102.5, 100.0)
                },
                new List<TargetLadderOption>()
            };

            int[] buy =
                TargetLadderSelectionRule.SelectBestPath(
                    1,
                    100.0,
                    0.25,
                    buyStages);

            Assert(
                buy.Length == 3 &&
                buy[0] == 1 &&
                buy[1] == 2 &&
                buy[2] == -1,
                "CI-13 global BUY ladder selection");

            IReadOnlyList<TargetLadderOption>[] sellStages =
            {
                new List<TargetLadderOption>
                {
                    new TargetLadderOption(0, 97.0, 100.0),
                    new TargetLadderOption(1, 98.0, 95.0)
                },
                new List<TargetLadderOption>
                {
                    new TargetLadderOption(2, 97.5, 100.0)
                },
                new List<TargetLadderOption>()
            };

            int[] sell =
                TargetLadderSelectionRule.SelectBestPath(
                    -1,
                    100.0,
                    0.25,
                    sellStages);

            Assert(
                sell.Length == 3 &&
                sell[0] == 1 &&
                sell[1] == 2 &&
                sell[2] == -1,
                "CI-13 global SELL ladder symmetry");

            IReadOnlyList<TargetLadderOption>[] singleStage =
            {
                new List<TargetLadderOption>
                {
                    new TargetLadderOption(4, 101.0, 40.0)
                }
            };

            int[] single =
                TargetLadderSelectionRule.SelectBestPath(
                    1,
                    100.0,
                    0.25,
                    singleStage);

            Assert(
                single.Length == 1 &&
                single[0] == 4,
                "CI-13 single-stage ladder remains valid");

            int[] invalidDirection =
                TargetLadderSelectionRule.SelectBestPath(
                    0,
                    100.0,
                    0.25,
                    singleStage);

            Assert(
                invalidDirection.Length == 1 &&
                invalidDirection[0] == -1,
                "CI-13 invalid direction fails closed");

            Console.WriteLine(
                "CI-13 coherent TP ladder contracts: global path selection, termination and BUY/SELL symmetry passed");
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


        private static void VerifyExecutionIntentGeometry()
        {
            ExecutionIntentGeometryResult buy =
                ExecutionIntentGeometryRule.Evaluate(
                    1,
                    100.00,
                    98.00,
                    106.00,
                    0.10);

            ExecutionIntentGeometryResult sell =
                ExecutionIntentGeometryRule.Evaluate(
                    -1,
                    100.00,
                    102.00,
                    94.00,
                    0.10);

            Assert(
                buy.Valid &&
                Math.Abs(buy.Entry - 100.00) < 1e-12 &&
                Math.Abs(buy.StopPips - 20.0) < 1e-12 &&
                Math.Abs(buy.TargetPips - 60.0) < 1e-12,
                "CI-15 BUY intent geometry projection");

            Assert(
                sell.Valid &&
                Math.Abs(sell.Entry - 100.00) < 1e-12 &&
                Math.Abs(sell.StopPips - 20.0) < 1e-12 &&
                Math.Abs(sell.TargetPips - 60.0) < 1e-12,
                "CI-15 SELL intent geometry projection");

            Assert(
                Math.Abs(buy.StopPips - sell.StopPips) < 1e-12 &&
                Math.Abs(buy.TargetPips - sell.TargetPips) < 1e-12,
                "CI-15 BUY/SELL intent pip symmetry");

            ExecutionIntentGeometryResult wrongBuy =
                ExecutionIntentGeometryRule.Evaluate(
                    1,
                    100.00,
                    102.00,
                    106.00,
                    0.10);

            ExecutionIntentGeometryResult wrongSell =
                ExecutionIntentGeometryRule.Evaluate(
                    -1,
                    100.00,
                    98.00,
                    94.00,
                    0.10);

            Assert(
                !wrongBuy.Valid &&
                wrongBuy.Reason == "WRONG-SIDE GEOMETRY" &&
                !wrongSell.Valid &&
                wrongSell.Reason == "WRONG-SIDE GEOMETRY",
                "CI-15 wrong-side intent geometry fails closed");

            ExecutionIntentGeometryResult invalidPip =
                ExecutionIntentGeometryRule.Evaluate(
                    1,
                    100.00,
                    98.00,
                    106.00,
                    0);

            Assert(
                !invalidPip.Valid &&
                invalidPip.Reason == "INVALID GEOMETRY",
                "CI-15 invalid pip size fails closed");

            ExecutionIntentGeometryResult subPip =
                ExecutionIntentGeometryRule.Evaluate(
                    1,
                    100.0000,
                    99.9995,
                    100.0045,
                    0.01);

            Assert(
                subPip.Valid &&
                Math.Abs(subPip.StopPips - 0.05) < 1e-12 &&
                Math.Abs(subPip.TargetPips - 0.45) < 1e-12,
                "CI-15 intent preserves physical sub-pip distances");
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


        private static void VerifyTargetObstacleCachePolicyF9()
        {
            TargetObstacleCacheKey baseKey =
                new TargetObstacleCacheKey(
                    1000,
                    500,
                    123456789,
                    1,
                    2,
                    180,
                    80,
                    true,
                    0.10,
                    0.01);

            Assert(
                TargetObstacleCachePolicy.MaximumEntries == 16,
                "F9 target-obstacle cache is bounded");

            Assert(
                TargetObstacleCachePolicy.IsSupportedDirection(1) &&
                TargetObstacleCachePolicy.IsSupportedDirection(-1) &&
                !TargetObstacleCachePolicy.IsSupportedDirection(0),
                "F9 cache direction validation");

            Assert(
                baseKey.Equals(baseKey),
                "F9 identical cache key is stable");

            Assert(
                !baseKey.Equals(
                    new TargetObstacleCacheKey(
                        1000, 501, 123456789, 1, 2, 180, 80, true, 0.10, 0.01)),
                "F9 closed-index participates in cache identity");

            Assert(
                !baseKey.Equals(
                    new TargetObstacleCacheKey(
                        1000, 500, 123456789, -1, 2, 180, 80, true, 0.10, 0.01)),
                "F9 BUY/SELL direction participates in cache identity");

            Assert(
                !baseKey.Equals(
                    new TargetObstacleCacheKey(
                        1000, 500, 123456789, 1, 2, 180, 80, true, 0.15, 0.01)),
                "F9 ATR-derived equality tolerance participates in cache identity");

            Assert(
                !baseKey.Equals(
                    new TargetObstacleCacheKey(
                        1001, 500, 123456789, 1, 2, 180, 80, true, 0.10, 0.01)),
                "F9 history-size participates in cache identity");

            TargetObstacleCacheKey nextBar =
                new TargetObstacleCacheKey(
                    1001,
                    501,
                    123456790,
                    1,
                    2,
                    180,
                    80,
                    true,
                    0.10,
                    0.01);

            Assert(
                TargetObstacleCachePolicy.IsObsoleteSameBars(
                    baseKey,
                    nextBar),
                "F9 new closed bar invalidates same-bars cache entries");

            Assert(
                !TargetObstacleCachePolicy.IsObsoleteSameBars(
                    baseKey,
                    new TargetObstacleCacheKey(
                        1000, 500, 123456789, 1, 2, 180, 80, true, 0.10, 0.01)),
                "F9 unchanged closed-bar context remains reusable");

            Console.WriteLine(
                "F9 target-obstacle cache key/invalidation contracts passed");
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
                    2.0,
                    12.0,
                    0.01);

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
                    1.8,
                    12.0,
                    0.01);

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
                    2.0,
                    12.0,
                    0.01);

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
                    2.0,
                    12.0,
                    0.01);

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
                    2.0,
                    12.0,
                    0.01);

            Assert(
                sell.Allowed &&
                sell.NominalRR > 2.4,
                "SELL reward-risk symmetry");
        }

        private static void VerifyCanonicalRiskRewardMath()
        {
            RiskRewardMathResult buy =
                RiskRewardMathRule.Evaluate(
                    1,
                    100.0,
                    98.0,
                    106.0,
                    0.20,
                    2.0,
                    12.0,
                    0.01);

            RiskRewardMathResult sell =
                RiskRewardMathRule.Evaluate(
                    -1,
                    100.0,
                    102.0,
                    94.0,
                    0.20,
                    2.0,
                    12.0,
                    0.01);

            Assert(
                buy.Valid &&
                sell.Valid &&
                Math.Abs(buy.Risk - 2.0) < 1e-12 &&
                Math.Abs(buy.Reward - 6.0) < 1e-12 &&
                Math.Abs(buy.EffectiveRisk - 2.20) < 1e-12 &&
                Math.Abs(buy.NominalRR - 3.0) < 1e-12 &&
                Math.Abs(buy.EffectiveRR - (5.80 / 2.20)) < 1e-12 &&
                Math.Abs(buy.NetReward - 5.80) < 1e-12,
                "CI-14 canonical BUY geometry");

            Assert(
                Math.Abs(
                    buy.NominalRR -
                    sell.NominalRR) < 1e-12 &&
                Math.Abs(
                    buy.EffectiveRR -
                    sell.EffectiveRR) < 1e-12,
                "CI-14 BUY/SELL RR mirror");

            RiskRewardMathResult floor =
                RiskRewardMathRule.Evaluate(
                    1,
                    100.0,
                    99.9995,
                    100.0045,
                    0,
                    0,
                    40.0,
                    0.01);

            Assert(
                floor.Valid &&
                Math.Abs(floor.Risk - 0.01) < 1e-12 &&
                Math.Abs(floor.NominalRR - 0.45) < 1e-12,
                "CI-14 pip-floor boundary");

            RiskRewardMathResult spread =
                RiskRewardMathRule.Evaluate(
                    1,
                    100.0,
                    98.0,
                    104.0,
                    0.50,
                    0,
                    12.0,
                    0.01);

            Assert(
                spread.Valid &&
                Math.Abs(spread.NominalRR - 2.0) < 1e-12 &&
                Math.Abs(spread.EffectiveRR - (3.50 / 2.50)) < 1e-12 &&
                Math.Abs(spread.NetReward - 3.50) < 1e-12,
                "CI-14 spread-adjusted RR is deterministic");

            RiskRewardMathResult wrongSide =
                RiskRewardMathRule.Evaluate(
                    1,
                    100.0,
                    98.0,
                    99.0,
                    0,
                    0,
                    12.0,
                    0.01);

            Assert(
                !wrongSide.Valid &&
                wrongSide.Reason == "TARGET WRONG SIDE",
                "CI-14 wrong-side target contract");

            Assert(
                Math.Abs(
                    RiskRewardMathRule.TargetFromRR(
                        1,
                        100.0,
                        2.0,
                        3.0) -
                    106.0) < 1e-12 &&
                Math.Abs(
                    RiskRewardMathRule.TargetFromRR(
                        1,
                        100.0,
                        2.0,
                        3.0,
                        0.50) -
                    108.0) < 1e-12 &&
                Math.Abs(
                    RiskRewardMathRule.TargetFromRR(
                        -1,
                        100.0,
                        2.0,
                        3.0) -
                    94.0) < 1e-12,
                "CI-14 synthetic target mirror");

            Assert(
                Math.Abs(
                    RiskRewardMathRule.DirectionalProgressRR(
                        1,
                        100.0,
                        106.0,
                        2.0,
                        0.01) -
                    3.0) < 1e-12 &&
                Math.Abs(
                    RiskRewardMathRule.DirectionalProgressRR(
                        -1,
                        100.0,
                        94.0,
                        2.0,
                        0.01) -
                    3.0) < 1e-12 &&
                RiskRewardMathRule.DirectionalProgressRR(
                    1,
                    100.0,
                    99.0,
                    2.0,
                    0.01) < 0,
                "CI-14 live progress RR mirror");
        }

        private static void VerifyExecutionTimeframeRoles()
        {
            Assert(
                ExecutionTimeframePolicy.IsPrimaryExecution("M15"),
                "M15 is primary execution timeframe");

            Assert(
                ExecutionTimeframePolicy.IsLowerDefensive("M5") &&
                ExecutionTimeframePolicy.IsLowerDefensive("M1"),
                "M5/M1 are defensive timeframes");

            Assert(
                ExecutionTimeframePolicy.IsHigherContext("H1") &&
                ExecutionTimeframePolicy.IsHigherContext("H4") &&
                ExecutionTimeframePolicy.IsHigherContext("D1") &&
                ExecutionTimeframePolicy.IsHigherContext("W1"),
                "H1+ are higher-timeframe context");

            Assert(
                !ExecutionTimeframePolicy.IsPrimaryExecution("M5"),
                "M5 cannot become primary execution");
        }

        private static void Assert(bool condition, string name)
        {
            if (!condition)
                throw new InvalidOperationException(
                    "Planning contract failed: " + name);
        }
    }
}
