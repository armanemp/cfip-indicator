#!/usr/bin/env python3

from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]

required = {
    "src/CFIP.Indicator/Indicator/CFIPIndicator.cs":
        ["AccessRights.None"],
    "src/CFIP.Indicator/Indicator/Parameters/28_news_guard.cs":
        [
            "EnableEconomicNewsCalendar",
            "EconomicNewsDataUri",
            "HighImpactNewsMinutesBefore",
            "HighImpactNewsMinutesAfter",
            "CancelPendingBeforeHighImpactNews",
            "CloseActiveBeforeHighImpactNews",
        ],
    "src/CFIP.Indicator/Trading/Intelligence/EconomicNewsCalendarClient.cs":
        [
            "RefreshEconomicNewsIfNeeded",
        ],
    "src/CFIP.Indicator/Trading/Intelligence/EconomicNewsRiskEvaluator.cs":
        [
            "FindBlockingNewsEvent",
            "NewsBlocked",
        ],
    "src/CFIP.Indicator/Trading/Intelligence/EconomicNewsProtection.cs":
        [
            "ArchiveEconomicNewsRisk",
            "ApplyEconomicNewsRiskProtection",
        ],
    "src/CFIP.Indicator/Analysis/Market/Decision/DecisionMarketGates.cs":
        ["NewsBlocked("],
    "src/CFIP.Indicator/Trading/Risk/AutoTradeSafetyGuard.cs":
        ["NewsBlocked("],
    "src/CFIP.Indicator/Runtime/Supervision/RuntimePanelHeartbeat.cs":
        ["RefreshEconomicNewsIfNeeded("],
    "src/CFIP.Indicator/Runtime/Calculation/CalculationStageIsolation.cs":
        ["ApplyEconomicNewsRiskProtection("],
    "src/CFIP.Indicator/Trading/Intelligence/EconomicNewsProtection.cs":
        ['"NEWS_RISK"'],
}

errors = []

for relative, needles in required.items():
    path = ROOT / relative
    if not path.exists():
        errors.append(f"missing required file: {relative}")
        continue

    text = path.read_text(encoding="utf-8")
    for needle in needles:
        if needle not in text:
            errors.append(
                f"{relative}: missing required news contract: {needle}"
            )

if errors:
    print("NEWS GUARD AUDIT: FAIL")
    for error in errors:
        print(f"  - {error}")
    raise SystemExit(1)

print("NEWS GUARD AUDIT: PASS")
for relative in required:
    print(f"  - {relative}")
