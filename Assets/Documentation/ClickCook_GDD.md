# ClickCook — Game Design Document

## 1. Game Overview

**Game:** ClickCook  
**Genre:** Restaurant/cooking clicker with collection, progression, and upgrade systems.

- [DESIGNED] The player selects a location, begins a work day, selects orders, prepares dishes through close-up clicking, receives a rarity result, earns currency/XP, and progresses through upgrades and locations.
- [IMPLEMENTED] Unity contains the early 3D environment/camera prototype, static location/dish data, and a verified placeholder Location Selection → Work Day flow. The complete gameplay loop, economy, and progression are not implemented.
- [INFERRED] The intended experience combines a persistent restaurant-management loop with focused, tactile dish-completion moments.
- [TBD] Platform, session length, target audience, monetization, and final scope.

## 2. Core Gameplay Loop

```text
Main Menu
→ Location Selection
→ Start Work Day
→ Order Board
→ Select Order
→ Cooking / Click Dish
→ Dish Complete
→ Rarity Result
→ More Orders
→ Finish Day
→ End-of-Day Summary
→ Upgrades / Next Day
```

- [DESIGNED] The order board offers dishes available at the chosen location.
- [DESIGNED] Cooking presents the selected dish in close-up while the restaurant remains visible.
- [DESIGNED] Each dish has hidden HP / required click progress.
- [DESIGNED] Completion produces a rarity result.
- [DESIGNED] Daily XP formula: `Daily Earnings / 100 = XP`.
- [TBD] Exact order replacement, work-day completion, timing, failure, and rarity calculations.

## 3. Complete Game Flow / State Machine

| State | Entry | Player actions | Exit | Known rules |
|---|---|---|---|---|
| Main Menu | Launch / return from play | Continue, New Game, Settings, Exit, Discord | Choose action | [DESIGNED] Menu items exist |
| Location Selection | New/continued session | Browse locations, inspect dishes/progress, Cook | Press Cook | [DESIGNED] Carousel/card UI exists |
| Work Day / Order Board | Start work day | Select an order, Finish Day | Select dish / finish day | [DESIGNED] Currency, remaining dishes, six visible slots |
| Cooking | Select order | Click dish; potentially use click-related upgrades | Dish completion | [DESIGNED] Close-up dish, hidden progress |
| Dish Result | Dish reaches completion | Continue | Return to orders | [DESIGNED] Dish, rarity, value shown |
| End of Day | Finish Day | View prepared dishes/results, Upgrades, Next Day | Upgrade screen / next day | [DESIGNED] Summary wireframe |
| Upgrades | End-of-day action/navigation | Inspect/purchase upgrades | Return | [DESIGNED] Upgrade tree exists |
| Achievements | Navigation | Inspect achievement list | Return | [DESIGNED] Screen exists |
| Journal | Navigation | Browse collection | Return | [DESIGNED] Grid/list screen exists |
| Settings | Main menu/pause | Change settings/reset data | Return | [DESIGNED] Settings categories exist |
| Pause | During gameplay | TBD | Resume/exit TBD | [DESIGNED] Screen exists |

- [TBD] Transition restrictions, animation timing, pause behavior, saving points, and state restoration.

## 4. Main Menu

- [DESIGNED] Title: `ClickCook`.
- [DESIGNED] Actions: Continue, New Game, Settings, Exit.
- [DESIGNED] Version label.
- [DESIGNED] Discord label/link area.
- [TBD] Continue eligibility, save-slot behavior, New Game confirmation, version format, Discord destination, Exit behavior by platform.

## 5. Location System

### Locations

