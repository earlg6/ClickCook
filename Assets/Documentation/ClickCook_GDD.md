# ClickCook — Game Design Document

## 1. Game Overview

**Game:** ClickCook  
**Genre:** Restaurant/cooking clicker with collection, progression, and upgrade systems.

- [DESIGNED] The player selects a location, begins a work day, selects orders, prepares dishes through close-up clicking, receives a rarity result, earns currency/XP, and progresses through upgrades and locations.
- [IMPLEMENTED] Unity contains static location/dish data and verified Location Selection → Work Day orders → base click cooking → rarity/Dish XP/payout result → replacement/day-ending flow (5B). The tenth result ends the day on Continue. Wallet and dish mastery are run-local; final visuals, full economy, Player XP and persistent progression remain unimplemented.
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
- [TBD] Final work-day transition, timing and failure. Approved prototype lifecycle and baseline rarity calculations are specified in Sections 6–7 and 10–11.

## 3. Complete Game Flow / State Machine

| State | Entry | Player actions | Exit | Known rules |
|---|---|---|---|---|
| Main Menu | Launch / return from play | Continue, New Game, Settings, Exit, Discord | Choose action | [DESIGNED] Menu items exist |
| Location Selection | New/continued session | Browse locations, inspect dishes/progress, Cook | Press Cook | [DESIGNED] Carousel/card UI exists |
| Work Day / Order Board | Start work day | Select an order, Finish Day | Select dish / finish day | [DESIGNED] Currency, remaining dishes, location-dependent capacity up to six visible orders |
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
- [APPROVED — Milestone 4 design] Baseline daily completion limit: `MAX_COMPLETED_DISHES_PER_DAY = 10`. This is not board capacity. Future increases by location/progression are TBD.
- [APPROVED — Milestone 4 design] The player may Finish Day early, even below ten completed dishes. Ending the day discards all unfinished orders and clears temporary order state; no unfinished orders persist into a new day.
- [APPROVED PACING TARGET] Approximately 60–90 seconds per Work Day, not a hard timer or automatic timeout.
- [HISTORICAL 4A SCOPE] Completion, daily counting/limit enforcement, cleanup, and end-state transitions were deferred in 4A, which retained a Finish Day placeholder message. Milestone 4B implements the approved base cooking/day lifecycle below; Finish Day still is not exposed inside cooking.
- [APPROVED — Milestone 4B] At ten completed dishes, automatically end the day. Both automatic completion and manual Finish Day clear unfinished orders and temporary session state, return to Location Selection, and display a day-finished message. This temporary return replaces an End-of-Day screen for this milestone only; no financial penalty, reward, XP, or date advancement is added. Finish Day is available on the board, not inside cooking.
- [IMPLEMENTED — Milestone 5B, supersedes the immediate 4B automatic transition/no-reward scope] The tenth completed dish is paid and shows its rarity/Dish XP result; Continue ends the day. Retain earned run-local wallet/mastery across day cleanup. Final End-of-Day summary, Player XP and date advancement remain unimplemented.
- [TBD] Day duration/timer policy, failure states, penalties, other daily reset behavior, date advancement, and earnings formula. No penalties, rewards, or XP are inferred for the prototype.

## 7. Order Board

