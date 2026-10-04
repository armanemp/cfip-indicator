from pathlib import Path
import re

ROOT = Path(__file__).resolve().parents[1]
DOC = ROOT / "docs/PHASE-CI-17-TARGET-TERMINAL-VALIDATION.md"

REQUIRED = [
    "M1 probe",
    "M5 probe",
    "history reload",
    "restart/reconnect",
    "BUY market",
    "SELL market",
    "BUY STOP",
    "SELL STOP",
    "BUY LIMIT",
    "SELL LIMIT",
    "cancellation/expiration",
    "panel/chart responsiveness",
    "signal/plan synchronization",
]

def fail(message):
    raise SystemExit("CI-17 evidence guard failed: " + message)

text = DOC.read_text(encoding="utf-8")
table_match = re.search(
    r"## Manual evidence table\n\n(?P<table>\| Test \| Evidence \| Result \|.*?)(?=\n## Boundary)",
    text,
    re.DOTALL,
)
if not table_match:
    fail("manual evidence table is missing or malformed")

rows = []
for line in table_match.group("table").splitlines():
    if not line.startswith("|") or line.startswith("|---"):
        continue
    cells = [cell.strip() for cell in line.strip("|").split("|")]
    if len(cells) == 3 and cells[0] != "Test":
        rows.append(cells)

if len(rows) != len(REQUIRED):
    fail(f"expected {len(REQUIRED)} evidence rows, found {len(rows)}")

seen = []
for test, evidence, result in rows:
    seen.append(test)
    if test not in REQUIRED:
        fail(f"unexpected test row: {test}")
    if result not in {"PENDING", "PASS"}:
        fail(f"{test} has unsupported result '{result}'")
    if result == "PASS":
        if not evidence or evidence.lower() in {"pending", "tbd", "todo", "n/a", "-"}:
            fail(f"{test} is marked PASS without concrete evidence reference")
        if "terminal log" in evidence.lower() and evidence.lower().strip() == "terminal log":
            fail(f"{test} is marked PASS with placeholder evidence")
        
if seen != REQUIRED:
    fail("manual evidence rows are out of canonical order or duplicated")

pending = sum(result == "PENDING" for _, _, result in rows)
passed = len(rows) - pending

if passed == 0:
    print(f"CI-17 target-terminal evidence guard PASS — {pending}/{len(rows)} rows remain PENDING")
else:
    print(f"CI-17 target-terminal evidence guard PASS — {passed}/{len(rows)} rows have non-placeholder PASS evidence; {pending} remain PENDING")
