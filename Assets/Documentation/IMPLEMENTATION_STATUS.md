# ClickCook Implementation Status

## Designer-source synchronization — 2026-10-15 (documentation only)

- The designer explicitly prioritizes ClickCook Figma/FigJam over previous GDD interpretations. GDD Section 0 now records source precedence and reconciled conflicts; FishSim is a mechanics/progression reference as well as a UI reference.
- Restored designer story/inner thoughts/culinary diary direction, detailed Pause/Settings contents, existing recipe Journal content and the 25 source achievement rows/reward notation. Achievement units, exact triggers and stacking remain unresolved; no rewards were granted.
- Expanded upgrade design: 27 planned effect families (existing 16 plus 11 extensions), connected/tiered progression, manual/critical/hold/extra-strike/automation/economy/XP/quality paths. Only Click Strength remains implemented and verified. The original 16-family draft is not a final content cap; its unimplemented numbers are still proposed balance.
- User approved: double strike is a chance of one extra hit on the same dish, without another portion/reward; critics remain part of the design but activation and career penalties/reset scope require separate approval. No reset behavior was authorized.
- Spatial presentation target: actual right-hand noticeboard sheets and cooking on the existing tray/plate, superseding UI-A3's floating central-card layout as the final target. UI-A3 verification below remains historical evidence, not verification of the new presentation.
- At source synchronization UI-A4 had pre-existing local script/scene/art/test work, not a complete acceptance pass. It was subsequently completed/verified in the separate UI-A4 report below; the documentation-only synchronization itself did not certify it. Existing unrelated settings changes were preserved.
- Documentation changed: ClickCook_GDD.md, ClickCook_Upgrade_Design_Proposal.md, ClickCook_UI_UX_Specification.md, ClickCook_Production_Roadmap.md, IMPLEMENTATION_STATUS.md. No gameplay code, data, scene, Figma or settings were changed by this synchronization task.
- The next checkpoint at synchronization was UI-A4; after its completion below, the next separate implementation is 7A profile/stable IDs. Do not automatically implement U0–U6 or mark the whole tree complete.
- Documentation verification: scoped `git diff --check` passed; original location/dish/rarity tables in GDD Sections 5/8/11 compare unchanged against HEAD; expanded tree contains 27 unique family IDs; achievement table contains 25 entries; Markdown fences/local document links passed checks. No Unity compilation/Play Mode test was run for this documentation-only task. Existing scene-diff whitespace warnings were not edited.

## Completed milestones

### Milestone 1 — ClickCook Data Foundation

Static ScriptableObject data for Locations and Dishes is implemented.

- `DishData`: documented numeric ID, English and Russian display names, sale value, recipe cost, click requirement, and explicitly documented location availability.
- `LocationData`: level requirement, English and Russian display names, passive income per second, and documented dish availability.
- 32 `DishData` assets and 11 `LocationData` assets are present.
- Bidirectional references are populated from the explicit **Known locations** entries in the GDD dish table.

### Milestone 2A — Runtime Location Catalog

A read-only `LocationCatalog` ScriptableObject provides a single, ordered runtime entry point for the 11 existing `LocationData` assets.

- The catalog preserves documented level order 0–10.
- `TryGetByDocumentedLevel` resolves a location by its documented level.
- The catalog contains no location-unlock, ownership, recipe-gating, order-generation, save/load, or gameplay logic.

### Milestone 2B — Location Selection to Work Day Prototype

`SampleScene` now contains a temporary uGUI prototype under `ClickCook_Prototype`.

- Location Selection is a left/right carousel over all 11 catalog locations, wrapping at either end.
- The displayed name, documented level, documented dish count, and income/sec update with the current location.
- Cook stores the current `LocationData` in an in-memory prototype session and opens a Work Day placeholder showing the selected location.
- Finish Day is a harmless prototype-only action that displays a placeholder message.
- The temporary scene UI and controller consistently use legacy `UnityEngine.UI.Text`; every serialized screen, label, button, and catalog reference is assigned.

#### Milestone 2B verification

- Unity 6000.3.14f1 compiles the milestone scripts without C# compilation failures.
- `LocationCatalog.asset` contains exactly 11 non-null `LocationData` references with documented levels 0–10 represented once each.
- Play Mode button-callback verification confirmed all 11 locations are browsable, Previous/Next wrap correctly, Cook opens Work Day with the selected location, and Finish Day displays its prototype-only response.
- `SampleScene` was saved with the prototype visible and the existing dormitory environment, lighting, and camera preserved.
- The Console still reports an unrelated Unity Version Control client configuration error because the local Plastic/UVCS client is not configured; no Milestone 2B runtime exception was observed.

## Intentionally unimplemented

- Current upgrade status (6A): Click Strength purchases/damage are implemented; critical/hold/auto-click and all other effects remain unimplemented. Older milestone sections below retain their historical scope.
- Location unlocking and ownership.
- Recipe ownership/gating.
- Final end-of-day summary and date-based next-day progression.
- Full economy/passive income, Player/restaurant XP, save/load, persistence, critical/hold/auto-click, timers, scoring, final UI, and final animations. Baseline rarity, Dish XP and run-local payouts/wallet are implemented in 5B; Click Strength purchases/damage are implemented in 6A.

## Known GDD ambiguities / TBDs retained

- The GDD separately describes new recipe groups and dishes served by locations; their precise relationship and recipe purchase gating are TBD.
- Several location descriptions use incomplete phrases such as “also serves selected ... dishes.” No extra dishes were inferred.
- Correction on 2026-10-12: current Dormitory LocationData has valid Sandwiches and Mug cakes references, confirmed through Unity MCP and 6A Play Mode. Earlier zero-candidate statements describe a historical verification report, not current data. No static asset was changed. Empty pools remain covered by transient test fixtures; final recipe eligibility is still TBD.
- The Michelin restaurant says “all dishes starting from the third restaurant,” which conflicts with / is less precise than its criteria dish list. No additional earlier dishes are inferred.
- Base hidden HP equals ClickRequirement, with one damage per unupgraded click (4B); 6A applies purchased Click Strength. Remaining upgrade modifiers are TBD.
- Location ownership/unlock state, passive-income accrual/collection, recipes, Player/restaurant progression and persistent progression remain unimplemented by design. Base rarity, Dish XP and payouts use the approved 5B rules.

## Files created

- `Assets/Scripts/Data/DishData.cs`
- `Assets/Scripts/Data/LocationData.cs`
- `Assets/Scripts/Runtime/Locations/LocationCatalog.cs`
- `Assets/Scripts/Runtime/ClickCookPrototypeController.cs`
- `Assets/Scripts/Runtime/PrototypeDishRewards.cs` and `.meta` — Milestone 5B
- `Assets/Data/Dishes/` — 32 `DishData` assets
- `Assets/Data/Locations/` — 11 `LocationData` assets
- `Assets/Data/LocationCatalog.asset`

## Files modified

- `Assets/Scenes/SampleScene.unity`
- `Assets/Documentation/IMPLEMENTATION_STATUS.md`

## TextMesh Pro cleanup

- The pre-existing untracked `Assets/TextMesh Pro/` import and `Assets/TextMesh Pro.meta` were removed after review.
- The removed import contained 366 files, including TMP Essential Resources and Examples & Extras.
- Before removal, a project-wide dependency scan found no GUID references to any of its 193 imported asset GUIDs and no TMP type/resource-name references outside the imported folder.
- The verified prototype uses `UnityEngine.UI.Text`; Unity compiled successfully after cleanup and the Location Selection → Work Day smoke test passed.
- The TextMesh Pro package remains installed; only unused imported project assets were removed.

## Post-Milestone 2B review

### Implemented and verified

