# ClickCook — roadmap до production

**Поточний checkpoint — 2026-10-28: 8B завершено — рецепти й eligibility.** Додано магазин32 рецептів (8 на сторінку), постійне володіння, точну одноразову купівлю за наявними цінами та фільтрацію початкових/замінних замовлень за власними рецептами. Новий профіль має сендвічі; без власних рецептів у локації Cook заблоковано з підказкою та доступною навігацією до магазину. Авторитетний serving map — чинні списки LocationData; за рішенням власника Michelin має лише4 страви29–32. Схема17 читає1–16: зберігає точний прогрес, надає старим профілям рецепти вже приготованих страв за позитивним Dish XP, не переписує файл під час читання. Нова гра скидає рецепти в обох копіях. Контракт: [8B](ClickCook_8B_Recipe_Contract.md).

Compilation, scoped model/storage/Play та native mouse пройдено. Купівля кексів150: wallet1000.25→850.25, рецепт2 і точний баланс збережено й відновлено після stop/restart/Continue. Перевірено owned/all-owned pools11 локацій, replay/pause/day/price guards, immutable snapshot, backup/reset/malformed/future protection,32 UI labels/details, paging та навігацію без disabled Cook. Новий Player build/широкі suites не потрібні. Play stopped; save override removed; scene clean. Попередні checkpoints нижче історичні.

**Наступний крок — 8C: Player XP/level і відкриття локацій.** Перед runtime реалізацією зафіксувати cumulative/per-level пороги, округлення/момент видачі earnings/100 та відповідність player levels1–10 і location levels0–10. Фінальний pacing, Journal, recipe discount, Restaurant XP та U6 лишаються окремими етапами.


**Поточний checkpoint — 2026-10-28: 8A, підсумок дня, завершено.** Звіт показує локацію, до десяти фактичних receipts (страва/якість/гроші/Dish XP), точні суми доходу та XP. Ранній Finish Day і Continue після десятої страви відкривають звіт. Сам звіт не платить, не додає XP і не змінює save: нагороди вже збережені на receipt boundary. Порожній день дає нулі, незавершена робота не враховується. Апгрейди повертають до того самого звіту; Next Day одноразово повертає до вибору локації, Cook починає нову дошку. Календар, Player/Restaurant XP та збереження історії звітів не додано. Контракт: [8A](ClickCook_8A_Day_Summary_Contract.md).

Локальна compilation/model/Play перевірка пройшла: empty/early/ten-receipt totals, fractional XP, immutable snapshot, paused/active/stale guards, owner cleanup, summary→upgrades→summary→Next Day, focus/text fit та точний reload тимчасового save. Native Computer Use огляд і кнопки пройдено у Game View Full HD (відображення 0.45×): звіт 90/110 XP, покупка strength за20, wallet70/rank1 у save, звіт залишився90/110. Зміни моделей/налаштувань інших робіт збережено окремо. Player build і широка регресія не потрібні. Нижчі checkpoints історичні.

**Наступний roadmap-крок: 8B — купівля рецептів та eligibility замовлень.** Перед runtime реалізацією закрити контракт стартових owned recipes і авторитетного availability map, включно з Michelin. U5 залишається частковим; U6 потребує числового quality-контракту.


**Поточний checkpoint — 2026-10-28: Upgrade batch B завершено.** Після batch A реалізовано ще 27 пізніх manual/Hold/auto вузлів: сила та множники кліку, crit, double/triple, Hold frequency, auto chance/count/strength/multiplier/frequency. Працюють 50 платних вузлів із 91 (54.9%; root окремо), повністю реалізовано ці manual/auto гілки. Це частка вузлів, не відсоток готовності всієї гри. Контракт: [Batch B](ClickCook_Upgrade_Batch_B_Contract.md). Schema 16 з backward read 1–15, власними caps/prerequisites/цінами та discount-aware quotes. Максимуми: manual 101078 HP, crit 50% ×7.75, mutually exclusive double35%/triple15%, Hold14/с, auto31×3384 HP ×10/с. Початковий баланс; pacing не доведено.

Локальні compilation/model/storage перевірки, незалежні maxima/RNG/proc/deadline/no-backlog/no-carry/receipt checks та суміжні старі manual/auto/Hold моделі пройдено. Native tier покупки й aggregate UI fit/autosave перевірено попередньою сесією; збережений Full-HD screenshot оглянуто (Temp/BatchB/tree.png). Попередня live controller completion дала payout11/XP276.75 один раз. Після втрати тимчасового Temp новим запуском повторено лише потрібний restart boundary: ізольовані покупки strengthVI/autoMultiplierII, wallet9524050000.25, damage80918/1862, дробова XP.37, restart/Continue відновлено точно. Новий Player build/широкі suites не потрібні. Play stopped, save override removed. Нижчі checkpoints історичні.

**Наступний roadmap-крок: 8A — підсумок дня**, сума фактичних receipts без повторної виплати, Next Day та Upgrades. U5 лишається частковим: recipe/passive/Player/Restaurant XP залежать від власних базових milestones; U6 quality потребує окремого числового контракту.


**Upgrade batch A виконано — 2026-10-27.** За дорученням власника продовжено апгрейди пакетами: знижка на апгрейди (до 5%), Dish XP flat II/III (+5/+14 за ранг), Dish Value II–IV (+4/+5/+6% за ранг). [Контракт](ClickCook_Upgrade_Batch_A_Contract.md). Разом 6 нових вузлів, власні caps/prices/prerequisites, агрегати flat XP до 125 та value factor до 2.20. Ціни всіх working nodes і UI враховують discount; quotes прив'язані до discount rank. Schema 15 читає 1–14. Compilation, targeted purchase/aggregate/storage та value-zero compatibility, native discount purchase/autosave, UI prices, restart/Continue і discounted strength purchase 213→211 пройдено. Full-HD discount screenshot: Temp/BatchA/discount.png. Пізні manual/auto tiers — наступний пакет, ще не завершені цим checkpoint. Build/широка регресія/pacing не виконувались.


**U5c завершено — 2026-10-27.** [Контракт оплати страв](ClickCook_U5c_Dish_Value_Contract.md): `node.dish_value.01` / «Вигідне меню» після Click Strength 1, 15 рангів по +3% (×1–×1.45), чинні `ceil(100 × 1.7^N)`. Receipt `Round(SaleValue × quality × factor, 2, AwayFromZero)`, одне остаточне округлення до копійок; одна виплата/order для manual/Hold/start/periodic. XP/fractions/rarity/combat незмінні. Працюють detail/current→next, восьмий рядок statistics, actual result/wallet і schema 14 з читанням 1–13. Compilation, локальні price/all-rarity/rank/rounding/source/storage та попередній XP model, дві native mouse покупки, result 6.63/wallet 36.88/XP 20.52/autosave/restart/Continue пройдено; Full-HD tree/result оглянуто. Build/native keyboard/широка регресія/pacing поза scope. **Далі U5d — перший node.upgrade_discount.01**, після власного discount/base-price/rounding/quote/save контракту. U5 ще частковий; пізні value tiers, passive/recipe/інші XP effects недоступні. Нижчі checkpoints — історія.