| Level | Location | Passive income/sec | Unlock requirement | Available dishes / recipe group | Notes | Source |
|---:|---|---:|---|---|---|---|
| 0 | Dormitory / `Общежитие` | 0 | Listed level 0 | Sandwiches; mug cakes | Prototype environment shares this name | [F] |
| 1 | Café / `кафе` | 1 | Listed level 1 | French hot dog; donuts | Also serves dormitory dishes | [F] |
| 2 | McDonald’s-style snack bar / `закусочная типа мака` | 5 | Listed level 2 | Fries; burgers | Also serves café dishes | [F] |
| 3 | Festival food court / `фудкорт фестиваля` | 15 | Listed level 3 | Vegetable salad; kebab | Also serves listed earlier dishes | [F] |
| 4 | Shashlik/grill restaurant / `шашлычная` | 30 | Listed level 4 | Grilled vegetables; grilled meat; grilled fish | Grill-focused group | [F] |
| 5 | Home restaurant / `домашний ресторан` | 50 | Listed level 5 | Pasta Bolognese; pizza; pie | Also serves grill dishes | [F] |
| 6 | Italian restaurant / `итальянский ресторан` | 75 | Listed level 6 | Gelato; tiramisu; rum baba | Also serves pasta/pizza/grilled fish | [F] |
| 7 | Pastry shop / `кондитерская` | 105 | Listed level 7 | Mousse pastries; macarons; croissant | Also serves pie/Italian desserts | [F] |
| 8 | Japanese restaurant / `японский ресторан` | 140 | Listed level 8 | Premium ramen; mochi; tartare; scallops | Also serves selected grill/pastry dishes | [F] |
| 9 | Fashion restaurant / `фешн ресторан` | 180 | Listed level 9 | Beef Wellington; risotto; duck confit; octopus | Also serves selected Japanese/pastry dishes | [F] |
| 10 | Player’s Michelin restaurant / `свой мишлен ресторан` | 220 | Listed level 10 | Level-9 dishes plus Lobster Thermidor, eel sushi, caviar pancakes, imperial set | “All dishes starting from third restaurant” is ambiguous | [F] |

### Location UI and rules

- [DESIGNED] Location selection includes left/right arrows, location name, progress display, dish slots, income/sec, and `Cook`.
- [DESIGNED] Locations determine which dishes can appear on the order board.
- [TBD] Unlock costs, non-level unlock conditions, ownership rules, carousel wrapping, progress meaning, passive-income collection, and whether recipe purchase gates appearance.

## 6. Work Day

- [DESIGNED] A day begins after selecting a location and pressing `Cook`.
- [DESIGNED] Gameplay includes a `Finish day` action.
- [DESIGNED] End-of-day UI includes location name, prepared dish rows, result area, date, Upgrades, and Next Day.
- [DESIGNED] Daily XP: `Daily Earnings / 100`.
- [TBD] Day duration, order count, minimum completion, early-finish consequences, failure states, penalties, daily reset behavior, date advancement, and earnings formula.

## 7. Order Board

- [DESIGNED] The board is on the right side of gameplay screens.
- [DESIGNED] It displays `Количество блюд осталось - ..` / dishes remaining.
- [DESIGNED] Current currency is shown above it; example: `215 у.е.`
- [INFERRED] Six visible slots form a 3×2 order grid.
- [INFERRED] A completed/selected order slot may become blank.
- [DESIGNED] Orders are selected by the player before cooking.
- [TBD] Generation algorithm, random weights, order refill/removal, timeout, selection limit, repeats, rarity influence, and rewards timing.

## 8. Dish System