- Static design data for 32 dishes and 11 locations.
- Bidirectional documented location/dish availability references.
- Ordered runtime location catalog with documented-level lookup.
- Temporary uGUI Location Selection carousel over all 11 locations.
- In-memory selected-location handoff from `Cook` to the Work Day placeholder.
- Prototype-only `Finish Day` response.
- Existing Dormitory environment, lighting, camera, URP presentation, and cursor-follow behavior remain intact.

### Partially implemented

- Core game flow: Location Selection → Work Day board → base click cooking → rarity/Dish XP/payout result → replacement / day end exists (5B). The tenth result ends on Continue; final summary remains absent.
- Location system: static data and browsing exist; unlocking, ownership, real progress, passive-income accrual, and recipe gating do not.
- Dish system: static data, base click cooking, approved rarity and independent run-local Dish XP/mastery exist; runtime recipes, persistent progression, final dish art and effects do not.
- Work Day: documented candidates, dynamic orders, base cooking/completion/replacement, ten-dish limit, payouts, early finish and cleanup exist; daily summary, dates and date-based next-day progression do not.
- UI: temporary prototype screens exist; final navigation, styling, responsiveness, feedback, and all other screens do not.
- Dish Result: dish name, rolled rarity, payout/wallet, earned Dish XP, level/progress and Continue exist (5B); final presentation does not.

### Not implemented

- Main Menu, final result presentation, full economy, Player/restaurant XP/progression, unlock systems, upgrades, save/load, achievements, journal, settings, pause, final animations, and audio content.

## Completed Milestone 3 — Documented Work Day Dish Candidates

Supporting GDD Sections 2, 5, and 8, the existing Work Day placeholder now displays the selected location's complete documented dish-candidate list. This is a data-to-runtime prototype, not an order board or a resolution of order eligibility.

### Implemented functionality

- One read-only legacy uGUI label, `ClickCook_Prototype_WorkDay_DishCandidates`, added to the existing Work Day panel.
- Cook populates the count, dish IDs, and English names directly from the session's `LocationData.DocumentedAvailableDishes`, preserving every entry and stored order without randomization or filtering.
- Missing dish references are explicitly labeled instead of silently omitted; all 11 current locations have valid references.
- The Work Day placeholder layout was enlarged to fit the complete lists; the Dormitory environment is unchanged.
- Existing in-memory session and prototype-only Finish Day response are retained. No order instances, eligibility rules, cooking, economy, progression, or persistence were added.

### Files modified (no new files)

- `Assets/Scripts/Runtime/ClickCookPrototypeController.cs`
- `Assets/Scenes/SampleScene.unity`
- `Assets/Documentation/ClickCook_GDD.md`
- `Assets/Documentation/IMPLEMENTATION_STATUS.md`

### Executed Unity verification

- Unity 6000.3.14f1 recompiled successfully with no C# compilation failures.
- All 15 serialized controller references are assigned; the catalog retains exactly 11 valid locations, ordered by levels 0–10.
- Play Mode tests invoked the actual button callbacks over two complete carousel passes (22 Cook transitions). Every candidate list matched the data's count, IDs, names, and order; text-height checks confirmed the lists fit without vertical truncation.
- Location name, level, count, income, placeholder progress, both wrap directions, selected-location handoff, screen visibility, and Finish Day response passed regression checks.
- A fresh Play Mode session reset correctly and displayed the Michelin location's candidates and Finish Day response. Rendered Pastry shop and Michelin Work Day views were inspected.
- Tests used button callbacks, not physical mouse input; final responsiveness and device support remain unverified and out of scope.
- SampleScene is saved, clean, and left out of Play Mode with the new serialized label binding retained. Static data assets were not modified.
- No ClickCook runtime exception occurred. The pre-existing Plastic/UVCS client configuration exception remains unrelated and unresolved.

### Design decisions deferred at the Milestone 3 checkpoint

Historical checkpoint list; Milestone 4 approvals and implementation below supersede the order count/selection/replacement questions where explicitly resolved.

- Whether recipe ownership gates order eligibility.
- Exact interpretation of incomplete “also serves selected ... dishes” statements and Michelin availability.
- Order count, six-slot interpretation, generation weights, refill/removal, repeats, timeout, and selection limits.
- Work-day completion, early-finish consequences, earnings, rewards, and XP.
- Cooking click/hidden-HP semantics and rarity behavior.

### Recommended next step at the Milestone 3 checkpoint

Resolve the minimum order-lifecycle and recipe-eligibility contract in GDD Sections 6–7 and 31 before implementing an order board: initial count, six-slot meaning, dish selection/repeats, refill/removal, and whether owned recipes gate candidates. Do not infer these rules from the candidate display.

## Completed Milestone 4A — Dynamic Order Board and Order Selection

### Approved design, separately recorded in GDD Sections 6–7

- Location-dependent simultaneous capacity, at most six; smaller boards early and larger boards later.
- Uniform random selection from existing `DocumentedAvailableDishes`, independent duplicates allowed, no recipe/progression/economy/rarity gating.
- One active order, immediate handoff, reservation without completion, prototype Back preserving the board.
- Baseline daily completion limit ten (not simultaneous capacity); successful completion/removal/counting/replacement, no replacement at the limit, and early Finish Day/discard are approved future lifecycle rules, NOT implemented in 4A.
- 60–90 seconds is a pacing target, not an implemented timer.
- Designer clarified to follow the GDD: Dormitory retains its existing two candidates, Sandwiches and Mug cakes. No static data assets were altered.

### Provisional implementation choices

No location-to-capacity mapping existed in the GDD. Inspector field `prototypeCapacities` on the existing controller centralizes an editable proposal for levels 0–10: **2, 2, 3, 3, 4, 4, 5, 5, 6, 6, 6**. These values are not approved balancing thresholds. The temporary board uses two columns and up to three rows on the right of Work Day; this is not final UI.

### Implemented functionality and runtime architecture

- `PrototypeOrder` is a plain transient instance with GUID identity, immutable dish/location/slot references, and reservation state. Duplicate dish references never share order identity.
- `PrototypeOrderSession` holds the fresh initial board and one active order. Capacity is clamped to 0–6; null candidates are excluded for safety, empty pools create no orders, and new days release reservations and clear old orders. It contains no completion/replacement API.
- Existing controller/session handoff extended rather than rebuilding the carousel or data architecture. Button callbacks validate state, prevent double selection/hidden navigation, reserve the exact instance, and release only its reservation on Back.
- Existing candidate display retained separately from generated orders. Board shows location, available orders, capacity, `Completed dishes: 0 / 10`, and `Currency: not implemented`.
- Clearly labeled cooking placeholder shows location, dish ID/name, slot and full runtime order identity. It has Back, not cooking or completion controls.
- Existing Finish Day message is unchanged; full early-finish cleanup, end-state flow, and Finish Day inside cooking remain deferred.

### Files created

- `Assets/Scripts/Runtime/PrototypeOrder.cs` and Unity-generated `.meta`
- `Assets/Scripts/Runtime/PrototypeOrderSession.cs` and Unity-generated `.meta`

### Files modified

- `Assets/Scripts/Runtime/ClickCookPrototypeController.cs`
- `Assets/Scenes/SampleScene.unity`
- `Assets/Documentation/ClickCook_GDD.md`
- `Assets/Documentation/IMPLEMENTATION_STATUS.md`

### Executed Unity verification