- [DESIGNED] The board is on the right side of gameplay screens.
- [DESIGNED] It displays `Количество блюд осталось - ..` / dishes remaining.
- [DESIGNED] Current currency is shown above it; example: `215 у.е.`
- [HISTORICAL INFERENCE] Six visible slots were interpreted as a 3×2 grid. The approved rule is a maximum of six simultaneously visible orders, not six orders at every location or a finite six-order day; final grid layout remains a presentation choice.
- [HISTORICAL INFERENCE — superseded] A completed/selected slot might become blank. Selection now reserves the order without removing it; successful completion removes that exact order and permits replacement as specified below.
- [DESIGNED] Orders are selected by the player before cooking.
- [APPROVED — Milestone 4 design] Capacity depends on location: early locations approximately 2–3 simultaneous orders, later locations up to six. Exact per-location capacities are not approved.
- [APPROVED PROTOTYPE RULE] Generate orders uniformly at random from the selected `LocationData.DocumentedAvailableDishes`. Duplicates are allowed as independent runtime instances. No recipe ownership, progression, economy, rarity, or weighted filtering. Empty pools generate no orders; missing references must be handled safely without invented dishes.
- [APPROVED — selection] Clicking a populated card immediately enters cooking, without confirmation. Only one order may be active. Preserve its runtime identity, dish, slot, and location; selection is not completion and other orders cannot be selected while it is reserved.
- [APPROVED PROTOTYPE CHOICE — 4A] Use a screen labeled `Cooking Prototype — Not Implemented`, showing the selected order identity, dish ID/name, and location. Back releases the active reservation and returns to the unchanged board without completion, regeneration, or counter changes. Final close-up art and clicking remain deferred.
- [APPROVED — Milestone 4B, supersedes the 4A cooking placeholder] A large clickable dish placeholder implements basic cooking, while final art remains unimplemented. Back resets cooking progress to its starting state but preserves the exact order and all other board entries. Successful cooking returns directly to the board with a replacement order, unless it reaches the daily limit. No intermediate result screen, rarity, or rewards in this milestone.
- [APPROVED — Milestone 5A, supersedes direct board return for dishes 1–9] After a dish completes below the daily limit, show a temporary Dish Result screen with its name, documented base `SaleValue` explicitly labeled reference-only, `Rarity: not implemented`, and Continue. Continue returns to the existing board/replacement without another completion, increment, or regeneration. No currency/XP is awarded. The tenth completed dish bypasses this screen and immediately ends the day as approved in Section 6. Other board/cooking actions are unavailable during the result screen.
- [IMPLEMENTED — Milestone 5B, supersedes the 5A result/no-payout and 4B immediate end at ten] Every completed dish shows its name, rolled rarity, payout, wallet, Dish XP award, old/new dish level and next-level progress (or mastery cap). Pay and award XP once at successful completion, before Continue. Continue returns to the same replacement board for completions 1–9; after the tenth result it ends/clears the day. Hidden/stale actions cannot repeat rewards. Retain earned wallet/XP on early Finish Day; discard unfinished orders without rewards.
- [APPROVED LIFECYCLE — 4B] Successful completion marks the exact instance completed, removes it, increments the daily completed count, and generates a replacement for the freed slot using the same pool/duplicate rules. No replacements after the daily limit is reached. Selection alone never triggers replacement.
- [PROVISIONAL BALANCING PROPOSAL — 4A] Configured capacities for documented levels 0–10: `2, 2, 3, 3, 4, 4, 5, 5, 6, 6, 6`. These are editable prototype values, not approved thresholds. Dormitory uses its two existing documented dishes (Sections 5 and 8); no empty-location override is applied.
- [PROTOTYPE UI — 4A/4B/5B] Display available order count, configured capacity, and completed dishes separately. The 4A `0 / 10` placeholder updates with real completions; 5B replaces the unimplemented currency label with the run-local wallet. Keep the read-only documented candidate list distinct from active orders.
- [TBD] Final capacity mapping, final recipe eligibility/availability reconciliation, order timeout policy, future weights/rarity influence on order generation, and final selection feedback. No hard timer or expiry is implemented.

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
- [APPROVED — Milestone 4B] Base hidden HP equals the documented `ClickRequirement`; each discrete click subtracts one HP, and zero completes the selected dish. All documented values are preserved, including 70,000 for the final dish.
- [TBD] Recipe-purchase behavior, dish unlock representation, recipe UI, dish art, and future modifiers. Base Dish XP awards are approved in Section 10.

## 9. Cooking / Clicking Mechanics

- [DESIGNED] Select an order, enter close-up cooking presentation, click the dish, reduce hidden dish HP/progress, complete dish, reveal rarity.
- [DESIGNED] The restaurant environment remains visible around the cooking presentation.
- [DESIGNED] Relevant upgrade branches: click strength, critical hit, click hold, auto-click strength, auto-click frequency.
- [DESIGNED] Screen 2 shows a central vertical feedback area with example values `67` and `23`, plus flame/impact graphics.
- [APPROVED — Milestone 4B] Without upgrades, hidden HP = `DishData.ClickRequirement`, click damage = 1, completion occurs at zero. Do not display HP or a numerical progress counter. Back resets partial progress; reselecting the same order starts at full HP. The prototype uses a large clickable placeholder showing the dish name rather than final models or effects.
- [TBD] Meaning of `67` and `23`; future click modifiers/feedback, critical formula, hold behavior, auto-click activation, and accessibility considerations. 5B rarity and payouts use separately approved Sections 10–11 and 14, not inferred click formulas.

