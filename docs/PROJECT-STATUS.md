# CFIP Indicator — Project Status

Date: 2026-09-27

## Migration wave 1

Status: IMPLEMENTED / ARCHITECTURAL HARDENING IN PROGRESS

The complete top-level v89 implementation surface has been transferred into the new repository as domain-oriented source files. The former single cTrader host class is represented by responsibility-based partial files.

Current migration inventory:
- 98 C# source files under src/CFIP.Indicator.
- 512 public cTrader parameters preserved.
- 7 cTrader host partial files.
- Core, Market, Analysis/Structure, Decision, Planning, Risk, Execution, CTrader infrastructure, Lifecycle, LiveManagement, Outcomes, Presentation and Configuration modules present.

## Important distinction

This is not yet the final clean-architecture release.

The current wave preserves the v89 type contracts and behavior while establishing physical module ownership. The next hardening wave will:
- enforce namespace and dependency direction;
- introduce interface boundaries where broker/platform dependencies remain;
- split reusable domain/application projects;
- add unit and architecture tests;
- remove remaining migration-era coupling;
- validate against the actual cTrader compiler/runtime.

No real broker execution or cTrader runtime acceptance is claimed by the source migration alone.
