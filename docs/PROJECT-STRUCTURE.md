# CFIP Indicator — Target Project Structure

The project is organized by responsibility rather than arbitrary file size.

```text
cfip-indicator/
├── CFIP.Indicator.sln
├── global.json
├── Directory.Build.props
├── README.md
├── .gitignore
├── .vscode/
├── docs/
│   ├── ROADMAP.md
│   ├── ARCHITECTURE.md
│   ├── WORKFLOW.md
│   ├── MIGRATION.md
│   ├── V89-TOPLEVEL-INVENTORY.md
│   ├── PARAMETER-INVENTORY.md
│   ├── CAPABILITY-TRACEABILITY.md
│   ├── MODULE-MAP.md
│   ├── ACCEPTANCE-MATRIX.md
│   ├── TRADING-SAFETY-MATRIX.md
│   └── phases/
├── src/
│   └── CFIP.Indicator/
│       ├── Core/
│       ├── Market/
│       ├── Analysis/
│       │   └── Structure/
│       ├── Decision/
│       │   └── Contracts/
│       ├── Planning/
│       │   └── Contracts/
│       ├── Risk/
│       ├── Execution/
│       │   └── Contracts/
│       ├── Infrastructure/
│       │   └── CTrader/
│       │       └── Contracts/
│       ├── Lifecycle/
│       ├── LiveManagement/
│       ├── Outcomes/
│       │   └── Contracts/
│       ├── Presentation/
│       │   └── Contracts/
│       ├── Configuration/
│       └── Indicator/
└── tools/
    └── CFIP.StaticVerifier/
```

## Current migration state

The modules remain in one deployable cTrader project to minimize migration risk. Core contracts are already isolated from the cTrader host namespace.

The next architecture-hardening wave can introduce separate Core/Application/Infrastructure assemblies after the dependency graph is verified. This avoids empty project boundaries before their ownership is proven.
