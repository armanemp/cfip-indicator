# Smart Trend Arrows — Single Owner / 9-Level Strength

Date: 2026-10-04
Branch: phase/smart-trend-arrows-single-owner-2026-10-04

## Contract

The chart must show trend arrows that are separated and intelligently graded across nine levels:

- 1-3: WEAK
- 4-6: MEDIUM
- 7-9: STRONG

Within each tier, level 1/2/3 is represented by one/two/three arrows respectively. Direction and strength must come from one canonical calculation path.

## Root-cause changes

1. MtfTrendStrengthRule is the single owner of direction, composite strength score, nine-level mapping and tier.
2. The preferredDirection override was removed from this calculation. It could force the strength calculation to follow another direction owner and therefore create inconsistent arrow intensity.
3. SignalVisualSnapshot now carries MtfTrendStrengthTier from the canonical rule.
4. SignalStackedArrowRenderer consumes the snapshot tier and no longer independently maps 1-9 to WATCH/CONFIRMED/STRONG.
5. Arrow spacing is now at least 1.5x offset (with pip/tick floors), replacing the previous 0.75x offset minimum.
6. M1 remains a circle trigger marker, not a second directional arrow.

## Verification

Repository-level verification still needs to run on the branch through the project CI/build. Terminal visual verification is required for exact glyph separation and real chart rendering.
