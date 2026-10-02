from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]

def read(path):
    return (ROOT / path).read_text(encoding="utf-8")

def check(condition, message):
    if not condition:
        raise SystemExit("CI-16 audit failed: " + message)

suite = read("tools/CFIP.Runtime.Contracts/DeterministicReplaySuite.cs")
program = read("tools/CFIP.Runtime.Contracts/Program.cs")
runtime_project = read("tools/CFIP.Runtime.Contracts/CFIP.Runtime.Contracts.csproj")
workflow = read(".github/workflows/source-check.yml")
roadmap = read("docs/ROADMAP.md")
continuation = read("docs/CONTINUATION-STATE.md")

required_scenarios = (
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
    "mirrored BUY/SELL",
)

required_fields = (
    "ReferenceUtc",
    "CausalUtc",
    "QuoteUtc",
    "Indicators",
    "Structure",
    "Decision",
    "Trigger",
    "Geometry",
    "Risk",
    "Reward",
    "NominalRR",
    "EffectiveRR",
    "FirstActionableUtc",
    "AlertUtc",
    "ExecutionAttemptUtc",
    "FillUtc",
)

check(
    "VerifyCi16DeterministicReplay();" in program and
    "private static void VerifyCi16DeterministicReplay()" in program,
    "Runtime Contracts Program invokes the CI-16 suite",
)

check(
    "ExecutionIntentGeometryRule.cs" in runtime_project,
    "Runtime Contracts project includes the canonical final-intent geometry owner",
)

check(
    all(name in suite for name in required_scenarios),
    "all required CI-16 counterexample scenarios are present",
)

check(
    all(name in suite for name in required_fields),
    "replay trace records all required fields",
)

check(
    "IReadOnlyList<ReplayTrace> first" in suite and
    "IReadOnlyList<ReplayTrace> second" in suite and
    "first[i].Serialize() == second[i].Serialize()" in suite,
    "deterministic replay is executed twice and compared",
)

check(
    "AuthoritativeGeometryFingerprint" in suite and
    "SubmissionGeometryFingerprint" in suite and
    "AuthoritativeGeometryFingerprint ==" in suite,
    "authoritative plan geometry is compared with execution submission geometry",
)

check(
    "causalToActionable" in suite and
    "alertToExecution" in suite and
    "executionToFill" in suite,
    "latency intervals are explicitly measurable",
)

check(
    "audit_phase_ci_15.py" in workflow and
    "audit_phase_ci_16.py" in workflow and
    workflow.index("audit_phase_ci_16.py") > workflow.index("audit_phase_ci_15.py"),
    "CI-16 audit is accumulated immediately after CI-15",
)

check(
    "CI-16" in roadmap and
    "CI-17" in roadmap and
    "CI-FINAL" in roadmap,
    "roadmap retains CI-16 → CI-17 → CI-FINAL sequence",
)

check(
    "Current implementation phase:" not in continuation or
    "CI-17" in continuation.split("Current implementation phase:", 1)[-1],
    "continuation state advances to CI-17 after CI-16 closeout",
)

print("CI-16 static audit PASS")