**U5b завершено — 2026-10-27.** [Контракт множника Dish XP](ClickCook_U5b_Dish_Xp_Multiplier_Contract.md): `node.dish_xp.01` після flat XP 1, 15 рангів, factor `1 + 0.07N` (×1–×2.05), чинні `ceil(100 × 1.7^N)`. Receipt `(base rarity XP + flat) × factor`, exact decimal із сотими для кожної страви; cap/old-level rarity/payout незмінні. Result/board/bar показують точну XP; schema 13 зберігає hundredths і читає 1–12. Compilation, all-rarity/ranks/carry/cap/source/storage/backup/reset/snapshot, попередній flat model, дві native mouse покупки, 18.24 XP result/autosave, restart/Continue та повторне накопичення до 36.48 XP пройдено; Full-HD tree/result оглянуто. Build/native keyboard/широка регресія/pacing поза scope. **Далі U5c — перший node.dish_value.01**, після власного payout/rounding/price контракту. U5 ще частковий; пізні XP tiers та інші effects недоступні. Нижчі checkpoints — історія.


**U5a реалізовано — 2026-10-26.** [Контракт додаткової Dish XP](ClickCook_U5a_Dish_Xp_Flat_Contract.md): `node.dish_xp_flat.01` / «Нотатки до рецепта» після Click Strength 1, 5 рангів по +3 XP (0–15), ціни `ceil(100 × 1.7^N)`. Бонус один раз за завершену страву через спільний receipt; actual XP обрізається на mastery cap, payout/rarity-before-reward незмінні. Працюють detail/current→next, сьомий рядок actual statistics, result XP та schema 12 з читанням 1–11. Compilation, локальні all-rarity/rank/cap/source/receipt/storage, попередній auto model без бонусу, дві native mouse покупки, controller result +16 XP/autosave і restart/Continue пройдено; Full-HD tree/result оглянуто. Build/native keyboard/широка регресія/pacing поза scope. **Далі U5b — перший node.dish_xp.01**, після окремого factor/order/fractional XP контракту. Інші U5 effects і пізні flat tiers ще недоступні. Нижчі checkpoints — історія.


**U4e реалізовано — 2026-10-26.** [Контракт частоти авто](ClickCook_U4e_Auto_Frequency_Contract.md): `node.auto_frequency.01` / «Темп помічника I» після action_count 1, 15 рангів, +0.15 циклу/с (1–3.25/с), чинні `ceil(600 × 1.8^N)`. Scheduler використовує `1/frequency` для першого й наступних deadlines, без catch-up; pause/Back/suspend починають новий повний інтервал. Сила/count/start chance/manual/Hold незалежні; UI показує поточну частоту. Schema 11 читає 1–10 без втрати прогресу. Compilation, локальні frequency/storage та rank-zero periodic model, дві native mouse покупки, live timer/pause/periodic result/autosave і restart/Continue пройдено; огляд 1920×1080. Build/широка регресія/native keyboard/pacing поза scope. **Перші вузли п'яти базових auto families U4a–e готові; всі пізні auto tiers ще не завершено. Наступна глава — U5**, найближчий незалежний slice — перший Dish XP upgrade після власного effect/числового/receipt контракту. Character/Restaurant XP та recipe/economy гілки потребують базових milestones 8–9. Нижчі checkpoints — історія.


**U4d реалізовано — 2026-10-26.** [Контракт множника авто](ClickCook_U4d_Auto_Multiplier_Contract.md): `node.auto_strength_multiplier.01` / «Школа су-шефа I» після auto_strength 1, 10 рангів, +0.1 до factor (×1–×2), `ceil(1000 × 1.8^N)`. Шкода `ceil((1 + strengthRank) × factor)` однакова для start/periodic; UI показує реальні HP та rounding plateaus. Schema 10 читає 1–9 без втрати wallet/XP/старих рангів. Локальні model/storage, попередній strength model, дві native mouse покупки, controller start/tick/result/autosave і restart/Continue пройдено через Unity MCP та ComputerUse; огляд 1920×1080. Native keyboard/build/широка регресія/pacing поза scope. **Наступний slice — U4e: перший node.auto_frequency.01**, після власного числового/scheduling контракту. Пізні tiers лишаються недоступними; U4 ще частковий. Нижчі checkpoints — історія.


**U4c реалізовано — 2026-10-26.** [Контракт](ClickCook_U4c_Auto_Strength_Contract.md): перша сила авто, 15 рангів по +1 HP, однаковий damage для start/periodic, schema 9. Локальні model/storage, дві native mouse покупки, controller tick/start/result/autosave і restart/Continue пройдено. ComputerUse та Unity MCP доступні. Native keyboard Submit не підтверджений; Player build і широка регресія поза scope. Наступний execution slice — **U4d: node.auto_strength_multiplier.01**, потім frequency; U4 ще не завершено. Нижчі checkpoints — історія.

**U4b реалізовано — 2026-10-25.** [Контракт](ClickCook_U4b_Auto_Start_Contract.md): перший auto_action_chance працює незалежно від periodic unlock, по одному стартовому roll на order; Back не повторює успіх/промах. Поточний звичайний auto batch без manual RNG, bounded completion/result/автозбереження з однією нагородою. 10 рангів, шанс до 20%, окрема ціна; schema 8 читає 1–7. Локальні model/storage/purchase/controller/Back/periodic/result/Continue та огляд 1920×1080 пройдено. Наступний execution slice — **U4c: node.auto_strength.01** після власного numeric/effect контракту; multiplier/frequency далі окремо. Фізичний input/build/pacing не перевірялися; U4 загалом ще не завершено.

**U4a реалізовано — 2026-10-24.** [Контракт авто](ClickCook_U4a_Auto_Contract.md): перший action_count вмикає 1 цикл/с із 2–11 діями по 1 HP, свої 10 рангів/ціни, без manual RNG. Лише активний order, один bounded batch/кадр без catch-up/переносу/вибору наступного; спільний completion і одна нагорода. Пауза/focus/system pause/disable/Back/result скидають таймер, Resume починає повний інтервал. Schema 7 переносить 1–6. Локальні model/storage/live tick/pause/result/Continue й огляд 1920×1080 пройдено; фізичний input/build/pacing не перевірялися. **U4 триває:** наступний U4b — auto_action_chance.01 з одним стартовим roll/order за manifest; інші auto effects — наступні окремі slices.