| ID | Name | Sale value | Recipe cost | Click requirement | Known locations | Source |
|---:|---|---:|---:|---:|---|---|
| 1 | Sandwiches / `Сендвичи` | 5 | 0 | 5 | Dormitory, Café, Festival food court | [F] |
| 2 | Mug cakes / `Кексы в чашке` | 8 | 150 | 8 | Dormitory, Café | [F] |
| 3 | French hot dog / `Французский хот дог` | 12 | 250 | 13 | Café, McDonald’s-style snack bar | [F] |
| 4 | Donuts / `Пончики` | 18 | 450 | 20 | Café, McDonald’s-style snack bar | [F] |
| 5 | Fries / `Картошка фри` | 25 | 700 | 30 | McDonald’s-style snack bar, Festival food court | [F] |
| 6 | Burgers / `Бургеры` | 40 | 1,200 | 42 | McDonald’s-style snack bar, Festival food court | [F] |
| 7 | Vegetable salad / `Салат овощной` | 60 | 1,800 | 58 | Festival food court, Shashlik/grill restaurant | [F] |
| 8 | Kebab / `Кебаб` | 80 | 2,500 | 80 | Festival food court, Shashlik/grill restaurant | [F] |
| 9 | Grilled vegetables / `Овощи на гриле` | 120 | 3,500 | 110 | Shashlik/grill, Home restaurant, Japanese restaurant | [F] |
| 10 | Grilled meat / `Мясо на гриле` | 180 | 5,000 | 150 | Shashlik/grill, Home restaurant | [F] |
| 11 | Grilled fish / `Рыба на гриле` | 250 | 7,000 | 205 | Shashlik/grill, Home restaurant, Italian restaurant | [F] |
| 12 | Pasta Bolognese / `Паста Болоньеза` | 350 | 9,500 | 280 | Home restaurant, Italian restaurant | [F] |
| 13 | Pizza / `Пицца` | 500 | 13,000 | 380 | Home restaurant, Italian restaurant | [F] |
| 14 | Pie / `Пирог` | 700 | 18,000 | 520 | Home restaurant, Pastry shop | [F] |
| 15 | Gelato / `Джелатто` | 950 | 24,000 | 700 | Italian restaurant, Pastry shop | [F] |
| 16 | Tiramisu / `Тирамису` | 1,300 | 32,000 | 950 | Italian restaurant, Pastry shop | [F] |
| 17 | Rum baba / `Ромовая баба` | 1,800 | 42,000 | 1,280 | Italian restaurant, Pastry shop | [F] |
| 18 | Mousse pastries / `Муссовые пироженные` | 2,400 | 55,000 | 1,730 | Pastry shop, Japanese restaurant | [F] |
| 19 | Macaron / `Макарон` | 3,200 | 70,000 | 2,330 | Pastry shop, Fashion restaurant | [F] |
| 20 | Croissant / `Круассан` | 4,300 | 90,000 | 3,140 | Pastry shop, Japanese restaurant | [F] |
| 21 | Premium ramen / `Рамен Примиум` | 5,600 | 115,000 | 4,220 | Japanese restaurant, Fashion restaurant | [F] |
| 22 | Mochi / `Моти` | 7,200 | 145,000 | 5,670 | Japanese restaurant, Fashion restaurant | [F] |
| 23 | Tartare / `Тартар` | 9,200 | 180,000 | 7,600 | Japanese restaurant, Fashion restaurant | [F] |
| 24 | Scallops / `Морские гребешки` | 11,500 | 220,000 | 10,200 | Japanese restaurant, Fashion restaurant | [F] |
| 25 | Beef Wellington / `Филе Веллингтон` | 14,500 | 270,000 | 13,600 | Fashion restaurant | [F] |
| 26 | Risotto ai Funghi / `Ризотто ai Funghi` | 18,000 | 330,000 | 18,200 | Fashion restaurant | [F] |
| 27 | Duck confit / `Утка Конфи` | 22,000 | 400,000 | 24,300 | Fashion restaurant | [F] |
| 28 | Octopus / `Осьминог` | 27,000 | 500,000 | 32,500 | Fashion restaurant | [F] |
| 29 | Eel sushi / `Суши с угрем` | 33,000 | 620,000 | 43,300 | Michelin restaurant criteria list | [F] |
| 30 | Lobster Thermidor / `Лобстер Термидор` | 40,000 | 760,000 | 52,000 | Michelin restaurant criteria list | [F] |
| 31 | Caviar with pancakes / `Икра с блинами` | 48,000 | 920,000 | 60,000 | Michelin restaurant criteria list | [F] |
| 32 | Imperial tasting set / `Императорский дегустационный сет` | 60,000 | 1,100,000 | 70,000 | Michelin restaurant criteria list | [F] |

- [DESIGNED] The table label is `Количество кликов` / number of clicks.
- [TBD] Whether click requirement is literal hidden HP, recipe-purchase behavior, dish unlock representation, recipe UI, dish art, dish-specific XP awards, and modifiers.

## 9. Cooking / Clicking Mechanics

- [DESIGNED] Select an order, enter close-up cooking presentation, click the dish, reduce hidden dish HP/progress, complete dish, reveal rarity.
- [DESIGNED] The restaurant environment remains visible around the cooking presentation.
- [DESIGNED] Relevant upgrade branches: click strength, critical hit, click hold, auto-click strength, auto-click frequency.
- [DESIGNED] Screen 2 shows a central vertical feedback area with example values `67` and `23`, plus flame/impact graphics.
- [TBD] Meaning of `67` and `23`; click damage; HP formula; whether number-of-clicks equals base HP; click feedback; critical formula; hold behavior; auto-click activation; accessibility considerations.

