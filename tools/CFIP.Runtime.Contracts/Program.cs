using System;
using System.Collections.Generic;
using System.IO;

namespace cAlgo
{
    internal static class Program
    {
        private static void Main()
        {
            VerifyM1TriggerSemantics();
            VerifySwingPlateauSemantics();
            VerifyFvgMathematics();
            VerifyFvgQualitySemantics();
            VerifyExecutionThresholdSemantics();
            VerifyVolumeSizingSemantics();
            VerifyOrderBlockMathematics();
            VerifyOrderBlockQualitySemantics();
            VerifyZoneConfluenceSymmetry();
            VerifyTopDownCalibration();
            VerifyProtectionProgressionSemantics();
            VerifyWaveTrendMathematics();
            VerifyWaveTrendEvidence();
            VerifyParallelOpportunityRule();
            VerifyMtfContextIntegrity();
            VerifySessionWindowSemantics();
            VerifyCalculationReadinessSemantics();
            VerifyBrokerStateRefreshSemantics();
            VerifyBufferedArchivePersistence();
            VerifyDailyLossSemantics();
            VerifyDailyLossBaselineSemantics();
            VerifyEconomicNewsFeedStateSemantics();
            VerifyEconomicNewsCurrencyMappingSemantics();
            VerifyClosedBarReferenceContract();
            VerifyMarketExecutionAcceptance();
            VerifyPendingOrderAcceptance();
            VerifyRejectedMutationHandling();
            VerifyFillEnvelopeSymmetry();
            VerifyInitialProtectionDirectionality();
            VerifyManagedBreakEvenDirectionality();
            VerifyProtectionProgression();
            VerifyTargetProgression();
            VerifyExecutionCapacity();
            VerifyStaleLivePlanRecovery();
            VerifyLifecycleFlows();
            VerifyLifecycleIdempotency();
            VerifyRuntimeStageIsolation();
            VerifyRuntimeFaultStateMachine();
            VerifyClosedBarRetryPolicy();
            VerifyUnifiedSubmissionGate();
            VerifyVisualAndExecutionControls();
            VerifyResponsivePanelRuntime();
            VerifyAggressiveEntryPolicy();
            VerifyTradePlanRegistry();
            VerifyScenarioExecutionPolicy();
            VerifyActionabilityAndDivergenceState();
            VerifyManagedIdentitySemantics();
            VerifyReversalProfitThresholdSemantics();
            VerifyStructuralEventSemantics();
            VerifyLiquiditySweepSemantics();
            VerifyRejectionSemantics();
            VerifyDivergenceConflictSemantics();
            VerifyStructuralTimeframeSemantics();
            VerifyReactionQualificationSemantics();
            VerifyIndicatorExecutionQualitySemantics();
            VerifyPendingDecisionArbiterSemantics();
            VerifyLifecycleOutcomeSemantics();

            Console.WriteLine("Runtime acceptance contracts OK");
        }


        private static void VerifySwingPlateauSemantics()
        {
            double[] highs = { 1, 2, 5, 5, 5, 3, 2, 4 };
            int start, end;
            double level;
            Assert(
                SwingPlateauRule.TryGetHighPlateau(highs.Length, 2, 1, 6, 0.001, i => highs[i], out start, out end, out level) &&
                start == 2 && end == 4 && level == 5,
                "flat swing-high plateau resolves once at its leftmost canonical bar");
            Assert(
                !SwingPlateauRule.TryGetHighPlateau(highs.Length, 3, 1, 6, 0.001, i => highs[i], out start, out end, out level),
                "interior plateau bars cannot create duplicate swing identities");
            Assert(
                !SwingPlateauRule.TryGetHighPlateau(highs.Length, 2, 1, 4, 0.001, i => highs[i], out start, out end, out level),
                "unconfirmed plateau cannot use bars beyond closed index");

            double[] lows = { 9, 8, 5, 5, 5, 7, 8, 6 };
            Assert(
                SwingPlateauRule.TryGetLowPlateau(lows.Length, 2, 1, 6, 0.001, i => lows[i], out start, out end, out level) &&
                start == 2 && end == 4 && level == 5,
                "flat swing-low plateau is directionally symmetric");

            Assert(
                SwingPlateauRule.IsWithinAnchor(100, 100.08, 0.1) &&
                !SwingPlateauRule.IsWithinAnchor(100, 100.19, 0.1),
                "equal-level membership is bounded by fixed anchor, not chained neighbors");
            Assert(
                SwingPlateauRule.IsWithinAnchor(100, 100.08, 0.1) &&
                SwingPlateauRule.IsWithinAnchor(100.08, 100.16, 0.1) &&
                !SwingPlateauRule.IsWithinAnchor(100, 100.16, 0.1),
                "transitive tolerance chain cannot manufacture one equal-level cluster");
            Assert(
                SwingPlateauRule.BreakIdentity(1, 12, 20) ==
                SwingPlateauRule.BreakIdentity(1, 12, 20) &&
                SwingPlateauRule.BreakIdentity(1, 12, 20) !=
                SwingPlateauRule.BreakIdentity(-1, 12, 20) &&
                SwingPlateauRule.BreakIdentity(1, 12, 20) !=
                SwingPlateauRule.BreakIdentity(1, 12, 21),
                "structural break identity distinguishes direction and closed-bar event");

            Assert(
                StructuralEvidenceRule.CanonicalEventCount(true, true, true) == 1 &&
                StructuralEvidenceRule.CanonicalEventCount(false, true, true) == 1 &&
                StructuralEvidenceRule.CanonicalEventCount(false, false, false) == 0,
                "structure, MSS and CHOCH collapse to one canonical event");
            Assert(
                !StructuralEvidenceRule.IsIndependentTransition(true, true, false) &&
                StructuralEvidenceRule.IsIndependentTransition(false, true, false) &&
                StructuralEvidenceRule.IsIndependentTransition(false, false, true),
                "transition is independent only when structure is absent");
        }

        private static void VerifySessionWindowSemantics()
        {
            Assert(
                SessionWindowRule.SessionResolutionMinutes == 60,
                "session parameters use explicit 60-minute UTC resolution");

            DateTime sameDay = Utc(12, 0);

            Assert(
                SessionWindowRule.IsInside(
                    sameDay,
                    6,
                    20),
                "same-day session accepts an in-session time");

            Assert(
                !SessionWindowRule.IsInside(
                    Utc(20, 0),
                    6,
                    20),
                "same-day session excludes its end boundary");

            Assert(
                SessionWindowRule.IsInside(
                    Utc(23, 0),
                    22,
                    6) &&
                SessionWindowRule.IsInside(
                    Utc(5, 59),
                    22,
                    6) &&
                !SessionWindowRule.IsInside(
                    Utc(6, 0),
                    22,
                    6),
                "overnight session is directionally correct at both boundaries");

            Assert(
                SessionWindowRule.IsInside(
                    Utc(5, 0),
                    6,
                    6),
                "start equals end represents a full-day session");

            DateTime sessionStart;
            DateTime sessionEnd;

            Assert(
                SessionWindowRule.TryResolveSessionWindow(
                    Utc(5, 0),
                    6,
                    20,
                    out sessionStart,
                    out sessionEnd) &&
                sessionStart == Utc(6, 0).AddDays(-1) &&
                sessionEnd == Utc(20, 0).AddDays(-1),
                "pre-session reference resolves the previous completed same-day window");

            Assert(
                SessionWindowRule.TryResolveSessionWindow(
                    Utc(3, 0),
                    22,
                    6,
                    out sessionStart,
                    out sessionEnd) &&
                sessionStart == Utc(22, 0).AddDays(-1) &&
                sessionEnd == Utc(6, 0),
                "overnight session range crosses midnight correctly");

            DateTime boundary;

            Assert(
                SessionWindowRule.TryResolveEndOfDayBoundary(
                    Utc(19, 45),
                    6,
                    20,
                    5,
                    out boundary) &&
                boundary == Utc(20, 0) &&
                SessionWindowRule.IsWithinPreBoundaryWindow(
                    Utc(19, 45),
                    boundary,
                    30),
                "same-day EOD pre-warning resolves the current session boundary");

            Assert(
                SessionWindowRule.IsWithinPostBoundaryWindow(
                    Utc(20, 3),
                    boundary,
                    5) &&
                !SessionWindowRule.IsWithinPostBoundaryWindow(
                    Utc(20, 6),
                    boundary,
                    5),
                "EOD cleanup is bounded to the five-minute post-boundary window");

            Assert(
                SessionWindowRule.TryResolveEndOfDayBoundary(
                    new DateTime(
                        2026, 1, 2, 5, 40, 0, DateTimeKind.Utc),
                    22,
                    6,
                    5,
                    out boundary) &&
                boundary ==
                    new DateTime(
                        2026, 1, 2, 6, 0, 0, DateTimeKind.Utc) &&
                SessionWindowRule.IsWithinPreBoundaryWindow(
                    new DateTime(
                        2026, 1, 2, 5, 40, 0, DateTimeKind.Utc),
                    boundary,
                    30),
                "overnight EOD warning uses the active session's next-day boundary");

            Assert(
                SessionWindowRule.TryResolveEndOfDayBoundary(
                    new DateTime(
                        2026, 1, 2, 6, 3, 0, DateTimeKind.Utc),
                    22,
                    6,
                    5,
                    out boundary) &&
                SessionWindowRule.IsWithinPostBoundaryWindow(
                    new DateTime(
                        2026, 1, 2, 6, 3, 0, DateTimeKind.Utc),
                    boundary,
                    5),
                "overnight EOD cleanup recognizes the completed session immediately after its boundary");

            int[] openMinutes = { 0, 60, 120, 180, 240 };

            Assert(
                ClosedBarReferenceRule.ResolveClosedIndex(
                    openMinutes.Length,
                    Utc(3, 0),
                    index =>
                        Utc(
                            openMinutes[index] / 60,
                            openMinutes[index] % 60)) == 2,
                "closed-bar reference resolves the latest fully closed period without a second-period offset");

            Assert(
                ClosedBarReferenceRule.IsFullyClosed(
                    openMinutes.Length,
                    2,
                    Utc(3, 0),
                    index =>
                        Utc(
                            openMinutes[index] / 60,
                            openMinutes[index] % 60)),
                "latest closed period is explicitly fully closed at its next open");

            Assert(
                !ClosedBarReferenceRule.IsFullyClosed(
                    openMinutes.Length,
                    3,
                    Utc(3, 0),
                    index =>
                        Utc(
                            openMinutes[index] / 60,
                            openMinutes[index] % 60)),
                "the current open period cannot be consumed as a closed reference");
        }

        private static void VerifyFvgMathematics()
        {
            double low;
            double high;
            double gap;

            Assert(
                FvgRule.TryGetThreeBarGap(
                    1, 100, 98, 102, 101,
                    out low, out high, out gap) &&
                low == 100 && high == 101 && gap == 1,
                "bullish 3-bar FVG uses older high to current low");

            Assert(
                !FvgRule.TryGetThreeBarGap(
                    1, 100, 98, 101, 100,
                    out low, out high, out gap) &&
                !FvgRule.TryGetThreeBarGap(
                    -1, 102, 100, 100, 99,
                    out low, out high, out gap),
                "FVG equality/touch is not a gap");

            Assert(
                FvgRule.TryGetThreeBarGap(
                    -1, 105, 104, 103, 102,
                    out low, out high, out gap) &&
                low == 103 && high == 104 && gap == 1,
                "bearish 3-bar FVG is directionally symmetric");

            Assert(
                FvgRule.TryGetTwoBarGap(
                    1, 100, 98, 102, 101,
                    out low, out high, out gap) &&
                low == 100 && high == 101 && gap == 1 &&
                FvgRule.TryGetTwoBarGap(
                    -1, 105, 104, 103, 102,
                    out low, out high, out gap) &&
                low == 103 && high == 104 && gap == 1,
                "two-bar imbalance geometry is explicit and symmetric");

            Assert(
                FvgRule.MeetsMinimumGap(0.80, 2.0, 0.30) &&
                !FvgRule.MeetsMinimumGap(0.80, 3.0, 0.30),
                "minimum FVG threshold is anchored to creation-bar ATR");

            Assert(
                FvgRule.IsOverlapInclusive(100, 101, 101, 102) &&
                !FvgRule.IsOverlapInclusive(100, 101, 101.01, 102) &&
                !FvgRule.IsOverlapInclusive(101, 100, 99, 102),
                "FVG overlap uses valid geometry and explicit boundary semantics");

            Assert(
                FvgRule.IsFullyFilled(1, 100, 101, 100) &&
                !FvgRule.IsFullyFilled(1, 100, 101, 100.01) &&
                FvgRule.IsFullyFilled(-1, 100, 101, 101) &&
                !FvgRule.IsFullyFilled(-1, 100, 101, 100.99),
                "bullish and bearish FVG full-fill boundaries are symmetric");

            Assert(
                FvgRule.TryApplyPartialMitigation(
                    1, 100, 101, 100.50, 0.01,
                    out low, out high) &&
                low == 100 && high == 100.50,
                "bullish partial FVG fill moves only the upper boundary");

            Assert(
                FvgRule.TryApplyPartialMitigation(
                    -1, 100, 101, 100.50, 0.01,
                    out low, out high) &&
                low == 100.50 && high == 101,
                "bearish partial FVG fill moves only the lower boundary");

            Assert(
                !FvgRule.TryApplyPartialMitigation(
                    1, 100, 101, 100, 0.01,
                    out low, out high) &&
                !FvgRule.TryApplyPartialMitigation(
                    -1, 100, 101, 101, 0.01,
                    out low, out high),
                "full fill cannot survive as an active partial FVG geometry");

            Assert(
                FvgRule.Identity(1, 42, false) ==
                FvgRule.Identity(1, 42, false) &&
                FvgRule.Identity(1, 42, false) !=
                FvgRule.Identity(1, 42, true) &&
                FvgRule.Identity(1, 42, false) !=
                FvgRule.Identity(-1, 42, false),
                "FVG identity separates direction and 3-bar/2-bar source variants");
        }

        private static void VerifyFvgQualitySemantics()
        {
            int weak =
                FvgQualityRule.Calculate(
                    0.08, 0.20, 30, 40, 0.05,
                    false, false, false);

            int moderate =
                FvgQualityRule.Calculate(
                    0.40, 0.80, 20, 40, 0.80,
                    true, false, false);

            int strong =
                FvgQualityRule.Calculate(
                    0.80, 1.00, 0, 40, 1.50,
                    true, true, false);

            Assert(
                weak < 60 &&
                moderate > weak &&
                strong > moderate &&
                strong >= 90,
                "FVG quality separates weak, contextual and strong candidates");

            Assert(
                FvgQualityRule.Calculate(
                    0.40, 0.80, 20, 40, 0.80,
                    true, true, false) >
                FvgQualityRule.Calculate(
                    0.40, 0.80, 20, 40, 0.80,
                    false, false, false),
                "structural and higher-timeframe context increase quality");

            Assert(
                FvgQualityRule.Calculate(
                    0.40, 1.0, 0, 40, 1.0,
                    true, true, false) >
                FvgQualityRule.Calculate(
                    0.40, 1.0, 20, 40, 1.0,
                    true, true, false),
                "fresher FVGs receive higher quality");

            Assert(
                FvgQualityRule.Calculate(
                    0.40, 1.0, 0, 40, 1.0,
                    true, true, false) >
                FvgQualityRule.Calculate(
                    0.40, 0.50, 0, 40, 1.0,
                    true, true, false),
                "remaining unmitigated geometry increases quality");

            Assert(
                FvgQualityRule.Calculate(
                    0.40, 1.0, 0, 40, 1.5,
                    true, true, false) >
                FvgQualityRule.Calculate(
                    0.40, 1.0, 0, 40, 0.2,
                    true, true, false),
                "directional displacement increases quality");

            Assert(
                FvgQualityRule.Calculate(
                    0.40, 1.0, 0, 40, 1.0,
                    true, true, false) -
                FvgQualityRule.Calculate(
                    0.40, 1.0, 0, 40, 1.0,
                    true, true, true) ==
                3,
                "two-bar imbalance retains an explicit quality penalty");

            Assert(
                FvgQualityRule.Calculate(
                    double.NaN,
                    1.0,
                    0,
                    40,
                    1.0,
                    true,
                    true,
                    false) == 0,
                "non-finite FVG quality input fails closed");
        }
        private static void VerifyExecutionThresholdSemantics()
        {
            Assert(
                IndicatorExecutionQualityRule.AutomaticMarketQualityMinimum == 60 &&
                IndicatorExecutionQualityRule.AutomaticMarketConflictMaximum == 52,
                "automatic-market indicator thresholds retain current defaults");

            Assert(
                IndicatorExecutionQualityRule.PendingSetupQualityMinimum == 62 &&
                IndicatorExecutionQualityRule.PendingContinuationConflictMaximum == 48 &&
                IndicatorExecutionQualityRule.PendingReversalConflictMaximum == 50 &&
                IndicatorExecutionQualityRule.PendingSubmissionQualityMinimum == 58 &&
                IndicatorExecutionQualityRule.PendingSubmissionConflictMaximum == 55,
                "pending-path indicator thresholds are explicitly centralized");

            Assert(
                ExecutionThresholdPolicy.NormalizeDirectionShare(49) == 50 &&
                ExecutionThresholdPolicy.NormalizeDirectionShare(57) == 57 &&
                ExecutionThresholdPolicy.NormalizeDirectionShare(96) == 95,
                "direction-share bounds match the public parameter range");

            Assert(
                ExecutionThresholdPolicy.NormalizeReversalEvidence(1) == 2 &&
                ExecutionThresholdPolicy.NormalizeReversalEvidence(5) == 5 &&
                ExecutionThresholdPolicy.NormalizeReversalMtf(49) == 50 &&
                ExecutionThresholdPolicy.NormalizeReversalMtf(72) == 72,
                "reversal evidence and MTF defensive bounds are explicit");

            Assert(
                ExecutionThresholdPolicy.NormalizeEndOfDayAlertMinutesBefore(4) == 5 &&
                ExecutionThresholdPolicy.NormalizeEndOfDayAlertMinutesBefore(30) == 30,
                "EOD alert defensive bounds match parameter limits");

            Assert(
                Math.Abs(ExecutionThresholdPolicy.NormalizeMaximumSpreadToStopRiskRatio(0.01) - 0.02) < 0.0000001 &&
                Math.Abs(ExecutionThresholdPolicy.NormalizeMaximumSpreadToStopRiskRatio(0.18) - 0.18) < 0.0000001 &&
                Math.Abs(ExecutionThresholdPolicy.NormalizeMaximumSpreadToStopRiskRatio(0.60) - 0.50) < 0.0000001 &&
                Math.Abs(ExecutionThresholdPolicy.NormalizeMaximumSpreadToStopRiskRatio(double.NaN) - 0.02) < 0.0000001,
                "spread/stop ratio is finite and parameter-aligned");
        }

