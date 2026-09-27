# Interface Boundaries

The v89 service contracts are now explicit files rather than hidden inside the monolith.

Contracts:
- Decision engine
- Entry/Trigger engine
- Trade-plan builder
- Execution policy
- Execution planner
- Broker gateway
- Broker state reader
- Outcome recorder
- Presentation projector

These contracts define the intended authority boundaries. Implementations remain compatible with the migrated v89 behavior while the next hardening stage removes remaining platform coupling from application/domain code.
