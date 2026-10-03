# CFIP — Modern / Advanced / Optimized Engineering Gate
Date: 2026-10-04
Status: MANDATORY

## Purpose
Every production change must target the most modern, advanced and optimized practical implementation compatible with CFIP contracts, cTrader constraints and safety requirements. Modernity means measurable engineering quality, not novelty.

## Required review
Architecture, single ownership, deterministic logic, performance, runtime/lifecycle, UI/UX, presentation, observability, persistence, testing/CI, dependencies, safety and documentation must be reviewed for every touched area.

## External research rule
For non-trivial decisions, current internet research may be used for ideas and comparison. Prefer official/vendor/primary sources and strong engineering literature. External research is design input only; CFIP contracts, measured behavior, platform constraints and single-owner law remain authoritative. Do not adopt fashionable technology without a demonstrated fit.

## Optimization rule
Optimize at the root: remove unnecessary work, simplify ownership, improve data flow, then improve scheduling/cache strategy. Arbitrary throttles, duplicate caches, parallel implementations or extra gates that hide architectural waste are not valid optimization.

## Closure requirement
A phase cannot close while a clearly superior, materially safer or materially more efficient practical design for the touched scope is identified but ignored without an explicit documented reason. Non-obvious modernization decisions must record what was considered, what was selected, why it fits CFIP, expected benefit and evidence.

Reference: docs/PHASE-M2-REPOSITORY-HYGIENE-OWNERSHIP-2026-10-04.md
