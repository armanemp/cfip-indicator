#!/usr/bin/env python3
"""Static contract audit for the repository GitHub Actions workflows."""

from pathlib import Path
import re
import sys

ROOT = Path(__file__).resolve().parents[1]
WORKFLOW_DIR = ROOT / ".github" / "workflows"
EXPECTED = {"ci-build.yml", "source-check.yml", "runtime-acceptance.yml", "oss-benchmark.yml"}

def fail(message: str) -> None:
    print(f"FAIL: {message}")
    sys.exit(1)

def require(text: str, needle: str, label: str) -> None:
    if needle not in text:
        fail(f"{label}: missing {needle!r}")

def main() -> int:
    actual = {p.name for p in WORKFLOW_DIR.glob("*.yml")}
    if actual != EXPECTED:
        fail(f"workflow set drift: expected {sorted(EXPECTED)}, found {sorted(actual)}")
    for name in sorted(EXPECTED):
        text = (WORKFLOW_DIR / name).read_text(encoding="utf-8")
        require(text, "permissions:\n  contents: read", name)
        require(text, "concurrency:\n  group: ${{ github.workflow }}-${{ github.ref }}", name)
        require(text, "cancel-in-progress: true", name)
        require(text, "timeout-minutes:", name)
        require(text, "uses: actions/checkout@v4", name)
        if "pull_request_target:" in text or "workflow_run:" in text:
            fail(f"{name}: unsafe/unapproved trigger present")
        if re.search(r"continue-on-error:\s*true", text):
            fail(f"{name}: continue-on-error bypasses failure semantics")
    for name in ("ci-build.yml", "source-check.yml", "runtime-acceptance.yml"):
        text = (WORKFLOW_DIR / name).read_text(encoding="utf-8")
        require(text, "pull_request:", name)
        require(text, "push:\n    branches: [ main ]", name)
    benchmark = (WORKFLOW_DIR / "oss-benchmark.yml").read_text(encoding="utf-8")
    require(benchmark, "set -o pipefail", "oss-benchmark.yml")
    require(benchmark, "uses: actions/upload-artifact@v4", "oss-benchmark.yml")
    require(benchmark, "if-no-files-found: ignore", "oss-benchmark.yml")
    source = (WORKFLOW_DIR / "source-check.yml").read_text(encoding="utf-8")
    require(source, "python tools/audit_github_workflows.py", "source-check.yml")
    print(f"PASS: GitHub workflow contract audit ({len(EXPECTED)} workflows)")
    return 0

if __name__ == "__main__":
    raise SystemExit(main())
