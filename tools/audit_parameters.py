from pathlib import Path
import re

ROOT = Path("src/CFIP.Indicator")
PARAMETER_ROOT = ROOT / "Indicator" / "Parameters"

files = sorted(ROOT.rglob("*.cs"))
parameter_files = sorted(PARAMETER_ROOT.glob("*.cs"))

PARAM_RE = re.compile(
    r'\[Parameter\s*\([^\]]*\]\s*'
    r'public\s+[A-Za-z_][\\w<>\[\],.?]*\s+'
    r'([A-Za-z_]\\w*)\s*\{\s*get;\\s*set;\s*\}',
    re.S,
)

def strip_non_code(source: str) -> str:
    source = re.sub(r"/\*[\s\S]*?\*/", " ", source)
    source = re.sub(r"//[^\r\n]*", " ", source)
    source = re.sub(r'@?"(?:""|\\.|[^"\\])*"', "S", source)
    source = re.sub(r'"(?:\\.|[^"\\])*"', "S", source)
    return source

parameter_defs = {}
for path in parameter_files:
    raw = path.read_text(encoding="utf-8")
    clean = strip_non_code(raw)
    for match in PARAM_RE.finditer(clean):
        name = match.group(1)
        parameter_defs[name] = str(path.relative_to(ROOT)).replace("\\", "/")

if not parameter_defs:
    raise SystemExit("No public parameters discovered")

source_texts = {}
for path in files:
    source_texts[path] = strip_non_code(path.read_text(encoding="utf-8"))

unused = []
multi = []
for name, owner in sorted(parameter_defs.items()):
    hits = []
    pattern = re.compile(rf"\b{re.escape(name)}\b")
    for path, source in source_texts.items():
        for match in pattern.finditer(source):
            line = source.count("\n", 0, match.start()) + 1
            hits.append((str(path.relative_to(ROOT)).replace("\\", "/"), line))
    non_owner_hits = [
        hit for hit in hits
        if hit[0] != owner
    ]
    if not non_owner_hits:
        unused.append((name, owner))
    elif len(non_owner_hits) > 1:
        multi.append((name, owner, non_owner_hits))

print(f"Parameter declarations: {len(parameter_defs)}")
print(f"Read-by-code candidates: {len(parameter_defs) - len(unused)}")
print(f"Unused/unread candidates: {len(unused)}")

if unused:
    print("\nUNUSED/UNREAD PARAMETERS")
    for name, owner in unused:
        print(f"- {name} :: {owner}")
    raise SystemExit(
        "Dead/unread public parameters detected; every retained parameter needs an explicit runtime owner."
    )

print("\nParameter audit PASS: every declared public parameter is referenced outside its declaration file.")
