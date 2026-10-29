# U5f — Player XP upgrade branches

Implemented2026-10-29 after8C. Adopted initial balance, not pacing proof.

| Node | Cap | Purchased parent | Base/growth | Per-rank effect |
|---|---:|---|---|---|
| node.character_xp_flat.01 |5|dish_xp_flat.01:1|150/1.7|+1 Player XP/full day|
| node.character_xp_flat.02 |5|character_xp_flat.01:max|2000/1.7|+5 Player XP/full day|
| node.character_xp.01 |15|character_xp_flat.01:1|200/1.7|+0.07 node factor|
| node.character_xp.02 |5|character_xp.01:max|50000/1.8|+0.07 node factor|

First multiplier retains the existing family catalog entry. Three new definitions are appended; existing prices remain unchanged. Upgrade discount applies through existing guarded quote/debit rules. Ownership, caps and purchased prerequisites are independent per node; no source grants or other unfinished effects are enabled.

- Award only on the tenth successful receipt: `min((paid day earnings/100 + sum(flat contributions)) × product(node factors), remaining Player XP to15000)` in decimal. All flat applies before both factors. Max flat30; max factor2.05×1.35=2.7675. An early/abandoned day still yields0 Player XP and forfeits pending money. Dish XP, recipe/upgrade prices, payout and location mapping are unchanged.
- A full zero-sale day may receive the flat bonus; there must still be ten completed dishes. At max chef level actual granted XP is0 but full cash is paid. Summary/result report the actual award, not the unclamped proposal.
- Cent cash produces4 fractional XP places; two2-place factors require up to8. Preserve all8 in memory/save/UI without per-award rounding. Example500.25 money at maxima→96.86941875 XP;14999.99999999 retains only the final0.00000001 before cap.
- Schema20 introduces only the four IDs and extends Player XP precision to8 places. Versions18/19 keep their original4-place validation;1–19 reject new IDs even at0, retain exact old progress/no eager rewrite/no free ranks. Caps/prerequisites/backup/reset/future21 protection apply. No redundant level/unlock or pending-day state is serialized.
- Detail previews show per-node flat/factor and explain the full-day formula. Statistics adds an eleventh aggregate line; panel/detail boundary moves24px. Header, result and summary show up to8 XP places with existing locale formatting; long header fits through existing best-fit.

## Scoped evidence

Changed behavior: day XP formula, four purchase IDs/caps and save precision. Risks: wrong operation order/stacking, early bonus, rounding loss, excess cap XP, changing cash, invalid old schemas and clipped UI. Minimum checks: compilation, independent rational model/storage, existing day-settlement boundary and isolated native purchases/controller day/restart.

Model PASS: definitions/parents/caps, explicit first-flat prices150/255/434/737/1253 (sum2829), purchase/pause/replay, aggregate maxima and valid rank combinations×five cent amounts against an independent integer rational oracle; exact8-place sample, final cap unit and cash at cap. Storage PASS:1–19 migration/no free IDs/no rewrite;20 eight-place XP/ranks,18/19 precision gate, backup/reset/caps/prerequisites and21 protection. Existing day-settlement model again passed all early1–9/full10 cases at the changed cash/XP boundary.

Native mouse bought flat1 for150 and multiplier1 for200 on an isolated funded profile. A controller early day granted0 Player XP; a following full day paid10.25 and granted exactly1.179675 XP, total100.179675, wallet9999660.50. Autosave and actual stop/restart/Continue restored exact ranks/flat/factor/XP/wallet and idle day. Eleven-stat height and summary fit passed; an8-place header sample fits two lines at22px. A text assertion initially expected a decimal dot while the active locale used a comma; corrected the assertion and checked the actual localized value/geometry without rerunning unchanged purchases.

Play stopped, temporary override removed, saved scene reopened clean. Catalog diff contains only three appended definitions; scene, dish/location data and unrelated settings/art edits retained. No Player build, broad regression/resolution matrix or pacing claim. Working paid nodes55/91.