## 10. Dish XP

- [DESIGNED] Upgrade tree includes XP branches: Character, Restaurant, Dish.
- [DESIGNED] Higher rarity should depend on player progression/experience with that specific dish.
- [TBD] Dish XP gain, level thresholds, max level, persistence, UI, rewards, and exact rarity relationship.

## 11. Rarity System

Canonical intended tiers from player-provided gameplay context:

1. Common
2. Uncommon
3. Rare
4. Fantastic
5. Legendary

- [DESIGNED] Completed-dish wireframe example: Sandwich, `Обычное`, value `5 у.е.`
- [INFERRED] Sparkles and opened cloche indicate a celebratory reveal.
- [TBD] Whether `Обычное` is the canonical equivalent of Common; probability curves; thresholds; dish-XP modifiers; currency multipliers; colors; animations; audiovisual feedback; mechanical effects.

## 12. Player XP and Level Progression

| Player level | XP value | Source |
|---:|---:|---|
| 1 | 0 | [F] |
| 2 | 100 | [F] |
| 3 | 300 | [F] |
| 4 | 700 | [F] |
| 5 | 1,400 | [F] |
| 6 | 2,500 | [F] |
| 7 | 4,200 | [F] |
| 8 | 6,600 | [F] |
| 9 | 10,000 | [F] |
| 10 | 15,000 | [F] |

- [DESIGNED] `Daily Earnings / 100 = Experience`.
- [TBD] Whether values are cumulative thresholds or per-level XP, maximum player level, benefits of player levels, and relationship between player level and location unlocks.

## 13. Upgrade System

### Complete tree

```text
Culinary / Кулинария
├─ Prices / Цены
│  ├─ Increase Bonus / Увеличение бонуса
│  ├─ Reduce Prices / Уменьшение цен
│  │  ├─ Reduce Recipe Prices
│  │  └─ Reduce Upgrade Prices
│  └─ Increase Value / Увеличение стоимости
│     ├─ Increase Dish Value
│     └─ Increase Auto-income Value
└─ XP
   ├─ Character
   ├─ Restaurant
   └─ Dish

Click Upgrades / Клики прокачка
├─ Click Strength / Сила клика
├─ Critical / Крит
│  ├─ Multiplier / Множитель
│  └─ Frequency / Частота
└─ Click Hold / Зажатие клика

Auto-click / Автоклик
├─ Strength / Сила
└─ Frequency / Частота

Passives / Пассивки
├─ Restaurant Auto-income / Автодоход ресторана
└─ Increase Dish-selection Time / Увеличение времени выбора блюда
```

- [DESIGNED] Upgrade UI is a connected-node tree with an `Active Skills Statistics` panel and `Hide` control.
- [TBD] Currency used, purchase rules, prerequisite semantics beyond visible links, costs, levels, caps, respec, displayed statistics, and exact effects.

## 14. Economy

- [DESIGNED] Currency: `у.е.`
- [DESIGNED] Example wallet amount: `215 у.е.`
- [DESIGNED] Dish sale values: `5` to `60,000 у.е.`
- [DESIGNED] Recipe costs: `0` to `1,100,000 у.е.`
- [DESIGNED] Location passive income: `0` to `220 у.е./sec`.
- [DESIGNED] Daily XP conversion: daily earnings divided by `100`.
- [TBD] Starting balance, transaction timing, passive-income accrual/collection, bonus calculations, sale-value modifiers, price-reduction order, upgrade spending, and loss/penalty systems.

## 15. Recipe and Location Unlock System

- [DESIGNED] Each dish has a recipe cost.
- [DESIGNED] Location table lists new recipe groups.
- [DESIGNED] Locations also have separately documented dishes-served lists.
- [TBD] Whether recipes are automatically granted, individually purchased, required before orders spawn, visible in Journal, or shared across locations.
- [TBD] Exact interpretation of Michelin location’s “all dishes starting from the third restaurant.”