**U3b реалізовано — 2026-10-24.** [Контракт](ClickCook_U3b_Triple_Strike_Contract.md): другий ступінь double chance має 5 незалежних рангів/цін, перший triple — 10. Сумарний double до 30%, triple до 10%, один взаємовиключний proc-roll, максимум три фактичні удари з чинною силою повтору, crit/one-receipt/no-carry guards. Каталог підтримує окремі node ціни/caps; schema 6 зберігає обидва вузли та переносить 1–5. Локальні model/storage/дві покупки/pointer/Submit/Continue й огляд 1920×1080 пройдено. Наступний execution slice — **U4: перша система автоготування**, після власних activation/числових правил; без автоматичного запуску всіх пізніх auto nodes. Фізичний input/build/pacing не перевірялися. Нижчі checkpoints — історія.

**U3 реалізовано — 2026-10-24.** [Double strike contract](ClickCook_U3_Double_Strike_Contract.md): перші chance/power вузли працюють і купуються, по 10 рангів, шанс до 20%, сила повтору від ×0.5 до ×1. Один bounded manual/Enter/Hold trigger з незалежними критами, no-chain/no-carry/reward-once; UI показує фактичні chance/power і два рухи. Schema 5 переносить профілі 1–4. Локальні model/storage/дві купівлі/pointer/Submit/completion/Continue й огляд 1920×1080 пройдено. Наступний execution slice — **U3b: node.double_strike_chance.02 та перший triple**, з окремими цінами/внесками й joint distribution. Інші пізні tiers недоступні; фізичний input/build/pacing не перевірялися. Нижчі checkpoints — історія.

**U2c реалізовано — 2026-10-24.** [Контракт частоти](ClickCook_U2c_Hold_Frequency_Contract.md): перший `node.hold_frequency.01`, 10 рангів після Hold 1; +0.5 руху/с за ранг, від 4 до 9/с, ціна `ceil(300 × 1.6^N)`. Реальні темп/preview/statistics/hint та schema 4 працюють; старі профілі зберігають прогрес. Локальні model/storage/купівля/Hold/Continue й огляд 1920×1080 пройдено; фізичний input/build/pacing не перевірялися. Другий frequency tier очікує окремого числового контракту. Наступний execution slice — **U3: шанс і сила додаткового удару**. Нижчі checkpoints — історія.

**U2b реалізовано — 2026-10-24.** [Hold contract](ClickCook_U2b_Hold_Contract.md): node.click_hold.01, один ранг за 250 у.е. після Click Strength 2. ЛКМ дає початковий удар при натисканні та повтори кожні 0.25 с через чинний manual/critical pipeline; release/click не додає удару. Pause/focus/Back/result/end/disable/зміна order скасовують Hold без накопичення повторів. Enter лишається дискретним. Schema 3 додає Hold, читає 1/2 зі збереженням wallet/XP/попередніх рангів. Локальні model/storage/PointerEventData/lifecycle/Continue перевірки й огляд 1920×1080 пройдено; фізичний input/build не виконувалися. Наступний окремий підкрок — **U2c: hold_frequency**, після числового контракту; frequency ще не продається.


**U2a реалізовано — 2026-10-24.** Перші вузли шансу та множника криту працюють і купуються за [контрактом U2a](ClickCook_U2a_Critical_Contract.md): шанс +2 п.п./ранг до 30%, множник ×2 +0.1/ранг до ×3.5, передумови Click Strength 3 / шанс криту 1. Один roll на прийнятий ручний удар; damage округлюється вгору, одна виплата/order. Дерево показує окремі ранги, ціну/current→next/locks і фактичні crit statistics; HP має текстовий critical feedback. Schema 2 зберігає nodeId→rank; schema 1 переносить legacy Click Strength без зміни wallet/XP/сили й без запису при Load. Локальні model/storage/UI/restart перевірки пройдено; build/фізичний input/full pacing не перевірялися. Наступний окремий підкрок — **U2b: Hold**; наступні crit tiers/частота ще недоступні.


**U1 реалізовано — 2026-10-24.** Екран дерева містить усі 92 плитки з [manifest](ClickCook_Upgrade_Node_Manifest.md), ребра за явними prerequisites, вибір мишею/клавіатурою, деталі з прокручуванням, фактичні активні характеристики зі Hide/Show, pan і zoom 30–140% із центруванням на вибраному вузлі. Купується тільки `node.click_strength.01` через чинні quotes/ціни/15 рангів/автозбереження 6A/7B; безкоштовний root віртуальний, інші вузли пояснюють відсутність ефекту та виконання передумов. Числових заглушок/нових записів schema-1 немає. Локальні contract/UI/interaction перевірки й візуальний огляд 1920×1080 пройдено; фізичний mouse/keyboard pass і Player build не виконувалися. Наступний окремий крок — U2, з локальним контрактом конкретного effect slice.


**Актуальний checkpoint — 2026-10-23: U0 завершено після повного читання наданих popup.** [Node manifest](ClickCook_Upgrade_Node_Manifest.md): 92 кухонні плитки, 38 effect families + root, власний узгоджений graph, описи та source provenance. 76 source records + 16 позначених доповнень ClickCook. Source-read gate закрито в межах наданого набору. Наступний крок — **U1: дерево / detail / active statistics / pan / zoom**, продаж лише чинного node.click_strength.01, інші вузли недоступні до реалізації ефекту. Legacy 6A/7B зберігається. Попередні 27/31/36 counts і відкритий source gate нижче — історичні checkpoints. Баланс нових ефектів перевіряється локально у U2–U6, а не широким build у U0.

**Продовження source audit — 2026-10-23:** Звірено й записано додаткові popup-спостереження; зарезервовано 36 сімейств. Нові відносно попереднього checkpoint: day_income_bonus, location_value, selection_precision_window, order_tracker_tier, order_tracker_frequency. На огляді 76 плиток, але детальні знімки містять додаткові ступені; повний count/graph ще не доведений. Source gate перед U1 лишається відкритим. Погоджені правила швидкого вибору та переносу часу від критів збережено. Поточна матриця/перелік непрочитаного: [FishSim audit](ClickCook_FishSim_Upgrade_Audit.md).


**Уточнення власника / source audit — 2026-10-22:** Перед U1 потрібна повна відповідність плиткам і механікам FishSim, а не лише 27 сімейств. [Матриця звірки](ClickCook_FishSim_Upgrade_Audit.md) фіксує прочитані вузли, кухонні назви та прогалини. Наразі зарезервовано 31 сімейство (+auto_action_chance, +selection_start_progress, +critical_time_bonus, +triple_strike_chance). На огляді пораховано 76 плиток; повний manifest ще уточнюється. Погоджено: бонусний таймер швидкого вибору дає стартовий прогрес, повільний вибір без штрафу. **Source gate U0 перед U1 відкритий; екран ще не впроваджувати.**


