#!/usr/bin/env python3
"""Machine-enforced public-parameter count and documentation consistency audit."""
from pathlib import Path
import re
import sys

ROOT = Path(__file__).resolve().parents[1]
PARAM_ROOT = ROOT / "src" / "CFIP.Indicator" / "Indicator" / "Parameters"
README = ROOT / "README.md"

PARAM_RE = re.compile(
    r'\[Parameter\s*\([^\]]*\]\s*'
    r'public\s+[A-Za-z_][\w<>\[\],.?]*\s+'
    r'([A-Za-z_]\w*)\s*\{\s*get;\s*set;\s*\}',
    re.S,
)
DOC_RE = re.compile(r"\b(\d+) configuration parameters are currently exposed\b")

def main() -> int:
    declaration_locations = {}
    duplicates = {}

    for path in sorted(PARAM_ROOT.glob("*.cs")):
        source = path.read_text(encoding="utf-8")
        for match in PARAM_RE.finditer(source):
            name = match.group(1)
            location = str(path.relative_to(ROOT)).replace("\\", "/")
            if name in declaration_locations:
                duplicates.setdefault(name, [declaration_locations[name]]).append(location)
            else:
                declaration_locations[name] = location

    count = len(declaration_locations)
    parameter_files = sorted(PARAM_ROOT.glob("*.cs"))
    documented_match = DOC_RE.search(README.read_text(encoding="utf-8"))

    print("PUBLIC PARAMETER COUNT AUDIT")
    print("=" * 72)
    print(f"Parameter source files: {len(parameter_files)}")
    print(f"Unique public parameters: {count}")

    if duplicates:
        print("DUPLICATES:")
        for name, owners in sorted(duplicates.items()):
            print(f"- {name} :: {", ".join(owners)}")
        return 1

    if documented_match is None:
        print("FAIL — README parameter-count statement is missing.")
        return 1

    documented = int(documented_match.group(1))
    print(f"README documented count: {documented}")
    if documented != count:
        print(f"FAIL — README count {documented} does not match machine-derived count {count}.")
        return 1

    if count == 0:
        print("FAIL — no public parameters discovered.")
        return 1

    print("PASS — README count matches the machine-derived public-parameter inventory.")
    return 0

if __name__ == "__main__":
    sys.exit(main())