- Unity 6000.3.14f1 recompiled successfully, without C# compilation failures.
- Final Play Mode suite checked all 11 locations twice and 92 order selection/Back cycles: capacity, candidate membership, unique identities, independent duplicates, selected dish/location/identity, one active reservation, repeated/invalid callbacks, unchanged board after Back, fresh-day state, zero completed count, and existing carousel/Cook/Finish Day behavior passed.
- Transient, unsaved test fixtures verified empty/all-null/mixed-null candidate pools, missing location, capacity clamping, and resetting an active reservation on a fresh day. No extra persistent data assets were created.
- Layout checks passed for the six-card board and candidate text, with Finish Day separated from cards. EventSystem raycast and pointer-click dispatch selected the correct order. Physical mouse input was NOT tested.
- Rendered Dormitory and Michelin boards and the cooking placeholder were inspected. A fresh Play Mode session reran the full suite after final code/layout changes.
- Initial environment test incorrectly assumed `Camera.main`; inspection confirmed the existing camera is untagged. The assertion was corrected to inspect the actual Camera without changing its tag. Retest passed.
- No ClickCook runtime exception observed; the pre-existing Plastic/UVCS client configuration exception remains. SampleScene is saved, clean, stopped, and retains the new bindings.
- Serialized scene diff confirms only prototype UI/controller blocks changed or were added; no existing blocks were removed. Dormitory, camera, lighting, volume, location/dish assets, catalog, and project settings are unchanged.

### Remaining limitations at the Milestone 4A checkpoint

Historical plan below; the designer subsequently approved actual base cooking, automatic ending at ten, and return to Location Selection. The completed 4B section supersedes the simulated-completion proposal.

Actual cooking, order completion/replacement, daily counting/limit enforcement, end-of-day cleanup/summary, economy, progression, rarity, and persistence remain unimplemented. Recommend a separate 4B prototype lifecycle slice for explicit simulated completion, exact-instance removal/counting/replacement up to ten, and early-day order cleanup, without cooking formulas or rewards.

Before 4B, approve the prototype end-state behavior at ten completions and the Finish Day destination. Final capacity balancing, future daily-limit increases, final recipe eligibility/availability, timers, click/HP math, rarity, and rewards remain open. No 4B implementation was started.

## Completed Milestone 4B — Base Click Cooking and Order/Day Lifecycle

### Approved rules recorded in GDD Sections 6–9 and 30–31

- Hidden HP = documented `DishData.ClickRequirement`; each discrete click deals one damage; zero completes the exact selected order. Documented values are unchanged, including 70,000 clicks for the Imperial tasting set.
- Back resets partial cooking progress but retains the exact order and the unchanged board. Reselecting starts at full HP.
- A large clickable temporary dish replaces the nonfunctional 4A placeholder; successful completion returns directly to the board, without rarity/result/reward screens.
- Completion marks/removes the exact order, increments completed dishes and generates a fresh identity in the same slot using the approved uniform pool/duplicate rules. No replacement after ten completions.
- Ten completions automatically end the day. Manual Finish Day is available on the board before ten, not during cooking. Both clear unfinished orders/current day state, return to Location Selection and display a message with the finished count; no penalties, rewards, XP or date advancement.

### Implemented files (modified only; no new files)

- `Assets/Scripts/Runtime/PrototypeOrder.cs` — completion flag and hidden remaining click progress.
- `Assets/Scripts/Runtime/PrototypeOrderSession.cs` — exact-instance base clicks, Back reset, slot-aware replacement, real completed count, cap and cleanup.
- `Assets/Scripts/Runtime/ClickCookPrototypeController.cs` — dish-button callbacks, automatic/manual end transitions and message; no hidden HP displayed.
- `Assets/Scenes/SampleScene.unity` — temporary clickable dish, cooking layout and existing finish-message label moved to Location Selection.
- `Assets/Documentation/ClickCook_GDD.md`
- `Assets/Documentation/IMPLEMENTATION_STATUS.md`

### Executed verification

- Unity 6000.3.14f1 recompiled successfully with no C# compilation failures.
- Play Mode button-callback suite checked all 11 locations: random pool/capacity, one active order, one-point damage, no premature completion, Back reset and full-HP reselection, exact last-click completion, replacement identity/slot/location, unchanged other orders, completed counter, early Finish cleanup, and fresh-day reset.
- Actual dish-button callbacks completed ten orders and verified automatic return, `10 / 10` message and empty inactive session; extra/stale callbacks cannot complete another order.
- A transient single-dish fixture tested all 32 documented click requirements, including 70,000, exactly at the final click; independent duplicates, wrong-order clicks and repeated completion guards passed. Model tests also proved no replacement at ten and no further selection at the limit.
- Empty/mixed-null pools, missing location and a pool becoming empty during cooking were tested without modifying static assets. Slot lookup remained correct when a completed slot had no replacement.
- The full suite passed again after a fresh Play Mode session. Automated high-click-count tests verify mechanics, not human pacing; the 60–90-second target has NOT been validated.
- EventSystem raycast/pointer dispatch hit the dish and dealt exactly one damage; cooking labels fit, the dish does not overlap Back, and both early/automatic completion messages fit. Physical mouse interaction was NOT tested.
- Rendered cooking and day-finished views were inspected. No ClickCook runtime exceptions observed; only the pre-existing Plastic/UVCS configuration error remains.
- Serialized scene diff contains only prototype UI/controller changes and added dish UI, with no existing block removed. Dormitory, camera, lighting, URP, static dish/location data, catalog and project settings are unchanged.
- SampleScene is saved, clean, left out of Play Mode, and retains the new serialized dish-button/label bindings.

### Intentionally unfinished and recommended next step at the Milestone 4B checkpoint

This is base cooking and a temporary day-ending flow, not the complete game. Final models, animation/effects/audio, rarity/result presentation, economy/rewards, XP, upgrades, recipes/unlocks, save/load and final End-of-Day summary remain unimplemented. Capacity values remain the existing provisional proposal; no timing or click-count balancing was invented.

Recommended next step: approve the minimal rarity/result and payout contract in GDD Sections 11 and 14 before implementing those systems. Questions include base rarity probabilities, dish-XP influence, currency multipliers and payout timing. Do not infer them from the now-working base cooking loop.

## Completed Milestone 5A — Informational Dish Result

### Approved scope (GDD Sections 7 and 11)

- For completed dishes 1–9, show the exact completed dish name and base `DishData.SaleValue`, explicitly reference-only, not a payout.
- Display `Rarity: not implemented` and `No currency or XP awarded.` Do not assign a fixed Common tier or invent rarity probabilities/multipliers.
- Continue returns to the existing replacement board. Completion/counting/replacement still occur exactly once at the final cooking click; Continue does not modify order state.
- The tenth dish immediately ends the day as in 4B, without a Dish Result screen. No additional Finish Day/Back/navigation action is exposed during the result screen.

### Implementation and files (modified only; no new files)

- `Assets/Scripts/Runtime/ClickCookPrototypeController.cs` — captures the completed order before clearing cooking state, populates the informational result, guards Continue by screen/day state and clears stale text on Continue/end/new day.
- `Assets/Scenes/SampleScene.unity` — adds `ClickCook_Prototype_DishResult` under the existing Canvas with title, details and Continue button, using the project's legacy uGUI Text. No TMP resources or dependencies added.
- `Assets/Documentation/ClickCook_GDD.md`
- `Assets/Documentation/IMPLEMENTATION_STATUS.md`

The existing order/session classes, static data, capacity configuration and all cooking/replacement formulas are unchanged. No economy, XP or rarity code introduced.

### Executed verification