## 10. Dish XP

- [DESIGNED] Upgrade tree includes XP branches: Character, Restaurant, Dish.
- [DESIGNED] Higher rarity should depend on player progression/experience with that specific dish.
- [APPROVED DESIGN — Milestone 5B] Rarity is randomly selected using Section 11.1 and the specific dish's level before this completion's XP award. Each dish starts at level 1 / 0 XP, independently of player/location levels. Maximum dish level = 18 for this version; historical rows 19–21 are retained for possible future expansion, not extra implemented levels.
- [APPROVED DESIGN — Milestone 5B] Transition cost from level L is `round(20 × 1.18^(L − 1))` XP, using nearest-integer rounding (half away from zero). This is a per-transition cost, not a cumulative threshold. Carry excess XP across levels, allowing multiple level-ups. Cap total XP at the sum of transition costs 1–17 (1,744 XP); at level 18 no more Dish XP is awarded, but payouts continue.
- [APPROVED DESIGN — Milestone 5B] Dish XP awards by Common / Uncommon / Rare / Fantastic / Legendary are `10 / 12 / 16 / 22 / 30`, truncated if necessary to the mastery cap. Unfinished dishes/Back do not award XP. Track dish XP and the wallet across Work Days only within the current game run; stopping/restarting the game or Play Mode resets them. No save/load or Player XP in this milestone.
- [TBD] Persistent progression, future level-cap expansion, XP upgrade modifiers and final UI. Approved numeric values are an initial playtest balance, not a verified guarantee of player engagement or human pacing.

## 11. Rarity System

Canonical intended tiers from player-provided gameplay context:

1. Common
2. Uncommon
3. Rare
4. Fantastic
5. Legendary

- [DESIGNED] Completed-dish wireframe example: Sandwich, `Обычное`, value `5 у.е.`
- [APPROVED PROTOTYPE SCOPE — Milestone 5A] Implement only the informational completed-dish name/base-value screen for completions 1–9. Do not assign even a fixed Common tier: rarity is explicitly unimplemented, base value is not a payout, and no money or XP is granted. Final rarity reveal remains TBD.
- [INFERRED] Sparkles and opened cloche indicate a celebratory reveal.
- [TBD] Whether `Обычное` is the canonical equivalent of Common; future probability/reward modifiers; colors; animations; audiovisual feedback and further mechanical effects. Baseline XP thresholds and payout multipliers are approved in Sections 10 and 14; the baseline probability table is below.

### 11.1 Approved baseline rarity probabilities — implemented for levels 1–18 in 5B

- [APPROVED DESIGN — after Milestone 5A] The designer supplied a historical table and then explicitly approved these changes: use Fantastic instead of Epic; level 3 Common = 57%; level 11 Legendary = 24%; explicitly copy level 11 to 12, level 13 to 14 and level 15 to 16. All other supplied percentages remain unchanged. Every row totals 100%.
- Values are percentages, indexed by Dish Level, not player level or location level. Milestone 5B verifies every integer percent roll for levels 1–18 against this table; no interpolation or added modifiers.
- Use levels 1–18 in the approved 5B runtime. Retain the historical 18–21 row unchanged; levels 19–21 are reserved for possible expansion. Do not interpolate or extrapolate beyond the documented table.

