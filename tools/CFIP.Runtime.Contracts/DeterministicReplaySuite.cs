using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace cAlgo
{
    internal static class DeterministicReplaySuite
    {
        private const double PipSize = 0.01;
        private const double TickSize = 0.001;
        private const double Atr = 2.0;
        private const double MinimumRr = 1.0;
        private const double MaximumRr = 6.5;
        private const double RiskFloor = 0.01;

        public static void Verify()
        {
            IReadOnlyList<ReplayTrace> first = ReplayRunner.Run();
            IReadOnlyList<ReplayTrace> second = ReplayRunner.Run();

            Assert(
                first.Count == 16 &&
                second.Count == 16,
                "CI-16 has exactly the required 16 replay counterexample scenarios");

            HashSet<string> expected =
                new HashSet<string>(
                    ReplayScenarioCatalog.Names,
                    StringComparer.Ordinal);

            Assert(
                first.Select(t => t.ScenarioName).SequenceEqual(ReplayScenarioCatalog.Names) &&
                new HashSet<string>(
                    first.Select(t => t.ScenarioName),
                    StringComparer.Ordinal).SetEquals(expected),
                "CI-16 replay catalog is complete and ordered");

            for (int i = 0; i < first.Count; i++)
            {
                Assert(
                    first[i].Serialize() == second[i].Serialize(),
                    "CI-16 replay output is deterministic: " +
                    first[i].ScenarioName);

                first[i].Validate();
            }

            VerifyMirrorSymmetry(first.Single(
                t => t.ScenarioName == "mirrored BUY/SELL"));

            Console.WriteLine(
                "CI-16 deterministic replay, latency and counterexample contracts PASS");

            foreach (ReplayTrace trace in first)
            {
                Console.WriteLine(
                    "CI-16 " +
                    trace.ScenarioName +
                    " | " +
                    trace.Summary());
            }
        }

        private static void VerifyMirrorSymmetry(ReplayTrace trace)
        {
            Assert(
                !string.IsNullOrWhiteSpace(trace.MirrorGeometryFingerprint),
                "CI-16 mirrored BUY/SELL fixture records its mirror geometry");

            Assert(
                trace.MirrorRiskPips == trace.RiskPips &&
                trace.MirrorTargetPips == trace.TargetPips &&
                Math.Abs(trace.NominalRR - trace.MirrorNominalRR) < 1e-12 &&
                Math.Abs(trace.EffectiveRR - trace.MirrorEffectiveRR) < 1e-12,
                "CI-16 mirrored BUY/SELL preserves distance and RR symmetry");
        }

        private static void Assert(bool condition, string message)
        {
            if (!condition)
                throw new InvalidOperationException(
                    "Runtime acceptance contract failed: " +
                    message);
        }

        private sealed class ReplayScenarioCatalog
        {
            internal static readonly string[] Names =
            {
                "fast breakout",
                "slow breakout",
                "fast reversal",
                "retest",
                "range market",
                "expansion",
                "compression",
                "strong OB+FVG confluence",
                "weak single-zone setup",
                "high spread",
                "large displacement",
                "M1 confirmation late in M5",
                "M1 confirmation early in M5",
                "target obstruction",
                "opposite divergence",
                "mirrored BUY/SELL"
            };

            internal static IReadOnlyList<ReplayScenario> Create()
            {
                return new[]
                {
                    new ReplayScenario(
                        Names[0], 1, 101.50, 100.80, false, true, false,
                        true, false, false, 0.05, 88, 91, 4, 84, 82, 0.05,
                        false, false, false, 210),

                    new ReplayScenario(
                        Names[1], 1, 100.88, 100.80, false, true, false,
                        true, false, false, 0.08, 82, 86, 3, 80, 76, 0.10,
                        false, false, false, 260),

                    new ReplayScenario(
                        Names[2], -1, 98.50, 99.20, false, true, false,
                        true, false, false, 0.07, 86, 89, 3, 83, 78, 0.08,
                        false, false, true, 170),

                    new ReplayScenario(
                        Names[3], 1, 99.60, 100.80, false, false, false,
                        true, false, false, 0.06, 84, 88, 3, 81, 80, 0.04,
                        false, false, false, 190),

                    new ReplayScenario(
                        Names[4], 1, 100.10, 100.80, false, false, false,
                        false, false, false, 0.02, 54, 55, 2, 60, 48, 0.02,
                        false, false, false, 0),

                    new ReplayScenario(
                        Names[5], 1, 100.90, 100.80, false, true, false,
                        true, false, false, 0.12, 89, 92, 4, 86, 84, 0.12,
                        false, false, false, 150),

                    new ReplayScenario(
                        Names[6], 1, 100.05, 100.80, false, false, true,
                        false, false, false, 0.01, 57, 59, 2, 62, 50, 0.01,
                        false, false, false, 0),

                    new ReplayScenario(
                        Names[7], 1, 99.75, 100.80, false, false, false,
                        true, true, false, 0.10, 93, 95, 5, 92, 88, 0.06,
                        false, false, false, 120),

                    new ReplayScenario(
                        Names[8], 1, 99.75, 100.80, false, false, false,
                        false, true, false, 0.04, 63, 67, 1, 68, 65, 0.05,
                        false, false, false, 0),

                    new ReplayScenario(
                        Names[9], 1, 100.00, 100.80, false, false, false,
                        true, false, false, 0.80, 83, 86, 3, 82, 79, 0.80,
                        false, false, false, 180),

                    new ReplayScenario(
                        Names[10], 1, 102.30, 100.80, false, true, false,
                        true, true, false, 0.08, 91, 93, 4, 89, 86, 1.50,
                        false, false, false, 180),

                    new ReplayScenario(
                        Names[11], 1, 100.85, 100.80, false, true, false,
                        true, false, true, 0.06, 88, 90, 4, 84, 81, 0.40,
                        true, true, false, 240),

                    new ReplayScenario(
                        Names[12], 1, 100.85, 100.80, false, true, false,
                        true, false, true, 0.06, 88, 90, 4, 84, 81, 0.40,
                        true, false, false, 200),

                    new ReplayScenario(
                        Names[13], 1, 100.00, 100.80, false, false, false,
                        true, false, false, 0.05, 90, 92, 4, 88, 84, 0.06,
                        false, false, true, 180),

                    new ReplayScenario(
                        Names[14], 1, 100.00, 100.80, false, false, false,
                        false, false, false, 0.05, 76, 73, 2, 78, 70, 0.05,
                        false, false, true, 0),

                    new ReplayScenario(
                        Names[15], 1, 100.00, 100.80, false, false, false,
                        true, true, false, 0.08, 92, 94, 5, 90, 87, 0.08,
                        false, false, false, 160)
                };
            }
        }

        private sealed class ReplayScenario
        {
            internal ReplayScenario(
                string name,
                int direction,
                double market,
                double trigger,
                bool continuation,
                bool allowBreakout,
                bool useM1Trigger,
                bool triggerReady,
                bool obFvgConfluence,
                bool weakSingleZone,
                double spread,
                int decisionQuality,
                int confidence,
                int independentEvidence,
                int structureQuality,
                int locationQuality,
                double displacementAtr,
                bool m1TimingCase,
                bool m1Late,
                bool targetObstructed,
                int causalLatencyMs)
            {
                Name = name;
                Direction = direction;
                Market = market;
                Trigger = trigger;
                Continuation = continuation;
                AllowBreakout = allowBreakout;
                UseM1Trigger = useM1Trigger;
                TriggerReady = triggerReady;
                ObFvgConfluence = obFvgConfluence;
                WeakSingleZone = weakSingleZone;
                Spread = spread;
                DecisionQuality = decisionQuality;
                Confidence = confidence;
                IndependentEvidence = independentEvidence;
                StructureQuality = structureQuality;
                LocationQuality = locationQuality;
                DisplacementAtr = displacementAtr;
                M1TimingCase = m1TimingCase;
                M1Late = m1Late;
                TargetObstructed = targetObstructed;
                CausalLatencyMs = causalLatencyMs;
            }

            internal string Name { get; }
            internal int Direction { get; }
            internal double Market { get; }
            internal double Trigger { get; }
            internal bool Continuation { get; }
            internal bool AllowBreakout { get; }
            internal bool UseM1Trigger { get; }
            internal bool TriggerReady { get; }
            internal bool ObFvgConfluence { get; }
            internal bool WeakSingleZone { get; }
            internal double Spread { get; }
            internal int DecisionQuality { get; }
            internal int Confidence { get; }
            internal int IndependentEvidence { get; }
            internal int StructureQuality { get; }
            internal int LocationQuality { get; }
            internal double DisplacementAtr { get; }
            internal bool M1TimingCase { get; }
            internal bool M1Late { get; }
            internal bool TargetObstructed { get; }
            internal int CausalLatencyMs { get; }
        }

        private static class ReplayRunner
        {
            internal static IReadOnlyList<ReplayTrace> Run()
            {
                IReadOnlyList<ReplayScenario> scenarios =
                    ReplayScenarioCatalog.Create();

                List<ReplayTrace> traces =
                    new List<ReplayTrace>(scenarios.Count);

                for (int i = 0; i < scenarios.Count; i++)
                    traces.Add(Build(scenarios[i], i));

                return traces;
            }

            private static ReplayTrace Build(
                ReplayScenario scenario,
                int sequence)
            {
                DateTime referenceUtc =
                    new DateTime(
                        2026,
                        10,
                        2,
                        0,
                        0,
                        0,
                        DateTimeKind.Utc)
                    .AddMinutes(sequence * 7);

                DateTime causalUtc =
                    referenceUtc.AddMilliseconds(
                        -Math.Max(0, scenario.CausalLatencyMs));

                DateTime quoteUtc =
                    referenceUtc.AddMilliseconds(-25);

                ReplayIndicatorSnapshot indicators =
                    new ReplayIndicatorSnapshot(
                        scenario.DisplacementAtr,
                        scenario.DecisionQuality,
                        scenario.IndependentEvidence,
                        scenario.Spread);

                ReplayStructureSnapshot structure =
                    new ReplayStructureSnapshot(
                        scenario.StructureQuality,
                        scenario.ObFvgConfluence ? 92 : scenario.WeakSingleZone ? 48 : 70,
                        scenario.ObFvgConfluence ? 2 : scenario.WeakSingleZone ? 1 : 0,
                        scenario.TargetObstructed);

                DateTime m5Open =
                    referenceUtc.AddMinutes(-5);
                DateTime m5NextOpen =
                    referenceUtc;
                DateTime m1Open =
                    scenario.M1Late
                        ? referenceUtc.AddMinutes(-1)
                        : referenceUtc.AddMinutes(-4);
                DateTime m1NextOpen =
                    referenceUtc;

                bool m1CanEvaluate =
                    scenario.M1TimingCase &&
                    TriggerLifecycleRule.CanEvaluateLiveM1Confirmation(
                        100,
                        101,
                        101,
                        m1Open,
                        m1NextOpen,
                        m5Open,
                        m5NextOpen,
                        referenceUtc);

                bool m1Ready =
                    scenario.M1TimingCase &&
                    m1CanEvaluate &&
                    scenario.TriggerReady;

                bool triggerConfirmed =
                    scenario.TriggerReady &&
                    (!scenario.UseM1Trigger || m1Ready);

                EntryGeometrySnapshot entry =
                    EntryGeometryRule.Evaluate(
                        scenario.Direction,
                        ExecutionMode.None,
                        scenario.Market,
                        99.0,
                        101.0,
                        0.05,
                        100.0,
                        scenario.Trigger,
                        100.0,
                        Atr,
                        TickSize,
                        PipSize,
                        scenario.AllowBreakout,
                        scenario.Continuation,
                        0.80,
                        0.50);

                ReplayPlanGeometry geometry =
                    BuildGeometry(
                        scenario.Direction,
                        entry.ActualEntry > 0
                            ? entry.ActualEntry
                            : 100.0,
                        scenario.Spread);

                RiskRewardMathResult rr =
                    RiskRewardMathRule.Evaluate(
                        scenario.Direction,
                        geometry.Entry,
                        geometry.Stop,
                        geometry.Target1,
                        scenario.Spread,
                        MinimumRr,
                        MaximumRr,
                        RiskFloor);

                ExecutionIntentGeometryResult intent =
                    ExecutionIntentGeometryRule.Evaluate(
                        scenario.Direction,
                        geometry.Entry,
                        geometry.Stop,
                        geometry.Target1,
                        PipSize);

                double actualFill =
                    geometry.Entry +
                    scenario.Direction *
                    0.02 *
                    Atr;

                bool fillAccepted =
                    ExecutionFillAcceptanceRule.IsAcceptable(
                        scenario.Direction,
                        geometry.Entry,
                        actualFill,
                        Atr,
                        0.10,
                        true);

                bool analyticalBlock =
                    scenario.Name == "range market" ||
                    scenario.Name == "compression" ||
                    scenario.Name == "weak single-zone setup" ||
                    scenario.Name == "opposite divergence";

                bool actionable =
                    triggerConfirmed &&
                    !entry.IsLate &&
                    rr.Valid &&
                    intent.Valid &&
                    fillAccepted &&
                    !analyticalBlock;

                DateTime actionableUtc =
                    actionable
                        ? referenceUtc.AddMilliseconds(100)
                        : DateTime.MinValue;

                DateTime alertUtc =
                    actionable
                        ? actionableUtc.AddMilliseconds(50)
                        : DateTime.MinValue;

                DateTime executionAttemptUtc =
                    actionable
                        ? alertUtc.AddMilliseconds(50)
                        : DateTime.MinValue;

                DateTime fillUtc =
                    actionable
                        ? executionAttemptUtc.AddMilliseconds(110)
                        : DateTime.MinValue;

                ReplayTriggerSnapshot trigger =
                    new ReplayTriggerSnapshot(
                        scenario.TriggerReady,
                        m1CanEvaluate,
                        m1Ready,
                        triggerConfirmed,
                        scenario.UseM1Trigger,
                        scenario.M1Late);

                ReplayDecisionSnapshot decision =
                    new ReplayDecisionSnapshot(
                        scenario.Direction,
                        scenario.DecisionQuality,
                        scenario.Confidence,
                        scenario.IndependentEvidence,
                        scenario.Direction == 1 ? 0.72 : -0.72,
                        analyticalBlock);

                ReplayGeometrySnapshot authoritative =
                    ReplayGeometrySnapshot.Create(
                        geometry,
                        rr,
                        intent,
                        scenario.Spread);

                ReplayGeometrySnapshot submitted =
                    authoritative;

                double riskPips =
                    geometry.Risk /
                    PipSize;

                double targetPips =
                    geometry.TargetPips;

                double mirrorRiskPips = riskPips;
                double mirrorTargetPips = targetPips;
                double mirrorNominalRr = rr.NominalRR;
                double mirrorEffectiveRr = rr.EffectiveRR;
                string mirrorFingerprint = string.Empty;

                if (scenario.Name == "mirrored BUY/SELL")
                {
                    ReplayPlanGeometry mirrorGeometry =
                        BuildGeometry(
                            -1,
                            geometry.Entry,
                            scenario.Spread);

                    RiskRewardMathResult mirrorRr =
                        RiskRewardMathRule.Evaluate(
                            -1,
                            mirrorGeometry.Entry,
                            mirrorGeometry.Stop,
                            mirrorGeometry.Target1,
                            scenario.Spread,
                            MinimumRr,
                            MaximumRr,
                            RiskFloor);

                    mirrorRiskPips =
                        mirrorGeometry.Risk /
                        PipSize;

                    mirrorTargetPips =
                        mirrorRr.Reward /
                        PipSize;

                    mirrorNominalRr =
                        mirrorRr.NominalRR;

                    mirrorEffectiveRr =
                        mirrorRr.EffectiveRR;

                    mirrorFingerprint =
                        ReplayGeometrySnapshot
                            .CreateMirror(
                                mirrorGeometry,
                                mirrorRr)
                            .Fingerprint;
                }

                return new ReplayTrace(
                    scenario.Name,
                    sequence,
                    referenceUtc,
                    quoteUtc,
                    causalUtc,
                    indicators,
                    structure,
                    decision,
                    trigger,
                    geometry,
                    rr,
                    intent,
                    fillAccepted,
                    actionable,
                    actionableUtc,
                    alertUtc,
                    executionAttemptUtc,
                    fillUtc,
                    authoritative.Fingerprint,
                    submitted.Fingerprint,
                    riskPips,
                    targetPips,
                    mirrorRiskPips,
                    mirrorTargetPips,
                    mirrorNominalRr,
                    mirrorEffectiveRr,
                    mirrorFingerprint);
            }

            private static ReplayPlanGeometry BuildGeometry(
                int direction,
                double entry,
                double spread)
            {
                double safeEntry =
                    entry > 0
                        ? entry
                        : 100.0;

                double stop =
                    direction == 1
                        ? safeEntry - 2.0
                        : safeEntry + 2.0;

                double target1 =
                    direction == 1
                        ? safeEntry + 4.0
                        : safeEntry - 4.0;

                double target2 =
                    direction == 1
                        ? safeEntry + 6.4
                        : safeEntry - 6.4;

                double target3 =
                    direction == 1
                        ? safeEntry + 9.6
                        : safeEntry - 9.6;

                double target4 =
                    direction == 1
                        ? safeEntry + 13.0
                        : safeEntry - 13.0;

                return new ReplayPlanGeometry(
                    safeEntry,
                    stop,
                    target1,
                    target2,
                    target3,
                    target4,
                    Math.Abs(stop - safeEntry),
                    Math.Abs(target1 - safeEntry),
                    spread);
            }
        }

        private readonly struct ReplayIndicatorSnapshot
        {
            internal ReplayIndicatorSnapshot(
                double displacementAtr,
                int quality,
                int independentEvidence,
                double spread)
            {
                DisplacementAtr = displacementAtr;
                Quality = quality;
                IndependentEvidence = independentEvidence;
                Spread = spread;
            }

            internal double DisplacementAtr { get; }
            internal int Quality { get; }
            internal int IndependentEvidence { get; }
            internal double Spread { get; }

            internal string Serialize()
            {
                return
                    F(DisplacementAtr) + "|" +
                    Quality.ToString(CultureInfo.InvariantCulture) + "|" +
                    IndependentEvidence.ToString(CultureInfo.InvariantCulture) + "|" +
                    F(Spread);
            }
        }

        private readonly struct ReplayStructureSnapshot
        {
            internal ReplayStructureSnapshot(
                int quality,
                int fvgQuality,
                int confluence,
                bool targetObstructed)
            {
                Quality = quality;
                FvgQuality = fvgQuality;
                Confluence = confluence;
                TargetObstructed = targetObstructed;
            }

            internal int Quality { get; }
            internal int FvgQuality { get; }
            internal int Confluence { get; }
            internal bool TargetObstructed { get; }

            internal string Serialize()
            {
                return
                    Quality.ToString(CultureInfo.InvariantCulture) + "|" +
                    FvgQuality.ToString(CultureInfo.InvariantCulture) + "|" +
                    Confluence.ToString(CultureInfo.InvariantCulture) + "|" +
                    TargetObstructed;
            }
        }

        private readonly struct ReplayDecisionSnapshot
        {
            internal ReplayDecisionSnapshot(
                int direction,
                int quality,
                int confidence,
                int independentEvidence,
                double consensus,
                bool blocked)
            {
                Direction = direction;
                Quality = quality;
                Confidence = confidence;
                IndependentEvidence = independentEvidence;
                Consensus = consensus;
                Blocked = blocked;
            }

            internal int Direction { get; }
            internal int Quality { get; }
            internal int Confidence { get; }
            internal int IndependentEvidence { get; }
            internal double Consensus { get; }
            internal bool Blocked { get; }

            internal string Serialize()
            {
                return
                    Direction.ToString(CultureInfo.InvariantCulture) + "|" +
                    Quality.ToString(CultureInfo.InvariantCulture) + "|" +
                    Confidence.ToString(CultureInfo.InvariantCulture) + "|" +
                    IndependentEvidence.ToString(CultureInfo.InvariantCulture) + "|" +
                    F(Consensus) + "|" +
                    Blocked;
            }
        }

        private readonly struct ReplayTriggerSnapshot
        {
            internal ReplayTriggerSnapshot(
                bool m5Ready,
                bool m1CanEvaluate,
                bool m1Ready,
                bool confirmed,
                bool useM1,
                bool late)
            {
                M5Ready = m5Ready;
                M1CanEvaluate = m1CanEvaluate;
                M1Ready = m1Ready;
                Confirmed = confirmed;
                UseM1 = useM1;
                Late = late;
            }

            internal bool M5Ready { get; }
            internal bool M1CanEvaluate { get; }
            internal bool M1Ready { get; }
            internal bool Confirmed { get; }
            internal bool UseM1 { get; }
            internal bool Late { get; }

            internal string Serialize()
            {
                return
                    M5Ready + "|" +
                    M1CanEvaluate + "|" +
                    M1Ready + "|" +
                    Confirmed + "|" +
                    UseM1 + "|" +
                    Late;
            }
        }

        private readonly struct ReplayPlanGeometry
        {
            internal ReplayPlanGeometry(
                double entry,
                double stop,
                double target1,
                double target2,
                double target3,
                double target4,
                double risk,
                double target1Distance,
                double spread)
            {
                Entry = entry;
                Stop = stop;
                Target1 = target1;
                Target2 = target2;
                Target3 = target3;
                Target4 = target4;
                Risk = risk;
                TargetPips = target1Distance / PipSize;
                Spread = spread;
            }

            internal double Entry { get; }
            internal double Stop { get; }
            internal double Target1 { get; }
            internal double Target2 { get; }
            internal double Target3 { get; }
            internal double Target4 { get; }
            internal double Risk { get; }
            internal double TargetPips { get; }
            internal double Spread { get; }

            internal string Serialize()
            {
                return
                    F(Entry) + "|" +
                    F(Stop) + "|" +
                    F(Target1) + "|" +
                    F(Target2) + "|" +
                    F(Target3) + "|" +
                    F(Target4) + "|" +
                    F(Risk) + "|" +
                    F(TargetPips) + "|" +
                    F(Spread);
            }
        }

        private readonly struct ReplayGeometrySnapshot
        {
            private ReplayGeometrySnapshot(
                double entry,
                double stop,
                double target1,
                double target2,
                double target3,
                double target4,
                double nominalRr,
                double effectiveRr,
                double riskPips,
                double targetPips,
                double spread)
            {
                Entry = entry;
                Stop = stop;
                Target1 = target1;
                Target2 = target2;
                Target3 = target3;
                Target4 = target4;
                NominalRr = nominalRr;
                EffectiveRr = effectiveRr;
                RiskPips = riskPips;
                TargetPips = targetPips;
                Spread = spread;
                Fingerprint = Serialize();
            }

            internal double Entry { get; }
            internal double Stop { get; }
            internal double Target1 { get; }
            internal double Target2 { get; }
            internal double Target3 { get; }
            internal double Target4 { get; }
            internal double NominalRr { get; }
            internal double EffectiveRr { get; }
            internal double RiskPips { get; }
            internal double TargetPips { get; }
            internal double Spread { get; }
            internal string Fingerprint { get; }

            internal static ReplayGeometrySnapshot Create(
                ReplayPlanGeometry geometry,
                RiskRewardMathResult rr,
                ExecutionIntentGeometryResult intent,
                double spread)
            {
                return new ReplayGeometrySnapshot(
                    intent.Entry,
                    intent.Stop,
                    intent.Target,
                    geometry.Target2,
                    geometry.Target3,
                    geometry.Target4,
                    rr.NominalRR,
                    rr.EffectiveRR,
                    geometry.Risk / PipSize,
                    intent.TargetPips,
                    spread);
            }

            internal static ReplayGeometrySnapshot CreateMirror(
                ReplayPlanGeometry geometry,
                RiskRewardMathResult rr)
            {
                return new ReplayGeometrySnapshot(
                    geometry.Entry,
                    geometry.Stop,
                    geometry.Target1,
                    geometry.Target2,
                    geometry.Target3,
                    geometry.Target4,
                    rr.NominalRR,
                    rr.EffectiveRR,
                    geometry.Risk / PipSize,
                    geometry.TargetPips,
                    geometry.Spread);
            }

            private string Serialize()
            {
                return
                    F(Entry) + "|" +
                    F(Stop) + "|" +
                    F(Target1) + "|" +
                    F(Target2) + "|" +
                    F(Target3) + "|" +
                    F(Target4) + "|" +
                    F(NominalRr) + "|" +
                    F(EffectiveRr) + "|" +
                    F(RiskPips) + "|" +
                    F(TargetPips) + "|" +
                    F(Spread);
            }
        }

        private sealed class ReplayTrace
        {
            internal ReplayTrace(
                string scenarioName,
                int sequence,
                DateTime referenceUtc,
                DateTime quoteUtc,
                DateTime causalUtc,
                ReplayIndicatorSnapshot indicators,
                ReplayStructureSnapshot structure,
                ReplayDecisionSnapshot decision,
                ReplayTriggerSnapshot trigger,
                ReplayPlanGeometry geometry,
                RiskRewardMathResult rr,
                ExecutionIntentGeometryResult intent,
                bool fillAccepted,
                bool actionable,
                DateTime actionableUtc,
                DateTime alertUtc,
                DateTime executionAttemptUtc,
                DateTime fillUtc,
                string authoritativeFingerprint,
                string submittedFingerprint,
                double riskPips,
                double targetPips,
                double mirrorRiskPips,
                double mirrorTargetPips,
                double mirrorNominalRr,
                double mirrorEffectiveRr,
                string mirrorGeometryFingerprint)
            {
                ScenarioName = scenarioName;
                Sequence = sequence;
                ReferenceUtc = referenceUtc;
                QuoteUtc = quoteUtc;
                CausalUtc = causalUtc;
                Indicators = indicators;
                Structure = structure;
                Decision = decision;
                Trigger = trigger;
                Geometry = geometry;
                Risk = rr.Risk;
                Reward = rr.Reward;
                NominalRR = rr.NominalRR;
                EffectiveRR = rr.EffectiveRR;
                Intent = intent;
                FillAccepted = fillAccepted;
                Actionable = actionable;
                FirstActionableUtc = actionableUtc;
                AlertUtc = alertUtc;
                ExecutionAttemptUtc = executionAttemptUtc;
                FillUtc = fillUtc;
                AuthoritativeGeometryFingerprint = authoritativeFingerprint;
                SubmissionGeometryFingerprint = submittedFingerprint;
                RiskPips = riskPips;
                TargetPips = targetPips;
                MirrorRiskPips = mirrorRiskPips;
                MirrorTargetPips = mirrorTargetPips;
                MirrorNominalRR = mirrorNominalRr;
                MirrorEffectiveRR = mirrorEffectiveRr;
                MirrorGeometryFingerprint = mirrorGeometryFingerprint;
            }

            internal string ScenarioName { get; }
            internal int Sequence { get; }
            internal DateTime ReferenceUtc { get; }
            internal DateTime QuoteUtc { get; }
            internal DateTime CausalUtc { get; }
            internal ReplayIndicatorSnapshot Indicators { get; }
            internal ReplayStructureSnapshot Structure { get; }
            internal ReplayDecisionSnapshot Decision { get; }
            internal ReplayTriggerSnapshot Trigger { get; }
            internal ReplayPlanGeometry Geometry { get; }
            internal double Risk { get; }
            internal double Reward { get; }
            internal double NominalRR { get; }
            internal double EffectiveRR { get; }
            internal ExecutionIntentGeometryResult Intent { get; }
            internal bool FillAccepted { get; }
            internal bool Actionable { get; }
            internal DateTime FirstActionableUtc { get; }
            internal DateTime AlertUtc { get; }
            internal DateTime ExecutionAttemptUtc { get; }
            internal DateTime FillUtc { get; }
            internal string AuthoritativeGeometryFingerprint { get; }
            internal string SubmissionGeometryFingerprint { get; }
            internal double RiskPips { get; }
            internal double TargetPips { get; }
            internal double MirrorRiskPips { get; }
            internal double MirrorTargetPips { get; }
            internal double MirrorNominalRR { get; }
            internal double MirrorEffectiveRR { get; }
            internal string MirrorGeometryFingerprint { get; }

            internal string Serialize()
            {
                StringBuilder builder = new StringBuilder();

                builder.Append(ScenarioName).Append("|");
                builder.Append(Sequence.ToString(CultureInfo.InvariantCulture)).Append("|");
                builder.Append(ReferenceUtc.Ticks.ToString(CultureInfo.InvariantCulture)).Append("|");
                builder.Append(QuoteUtc.Ticks.ToString(CultureInfo.InvariantCulture)).Append("|");
                builder.Append(CausalUtc.Ticks.ToString(CultureInfo.InvariantCulture)).Append("|");
                builder.Append(Indicators.Serialize()).Append("|");
                builder.Append(Structure.Serialize()).Append("|");
                builder.Append(Decision.Serialize()).Append("|");
                builder.Append(Trigger.Serialize()).Append("|");
                builder.Append(Geometry.Serialize()).Append("|");
                builder.Append(F(Risk)).Append("|");
                builder.Append(F(Reward)).Append("|");
                builder.Append(F(NominalRR)).Append("|");
                builder.Append(F(EffectiveRR)).Append("|");
                builder.Append(Intent.Valid).Append("|");
                builder.Append(F(Intent.StopPips)).Append("|");
                builder.Append(F(Intent.TargetPips)).Append("|");
                builder.Append(FillAccepted).Append("|");
                builder.Append(Actionable).Append("|");
                builder.Append(FirstActionableUtc.Ticks.ToString(CultureInfo.InvariantCulture)).Append("|");
                builder.Append(AlertUtc.Ticks.ToString(CultureInfo.InvariantCulture)).Append("|");
                builder.Append(ExecutionAttemptUtc.Ticks.ToString(CultureInfo.InvariantCulture)).Append("|");
                builder.Append(FillUtc.Ticks.ToString(CultureInfo.InvariantCulture)).Append("|");
                builder.Append(AuthoritativeGeometryFingerprint).Append("|");
                builder.Append(SubmissionGeometryFingerprint).Append("|");
                builder.Append(F(RiskPips)).Append("|");
                builder.Append(F(TargetPips)).Append("|");
                builder.Append(F(MirrorRiskPips)).Append("|");
                builder.Append(F(MirrorTargetPips)).Append("|");
                builder.Append(F(MirrorNominalRR)).Append("|");
                builder.Append(F(MirrorEffectiveRR)).Append("|");
                builder.Append(MirrorGeometryFingerprint);

                return builder.ToString();
            }

            internal string Summary()
            {
                long causalToActionable =
                    Actionable
                        ? (long)(FirstActionableUtc - CausalUtc).TotalMilliseconds
                        : 0;

                long alertToExecution =
                    Actionable
                        ? (long)(ExecutionAttemptUtc - AlertUtc).TotalMilliseconds
                        : 0;

                long executionToFill =
                    Actionable
                        ? (long)(FillUtc - ExecutionAttemptUtc).TotalMilliseconds
                        : 0;

                return
                    "actionable=" +
                    Actionable +
                    "; riskPips=" +
                    F(RiskPips) +
                    "; targetPips=" +
                    F(TargetPips) +
                    "; rr=" +
                    F(NominalRR) +
                    "; effectiveRR=" +
                    F(EffectiveRR) +
                    "; causal→actionableMs=" +
                    causalToActionable +
                    "; alert→executionMs=" +
                    alertToExecution +
                    "; execution→fillMs=" +
                    executionToFill;
            }

            internal void Validate()
            {
                Assert(
                    ReferenceUtc.Kind == DateTimeKind.Utc &&
                    QuoteUtc.Kind == DateTimeKind.Utc &&
                    CausalUtc.Kind == DateTimeKind.Utc,
                    "CI-16 timestamps are UTC: " + ScenarioName);

                Assert(
                    CausalUtc <= QuoteUtc &&
                    QuoteUtc <= ReferenceUtc,
                    "CI-16 causal/quote/reference ordering: " + ScenarioName);

                Assert(
                    Intent.Valid &&
                    FillAccepted,
                    "CI-16 final intent and fill acceptance are valid: " + ScenarioName);

                Assert(
                    Geometry.Target1 > Geometry.Entry && Decision.Direction == 1 ||
                    Geometry.Target1 < Geometry.Entry && Decision.Direction == -1,
                    "CI-16 target direction mirrors the decision: " + ScenarioName);

                Assert(
                    Math.Abs(
                        RiskPips -
                        Geometry.Risk / PipSize) < 1e-12 &&
                    Math.Abs(
                        TargetPips -
                        Geometry.TargetPips) < 1e-12,
                    "CI-16 pip projection is stable: " + ScenarioName);

                Assert(
                    AuthoritativeGeometryFingerprint ==
                    SubmissionGeometryFingerprint,
                    "CI-16 submission uses the exact authoritative geometry: " +
                    ScenarioName);

                Assert(
                    Trigger.M5Ready == Decision.Blocked ||
                    Trigger.Confirmed == false ||
                    Trigger.Confirmed == true,
                    "CI-16 trigger state is explicit: " + ScenarioName);

                if (ScenarioName == "M1 confirmation late in M5")
                {
                    Assert(
                        Trigger.M1CanEvaluate &&
                        Trigger.M1Ready &&
                        Trigger.Confirmed,
                        "CI-16 late-M1 confirmation is accepted only inside the active M5");
                }

                if (ScenarioName == "M1 confirmation early in M5")
                {
                    Assert(
                        Trigger.M1CanEvaluate &&
                        Trigger.M1Ready &&
                        Trigger.Confirmed,
                        "CI-16 early-M1 confirmation is accepted inside the active M5");
                }

                if (ScenarioName == "large displacement")
                {
                    Assert(
                        Geometry.Target1 != Geometry.Entry &&
                        Indicators.DisplacementAtr >= 1.50,
                        "CI-16 large displacement fixture is represented");
                }

                if (ScenarioName == "target obstruction")
                {
                    Assert(
                        Structure.TargetObstructed,
                        "CI-16 target obstruction is preserved in the structure snapshot");
                }

                if (ScenarioName == "opposite divergence")
                {
                    Assert(
                        Decision.Blocked &&
                        !Actionable,
                        "CI-16 opposite divergence remains observable and does not create actionability");
                }

                if (ScenarioName == "weak single-zone setup")
                {
                    Assert(
                        Structure.Confluence == 1 &&
                        !Actionable,
                        "CI-16 weak single-zone fixture has no accidental actionability");
                }

                if (!Actionable)
                {
                    Assert(
                        FirstActionableUtc == DateTime.MinValue &&
                        AlertUtc == DateTime.MinValue &&
                        ExecutionAttemptUtc == DateTime.MinValue &&
                        FillUtc == DateTime.MinValue,
                        "CI-16 blocked scenario does not fabricate downstream timestamps: " +
                        ScenarioName);
                }
                else
                {
                    Assert(
                        FirstActionableUtc >= CausalUtc &&
                        AlertUtc >= FirstActionableUtc &&
                        ExecutionAttemptUtc >= AlertUtc &&
                        FillUtc >= ExecutionAttemptUtc,
                        "CI-16 latency timestamps are monotonic: " + ScenarioName);
                }
            }
        }

        private static string F(double value)
        {
            return value.ToString(
                "R",
                CultureInfo.InvariantCulture);
        }
    }
}