        private static void VerifyVolumeSizingSemantics()
        {
            Assert(
                VolumeSizingRule.IsValidStopPips(0.1) &&
                !VolumeSizingRule.IsValidStopPips(0) &&
                !VolumeSizingRule.IsValidStopPips(-1) &&
                !VolumeSizingRule.IsValidStopPips(double.NaN) &&
                !VolumeSizingRule.IsValidStopPips(double.PositiveInfinity),
                "volume sizing rejects zero, negative and non-finite stop risk");

            Assert(
                VolumeSizingRule.IsValidRiskInput(1000, 0.5) &&
                !VolumeSizingRule.IsValidRiskInput(0, 0.5) &&
                !VolumeSizingRule.IsValidRiskInput(1000, double.NaN),
                "risk sizing accepts only finite positive account inputs");

            Assert(
                Math.Abs(RiskAmountCalculator.Calculate(1000, 0.5) - 5.0) < 0.0000001 &&
                RiskAmountCalculator.Calculate(1000, 0) == 0 &&
                RiskAmountCalculator.Calculate(double.NaN, 0.5) == 0 &&
                RiskAmountCalculator.Calculate(1000, double.PositiveInfinity) == 0,
                "risk amount calculation is finite and fail-closed");

            Assert(
                VolumeSizingRule.IsValidNormalizedVolume(1000, 1000, 10000) &&
                !VolumeSizingRule.IsValidNormalizedVolume(999, 1000, 10000) &&
                !VolumeSizingRule.IsValidNormalizedVolume(10001, 1000, 10000) &&
                !VolumeSizingRule.IsValidNormalizedVolume(double.NaN, 1000, 10000),
                "normalized volume must remain inside broker bounds");

            Assert(
                Math.Abs(RiskPercentPolicy.Calculate(7, false, 1.0) - 5.0) < 0.0000001 &&
                Math.Abs(RiskPercentPolicy.Calculate(0.5, false, 1.0) - 0.5) < 0.0000001 &&
                Math.Abs(RiskPercentPolicy.Calculate(1.0, true, 0.5) - 0.5) < 0.0000001,
                "risk percent policy respects the five-percent parameter ceiling");
        }

        private static void VerifyDailyLossBaselineSemantics()
        {
            double startEquity;
            double baselineFloating;

            Assert(
                DailyLossBaselineRule.TryReconstruct(
                    975,
                    0,
                    -25,
                    0,
                    false,
                    out startEquity,
                    out baselineFloating) &&
                Math.Abs(startEquity - 1000) < 0.0001 &&
                Math.Abs(baselineFloating) < 0.0001,
                "midday restart reconstructs start-of-day equity from realized facts");

            Assert(
                DailyLossBaselineRule.TryReconstruct(
                    1480,
                    0,
                    -20,
                    500,
                    false,
                    out startEquity,
                    out baselineFloating) &&
                Math.Abs(startEquity - 1000) < 0.0001,
                "midday deposit is excluded from reconstructed trading loss");

            Assert(
                !DailyLossBaselineRule.TryReconstruct(
                    1000,
                    -12,
                    0,
                    0,
                    true,
                    out startEquity,
                    out baselineFloating),
                "restart baseline reconstruction refuses unknown prior floating P/L");

            Assert(
                !DailyLossBaselineRule.TryReconstruct(
                    0,
                    0,
                    0,
                    0,
                    false,
                    out startEquity,
                    out baselineFloating),
                "invalid equity cannot produce a synthetic baseline");
        }

        private static void VerifyBrokerStateRefreshSemantics()
        {
            DateTime now =
                new DateTime(
                    2026,
                    9,
                    30,
                    12,
                    0,
                    0,
                    DateTimeKind.Utc);

            Assert(
                BrokerStateRefreshRule.IsRefreshDue(
                    true,
                    now,
                    now,
                    1000),
                "dirty broker state always requires reconciliation");

            Assert(
                BrokerStateRefreshRule.IsRefreshDue(
                    false,
                    DateTime.MinValue,
                    now,
                    1000),
                "broker state without a prior refresh requires reconciliation");

            Assert(
                !BrokerStateRefreshRule.IsRefreshDue(
                    false,
                    now,
                    now.AddMilliseconds(999),
                    1000) &&
                BrokerStateRefreshRule.IsRefreshDue(
                    false,
                    now,
                    now.AddMilliseconds(1000),
                    1000),
                "broker refresh cadence is bounded by the configured interval");

            Assert(
                BrokerStateRefreshRule.IsRefreshDue(
                    false,
                    now,
                    now.AddMilliseconds(-1),
                    1000),
                "broker refresh becomes due after clock rollback");
        }

        private static void VerifyBufferedArchivePersistence()
        {
            string directory =
                Path.Combine(
                    Path.GetTempPath(),
                    "CFIP.Runtime.Contracts",
                    "buffered-archive");

            Directory.CreateDirectory(
                directory);

            string path =
                Path.Combine(
                    directory,
                    "buffered.csv");

            try
            {
                if (File.Exists(path))
                    File.Delete(path);

                BufferedArchivePersistence store =
                    new BufferedArchivePersistence();

                Assert(
                    store.Enqueue(
                        path,
                        "CFIP-TEST,1",
                        "1,alpha",
                        "1"),
                    "first keyed archive row is queued");

                Assert(
                    !store.Enqueue(
                        path,
                        "CFIP-TEST,1",
                        "1,duplicate",
                        "1"),
                    "duplicate queued archive key is rejected");

                Assert(
                    store.Enqueue(
                        path,
                        "CFIP-TEST,1",
                        "2,beta",
                        "2"),
                    "second keyed archive row is queued");

                Assert(
                    store.PendingLineCount == 2,
                    "buffer contains exactly unique pending rows");

                int firstFlush =
                    store.Flush(1);

                Assert(
                    firstFlush == 1 &&
                    store.PendingLineCount == 1,
                    "flush budget writes only one queued row");

                int secondFlush =
                    store.Flush(1);

                Assert(
                    secondFlush == 1 &&
                    store.PendingLineCount == 0,
                    "second bounded flush drains remaining row");

                string[] lines =
                    File.ReadAllLines(path);

                Assert(
                    lines.Length == 3 &&
                    lines[0] == "CFIP-TEST,1" &&
                    lines[1] == "1,alpha" &&
                    lines[2] == "2,beta",
                    "buffered archive writes header and each unique row once");

                Assert(
                    !store.Enqueue(
                        path,
                        "CFIP-TEST,1",
                        "2,duplicate-after-flush",
                        "2"),
                    "existing on-disk keyed row remains idempotent");

                Assert(
                    store.PendingLineCount == 0,
                    "existing archive key creates no pending duplicate");
            }
            finally
            {
                try
                {
                    if (File.Exists(path))
                        File.Delete(path);

                    if (Directory.Exists(directory))
                        Directory.Delete(
                            directory,
                            true);
                }
                catch
                {
                    // Test cleanup must not mask the contract result.
                }
            }
        }

        private static void VerifyCalculationReadinessSemantics()
        {
            DateTime now =
                new DateTime(
                    2026,
                    9,
                    30,
                    12,
                    0,
                    0,
                    DateTimeKind.Utc);

            Assert(
                CalculationReadinessRule.ResolveState(
                    false,
                    false,
                    false,
                    -1) ==
                CalculationReadinessState.BuildingHistory,
                "missing bars remain in BUILDING_HISTORY");

            Assert(
                CalculationReadinessRule.ResolveState(
                    true,
                    false,
                    false,
                    -1) ==
                CalculationReadinessState.BuildingHistory,
                "insufficient history remains in BUILDING_HISTORY");

            Assert(
                CalculationReadinessRule.ResolveState(
                    true,
                    true,
                    true,
                    -1) ==
                CalculationReadinessState.WaitingForClosedM5,
                "no stable closed M5 is explicit WAITING_FOR_CLOSED_M5");

            Assert(
                CalculationReadinessRule.ResolveState(
                    true,
                    true,
                    false,
                    30) ==
                CalculationReadinessState.WaitingForMtfData,
                "primary MTF data gap is explicit WAITING_FOR_MTF_DATA");

            Assert(
                CalculationReadinessRule.ResolveState(
                    true,
                    true,
                    true,
                    30) ==
                CalculationReadinessState.Ready,
                "complete closed MTF context becomes READY");

            Assert(
                CalculationReadinessRule.IsProbeDue(
                    DateTime.MinValue,
                    now,
                    500),
                "first readiness probe is due immediately");

            Assert(
                !CalculationReadinessRule.IsProbeDue(
                    now,
                    now.AddMilliseconds(499),
                    500) &&
                CalculationReadinessRule.IsProbeDue(
                    now,
                    now.AddMilliseconds(500),
                    500),
                "waiting readiness probe is bounded by an explicit interval");

            Assert(
                CalculationReadinessRule.IsProbeDue(
                    now,
                    now.AddMilliseconds(-1),
                    500),
                "clock rollback reopens readiness probing");

            Assert(
                CalculationReadinessRule.ProbeIntervalMilliseconds(
                    CalculationReadinessState.WaitingForMtfData) == 500 &&
                CalculationReadinessRule.ProbeIntervalMilliseconds(
                    CalculationReadinessState.BuildingHistory) == 250 &&
                CalculationReadinessRule.ProbeIntervalMilliseconds(
                    CalculationReadinessState.Ready) == 0,
                "readiness backoff intervals are deterministic");

            Assert(
                CalculationReadinessRule.StatusText(
                    CalculationReadinessState.WaitingForMtfData) ==
                    "WAITING FOR MTF DATA" &&
                CalculationReadinessRule.StatusText(
                    CalculationReadinessState.Ready) ==
                    "READY",
                "readiness status text is deterministic");
        }

        private static void VerifyDailyLossSemantics()
        {
            DailyLossEvaluation evaluation =
                DailyLossRule.Evaluate(
                    1000,
                    0,
                    970,
                    0,
                    -30,
                    0,
                    3.0,
                    true,
                    true,
                    false);

            Assert(
                evaluation.DataReady &&
                evaluation.LimitHit &&
                evaluation.Locked &&
                Math.Abs(evaluation.LossAmount - 30) < 0.0001 &&
                Math.Abs(evaluation.LossPercent - 3.0) < 0.0001,
                "realized daily loss is measured from a stable equity baseline");

            evaluation =
                DailyLossRule.Evaluate(
                    1000,
                    -20,
                    970,
                    -50,
                    0,
                    0,
                    3.0,
                    true,
                    true,
                    false);

            Assert(
                evaluation.LimitHit &&
                Math.Abs(evaluation.LossAmount - 30) < 0.0001,
                "change in floating P/L from the day baseline is included");

            evaluation =
                DailyLossRule.Evaluate(
                    1000,
                    0,
                    1500,
                    0,
                    0,
                    500,
                    3.0,
                    false,
                    true,
                    false);

            Assert(
                !evaluation.LimitHit &&
                evaluation.UsedEquityFallback &&
                Math.Abs(evaluation.DailyNetPnl) < 0.0001,
                "deposit does not become daily trading loss in equity fallback");

            evaluation =
                DailyLossRule.Evaluate(
                    1000,
                    0,
                    700,
                    0,
                    0,
                    -300,
                    3.0,
                    false,
                    true,
                    false);

            Assert(
                !evaluation.LimitHit &&
                evaluation.UsedEquityFallback &&
                Math.Abs(evaluation.DailyNetPnl) < 0.0001,
                "withdrawal does not become daily trading loss in equity fallback");

            evaluation =
                DailyLossRule.Evaluate(
                    1000,
                    0,
                    970,
                    0,
                    0,
                    0,
                    3.0,
                    true,
                    true,
                    true);

            Assert(
                evaluation.LimitHit &&
                evaluation.Locked &&
                evaluation.Reason ==
                    "DAILY LOSS LIMIT ALREADY LOCKED",
                "daily loss lock remains latched after equity recovery");

            evaluation =
                DailyLossRule.Evaluate(
                    1000,
                    0,
                    969.9,
                    0,
                    -30.1,
                    0,
                    3.0,
                    true,
                    true,
                    false);

            Assert(
                evaluation.LimitHit &&
                Math.Abs(
                    evaluation.LossPercent -
                    3.01) < 0.02,
                "threshold comparison is inclusive at the configured percentage");

            evaluation =
                DailyLossRule.Evaluate(
                    1000,
                    0,
                    970,
                    double.NaN,
                    -30,
                    0,
                    3.0,
                    true,
                    true,
                    false);

            Assert(
                !evaluation.DataReady,
                "non-finite floating P/L cannot produce a false-safe result");

            evaluation =
                DailyLossRule.Evaluate(
                    1000,
                    0,
                    970,
                    0,
                    -30,
                    0,
                    double.NaN,
                    true,
                    true,
                    false);

            Assert(
                !evaluation.DataReady,
                "non-finite daily-loss threshold fails closed");

            evaluation =
                DailyLossRule.Evaluate(
                    1000,
                    0,
                    970,
                    0,
                    -30,
                    0,
                    3.0,
                    true,
                    false,
                    false);

            Assert(
                !evaluation.DataReady &&
                evaluation.Reason ==
                    "DAILY LOSS TRANSACTION DATA UNAVAILABLE",
                "missing transaction facts fail closed");

            evaluation =
                DailyLossRule.Evaluate(
                    0,
                    0,
                    1000,
                    0,
                    0,
                    0,
                    3.0,
                    true,
                    true,
                    false);

            Assert(
                !evaluation.DataReady &&
                !evaluation.LimitHit,
                "invalid baseline is reported as unavailable rather than as a false limit hit");

            evaluation =
                DailyLossRule.Evaluate(
                    1000,
                    0,
                    975,
                    0,
                    -25,
                    0,
                    0,
                    true,
                    true,
                    false);

            Assert(
                !evaluation.LimitHit,
                "zero configured threshold does not create an implicit hidden floor");
        }

        private static void VerifyEconomicNewsFeedStateSemantics()
        {
            DateTime now =
                new DateTime(
                    2026,
                    9,
                    30,
                    12,
                    0,
                    0,
                    DateTimeKind.Utc);

            Assert(
                EconomicNewsFeedStateRule.Evaluate(
                    false,
                    DateTime.MinValue,
                    now,
                    90,
                    false) ==
                EconomicNewsFeedState.Disabled,
                "disabled news feed reports DISABLED state");

            Assert(
                EconomicNewsFeedStateRule.Evaluate(
                    true,
                    DateTime.MinValue,
                    now,
                    90,
                    false) ==
                EconomicNewsFeedState.NeverLoaded,
                "missing successful refresh reports NEVER_LOADED");

            Assert(
                EconomicNewsFeedStateRule.Evaluate(
                    true,
                    now.AddMinutes(-30),
                    now,
                    90,
                    false) ==
                EconomicNewsFeedState.Healthy,
                "fresh successful refresh reports HEALTHY");

            Assert(
                EconomicNewsFeedStateRule.Evaluate(
                    true,
                    now.AddMinutes(-91),
                    now,
                    90,
                    false) ==
                EconomicNewsFeedState.Stale,
                "expired successful refresh reports STALE");

            Assert(
                EconomicNewsFeedStateRule.Evaluate(
                    true,
                    now.AddMinutes(-1),
                    now,
                    90,
                    true) ==
                EconomicNewsFeedState.BlockingEvent,
                "active event overrides feed health with BLOCKING_EVENT");
        }