| Dish Level | Common | Uncommon | Rare | Fantastic | Legendary |
|------------|--------|----------|------|-----------|-----------|
| 1 | 85 | 13 | 2 | 0 | 0 |
| 2 | 71 | 27 | 2 | 0 | 0 |
| 3 | 57 | 36 | 7 | 0 | 0 |
| 4 | 42 | 42 | 16 | 0 | 0 |
| 5 | 28 | 44 | 25 | 3 | 0 |
| 6 | 14 | 42 | 35 | 9 | 0 |
| 7 | 0 | 38 | 44 | 17 | 1 |
| 8 | 0 | 26 | 46 | 24 | 4 |
| 9 | 0 | 14 | 45 | 32 | 9 |
| 10 | 0 | 2 | 42 | 39 | 17 |
| 11 | 0 | 2 | 33 | 41 | 24 |
| 12 | 0 | 2 | 33 | 41 | 24 |
| 13 | 0 | 2 | 14 | 40 | 44 |
| 14 | 0 | 2 | 14 | 40 | 44 |
| 15 | 0 | 2 | 5 | 30 | 63 |
| 16 | 0 | 2 | 5 | 30 | 63 |
| 17 | 0 | 2 | 5 | 16 | 77 |
| 18–21 | 0 | 2 | 5 | 9 | 84 |

- [IMPLEMENTED — Milestone 5B, supersedes the 5A tenth-dish bypass] Show the result for the tenth completed dish too; Continue then ends the day. No replacement after the tenth completion, and no additional payout on Continue.

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
- [APPROVED DESIGN — Milestone 5B] Initial wallet balance = 0 у.е.; pay `SaleValue × rarity multiplier` immediately on successful completion, exactly once. Multipliers by Common / Uncommon / Rare / Fantastic / Legendary are `×1 / ×1.25 / ×1.75 / ×2.5 / ×4`. With the documented integer SaleValue these produce exact amounts to at most two decimal places; display two decimal places without floating-point money arithmetic. Continue/Back/Finish Day do not repeat payments, and early Finish Day retains already-earned money/XP without paying unfinished orders. No Player XP, passive income or save/load in this milestone.
- [TBD] Passive-income accrual/collection, future bonus calculations and sale-value modifiers, rounding for future finer-precision modifiers, price-reduction order, upgrade spending, and loss/penalty systems.

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
| Order Board | Currency, dishes remaining, up to six location-dependent cards, plate/cloche, Finish Day | Select order/end day | Final capacity mapping and feedback |
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
| Game flow | Partially implemented: Location Selection → board → base cooking → rarity/payout/XP result → board or day end | Existing prototype controller/session/rewards | Final summary, visuals and persistent progression remain unimplemented |
| Locations | Partially implemented: 11 static data assets, ordered catalog, placeholder carousel | Data assets/serializable location definitions | Unlocks, ownership, progress, and income runtime remain TBD/unimplemented |
| Dishes/recipes | Partially implemented: 32 static dish data assets; no runtime recipe system | Data assets/serializable dish and recipe definitions | Recipe ownership/purchase behavior remains TBD/unimplemented |
| Orders | Partially implemented: random instances, reservation, completion/replacement, daily cap and cleanup (4B) | PrototypeOrder / PrototypeOrderSession | Final capacities and advanced eligibility remain TBD |
| Cooking | Partially implemented: hidden base HP and one-damage clicks on a large temporary uGUI dish | Existing prototype controller/session | Final art, effects and upgraded click formulas remain absent |
| Rarity | Baseline levels 1–18 implemented (5B) | PrototypeDishRewards / approved table | Future modifiers and final reveal presentation |
| Economy | Partially implemented: exact run-local wallet/payouts (5B) | PrototypeDishRewards | Spending, passive income and persistence |
| XP | Partially implemented: run-local Dish XP/mastery (5B) | PrototypeDishRewards | Player/restaurant XP, upgrades and persistence |
| Upgrades | Not implemented | Upgrade graph/data definitions + UI | Costs/effects TBD |
| Save/load | Not implemented | Persistence service | Format/slots TBD |
| UI | Partially implemented: temporary uGUI Location Selection, Work Day, base cooking and rarity/XP/payout Dish Result screens | Existing Canvas/prototype controller | Final UI and remaining screens are unimplemented |
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
- [IMPLEMENTED] Authored runtime scripts: `Camera_Move.cs`, `DishData.cs`, `LocationData.cs`, `LocationCatalog.cs`, `ClickCookPrototypeController.cs`, `PrototypeOrder.cs`, and `PrototypeOrderSession.cs`.
- [IMPLEMENTED] Camera position approximately `(1.138, 1.184, -2.718)`, 60° perspective FOV, slight mouse-driven rotation.
- [IMPLEMENTED] Dormitory FBX with two materials and 17 supporting texture maps.
- [IMPLEMENTED] Global post-processing includes bloom, film grain, vignette, chromatic aberration, and color grading.
- [IMPLEMENTED] Static data contains 32 `DishData` assets, 11 `LocationData` assets, and one ordered `LocationCatalog` asset.
- [IMPLEMENTED] `SampleScene` contains a temporary uGUI Canvas/EventSystem and verified Location Selection → board → base click cooking → rarity/Dish XP/payout result / day end flow (5B), with Back resetting partial progress and Continue preserving replacement orders without duplicate rewards.
- [IMPLEMENTED] No gameplay prefabs, audio, animation, save/load, final result presentation, complete economy, upgrades, Player XP/progression, or complete gameplay architecture.
- [TBD] Whether the Dormitory environment is retained, adapted, or replaced.