- Unity 6000.3.14f1 recompiled successfully, without C# compilation failures.
- Play Mode suite passed twice across fresh sessions. Each run checked all 11 locations, 52 informational result screens, and exact name/base-value text for all 32 real dish assets using transient unsaved fixtures.
- Results appear only after the exact final cooking click. While a result is visible, hidden/stale order, Cook, navigation, Finish Day, Back and dish-click callbacks do not change the session.
- Continue returns to the same replacement instances/count; repeated Continue does not increment completion or regenerate orders. All untouched slots, pool/capacity rules, Back/full-HP reset, manual finish, fresh-day clearing, empty pools and carousel wrapping passed regression checks.
- Real cooking button callbacks tested the complete ten-dish path: results 1–9, then immediate automatic end/cleanup at ten with no result screen or stale result text.
- Continue EventSystem raycast/pointer dispatch and label-height bounds passed. The initial pointer check failed immediately after the UI/test transition; subsequent inspection and retest after Game View updated passed without code changes. Physical mouse input was NOT tested.
- Rendered result screen was inspected. SampleScene is saved, clean, stopped, and retains all result bindings. No ClickCook runtime exception observed; the pre-existing Plastic/UVCS configuration exception remains.
- Scene serialization comparison shows only the prototype Canvas child list/controller bindings changed, plus new result UI blocks; no existing blocks removed. Dormitory, camera, lighting/URP, static data, catalog and project settings remain unchanged.

### Remaining limitations and recommended next step

This is a partial Dish Result system, not implemented rarity or economy. Base value is informational only. Final graphics/animations/audio, actual rarity, rewards, XP, upgrades, recipes/unlocks, persistence and final End-of-Day summary remain absent. The provisional capacities and 70,000-click values were not rebalanced; human pacing remains unverified.

Recommended next step: a read-only design decision checkpoint for rarity/payout (GDD Sections 11 and 14): base probabilities or non-random progression policy, dish-XP influence, currency multipliers, payout timing and starting wallet. Do not implement these systems without approved values.

## Approved post-5A design decisions — historical approval checkpoint, implemented by 5B below

- GDD Section 11.1 records the designer-approved rarity probability table. Fantastic is canonical for the historical Epic column. Level 3 Common is corrected to 57%, level 11 Legendary to 24%; levels 12/14/16 explicitly repeat 11/13/15. All rows total 100%. No interpolation or runtime rarity implementation was performed.
- Random rarity uses the specific dish's level; Dish XP awards depend on rarity. XP-to-level thresholds, initial/max level, XP awards and calculation timing still need approval.
- Wallet starts at 0 у.е.; payout is SaleValue × the approved future rarity multiplier, once at successful completion, with amounts to two decimal places. Multiplier values and any finer-precision rounding remain unresolved. No wallet/reward code implemented.
- The future tenth-dish result will remain visible until Continue ends the day. This supersedes the 5A bypass only when implemented and verified; current runtime behavior is unchanged.
- Next step: approve remaining numeric progression/reward rules before implementation. Only GDD and this tracker changed; no new Unity verification or gameplay milestone completion claimed.

## Completed Milestone 5B — Dish Rarity, Mastery and Run-Local Payouts

### Approved scope and balance

- Implement GDD Sections 10, 11.1 and 14 together. No further roadmap system is included.
- Per-dish initial level 1 / 0 XP, maximum 18. Transition cost is round(20 × 1.18^(L − 1)), half away from zero; carry excess XP. Total XP cap is 1,744; no additional XP at mastery, payouts continue.
- Common / Uncommon / Rare / Fantastic / Legendary award 10 / 12 / 16 / 22 / 30 Dish XP and pay SaleValue × 1 / 1.25 / 1.75 / 2.5 / 4. XP gains are truncated to the mastery cap.
- Use the approved chance row for the dish level before awarding new XP. Historical levels 19–21 remain in the GDD, not implemented levels. No extra rarity weighting, pity system or click-based XP.
- Zero-starting wallet and Dish XP persist between days in the current run, not across stopping/restarting game/Play Mode. Unfinished orders/Back pay nothing; completed dishes pay exactly once at completion. All ten results are shown; tenth Continue ends the day. No Player XP or save/load.

### Files

- Created: `Assets/Scripts/Runtime/PrototypeDishRewards.cs` and Unity-generated `.meta` — approved probability table/resolver, mastery thresholds, per-dish XP dictionary, decimal wallet and immutable completion receipt.
- Modified: `Assets/Scripts/Runtime/PrototypeOrderSession.cs` — rewards only in the exact completion branch; retain mastery/wallet when day orders/receipt clear.
- Modified: `Assets/Scripts/Runtime/ClickCookPrototypeController.cs` — show real receipt/mastery/wallet, tenth result Continue ends day; stale-screen guards retained.
- Modified: `Assets/Scenes/SampleScene.unity` — only result details font size 28 → 24 for the additional text. Existing hierarchy/bindings, environment, camera, lighting and static data unchanged.
- Modified: `Assets/Documentation/ClickCook_GDD.md` and this tracker — approved rules and verified partial-system progress.

### Executed verification

- Unity 6000.3.14f1 recompiled successfully; no C# compilation failures.
- Play Mode controller suite passed twice in fresh sessions: all 11 locations, carousel wrapping, Cook, candidate pools/capacities, empty Dormitory, Back/full-HP reset, exact click completion, hidden/double/stale callbacks, replacement preservation, early Finish, retained wallet/XP and all ten result screens. The tenth has no replacement and ends only on Continue; repeated Continue does not award again.
- Enumerated all 100 integer rolls at each level 1–18 (1,800 checks) against the designer's exact table. Zero-probability tiers cannot appear. This tests the resolver mapping, not statistical player enjoyment.
- Separate Play Mode model suite passed 160 full-cooking completions: all 32 actual DishData assets × all five tiers using seeded rolls and transient unsaved candidate fixtures. Exact decimal payouts and XP awards, pre-award rarity, duplicate-completion protection and cross-day retention passed.
- Every XP level boundary, overflow carry, cap truncation, no XP at maximum with continued payout, independent dish progress, invalid resolver inputs and fresh session zero-state passed. All game-run progress reset on a fresh Play Mode session.
- Result text preferred-height checks passed across all location results. Continue raycast/EventSystem pointer dispatch passed without changing wallet/count. Physical mouse input was NOT tested.
- SampleScene is saved/clean outside Play Mode; bindings retained and new script recognized by Unity. No ClickCook runtime exceptions observed. Pre-existing Plastic/UVCS NotConfiguredClientException remains unrelated.
- A later Unity MCP request encountered a transport disconnect/server error; subsequent Editor/eval calls recovered. This is not a ClickCook gameplay exception. Game View capture showed Location Selection, but result-screen screenshot requests did not reliably capture the active result; visual screenshot verification of that screen is NOT claimed. Its active state, text contents, layout bounds and EventSystem interaction were verified separately.

### Limitations and next step

This is baseline rarity/mastery and payout, not a complete economy or persistent progression system. Spending, passive income, Player/restaurant XP, saves, final result art/audio/animations and End-of-Day summary remain unimplemented. No DishData/LocationData/catalog values or click requirements were changed. Human pacing and the engagement of this initial balance are NOT verified; high click requirements still need separately approved upgrade mechanics.

Recommended next step: a scoped design checkpoint for the documented upgrade/cooking-speed dependency (GDD Sections 9 and 13), including cost, click-strength effect and purchase rules, before coding an upgrade slice. No next milestone implementation started.

## Completed Milestone 6A — Click Strength Purchases (2026-10-12)

The user authorized the next roadmap step and delegated routine design choices. Adopted the Click Strength subset of the existing proposal: 15 purchasable ranks, initial strength 1, damage ceil(1.75^rank), next price ceil(20 × 2.20^ownedRank), run-local ownership and purchases between days. Remaining proposal mechanics are not implicitly approved.

### Implemented files