        private static void VerifyEconomicNewsCurrencyMappingSemantics()
        {
            string[] eurUsd =
                EconomicNewsCurrencyRule.ResolveCurrencies(
                    "EURUSD.m",
                    "",
                    "");

            Assert(
                eurUsd.Length == 2 &&
                eurUsd[0] == "EUR" &&
                eurUsd[1] == "USD",
                "FX symbol resolves both traded currencies");

            string[] indexUsd =
                EconomicNewsCurrencyRule.ResolveCurrencies(
                    "US30.cash",
                    "",
                    "US30=USD;GER40=EUR");

            Assert(
                indexUsd.Length == 1 &&
                indexUsd[0] == "USD",
                "index mapping resolves configured USD news relevance");

            string[] indexEur =
                EconomicNewsCurrencyRule.ResolveCurrencies(
                    "GER40",
                    "",
                    "US30=USD;GER40=EUR");

            Assert(
                indexEur.Length == 1 &&
                indexEur[0] == "EUR",
                "index mapping resolves configured EUR news relevance");

            string[] crypto =
                EconomicNewsCurrencyRule.ResolveCurrencies(
                    "BTCUSDT",
                    "",
                    "BTC=USD;ETH=USD");

            Assert(
                crypto.Length == 1 &&
                crypto[0] == "USD",
                "crypto mapping resolves USD news relevance");

            string[] extras =
                EconomicNewsCurrencyRule.ResolveCurrencies(
                    "EURUSD",
                    "JPY, CHF",
                    "");

            Assert(
                extras.Length == 4 &&
                extras[0] == "CHF" &&
                extras[1] == "EUR" &&
                extras[2] == "JPY" &&
                extras[3] == "USD",
                "additional currencies merge deterministically without duplicates");

            string[] normalized =
                EconomicNewsCurrencyRule.ResolveCurrencies(
                    "US_500.cash",
                    "",
                    "US500=USD");

            Assert(
                normalized.Length == 1 &&
                normalized[0] == "USD",
                "broker symbol punctuation does not break configured mapping");
        }

        private static void VerifyM1TriggerSemantics()
        {
            DateTime m5Open = Utc(12, 0);
            DateTime m5NextOpen = Utc(12, 5);
            DateTime m1Open = Utc(12, 4);
            DateTime m1NextOpen = Utc(12, 5);

            Assert(
                M1TriggerRule.IsClosedInsideM5Window(
                    m1Open,
                    m1NextOpen,
                    m5Open,
                    m5NextOpen,
                    Utc(12, 5)),
                "closed M1 bar belongs to the closed M5 window");

            Assert(
                !M1TriggerRule.IsClosedInsideM5Window(
                    Utc(12, 5),
                    Utc(12, 6),
                    m5Open,
                    m5NextOpen,
                    Utc(12, 5)),
                "M1 bar outside the M5 window is rejected");

            Assert(
                !M1TriggerRule.IsClosedInsideM5Window(
                    m1Open,
                    m1NextOpen,
                    m5Open,
                    m5NextOpen,
                    Utc(12, 4)),
                "M1 bar still open at the reference is rejected");

            Assert(
                M1TriggerRule.IsReady(
                    1,
                    1,
                    100,
                    104,
                    99,
                    103,
                    2,
                    0.50,
                    0.70,
                    2.5,
                    4,
                    4,
                    102,
                    98,
                    0.05,
                    false,
                    0),
                "bullish M1 trigger requires and accepts a causal micro-structure break");

            Assert(
                M1TriggerRule.IsReady(
                    -1,
                    -1,
                    100,
                    101,
                    96,
                    97,
                    2,
                    0.50,
                    0.70,
                    2.5,
                    4,
                    4,
                    102,
                    98,
                    0.05,
                    false,
                    0),
                "bearish M1 trigger is directionally symmetric");

            Assert(
                M1TriggerRule.IsReady(
                    1,
                    1,
                    100,
                    104,
                    99,
                    103,
                    2,
                    0.12,
                    0.70,
                    2.5,
                    6,
                    4,
                    104,
                    98,
                    0.05,
                    true,
                    0.80),
                "configured displacement can provide the causal confirmation when micro-break is absent");

            Assert(
                !M1TriggerRule.IsReady(
                    1,
                    1,
                    100,
                    104,
                    99,
                    103,
                    2,
                    0.12,
                    0.70,
                    2.5,
                    6,
                    4,
                    104,
                    98,
                    0.05,
                    false,
                    0),
                "high technical trigger score cannot replace structural/displacement evidence");

            Assert(
                !M1TriggerRule.IsReady(
                    1,
                    1,
                    100,
                    103,
                    99,
                    102,
                    2,
                    0.50,
                    0.70,
                    2.5,
                    4,
                    4,
                    102.5,
                    98,
                    0.05,
                    false,
                    0),
                "a close that does not clear the buffered prior micro-high cannot confirm");

            Assert(
                !M1TriggerRule.IsReady(
                    1,
                    -1,
                    100,
                    104,
                    99,
                    103,
                    2,
                    0.50,
                    0.70,
                    2.5,
                    4,
                    4,
                    102,
                    98,
                    0.05,
                    true,
                    0.80),
                "opposite M1 direction cannot confirm the selected decision");

            Assert(
                !M1TriggerRule.IsReady(
                    1,
                    1,
                    100,
                    101,
                    99,
                    100.5,
                    2,
                    0.50,
                    0.70,
                    2.5,
                    4,
                    4,
                    100,
                    98,
                    0.05,
                    false,
                    0),
                "weak M1 body cannot confirm");

            Assert(
                !M1TriggerRule.IsReady(
                    1,
                    1,
                    100,
                    104,
                    99,
                    100.5,
                    2,
                    0.12,
                    0.70,
                    2.5,
                    4,
                    4,
                    100,
                    98,
                    0.05,
                    false,
                    0),
                "poor M1 close location cannot confirm");

            Assert(
                !M1TriggerRule.IsReady(
                    1,
                    1,
                    100,
                    103,
                    99,
                    102,
                    2,
                    0.50,
                    0.70,
                    2.5,
                    3,
                    4,
                    101,
                    98,
                    0.05,
                    false,
                    0),
                "insufficient M1 trigger score cannot confirm");

            Assert(
                !M1TriggerRule.IsReady(
                    1,
                    1,
                    100,
                    107,
                    99,
                    106,
                    2,
                    0.50,
                    0.70,
                    3.0,
                    4,
                    4,
                    102,
                    98,
                    0.05,
                    false,
                    0),
                "abnormally large M1 range cannot confirm");
        }

        private static void VerifyZoneConfluenceSymmetry()
        {
            DateTime liveM5Open = Utc(12, 5);
            DateTime liveM5NextOpen = Utc(12, 10);
            DateTime closedM1Open = Utc(12, 6);
            DateTime closedM1NextOpen = Utc(12, 7);

            Assert(
                M1TriggerRule.IsClosedM1InsideM5Window(
                    closedM1Open,
                    closedM1NextOpen,
                    liveM5Open,
                    liveM5NextOpen,
                    Utc(12, 7)),
                "closed M1 can confirm inside the currently forming M5 window");

            Assert(
                !M1TriggerRule.IsClosedM1InsideM5Window(
                    closedM1Open,
                    Utc(12, 8),
                    liveM5Open,
                    liveM5NextOpen,
                    Utc(12, 7)),
                "M1 trigger runtime rejects an M1 bar that is not closed yet");

            Assert(
                ZoneConfluenceRule.HasOverlap(
                    100,
                    105,
                    104,
                    110,
                    0) &&
                ZoneConfluenceRule.HasOverlap(
                    104,
                    110,
                    100,
                    105,
                    0),
                "zone overlap is BUY/SELL independent and argument-order symmetric");

            Assert(
                !ZoneConfluenceRule.HasOverlap(
                    100,
                    102,
                    102,
                    104,
                    0),
                "touch-only zone boundaries are not treated as positive-width confluence");

            Assert(
                ZoneConfluenceRule.HasOverlap(
                    100,
                    102,
                    102.05,
                    104,
                    0.10),
                "explicit confluence tolerance is symmetric");

            Assert(
                ZoneConfluenceRule.IsDirectionalMatch(1, 1) &&
                ZoneConfluenceRule.IsDirectionalMatch(-1, -1) &&
                !ZoneConfluenceRule.IsDirectionalMatch(1, -1) &&
                !ZoneConfluenceRule.IsDirectionalMatch(-1, 1),
                "zone direction matching is explicitly mirrored");
        }

        private static void VerifyTopDownCalibration()
        {
            TopDownCalibrationSnapshot strongBuy =
                TopDownCalibrationRule.Evaluate(
                    new[] { 1, 1, 1, 0 },
                    new[] { 90, 84, 80, 0 },
                    new[] { 3.0, 2.0, 2.0, 0.0 },
                    new[] { 1, 1 },
                    new[] { 82, 78 },
                    new[] { 5.0, 8.0 },
                    1,
                    85,
                    72,
                    1);

            Assert(
                strongBuy.HtfStrong &&
                strongBuy.HtfDirection == 1 &&
                strongBuy.Eligible &&
                strongBuy.Stage == "ENTRY CALIBRATED",
                "strong H1+ anchor plus aligned mid/entry frames calibrates an actionable setup");

            TopDownCalibrationSnapshot entryConflict =
                TopDownCalibrationRule.Evaluate(
                    new[] { 1, 1, 1, 0 },
                    new[] { 90, 84, 80, 0 },
                    new[] { 3.0, 2.0, 2.0, 0.0 },
                    new[] { 1, 1 },
                    new[] { 82, 78 },
                    new[] { 5.0, 8.0 },
                    -1,
                    85,
                    72,
                    -1);

            Assert(
                !entryConflict.Eligible &&
                entryConflict.Stage == "ENTRY CONFLICT",
                "M5 entry direction cannot override a strong H1+ anchor");

            TopDownCalibrationSnapshot midConflict =
                TopDownCalibrationRule.Evaluate(
                    new[] { 1, 1, 1, 0 },
                    new[] { 90, 84, 80, 0 },
                    new[] { 3.0, 2.0, 2.0, 0.0 },
                    new[] { -1, -1 },
                    new[] { 82, 78 },
                    new[] { 5.0, 8.0 },
                    1,
                    85,
                    72,
                    1);

            Assert(
                !midConflict.Eligible &&
                midConflict.Stage == "MIDFRAME CONFLICT",
                "strong middle-timeframe conflict blocks lower-frame override");

            TopDownCalibrationSnapshot mixedHtf =
                TopDownCalibrationRule.Evaluate(
                    new[] { 1, -1, 0, 0 },
                    new[] { 90, 90, 0, 0 },
                    new[] { 3.0, 2.0, 2.0, 0.0 },
                    new[] { 1, 0 },
                    new[] { 85, 0 },
                    new[] { 5.0, 8.0 },
                    1,
                    85,
                    72,
                    1);

            Assert(
                mixedHtf.Stage == "HTF MIXED",
                "mixed H1+ context never pretends to be a calibrated anchor");

            TopDownCalibrationSnapshot weakMiddleAgreement =
                TopDownCalibrationRule.Evaluate(
                    new[] { 1, 1, 1, 0 },
                    new[] { 90, 84, 80, 0 },
                    new double[] { 3.0, 2.0, 2.0, 0.0 },
                    new[] { 1, -1 },
                    new[] { 100, 100 },
                    new double[] { 5.0, 5.0 },
                    1,
                    85,
                    72,
                    1);

            Assert(
                weakMiddleAgreement.MidDirection == 0 &&
                weakMiddleAgreement.Stage == "MIDFRAME CALIBRATION",
                "directionally mixed middle frames cannot be promoted to calibrated entry");

        }

        private static void VerifyWaveTrendMathematics()
        {
            Assert(
                WaveTrendMovingAverageCalculator.RequiredSourceBars(
                    WaveTrendMovingAverageCalculator.Simple,
                    3) == 3 &&
                WaveTrendMovingAverageCalculator.RequiredSourceBars(
                    WaveTrendMovingAverageCalculator.DoubleExponential,
                    3) == 5 &&
                WaveTrendMovingAverageCalculator.RequiredSourceBars(
                    WaveTrendMovingAverageCalculator.TripleExponential,
                    3) == 7 &&
                WaveTrendMovingAverageCalculator.RequiredSourceBars(
                    WaveTrendMovingAverageCalculator.Hull,
                    4) == 5,
                "WaveTrend MA dependency depth is explicit for simple, DEMA, TEMA and HMA");

            double[] source =
            {
                1, 2, 3, 4, 5, 6, 7
            };

            WaveTrendMovingAverageCalculator simple =
                new WaveTrendMovingAverageCalculator(
                    WaveTrendMovingAverageCalculator.Simple,
                    3,
                    0);

            double value;
            Assert(
                !simple.TryCalculateWaveTrendAverage(source, 1, out value) &&
                simple.TryCalculateWaveTrendAverage(source, 2, out value) &&
                value == 2,
                "WaveTrend SMA uses a closed source window");

            WaveTrendMovingAverageCalculator exponential =
                new WaveTrendMovingAverageCalculator(
                    WaveTrendMovingAverageCalculator.Exponential,
                    3,
                    0);

            Assert(
                exponential.TryCalculateWaveTrendAverage(source, 2, out value) &&
                value == 2 &&
                exponential.TryCalculateWaveTrendAverage(source, 3, out value) &&
                value == 3,
                "WaveTrend EMA seeds from an SMA and then uses alpha 2/(L+1)");

            WaveTrendMovingAverageCalculator wilder =
                new WaveTrendMovingAverageCalculator(
                    WaveTrendMovingAverageCalculator.WilderSmoothing,
                    3,
                    0);

            Assert(
                wilder.TryCalculateWaveTrendAverage(source, 2, out value) &&
                value == 2 &&
                wilder.TryCalculateWaveTrendAverage(source, 3, out value) &&
                Math.Abs(value - 2.6666666667) < 1e-9,
                "WaveTrend Wilder smoothing uses alpha 1/L");

            WaveTrendMovingAverageCalculator weighted =
                new WaveTrendMovingAverageCalculator(
                    WaveTrendMovingAverageCalculator.Weighted,
                    3,
                    0);

            Assert(
                weighted.TryCalculateWaveTrendAverage(source, 2, out value) &&
                Math.Abs(value - 14.0 / 6.0) < 1e-9,
                "WaveTrend WMA uses linearly increasing weights");

            WaveTrendMovingAverageCalculator timeSeries =
                new WaveTrendMovingAverageCalculator(
                    WaveTrendMovingAverageCalculator.TimeSeries,
                    3,
                    0);

            Assert(
                timeSeries.TryCalculateWaveTrendAverage(source, 2, out value) &&
                value == 3,
                "WaveTrend TimeSeries MA uses the regression endpoint");

            WaveTrendMovingAverageCalculator triangular =
                new WaveTrendMovingAverageCalculator(
                    WaveTrendMovingAverageCalculator.Triangular,
                    3,
                    0);

            Assert(
                triangular.TryCalculateWaveTrendAverage(source, 2, out value) &&
                value == 2,
                "WaveTrend triangular smoothing composes two canonical SMA windows");

            WaveTrendMovingAverageCalculator vidya =
                new WaveTrendMovingAverageCalculator(
                    WaveTrendMovingAverageCalculator.Vidya,
                    3,
                    0);

            Assert(
                vidya.TryCalculateWaveTrendAverage(source, 2, out value) &&
                value == 2 &&
                vidya.TryCalculateWaveTrendAverage(source, 3, out value) &&
                Math.Abs(value - 3.0) < 1e-9,
                "WaveTrend VIDYA adapts EMA alpha from source momentum efficiency");

            WaveTrendMovingAverageCalculator hull =
                new WaveTrendMovingAverageCalculator(
                    WaveTrendMovingAverageCalculator.Hull,
                    4,
                    0);

            Assert(
                hull.TryCalculateWaveTrendAverage(source, 4, out value) &&
                Math.Abs(value - 3.0) < 1e-9,
                "WaveTrend HMA composes half-length WMA, full-length WMA and sqrt-length WMA");

            WaveTrendMovingAverageCalculator dema =
                new WaveTrendMovingAverageCalculator(
                    WaveTrendMovingAverageCalculator.DoubleExponential,
                    3,
                    0);

            Assert(
                dema.TryCalculateWaveTrendAverage(source, 4, out value) &&
                Math.Abs(value - 5.0) < 1e-9,
                "WaveTrend DEMA is 2*EMA1-EMA2 with explicit warm-up");

            WaveTrendMovingAverageCalculator tema =
                new WaveTrendMovingAverageCalculator(
                    WaveTrendMovingAverageCalculator.TripleExponential,
                    3,
                    0);

            Assert(
                tema.TryCalculateWaveTrendAverage(source, 6, out value) &&
                Math.Abs(value - 7.0) < 1e-9,
                "WaveTrend TEMA is 3*EMA1-3*EMA2+EMA3 with explicit warm-up");

            WaveTrendMovingAverageCalculator kama =
                new WaveTrendMovingAverageCalculator(
                    WaveTrendMovingAverageCalculator.KaufmanAdaptive,
                    3,
                    0);

            Assert(
                kama.TryCalculateWaveTrendAverage(source, 2, out value) &&
                value == 2 &&
                kama.TryCalculateWaveTrendAverage(source, 3, out value) &&
                Math.Abs(value - 2.8888888889) < 1e-9,
                "WaveTrend Kaufman Adaptive MA uses efficiency-ratio smoothing");

            int componentReady =
                WaveTrendReadinessRule.ResolveComponentReadyIndex(
                    10,
                    5);

            int smoothReady =
                WaveTrendReadinessRule.ResolveSmoothReadyIndex(
                    componentReady,
                    WaveTrendMovingAverageCalculator.Exponential,
                    4);

            int signalReady =
                WaveTrendReadinessRule.ResolveSignalReadyIndex(
                    smoothReady,
                    WaveTrendMovingAverageCalculator.Simple,
                    5);

            Assert(
                componentReady == 14 &&
                smoothReady == 17 &&
                signalReady == 21 &&
                !WaveTrendReadinessRule.IsSnapshotReady(
                    21,
                    signalReady) &&
                WaveTrendReadinessRule.IsSnapshotReady(
                    22,
                    signalReady),
                "WaveTrend readiness includes RSI/MFI/RMI dependencies, both smoothing layers and previous-bar stability");

            WaveTrendMovingAverageCalculator stateCalculator =
                new WaveTrendMovingAverageCalculator(
                    WaveTrendMovingAverageCalculator.Exponential,
                    3,
                    0);

            Assert(
                double.IsNaN(
                    stateCalculator.GetWaveTrendAverageValue(0)),
                "WaveTrend calculator keeps pre-warm-up state non-ready");
        }