## 16. Achievements

- [DESIGNED] An achievements screen exists with five visible rows.
- [DESIGNED] Each row has icon, title/main text, and right-side reward/value area.
- [DESIGNED] It shares the Active Skills Statistics panel.
- [TBD] Achievement names, requirements, rewards, progress, notification behavior, and persistence.

## 17. Journal

- [DESIGNED] Journal screen exists as a collection grid/list.
- [DESIGNED] It shares top navigation with Locations, Upgrades, and Achievements.
- [TBD] Entry types, dish/recipe relationship, unlock rules, text/image content, sorting/filtering, and persistence.

## 18. Settings

- [DESIGNED] Language selection.
- [DESIGNED] Display/window controls.
- [DESIGNED] Volume controls.
- [DESIGNED] Reset data action.
- [TBD] Exact labels, defaults, languages, supported display modes, audio categories, reset confirmation, and storage behavior.

## 19. Pause

- [DESIGNED] A pause screen exists.
- [TBD] Pause menu content, whether passive income/cooking stops, navigation, save behavior, and quitting behavior.

## 20. Save / Load / New Game / Continue

- [DESIGNED] Main menu contains Continue and New Game.
- [DESIGNED] Settings contains Reset Data.
- [IMPLEMENTED] No save/load code, PlayerPrefs usage, data model, or save UI exists in Unity.
- [TBD] Save slots, autosave points, serialization scope, cloud support, New Game overwrite behavior, Continue fallback behavior, reset scope, and version migration.

## 21. UI/UX

| Screen | Designed contents | Actions / feedback | TBD |
|---|---|---|---|
| Main Menu | Title, Continue, New Game, Settings, Exit, version, Discord | Navigate/select | Navigation input, transitions |
| Location Selection | Arrows, location name, progress, dish slots, income/sec, Cook | Browse/start day | Lock presentation, carousel rules |
| Order Board | Currency, dishes remaining, six cards, plate/cloche, Finish Day | Select order/end day | Order state/feedback |
| Cooking | Selected dish, plate, central `67/23` feedback, impacts/flames | Click dish | Exact display meaning/mechanics |
| Dish Result | Dish, opened cloche, sparkles, rarity/result card, value | Continue | Rarity styling/transition |
| End of Day | Prepared rows, results, date, Upgrades, Next Day | Review/navigate | Summary statistics/formulas |
| Upgrades | Connected tree, active-skills panel, Hide | Inspect/purchase | Purchase UI/states |
| Achievements | Five-row list, active-skills panel | Browse | Achievement data |
| Journal | Collection grid/list, shared top navigation | Browse | Content/filtering |
| Settings | Language, display/window, volume, reset data | Change/reset | Details/confirmation |
| Pause | Pause screen | TBD | All behavior |

## 22. Visual Direction

- [DESIGNED] Warm cream/light-yellow primary UI background.
- [DESIGNED] Orange-yellow action buttons and currency treatment.
- [DESIGNED] Muted gray/brown panels and cards.
- [DESIGNED] Plates and cloches are central visual motifs.
- [DESIGNED] Location selection has service-counter/food-establishment silhouettes.
- [INFERRED] Cooking viewpoint is framed from a counter/service area toward the dish.
- [INFERRED] Sparkles and cloche opening create a celebratory result reveal.
- [TBD] Typography, icon system, accessibility contrast rules, exact palette tokens, responsive layout, final 3D art direction.

## 23. 3D / Environment / Characters / Animations

- [DESIGNED] FigJam includes areas for Locations, Dishes, Characters, and `3D модели и анимации`.
- [DESIGNED] Food/reference imagery and external visual references exist.
- [IMPLEMENTED] Unity contains one imported `Dormitory.fbx` interior model, materials, 17 texture maps, embedded lights/cameras, and one added capsule collider.
- [IMPLEMENTED] The scene is an early visual prototype, not a confirmed final restaurant.
- [TBD] Final environment list, individual dish models, characters, rigs, animation list, ownership/licensing, production status, and asset pipeline.

## 24. Audio

- [IMPLEMENTED] Unity’s audio module exists, but no audio clips, mixers, or `AudioSource` components were found.
- [TBD] Music, ambience, click sounds, impact sounds, cooking sounds, rarity reveal sounds, UI sounds, mix categories, and volume defaults.