## 30. Contradictions and Design Risks

- [OBSERVED — Milestone 5B verification, unresolved existing data/design discrepancy] The current Dormitory LocationData candidate pool is empty (zero documented dishes at runtime), whereas the earlier Section 7 capacity note says it uses two dishes and Section 8 lists Dormitory availability. The prototype preserves the existing assets and supports the approved empty-board state; no dishes or eligibility rules were invented to reconcile this. Decide the final location availability separately.

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
   - [APPROVED — Milestone 4B] Values are base hidden HP with one point of damage per click in the unupgraded prototype. Future upgrade effects remain TBD.

## 31. Complete TBD / Open Decisions List

### Critical

- Work-day duration/timer, failure, order timeout, final capacity mapping, and future daily-limit progression. Baseline limit, early finish/discard, generation, selection, completion replacement, and automatic end transition are approved in Sections 6–7 but design approval alone does not establish implementation.
- Future upgraded click-damage calculations. Base hidden HP and damage are approved in Sections 8–9.
- Critical, click-hold, and auto-click formulas.
- Baseline rarity probabilities, dish-XP relationship and payout multipliers are approved/implemented in 5B; future modifiers, persistent progression and localized terminology remain open.
- Recipe purchase/unlock rules and location unlock rules.
- Upgrade costs, effects, levels, prerequisites, and caps.
- Save/load, New Game, Continue, reset-data behavior.
- Future economic modifiers/order and spending rules; base payout timing is approved/implemented in 5B.
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