- Created `Assets/Scripts/Data/UpgradeCatalog.cs` and `Assets/Data/UpgradeCatalog.asset` (+ Unity metas): one ScriptableObject with 16 serialized definitions (stable IDs/names/base prices/growth/caps). Only Click Strength is enabled for runtime purchases. Other prices remain provisional; no prerequisite/effect framework was introduced for unavailable nodes.
- Modified `Assets/Scripts/Runtime/PrototypeOrderSession.cs`: immutable session/rank/price quote, guarded purchase, persistent-within-run rank and upgraded hidden-HP damage.
- Modified `Assets/Scripts/Runtime/PrototypeDishRewards.cs`: internal positive-amount/exact-balance debit; reward and rarity formulas unchanged.
- Modified `Assets/Scripts/Runtime/ClickCookPrototypeController.cs`: between-day Upgrades/Back/Buy, wallet/rank/current-next strength/price/status, unavailable-node labels and per-frame quote consumption.
- Modified `Assets/Scenes/SampleScene.unity`: prototype upgrade panel/bindings, navigation button and adjacent Cook position. No existing object removed; four existing serialized blocks changed (controller, two parent child lists, Cook RectTransform), plus 55 new blocks. Dormitory/camera/lights/URP unchanged.
- Created `Assets/Tests/Editor/Milestone6AVerification.cs` (+ folder/metas): repeatable Editor-only model and Play Mode verification harness. It is invoked through MCP eval, not registered NUnit tests. Fixture wallet funding stays in Editor-only code.
- Updated GDD, this tracker, upgrade proposal and production roadmap. Previously prepared design documents are included in this checkpoint as related context.

### Executed verification

- Unity 6000.3.14f1 recompiled successfully with compilationFailed=false and no C# errors.
- `Milestone6AVerification.RunModelChecks()` passed 4,685 assertions, including fixed expected prices/damage, all 32 real dish definitions × ranks 0–15 (512 exact-completion cases), debit/insufficient cents/max rank, stale/cross-session quotes, unavailable nodes, mid-day rejection, Back reset, replacement identity, exact payout, empty pools and cross-day/new-session behavior.
- In fresh Play Mode, `RunPlayChecks()` passed 236 assertions: all 11 locations before/after upgrades, carousel wrapping, actual button callbacks, two purchases, repeated callback protection, exact upgraded cooking, result/Continue, early Finish and a full ten-dish day. Upgrade labels fit their bounds.
- A separate fresh Play Mode confirmed wallet/rank reset to zero. Five real UI-cooked dishes earned 31.00 in that sampled run; Buy through EventSystem raycast/pointer dispatch produced rank 1 and wallet 11.00. Repeated pointer dispatch did not buy twice. After the next frame, UI showed next price 44.00 and insufficient balance.
- `RunInteractionChecks()` passed three Back topmost-raycast/pointer assertions. Initial raycasts failed while the background Editor remained at frame 1 and new graphics had depth -1; advancing one frame with EditorApplication.Step resolved this. Run interaction checks after a rendered/advanced frame, not immediately after changing active panels. Physical mouse input was NOT tested.
- Inspected rendered upgrade panel and location navigation. Screen capture can lag state: exact post-purchase values were verified from live UI fields rather than claiming the screenshot depicted them.
- No ClickCook runtime exception observed. Existing Plastic/UVCS NotConfiguredClientException recurred on domain reload; unrelated and not changed. One transient MCP transport failure during entering Play Mode recovered on the next status/eval call.
- Existing DishData/LocationData/LocationCatalog and project settings have no diff. Scene saved; verification leaves Play Mode stopped. Current Dormitory valid two-dish pool confirmed; previous zero-pool documentation corrected as historical.

### Limits and next step

Only Click Strength is playable. No saves, recipe/location gates, other upgrade effects, final tree UI, passive/time-bonus policy, new rarity rules or production build are claimed. Human pacing/full economy balance remains unverified. Next roadmap slice: 7A, one in-memory player profile with stable IDs and separation from day/screen lifetime, preparing 7B save/load. Stop after the 6A Git checkpoint.

### Upgrade design checkpoint — 2026-10-08, not implemented

Prepared `Assets/Documentation/ClickCook_Upgrade_Design_Proposal.md`: all 16 GDD leaf nodes, provisional prices/effects/caps/prerequisites, purchase safety, subsystem gating and staged implementation recommendations. Referenced from GDD Section 13 without changing its original design requirements or marking upgrades complete.

The draft uses the available FishSim screenshots and primary Kongregate/Orteil references; the remaining Figma screenshots were unavailable due to MCP quota. Executed 1,152 mathematical price checks and conditional economic simulations, NOT Unity tests or human pacing validation. Full economy balance depends on unresolved recipe/location progression. Bonus-choice windows, passive-income accrual policy and the numeric upgrade package require approval. Recommended first implementation slice after approval: 6A, run-local multilevel Click Strength and safe between-day purchases; other unsupported nodes remain unavailable. No runtime/scene/data changes, implementation commit or push performed by this design checkpoint.

## UI/UX planning checkpoint — 2026-10-12 (not implemented)

The user paused the next gameplay step and supplied five screenshots: three screen-flow prototypes and two visual references. Created `ClickCook_UI_UX_Specification.md` covering all screens, visual/text tokens, Ukrainian copy proposals, component states, layout/input/accessibility, data bindings and the distinction between illustrative screenshot values and approved GDD mechanics.

Updated `ClickCook_Production_Roadmap.md`: recommend UI-A1 (shared visual components + Location Selection/Click Strength purchase), then UI-A2 (Board/Cooking/Result), before returning to 7A/7B. Complete other screens alongside their runtime dependencies. Both UI-A steps remain not started; no gameplay, Unity scenes/assets, Figma, commit or push changed by this planning checkpoint. Primary GDD rules and implementation checkboxes remain unchanged. Sample bars are not interpreted as hidden HP or new timers.

## Completed UI-A1 — Location Selection / Click Strength UI (2026-10-12)

### Implemented subset

- Reused SampleScene Canvas/EventSystem and all controller bindings. Styled only Location Selection and Click Strength purchase with a blue backdrop, paper panels, orange controls, larger readable type, Ukrainian presentation copy and explicit focus navigation/outline. Disabled purchase is skipped by navigation. Gameplay math/ownership/order rules are unchanged.
- Created three original procedural sprites with sliced paper/button borders (not cropped screenshots), and four reusable component prefabs. Shared venue shape is labeled as a stylized placeholder, not final location art. Used Unity's existing LegacyRuntime font after checking Ukrainian glyphs; no external font import, TMP resources or packages.
- Location data fields show actual level, income and candidate count; display at most three candidate names plus remaining count. Income is explicitly unimplemented, progress bars are not invented. Static IDs/names/numeric assets are unchanged. Only these two screens and the day-finished message have Ukrainian copy; other screens remain for UI-A2.

### Files

- Created `Assets/Scripts/UI/ClickCookUiCopy.cs`, `UiFocusOutline.cs` (+ metas/folder meta).
- Created `Assets/Scripts/Editor/UiA1Setup.cs` (+ metas): explicit one-time live-Editor authoring helper; refuses an already authored screen, clean-scene check, procedural art generation, component prefabs and scene layout. Not player runtime code.
- Created `Assets/UI/Art/MenuBlue.png`, `PaperPanel.png`, `PrimaryButton.png` (+ metas).
- Created `Assets/UI/Prefabs/Components/PaperPanel.prefab`, `PrimaryButton.prefab`, `SecondaryButton.prefab`, `WalletBadge.prefab` (+ metas/folder metas).
- Modified `Assets/Scripts/Runtime/ClickCookPrototypeController.cs`: presentation copy, formatted price labels, focus entry/return and disabled-control navigation; existing transaction/completion calls preserved.
- Modified `Assets/Scenes/SampleScene.unity`: only prototype UI authoring. Serialization comparison: zero existing blocks removed, 60 existing blocks changed, 94 new blocks. Static data/project settings/packages have no diff. Environment/camera/lights/URP content retained.
- Modified `Assets/Tests/Editor/Milestone6AVerification.cs`: price-text expectation uses the presentation formatter instead of a hardcoded English decimal string; gameplay assertions retained.
- Created `Assets/Tests/Editor/UiA1Verification.cs` (+ meta): Editor-only assets/font/contrast, rendered Canvas bounds/text, topmost raycast, focus and temporary Game View resolution checks. Invoke SetResolution only after entering Play Mode, resume/focus Game View so it renders, and RestoreResolution before leaving Play Mode.
- Updated GDD, UI/UX specification, roadmap and this tracker. Previous planning changes are preserved.

