# Upgrade batch A — discount and extended dish rewards, 2026-10-27

Owner authorized continued upgrades while usage remains. This batch extends only working purchase/Dish XP/payout systems. All numbers are adopted initial ClickCook balance, not pacing proof. Existing first tiers are retained.

| Node | Cap | Purchased prerequisite | Base / growth | Per-rank contribution |
|---|---:|---|---|---|
| upgrade_discount.01 | 5 | dish_value.01:1 | 300 / 2 | +1 percentage point discount |
| dish_xp_flat.02 | 8 | dish_xp_flat.01:max | 2500 / 1.7 | +5 XP |
| dish_xp_flat.03 | 5 | dish_xp_flat.02:max | 25000 / 1.8 | +14 XP |
| dish_value.02 | 5 | dish_value.01:max | 250000 / 1.7 | +0.04 factor |
| dish_value.03 | 5 | dish_value.02:max | 2500000 / 1.8 | +0.05 factor |
| dish_value.04 | 5 | dish_value.03:max | 25000000 / 1.9 | +0.06 factor |

IDs have the `node.` prefix. Each extended tier has its own catalog entry/cap/price. Families aggregate purchased contributions: max flat XP=15+40+70=125; max value factor=1+0.45+0.20+0.25+0.30=2.20. XP still `(base+flat)×XP factor` with exact hundredths/mastery clamp; payout still rounds only once at the final receipt. One completion/reward across manual/Hold/auto, no changes to authored DishData.

Discounted upgrade price=`max(1,ceil(catalogPriceForNextRank × (1-currentDiscount)))`, catalogPrice already uses its existing ceil. Discount applies to every working paid upgrade including itself, using OLD purchased rank. Self prices 300/594/1176/2328/4608; maximum discount 5%, no refund. Quotes bind session/node/rank AND discount rank; any discount change invalidates old quotes even if their rounded price is unchanged. UI details, tiles, funds and Buy use the same effective-price function. All normal pause/day/owner/replay/funds guards retained.

Schema 15 introduces only these six IDs, with per-node caps and purchased prerequisites. Older schemas reject them even at zero; legacy wallet/XP/fractions/ranks/backups unchanged, no eager Load rewrite/free ranks. Future versions protected. UI previews distinguish per-tier contribution from actual aggregate statistics; new discount statistic is the ninth line, panel grows 24 px with scrollable detail shortened accordingly. Future systems remain unpurchasable.

Verification risks: self-discount ordering/stale quotes/mismatched UI prices; aggregate receipt math and caps; versioned whitelist. Minimal checks: compilation, targeted model/transaction/receipt checks, schemas 1–14→15 and new-node validation/fraction roundtrip, isolated live purchases/UI/autosave/Continue, current Full-HD view. No broad regressions or Player build.