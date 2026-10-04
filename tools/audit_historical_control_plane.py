from pathlib import Path

ROOT = Path(".")
LEGACY = "docs/ROADMAP.md"
ARCHIVE = Path("docs/archive/ROADMAP-LEGACY-2026-10-04.md")
CANONICAL = [Path("docs/CFIP-PROMPT.md"), Path("docs/CFIP-ROADMAP.md"), Path("docs/CFIP-LIST.md"), Path("docs/CFIP_GATE.md")]

if Path(LEGACY).exists():
    raise SystemExit("Superseded active roadmap still exists at docs/ROADMAP.md")
if not ARCHIVE.exists():
    raise SystemExit("Historical roadmap archive is missing")
for path in CANONICAL:
    if not path.exists():
        raise SystemExit(f"Canonical control-plane file is missing: {path}")

violations = []
for path in ROOT.rglob("*"):
    if (not path.is_file() or ".git" in path.parts or path == ARCHIVE or path == Path("tools/audit_historical_control_plane.py") or path in CANONICAL or path == Path("docs/CFIP-PREPROMPT.md")):
        continue
    try:
        text = path.read_text(encoding="utf-8")
    except (UnicodeDecodeError, OSError):
        continue
    if LEGACY in text:
        violations.append(str(path).replace("\\", "/"))

if violations:
    raise SystemExit("Active legacy roadmap references remain: " + ", ".join(sorted(violations)))

print("Historical control-plane isolation: PASS")
print("Canonical files: PASS")
print("Legacy active roadmap: absent")
print("Legacy reference scan: PASS")

# Regression guard: active legacy roadmap references are forbidden outside the archive.