## 25. Input / Controls

- [DESIGNED] Primary cooking action is clicking the selected dish.
- [IMPLEMENTED] Unity uses the Input System package and has a broad default-style input-actions asset.
- [IMPLEMENTED] `CameraCursorFollow` reads `Mouse.current` directly.
- [TBD] Mouse/touch/gamepad support, keyboard shortcuts, hold-click behavior, rebinding, accessibility controls, and mobile support.

## 26. Game States

| State | Relevant data | Known behavior | TBD |
|---|---|---|---|
| Main Menu | Save availability/version | Menu choices | Continue/New Game rules |
| Location Selection | Locations, unlocks, income, recipes | Choose restaurant | Locks/progress |
| Work Day | Location, earnings, orders | Day active | Timer/rules |
| Order Selection | Current orders, currency | Choose dish | Refill/timeout |
| Cooking | Dish, hidden progress, upgrades | Click to progress | Damage math |
| Dish Result | Dish, rarity, value | Reveal result | Reward application |
| End of Day | Prepared dishes, earnings, XP, date | Review / next actions | Summary formula |
| Upgrades | Upgrade ownership/currency | Tree browsing/purchase | Costs/effects |
| Achievements | Achievement progress | List display | Data/rules |
| Journal | Collection entries | Browse | Unlock/content |
| Settings | Preferences | Change preferences | Persistence |
| Pause | Active state | Pause | Behavior/menu |

## 27. Conceptual Data Model

| Entity | Confirmed fields | Fields required by described mechanics | TBD |
|---|---|---|---|
| Player | Currency, player XP, player level | Owned recipes/upgrades, progression | Save identity, stats |
| Location | Level, name, passive income/sec, served dishes | Unlock state, availability | Costs/progress definition |
| Dish | ID, name, sale value, recipe cost, click requirement, known locations | Recipe ownership, dish XP | Art, rarity modifiers |
| Recipe | Cost, linked dish | Purchased/unlocked state | Purchase rules/content |
| Order | Selected dish/location | Active/completed state | Spawn time/weights/rewards |
| Work Day | Location, prepared dishes, daily earnings, date | Active orders, XP gain | Duration/failure/end conditions |
| Dish Progression | Dish XP | Rarity influence | Thresholds/rewards |
| Rarity | Common–Legendary intended tiers | Result tier | Probability/reward modifiers |
| Upgrade | Tree node/name/dependencies | Ownership/level/effect | Costs/caps/prerequisites |
| Achievement | UI row/reward slot | Progress/unlocked state | Names/rules/rewards |
| Journal Entry | Collection presence | Linked content/unlock state | Types/content |

## 28. Unity Implementation Mapping

This is a design-to-technical mapping, not existing implementation.

| System | Current Unity state | Likely Unity representation | Notes |
|---|---|---|---|
| Game flow | Partially implemented: Location Selection → Work Day placeholder | Scene/state controller plus UI state machine | [TBD] Architecture choice beyond the prototype |
| Locations | Partially implemented: 11 static data assets, ordered catalog, placeholder carousel | Data assets/serializable location definitions | Unlocks, ownership, progress, and income runtime remain TBD/unimplemented |
| Dishes/recipes | Partially implemented: 32 static dish data assets; no runtime recipe system | Data assets/serializable dish and recipe definitions | Recipe ownership/purchase behavior remains TBD/unimplemented |
| Orders | Not implemented | Runtime order service/controller | Weights/rules TBD |
| Cooking | Not implemented | Dish presentation controller + click input handler | Formula TBD |
| Rarity | Not implemented | Result resolver/data table | Probability model TBD |
| Economy | Not implemented | Player economy service/data | Payout rules TBD |
| XP | Not implemented | Player/location/dish progression data | Dish/restaurant details TBD |
| Upgrades | Not implemented | Upgrade graph/data definitions + UI | Costs/effects TBD |
| Save/load | Not implemented | Persistence service | Format/slots TBD |
| UI | Partially implemented: temporary uGUI Location Selection and Work Day screens | Canvas/UI framework and screen controllers | Final UI and all other screens remain unimplemented |
| Environment | Dormitory scene/model | Scene/environment presentation layer | Reuse undecided |
| Camera | One camera with cursor-follow script | Gameplay presentation camera | Existing script may be reusable |
| Input | Input System asset, direct mouse read | Central input bindings | Current asset unwired |
| Visual effects | URP/Global Volume | Presentation/post-processing profile | Must be validated against Figma |