        private static void VerifyWaveTrendEvidence()
        {
            WaveTrendSnapshot bullCross =
                new WaveTrendSnapshot(
                    true,
                    8,
                    3,
                    5,
                    1,
                    4,
                    7,
                    true,
                    false,
                    true,
                    false,
                    false,
                    false,
                    true,
                    false);

            WaveTrendEvidenceResult bull =
                WaveTrendEvidenceRule.Evaluate(
                    bullCross,
                    58);

            Assert(
                bull.Bull &&
                bull.Direction == 1 &&
                bull.Quality >= 58,
                "WaveTrend bullish cross/rising state becomes bounded bullish evidence");

            WaveTrendSnapshot bearCross =
                new WaveTrendSnapshot(
                    true,
                    -8,
                    -3,
                    -5,
                    -1,
                    -4,
                    -7,
                    false,
                    true,
                    false,
                    true,
                    false,
                    false,
                    false,
                    true);

            WaveTrendEvidenceResult bear =
                WaveTrendEvidenceRule.Evaluate(
                    bearCross,
                    58);

            Assert(
                bear.Bear &&
                bear.Direction == -1 &&
                bear.Quality >= 58,
                "WaveTrend bearish cross/falling state becomes bounded bearish evidence");

            WaveTrendSnapshot weak =
                new WaveTrendSnapshot(
                    true,
                    1,
                    0.5,
                    0.5,
                    0.9,
                    0.5,
                    0.1,
                    false,
                    false,
                    true,
                    false,
                    false,
                    false,
                    true,
                    false);

            WaveTrendEvidenceResult weakResult =
                WaveTrendEvidenceRule.Evaluate(
                    weak,
                    70);

            Assert(
                !weakResult.Bull &&
                !weakResult.Bear,
                "weak WaveTrend movement cannot manufacture a strong evidence direction");
        }

        private static void VerifyParallelOpportunityRule()
        {
            TacticalOpportunityResult regular =
                TacticalOpportunityRule.Evaluate(
                    1,
                    92,
                    78,
                    4,
                    4,
                    1,
                    1,
                    55,
                    72,
                    1.90,
                    70,
                    1.75,
                    82,
                    2.20);

            Assert(
                regular.Allowed &&
                regular.Lane == OpportunityLane.Tactical &&
                regular.RiskReward >= 1.75,
                "aligned LTF opportunity can qualify without requiring strategic HTF calibration");

            TacticalOpportunityResult counterWeak =
                TacticalOpportunityRule.Evaluate(
                    1,
                    80,
                    75,
                    4,
                    4,
                    1,
                    -1,
                    84,
                    72,
                    2.25,
                    70,
                    1.75,
                    82,
                    2.20);

            Assert(
                !counterWeak.Allowed &&
                counterWeak.Lane == OpportunityLane.CounterHtfTactical,
                "counter-HTF opportunity needs the stricter tactical quality gate");

            TacticalOpportunityResult counterStrong =
                TacticalOpportunityRule.Evaluate(
                    1,
                    95,
                    86,
                    5,
                    5,
                    1,
                    -1,
                    84,
                    72,
                    2.35,
                    70,
                    1.75,
                    82,
                    2.20);

            Assert(
                counterStrong.Allowed &&
                counterStrong.Lane == OpportunityLane.CounterHtfTactical,
                "very strong LTF RR opportunity can survive a strong HTF conflict");
        }

        private static void VerifyProtectionProgressionSemantics()
        {
            Assert(
                ProtectionProgressionRule.ShouldAdvanceStop(
                    1,
                    100,
                    101) &&
                !ProtectionProgressionRule.ShouldAdvanceStop(
                    1,
                    101,
                    100),
                "BUY stop progression never moves backward");

            Assert(
                ProtectionProgressionRule.ShouldAdvanceStop(
                    -1,
                    100,
                    99) &&
                !ProtectionProgressionRule.ShouldAdvanceStop(
                    -1,
                    99,
                    100),
                "SELL stop progression never moves backward");

            Assert(
                ProtectionProgressionRule.ShouldAdvanceTarget(
                    1,
                    110,
                    120,
                    true) &&
                !ProtectionProgressionRule.ShouldAdvanceTarget(
                    1,
                    120,
                    110,
                    true) &&
                ProtectionProgressionRule.ShouldAdvanceTarget(
                    -1,
                    110,
                    100,
                    true) &&
                !ProtectionProgressionRule.ShouldAdvanceTarget(
                    -1,
                    100,
                    110,
                    true),
                "target progression preserves directional monotonicity");
        }

        private static void VerifyOrderBlockMathematics()
        {
            double low;
            double high;
            double remainingRatio;
            bool partial;
            OrderBlockLifecycleState lifecycleState;

            Assert(
                OrderBlockRule.IsOppositeSourceCandle(1, 101, 100) &&
                OrderBlockRule.IsOppositeSourceCandle(-1, 100, 101) &&
                !OrderBlockRule.IsOppositeSourceCandle(1, 100, 101),
                "Order Block source candle must be opposite to intended direction");

            Assert(
                OrderBlockRule.TryGetZone(
                    1, false, 101, 100, 103, 99,
                    out low, out high) &&
                low == 99 && high == 103,
                "wick-based Order Block zone preserves full source range");

            Assert(
                OrderBlockRule.TryGetZone(
                    1, true, 101, 100, 103, 99,
                    out low, out high) &&
                low == 100 && high == 101,
                "body-based Order Block zone uses source body");

            Assert(
                OrderBlockRule.MeetsDisplacement(
                    1, 100, 101, 2.0, 0.50) &&
                !OrderBlockRule.MeetsDisplacement(
                    1, 100, 100.99, 2.0, 0.50) &&
                OrderBlockRule.MeetsDisplacement(
                    -1, 101, 100, 2.0, 0.50),
                "displacement is directional and anchored to creation-bar ATR");

            Assert(
                OrderBlockRule.BreaksStructure(
                    1, 101.1, 100, 2.0, 0.50) &&
                !OrderBlockRule.BreaksStructure(
                    1, 101.0, 100, 2.0, 0.50) &&
                OrderBlockRule.BreaksStructure(
                    -1, 98.9, 100, 2.0, 0.50),
                "Order Block structure break uses explicit creation-ATR threshold");

            Assert(
                OrderBlockRule.GetMitigationProbe(
                    1, 100, 101, 103, 99, false) == 100 &&
                OrderBlockRule.GetMitigationProbe(
                    -1, 100, 101, 103, 99, true) == 103,
                "Order Block wick/body probe semantics are directional");

            Assert(
                OrderBlockRule.TryApplyOrderBlockPartialMitigation(
                    1, 99, 103, 101, 0.01,
                    out low, out high, out partial,
                    out remainingRatio,
                    out lifecycleState) &&
                partial &&
                lifecycleState ==
                    OrderBlockLifecycleState.Mitigated &&
                low == 99 && high == 101,
                "bullish Order Block partial mitigation moves upper boundary and marks the zone mitigated");

            Assert(
                OrderBlockRule.TryApplyOrderBlockPartialMitigation(
                    -1, 99, 103, 101, 0.01,
                    out low, out high, out partial,
                    out remainingRatio,
                    out lifecycleState) &&
                partial &&
                lifecycleState ==
                    OrderBlockLifecycleState.Mitigated &&
                low == 101 && high == 103,
                "bearish Order Block partial mitigation moves lower boundary and remains directionally symmetric");

            Assert(
                OrderBlockRule.ClassifyLifecycle(
                    false,
                    1.0) ==
                    OrderBlockLifecycleState.Fresh &&
                OrderBlockRule.ClassifyLifecycle(
                    true,
                    0.5) ==
                    OrderBlockLifecycleState.Mitigated &&
                OrderBlockRule.ClassifyLifecycle(
                    true,
                    OrderBlockRule.MinimumRetainedRatio) ==
                    OrderBlockLifecycleState.Broken,
                "Order Block lifecycle distinguishes fresh, mitigated and broken zones");

            Assert(
                OrderBlockRule.IsOnCorrectMarketSide(
                    1, 100, 95, 99, 0.01) &&
                !OrderBlockRule.IsOnCorrectMarketSide(
                    1, 100, 99, 101, 0.01) &&
                OrderBlockRule.IsOnCorrectMarketSide(
                    -1, 100, 101, 105, 0.01) &&
                !OrderBlockRule.IsOnCorrectMarketSide(
                    -1, 100, 99, 101, 0.01),
                "Order Block selection accepts only the intended side of market");

            Assert(
                OrderBlockRule.IsFullyMitigated(1, 99, 103, 99) &&
                OrderBlockRule.IsFullyMitigated(-1, 99, 103, 103) &&
                !OrderBlockRule.IsFullyMitigated(1, 99, 103, 99.01),
                "Order Block full-fill boundary is symmetric");

            Assert(
                !OrderBlockRule.TryApplyOrderBlockPartialMitigation(
                    1, 99, 103, 99, 0.01,
                    out low, out high, out partial,
                    out remainingRatio,
                    out lifecycleState) &&
                lifecycleState ==
                    OrderBlockLifecycleState.Broken &&
                !OrderBlockRule.TryApplyOrderBlockPartialMitigation(
                    -1, 99, 103, 103, 0.01,
                    out low, out high, out partial,
                    out remainingRatio,
                    out lifecycleState) &&
                lifecycleState ==
                    OrderBlockLifecycleState.Broken,
                "fully mitigated Order Blocks are explicitly broken and cannot remain active");

            Assert(
                OrderBlockRule.OrderBlockIdentity(1, 42, false) ==
                OrderBlockRule.OrderBlockIdentity(1, 42, false) &&
                OrderBlockRule.OrderBlockIdentity(1, 42, false) !=
                OrderBlockRule.OrderBlockIdentity(1, 42, true) &&
                OrderBlockRule.OrderBlockIdentity(1, 42, false) !=
                OrderBlockRule.OrderBlockIdentity(-1, 42, false),
                "Order Block identity separates direction and zone geometry variant");
        }

        private static void VerifyOrderBlockQualitySemantics()
        {
            int baseQuality =
                OrderBlockQualityRule.Calculate(
                    0.50,
                    0.00,
                    0.00,
                    0,
                    false,
                    false,
                    false,
                    false,
                    false);

            Assert(
                baseQuality == 54,
                "Order Block quality has one explicit base score");

            Assert(
                OrderBlockQualityRule.Calculate(
                    0.50, 0, 0, 0,
                    true, false, false, false, false) -
                baseQuality == 14 &&
                OrderBlockQualityRule.Calculate(
                    0.50, 0, 0, 0,
                    false, true, false, false, false) -
                baseQuality == 13 &&
                OrderBlockQualityRule.Calculate(
                    0.50, 0, 0, 0,
                    false, false, true, false, false) -
                baseQuality == 8 &&
                OrderBlockQualityRule.Calculate(
                    0.50, 0, 0, 0,
                    false, false, false, true, false) -
                baseQuality == 8,
                "displacement, structure break, liquidity sweep and FVG confluence are independently traceable");

            Assert(
                OrderBlockQualityRule.Calculate(
                    0.50, 0, 1.0, 0,
                    false, false, false, false, false) == 62 &&
                OrderBlockQualityRule.Calculate(
                    0.50, 1.0, 0, 0,
                    false, false, false, false, false) == 56 &&
                OrderBlockQualityRule.Calculate(
                    0.50, 1.50, 0, 0,
                    false, false, false, false, false) == 58,
                "remaining-width and impulse thresholds keep their explicit contributions");

            Assert(
                OrderBlockQualityRule.Calculate(
                    0.20, 0, 0, 0,
                    false, false, false, false, false) == 50 &&
                OrderBlockQualityRule.Calculate(
                    0.70, 0, 0, 0,
                    false, false, false, false, false) == 57 &&
                OrderBlockQualityRule.Calculate(
                    0.50, 0, 0, 6,
                    false, false, false, false, false) == 53 &&
                OrderBlockQualityRule.Calculate(
                    0.50, 0, 0.50, 0,
                    false, false, false, false, true) == 53,
                "body shape, age and partial-mitigation penalties are deterministic");

            Assert(
                OrderBlockQualityRule.Calculate(
                    0.70,
                    1.50,
                    1.0,
                    0,
                    true,
                    true,
                    true,
                    true,
                    false) == 100,
                "Order Block quality is explicitly clamped at the upper bound");

            Assert(
                OrderBlockQualityRule.Calculate(
                    double.NaN,
                    1.0,
                    1.0,
                    0,
                    false,
                    false,
                    false,
                    false,
                    false) == 0,
                "non-finite Order Block quality inputs fail closed");
        }

        private static void VerifyMtfContextIntegrity()
        {
            DateTime reference =
                new DateTime(
                    2026,
                    1,
                    1,
                    12,
                    5,
                    0,
                    DateTimeKind.Utc);

            MtfClosedContext context =
                new MtfClosedContext(
                    reference,
                    101,
                    605,
                    41,
                    31,
                    31,
                    30,
                    3,
                    1);

            Assert(
                context.Reference == reference,
                "MTF reference");

            Assert(
                context.M5 == 101 &&
                context.M1 == 605 &&
                context.M15 == 41 &&
                context.M30 == 31 &&
                context.H1 == 31 &&
                context.H4 == 30 &&
                context.D1 == 3 &&
                context.W1 == 1,
                "MTF indices preserved");

            Assert(
                context.HasPrimaryDecisionHistory,
                "primary MTF history");

            MtfClosedContext incomplete =
                new MtfClosedContext(
                    reference,
                    29,
                    605,
                    41,
                    21,
                    11,
                    8,
                    3,
                    1);

            Assert(
                !incomplete.HasPrimaryDecisionHistory,
                "insufficient closed history blocked");
        }

        private static void VerifyClosedBarReferenceContract()
        {
            DateTime[] opens =
            {
                Utc(12, 0),
                Utc(12, 5),
                Utc(12, 15),
                Utc(12, 20)
            };

            Assert(
                ClosedBarReferenceRule.ResolveClosedIndex(
                    opens.Length,
                    Utc(12, 4),
                    index => opens[index]) == -1,
                "no closed bar before first boundary");

            Assert(
                ClosedBarReferenceRule.ResolveClosedIndex(
                    opens.Length,
                    Utc(12, 5),
                    index => opens[index]) == 0,
                "exact boundary closes prior bar");

            Assert(
                ClosedBarReferenceRule.ResolveClosedIndex(
                    opens.Length,
                    Utc(12, 14),
                    index => opens[index]) == 0,
                "between boundaries keeps prior bar closed");

            Assert(
                ClosedBarReferenceRule.ResolveClosedIndex(
                    opens.Length,
                    Utc(12, 15),
                    index => opens[index]) == 1,
                "gap boundary uses next actual open time");

            Assert(
                ClosedBarReferenceRule.ResolveClosedIndex(
                    opens.Length,
                    Utc(12, 21),
                    index => opens[index]) == 2,
                "latest available fully closed bar");

            Assert(
                ClosedBarReferenceRule.IsFullyClosed(
                    opens.Length,
                    1,
                    Utc(12, 15),
                    index => opens[index]),
                "resolved bar is fully closed at reference");

            Assert(
                !ClosedBarReferenceRule.IsFullyClosed(
                    opens.Length,
                    2,
                    Utc(12, 19),
                    index => opens[index]),
                "future bar cannot be considered closed");

            Assert(
                ClosedBarReferenceRule.ResolveClosedIndex(
                    opens.Length,
                    Utc(12, 30),
                    index => opens[index]) == 2,
                "reference after last open is bounded to last closed bar");
        }

