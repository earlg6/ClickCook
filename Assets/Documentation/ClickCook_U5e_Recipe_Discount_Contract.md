# U5e — First recipe-price discount

Implemented2026-10-29 after8B purchases and8C progression. Initial balance, not pacing proof.

- Enable `node.recipe_discount.01` after purchased `node.dish_value.01:1`. Five ranks,1 percentage point/rank, maximum5%. Reuse authored catalog prices150/300/600/1200/2400, total4650 before the independent upgrade discount.
- Paid recipe price=`max(1,ceil(RecipeCost × (1 − .01 × rank)))`. Starter0 stays0 and cannot be bought. Mug cakes150→149 at1%,143 at5%; a positive cost1 remains1. No refunds or retroactive repricing.
- Upgrade discount applies to purchasing this node. Recipe discount does not discount upgrades; upgrade discount does not discount recipes. Serving maps, chef-level locks and cash/XP rewards are unchanged.
- Recipe quotes capture session, stable dish ID/object, base cost, discount rank and effective price. Purchase rechecks all of them. A changed rank invalidates an old offer even if ceil produces the same price. Existing pause/day/foreign/funds/replay guards remain; only the effective price is debited once.
- Shop rows/details/base/discount/affordability/Buy share the same effective-price function. Upgrade detail previews current→next; statistics gains a tenth line. Panel grows24px; existing scrollable detail shortens24px.
- Schema19 introduces only this node/cap/prerequisite.1–18 reject its ID even at0 and retain exact prior progress/no free ranks/no eager rewrite. Backup/reset and20+ protection remain. No scene, catalog-price, dish/location or settings edit.

## Scoped evidence

Changed behavior is recipe pricing, stale-offer validation and the new upgrade/save ID. Risks: ceil/debit errors, mixing discounts, stale quotes, accepting future nodes in old schemas, lost Player XP and UI clipping. Smallest checks: compilation, targeted model/storage, zero-discount recipe model and isolated live purchase/restart.

Model PASS: all32 recipe prices×ranks0–5 against an integer ceil oracle, prerequisite/cap/five explicit prices, separate discounts, stale/equal-rounded-price rank stamps and exact purchase143 at5%. Storage PASS:1–18 exact/no free ranks/no rewrite;19 ranks1–5 with independent upgrade discount and Player XP, backup/reset/cap/prerequisite validation,20 protection. Existing recipe model passed at zero discount.

Native Game View mouse bought rank1 for150 (10000.25→9850.25), then Mug cakes149 (→9701.25). Autosave and actual stop/restart/Continue retained rank1, recipe2, wallet9701.25 and Player XP100.0001. Full-HD/0.45 display inspection, ten-stat height and all32 updated shop detail heights passed. Unchanged ownership/fraction/file-atomicity evidence reused.

Play stopped, fixture override removed, saved scene clean. No Player build, broad regression/resolution matrix or physical-keyboard retest. Unrelated art/settings/other-chat edits remain separate. Working paid nodes51/91. Next: flat/multiplier Player XP; Restaurant XP still needs its own source/threshold/benefit contract.