## 29. Current Unity Project State

- [IMPLEMENTED] Unity `6000.3.14f1` / Unity 6.3 LTS.
- [IMPLEMENTED] URP `17.3.0`.
- [IMPLEMENTED] Product: ClickCook.
- [IMPLEMENTED] One enabled scene: `Assets/Scenes/SampleScene.unity`.
- [IMPLEMENTED] Scene roots: Directional Light, Dormitory, Global Volume, Camera, Directional Light (1), and `ClickCook_Prototype`.
- [IMPLEMENTED] Authored runtime scripts: `Camera_Move.cs`, `DishData.cs`, `LocationData.cs`, `LocationCatalog.cs`, and `ClickCookPrototypeController.cs`.
- [IMPLEMENTED] Camera position approximately `(1.138, 1.184, -2.718)`, 60° perspective FOV, slight mouse-driven rotation.
- [IMPLEMENTED] Dormitory FBX with two materials and 17 supporting texture maps.
- [IMPLEMENTED] Global post-processing includes bloom, film grain, vignette, chromatic aberration, and color grading.
- [IMPLEMENTED] Static data contains 32 `DishData` assets, 11 `LocationData` assets, and one ordered `LocationCatalog` asset.
- [IMPLEMENTED] `SampleScene` contains a temporary uGUI Canvas/EventSystem and verified Location Selection → Work Day placeholder flow.
- [IMPLEMENTED] No gameplay prefabs, audio, animation, save/load, orders, economy, upgrades, progression, or complete gameplay architecture.
- [TBD] Whether the Dormitory environment is retained, adapted, or replaced.

## 30. Contradictions and Design Risks

1. **Rarity naming**
   - [DESIGNED] User context gives Common, Uncommon, Rare, Fantastic, Legendary.
   - [F] Result wireframe says `Обычное`.
   - [TBD] Confirm canonical localization and whether `Обычное` equals Common.

2. **Visible versus hidden cooking values**
   - [DESIGNED] Hidden HP/click progress is intended.
   - [F] Cooking screen visibly shows `67` and `23`.
   - [TBD] Meaning and player visibility of those values.

3. **Location recipe versus dish-served data**
   - [F] These are separate lists.
   - [TBD] Whether location criteria unlock recipes, define availability, or are presentation-only.

4. **Michelin restaurant availability**
   - [F] It lists “all dishes starting from the third restaurant.”
   - [TBD] Exact included catalog.

5. **Dormitory prototype versus restaurant design**
   - [IMPLEMENTED] Current scene/model is Dormitory-based.
   - [DESIGNED] Final locations are restaurant/food-service venues.
   - [TBD] Prototype reuse decision.

6. **Click requirement semantics**
   - [F] Dish data exposes “number of clicks.”
   - [DESIGNED] User context describes hidden dish HP.
   - [TBD] Whether values are identical base HP, baseline clicks, or another progression metric.

## 31. Complete TBD / Open Decisions List

### Critical

- Work-day duration, completion, failure, timer, order count, order timeout, replacement, and early-end rules.
- Hidden HP / click requirement relationship and all click-damage calculations.
- Critical, click-hold, and auto-click formulas.
- Rarity probabilities, dish-XP relationship, rewards, and canonical terminology.
- Recipe purchase/unlock rules and location unlock rules.
- Upgrade costs, effects, levels, prerequisites, and caps.
- Save/load, New Game, Continue, reset-data behavior.
- Exact economic payout timing and modifier order.
- Final environmental/3D content scope and Dormitory prototype decision.

### Important

- Character, restaurant, and dish XP gain/thresholds/rewards.
- Player-level interpretation and benefits.
- Achievement definitions/rewards.
- Journal content and unlock rules.
- UI navigation, feedback, transitions, and responsive behavior.
- Input/device support and accessibility.
- Audio direction and controls.