### Executed verification

- Unity recompiled successfully, compilationFailed=false; saved SampleScene, clean and stopped after testing.
- 12 asset/font/palette contrast checks passed. LegacyRuntime includes UA glyphs; text-on-paper, secondary text, header-on-blue and primary-button text meet the tested 4.5:1 color-pair target. Rendered-texture inspection was separate.
- Current-loop regression passed with 237 Play Mode assertions, including all 11 locations, wrapping, purchases, base/upgraded cooking, result/Continue, early Finish and a ten-dish day. Repeated after final focus/copy changes.
- Glyph/label-height/button-bounds/focus/topmost-raycast checks passed for both screens at actual rendered Canvas sizes 1280×720, 1920×1080, 2560×1440, 1920×1200 and 2560×1080. Checked the longest Michelin title and large wallet/price states; rendered Location Selection and purchase screen inspected. Initial matrix attempts failed because paused/background Game View still had the previous Canvas size; resuming/focusing rendered the requested sizes and successful checks replaced those failed attempts. Resized screenshot dimensions alone were not counted as validation.
- Max-rank fixture: rank-14 price 1,244,365.00, large wallet display, EventSystem pointer purchase to rank 15 and repeated dispatch protection passed; max-state text/button bounds passed. Fixture funding/rank setup occurred only in Play Mode verification; new-run progress is unchanged.
- Back EventSystem raycast/pointer dispatch passed three assertions. Physical mouse/keyboard input is NOT VERIFIED.
- Restored the original Game View selection and removed only the temporary test preset. Removed only eight generated failed-resolution captures and their metas from Assets/Temp/UI-A1, then its newly created empty parent; no user assets deleted.
- No ClickCook runtime exception observed. Existing Plastic/UVCS NotConfiguredClientException recurs on domain reload. An early MCP main-thread timeout recovered; no gameplay fix was needed.

### Initial blocker history (resolved below)

Computer Use was explicitly attempted using the installed skill and @oai/sky through node_repl. Initialization failed with `failed to write kernel assets: The system cannot find the path specified. (os error 3)`; retry and kernel reset/reinitialize produced the same failure. No native inputs were issued and no substitute Windows automation was used.

At that initial checkpoint, physical input acceptance remained pending and commit/push was deferred. This historical blocker was subsequently resolved by the recovery and acceptance checks below. No profile/save implementation started.

### UI-A1 recovery and native-input review — 2026-10-12

Computer Use initialized successfully and targeted the ClickCook Unity window. Native mouse checks passed for carousel 0→10→0 wrapping and long title/correct fields, opening/closing upgrades, disabled zero-wallet purchase, Cook into Dormitory Work Day, and early Finish with cleared day state. On a Play Mode-only wallet fixture of 60, a native purchase produced rank 1, damage 2 and wallet 40; the next price 44 was correctly unavailable. No fixture values were saved.

Native purchase exposed a focus defect: disabling the consumed offer cleared EventSystem selection before the existing refresh guard ran. Updated controller LateUpdate to restore selection only when absent, choosing the next available Buy or Back. Recompiled successfully, reran all 237 Play Mode regression assertions and 12 static checks; native purchase retest confirmed Back focus and visible outline. SampleScene remains clean; six original root objects remain, static data/packages/project settings are unchanged.

Computer Use keyboard chords operate Editor menus, but Right/Down/Left/D/Enter did not trigger runtime Navigate/Submit even with Game View focus and enabled correct bindings. This is not evidence that a physical keyboard works or fails. Requested a physical keyboard check from the user; keyboard acceptance and commit/push remain pending. Left Play Mode open on Upgrades with Back selected for that check; wallet 40/rank 1 are temporary fixture state and reset when Play Mode stops. No workaround input implementation or project-settings changes were introduced. One MCP observation timed out at 5000ms and logged a pipeline error, then recovered; one post-domain-reload connection reset also recovered. Existing UVCS configuration errors remain unrelated. No ClickCook exception observed.

### Final acceptance — 2026-10-12

The user confirmed physical Enter and arrow navigation work in the Unity Game View. Together with the executed native mouse, automated regression and layout checks, this closes UI-A1 acceptance. Keyboard verification is user-executed, not claimed as successful Computer Use key injection. Stopped Play Mode afterward: temporary wallet/rank/input-observation fixtures were discarded; SampleScene is saved/clean, with the six original roots intact and no C# compilation failures. Known unrelated UVCS configuration errors remain.

UI-A1 is complete; full UI/UX, final art and remaining screens are still unfinished. The checkpoint includes the related UI planning documents, scene/UI code/assets/metas and verification helpers. Recommended next separate step: UI-A2, Order Board/HUD, Cooking and Dish Result presentation using these shared components. No UI-A2 or 7A implementation started.

## Completed UI-A2 — Work Day / Cooking / Dish Result presentation (2026-10-12)

### Implemented

- Reused the existing three screens, buttons, Canvas/EventSystem, UI-A1 sprites/font/focus outlines and current order/reward session. Work Day shows wallet, completed dishes / 10 and orders / actual capacity separately; six possible paper cards occupy a non-overlapping right-hand 3×2 area. Full documented candidate text remains populated but hidden in the Editor-inspectable component, separate from active orders. No eligibility or capacity values changed.
- Cooking shows the selected dish/location, a large clickable plate sketch, a click instruction and an explicit Back reset warning. No HP, damage numbers, fake progress or new effects. Finish remains a board-only action.
- Dish Result shows Ukrainian rarity copy plus color cue, exact immutable receipt payout, actual wallet, XP award and level change. A labeled mastery bar uses actual level XP / transition cost; cap reads maximum/full. Continue uses existing lifecycle; tenth-result label becomes Finish Day. New UI state clears with the existing result lifetime.
- Focus transfers to the first available order (Finish for empty board), dish, Continue or Cook on state entry. Board navigation excludes disabled slots. All art is explicitly temporary; final food illustrations/models/audio/animations are not introduced.

### Files created / modified

- Created `Assets/Scripts/Editor/UiA2Setup.cs` (+ meta): explicit stopped-Editor authoring helper, preserving existing object bindings; paper-backed contrast corrections and draw order.
- Created `Assets/Tests/Editor/UiA2Verification.cs` (+ meta): Editor-only MCP-invoked real-controller presentation/lifecycle, rendered-layout/raycast/focus and transient empty/mastery fixture checks; not registered NUnit tests.
- Created `Assets/UI/Art/PlateCircle.png` and `Assets/UI/Prefabs/Components/OrderCard.prefab` (+ metas); circle is original procedural UI art, not a screenshot crop.
- Modified `Assets/Scripts/Runtime/ClickCookPrototypeController.cs`: five presentation references, Ukrainian day/cooking/receipt copy, real mastery fill, tenth CTA and focus/navigation only; order identity/completion/replacement/payout/XP/upgrade formulas unchanged.
- Modified `Assets/Scripts/UI/ClickCookUiCopy.cs`: Ukrainian display labels for the five canonical rarity tiers; canonical enum/data IDs unchanged.
- Modified `Assets/Scenes/SampleScene.unity`: UI authoring only. Serialization review found zero existing blocks removed; 83 modified existing blocks all belong to ClickCook prototype UI/controller. Dormitory/camera/lights/URP preserved; Data/ProjectSettings/Packages have no diff.
- Updated GDD, this tracker, roadmap and UI/UX specification. No profile/save implementation started.