**Актуальний checkpoint — U0, 2026-10-22:** виконано контракт дерева/даних: [U0](ClickCook_U0_Upgrade_Contract.md). Закріплено 27 ID, граф і стартовий баланс першої manual-гілки та окремий облік purchased ranks/source grants. Нових runtime effects немає. Наступний крок — **U1**. Правило локальних перевірок власника збережено в кореневому AGENTS.md.


**Актуальний checkpoint — 2026-10-22: 7B виконано й перевірено в Editor та Windows x64 Player.** Один локальний профіль і backup, автозапис wallet/Dish XP/Click Strength, Continue, підтверджена New Game, мінімальна Pause й обробка помилки запису. Погоджено: незавершений день після перезапуску скидається без додаткових нагород; зароблене зберігається; New Game замінює також backup; пауза блокує ігрові дії. Native mouse цикл «заробив → купив → вийшов → Continue» зберіг wallet/XP/ранг; фінальна збірка Succeeded/0 errors. Докази — IMPLEMENTATION_STATUS.md. Наступний крок — **U0**, потім U1. Нижчі датовані твердження про відсутність 7B є історією попередніх checkpoints, а не поточним станом.

**Окрема правка — 2026-10-21:** відновлено роботу прилавка в оновленому користувачем Dormitory: посилання на дошку/тарілку та орієнтація площини аркушів. Збережено нову модель, матеріали й розташування; копія до ремонту — `ArtSource/Recovery/Dormitory_BeforeRepair_2026-10-21/`. Також завершено запитану поза milestone анімацію складання Burgers (ID 6) з готових Blender-частин за фактичним HP-прогресом. Деталі перевірок — IMPLEMENTATION_STATUS.md. Номер наступного roadmap-кроку залишається **7B**, save/load не розпочато.

**Останній checkpoint — 7A, 2026-10-15: виконано й перевірено.** Єдиний run-local PlayerProfile володіє wallet/Dish XP/рангами; день/екрани відокремлені; стабільні наявні ID, незалежний read-only snapshot. Прогрес переживає перезавантаження SampleScene, але не перезапуск гри. Деталі перевірок/файли — IMPLEMENTATION_STATUS.md. Наступний окремий крок — 7B save/load; recovery/New Game/reset/pause правила закрити перед реалізацією. Нижчі згадки «наступний 7A» описують попередні checkpoints.

## Поточний пріоритет — синхронізація Figma/GDD, 2026-10-15

Figma/FigJam дизайнера має вищий пріоритет за ранні трактування GDD; останні явні рішення користувача уточнюють Figma. Див. GDD §0, §13.2 та upgrade proposal §12. FishSim визначає і ширину/механіки прогресії, не лише стиль. Нижче початкові факти аудиту 2026-10-12 залишені історичними; ця синхронізація не є новою Unity compilation/Play Mode перевіркою.

**UI-A4 завершено й перевірено 2026-10-15:** аркуші на фізичній дошці праворуч → страва на реальній тарілці, pointer/Submit/камера/HP. Усі 11 локацій на п'яти rendered sizes, regression і native mouse цикл пройдено. Деталі/файли — IMPLEMENTATION_STATUS.md; final art, physical keyboard retest і standalone build залишаються поза цим checkpoint. Найближчий окремий крок — 7A profile/stable IDs.

Потім 7A/7B profile/save та U0/U1 широкого дерева; механіки додаються U2–U6 маленькими slices. Ціль — 27 планових сімейств (16 існуючих + 11 доповнень), а не довічне обмеження поточним каталогом. Точний баланс/граф кожної гілки закривати до її продажу. Наявність design specification не дає [x] runtime.

Затверджені уточнення: double strike = шанс додаткового удару по тій самій страві, не друга порція; критики/ризик кар'єри збережені, але їхній штраф/reset й активація очікують окремого погодження. Рецепти й таблиця досягнень уже мають дизайнерський контент; залишаються семантика/інтеграція, не завдання вигадати все наново.

Дата перевірки: 2026-10-12. База Git: 8be8222, Milestone 5B.
Статус: план робіт і рекомендації для погодження. Нові правила гри не затверджуються цим документом.

Оновлення після виконання 6A (2026-10-12): Click Strength і міжденна купівля реалізовані та перевірені; нижче опис поточного стану розділу 2 фіксує аудит ДО 6A. Каталог має 16 визначень, лише один доступний ефект. Після окремого завершення UI-A4 актуальний next slice — 7A, потім 7B; інші R0-рішення закриваються перед залежними кроками.

Джерела: [GDD](ClickCook_GDD.md), [Implementation Status](IMPLEMENTATION_STATUS.md), [проєкт апгрейдів](ClickCook_Upgrade_Design_Proposal.md), поточні C# скрипти/дані та read-only перевірка Unity MCP.

Оновлення UI/UX: [повна Unity UI/UX специфікація](ClickCook_UI_UX_Specification.md). UI-A1/A2/A3/A4 завершено у своїх межах. Фізичні Enter/стрілки користувач перевірив для UI-A1; нові spatial Submit/move перевірені через EventSystem, новий фізичний keyboard pass не виконано. Синтетична клавіатура Computer Use залишається обмеженням автоматизації. Figma-звірка 2026-10-15 уточнила spatial дизайн і gameplay scope; структуровані таблиці мають перевагу над ілюстративними числами. Нові upgrade mechanics не реалізовані цими UI-кроками.

## 1. Висновок

UI-A3 — виконані правки після тесту користувача (2026-10-12): Exit у поточному prototype menu, видимий HP/залишок кліків, великі центральні картки за новим референсом і підписаний Dish mastery. Hidden-HP/right-board presentation superseded; числові правила не змінено. Standalone quit перевірити разом із наступним build checkpoint; повний Main Menu залишається 7B, final dish art — UI-C/11A.

ClickCook має реалізований прототип основного циклу до готування, рідкості, Dish XP, виплати та завершення дня. Наступна продуктова мета — повний повторюваний цикл «заробив → покращив → зберіг → повернувся → відкрив новий контент».

Рекомендована послідовність після виконаного UI-A4: постійний профіль/save → U0/U1 дерево й working Click Strength у ньому → crit/hold/double strike/auto незалежними підкроками → підсумок дня/рецепти/рівні/локації → залежні XP/value/passive/quality та source achievements → контент/сюжет і фінальний UX → beta → release candidate → публікація. R0-рішення закриваються перед залежними етапами. Розташування рядків таблиці не є дозволом реалізувати всі підкроки за один запуск.

Одиниця виконання — один малий підкрок з окремою перевіркою і Git checkpoint. Повне дерево апгрейдів не потрібно реалізовувати до першої збережуваної ігрової сесії.