        private static DateTime Utc(int hour, int minute)
        {
            return new DateTime(
                2026,
                1,
                1,
                hour,
                minute,
                0,
                DateTimeKind.Utc);
        }

        private static void VerifyMarketExecutionAcceptance()
        {
            Assert(
                BrokerConfirmationPolicy.CanAdoptPosition(
                    true,
                    true,
                    true),
                "confirmed market position");

            Assert(
                !BrokerConfirmationPolicy.CanAdoptPosition(
                    true,
                    false,
                    true),
                "rejected market position");
        }

        private static void VerifyPendingOrderAcceptance()
        {
            Assert(
                BrokerConfirmationPolicy.CanAdoptPendingOrder(
                    true,
                    true,
                    true),
                "confirmed pending order");

            Assert(
                !BrokerConfirmationPolicy.CanAdoptPendingOrder(
                    true,
                    false,
                    true),
                "rejected pending order");
        }

        private static void VerifyRejectedMutationHandling()
        {
            Assert(
                !BrokerConfirmationPolicy.IsSuccessfulMutation(
                    false,
                    true),
                "missing mutation result");

            Assert(
                !BrokerConfirmationPolicy.IsSuccessfulMutation(
                    true,
                    false),
                "unsuccessful mutation");

            Assert(
                !BrokerConfirmationPolicy.CanAdoptPosition(
                    true,
                    true,
                    false),
                "missing confirmed position entity");

            Assert(
                !BrokerConfirmationPolicy.CanAdoptPendingOrder(
                    true,
                    true,
                    false),
                "missing confirmed pending entity");
        }

        private static void VerifyFillEnvelopeSymmetry()
        {
            Assert(
                ExecutionFillAcceptanceRule.IsAcceptable(
                    100,
                    102,
                    10,
                    0.25),
                "BUY-side fill inside envelope");

            Assert(
                ExecutionFillAcceptanceRule.IsAcceptable(
                    100,
                    98,
                    10,
                    0.25),
                "SELL-side mirrored fill inside envelope");

            Assert(
                !ExecutionFillAcceptanceRule.IsAcceptable(
                    100,
                    103,
                    10,
                    0.25),
                "BUY-side fill outside envelope");

            Assert(
                !ExecutionFillAcceptanceRule.IsAcceptable(
                    100,
                    97,
                    10,
                    0.25),
                "SELL-side mirrored fill outside envelope");
        }

        private static void VerifyInitialProtectionDirectionality()
        {
            Assert(
                PriceProtectionRule.ValidateStop(
                    1,
                    100,
                    98,
                    1),
                "BUY initial stop");

            Assert(
                PriceProtectionRule.ValidateTarget(
                    1,
                    100,
                    102,
                    1),
                "BUY initial target");

            Assert(
                PriceProtectionRule.ValidateStop(
                    -1,
                    100,
                    102,
                    1),
                "SELL initial stop");

            Assert(
                PriceProtectionRule.ValidateTarget(
                    -1,
                    100,
                    98,
                    1),
                "SELL initial target");
        }

        private static void VerifyManagedBreakEvenDirectionality()
        {
            Assert(
                ManagedStopProtectionRule.Validate(
                    1,
                    100,
                    104,
                    102,
                    1),
                "BUY profit-lock stop");

            Assert(
                !ManagedStopProtectionRule.Validate(
                    1,
                    100,
                    104,
                    106,
                    1),
                "BUY stop beyond market");

            Assert(
                ManagedStopProtectionRule.Validate(
                    -1,
                    100,
                    96,
                    98,
                    1),
                "SELL profit-lock stop");

            Assert(
                !ManagedStopProtectionRule.Validate(
                    -1,
                    100,
                    96,
                    94,
                    1),
                "SELL stop beyond market");
        }

        private static void VerifyProtectionProgression()
        {
            Assert(
                ProtectionProgressionRule.ShouldAdvanceStop(
                    1,
                    100,
                    101),
                "BUY SL advances upward");

            Assert(
                !ProtectionProgressionRule.ShouldAdvanceStop(
                    1,
                    101,
                    100),
                "BUY SL backward move blocked");

            Assert(
                ProtectionProgressionRule.ShouldAdvanceStop(
                    -1,
                    100,
                    99),
                "SELL SL advances downward");

            Assert(
                !ProtectionProgressionRule.ShouldAdvanceStop(
                    -1,
                    99,
                    100),
                "SELL SL backward move blocked");

            Assert(
                ProtectionProgressionRule.ShouldAdvanceTarget(
                    1,
                    105,
                    106,
                    true),
                "BUY TP advances forward");

            Assert(
                !ProtectionProgressionRule.ShouldAdvanceTarget(
                    1,
                    106,
                    105,
                    true),
                "BUY TP backward move blocked");

            Assert(
                ProtectionProgressionRule.ShouldAdvanceTarget(
                    -1,
                    95,
                    94,
                    true),
                "SELL TP advances forward");

            Assert(
                !ProtectionProgressionRule.ShouldAdvanceTarget(
                    -1,
                    94,
                    95,
                    true),
                "SELL TP backward move blocked");

            Assert(
                ProtectionProgressionRule.ShouldAdvanceTarget(
                    1,
                    106,
                    105,
                    false),
                "TP policy can explicitly allow backward move");
        }

        private static void VerifyTargetProgression()
        {
            Assert(
                TargetProgressionRule.IsValid(
                    1,
                    102,
                    105),
                "BUY target progression");

            Assert(
                TargetProgressionRule.IsValid(
                    -1,
                    98,
                    95),
                "SELL target progression");

            Assert(
                !TargetProgressionRule.IsValid(
                    1,
                    105,
                    102),
                "BUY backward target blocked");

            Assert(
                !TargetProgressionRule.IsValid(
                    -1,
                    95,
                    98),
                "SELL backward target blocked");
        }

        private static void VerifyExecutionCapacity()
        {
            Assert(
                ExecutionCapacityRule.IsSupportedSinglePlanCapacity(1),
                "single-plan capacity 1 is supported");

            Assert(
                !ExecutionCapacityRule.IsSupportedSinglePlanCapacity(2),
                "multi-position capacity is not supported");

            Assert(
                ExecutionCapacityRule.AllowsNewSinglePlan(
                    1,
                    false,
                    0,
                    0),
                "new single plan is allowed with empty broker capacity");

            Assert(
                !ExecutionCapacityRule.AllowsNewSinglePlan(
                    1,
                    true,
                    0,
                    0),
                "new plan is blocked by an existing local plan");

            Assert(
                !ExecutionCapacityRule.AllowsNewSinglePlan(
                    1,
                    false,
                    1,
                    0),
                "new plan is blocked by an existing managed position");

            Assert(
                !ExecutionCapacityRule.AllowsNewSinglePlan(
                    1,
                    false,
                    0,
                    1),
                "new plan is blocked by an existing managed pending order");

            Assert(
                ExecutionCapacityRule.AllowsNewSingleExecution(
                    1,
                    0,
                    0),
                "new broker execution is allowed with empty capacity");

            Assert(
                !ExecutionCapacityRule.AllowsNewSingleExecution(
                    1,
                    1,
                    0),
                "new broker execution is blocked by a managed position");

            Assert(
                !ExecutionCapacityRule.AllowsNewSingleExecution(
                    1,
                    0,
                    1),
                "new broker execution is blocked by a managed pending order");

            Assert(
                !ExecutionCapacityRule.AllowsNewSingleExecution(
                    2,
                    0,
                    0),
                "unsupported configured capacity blocks broker execution");
        }

        private static void VerifyStaleLivePlanRecovery()
        {
            Assert(
                LivePlanRecoveryRule.ShouldClearStaleLivePlan(
                    true,
                    false),
                "stale live plan clears when broker position disappears");

            Assert(
                !LivePlanRecoveryRule.ShouldClearStaleLivePlan(
                    true,
                    true),
                "live plan remains when broker position exists");

            Assert(
                !LivePlanRecoveryRule.ShouldClearStaleLivePlan(
                    false,
                    false),
                "non-live plan is not cleared by broker absence");
        }

        private static void VerifyLifecycleFlows()
        {
            Assert(
                LifecycleTransitionPolicy.IsAllowed(
                    LifecycleState.Flat,
                    LifecycleState.Signal),
                "signal entry");

            Assert(
                LifecycleTransitionPolicy.IsAllowed(
                    LifecycleState.Signal,
                    LifecycleState.PlanReady),
                "plan readiness");

            Assert(
                LifecycleTransitionPolicy.IsAllowed(
                    LifecycleState.PlanReady,
                    LifecycleState.ExecutionReady),
                "execution readiness");

            Assert(
                LifecycleTransitionPolicy.IsAllowed(
                    LifecycleState.ExecutionReady,
                    LifecycleState.LivePosition),
                "market fill flow");

            Assert(
                LifecycleTransitionPolicy.IsAllowed(
                    LifecycleState.ExecutionReady,
                    LifecycleState.PendingOrder),
                "pending placement flow");

            Assert(
                LifecycleTransitionPolicy.IsAllowed(
                    LifecycleState.PendingOrder,
                    LifecycleState.LivePosition),
                "pending fill flow");

            Assert(
                LifecycleTransitionPolicy.IsAllowed(
                    LifecycleState.LivePosition,
                    LifecycleState.RecoveryRequired),
                "live recovery flow");

            Assert(
                LifecycleTransitionPolicy.IsAllowed(
                    LifecycleState.RecoveryRequired,
                    LifecycleState.LivePosition),
                "recovery completion flow");

            Assert(
                LifecycleTransitionPolicy.IsAllowed(
                    LifecycleState.LivePosition,
                    LifecycleState.ExitRequested),
                "reversal/invalidation exit");

            Assert(
                LifecycleTransitionPolicy.IsAllowed(
                    LifecycleState.ExitRequested,
                    LifecycleState.Closed),
                "end-of-day/exit close");

            Assert(
                !LifecycleTransitionPolicy.IsAllowed(
                    LifecycleState.Closed,
                    LifecycleState.PendingOrder),
                "closed state cannot create pending order");
        }

        private static void VerifyRuntimeStageIsolation()
        {
            string cyclePath =
                Path.Combine(
                    "src",
                    "CFIP.Indicator",
                    "Runtime",
                    "Calculation",
                    "CalculationCycle.cs");

            string stagesPath =
                Path.Combine(
                    "src",
                    "CFIP.Indicator",
                    "Runtime",
                    "Calculation",
                    "CalculationStageIsolation.cs");

            Assert(
                File.Exists(cyclePath),
                "calculation cycle source exists");

            Assert(
                File.Exists(stagesPath),
                "calculation stage isolation source exists");

            string cycle =
                File.ReadAllText(
                    cyclePath);

            string stages =
                File.ReadAllText(
                    stagesPath);

            foreach (string requiredCall in new[]
            {
                "RunCalculationPreparationStage(",
                "RunClosedBarAnalysisStage(",
                "ProcessLiveCalculationStages("
            })
            {
                Assert(
                    cycle.Contains(requiredCall),
                    "Calculate uses " + requiredCall.Trim('(', ' '));
            }

            Assert(
                !cycle.Contains("ProcessNewClosedBar("),
                "Calculate does not directly own closed-bar analysis");

            Assert(
                !cycle.Contains("ProcessLiveCalculation("),
                "Calculate does not directly own live-cycle orchestration");

            foreach (string requiredStage in new[]
            {
                "BROKER RECONCILIATION • PREFLIGHT",
                "BROKER LIFECYCLE RECOVERY",
                "BROKER RECONCILIATION • POST-RECOVERY",
                "ACTIVE PLAN MANAGEMENT",
                "BROKER PROTECTION • PRE-ANALYSIS",
                "LIVE ANALYSIS",
                "PLAN SYNCHRONIZATION",
                "PLAN CREATION",
                "EXECUTION",
                "BROKER RECONCILIATION • POST-EXECUTION",
                "BROKER PROTECTION • POST-EXECUTION",
                "TELEMETRY",
                "REVERSAL MANAGEMENT",
                "BROKER STATE FINALIZATION",
                "PRESENTATION"
            })
            {
                Assert(
                    stages.Contains(requiredStage),
                    "isolated stage " + requiredStage);
            }

            Assert(
                stages.Contains("HandleRuntimeFault("),
                "stage fault containment");

            Assert(
                stages.Contains("return true;"),
                "stage fault continues to next stage");

            Assert(
                stages.IndexOf(
                    "BROKER RECONCILIATION • PREFLIGHT",
                    StringComparison.Ordinal) <
                stages.IndexOf(
                    "LIVE ANALYSIS",
                    StringComparison.Ordinal),
                "broker reconciliation precedes live analysis");

            Assert(
                stages.IndexOf(
                    "BROKER LIFECYCLE RECOVERY",
                    StringComparison.Ordinal) <
                stages.IndexOf(
                    "LIVE ANALYSIS",
                    StringComparison.Ordinal),
                "broker recovery precedes live analysis");

            Assert(
                stages.IndexOf(
                    "ACTIVE PLAN MANAGEMENT",
                    StringComparison.Ordinal) <
                stages.IndexOf(
                    "LIVE ANALYSIS",
                    StringComparison.Ordinal),
                "active plan management precedes live analysis");

            Assert(
                stages.IndexOf(
                    "BROKER PROTECTION • PRE-ANALYSIS",
                    StringComparison.Ordinal) <
                stages.IndexOf(
                    "LIVE ANALYSIS",
                    StringComparison.Ordinal),
                "pre-analysis protection precedes live analysis");

            Assert(
                stages.IndexOf(
                    "LIVE ANALYSIS",
                    StringComparison.Ordinal) <
                stages.IndexOf(
                    "PLAN SYNCHRONIZATION",
                    StringComparison.Ordinal),
                "analysis stage precedes downstream planning");

            Assert(
                stages.IndexOf(
                    "PLAN SYNCHRONIZATION",
                    StringComparison.Ordinal) <
                stages.IndexOf(
                    "EXECUTION",
                    StringComparison.Ordinal),
                "planning synchronization precedes execution");

            Assert(
                stages.IndexOf(
                    "BROKER RECONCILIATION • POST-EXECUTION",
                    StringComparison.Ordinal) <
                stages.IndexOf(
                    "BROKER PROTECTION • POST-EXECUTION",
                    StringComparison.Ordinal),
                "post-execution reconciliation precedes protection");

            Assert(
                stages.IndexOf(
                    "BROKER PROTECTION • POST-EXECUTION",
                    StringComparison.Ordinal) <
                stages.IndexOf(
                    "TELEMETRY",
                    StringComparison.Ordinal),
                "post-execution protection precedes telemetry");

            Assert(
                stages.IndexOf(
                    "BROKER STATE FINALIZATION",
                    StringComparison.Ordinal) <
                stages.IndexOf(
                    "PRESENTATION",
                    StringComparison.Ordinal),
                "final broker state synchronization precedes presentation");
        }

        private static void VerifyLifecycleIdempotency()
        {
            LifecycleEventIdempotencyGuard guard =
                new LifecycleEventIdempotencyGuard();

            Assert(
                guard.TryBegin("POSITION_OPENED", 501),
                "first open event");

            Assert(
                !guard.TryBegin("POSITION_OPENED", 501),
                "duplicate open event");

            Assert(
                guard.TryBegin("PENDING_CREATED", 501),
                "different event type");

            Assert(
                guard.TryBegin("POSITION_OPENED", 502),
                "different entity");

            Assert(
                !guard.TryBegin("POSITION_OPENED", 0),
                "invalid entity");
        }

        private static void VerifyRuntimeFaultStateMachine()
        {
            RuntimeFaultStateMachine machine =
                new RuntimeFaultStateMachine();

            machine.BeginCycle();
            machine.ObserveAutoTradingSetting(true);

            Assert(
                machine.State == RuntimeFaultState.Healthy,
                "initial runtime state healthy");

            Assert(
                machine.CanAutomaticEntryProceed,
                "initial automatic entry armed");

            machine.RecordRecoverableFault();

            Assert(
                machine.State == RuntimeFaultState.Degraded,
                "recoverable fault enters degraded");

            machine.BlockAutomaticEntry();

            Assert(
                machine.State == RuntimeFaultState.EntryBlocked,
                "fault blocks automatic entry");

            Assert(
                !machine.CanAutomaticEntryProceed,
                "entry remains blocked after fault");

            machine.BeginCycle();
            machine.MarkManagementReadyForRecovery();

            Assert(
                machine.State == RuntimeFaultState.Recovering,
                "healthy management enters recovery");

            Assert(
                !machine.CanAutomaticEntryProceed,
                "recovery does not re-arm entry");

            machine.CompleteCycle();

            Assert(
                machine.State == RuntimeFaultState.Healthy,
                "clean recovery returns healthy");

            Assert(
                !machine.CanAutomaticEntryProceed,
                "healthy recovery remains disarmed");

            machine.ObserveAutoTradingSetting(false);
            machine.ObserveAutoTradingSetting(true);

            Assert(
                machine.CanAutomaticEntryProceed,
                "explicit enable transition re-arms entry");

            machine.BeginCycle();
            machine.RecordRecoverableFault();
            machine.BlockAutomaticEntry();

            machine.ObserveAutoTradingSetting(false);
            machine.ObserveAutoTradingSetting(true);

            Assert(
                !machine.CanAutomaticEntryProceed,
                "enable transition cannot bypass blocked state");
        }

