# 8C — Player XP, chef levels and location access

Implemented2026-10-29. Owner approved both choices in this chat: the source XP table is cumulative; only Dormitory is available at level1, Café at2, then sequentially, with Fashion and Michelin both at10.

## Runtime rules

- Cumulative thresholds for levels1–10 are0,100,300,700,1400,2500,4200,6600,10000,15000. `PlayerProgression` is the single authority. Reaching a threshold includes that level; excess XP carries through multiple levels. XP caps at15000/level10; wallet earnings continue at the cap.
- Award `min(full-day earnings /100, remaining XP to cap)` in decimal, retaining all four fractional places implied by cent money. No rounding/truncation per dish or day. Only the tenth successful dish settles the day; its money and Player XP enter the same profile notification/autosave transaction. Dish XP remains separate.
- Early Finish, abandoned/menu/scene days, partial work, purchases, reading summary, Continue, stale callbacks, Next Day and reload grant no Player XP. Summary records the actual awarded XP, including a truncated near-cap award or0 at cap. No retroactive XP is calculated from existing wallet or Dish XP.
- Explicit location map: documented level0→chef1,1→2,2→3,3→4,4→5,5→6,6→7,7→8,8→9,9→10,10→10. Invalid/null locations are unavailable. The model rejects locked StartDay without discarding an existing valid day; the controller separately guards Cook.
- Level access and recipe ownership are independent requirements. Owned recipes do not bypass level locks. Bought recipes remain global, serving pools and owner-approved Michelin dishes29–32 are unchanged. Shop purchases remain allowed before the serving location unlocks; details show each location's required chef level.
- Carousel permits previewing all locations. UI shows current chef level, cumulative XP/next threshold or cap, required level, and the appropriate level/recipe hint. Disabled Cook is excluded from navigation. This is functional progression, not final pacing approval or a new location environment.

## Schema18

- Saves contain canonical decimal-string `playerXp`,0–15000 with at most four decimal places; snapshot copies it. Missing/malformed/negative/over-cap/overprecision and old-schema XP injection are rejected. Level and unlocked locations are derived, not stored redundantly.
- Versions1–17 retain exact prior wallet, Dish XP/fractions, ranks and recipe migration behavior, starting this newly introduced system at0 XP/level1. Old wallet is not a lifetime earnings ledger, so no fabricated levels are granted. No eager rewrite on load.
- Primary/backup, reset, newer-version protection and retry handling are inherited. New Game resets XP in both copies;19+ is protected. Pending day money/XP and report history are not serialized.

## Scoped evidence

Changed behavior is the XP/cash settlement transaction, persistence and two independent location-start guards. Risks: duplicate/early XP, incorrect threshold boundary or cap, free levels from legacy saves, lost existing progress, recipe/level bypass and inaccessible/clipped UI. Smallest checks were compilation, progression-specific model/storage fixtures, the existing day-settlement model at the changed transaction boundary and Play/restart integration.

- Model PASS: all exact/below/above threshold boundaries, all10 chef levels×11 location mapping combinations, locked start preserving an active valid day, exact99.95→100.05 XP, one tenth-receipt notification, early/abandon/replay guards, summary actual XP, cap14999.9999→15000, cash at cap and multi-level0→1000/level4.
- Storage PASS: versions1–17 preserve exact wallet/whole or fractional Dish XP/recipes and source bytes with0 new XP;18 exact fractional/threshold/cap roundtrips, backup recovery/reset, malformed values and19 protection. Unchanged upgrade-rank/atomic-file validation evidence reused.
- Existing targeted day-settlement model PASS for every early count1–9/full10 after adding Player XP. Recipe model PASS after explicitly funding its authored-pool fixtures with level10; recipe spending/filtering behavior retained. Unrelated gameplay/resolution suites not rerun.
- Isolated Play PASS: owned Café recipes at level1 still cannot Cook; full day moves XP99.95→100.05 and wallet123.25→133.25 in one autosave, summary shows0.1 Player XP and level2, Café becomes available. Actual stop/restart/Continue restored exact XP/level/wallet, started Café with recipe filtering and gave no extra XP for an empty early day.
- New XP/required-level text inspected for all11 location previews; all32 updated shop descriptions fit. A maximum fractional XP/header sample initially exceeded its fixed height; automatic font sizing corrected it to two lines at24px. Model/storage results were reused after this presentation-only change.
- Native Game View inspected at Full-HD render/0.45 display scale: level2/XP100.05 header and available Café; native carousel click showed next location requiring level3 with Cook disabled. Main controller preparation used MCP, not physical cooking claims.

No Player build, broad regression, native keyboard pass or pacing proof. Play stopped, temporary save override removed, saved scene reopened clean. Existing art/settings/other-chat edits were retained outside the checkpoint.

Next dependent slices include remaining U5 recipe/Player-XP modifiers and9A Restaurant XP, whose own sources/thresholds still require a contract.
