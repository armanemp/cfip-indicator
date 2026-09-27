# FacioQuo Stock Indicators benchmark

This is a research/validation harness, not a second CFIP trading engine.

It evaluates the current FacioQuo.Stock.Indicators 3.0.1 library against CFIP fixtures. Production cTrader code remains the authoritative decision and execution path.

The benchmark targets .NET 8 because the current v3 package line targets newer .NET runtimes than the net6 cTrader production target.

Promotion into production requires numerical fixture validation, performance measurement, license review and an adapter boundary.