        private static void VerifyClosedBarRetryPolicy()
        {
            RuntimeFaultStateMachine machine =
                new RuntimeFaultStateMachine();

            DateTime t =
                new DateTime(
                    2026,
                    1,
                    1,
                    12,
                    0,
                    0,
                    DateTimeKind.Utc);

            string reason;

            Assert(
                machine.CanAttemptClosedBarAnalysis(
                    101,
                    t,
                    out reason),
                "first closed-bar attempt allowed");

            machine.RecordClosedBarAnalysisFailure(
                101,
                t);

            Assert(
                machine.ClosedBarFailureCount == 1,
                "first closed-bar failure counted");

            Assert(
                machine.ClosedBarLastFailureUtc == t,
                "failure timestamp recorded");

            Assert(
                !machine.CanAttemptClosedBarAnalysis(
                    101,
                    t.AddMilliseconds(500),
                    out reason),
                "first closed-bar retry is backed off");

            Assert(
                machine.CanAttemptClosedBarAnalysis(
                    101,
                    t.AddSeconds(1),
                    out reason),
                "first closed-bar retry becomes eligible");

            machine.RecordClosedBarAnalysisFailure(
                101,
                t.AddSeconds(1));

            Assert(
                machine.ClosedBarFailureCount == 2,
                "second closed-bar failure counted");

            Assert(
                !machine.CanAttemptClosedBarAnalysis(
                    101,
                    t.AddSeconds(2),
                    out reason),
                "second retry uses longer backoff");

            Assert(
                machine.CanAttemptClosedBarAnalysis(
                    102,
                    t.AddSeconds(2),
                    out reason),
                "new closed bar is not blocked by prior bar failure");

            Assert(
                machine.HasStaleClosedBarFailure(102),
                "prior closed-bar failure is detectable as stale");

            machine.RecordClosedBarAnalysisFailure(
                101,
                t.AddSeconds(3));

            machine.RecordClosedBarAnalysisFailure(
                101,
                t.AddSeconds(7));

            Assert(
                machine.ClosedBarFailureCount == 4,
                "fourth closed-bar failure counted");

            Assert(
                !machine.CanAttemptClosedBarAnalysis(
                    101,
                    t.AddSeconds(8),
                    out reason),
                "repeated closed-bar failure opens retry circuit");

            Assert(
                machine.CanAttemptClosedBarAnalysis(
                    102,
                    t.AddSeconds(8),
                    out reason),
                "new closed bar bypasses stale retry circuit");

            machine.RecordClosedBarAnalysisSuccess(101);

            Assert(
                machine.ClosedBarFailureCount == 0,
                "successful retry clears failure state");

            Assert(
                machine.ClosedBarLastFailureUtc == DateTime.MinValue,
                "successful retry clears failure timestamp");
        }

        private static void VerifyUnifiedSubmissionGate()
        {
            SubmissionGate gate =
                new SubmissionGate();

            DateTime t =
                new DateTime(
                    2026,
                    1,
                    1,
                    12,
                    0,
                    0,
                    DateTimeKind.Utc);

            SubmissionAttemptIdentity firstSignal =
                new SubmissionAttemptIdentity(
                    "EURUSD|100|1",
                    "AutomaticMarket|100|1",
                    ExecutionSubmissionPath.AutomaticMarket);

            SubmissionAttemptIdentity secondSignal =
                new SubmissionAttemptIdentity(
                    "EURUSD|101|1",
                    "AutomaticMarket|101|1",
                    ExecutionSubmissionPath.AutomaticMarket);

            SubmissionAttemptIdentity otherPath =
                new SubmissionAttemptIdentity(
                    "EURUSD|100|1",
                    "PendingStop|100|1",
                    ExecutionSubmissionPath.PendingStop);

            string reason;

            Assert(
                gate.TryAcquire(
                    firstSignal,
                    t,
                    out reason),
                "first submission attempt allowed");

            gate.Record(
                firstSignal,
                t,
                false);

            Assert(
                !gate.TryAcquire(
                    firstSignal,
                    t.AddMilliseconds(500),
                    out reason),
                "failed signal enters backoff");

            Assert(
                gate.TryAcquire(
                    secondSignal,
                    t.AddMilliseconds(500),
                    out reason),
                "different signal remains independently eligible");

            Assert(
                gate.TryAcquire(
                    otherPath,
                    t.AddMilliseconds(500),
                    out reason),
                "different execution path remains independently eligible");

            Assert(
                gate.TryAcquire(
                    firstSignal,
                    t.AddSeconds(1),
                    out reason),
                "first retry becomes eligible after backoff");

            for (int i = 0; i < 3; i++)
            {
                gate.Record(
                    firstSignal,
                    t.AddSeconds(2 + i * 2),
                    false);
            }

            Assert(
                !gate.TryAcquire(
                    firstSignal,
                    t.AddSeconds(10),
                    out reason),
                "repeated same-attempt failures open circuit");

            Assert(
                gate.TryAcquire(
                    secondSignal,
                    t.AddSeconds(10),
                    out reason),
                "open circuit is isolated to the failing attempt");

            gate.Record(
                firstSignal,
                t.AddSeconds(70),
                true);

            Assert(
                gate.TryAcquire(
                    firstSignal,
                    t.AddSeconds(70),
                    out reason),
                "successful submission clears retry state");

            Assert(
                firstSignal.CanonicalKey !=
                secondSignal.CanonicalKey &&
                firstSignal.CanonicalKey !=
                otherPath.CanonicalKey,
                "submission identities remain distinct");
        }
        private static void VerifyVisualAndExecutionControls()
        {
            string snapshotPath = Path.Combine("src", "CFIP.Indicator", "UI", "Chart", "SignalVisualSnapshot.cs");
            string previewPath = Path.Combine("src", "CFIP.Indicator", "Core", "Models", "TradeSetupPreview.cs");
            string previewBuilderPath = Path.Combine("src", "CFIP.Indicator", "Planning", "TradePlan", "PlanPreviewBuilder.cs");
            string calculationPath = Path.Combine("src", "CFIP.Indicator", "Runtime", "Calculation", "CalculationLiveCycle.cs");
            string calculationStagePath = Path.Combine("src", "CFIP.Indicator", "Runtime", "Calculation", "CalculationStageIsolation.cs");
            string rendererPath = Path.Combine("src", "CFIP.Indicator", "UI", "Chart", "PlanRenderCoordinator.cs");
            string pendingRendererPath = Path.Combine("src", "CFIP.Indicator", "UI", "Chart", "PendingOrderRenderer.cs");
            string alertRendererPath = Path.Combine("src", "CFIP.Indicator", "UI", "Chart", "AlertSignalRenderer.cs");
            string alertEnginePath = Path.Combine("src", "CFIP.Indicator", "Trading", "Alerts", "AlertEngine.cs");
            string statePath = Path.Combine("src", "CFIP.Indicator", "Indicator", "State.cs");
            string predictiveSelectorPath = Path.Combine("src", "CFIP.Indicator", "Planning", "Execution", "PredictivePendingLevelSelector.cs");
            string predictiveCollectorPath = Path.Combine("src", "CFIP.Indicator", "Planning", "Execution", "PredictivePendingZoneCollector.cs");
            string predictiveScorerPath = Path.Combine("src", "CFIP.Indicator", "Planning", "Execution", "PredictivePendingCandidateScorer.cs");
            string reversalLimitPath = Path.Combine("src", "CFIP.Indicator", "Trading", "Pending", "Placement", "ReversalLimitPreparation.cs");
            string controlFactoryPath = Path.Combine("src", "CFIP.Indicator", "UI", "Controls", "ExecutionControlsFactory.cs");
            string controlHandlersPath = Path.Combine("src", "CFIP.Indicator", "UI", "Controls", "ExecutionToggleHandlers.cs");
            string controlSyncPath = Path.Combine("src", "CFIP.Indicator", "UI", "Controls", "ExecutionControlsSynchronizer.cs");

            Assert(
                File.Exists(snapshotPath) &&
                File.Exists(previewPath) &&
                File.Exists(previewBuilderPath) &&
                File.Exists(calculationPath) &&
                File.Exists(calculationStagePath) &&
                File.Exists(rendererPath) &&
                File.Exists(pendingRendererPath) &&
                File.Exists(alertRendererPath) &&
                File.Exists(alertEnginePath) &&
                File.Exists(statePath) &&
                File.Exists(predictiveSelectorPath) &&
                File.Exists(predictiveCollectorPath) &&
                File.Exists(predictiveScorerPath) &&
                File.Exists(reversalLimitPath) &&
                File.Exists(controlFactoryPath) &&
                File.Exists(controlHandlersPath) &&
                File.Exists(controlSyncPath),
                "visual/control sources exist");

            string snapshot = File.ReadAllText(snapshotPath);
            string preview = File.ReadAllText(previewPath);
            string previewBuilder = File.ReadAllText(previewBuilderPath);
            string calculation = File.ReadAllText(calculationPath);
            string calculationStage = File.ReadAllText(calculationStagePath);
            string renderer = File.ReadAllText(rendererPath);
            string pendingRenderer = File.ReadAllText(pendingRendererPath);
            string alertRenderer = File.ReadAllText(alertRendererPath);
            string alertEngine = File.ReadAllText(alertEnginePath);
            string state = File.ReadAllText(statePath);
            string predictiveSelector = File.ReadAllText(predictiveSelectorPath);
            string predictiveCollector = File.ReadAllText(predictiveCollectorPath);
            string predictiveScorer = File.ReadAllText(predictiveScorerPath);
            string reversalLimit = File.ReadAllText(reversalLimitPath);
            string controlFactory = File.ReadAllText(controlFactoryPath);
            string controlHandlers = File.ReadAllText(controlHandlersPath);
            string controlSync = File.ReadAllText(controlSyncPath);

            Assert(
                snapshot.Contains("SetupPreviewActive") &&
                snapshot.Contains("SetupEntry") &&
                snapshot.Contains("SetupStop") &&
                snapshot.Contains("SetupTp1"),
                "canonical snapshot carries setup levels");

            Assert(
                preview.Contains("never consumed by broker submission"),
                "visual preview is non-executable");

            Assert(
                previewBuilder.Contains("BuildStructuralStop(") &&
                previewBuilder.Contains("BuildTargetLevels(") &&
                previewBuilder.Contains("SelectTargets("),
                "visual preview reuses planning authorities");

            Assert(
                calculation.Contains("RenderSetupPreview("),
                "calculation renders setup preview");

            string visualSnapshotBuilderPath =
                Path.Combine("src", "CFIP.Indicator", "UI", "Chart", "SignalVisualSnapshotBuilder.cs");
            string visualSnapshotBuilder =
                File.ReadAllText(visualSnapshotBuilderPath);

            Assert(
                visualSnapshotBuilder.Contains("_setupPreview.Direction != 0") &&
                visualSnapshotBuilder.Contains("The setup preview is the pre-trigger structural forecast") &&
                !visualSnapshotBuilder.Contains("_setupPreview.Direction == visualDirection"),
                "setup preview remains visible before TriggerReady and does not wait for post-trigger visual direction");

            string planEligibilityPath =
                Path.Combine("src", "CFIP.Indicator", "Trading", "Validation", "PlanCreationEligibility.cs");
            string planEligibility =
                File.ReadAllText(planEligibilityPath);

            Assert(
                planEligibility.Contains("if (!_decision.TriggerReady)") &&
                planEligibility.Contains("return false;"),
                "execution plan creation remains TriggerReady-gated even while preview is visible");

            Assert(
                !File.ReadAllText(
                    Path.Combine(
                        "src",
                        "CFIP.Indicator",
                        "Indicator",
                        "Parameters",
                        "15_control_advanced.cs"))
                    .Contains("EnableDynamicSlTrail"),
                "semantic duplicate structural-stop alias is removed");

            Assert(
                renderer.Contains("RenderLevelLines("),
                "plan renderer owns shared level rendering");

            string lineRendererPath =
                Path.Combine("src", "CFIP.Indicator", "UI", "Chart", "PlanLineRenderer.cs");
            string labelCoordinatorPath =
                Path.Combine("src", "CFIP.Indicator", "UI", "Chart", "PlanLabelRenderCoordinator.cs");
            string lineRenderer =
                File.ReadAllText(lineRendererPath);
            string labelCoordinator =
                File.ReadAllText(labelCoordinatorPath);

            Assert(
                lineRenderer.Contains("GetPlanLineRightBar()") &&
                lineRenderer.Contains("return Bars.Count - 1") &&
                lineRenderer.Contains("CompactPlanLineLengthBars = 40") &&
                !lineRenderer.Contains("MapM5ToChart(") &&
                !lineRenderer.Contains("anchorM5"),
                "plan levels terminate at the latest chart candle without stale M5 anchoring");

            Assert(
                labelCoordinator.Contains("GetPlanLineLeftBar(") &&
                !labelCoordinator.Contains("GetCompactPlanLineLeftBar("),
                "plan labels reuse the canonical line left edge");

            Assert(
                controlFactory.Contains("CreateExecutionToggle(") &&
                controlFactory.Contains("_autoTradingQuickToggle.Click +=") &&
                controlFactory.Contains("_automaticOrdersQuickToggle.Click +="),
                "execution controls are interactive canonical toggle surfaces");

            Assert(
                controlHandlers.Contains("ApplyAutoTradingQuickToggleClick(") &&
                controlHandlers.Contains("ApplyAutomaticOrdersQuickToggleClick(") &&
                controlHandlers.Contains("SetAutoTradingRuntimeState(") &&
                controlHandlers.Contains("SetAutomaticOrdersRuntimeState("),
                "execution UI controls are bound to canonical runtime setters");

            Assert(
                controlSync.Contains("_executionToggleSyncing = true"),
                "execution control synchronization is guarded against operator-event recursion");

            int pendingExecution =
                calculationStage.IndexOf(
                    "PREDICTIVE PENDING EXECUTION",
                    StringComparison.Ordinal);
            int aggressiveExecution =
                calculationStage.IndexOf(
                    "AGGRESSIVE AUTO EXECUTION",
                    StringComparison.Ordinal);
            int planCreation =
                calculationStage.IndexOf(
                    "PLAN CREATION",
                    StringComparison.Ordinal);
            int marketExecution =
                calculationStage.IndexOf(
                    "AUTOMATIC MARKET EXECUTION",
                    StringComparison.Ordinal);

            Assert(
                pendingExecution >= 0 &&
                aggressiveExecution > pendingExecution &&
                planCreation > aggressiveExecution &&
                marketExecution > planCreation,
                "execution priority is predictive pending -> aggressive -> plan -> market");

            Assert(
                calculationStage.Contains("TrySmartPendingOrders(") &&
                calculationStage.Contains("TryAggressiveAutoTrade(") &&
                calculationStage.Contains("TryAutoTrade("),
                "all automatic execution paths remain connected");

            Assert(
                calculation.Contains("TryEnsureAutomaticPlan(") &&
                calculation.Contains("RenderPredictionObjects(") &&
                calculation.Contains("RenderLatestAlertSignalMarker("),
                "live cycle retains plan plus prediction/alert presentation orchestration");

            Assert(
                predictiveSelector.Contains("TrySelectPredictivePendingLevel(") &&
                predictiveSelector.Contains("CollectPredictiveZoneCandidates(") &&
                predictiveSelector.Contains("FindEqualLow(") &&
                predictiveSelector.Contains("FindEqualHigh(") &&
                predictiveCollector.Contains("BuildManagedFvgZone(") &&
                predictiveCollector.Contains("BuildOrderBlockCandidate(") &&
                predictiveScorer.Contains("PredictivePendingContextQuality(") &&
                predictiveScorer.Contains("PredictivePendingSourceKey("),
                "predictive pending selector combines structural level sources");

            Assert(
                reversalLimit.Contains("TrySelectPredictivePendingLevel(") &&
                reversalLimit.Contains("candidate.Source") &&
                reversalLimit.Contains("candidate.DistanceAtr"),
                "reversal limit consumes predictive candidate evidence");

            Assert(
                pendingRenderer.Contains("RenderCompactPlanLabel(") &&
                pendingRenderer.Contains("RemovePlanLabel("),
                "pending levels render through compact semantic boxes");

            Assert(
                alertEngine.Contains("RememberVisualSignalAlert(") &&
                state.Contains("_lastVisualAlertM5") &&
                state.Contains("_lastVisualAlertDirection") &&
                alertRenderer.Contains("RenderLatestAlertSignalMarker("),
                "audible signal alert has a non-authoritative visual presentation path");

            string protectionPath = Path.Combine(
                "src", "CFIP.Indicator", "Trading", "LiveManagement", "ProtectionManager.cs");
            string protection = File.ReadAllText(protectionPath);
            Assert(
                !protection.Contains("pressureStop") &&
                !protection.Contains("market - tightRoom") &&
                !protection.Contains("market + tightRoom") &&
                protection.Contains("pressureTighten"),
                "smart trailing remains structural under exit pressure");
        }