### Verification actually executed

- Unity 6000.3.14f1 compiled successfully with no C# errors. Existing 6A regression passed 237 assertions: all 11 locations before/after purchases, exact cooking, Back, rewards/replacement and full ten-result day. UI-A2 presentation checks passed 240 assertions across 11 boards and ten receipts/Continue; empty-pool/mastery-cap fixtures passed ten more. Fixtures were transient Play Mode state, never saved gameplay assets.
- All three screens passed 795 total glyph/height/button-bounds/non-overlap/topmost-raycast/focus assertions at actual rendered Canvas 1280×720, 1920×1080, 2560×1440, 1920×1200 and 2560×1080. Game View preset was restored before stopping Play Mode. A zoomed Editor viewport screenshot during the fixed-resolution matrix was cropped by Game View scaling; this is not claimed as a complete visual inspection of every target resolution. Canvas checks and final fitted native-view inspection are separate evidence.
- Computer Use native mouse executed Cook → order → cooking. Back after a partial click preserved that exact order identity and zero counter/wallet. In a fresh run, native 2+2+1 sandwich clicks completed on the fifth click: Common, payout/wallet 5.00, +10 XP, level 1 and 10/20 mastery. Native Continue preserved wallet 5, showed 1/10 and two orders with a new slot-0 identity; native Finish returned to Location Selection with the correct one-dish message. These are real pointer inputs, not callback-only claims.
- Visually corrected low-contrast headings over the ceiling, cooking instruction obscured by the decorative plate, and tiny icon copy clipping. Retested after corrections. Keyboard focus/navigation used EventSystem move tests; physical keyboard input on the new screens was NOT tested (UI-A1 physical Enter/arrows were user-verified previously). Production build/human pacing were not tested.
- No ClickCook runtime exceptions observed. Existing Plastic/UVCS client configuration errors remain; transient MCP network/domain-reload and main-thread timeout errors recovered, successful explicit reruns replace inconclusive tool calls.
- Final SampleScene saved clean outside Play Mode, original six roots retained. Temporary run state reset, no test captures or test presets left in Assets.

### Next separate step

UI-A2 is complete for the defined presentation slice. Full UI, final dish/location art, End-of-Day report, saves and release polish remain unfinished. Return to roadmap 7A: one in-memory player profile with stable IDs and wallet/dish mastery/Click Strength ownership separated from day/screen lifetime; 7B save/load follows later. Do not implement 7A in this execution.

## Completed UI-A3 — Designer playtest corrections (2026-10-12)

The user explicitly requested Exit in the menu, visible dish HP/click progress and central dish selection cards following a new gameplay screenshot. These decisions supersede prior hidden-HP/right-board presentation constraints; original HP/damage/XP/payout/capacity formulas and authored data stay intact.

- Current prototype Location Selection menu now has «Вийти з гри». Native mouse test stopped Play Mode without closing Unity. Standalone implementation uses Application.Quit; a standalone build/quit test was NOT executed. Full Main Menu/save/load remains 7B.
- Cooking shows remaining/max HP, a decreasing orange bar and ceil(remaining/current Click Strength) remaining clicks. Native sandwich click changed 5/5 and five clicks to 4/5 and four; Back/reselect restored 5/5. Back still preserves the order identity; no new damage or reward rule.
- Orders are large central cards above the plate: centered pairs/three-card row, or three columns × two rows for 4–6. No overlap/tilted hit areas; pool/capacity/duplicate rules unchanged. Green bars are explicitly Dish mastery, using real XP/threshold and cap; they are not cooking HP/expiry. Food art remains clearly labeled temporary sketches.
- Modified controller/SampleScene/OrderCard.prefab; created `Assets/Scripts/Editor/UiA3Setup.cs` and `Assets/Tests/Editor/UiA3Verification.cs` (+ metas). Reused UI-A2 authoring primitives through internal helpers in `UiA2Setup.cs`; no new gameplay framework/art dependencies. Updated `UiA1Verification.cs` to target the focused Game View when multiple views exist, avoiding ambiguous resolution tests. Updated GDD, roadmap, UI/UX specification and this tracker.
- Compilation passed. Existing 237 Play Mode regression and 240 presentation/lifecycle assertions passed. New review checks passed 15 assertions at base strength and 15 at purchased strength 2 (temporary Play Mode-only wallet funding), covering HP/fraction/click count/reset, central layout, real mastery and hidden Exit guard. 980 glyph/layout/bounds/non-overlap/topmost-raycast/focus assertions passed for two-card board, six-card board and cooking across 1280×720, 1920×1080, 2560×1440, 1920×1200, 2560×1080.
- First resolution check was inconclusive because two Game Views selected different rendering contexts. The temporary preset was restored, helper corrected, intended Game View focused, and all target checks explicitly rerun successfully. Original preset restored afterward; temporary maximized Game View returned to original dock layout on leaving Play Mode. No production build/physical keyboard retest claimed. Existing UVCS errors remain unrelated; no ClickCook runtime exception observed.
- SampleScene saved clean outside Play Mode; serialization comparison found zero existing blocks removed and 55 modified existing blocks, all within prototype UI/controller. Dormitory, camera and lights retained. Pre-existing unrelated changes in `Assets/Settings/PC_RPAsset.asset`, `Assets/Settings/UniversalRenderPipelineGlobalSettings.asset`, `ProjectSettings/ProjectSettings.asset`, `ProjectSettings/UnityConnectSettings.asset` were preserved and excluded from the UI checkpoint. DishData/LocationData/catalogs were not modified.

Next separate roadmap step remains 7A (run-local player profile/stable IDs), unless the user chooses another UI refinement. Final dish illustrations and standalone Quit verification are explicitly pending.

## UI-A4 — Scene-bound board and plate interaction (2026-10-15)

Completed the existing local work rather than rebuilding the prototype. Six existing order buttons now belong to a world-space Canvas anchored to the actual Dormitory noticeboard. Board capacity/pool/duplicate rules are unchanged. Selecting a sheet displays the exact selected dish on the existing plate/tray; sheets hide during close-up/result and return unchanged on Back. The old floating plate/cooking button remains inactive, with serialized bindings preserved.

- `CounterScenePresentation` handles only scene presentation: environment anchors, 2–6 sheet layout, temporary dish variant, camera look-at/FOV and parallax suspension/restoration. Board framing uses a 12-degree presentation yaw, leaving the authored camera pose unchanged. Mouse-follow normalization clamps outside-window coordinates to keep the counter bounded.
- `CounterDishPointer` routes native collider pointer clicks and EventSystem Submit through the existing cooking guard. Explicit arrow navigation connects the dish target with Back; the hint distinguishes temporary 3D art and shows keyboard input while focused. No new damage/reward/order architecture or upgrade effect.
- Simple sandwich and mug-cake sketches reuse six new URP materials; other dishes use a labeled neutral sketch. Final food models/card illustrations and other location environments remain unimplemented.

### Files

- Created `Assets/Scripts/UI/CounterScenePresentation.cs`, `Assets/Scripts/UI/CounterDishPointer.cs`, `Assets/Scripts/Editor/UiA4Setup.cs`, `Assets/Tests/Editor/CounterSceneVerification.cs` and their `.meta` files. Authoring helpers are explicit stopped-Editor operations; tests are MCP-invoked Editor checks, not registered NUnit tests.
- Created six materials and `.meta` files under `Assets/Art/Prototype/Materials/`, plus folder metadata for Art/Prototype/Materials.
- Modified `Assets/Scripts/Runtime/ClickCookPrototypeController.cs` (world-space layout bypass, board hint, safe initial focus), `Assets/Scripts/Camera_Move.cs` (read-only cursor rotation and normalized input bounds), and `Assets/Scenes/SampleScene.unity`.
- Updated GDD, this tracker, roadmap, UI/UX specification and the upgrade proposal's next-step notes. No changes to DishData, LocationData, catalogs, environment meshes/materials, Packages or project settings in this milestone. Pre-existing settings diffs remain excluded.

