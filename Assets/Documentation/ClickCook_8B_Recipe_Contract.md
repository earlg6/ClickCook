# 8B — Recipe ownership, purchases and order eligibility

Implemented and locally verified 2026-10-28.

## Rules

- New profiles own Sandwiches (dish1, authored recipe price0). Dishes2–32 are purchased individually for the existing integer `DishData.RecipeCost`; ownership is permanent and shared across locations. There are no recipe discounts, location-purchase prerequisites, refunds or recurring recipe charges in this slice.
- `RecipeCatalog.asset` references all32 existing dish assets in stable ID order. It introduces no duplicate balance table. Quotes bind the session, dish identity and exact current catalog price. Purchase rejects insufficient funds, foreign/stale/replayed offers, changed identity/price, pause or any active day owned by the profile. A successful purchase debits once, grants ownership and invokes the existing profile autosave notification once. It grants no XP or cooking reward.
- `LocationData.DocumentedAvailableDishes` is the authoritative runtime serving map; the reverse dish→location list is reference data. Board generation and replacement choose uniformly from distinct valid dish IDs in the intersection of that map and profile ownership. Runtime duplicate orders remain allowed as independent instances. Null entries and nonpositive cooking HP are excluded.
- Owner decision in this chat: Michelin retains **only dishes29,30,31,32**, as already authored in Unity. No level9 union or “all from restaurant3” expansion is inferred. The other ten pools remain unchanged.
- A location with zero owned recipes cannot start a day through the controller, including a stale Cook callback. UI shows owned/total recipes and directs the player to the shop or another location. Dormitory always retains the free starter path, so spending money cannot remove the ability to earn. Location level unlocks remain8C; the existing carousel remains accessible.
- Shop is available between days from location selection:32 recipes, eight rows/page, owned/price labels, sale value and the authoritative serving locations in details, funds/owned purchase state, balance and explicit navigation. It is purchase UI, not the later Journal or authored recipe-instruction screen.
- Purchases use a consumed displayed quote; repeated callbacks in the same frame do not buy another recipe. Closing/pause/menu cannot bypass model guards. New Game/reset warning now explicitly includes bought recipes.

## Storage17

- Snapshot contains a detached read-only list of owned recipe IDs. Saves write sorted IDs1–32, including starter1. Null/empty/missing-starter/duplicate/out-of-range lists and positive dish XP without the corresponding owned recipe are rejected.
- Schemas1–16 still read their original wallet, whole/fractional Dish XP and upgrade ranks. Migration grants starter1 plus recipes with existing positive whole or fractional Dish XP, preserving access to already-cooked dishes without granting every recipe. Zero-XP entries alone grant nothing. Legacy files with an injected nonempty recipes field are invalid. Loading does not eagerly rewrite the file.
- The next existing receipt/purchase save writes17. Primary/backup atomic handling, recovery and failed-write notices are reused. New Game replaces both copies with starter-only ownership; newer schema18+ stays protected. No active orders, pending quotes, day reports or Unity references are serialized.

## Scoped verification

Changed behavior: spending for permanent recipes, save migration and board filtering. Risks: duplicate debit, missing earned access, loss of existing money/XP/ranks, wrong serving pools, empty-location trap and inaccessible UI after Cook becomes disabled. Smallest checks: compilation, local model/storage fixtures and controller/shop/save boundaries in Play.

- Model PASS:32 stable catalog entries/authored prices, new starter, exact owned intersections and all-owned pools for all11 locations, owner-selected Michelin map, initial/replacement filtering, quote/funds/pause/shared-day/foreign/replay/changed-price guards, single debit/notification, no purchase XP, read-only/detached snapshots and compatibility with the8A receipt summary.
- Storage PASS: schemas1–16 with exact wallet/whole or fractional XP and no eager rewrite; zero-XP migration case;17 ownership plus exact wallet/XP/strength-rank roundtrip; backup recovery/reset; malformed lists/XP relationships, old-schema field injection and18 protection. Existing unchanged rank validation and atomic-write implementation evidence is reused.
- Play PASS on isolated `Temp/ClickCookSaveTests/RecipesPlay-*` profiles: starter-only board, shop blocked during active day, zero-owned Cook disabled plus callback guard, purchase detail fit, ownership autosave and actual stop/restart/Continue. Native mouse bought Mug cakes for150:1000.25→850.25 once, recipe2 on disk, replay no debit; restart retained exact values and eligible board, then8A day-end flow remained usable.
- All32 row labels/details fit; four pages wrap. Location hints were inspected for all11 locations at Full HD. Michelin uses five rendered lines at26px within160px height. Final disabled-Cook navigation walks a five-button loop and focuses Recipes on menu entry. Updated reset warning fits and mentions recipes.
- Native Game View inspection at Full-HD render /0.45 display scale: actual recipe purchase/owned state, Michelin’s zero-owned hint/disabled Cook, open-shop button, page1→4, unavailable expensive recipe and corrected non-overlapping footer. A first purchase attempt was blocked by normal focus-loss pause; after native Resume the click and saved result passed. No claim of32 native purchases or physical-keyboard verification.
- Saved scene has zero existing YAML blocks removed. Existing changes are UI child lists, controller bindings, Cook navigation, hint best-fit, reset copy and an inert font maximum normalization. Environment, camera, lights, authored dishes and all11 location data assets are retained. Prior settings/art/other-chat changes remain separate.

Play stopped, temporary save override removed, saved scene reopened clean. No Player build, broad gameplay suite, resolution matrix or pacing claim. Only known Unity VCS configuration errors and an MCP startup/connection interruption were observed; no ClickCook runtime exception was observed.

Next:8C — define cumulative Player XP/level thresholds, earnings award boundary and mapping to location levels0–10 before implementing unlocks. Recipe discount upgrades now have a base ownership/purchase system but remain separately unimplemented.
