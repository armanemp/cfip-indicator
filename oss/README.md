# CFIP OSS Boundary

Third-party/open-source components are admitted only through narrow adapters or
offline benchmark projects.

## Production

The indicator currently uses one Apache-2.0 numerical dependency:

- Skender.Stock.Indicators 2.7.3

The strategy feature is optional at runtime, but the package remains a
build/runtime dependency because the adapter code is compiled into the
production assembly.

## Research

FacioQuo.Stock.Indicators 3.0.1 is benchmark-only because the package targets
.NET 8, .NET 9 and .NET 10 while the production cTrader target remains .NET 6.

TA-Lib.NETCore and QuantConnect LEAN remain benchmark/reference candidates only.

Every admitted component must document:

- upstream repository;
- exact version/tag/commit;
- license and attribution;
- files adapted or referenced;
- dependency tree;
- cTrader/.NET compatibility;
- reason for adoption;
- numerical fixtures;
- benchmark results;
- isolated adapter owner.

OSS components never own CFIP structure, decision, risk, order placement, broker
mutation or lifecycle authority.