## 2. Факти поточного стану

### Перевірено цього разу

- Unity MCP відповідає: ClickCook, Unity 6000.3.14f1, Editor ready, compiling=false, Play Mode stopped.
- Активна SampleScene завантажена, isDirty=false. Єдина сцена у Build Settings; ціль StandaloneWindows64.
- URP 17.3.0, Input System 1.19.0, uGUI 2.0.0, Test Framework 1.6.0 за manifest.
- Шість коренів сцени: Directional Light, Dormitory, Global Volume, Camera, Directional Light (1), ClickCook_Prototype.
- Під прототипом: Canvas, Location Selection, Work Day із шістьма слотами, Cooking, Dish Result та EventSystem. Використовується UnityEngine.UI.Text.
- На диску 32 DishData, 11 LocationData; Unity AssetDatabase розпізнає всі 11 посилань LocationCatalog.
- Вісім авторських C# файлів: два типи static data, LocationCatalog, controller, order, session, rewards і camera.
- Одна FBX-модель Dormitory; у перевіреному Assets немає .prefab, .wav/.mp3/.ogg чи власних .asmdef. Gameplay-тести, записані в трекері як попередні MCP перевірки, не є збереженим тестовим набором у репозиторії.
- На початку огляду вже були незакомічені зміни GDD/трекера та upgrade proposal + його .meta. Вони збережені.

### Реалізовано за кодом і попереднім протоколом перевірки 5B

- Каталог і carousel всіх 11 локацій; наразі всі доступні для перегляду/старту дня.
- Рівномірна випадкова генерація незалежних замовлень; дублікати дозволені; одна активна страва.
- Приховані HP = ClickRequirement; один damage за клік; Back скидає часткове готування.
- Завершення/заміна замовлення, максимум десять завершень, ручний Finish Day.
- Рідкість за старим рівнем конкретної страви, Dish XP/рівні 1–18, виплата один раз.
- Decimal wallet і Dish XP зберігаються між днями лише поточного запуску.
- Десятий результат залишається видимим; Continue завершує день.

Цього разу не запускали Play Mode, повторну компіляцію або build. Ready/compiling=false не є новим доказом успішної компіляції чи runtime-тестом. Історичні успішні перевірки — у трекері 5B.

### Виявлені неузгодженості документації

1. GDD §30, трекер і upgrade proposal називають Dormitory порожнім. Поточний 00_Dormitory.asset має два коректні посилання, підтверджені Unity MCP: 01_Sandwiches.asset та 02_Mugcakes.asset. Історичну заяву про нуль кандидатів не переносити як поточний факт. Це не доводить майбутнє право власності на обидва рецепти.
2. GDD §29 не перелічує PrototypeDishRewards.cs серед авторських скриптів, хоча він існує та виконує 5B.
3. Деякі TBD у §26–27/31 вже частково закриті 4B/5B; спиратися на конкретні approved/implemented правила й §33, не повторювати реалізацію.
4. Upgrade proposal залишається DRAFT. Відповіді про перевірку MCP або запит roadmap не означають схвалення його формул, таймера бонусу чи пасивної квоти.

Перед реалізацією синхронізувати ці факти в GDD/трекері, зберігши історію перевірок і всі дизайн-вимоги.

## 3. Релізний контракт — R0

GDD §1 залишає платформу, аудиторію, монетизацію та фінальний обсяг TBD. Це перший продуктовий checkpoint, який можна закрити паралельно з уточненням апгрейдів.

Рекомендації:
- Перший технічний маршрут — Windows x64 із мишею/клавіатурою, бо такий поточний build target. Магазин та комерційна модель потребують окремого вибору; Steam не вважається автоматично погодженим.
- План повного GDD-релізу включає 11 локацій, 32 страви, погоджені гілки апгрейдів, Journal, Achievements, Settings/Pause та persistence.
- Vertical slice на перших 2–3 локаціях — внутрішній етап перевірки, не мовчазне скорочення релізного обсягу.
- Наявний Dormitory зберігати; обсяг доробок і варіанти інших середовищ погодити до серійного виробництва артів.
- Визначити цільову тривалість загального проходження, мови, мінімальне обладнання, підтримувані роздільності та бюджети performance. 60–90 секунд у GDD стосується темпу дня, не автоматичного таймера.
- Визначити release acceptance і відповідальних за дизайн, код, UI/арт, звук, QA та публікацію. Реальні календарні терміни оцінити після цих рішень.

Gate R0: один погоджений release brief; окремий список відкритих рішень із milestone, який кожне блокує.

## 4. Дорожня карта з залежностями

Усі незавершені рядки нижче заплановані; не позначати їх як виконані до відповідної перевірки. Повний дизайн UI описаний у специфікації; його оформлення в Unity відбувається по готовності систем.