        private static void VerifyResponsivePanelRuntime()
        {
            string heartbeatPath = Path.Combine(
                "src", "CFIP.Indicator", "Runtime", "Supervision", "RuntimePanelHeartbeat.cs");
            string initPath = Path.Combine(
                "src", "CFIP.Indicator", "Runtime", "Initialization", "RuntimeInitialization.cs");
            string panelPath = Path.Combine(
                "src", "CFIP.Indicator", "UI", "Panel", "PanelMainRenderer.cs");
            string rowsPath = Path.Combine(
                "src", "CFIP.Indicator", "UI", "Panel", "PanelRowsFactory.cs");
            string writerPath = Path.Combine(
                "src", "CFIP.Indicator", "UI", "Panel", "PanelRowWriter.cs");
            string optimizationPath = Path.Combine(
                "src", "CFIP.Indicator", "UI", "Panel", "PanelRenderOptimization.cs");
            string statePath = Path.Combine(
                "src", "CFIP.Indicator", "Indicator", "State.cs");

            Assert(
                File.Exists(heartbeatPath) &&
                File.Exists(initPath) &&
                File.Exists(panelPath) &&
                File.Exists(rowsPath) &&
                File.Exists(writerPath) &&
                File.Exists(optimizationPath) &&
                File.Exists(statePath),
                "responsive panel sources exist");

            string heartbeat = File.ReadAllText(heartbeatPath);
            string init = File.ReadAllText(initPath);
            string panel = File.ReadAllText(panelPath);
            string rows = File.ReadAllText(rowsPath);
            string writer = File.ReadAllText(writerPath);
            string optimization = File.ReadAllText(optimizationPath);
            string state = File.ReadAllText(statePath);

            Assert(
                !heartbeat.Contains("RenderPanel();") &&
                heartbeat.Contains("UpdatePanelHeartbeatRows(") &&
                heartbeat.Contains("UpdatePanelHeartbeatLiveRows("),
                "heartbeat stays lightweight and refreshes live rows without full panel layout");

            Assert(
                heartbeat.Contains("TotalMilliseconds >= 1000"),
                "safety supervisor remains one-second bounded");

            Assert(
                init.Contains("TimeSpan.FromMilliseconds(500)"),
                "ready timer uses responsive panel cadence");

            Assert(
                init.Contains("TimeSpan.FromMilliseconds(250)"),
                "initialization polling avoids 100ms timer churn");

            Assert(
                init.Contains("_status =") &&
                init.Contains("RenderPanel();"),
                "initialization status reaches the panel");

            Assert(
                panel.Contains("BuildSignalVisualSnapshot(") &&
                panel.Contains("ShouldRenderFullPanel(") &&
                optimization.Contains("BuildPanelPresentationKey(") &&
                state.Contains("_lastPanelPresentationKey"),
                "panel uses one canonical snapshot and state-change-driven full render");

            Assert(
                rows.Contains("EnsurePanelRow("),
                "panel row allocation is lazy");

            Assert(
                writer.Contains("if (row.Text != nextText)"),
                "panel writer skips duplicate text writes");
        }

        private static void VerifyTradePlanRegistry()
        {
            TradePlanRegistry registry =
                new TradePlanRegistry();

            TradeOpportunityCandidate buy =
                new TradeOpportunityCandidate
                {
                    Id = "TACTICAL_BUY",
                    Lane = OpportunityLane.Tactical,
                    Direction = 1,
                    Quality = 82,
                    ActionableNow = true
                };

            TradeOpportunityCandidate sell =
                new TradeOpportunityCandidate
                {
                    Id = "TACTICAL_SELL",
                    Lane = OpportunityLane.Tactical,
                    Direction = -1,
                    Quality = 79,
                    ActionableNow = false,
                    ActionabilityReason = "RR BELOW ACTIONABLE FLOOR"
                };

            registry.Upsert(buy);
            registry.Upsert(sell);

            TradeOpportunityCandidate actual;
            Assert(registry.Count == 2, "multi-plan registry retains independent lanes");
            Assert(registry.Contains("TACTICAL_BUY"), "registry BUY identity");
            Assert(registry.Contains("TACTICAL_SELL"), "registry SELL identity");
            Assert(registry.TryGetCandidate("TACTICAL_BUY", out actual) &&
                   actual.ActionableNow,
                   "registry preserves candidate actionability");

            registry.Remove("TACTICAL_BUY");
            Assert(registry.Count == 1, "registry removal");
        }

        private static void VerifyScenarioExecutionPolicy()
        {
            Decision decision =
                new Decision
                {
                    Direction = 1,
                    EntryAllowed = true,
                    TriggerReady = true,
                    ActionableNow = true
                };

            TradeOpportunityCandidate strategic =
                new TradeOpportunityCandidate
                {
                    ScenarioId = "CANONICAL-HTF-BUY",
                    Lane = OpportunityLane.Strategic,
                    Direction = 1,
                    CreatedM5 = 100,
                    Quality = 90,
                    Tp1RR = 3.0,
                    ActionableNow = true,
                    ExecutionPolicyAllowed = true
                };

            TradeOpportunityCandidate tactical =
                new TradeOpportunityCandidate
                {
                    ScenarioId = "CANONICAL-TACTICAL-BUY",
                    Lane = OpportunityLane.Tactical,
                    Direction = 1,
                    CreatedM5 = 100,
                    Quality = 84,
                    Tp1RR = 2.5,
                    ActionableNow = true,
                    ExecutionPolicyAllowed = true
                };

            TradeOpportunityCandidate independent =
                new TradeOpportunityCandidate
                {
                    ScenarioId = "TF-H1-BUY",
                    SourceTimeframe = "H1",
                    Lane = OpportunityLane.Tactical,
                    Direction = 1,
                    CreatedM5 = 100,
                    Quality = 96,
                    Tp1RR = 4.0,
                    ActionableNow = true,
                    ExecutionPolicyAllowed = false,
                    ExecutionPolicyReason =
                        "INDEPENDENT TIMEFRAME • OBSERVE ONLY"
                };

            string reason;

            Assert(
                ScenarioExecutionPolicy.IsCanonicalCandidateEligible(
                    strategic,
                    decision,
                    OpportunityLane.Strategic,
                    out reason),
                "canonical strategic scenario may be execution-eligible");

            Assert(
                ScenarioExecutionPolicy.IsCanonicalCandidateEligible(
                    independent,
                    decision,
                    OpportunityLane.Tactical,
                    out reason),
                "independent timeframe candidate can still be structurally evaluated");

            Assert(
                !ScenarioExecutionPolicy.IsExecutionAuthorizedCandidate(
                    independent,
                    decision,
                    OpportunityLane.Tactical,
                    out reason),
                "independent timeframe scenario cannot authorize broker execution");

            Plan plan =
                new Plan
                {
                    Direction = 1,
                    Lane = OpportunityLane.Tactical,
                    CreatedM5 = 100,
                    Entry = 100,
                    Stop = 98,
                    Tp1 = 104
                };

            tactical.Entry = 100;
            tactical.Stop = 98;
            tactical.Tp1 = 104;

            strategic.Entry = 101;
            strategic.Stop = 98;
            strategic.Tp1 = 105;

            TradeOpportunityCandidate selected;

            Assert(
                ScenarioExecutionPolicy.TryResolvePlanScenario(
                    new[]
                    {
                        strategic,
                        tactical,
                        independent
                    },
                    plan,
                    decision,
                    0.001,
                    out selected,
                    out reason) &&
                selected == tactical,
                "plan scenario resolution selects the exact lane and geometry");

            decision.ActionableNow = false;

            Assert(
                !ScenarioExecutionPolicy.IsCanonicalCandidateEligible(
                    tactical,
                    decision,
                    OpportunityLane.Tactical,
                    out reason),
                "non-actionable canonical state cannot authorize a scenario");

            SubmissionAttemptIdentity scopedA =
                new SubmissionAttemptIdentity(
                    "EURUSD|100|1",
                    "AutomaticMarket|100|1|CANONICAL-TACTICAL-BUY",
                    ExecutionSubmissionPath.AutomaticMarket,
                    "CANONICAL-TACTICAL-BUY");

            SubmissionAttemptIdentity scopedB =
                new SubmissionAttemptIdentity(
                    "EURUSD|100|1",
                    "AutomaticMarket|100|1|CANONICAL-HTF-BUY",
                    ExecutionSubmissionPath.AutomaticMarket,
                    "CANONICAL-HTF-BUY");

            Assert(
                scopedA.CanonicalKey != scopedB.CanonicalKey &&
                scopedA.ScenarioId != scopedB.ScenarioId,
                "submission retry state is isolated by scenario identity");
        }

        private static void VerifyActionabilityAndDivergenceState()
        {
            DivergenceResult divergence =
                new DivergenceResult(
                    1,
                    88,
                    "REGULAR_BULL",
                    true,
                    false,
                    false,
                    false);

            Assert(
                divergence.HasSignal &&
                divergence.Direction == 1 &&
                divergence.Quality == 88 &&
                divergence.RegularBull,
                "strong regular divergence state");

            TradeActionabilityResult blocked =
                TradeActionabilityResult.Blocked(
                    "LATE / PRICE EXTENDED",
                    81,
                    -1,
                    "REGULAR_BEAR");

            Assert(
                !blocked.Actionable &&
                blocked.Reason == "LATE / PRICE EXTENDED" &&
                blocked.DivergenceQuality == 81 &&
                blocked.DivergenceDirection == -1,
                "actionability block preserves divergence diagnostics");
        }

        private static void VerifyManagedIdentitySemantics()
        {
            string first;
            string same;
            string other;

            Assert(
                ManagedIdentityRule.TryBuildLabel(
                    "CFIP-SMART",
                    "INSTANCE-A",
                    out first) &&
                first == "CFIP-SMART|CFIP-I:INSTANCE-A",
                "managed identity label embeds the cTrader instance identity");

            Assert(
                ManagedIdentityRule.TryBuildLabel(
                    " CFIP|SMART ",
                    "INSTANCE|A",
                    out first) &&
                first == "CFIP/SMART|CFIP-I:INSTANCE/A",
                "managed identity label sanitizes separators deterministically");

            Assert(
                ManagedIdentityRule.TryBuildLabel(
                    "CFIP-SMART",
                    "INSTANCE-A",
                    out first) &&
                ManagedIdentityRule.TryBuildLabel(
                    "CFIP-SMART",
                    "INSTANCE-A",
                    out same) &&
                first == same,
                "same instance produces stable ownership identity");

            Assert(
                ManagedIdentityRule.TryBuildLabel(
                    "CFIP-SMART",
                    "INSTANCE-A",
                    out first) &&
                ManagedIdentityRule.TryBuildLabel(
                    "CFIP-SMART",
                    "INSTANCE-B",
                    out other) &&
                first != other,
                "different instances produce isolated ownership identities");

            Assert(
                !ManagedIdentityRule.TryBuildLabel(
                    "CFIP-SMART",
                    "",
                    out first) &&
                !ManagedIdentityRule.TryBuildLabel(
                    "",
                    "INSTANCE-A",
                    out first),
                "missing identity inputs fail closed");

            Assert(
                ManagedIdentityRule.InstanceMarker == "|CFIP-I:",
                "managed identity marker is centralized and stable");
        }

        private static void VerifyReversalProfitThresholdSemantics()
        {
            Assert(
                ReversalProfitThresholdRule.MeetsMinimumNetProfit(0.01, 0),
                "default reversal threshold retains strictly-positive-profit behavior");

            Assert(
                !ReversalProfitThresholdRule.MeetsMinimumNetProfit(0, 0),
                "zero net profit does not satisfy the strict reversal threshold");

            Assert(
                ReversalProfitThresholdRule.MeetsMinimumNetProfit(10, 5) &&
                !ReversalProfitThresholdRule.MeetsMinimumNetProfit(5, 5),
                "configured minimum net profit is a strict lower boundary");

            Assert(
                !ReversalProfitThresholdRule.MeetsMinimumNetProfit(
                    double.NaN,
                    0) &&
                !ReversalProfitThresholdRule.MeetsMinimumNetProfit(
                    10,
                    double.NaN),
                "non-finite reversal profit inputs fail closed");

            Assert(
                !ReversalProfitThresholdRule.MeetsMinimumNetProfit(10, -1),
                "negative configured reversal profit threshold is rejected");
        }

        private static void VerifyStructuralEventSemantics()
        {
            Assert(
                StructuralEventRule.IsFreshBreak(
                    1,
                    100.50,
                    100.60,
                    100.0,
                    1.0,
                    0.50),
                "bullish structural break is a fresh threshold crossing");

            Assert(
                !StructuralEventRule.IsFreshBreak(
                    1,
                    100.60,
                    100.70,
                    100.0,
                    1.0,
                    0.50),
                "bullish structural break does not repeat after the level is already broken");

            Assert(
                StructuralEventRule.IsFreshBreak(
                    -1,
                    99.50,
                    99.40,
                    100.0,
                    1.0,
                    0.50),
                "bearish structural break is a fresh threshold crossing");

            Assert(
                !StructuralEventRule.IsFreshBreak(
                    -1,
                    99.40,
                    99.30,
                    100.0,
                    1.0,
                    0.50),
                "bearish structural break does not repeat after the level is already broken");

            Assert(
                StructuralEventRule.IsChangeOfCharacter(
                    1,
                    true,
                    true) &&
                !StructuralEventRule.IsChangeOfCharacter(
                    1,
                    false,
                    true) &&
                !StructuralEventRule.IsChangeOfCharacter(
                    -1,
                    true,
                    false),
                "CHOCH requires prior opposite structure and a fresh break");

            Assert(
                StructuralEventRule.EventIdentity(
                    1,
                    "bos",
                    20,
                    25) ==
                StructuralEventRule.EventIdentity(
                    1,
                    "BOS",
                    20,
                    25) &&
                StructuralEventRule.EventIdentity(
                    1,
                    "BOS",
                    20,
                    25) !=
                StructuralEventRule.EventIdentity(
                    1,
                    "MSS",
                    20,
                    25) &&
                StructuralEventRule.EventIdentity(
                    1,
                    "BOS",
                    20,
                    25) !=
                StructuralEventRule.EventIdentity(
                    -1,
                    "BOS",
                    20,
                    25),
                "structural event identity separates type and direction");
        }

        private static void VerifyLiquiditySweepSemantics()
        {
            double[] intact =
            {
                100.10,
                99.95,
                100.05,
                100.20,
                100.30
            };

            Assert(
                LiquiditySweepRule.IsActiveUnbrokenLevel(
                    1,
                    0,
                    4,
                    100.0,
                    0.10,
                    i => intact[i]),
                "bullish liquidity remains active while prior closes stay above tolerance");

            double[] broken =
            {
                100.10,
                99.70,
                100.05,
                100.20,
                100.30
            };

            Assert(
                !LiquiditySweepRule.IsActiveUnbrokenLevel(
                    1,
                    0,
                    4,
                    100.0,
                    0.10,
                    i => broken[i]),
                "bullish sweep rejects a level already invalidated by a prior close");

            double[] bearIntact =
            {
                99.90,
                100.05,
                99.95,
                99.80,
                99.70
            };

            Assert(
                LiquiditySweepRule.IsActiveUnbrokenLevel(
                    -1,
                    0,
                    4,
                    100.0,
                    0.10,
                    i => bearIntact[i]),
                "bearish liquidity remains active while prior closes stay below tolerance");

            double[] bearBroken =
            {
                99.90,
                100.30,
                99.95,
                99.80,
                99.70
            };

            Assert(
                !LiquiditySweepRule.IsActiveUnbrokenLevel(
                    -1,
                    0,
                    4,
                    100.0,
                    0.10,
                    i => bearBroken[i]),
                "bearish sweep rejects a level already invalidated by a prior close");
        }

        private static void VerifyRejectionSemantics()
        {
            Assert(
                RejectionRule.IsRejection(
                    100.0,
                    100.5,
                    101.0,
                    98.0,
                    1,
                    0.10),
                "bullish rejection requires a meaningful body and lower wick");

            Assert(
                RejectionRule.IsRejection(
                    100.5,
                    100.0,
                    102.0,
                    99.0,
                    -1,
                    0.10),
                "bearish rejection is directionally symmetric");

            Assert(
                RejectionRule.IsDoji(
                    100.0,
                    100.01,
                    101.0,
                    99.0,
                    0.10) &&
                !RejectionRule.IsRejection(
                    100.0,
                    100.01,
                    101.0,
                    99.0,
                    1,
                    0.10),
                "doji-like body is not promoted to directional rejection evidence");
        }

        private static void VerifyDivergenceConflictSemantics()
        {
            DivergenceResult conflict =
                DivergenceResult.CreateConflict(
                    true,
                    true,
                    false,
                    false);

            Assert(
                conflict.Direction == 0 &&
                conflict.Quality == 0 &&
                conflict.Type == "CONFLICT" &&
                conflict.RegularBull &&
                conflict.RegularBear &&
                !conflict.HasSignal,
                "divergence conflict remains visible but cannot act as directional strength");
        }

