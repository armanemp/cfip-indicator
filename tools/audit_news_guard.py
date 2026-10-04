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
            "NewsRefreshMinutes",
            "Additional News Currencies / Symbol Map",
            "US30=USD",
            "HighImpactNewsMinutesBefore",
            "HighImpactNewsMinutesAfter",
            "CancelPendingBeforeHighImpactNews",
            "CloseActiveBeforeHighImpactNews",
            "NewsFailClosedWhenStale",
            "MaximumNewsFeedAgeMinutes",
        ],
    "src/CFIP.Indicator/Core/Math/EconomicNewsFeedStateRule.cs":
        [
            "EconomicNewsFeedState",
            "NeverLoaded",
            "Healthy",
            "Stale",
            "BlockingEvent",
            "Evaluate(",
        ],
    "src/CFIP.Indicator/Core/Math/EconomicNewsCurrencyRule.cs":
        [
            "NormalizeSymbol(",
            "symbolCurrencyMap",
            "AddSymbolMapMatches",
        ],
    "src/CFIP.Indicator/Trading/Intelligence/EconomicNewsCalendarClient.cs":
        [
            "RefreshEconomicNewsIfNeeded",
            "Http.SendAsync(",
            "EconomicNewsCalendarParser.Parse(",
            "EconomicNewsFeedCoordinator.TryStart(",
            "EconomicNewsFeedCoordinator.CompleteSuccess(",
            "EconomicNewsFeedCoordinator.CompleteFailure(",
            "_economicNewsRequestInFlight",
            "_economicNewsRequestGeneration",
        ],
    "src/CFIP.Indicator/Trading/Intelligence/EconomicNewsFeedCoordinator.cs":
        [
            "ProviderMinimumRefreshMinutes",
            "TryReadEconomicNewsSnapshot(",
            "TryStart(",
            "CompleteSuccess(",
            "CompleteFailure(",
            "RequestInFlight",
        ],
    "src/CFIP.Indicator/Trading/Intelligence/EconomicNewsCalendarParser.cs":
        [
            "NormalizePayloadPrefix(",
            "StartsWith(",
            "NEWS FEED PAYLOAD FORMAT FAILURE",
            "TryParseEconomicEventTime(",
        ],
    "src/CFIP.Indicator/Trading/Intelligence/EconomicNewsRiskEvaluator.cs":
        [
            "EconomicNewsFeedStateRule.Evaluate(",
            "NeverLoaded",
            "Stale",
            "NewsBlocked(",
        ],
    "src/CFIP.Indicator/Trading/Intelligence/EconomicNewsProtection.cs":
        [
            "ArchiveEconomicNewsRisk",
            "ApplyEconomicNewsRiskProtection",
            '"NEWS_RISK"',
        ],
    "src/CFIP.Indicator/Analysis/Market/Decision/DecisionMarketGates.cs":
        ["NewsBlocked("],
    "src/CFIP.Indicator/Trading/Risk/AutoTradeSafetyGuard.cs":
        ["NewsBlocked("],
    "src/CFIP.Indicator/Runtime/Supervision/RuntimePanelHeartbeat.cs":
        ["RefreshEconomicNewsIfNeeded("],
}

errors = []


def read(relative: str) -> str:
    path = ROOT / relative
    if not path.exists():
        errors.append(f"missing required file: {relative}")
        return ""
    return path.read_text(encoding="utf-8")


for relative, needles in required.items():
    source = read(relative)
    for needle in needles:
        if needle not in source:
            errors.append(
                f"{relative}: missing required news contract: {needle}"
            )

calendar = read(
    "src/CFIP.Indicator/Trading/Intelligence/EconomicNewsCalendarClient.cs"
)
initialization = read(
    "src/CFIP.Indicator/Runtime/Initialization/RuntimeInitialization.cs"
)

if "Http.Get(" in calendar or "Http.Send(" in calendar:
    errors.append(
        "EconomicNewsCalendarClient.cs: synchronous HTTP API usage remains"
    )

coordinator = read(
    "src/CFIP.Indicator/Trading/Intelligence/EconomicNewsFeedCoordinator.cs"
)
if "TryGetSnapshot(" in coordinator:
    errors.append(
        "EconomicNewsFeedCoordinator.cs: generic snapshot owner name is forbidden"
    )
if "TryReadEconomicNewsSnapshot(" not in coordinator:
    errors.append(
        "EconomicNewsFeedCoordinator.cs: canonical snapshot owner is missing"
    )

parameter_source = read(
    "src/CFIP.Indicator/Indicator/Parameters/28_news_guard.cs"
)
if "DefaultValue = 5, MinValue = 5, MaxValue = 60" not in parameter_source:
    errors.append(
        "News Refresh Minutes must enforce the provider-safe 5-minute minimum"
    )

if "RefreshEconomicNewsIfNeeded(" in initialization:
    errors.append(
        "RuntimeInitialization.cs: news refresh must not run during initialization"
    )

# No production source may retain a synchronous news HTTP mutation.
for path in (ROOT / "src").rglob("*.cs"):
    source = path.read_text(encoding="utf-8")
    if "Http.Get(" in source or "Http.Send(" in source:
        errors.append(
            f"{path.relative_to(ROOT)}: synchronous Http.Get/Http.Send is forbidden"
        )

if errors:
    print("NEWS GUARD AUDIT: FAIL")
    for error in errors:
        print(f"  - {error}")
    raise SystemExit(1)

print("NEWS GUARD AUDIT: PASS")
for relative in required:
    print(f"  - {relative}")