| Крок | Результат для гравця / обсяг | GDD | Залежності | Критерій виходу |
|---|---|---|---|---|
| R0 | Релізний контракт і актуалізація фактів документації | 1, 25, 29–33 | Поточний огляд | Платформа/обсяг визначені; поточний стан відділено від історії |
| 6A — виконано | Заробити → купити ранг Click Strength → готувати швидше; тимчасовий екран покупки | 9, 13–14 | Піднабір формул прийнято за дорученням користувача | Unity compilation, 4,685 model / 236 Play Mode assertions, EventSystem перевірки пройдено |
| UI-A1 — виконано | Дизайн-система, читабельна типографіка, scalable компоненти, Location Selection і Click Strength purchase у стилі референсу 4 | 5, 13, 21–22 | 6A; чесно позначена venue illustration placeholder | Compilation, 237 regression assertions, 5 layouts/raycast/focus і native mouse PASS; фізичні Enter/стрілки підтверджено користувачем |
| UI-A2 — виконано | OrderCard/HUD, Cooking і Dish Result поверх середовища у стилі референсу 5; тимчасові ескізи | 7, 9–11, 21–23 | UI-A1 | 237 regression + 240 presentation + 10 edge + 795 layouts/raycast/focus checks PASS; нативний цикл мишею PASS; daily count окремо від capacity, XP bar із реальних даних |
| UI-A3 — виконано | Exit у поточному меню; HP/click count; центральні великі 2–6 картки з підписаною майстерністю | 7, 9, 21 | Погоджені правки користувача після тесту UI-A2 | Compilation, 237 regression + 240 presentation + 30 review + 980 layout/input checks PASS; native HP/Back/Exit Editor PASS; standalone quit ще перевірити |
| UI-A4 — виконано | Аркуші на справжній дошці праворуч; 3D-ескіз на реальній тарілці; camera/pointer/Submit/HP | 0, 7, 9, 21–23 | Завершено існуючу локальну роботу; без зміни gameplay math | Compilation, 4685 model + 237 Play + 240 presentation + 10 edge PASS; 11 locations × 5 sizes, 880 lifecycle + 69391 spatial/text checks PASS; native mouse board/cook/Back/result/Finish і slot 6 PASS; physical keyboard/build не перевіряли |
| 7A — виконано | Єдиний in-memory профіль гравця й відокремлення даних дня від прогресу | 20, 26–27 | 6A; disk recovery лишається 7B | Wallet, Dish XP і ранги мають стабільні ID та незалежний snapshot; один run-власник; scene reload/fresh run перевірено |
| 7B — виконано | Save/load, Main Menu, Continue/New Game/Reset та мінімальний Pause, використовуючи UI-A компоненти | 3–4, 18–20 | 7A; правила погоджено 2026-10-22 | Editor restart, atomic file/backup, failure/retry/reset, UI й Windows native earn/purchase/quit/restart/Continue перевірено; звіт — IMPLEMENTATION_STATUS.md |
| U0 — виконано | Точний graph/баланс першої нової гілки, stable IDs усіх планових сімейств, розділення rank і achievement source | 0, 13.2, 16 | Upgrade proposal §12; без runtime продажу неготових ефектів | Визначені ціна/ефект/rounding/передумови майбутнього slice; немає циклів або несумісного перейменування 6A |
| U1 — виконано 2026-10-24 | Широке дерево, node detail, active statistics, zoom/pan; робочий Click Strength і чесно недоступні інші ефекти | 13, 21 | U0, чинна purchase логіка; 7B для production retention | Mouse/keyboard навігація; Buy/locked/unaffordable/max/unimplemented розрізняються; жодних фіктивних продажів |
| 8A — виконано 2026-10-28 | Підсумок дня: страви, отримані гроші/Dish XP, Next Day, Upgrades | 6, 14, 21 | 7B; earnings = фактичні receipts; календар відкладено явно | Звіт дорівнює сумі receipts, без повторної виплати; receipt autosave/reload і переходи перевірено; контракт 8A |
| 8B — виконано 2026-10-28 | Купівля32 рецептів і фактична допустимість замовлень | 5, 8, 15 | 7B; starter Sandwiches; LocationData serving map; Michelin4 за рішенням власника | Пул = власні рецепти ∩ чинний список локації; старт завжди має безкоштовні сендвічі; schema17/purchase/save/restart перевірено; контракт8B |
| 8C | Player XP/level і відкриття локацій | 5, 12, 15 | 8A–8B; пороги/зв'язок рівнів | Гравець проходить початкові локації без debug-доступу та повторного XP |
| U2a/U2b/U2c — виконано | Перші Critical chance/multiplier, Hold і hold_frequency вузли працюють; наступні tiers — окремі slices | 9, 13 | U0/U1, 6A, 7B; локальні числові контракти | Коректні RNG/округлення/input, частота 4–9/с; pause/Back не залишають натиснення |
| U3 — виконано | Перші вузли шансу й сили додаткового удару по тій самій страві | 9, 13.2 | U0/U1, U2; локальний U3 contract | Незалежні крити, no-chain/no-carry/reward-once, schema 5 |
| U3b — виконано | Другий ступінь double chance та перший triple | 9, 13.2 | U3; контракт U3b, node prices/caps, manifest prerequisites | P(2)=d / P(3)=t, максимум три фактичні удари, no-chain/no-carry/reward-once; schema 6 |
| U4a–U4e — перші вузли виконано | Periodic action_count, start chance, strength, multiplier і frequency працюють; пізні tiers окремо | 9, 13 | U0/U1, 7B; контракт кожного slice | Exact interval 1/frequency, без combat proc/catch-up/carry/повторного старту на Back, одна нагорода; schema 11 |
| U5 — U5a/U5b/U5c виконано / U5d далі | Перші Dish XP flat/multiplier та Dish Value працюють; далі upgrade discount, Character XP, інші гілки окремими slices | 10, 12–15 | Відповідна базова XP/recipe система, 7B, U0; effect/order/rounding перед продажем | Exact XP, payout final-round-to-cents, одна receipt/XP/виплата/save, schema 14; інші U5 effects ще не завершено |
| 9A / U5 | Restaurant XP та flat/multiplier upgrade-гілка | 10, 13, 27 | 8C; базові джерела/пороги | Кожна дія має погоджений ефект, незалежний від Player/Dish XP |
| 9B / U5 | Passive income та flat/multiplier | 5, 13–14 | 8A, 7B; accrual/focus/offline рішення | Дохід нараховується/зберігається один раз у погоджених станах |
| 9C / U5 | Timely-choice bonus / Dish-selection Time | 0.1, 7, 13 | Бонусне, не примусове вікно; exact seconds/reward контракт | Slow choice не видаляє orders; Back/reload не створюють зайвий бонус; hard timer дня не додається |
| 10A | Journal, авторські рецепти/collection і tutorial першої сесії | 17, 21 | 8B, 7B; наявні FigJam тексти, правила відкриття/індикаторів | Контент імпортований із джерела; немає вигаданих recipe minigames; перша покупка/Continue зрозумілі |
| 10B | Achievements та source бонуси | 16 | 25 source rows; уточнені trigger/units/stacking; стабільні події/профіль | Власність бонусу відокремлена від paid rank; grant і recovery один раз |
| U6 | Quality flat/multiplier модифікатори | 11, 13.2, 16 | Exact probability operator і source quality/chance units | Розподіл завжди 100%, без від'ємних шансів; baseline таблиця не переписується; усі крайні значення тестовані |
| N0 | Історія студента-кухаря, внутрішні думки та кулінарний щоденник | 1.1, 17, 23–24 | Designer copy/characters/triggers; готові milestone події | Контент має джерело й локалізаційні ключі; жодної нової економічної формули з тексту історії |
| N1 — design gate | Критики / ризик кар'єри | 0.1, 1.1 | Окреме погодження активації, штрафу/retained state, частоти й restaurant level meaning | Ніякого reset до погодження; після погодження — окремий implementation milestone та save/recovery тести |
| UI-C — по готовності систем | Повне дерево/statistics, Journal/Achievements, summary, menu/settings/pause, responsive й localization pass за UI/UX специфікацією | 3–4, 13, 16–22, 25 | 7B–10B та відповідні runtime effects | Усі екрани мають реальні bindings/поведінку; немає фіктивного прогресу, rewards чи Continue; навігація й довгі тексти перевірені |
| 11A | Один завершений візуально й звуково slice ранньої гри | 21–25 | 8C і базові upgrades | Справжній input/build, close-up, середовище, readability, звук, save |
| 11B | Повний контент 11 локацій/32 страв і всі погоджені вузли | 5, 8, 13, 22–24 | Slice прийнятий; усі потрібні базові системи | Кожна локація/страва/ефект доступні за правилами; ліцензії й реєстр артів |
| 12 | Feature-complete alpha: повний маршрут і повторний баланс | Увесь gameplay GDD | 6–11 | Новий профіль → фінальна локація → повернення в гру; усі екрани й системи |
| 13 | Beta: device/input/save/performance/локалізаційна QA | 18–25 + release brief | 12 | Пройдено затверджену матрицю пристроїв і сценаріїв; немає release blockers |
| 14 | Release candidate, магазин, публікація й підтримка | Release brief | 13 | Відтворювана перевірена збірка, релізні матеріали, rollback/hotfix план |