### Verification executed

- Unity 6000.3.14f1 compilation PASS, no C# failures. Existing regression: 4,685 model assertions (32 dishes × 16 strength ranks), 237 Play Mode, 240 presentation/lifecycle and 10 empty/mastery edge assertions PASS.
- Frame-settled world-space verification ran all 11 locations at every rendered/input size below. It tests paper corners, topmost rays, text/glyphs, HUD bounds, exact selected instance, one active order, Submit/arrow event routing, Back/no regeneration/reset HP, Finish cleanup, transient empty/independent-duplicate fixtures, ten results/replacements and stale callbacks. These are automated events, NOT physical input claims.

| Actual render size | Lifecycle/input assertions | Spatial/text/glyph/raycast assertions | Result |
|---|---:|---:|---|
| 1280×720 | 176 | 13,812 | PASS |
| 1920×1080 | 176 | 13,851 | PASS |
| 2560×1440 | 176 | 13,918 | PASS |
| 1920×1200 | 176 | 13,905 | PASS |
| 2560×1080 | 176 | 13,905 | PASS |

- Native Computer Use mouse PASS: carousel 0→10→0, Cook, physical sheet → correct mug-cake order, actual 3D dish HP 8→7, Back preserving both IDs and completed=0, reselect HP=8, eight physical clicks → Common result/8.00 wallet/+10 XP/one completion, Continue retaining the replacement IDs without payment, Finish clearing orders and retaining earned wallet, and a fresh day with fresh IDs/completed=0. A six-card location was prepared through MCP, then native mouse selected slot 6/Imperial (70,000 HP), returned with Back and finished; no physical late-dish completion is claimed.
- Initial diagnostics found an offscreen third-sheet edge at fixed 720p; adjusted the board-only camera yaw and reran the complete five-size matrix successfully. Two simultaneous Game Views initially produced conflicting camera/input aspect contexts; final tests targeted the intended maximized Game View and checked actual render/input dimensions. Original resolution preset was restored before stopping Play Mode.
- Existing scene blocks removed: zero; 23 existing blocks changed, all prototype presentation except the Camera GameObject gaining a PhysicsRaycaster. Original Dormitory objects, camera Transform, lights and URP volume retained. No static gameplay asset diff. Scene saved outside Play Mode; Play Mode stopped.
- Console contains only the known Plastic/UVCS client configuration error; no new ClickCook runtime exception observed. No standalone build/quit or physical-keyboard retest was executed. Runtime Submit/move was tested through EventSystem, not synthetic keys presented as real hardware. Full production typography/art/performance acceptance remains later work.

Next separate milestone: 7A — in-memory player profile/stable IDs, followed by 7B persistence. Expanded tree U0–U6 remains planned, not implemented.

## Milestone 7A — run-owned player profile and stable IDs — completed 2026-10-15

### Implemented scope and files

- New `Assets/Scripts/Runtime/PlayerProfile.cs`: owns one existing `PrototypeDishRewards` component (wallet/Dish XP) and ranks keyed by stable upgrade ID. A profile-wide active-day guard prevents concurrent days and purchases through another session while a day is active.
- New `Assets/Scripts/Runtime/PlayerProfileRuntime.cs`: run-lifetime owner, not a screen/controller owner. SubsystemRegistration resets it for a new run; no disk I/O or persistent Unity object is required.
- New `Assets/Scripts/Runtime/PlayerProfileSnapshot.cs`: detached read-only copies of exact decimal wallet, numeric dish-ID→XP and upgrade-ID→rank. No Unity asset references, active orders, results or location ownership. This is an in-memory boundary, not a save/restore format.
- Modified `Assets/Scripts/Runtime/PrototypeOrderSession.cs`: injected profile, existing UI/rewards facade retained, Click Strength rank delegated to profile; day cleanup releases only its own active-day ownership. Default constructor still creates an isolated fresh profile for fixtures.
- Modified `Assets/Scripts/Runtime/PrototypeDishRewards.cs`: internal XP-copy helper only; probabilities, rewards and XP formulas unchanged.
- Modified `Assets/Scripts/Runtime/ClickCookPrototypeController.cs`: binds the run profile in Awake; OnDestroy clears its temporary day without deleting earned progress.
- New `Assets/Tests/Editor/PlayerProfileVerification.cs`; Unity generated `.meta` for each of the four new scripts. Documentation updated: GDD, this tracker and Production Roadmap.
- Identity policy reuses existing authored DishId, upgrade string IDs and catalog level keys 0–10; no assets renumbered. Location level remains a lookup key, not an inferred player-level gate. Future location/recipe/achievement progress is not invented or stored now.

### Executed Unity verification

- Unity MCP compilation: succeeded, no C# failures.
- New isolated profile tests: **58 PASS** — all 32 dish IDs, 11 unique ordered location keys, unique upgrade definitions, fresh profile, same-profile sessions, concurrent-day/purchase guards, foreign/stale quotes, single receipt, exact debit, day replacement, snapshot isolation/mutation rejection, null profile/location, discarded-order callbacks.
- Existing model regression: **4,685 PASS**, all 32 dishes × 16 strength ranks (512 exact cooking cases).
- Fresh Play Mode regression: **237 PASS** — all 11 locations before/after purchases, ten-dish day, carousel wrap, Cook/Back/Continue/Finish and repeat callback guards. Existing Back raycast/pointer test: **3 PASS**.
- Actual SampleScene reload in Play Mode: **14 PASS** — new controller/session retained the same funded profile and purchased ranks/XP; old unfinished order invalidated; a fresh day started and finished without additional rewards. No scene asset write occurred.
- Existing UI presentation **240 PASS**, empty-pool/mastery-cap fixtures **10 PASS**.
- Existing rendered-frame scene runner: **176 lifecycle/input + 14,036 spatial/text/glyph/raycast assertions PASS**, all 11 locations, physical board/plate targets, same-order Back, exact completion/replacement, ten receipts and cleanup at the current Game View size. This was not a repeat of the five-resolution UI-A4 matrix. The runner was initially stalled by Unity's normal unfocused Game View behavior; `Application.runInBackground` was temporarily enabled at runtime and restored to false before stopping. No project setting was edited.
- A second fresh Play Mode entry explicitly verified zero wallet, empty XP/rank snapshots, no active day and the controller bound to the run owner. Play Mode then stopped; SampleScene clean. The domain-reload-disabled configuration itself was not toggled/tested; the SubsystemRegistration reset hook is present.
- No new ClickCook runtime exceptions observed. Only the pre-existing Plastic/UVCS missing client configuration exception remains. These tests used Unity MCP, model calls and EventSystem/button callbacks; no new physical keyboard/mouse or standalone build test is claimed.
- `SampleScene`, static data, environment, packages and UI assets unchanged. Four pre-existing settings edits remain preserved and excluded from the checkpoint: PC_RPAsset, UniversalRenderPipelineGlobalSettings, ProjectSettings and UnityConnectSettings.

### Intentionally unfinished / next step

7A is complete only for run-local progress ownership. No save file, restore, autosave, New Game/reset UI, Player/Restaurant XP, new upgrade effects or balance changes. Expanded tree and other systems remain pending. Next separate milestone: **7B — local save/load and associated menu/recovery behavior**, after explicitly documenting the interrupted-day/New Game/reset/pause policies. Do not start 7B automatically.

## Documentation policy (ongoing)

After every completed and Unity-verified milestone, update both `ClickCook_GDD.md` and this file. Do not mark an entire gameplay system complete when only static data or placeholder UI exists.
