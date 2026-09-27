# CFIP Indicator — Workflow

Before changing code, read the roadmap, architecture, affected module, relevant reference behavior and tests.

For every behavior record the owner, inputs, outputs, side effects, broker effects, presentation effects, failure modes, direction symmetry, tests and migration destination.

Implementation order:
1. Contract.
2. Authoritative implementation.
3. Caller migration.
4. Tests.
5. Duplicate-path removal.
6. Runtime validation.
7. Documentation.

Do not add compatibility layers for obsolete names. Do not duplicate business rules. Do not let UI call broker APIs. Do not treat broker submission acceptance as fill confirmation.

Automated-trading validation uses controlled environments and covers market entry, pending orders, rejection, slippage, missing protection, partial close, close confirmation, disconnect/reconnect, restart adoption, duplicate events, daily loss, reversal, exhaustion, invalidation, EOD and multi-position fills.