U2/U4/U5 зіставлені зі старими 6B–6D, щоб не втратити історію. U1 можна зробити до готовності всіх ефектів, але з чесними unavailable states. Production retention потребує 7A/7B; залежні XP/recipe/passive/quality гілки — відповідних базових систем. Один рядок із кількома ефектами розбивається на окремі виконання, а не одну велику реалізацію.

## 5. Завершені UI-A1/A2/A3/A4; наступний — 7A

UI-A4 завершено окремою реалізацією після документаційної синхронізації: реальна дошка/plate, кадр/ввід, same-order Back і весь чинний цикл перевірені. SampleScene збережено чистою поза Play Mode; static gameplay data й середовище збережені. Наступна окрема команда — 7A profile/stable IDs, потім 7B save/load та U0/U1; ці підкроки ще не розпочато.

Обсяг:
- Теплі пергаментні компоненти, orange CTA, readable font з українськими glyphs і style/text tokens.
- Location Selection: ілюстрація, один current location, arrows, actual level/income/dishes, Cook і доступ до готового Click Strength purchase.
- Upgrade card: rank/current → next strength, exact price/wallet та working states; недоступні гілки не отримують фіктивних ефектів.
- Адаптація layout до 1280×720, 1080p/1440p, 16:10/21:9 без дрібного body text.
- Runtime purchases/cooking/data використовуються з 6A; підкрок не додає нові gameplay правила.

Цей обмежений UI підкрок виконано зі стилізованими placeholders та наявним шрифтом. Повний фінальний UI усіх екранів завершується по готовності runtime систем; UI-A1 не залежить від save/load або recipe gating.

Рекомендовані області змін:
- Assets/UI/Art, Fonts і Prefabs/Components для необхідних scalable елементів;
- відповідні screen prefabs або точкові UI blocks у SampleScene;
- view/text bindings у поточному controller чи мінімальні Assets/Scripts/UI компоненти за потреби;
- screenshots/layout/regression verification і GDD/трекер тільки після фактичного виконання.

Критерії:
- Читабельність/contrast, довгі назви, ціни та states перевірені на цільових layouts.
- Усі 11 carousel locations й Click Strength transactions проходять існуючу регресію.
- Дані level/income/XP беруться з runtime, не з намальованих прикладів референсу.
- Реальні mouse/raycast/focus взаємодії перевірені після оновлення кадру.
- Unity compilation, bindings і saved scene перевірені; середовище/статичні дані збережені.
- [x] UI-A1 перевірки виконано.
- [x] UI-A2 Board/Cooking/Result оформлено, automated layout/input та native mouse перевірки виконано; фінальні ілюстрації/анімації залишаються майбутніми кроками.

## 6. Persistence — рання обов'язкова залежність

7A виконано: PlayerProfileRuntime володіє PlayerProfile, який містить єдиний Rewards та ранги; controller створює PrototypeOrderSession із цим профілем. Session зберігає тільки тимчасовий день/замовлення/result. Незалежний snapshot містить wallet/Dish XP/ранги за чинними ID. Scene reload перевірено без втрати прогресу чи перенесення незавершених замовлень; новий запуск починається з нуля. Це не disk persistence або рішення interrupted-save recovery. Не будувати загальний framework і не переписувати весь прототип одним кроком.

Зберігати:
- версію схеми, wallet, XP/прогрес за стабільними dish/location/upgrade ID;
- власні рецепти/апгрейди, Player/Restaurant XP після їх додавання;
- journal/achievement flags, налаштування та погоджені day/date поля;
- стан незавершеної транзакції/дня лише відповідно до погодженої recovery-політики.

Важливе рішення: або відновлювати активний день, або завершувати/відкидати незакінчені замовлення при завантаженні. Другий варіант простіший, але його треба погодити. Уже видані нагороди не мають губитися або видаватися повторно в жодному варіанті.

Потрібні atomic write, попередня валідна копія, schema migration, обробка пошкодженого/відсутнього файлу й повідомлення при помилці запису. Збереження при підтвердженій нагороді/покупці/завершенні дня має відповідати транзакційній межі. Unity object references не записувати як persistent identity.

## 7. Правила прогресії, без яких не випускати гру

- Стартові owned recipes: Sandwiches безкоштовний за таблицею, Mug cakes коштує 150; наявність у location pool сама по собі не означає купівлю. Потрібне явне правило.
- Визначити один авторитетний availability map; перевірити суперечності між двома напрямками статичних посилань.
- Michelin: погодити точний перелік доступних страв.
- Player level 1–10 і location level 0–10 не мають документованого однозначного зіставлення. Не порівнювати їх напряму без рішення про фінальне відкриття.
- Player XP: кумулятивні пороги чи ціна переходу; округлення Daily Earnings/100; що входить в earnings; видача при ручному/повному завершенні.
- Фінальна capacity mapping і ціль проходження/повторної гри. Поточні 2,2,3,3,4,4,5,5,6,6,6 лишаються provisional.
- Визначити, як гравець отримує першу доступну страву після нового unlock, не витрачаючи всі кошти на бездіяльний контент.

Gate progression: стартовий профіль завжди має шлях до заробітку, кожна необхідна покупка/локація досяжні, гравець розуміє наступну ціль.

## 8. Баланс і реальні ігрові тести

Математична коректність 5B та draft-формул не доводить темп гри. 70,000 базових HP потребують окремої перевірки в контексті реально доступних апгрейдів.

На кожному етапі вимірювати:
- час до першого завершення, першої покупки, наступного рецепта/локації;
- median/p90 тривалості страви та дня, кількість ручних кліків;
- приріст wallet і XP, час окупності покупки, вибір гравців між вузлами;
- випадки одноклікових дешевих страв, надмірного grind, пропуску цілих гілок, безвихідних станів;
- зрозумілість наступної цілі й зручність фізичного input.

Почати з локальних діагностичних вимірювань і спостереження за тестувальником; зовнішня аналітика потребує вибору платформи й погодження даних. Не вигадувати retention/монетизаційні KPI до визначення продукту.

Числа змінювати лише в погодженому балансувальному checkpoint, з міграцією прогресу, якщо це впливає на вже існуючі saves.