### Minor

- Exact palette tokens, typography, iconography, animations, particle behavior, and effects.
- Discord destination/version format.
- Sorting/filtering in achievements/journal.
- Detailed pause-menu behavior.

## 32. Recommended Development Order

1. Confirm critical TBD decisions and retain this GDD as the data source.
2. Define project architecture, state flow, and persistence boundaries.
3. Establish the conceptual data model for player, locations, dishes, recipes, upgrades, and progression.
4. Decide the scene/environment foundation and evaluate the Dormitory prototype against final location needs.
5. Build UI navigation foundations: Main Menu, Location Selection, Settings, Pause.
6. Encode location, dish, recipe, and player-XP data exactly as documented.
7. Implement work-day and order lifecycle once its unresolved rules are decided.
8. Implement cooking/clicking, then dish completion and result presentation.
9. Define and implement dish XP and rarity together.
10. Implement economy, passive income, recipes, and location progression.
11. Implement the connected upgrade tree.
12. Implement end-of-day summary, save/load, achievements, and journal.
13. Add final 3D content, animation, audio, accessibility, and visual polish.

## 33. Implementation Progress

This checklist records verified Unity implementation without changing the design requirements above. Milestone labels identify the checkpoint that delivered each implemented or partial step.

### Verified foundations

- [x] Static data architecture for locations and dishes, preserving documented names, numeric values, and explicit availability relationships. — **Milestone 1**
- [x] All 11 documented `LocationData` assets exist. — **Milestone 1**
- [x] All 32 documented `DishData` assets exist. — **Milestone 1**
- [x] A read-only `LocationCatalog` references exactly the 11 locations in documented level order 0–10 and supports lookup by documented level. — **Milestone 2A**
- [x] The Dormitory environment, lighting, camera, URP volume, Input System presence, and `CameraCursorFollow` baseline remain available. — **Existing project baseline, retained through Milestone 2B**

### Game flow and UI

- [ ] Partially implemented — The core gameplay loop currently covers only Location Selection → Start Work Day placeholder. Order selection, cooking, dish completion, rarity, rewards, end-of-day summary, upgrades, and next-day flow remain unimplemented. — **Milestone 2B**
- [ ] Main Menu is not implemented.
- [ ] Partially implemented — Location Selection has a verified temporary uGUI carousel with one current location, Previous/Next wraparound across all 11 locations, name, documented level, placeholder progress, documented dish count, passive income/sec, and `Cook`. Unlock/ownership rules, real progress, dish-slot contents, and final visual design remain unimplemented. — **Milestone 2B**
- [ ] Partially implemented — Pressing `Cook` stores the selected `LocationData` in memory and opens a Work Day placeholder showing the selected location. The order board and real work-day lifecycle remain unimplemented. — **Milestone 2B**
- [ ] Partially implemented — A `Finish Day` button can be triggered and displays a prototype-only response. End-of-day rules, summary, rewards, date advancement, and next-day behavior remain unimplemented. — **Milestone 2B**
- [ ] Order Board is not implemented.
- [ ] Cooking / clicking is not implemented.
- [ ] Dish Result and rarity reveal are not implemented.
- [ ] End-of-Day Summary is not implemented.
- [ ] Final UI/UX styling, transitions, responsiveness, and animations are not implemented.

### Runtime systems

- [ ] Partially implemented — Location and dish design data are available at runtime, but location unlocking/ownership, passive-income accrual, recipe ownership/gating, and order eligibility rules are not implemented. — **Milestones 1 and 2A**
- [ ] Economy is not implemented.
- [ ] Player, restaurant, and dish XP/progression are not implemented.
- [ ] Recipe and location unlock systems are not implemented.
- [ ] Upgrade system is not implemented.
- [ ] Save/load, New Game, Continue, and Reset Data behavior are not implemented.
- [ ] Achievements are not implemented.
- [ ] Journal is not implemented.
- [ ] Settings are not implemented.
- [ ] Pause behavior is not implemented.
- [ ] Audio content and runtime audio behavior are not implemented.

Going forward, every completed and Unity-verified milestone must update this checklist and `Assets/Documentation/IMPLEMENTATION_STATUS.md`.
