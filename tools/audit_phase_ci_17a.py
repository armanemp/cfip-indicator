from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]


def read(relative: str) -> str:
    path = ROOT / relative
    if not path.exists():
        raise SystemExit(f"Missing panel refresh source: {relative}")
    return path.read_text(encoding="utf-8")


content = read("src/CFIP.Indicator/UI/Panel/PanelContentRefresh.cs")
heartbeat = read("src/CFIP.Indicator/Runtime/Supervision/RuntimePanelHeartbeat.cs")
main = read("src/CFIP.Indicator/UI/Panel/PanelMainRenderer.cs")
state = read("src/CFIP.Indicator/Indicator/State.cs")
visibility = read("src/CFIP.Indicator/UI/Panel/PanelVisibility.cs")
live = read("src/CFIP.Indicator/Runtime/Supervision/PanelHeartbeatLiveState.cs")
runtime = read("tools/CFIP.Runtime.Contracts/Program.cs")
workflow = read(".github/workflows/source-check.yml")
roadmap = read("docs/ROADMAP.md")
continuation = read("docs/CONTINUATION-STATE.md")
development = read("docs/DEVELOPMENT-LOG.md")

checks = (
    (
        "bounded 500ms content refresh exists",
        "PanelContentRefreshMilliseconds = 500" in content and
        "ShouldRefreshPanelContent(" in content and
        "RenderPanelRows(" in content,
    ),
    (
        "content refresh never calls full layout renderer",
        "RenderPanel();" not in content,
    ),
    (
        "heartbeat invokes content refresh",
        "RefreshPanelContentIfDue(" in heartbeat,
    ),
    (
        "content refresh owns a dedicated timestamp",
        "_lastPanelContentRefreshUtc" in state and
        "_lastPanelContentRefreshUtc" in content,
    ),
    (
        "full layout remains state-key optimized",
        "ShouldRenderFullPanel(" in main and
        "BuildPanelPresentationKey(" in main,
    ),
    (
        "full render marks content refresh current",
        "_lastPanelContentRefreshUtc =" in main,
    ),
    (
        "restore forces immediate content refresh",
        "_lastPanelContentRefreshUtc =\n                                            DateTime.MinValue" in visibility,
    ),
    (
        "live RR reads current quote",
        "double liveMarket =" in live and
        "Symbol.Bid" in live and
        "Symbol.Ask" in live and
        "_lastMarket = liveMarket" in live,
    ),
    (
        "runtime contract is accumulated",
        "VerifyPanelLiveContentRefresh();" in runtime and
        "python tools/audit_phase_ci_17a.py" in workflow,
    ),
    (
        "continuity records CI-17A",
        "CI-17A" in roadmap and
        "CI-17A" in continuation and
        "CI-17A" in development,
    ),
)

for name, condition in checks:
    print(f"{'PASS' if condition else 'FAIL'} | {name}")
    if not condition:
        raise SystemExit("CI-17A audit failed: " + name)

print("CI-17A panel live-content refresh audit PASS")