## 9. UI, контент та архітектура

Поточні папки:
- Assets/Scripts/Data — DishData, LocationData.
- Assets/Scripts/Runtime — controller, order/session, rewards; Locations/LocationCatalog.
- Assets/Data — Dishes, Locations, LocationCatalog.
- Assets/Scenes — SampleScene.
- Assets/Models — Dormitory; Assets/Settings — URP/settings assets.

Розширювати цю структуру за потреби: Runtime/Progression, Persistence, UI, Audio; Tests/EditMode і PlayMode; UI prefabs, Art/Dishes, Art/Locations, Audio. Це цільова організація, не завдання негайно перейменувати всі файли.

Під час 7A винести профіль від screen lifetime. Під час summary/menu додати явний state flow і захист переходів у runtime: нині частина заборон заснована на activeSelf UI. При винесенні reusable UI використовувати prefabs, зберігаючи scene references. Не міняти uGUI на іншу UI-систему без конкретної потреби.

Візуальний slice: один підготовлений dish asset, close-up з видимим середовищем, стан приготування, rarity reveal, звук кліку/результату, зрозумілий upgrade preview. Після прийняття стандартів — виготовлення 32 страв і презентацій 11 локацій. Число унікальних моделей/сцен для кожної локації потребує затвердження; дані 11 локацій не означають 11 готових 3D середовищ.

Для фінального UI погодити мови, font coverage, навігацію, масштабування, читабельність, reduced motion/flash, альтернативи надмірному кліканню. Tutorial/меню/pause/налаштування мають працювати в білдах на обраній платформі.

## 10. Release gates

### Vertical slice

Новий гравець без debug меню проходить перший день, отримує результат, купує upgrade/recipe, закриває гру, повертається через Continue й досягає наступної локації. Є прийнятна якість репрезентативного UI/3D/звуку. Не випускати зовнішню alpha до перевірки втрати/дублювання прогресу.

### Feature-complete alpha

Усі погоджені gameplay-гілки GDD реалізовані; потрібні рецепти/локації/вузли досяжні. Journal/Achievements/Settings/Pause/Main Menu/Summary готові функціонально. Відомі placeholder-и мають перелік і строк заміни до RC; непогоджені системи не видаються за готові.

### Beta

Перевірити новий save, upgrade старого save, помилку запису, пошкоджену копію, закриття під час нагороди/покупки/дня, повторне натискання, втрату фокусу й pause. Окремо — фізичний mouse/keyboard на заявлених пристроях, різні роздільності/мови, довга сесія, підтримувані FPS/memory/loading бюджети.

Створити збережений regression suite для math/transactions/save та короткі Play Mode/Player сценарії критичного циклу. Попередні одноразові MCP callbacks недостатні для довготривалої регресії. Unity підтримує Edit Mode, Play Mode й Player-тестування: [Unity 6.3 testing documentation](https://docs.unity.com/en-us/engine/6000.3/manual/scripting/test-framework-introduction).

Performance оцінювати на цільовому обладнанні у build. Unity рекомендує профілювання на цільовому пристрої: [Unity profiling guide](https://unity.com/how-to/profiling-and-debugging-tools).

### Release candidate / publication

- Жодного відкритого blocker/critical дефекту, що заважає запуску/проходженню або псує прогрес; інші відомі проблеми прийняті відповідальним за реліз.
- Чиста відтворювана збірка з зафіксованого commit/tag; включено правильні сцени, assets і версії packages.
- Перевірені release build і інсталяція на чистому оточенні; debug tools/remote development endpoints вимкнені або виключені.
- У проєкті є com.unity.pipeline: окремо перевірити його runtime/build конфігурацію перед поширенням; наявність пакета сама по собі не доводить відкритого endpoint.
- Ліцензії на арт/звук/шрифти, credits, магазинні матеріали, контакт підтримки; вимоги конкретного магазину перевірити після вибору каналу.
- Privacy/analytics/SDK документи — відповідно до фактично використаних сервісів. Не додавати рекламу/IAP/cloud лише заради релізу.
- Перевірка update/migration, резервна попередня збірка, changelog, порядок hotfix і відкату. Відкат executable не повинен ламати нові saves.
- Затвердження дати/публікації власником гри — після виконання release gates.

Після релізу: збирати повідомлення про crash/save/progression блокування, пріоритезувати виправлення, повторно проходити regression suite перед патчем.

## 11. Пріоритетні рішення власника гри

| Коли потрібне | Рішення | Рекомендація, яка ще потребує погодження |
|---|---|---|
| До release planning | Платформа, магазин, модель продажу, обсяг 1.0 | Почати технічно з Windows x64; окремо вибрати канал/комерційну модель |
| До 6A | Click Strength ціни/ранги/ефект, purchase screen | Використати погоджений піднабір upgrade proposal |
| До 7B | Save slots/recovery/New Game/reset/pause | Один локальний профіль з backup; interrupted-day policy погодити |
| До 8A–8C | Earnings/XP thresholds/стартові рецепти/unlocks/Michelin | Скласти явні таблиці правил; не підміняти їх поточним debug-доступом |
| До 9A | Restaurant XP джерела/пороги/користь | Визначити базову систему до продажу XP upgrade |
| До 9B–9C | Passive/bonus/time semantics | Окремо погодити; 90-секундна квота і 3–6-секундне вікно лише draft |
| До серійного art | Остаточний стиль/середовища/мови/input | Прийняти один завершений slice як зразок |
| До beta | Journal/Achievements та завершення/повторна гра | Повні таблиці контенту, прогресу й нагород; не вигадувати під час UI |

Не потрібно вирішувати всі TBD одразу. Кожне рішення закривається перед конкретним залежним кроком. Широке дерево, дизайнерський контент Journal/Achievements і сюжет не вилучаються через відсутність їхнього runtime. Зменшення продуктового обсягу можливе лише як явне погоджене рішення; critics/reset наразі мають окремий design gate за вибором користувача.

## 12. Workflow і результат цього огляду

Кожний підкрок: погоджений контракт → мінімальна реалізація → змістовна перевірка → GDD/Implementation Status → diff review → commit/push milestone-файлів → remote/local verification → STOP.

Початковий огляд створив лише цей roadmap. Пізніше UI-A1 додав пов'язану UI/UX специфікацію, Unity UI assets/код, оновлення SampleScene та перевірки; gameplay data, середовище, project settings і Figma не змінено.

Після реалізованого 7B найближчий checkpoint — U0 (контракт дерева/даних і залежностей), потім U1 (широке дерево та робочий Click Strength; інші ефекти чесно недоступні). U2–U6 додаються окремими slices за залежностями. Деталі acceptance 7B та обмеження — IMPLEMENTATION_STATUS.md. Не переходити автоматично до реалізації всіх upgrade effects.