- [ ] Partially implemented — The core loop covers Location Selection → board → base click cooking → rarity/Dish XP/payout result → replacement / day end. The tenth result ends on Continue. Final summary, upgrades, persistence and date-based next-day progression remain unimplemented. — **Milestones 2B–5B**
- [ ] Main Menu is not implemented.
- [ ] Partially implemented — Location Selection has a verified temporary uGUI carousel with one current location, Previous/Next wraparound across all 11 locations, name, documented level, placeholder progress, documented dish count, passive income/sec, and `Cook`. Unlock/ownership rules, real progress, dish-slot contents, and final visual design remain unimplemented. — **Milestone 2B**
- [ ] Partially implemented — Cook starts a fresh in-memory day/board; completion, replacement, ten-dish limit, run-local payouts/XP and cleanup work. Dates and final day summary remain unimplemented. — **Milestones 2B–5B**
- [x] Work Day displays the selected location's complete `DocumentedAvailableDishes` list as read-only documented candidates, with count, dish IDs, and English names in stored order. This is not an order board or an eligibility rule. — **Milestone 3**, supporting Sections 2, 5, and 8; files: `Assets/Scripts/Runtime/ClickCookPrototypeController.cs`, `Assets/Scenes/SampleScene.unity`
- [x] Manual Finish Day clears unfinished orders and temporary day state, returns to Location Selection, and displays a day-finished message; since 5B, ten completions trigger cleanup on result Continue. Earned run-local wallet/Dish XP survive day cleanup. No Finish Day action during cooking/results. — **Milestones 4B/5B**; files: `Assets/Scripts/Runtime/ClickCookPrototypeController.cs`, `Assets/Scripts/Runtime/PrototypeOrderSession.cs`, `Assets/Scripts/Runtime/PrototypeDishRewards.cs`, `Assets/Scenes/SampleScene.unity`
- [x] Initial Order Board population: location-configured capacity capped at six, uniform random documented candidates, independent duplicate order identities, empty-state handling and separate available/capacity/completed counters. 5B replaces the original unimplemented currency label with a real run-local wallet. — **Milestones 4A/5B**; files: `Assets/Scripts/Runtime/PrototypeOrder.cs`, `Assets/Scripts/Runtime/PrototypeOrderSession.cs`, `Assets/Scripts/Runtime/ClickCookPrototypeController.cs`, `Assets/Scenes/SampleScene.unity`
- [x] Single-order reservation and selected-order/dish/location handoff; Back preserves the board without completion/regeneration and resets partial cooking progress. — **Milestones 4A–4B**, same files as initial board population.
- [x] Base cooking: hidden HP equals `ClickRequirement`, one damage per discrete dish-button click, exact-instance completion at zero, replacement in the same slot before the ten-dish cap, and no replacement at the cap. — **Milestone 4B**; files: `Assets/Scripts/Runtime/PrototypeOrder.cs`, `Assets/Scripts/Runtime/PrototypeOrderSession.cs`, `Assets/Scripts/Runtime/ClickCookPrototypeController.cs`, `Assets/Scenes/SampleScene.unity`
- [ ] Partially implemented — Order Board/basic day lifecycle works; final balancing, eligibility, economy, expiry rules and presentation remain unfinished. — **Milestones 4A–4B**
- [ ] Partially implemented — Basic click cooking works on a large temporary dish placeholder; final close-up dish art, effects, critical/hold/auto-click upgrades and final input support remain unfinished. — **Milestone 4B**
- [x] Dish Result foundation (5A) is extended in 5B: exact dish identity/name, resolved rarity, earned payout/wallet, Dish XP gain and old/new level/progress; Continue preserves the replacement board without repeat rewards. All ten dishes show results; tenth Continue ends the day. Stale text clears on Continue/end/new day. — **Milestones 5A–5B**; files: `Assets/Scripts/Runtime/ClickCookPrototypeController.cs`, `Assets/Scripts/Runtime/PrototypeDishRewards.cs`, `Assets/Scripts/Runtime/PrototypeOrderSession.cs`, `Assets/Scenes/SampleScene.unity`
- [ ] Partially implemented — Dish Result uses temporary uGUI text/buttons; final reveal art, audiovisual feedback and animations remain unimplemented. — **Milestones 5A–5B**
- [ ] End-of-Day Summary is not implemented.
- [ ] Final UI/UX styling, transitions, responsiveness, and animations are not implemented.

### Runtime systems

- [ ] Partially implemented — Location and dish design data are available at runtime, but location unlocking/ownership, passive-income accrual, recipe ownership/gating, and order eligibility rules are not implemented. — **Milestones 1 and 2A**
- [x] Baseline rarity resolved by the approved dish-level table (1–18), before awarding Dish XP; independently tracked dish mastery, carry-over XP and level-18 cap. — **Milestone 5B**; file: `Assets/Scripts/Runtime/PrototypeDishRewards.cs`
- [x] Exact decimal payouts once per successful completion into a zero-starting run-local wallet; wallet/Dish XP survive day changes, not game restart. — **Milestone 5B**; files: `Assets/Scripts/Runtime/PrototypeDishRewards.cs`, `Assets/Scripts/Runtime/PrototypeOrderSession.cs`
- [ ] Partially implemented — Economy has baseline payouts/wallet only; spending, passive income and persistence remain unimplemented. — **Milestone 5B**
- [ ] Partially implemented — Dish XP/mastery works in memory; Player/restaurant XP, XP upgrade effects and persistent progression remain unimplemented. — **Milestone 5B**
- [ ] Recipe and location unlock systems are not implemented.
- [ ] Upgrade system is not implemented.
- [ ] Save/load, New Game, Continue, and Reset Data behavior are not implemented.
- [ ] Achievements are not implemented.
- [ ] Journal is not implemented.
- [ ] Settings are not implemented.
- [ ] Pause behavior is not implemented.
- [ ] Audio content and runtime audio behavior are not implemented.

Going forward, every completed and Unity-verified milestone must update this checklist and `Assets/Documentation/IMPLEMENTATION_STATUS.md`.
