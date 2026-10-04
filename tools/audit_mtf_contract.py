from pathlib import Path

ROOTS = (Path("src/CFIP.Indicator"), Path("src/CFIP.Contracts"), Path("src/CFIP.cBot"))
FORBIDDEN = (
    "TimeFrame.Minute2", "M2PrecisionRule", "M2PrecisionSnapshot",
    "_m2Frame", "MICRO-ALIGNED", "MICRO-CONFLICT",
)
EXPECTED_RUNTIME_FRAMES = (
    "TimeFrame.Minute", "TimeFrame.Minute5", "TimeFrame.Minute15",
    "TimeFrame.Minute30", "TimeFrame.Hour", "TimeFrame.Hour4",
    "TimeFrame.Daily", "TimeFrame.Weekly",
)

def fail(message: str) -> None:
    raise SystemExit(message)

files = sorted({p for root in ROOTS for p in root.rglob("*.cs")})
if not files:
    fail("No production C# files found")

hits=[]
for path in files:
    source=path.read_text(encoding="utf-8")
    for token in FORBIDDEN:
        if token in source:
            hits.append((str(path), token))
if hits:
    for path, token in hits:
        print(f"FORBIDDEN M2 TIMEFRAME TOKEN: {token} -> {path}")
    fail(f"M2/2-minute production timeframe reintroduced: {len(hits)} hit(s)")

runtime=Path("src/CFIP.Indicator/Runtime/Initialization/RuntimeInitialization.cs").read_text(encoding="utf-8")
for token in EXPECTED_RUNTIME_FRAMES:
    if token not in runtime:
        fail(f"Canonical timeframe missing from runtime initialization: {token}")

panel=Path("src/CFIP.Indicator/UI/Panel/Rows/PanelContextRowsRenderer.cs").read_text(encoding="utf-8")
for label in ("M1","M5","M15","M30","H1","H4"):
    if label not in panel:
        fail(f"Canonical panel timeframe missing: {label}")
if "M2" in panel:
    fail("M2 panel presentation still exists")

print("M2 timeframe contract audit PASS")
print(f"Production C# files scanned: {len(files)}")
print("Allowed production MTF set: M1/M5/M15/M30/H1/H4/D1/W1")
print("M2 / 2-minute timeframe: ABSENT")
