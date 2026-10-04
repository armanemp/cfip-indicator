#!/usr/bin/env python3
"""MTF-P3 static acceptance gate: provider scenario/source-timeframe identity cohesion."""
from pathlib import Path
import re

ROOT = Path(__file__).resolve().parents[1]

def read(rel):
    path = ROOT / rel
    if not path.exists():
        raise SystemExit("MTF-P3 audit failed: missing " + rel)
    return path.read_text(encoding="utf-8")

def require(cond, msg):
    if not cond:
        raise SystemExit("MTF-P3 audit failed: " + msg)

provider = read("src/CFIP.Indicator/Runtime/Provider/CFIPReadOnlyProviderRefresh.cs")
intent = read("src/CFIP.Indicator/Runtime/Provider/CFIPReadOnlyProviderPlan.cs")
identity = read("src/CFIP.Indicator/Core/Math/ProviderScenarioIdentityRule.cs")
pending_limit = read("src/CFIP.Indicator/Trading/Pending/Placement/ReversalLimitPlacement.cs")
pending_stop = read("src/CFIP.Indicator/Trading/Pending/Placement/ContinuationStopPlacement.cs")
runtime = read("tools/CFIP.Runtime.Contracts/Program.cs")
runtime_csproj = read("tools/CFIP.Runtime.Contracts/CFIP.Runtime.Contracts.csproj")
workflow = read(".github/workflows/source-check.yml")
phase = read("docs/PHASE-MTF-P3-PRIMARY-PROVIDER-IDENTITY.md")

require(
    "ProviderScenarioIdentityRule.ResolveSourceTimeframe" in provider and
    "_tradePlanRegistry.TryGetCandidate" in provider,
    "provider must map envelope source timeframe from the exact scenario candidate",
)
require(
    "ScenarioExecutionPolicyRule.TryResolvePlanScenario" in provider and
    'ResolveProviderScenarioId(' in provider,
    "provider plan scenario identity must reuse the canonical scenario resolver",
)
require(
    "sourceTimeframe" in provider and
    "ProviderScenarioIdentityRule.ResolveSourceTimeframe(" in intent,
    "execution intent identity must use the provider-resolved source timeframe",
)
require(
    "ResolveScenarioId" in identity and
    "ResolveSourceTimeframe" in identity and
    'CanonicalM5 = "M5"' in identity,
    "shared provider identity rule must be deterministic and canonical",
)
require(
    "ResolveDirectionExecutionScenarioId" in pending_limit and
    "_activeExecutionScenarioId" in pending_limit and
    "PENDING-LIMIT-" not in pending_limit,
    "pending-limit execution must use the canonical scenario identity",
)
require(
    "ResolveDirectionExecutionScenarioId" in pending_stop and
    "_activeExecutionScenarioId" in pending_stop and
    "PENDING-STOP-" not in pending_stop,
    "pending-stop execution must use the canonical scenario identity",
)
require(
    "VerifyMtfPrimaryProviderIdentity();" in runtime,
    "runtime contracts must accumulate the MTF-P3 provider identity contract",
)
require(
    "ProviderScenarioIdentityRule.cs" in runtime_csproj,
    "runtime contract project must compile the provider identity rule",
)
require(
    "tools/audit_phase_mtf_primary_provider_identity.py" in workflow and
    "tools/audit_phase_mtf_primary_location_obfvg.py" in workflow,
    "MTF-P3 audit must accumulate after MTF-P2",
)
parameter_count = sum(
    len(re.findall(r"\[Parameter\s*\(", p.read_text(encoding="utf-8")))
    for p in (ROOT / "src/CFIP.Indicator/Indicator/Parameters").glob("*.cs")
)
require(
    parameter_count == 547,
    f"public parameter contract changed: found {parameter_count}",
)
require(
    "No public parameter" in phase and
    "execution authority" in phase.lower() and
    "threshold" in phase.lower(),
    "phase record must document non-change of parameters, thresholds and broker authority",
)

print("MTF-P3 provider scenario/source-timeframe identity audit PASS")
