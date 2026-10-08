# ClickCook Implementation Status

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

- Location unlocking and ownership.
- Recipe ownership/gating.
- Real order completion/replacement, daily-limit enforcement, and day-ending cleanup.
- Economy, rewards, XP, save/load, persistence, cooking, timers, scoring, final UI, and final animations.

## Known GDD ambiguities / TBDs retained

- The GDD separately describes new recipe groups and dishes served by locations; their precise relationship and recipe purchase gating are TBD.
- Several location descriptions use incomplete phrases such as “also serves selected ... dishes.” No extra dishes were inferred.
- The Michelin restaurant says “all dishes starting from the third restaurant,” which conflicts with / is less precise than its criteria dish list. No additional earlier dishes are inferred.
- The click requirement’s relationship to future hidden HP/progress remains TBD.
- Location ownership/unlock state, passive-income accrual/collection, recipes, and runtime rules beyond initial prototype order population/selection remain unimplemented by design.

## Files created

- `Assets/Scripts/Data/DishData.cs`
- `Assets/Scripts/Data/LocationData.cs`
- `Assets/Scripts/Runtime/Locations/LocationCatalog.cs`
- `Assets/Scripts/Runtime/ClickCookPrototypeController.cs`
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

- Core game flow: Location Selection → Work Day board → order selection → cooking placeholder / Back exists (4A); actual cooking and completion remain absent.
- Location system: static data and browsing exist; unlocking, ownership, real progress, passive-income accrual, and recipe gating do not.
- Dish system: static data exists; runtime recipes, dish progression, cooking, and presentation do not.
- Work Day: selected-location context, documented dish candidates (Milestone 3), initial orders/selection (4A), and a Finish Day placeholder exist; earnings, completion/replacement, summary, and next-day behavior do not.
- UI: temporary prototype screens exist; final navigation, styling, responsiveness, feedback, and all other screens do not.

### Not implemented

- Main Menu, completed-order lifecycle/replacement, cooking/clicking, dish results, rarity, economy, rewards, XP/progression, unlock systems, upgrades, save/load, achievements, journal, settings, pause, final animations, and audio content.

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

### Remaining limitations and recommended Milestone 4B

Actual cooking, order completion/replacement, daily counting/limit enforcement, end-of-day cleanup/summary, economy, progression, rarity, and persistence remain unimplemented. Recommend a separate 4B prototype lifecycle slice for explicit simulated completion, exact-instance removal/counting/replacement up to ten, and early-day order cleanup, without cooking formulas or rewards.

Before 4B, approve the prototype end-state behavior at ten completions and the Finish Day destination. Final capacity balancing, future daily-limit increases, final recipe eligibility/availability, timers, click/HP math, rarity, and rewards remain open. No 4B implementation was started.

## Documentation policy

After every completed and Unity-verified milestone, update both `ClickCook_GDD.md` and this file. Do not mark an entire gameplay system complete when only static data or placeholder UI exists.