        private static void VerifyStructuralTimeframeSemantics()
        {
            Assert(
                StructuralTimeframeRule.IsSupported("M15") &&
                StructuralTimeframeRule.IsSupported("H4") &&
                StructuralTimeframeRule.IsSupported("W1"),
                "known structural timeframes are accepted");

            Assert(
                !StructuralTimeframeRule.IsSupported("H2") &&
                !StructuralTimeframeRule.IsSupported("") &&
                !StructuralTimeframeRule.IsSupported("  "),
                "unknown structural timeframes fail explicitly");
        }

        private static void VerifyAggressiveEntryPolicy()
        {
            AggressiveEntryPolicy policy =
                new AggressiveEntryPolicy();

            DateTime t =
                new DateTime(
                    2026,
                    1,
                    1,
                    12,
                    0,
                    0,
                    DateTimeKind.Utc);

            Assert(
                !policy.ObserveReactionSample(200, 1, true, t),
                "first intrabar qualification sample does not arm");

            Assert(
                policy.QualifyingSamples == 1 &&
                policy.Direction == 1 &&
                policy.ReactionM5 == 200,
                "first qualifying sample is tracked");

            Assert(
                !policy.ObserveReactionSample(200, 1, true, t),
                "duplicate reaction sample does not count twice");

            Assert(
                policy.QualifyingSamples == 1,
                "duplicate sample count remains stable");

            Assert(
                policy.ObserveReactionSample(
                    200,
                    1,
                    true,
                    t.AddMilliseconds(800)),
                "second qualifying intrabar sample arms");

            Assert(
                policy.IsQualified &&
                policy.GetQualificationStateText() == "ARMED",
                "policy reaches armed state");

            Assert(
                !policy.ObserveReactionSample(
                    200,
                    -1,
                    true,
                    t.AddMilliseconds(1600)) &&
                !policy.IsQualified &&
                policy.Direction == -1 &&
                policy.QualifyingSamples == 1,
                "direction change invalidates prior qualification");

            Assert(
                !policy.ObserveReactionSample(
                    200,
                    -1,
                    false,
                    t.AddMilliseconds(2400)) &&
                !policy.IsQualified &&
                policy.QualifyingSamples == 0,
                "lost reaction qualification invalidates immediately");

            Assert(
                !policy.ObserveReactionSample(
                    201,
                    1,
                    true,
                    t.AddMilliseconds(3200)) &&
                policy.ReactionM5 == 201 &&
                policy.Direction == 1 &&
                policy.QualifyingSamples == 1,
                "new M5 starts a fresh qualification window");
        }

        private static void VerifyReactionQualificationSemantics()
        {
            Assert(
                ReactionQualificationRule.ResolveDirection(80, 70) == 1 &&
                ReactionQualificationRule.ResolveDirection(70, 80) == -1 &&
                ReactionQualificationRule.ResolveDirection(80, 80) == 0,
                "reaction direction resolves symmetrically and ties to neutral");

            Assert(
                ReactionQualificationRule.HasPriorCounterMove(
                    1,
                    100.0,
                    99.0,
                    98.0) &&
                ReactionQualificationRule.HasPriorCounterMove(
                    -1,
                    100.0,
                    101.0,
                    102.0),
                "reversal requires a prior move against the candidate direction");

            Assert(
                !ReactionQualificationRule.HasPriorCounterMove(
                    1,
                    100.0,
                    101.0,
                    101.5) &&
                !ReactionQualificationRule.HasPriorCounterMove(
                    -1,
                    100.0,
                    99.0,
                    98.5),
                "same-direction continuation is not accepted as prior counter-move");

            Assert(
                ReactionQualificationRule.HasQualifyingContext(
                    true,
                    false,
                    false,
                    false) &&
                ReactionQualificationRule.HasQualifyingContext(
                    false,
                    false,
                    false,
                    true),
                "no-zone reversal requires a real non-zone structural context");

            Assert(
                ReactionQualificationRule.HasQualifyingContext(
                    false,
                    true,
                    true,
                    false) &&
                !ReactionQualificationRule.HasQualifyingContext(
                    true,
                    true,
                    false,
                    true),
                "present zones must be qualifying and cannot be bypassed by unrelated context");

            Assert(
                ReactionQualificationRule.IsQualified(
                    1,
                    75,
                    3,
                    true,
                    70,
                    2,
                    false,
                    false) &&
                !ReactionQualificationRule.IsQualified(
                    1,
                    75,
                    3,
                    true,
                    70,
                    2,
                    true,
                    false) &&
                ReactionQualificationRule.IsQualified(
                    1,
                    75,
                    3,
                    true,
                    70,
                    2,
                    true,
                    true),
                "intrabar qualification can arm immediately, while pending confirmation requires a closed bar");

            Assert(
                !ReactionQualificationRule.IsQualified(
                    1,
                    90,
                    3,
                    false,
                    70,
                    2,
                    false,
                    false) &&
                !ReactionQualificationRule.IsQualified(
                    0,
                    90,
                    3,
                    true,
                    70,
                    2,
                    false,
                    false),
                "missing context or direction always fails closed");

            Assert(
                ReactionQualificationRule.IsClosedBarConfirmed(
                    20,
                    20) &&
                ReactionQualificationRule.IsClosedBarConfirmed(
                    20,
                    21) &&
                !ReactionQualificationRule.IsClosedBarConfirmed(
                    20,
                    19),
                "closed-bar confirmation never accepts a candidate after the known closed index");
        }

        private static void VerifyIndicatorExecutionQualitySemantics()
        {
            IndicatorQualityGateResult marketPass =
                IndicatorExecutionQualityRule.EvaluateIndicatorExecutionQuality(
                    IndicatorQualityGateStage.AutomaticMarket,
                    60,
                    52);

            Assert(
                marketPass.Allowed &&
                marketPass.MinimumQuality == 60 &&
                marketPass.MaximumConflict == 52,
                "automatic market uses its documented final-entry indicator gate");

            Assert(
                !IndicatorExecutionQualityRule.EvaluateIndicatorExecutionQuality(
                    IndicatorQualityGateStage.AutomaticMarket,
                    59,
                    40).Allowed &&
                !IndicatorExecutionQualityRule.EvaluateIndicatorExecutionQuality(
                    IndicatorQualityGateStage.AutomaticMarket,
                    80,
                    53).Allowed,
                "automatic market rejects below quality or above conflict");

            Assert(
                IndicatorExecutionQualityRule.EvaluateIndicatorExecutionQuality(
                    IndicatorQualityGateStage.PendingSubmission,
                    58,
                    55).Allowed &&
                !IndicatorExecutionQualityRule.EvaluateIndicatorExecutionQuality(
                    IndicatorQualityGateStage.PendingSubmission,
                    57,
                    10).Allowed &&
                !IndicatorExecutionQualityRule.EvaluateIndicatorExecutionQuality(
                    IndicatorQualityGateStage.PendingSubmission,
                    80,
                    56).Allowed,
                "pending submission keeps a separate lower defense-in-depth floor");

            IndicatorQualityGateResult continuation =
                IndicatorExecutionQualityRule.EvaluateIndicatorExecutionQuality(
                    IndicatorQualityGateStage.PendingContinuation,
                    62,
                    48);

            IndicatorQualityGateResult reversal =
                IndicatorExecutionQualityRule.EvaluateIndicatorExecutionQuality(
                    IndicatorQualityGateStage.PendingReversal,
                    62,
                    50);

            Assert(
                continuation.Allowed &&
                reversal.Allowed &&
                continuation.MinimumQuality ==
                    reversal.MinimumQuality &&
                continuation.MinimumQuality == 62,
                "continuation and reversal share one pending setup quality floor");

            Assert(
                !IndicatorExecutionQualityRule.EvaluateIndicatorExecutionQuality(
                    IndicatorQualityGateStage.PendingContinuation,
                    70,
                    49).Allowed &&
                !IndicatorExecutionQualityRule.EvaluateIndicatorExecutionQuality(
                    IndicatorQualityGateStage.PendingReversal,
                    70,
                    51).Allowed,
                "continuation and reversal retain explicitly different conflict tolerance");

            Assert(
                IndicatorExecutionQualityRule.EvaluateIndicatorExecutionQuality(
                    IndicatorQualityGateStage.PendingContinuation,
                    61,
                    48).Reason.Contains(
                        "PENDING CONTINUATION") &&
                IndicatorExecutionQualityRule.EvaluateIndicatorExecutionQuality(
                    IndicatorQualityGateStage.PendingSubmission,
                    57,
                    20).Reason.Contains(
                        "PENDING SUBMISSION"),
                "blocked diagnostics identify the canonical semantic gate");

            Assert(
                IndicatorExecutionQualityRule.EvaluateIndicatorExecutionQuality(
                    (IndicatorQualityGateStage)999,
                    100,
                    0).Allowed == false,
                "unknown indicator gate stage fails closed");
        }

        private static void VerifyPendingDecisionArbiterSemantics()
        {
            Assert(
                PendingDecisionArbiterRule.ScoreCandidate(
                    90,
                    80,
                    70,
                    4,
                    4) >
                PendingDecisionArbiterRule.ScoreCandidate(
                    80,
                    70,
                    60,
                    3,
                    3),
                "pending candidate score rewards stronger decision evidence deterministically");

            PendingArbiterResult continuationOnly =
                PendingDecisionArbiterRule.SelectWinner(
                    true,
                    1,
                    80,
                    false,
                    -1,
                    90,
                    PendingOrderMode.Adaptive);

            Assert(
                continuationOnly.Choice ==
                    PendingArbiterChoice.ContinuationStop &&
                continuationOnly.Direction == 1,
                "single eligible continuation is selected");

            PendingArbiterResult reversalOnly =
                PendingDecisionArbiterRule.SelectWinner(
                    false,
                    1,
                    80,
                    true,
                    -1,
                    90,
                    PendingOrderMode.Adaptive);

            Assert(
                reversalOnly.Choice ==
                    PendingArbiterChoice.ReversalLimit &&
                reversalOnly.Direction == -1,
                "single eligible reversal is selected");

            PendingArbiterResult bothContinuationWins =
                PendingDecisionArbiterRule.SelectWinner(
                    true,
                    1,
                    92,
                    true,
                    -1,
                    84,
                    PendingOrderMode.Both);

            PendingArbiterResult bothReversalWins =
                PendingDecisionArbiterRule.SelectWinner(
                    true,
                    1,
                    84,
                    true,
                    -1,
                    92,
                    PendingOrderMode.Both);

            Assert(
                bothContinuationWins.Choice ==
                    PendingArbiterChoice.ContinuationStop &&
                bothReversalWins.Choice ==
                    PendingArbiterChoice.ReversalLimit,
                "when both candidates are eligible, quality chooses exactly one winner");

            PendingArbiterResult tie =
                PendingDecisionArbiterRule.SelectWinner(
                    true,
                    1,
                    88,
                    true,
                    -1,
                    88,
                    PendingOrderMode.Both);

            Assert(
                tie.Choice ==
                    PendingArbiterChoice.ContinuationStop &&
                tie.Reason.Contains(
                    "TIE"),
                "exact candidate ties use one explicit deterministic policy");

            Assert(
                PendingDecisionArbiterRule.SelectWinner(
                    true,
                    1,
                    100,
                    true,
                    -1,
                    100,
                    PendingOrderMode.ContinuationStop).Choice ==
                    PendingArbiterChoice.ContinuationStop &&
                PendingDecisionArbiterRule.SelectWinner(
                    true,
                    1,
                    100,
                    true,
                    -1,
                    100,
                    PendingOrderMode.ReversalLimit).Choice ==
                    PendingArbiterChoice.ReversalLimit,
                "pending mode constrains the arbiter before quality comparison");

            Assert(
                PendingDecisionArbiterRule.SelectWinner(
                    false,
                    1,
                    0,
                    false,
                    -1,
                    0,
                    PendingOrderMode.Both).Choice ==
                    PendingArbiterChoice.None,
                "no eligible candidate produces no pending decision");

            Assert(
                PendingDecisionArbiterRule.IsSameChoice(
                    PendingArbiterChoice.ContinuationStop,
                    true) &&
                !PendingDecisionArbiterRule.IsSameChoice(
                    PendingArbiterChoice.ContinuationStop,
                    false) &&
                PendingDecisionArbiterRule.IsSameChoice(
                    PendingArbiterChoice.ReversalLimit,
                    false) &&
                !PendingDecisionArbiterRule.IsSameChoice(
                    PendingArbiterChoice.ReversalLimit,
                    true),
                "existing pending order type maps unambiguously to its policy choice");

            Assert(
                !PendingDecisionArbiterRule.ShouldCancelAfterHysteresis(
                    true,
                    1,
                    2) &&
                PendingDecisionArbiterRule.ShouldCancelAfterHysteresis(
                    true,
                    2,
                    2) &&
                !PendingDecisionArbiterRule.ShouldCancelAfterHysteresis(
                    false,
                    9,
                    2),
                "pending cancellation needs two consecutive invalidation bars");

            Assert(
                PendingDecisionArbiterRule.ScoreCandidate(
                    90,
                    90,
                    90,
                    8,
                    8) <= 100 &&
                PendingDecisionArbiterRule.ScoreCandidate(
                    0,
                    0,
                    0,
                    0,
                    0) == 0,
                "candidate score remains bounded");
        }

        private static void VerifyLifecycleOutcomeSemantics()
        {
            LifecycleEventIdempotencyGuard guard =
                new LifecycleEventIdempotencyGuard();

            Assert(
                guard.TryBegin("POSITION_OPENED", 101) &&
                !guard.TryBegin("POSITION_OPENED", 101) &&
                guard.TryBegin("POSITION_CLOSED", 101),
                "lifecycle idempotency distinguishes event type and rejects duplicates");

            for (long id = 1; id <= 600; id++)
            {
                Assert(
                    guard.TryBegin("TEST_EVENT", id),
                    "bounded idempotency accepts fresh lifecycle identities");
            }

            Assert(
                guard.RememberedEventCount <= 512,
                "lifecycle idempotency memory is bounded");

            HistoricalOutcomeAggregate empty =
                HistoricalOutcomeAggregationRule.AggregateHistoricalOutcomeRecords(
                    null);

            Assert(
                !empty.Available &&
                empty.TradeCount == 0,
                "empty historical outcome set fails closed");

            List<HistoricalOutcomeRecord> trades =
                new List<HistoricalOutcomeRecord>
                {
                    new HistoricalOutcomeRecord
                    {
                        NetProfit = 40,
                        GrossProfit = 45,
                        Swap = -2,
                        Commissions = -3,
                        Pips = 10,
                        ClosingTime =
                            new DateTime(
                                2026,
                                9,
                                30,
                                10,
                                0,
                                0,
                                DateTimeKind.Utc)
                    },
                    new HistoricalOutcomeRecord
                    {
                        NetProfit = -10,
                        GrossProfit = -8,
                        Swap = -1,
                        Commissions = -1,
                        Pips = -2,
                        ClosingTime =
                            new DateTime(
                                2026,
                                9,
                                30,
                                10,
                                5,
                                0,
                                DateTimeKind.Utc)
                    }
                };

            HistoricalOutcomeAggregate aggregate =
                HistoricalOutcomeAggregationRule.AggregateHistoricalOutcomeRecords(
                    trades);

            Assert(
                aggregate.Available &&
                aggregate.TradeCount == 2 &&
                Math.Abs(aggregate.NetProfit - 30) < 0.000001 &&
                Math.Abs(aggregate.GrossProfit - 37) < 0.000001 &&
                Math.Abs(aggregate.Swap + 3) < 0.000001 &&
                Math.Abs(aggregate.Commissions + 4) < 0.000001 &&
                Math.Abs(aggregate.Pips - 8) < 0.000001 &&
                aggregate.ClosingTime ==
                    new DateTime(
                        2026,
                        9,
                        30,
                        10,
                        5,
                        0,
                        DateTimeKind.Utc),
                "multiple closing historical trades aggregate to one final realized outcome");

            HistoricalOutcomeAggregate invalid =
                HistoricalOutcomeAggregationRule.AggregateHistoricalOutcomeRecords(
                    new[]
                    {
                        new HistoricalOutcomeRecord
                        {
                            NetProfit = double.NaN,
                            GrossProfit = 1,
                            Swap = 0,
                            Commissions = 0,
                            Pips = 1,
                            ClosingTime = DateTime.UtcNow
                        }
                    });

            Assert(
                !invalid.Available &&
                invalid.TradeCount == 0,
                "non-finite historical outcome data is ignored");

            HistoricalOutcomeAggregate fallback =
                HistoricalOutcomeAggregationRule.FromPositionFallback(
                    25,
                    5);

            Assert(
                fallback.Available &&
                fallback.TradeCount == 1 &&
                Math.Abs(fallback.NetProfit - 25) < 0.000001 &&
                Math.Abs(fallback.Pips - 5) < 0.000001,
                "position outcome fallback remains deterministic when history is unavailable");
        }

        private static void Assert(bool condition, string name)
        {
            if (!condition)
                throw new InvalidOperationException(
                    "Runtime acceptance contract failed: " +
                    name);
        }
    }
}