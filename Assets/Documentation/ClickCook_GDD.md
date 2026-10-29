# ClickCook — Game Design Document

**Поточний checkpoint — 2026-10-29: 8C завершено.** Player XP зараховується один раз із виплатою повного дня: earnings/100, точно до4 знаків, cap15000/level10. Власник погодив cumulative пороги0/100/300/700/1400/2500/4200/6600/10000/15000 та доступ: Dormitory на1, Café на2, далі послідовно, Fashion/Michelin обидва на10. Model і controller блокують закриті локації незалежно від купленого рецепта. Carousel показує вимоги; summary — фактично виданий Player XP; рецептні serving pools не змінено. Schema18 читає1–17 без втрати старого прогресу й без вигаданого ретро-XP. [Контракт8C](ClickCook_8C_Player_Progression_Contract.md).

Scoped compilation/model/storage/Play/restart пройдено. В ізольованому профілі повний день дав XP99.95→100.05, wallet123.25→133.25, level2/Café; restart відновив точні значення й доступ, ранній день не додав XP. Перевірено межі/cap/multi-level, рівні×локації, legacy/backup/reset/future protection, суміжні day-settlement і recipe моделі, нові UI підписи/32 shop details. Full-HD Game View оглянуто; native carousel показав level3 lock. Широкі suites/build/pacing не потрібні. Play stopped; save override removed; scene clean. Нижчі checkpoints історичні.

**Далі:** залежні U5 recipe/Player XP upgrades та9A Restaurant XP після окремого source/threshold-контракту.


**Поправка власника — 2026-10-28: достроковий день не приносить грошей.** Вартість замовлень1–9 накопичується лише в тимчасовому PendingDayEarnings. Finish раніше10 страв, меню/вихід/перезапуск анулюють її; попередній баланс і Dish XP залишаються. Десята receipt один раз зараховує всю суму дня в Wallet у своїй autosave-транзакції, ще до Continue. Summary показує реальні Earnings0 та ForfeitedEarnings для раннього дня. Result, Finish і Pause пояснюють правило. Schema17 незмінна: pending income не записується. Ця поправка має пріоритет над старими описами негайної оплати нижче. [Контракт8A](ClickCook_8A_Day_Summary_Contract.md).

Scoped compilation/model/save/Play перевірки пройдено: кожне раннє завершення1–9, повний день10, точний wallet/дробовий XP, replay/cleanup/no-carry, три страви→нуль грошей у save, наступний повний день→одна виплата та actual restart/Continue. Ранній summary оглянуто в Game View; тексти result/summary/Finish/Pause вміщаються. Player build/широка регресія не потрібні. Play stopped, fixture override removed, scene clean. Наступний roadmap-крок залишається8C.


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


**U4c реалізовано — 2026-10-26.** [Сила авто-дії](ClickCook_U4c_Auto_Strength_Contract.md): `node.auto_strength.01` після action_count 1, 15 рангів, +1 HP/дію (1–16 HP), `ceil(500 × 2^N)`. Спільна сила для першого входу й періодичного batch, без зміни manual/crit/proc, кількості, шансу чи таймера. Lethal зупиняє batch з однією нагородою; detail/count/chance/статистика показують реальну силу. Schema 9 читає 1–8 без втрати прогресу. Локальні model/storage, native mouse purchase через ComputerUse, controller start/tick/result/autosave/Continue пройдено через Unity MCP. Клавіатурний native Submit не підтверджений; build/pacing/широка регресія не виконувалися. **Далі U4d — перший auto_strength_multiplier**, потім frequency; U4 ще частковий. Нижчі checkpoints — історія.

**U4b реалізовано — 2026-10-25.** [Стартова дія помічника](ClickCook_U4b_Auto_Start_Contract.md): `node.auto_action_chance.01`, 10 рангів після Click Strength 2, +2 п.п./ранг до 20%, `ceil(250 × 1.8^N)`. Один шанс при першому вході в конкретне замовлення: без періодичного авто успіх дає 1 дію по 1 HP, з action_count — поточний batch. Успіх/промах/нульовий шанс позначають order відвіданим, Back не повторює roll; інший order має власну позначку. Lethal-старт одразу показує result з однією нагородою. Періодичний таймер незалежний. Detail/статистика й schema 8 працюють, старі 1–7 зберігають прогрес. Локальні model/storage/controller/Back/result/Continue та огляд 1920×1080 пройдено; фізичний input/build/pacing не перевірялися. **U4 триває:** наступний окремий slice U4c — перша auto_strength, потім multiplier/frequency. Нижчі checkpoints — історія.

**U4a реалізовано — 2026-10-24.** [Базове авто](ClickCook_U4a_Auto_Contract.md): перший `node.auto_action_count.01`, 10 рангів після Click Strength 2, `ceil(500 × 1.8^N)`. Перша покупка вмикає цикл 1/с, 2 дії по 1 HP; наступні ранги додають дію до 11/цикл. Лише вибрана страва, без критів/double/triple, боргу tick, переносу batch чи автоматичного вибору наступної. Пауза/focus/disable/Back/result скидають інтервал; після Resume повна секунда заново. Спільне completion нараховує одну нагороду. Авто feedback/статистика й schema 7 працюють; старі 1–6 зберігають прогрес. Локальні model/storage/реальні ticks/пауза/completion/Continue й огляд 1920×1080 пройдено; фізичний input/build/pacing не перевірялися. **U4 не завершено цілком:** наступний slice U4b — перший auto_action_chance, далі сила/множник/частота окремо. Нижчі checkpoints — історія.

**U3b реалізовано — 2026-10-24.** [Контракт triple](ClickCook_U3b_Triple_Strike_Contract.md): «Другий рух II» має власні 5 рангів/ціни й додає до 10 п.п. до перших 20%; «Потрійний рух I» — 10 рангів до 10% після II:1. Один proc-roll обирає взаємовиключний double/triple/звичайний результат: P(2)=d, P(3)=t, P(1)=1−d−t. До трьох фактичних ударів із незалежними критами, обидва повтори використовують double_strike_power; completion пригнічує решту ударів/RNG, нагорода одна. Node-specific ціна/межа, статистика й feedback працюють; schema 6 читає 1–5. Локальні model/storage/кнопки/pointer/Submit/Continue та огляд 1920×1080 пройдено; фізичний input/build/pacing не перевірялися. Наступний окремий крок — **U4: перший slice автоготування**, з локальним activation/numeric контрактом. Пізні manual tiers залишаються окремими slices. Нижчі checkpoints — історія.

**U3 реалізовано — 2026-10-24.** [Контракт додаткового удару](ClickCook_U3_Double_Strike_Contract.md): перші chance/power вузли мають по 10 рангів; шанс +2 п.п./ранг до 20%, сила повтору ×0.5 +0.05/ранг до ×1. Один manual/Enter/Hold trigger дає максимум два удари по тому самому order з незалежними критами; перший lethal удар пригнічує proc, повтор не породжує наступний proc. Damage округлюється після всіх множників, нагорода одна, переносу шкоди немає. Дерево/статистика/feedback працюють, schema 5 зберігає обидва вузли та читає 1–4. Локальні model/storage/купівля/pointer/Submit/Continue й огляд 1920×1080 пройдено; фізичний input/build/pacing не перевірялися. Наступний окремий slice — **U3b: другий ступінь chance та triple**, зі спільним розподілом proc до продажу. Нижчі checkpoints — історія.

**U2c реалізовано — 2026-10-24.** [Контракт частоти](ClickCook_U2c_Hold_Frequency_Contract.md): перший `node.hold_frequency.01`, 10 рангів після Hold 1; +0.5 руху/с за ранг, від 4 до 9/с. Ціна `ceil(300 × 1.6^N)`. Планувальник використовує 1/f, без накопичення повторів; дерево й підказки показують фактичний темп. Schema 4 читає 1/2/3 без втрати wallet/XP/рангів. Локальні model/storage/купівля/Hold/Continue й огляд 1920×1080 пройдено; фізичний input/build/pacing не перевірялися. Другий frequency tier недоступний до власного контракту. Наступний окремий крок — **U3: шанс і сила додаткового удару**. Нижчі checkpoints — історія.

**U2b реалізовано — 2026-10-24.** [Hold contract](ClickCook_U2b_Hold_Contract.md): node.click_hold.01, один ранг за 250 у.е. після Click Strength 2. ЛКМ дає початковий удар при натисканні та повтори кожні 0.25 с через чинний manual/critical pipeline; release/click не додає удару. Pause/focus/Back/result/end/disable/зміна order скасовують Hold без накопичення повторів. Enter лишається дискретним. Schema 3 додає Hold, читає 1/2 зі збереженням wallet/XP/попередніх рангів. Локальні model/storage/PointerEventData/lifecycle/Continue перевірки й огляд 1920×1080 пройдено; фізичний input/build не виконувалися. Наступний окремий підкрок — **U2c: hold_frequency**, після числового контракту; frequency ще не продається.


**U2a реалізовано — 2026-10-24.** Перші вузли шансу та множника криту працюють і купуються за [контрактом U2a](ClickCook_U2a_Critical_Contract.md): шанс +2 п.п./ранг до 30%, множник ×2 +0.1/ранг до ×3.5, передумови Click Strength 3 / шанс криту 1. Один roll на прийнятий ручний удар; damage округлюється вгору, одна виплата/order. Дерево показує окремі ранги, ціну/current→next/locks і фактичні crit statistics; HP має текстовий critical feedback. Schema 2 зберігає nodeId→rank; schema 1 переносить legacy Click Strength без зміни wallet/XP/сили й без запису при Load. Локальні model/storage/UI/restart перевірки пройдено; build/фізичний input/full pacing не перевірялися. Наступний окремий підкрок — **U2b: Hold**; наступні crit tiers/частота ще недоступні.


**U1 реалізовано — 2026-10-24.** Екран дерева містить усі 92 плитки з [manifest](ClickCook_Upgrade_Node_Manifest.md), ребра за явними prerequisites, вибір мишею/клавіатурою, деталі з прокручуванням, фактичні активні характеристики зі Hide/Show, pan і zoom 30–140% із центруванням на вибраному вузлі. Купується тільки `node.click_strength.01` через чинні quotes/ціни/15 рангів/автозбереження 6A/7B; безкоштовний root віртуальний, інші вузли пояснюють відсутність ефекту та виконання передумов. Числових заглушок/нових записів schema-1 немає. Локальні contract/UI/interaction перевірки й візуальний огляд 1920×1080 пройдено; фізичний mouse/keyboard pass і Player build не виконувалися. Наступний окремий крок — U2, з локальним контрактом конкретного effect slice.


**Актуальне рішення — 2026-10-23: U0 завершено.** [Повний план дерева](ClickCook_Upgrade_Node_Manifest.md): 92 окремі плитки, 38 effect families + root; 76 референсних записів FishSim і 16 власних доповнень. Усі popup із черги прочитано. Збережені crit, double/triple, hold, auto chance/count/strength/frequency, passive, окремі timing-вікна, XP/quality/economy й зменшення роботи страви. Назви/описи кухонні; кожна плитка має stable nodeId і прийняті prerequisites ClickCook. Повнота інших версій FishSim і буквальне копіювання ребер не заявляються. Погоджені власником бонусний вибір без штрафу й bounded carry від критів залишаються чинними. Наступний milestone — U1; він продає лише working legacy Click Strength. Баланс та реалізація інших effects — окремі U2–U6 slices. Manifest має пріоритет над історичними counts/відкритим source gate нижче.

**Продовження source audit — 2026-10-23:** Звірено й записано додаткові popup-спостереження; зарезервовано 36 сімейств. Нові відносно попереднього checkpoint: day_income_bonus, location_value, selection_precision_window, order_tracker_tier, order_tracker_frequency. На огляді 76 плиток, але детальні знімки містять додаткові ступені; повний count/graph ще не доведений. Source gate перед U1 лишається відкритим. Погоджені правила швидкого вибору та переносу часу від критів збережено. Поточна матриця/перелік непрочитаного: [FishSim audit](ClickCook_FishSim_Upgrade_Audit.md).


**Уточнення власника / source audit — 2026-10-22:** Перед U1 потрібна повна відповідність плиткам і механікам FishSim, а не лише 27 сімейств. [Матриця звірки](ClickCook_FishSim_Upgrade_Audit.md) фіксує прочитані вузли, кухонні назви та прогалини. Наразі зарезервовано 31 сімейство (+auto_action_chance, +selection_start_progress, +critical_time_bonus, +triple_strike_chance). На огляді пораховано 76 плиток; повний manifest ще уточнюється. Погоджено: бонусний таймер швидкого вибору дає стартовий прогрес, повільний вибір без штрафу. **Source gate U0 перед U1 відкритий; екран ще не впроваджувати.**


**U0 checkpoint — 2026-10-22:** Design/data contract completed: [U0 upgrade contract](ClickCook_U0_Upgrade_Contract.md). All 27 family IDs reserved, existing 16 retained, initial manual branch prerequisites/prices/effects fixed as adopted initial balance; purchased ranks separated from achievement grants. No runtime/catalog/save changes. Next milestone: U1. Later implementation reports below remain historical.


**Current implementation checkpoint — 2026-10-22:** 7B local persistence, Main Menu/Continue/New Game and minimal Pause implemented. Owner approved one local profile + backup; immediate save of earned wallet/Dish XP/purchases; interrupted days discarded without extra rewards; confirmed New Game resets wallet/XP/ranks and backup; pause blocks gameplay. Editor verification passed; Windows build verification is recorded in IMPLEMENTATION_STATUS.md. Historical milestone reports below retain their original scope. Next numbered work is U0, then U1; recipes/unlocks, the other upgrade effects, full Settings and summary remain planned.

## 0. Source authority and designer synchronization — 2026-10-15

This GDD is the maintained implementation specification, not a replacement for the designer's source. The designer explicitly gave Figma/FigJam higher priority than earlier GDD interpretations on 2026-10-15.

Source: [ClickCook designer board](https://www.figma.com/board/FfbBcNEBFZz9LSIpDAK9mI/Untitled). Reviewed through Chrome Computer Use: ClickCook descriptions, UI wireframes, counter concepts, location/dish examples, upgrade tree, achievements, Journal/recipe content, and FishSim upgrade references. This is a design review, not a new Unity verification or a claim that every reference number has been transcribed.

Resolve sources in this order:

1. Latest explicit designer decisions in this conversation, including approved corrections to historical Figma content.
2. ClickCook-specific Figma descriptions, structured data tables and tree; use developed visual examples for visual direction and wireframes for screen contents/flow.
3. FishSim screenshots as the intended breadth and mechanics/economic-progression reference, adapted to ClickCook rather than dismissed as styling only. Fishing-specific content and literal prices are not automatically ClickCook rules.
4. Earlier GDD interpretations, prototype limitations and developer recommendations. Existing code is evidence of implementation, not design authority.

When a presentation image conflicts with a ClickCook data table, use the structured table for numbers and the image for composition. Thus Dormitory remains documented level 0 / income 0, not the illustrative level 1 / income 5. Previously approved rarity corrections and visible cooking HP remain in force. Do not silently change static assets or verified runtime rules during documentation synchronization.

Status vocabulary: **DESIGN SOURCE** = present in designer Figma; **APPROVED** = explicit designer decision; **ADOPTED DESIGN** = a documented implementation decision under the user's delegation; **PROPOSED BALANCE** = unverified tuning; **IMPLEMENTED/VERIFIED** = separately evidenced Unity behavior. None of the first four implies implementation. Historical milestone paragraphs describe their checkpoint and are superseded by later rules where explicitly stated.

### 0.1 Reconciled conflicts

| Topic | Current specification / decision | Implementation consequence |
|---|---|---|
| Background and order placement | Latest designer instruction plus counter concepts: take paper orders from the actual right-hand 3D board; cook the selected dish on the actual tray/plate. Central floating UI-A3 cards are a previous presentation checkpoint. | UI-A4 was pending at design synchronization; subsequently implemented/verified below in Sections 21/33. Final food art remains unfinished. |
| Broad upgrades | FishSim is also a mechanics/progression reference. The current 16 definitions are not a final content cap. See Section 13.2. | Expand in independently verified slices; no sale of effects that do not work. |
| Double click | APPROVED 2026-10-15: chance of an extra strike on the same dish, not a physical double-click requirement or a second dish/reward. | New future combat effect; existing completed-day and payout identity stay intact. |
| Other choices disappear | ADOPTED DESIGN: hide/disable the other sheets during close-up cooking, without deleting their order instances. This reconciles the original Figma wording with the later approved reservation/Back contract. | No reroll/discard on selection or Back. |
| Limited selection time / timely bonus | DESIGN SOURCE: timed choice and a timely-choice bonus exist in the concept. ADOPTED DESIGN: implement as a bonus window, not order expiry or a hard day timeout; slow selection still works. Exact seconds/reward remain proposed balance. | Add in a separate timing/economy slice; no timer is implemented by this sync. |
| Dish level increases value | DESIGN SOURCE: stronger dish mastery leads to more valuable dishes. Current approved implementation achieves this via level-dependent rarity and payout multipliers. | Do not add a second direct level-price multiplier until the economy specification explicitly defines it. |
| Critics and career reset | DESIGN SOURCE: critics may reset a career if the chosen dish is below level 8; appear after restaurant level 3. APPROVED 2026-10-15: retain the concept, defer exact penalty and activation for separate approval. | No destructive reset is authorized. Exact restaurant-level meaning, encounter rate and retained/lost progress are unresolved. |
| Achievements / Journal | Figma already contains achievement rows/rewards and recipe text; these are not wholly unspecified. See Sections 16–17. | Preserve source content; distinguish missing runtime rules from missing content. |

### 0.2 Synchronization boundaries

- The source-synchronization checkpoint changed documentation only; it did not implement upgrades, critics, timing, recipes, achievements, story or persistence or certify the then-unfinished UI-A4. Later implementation/verification is recorded separately in Sections 21/33.
- Keep approved 5B rarity/XP/payout math and 6A Click Strength behavior until a separately reviewed gameplay milestone changes them.
- FishSim's progression business model means earn → choose an investment → feel its benefit → reach harder content → unlock another branch. This does not approve real-money purchases, advertising, premium currencies or a store monetization model.

## 1. Game Overview

**Game:** ClickCook  
**Genre:** Restaurant/cooking clicker with collection, progression, and upgrade systems.

- [DESIGNED] The player selects a location, begins a work day, selects orders, prepares dishes through close-up clicking, receives a rarity result, earns currency/XP, and progresses through upgrades and locations.
- [IMPLEMENTED] Unity contains static location/dish data and verified Location Selection → Work Day orders → click cooking → rarity/Dish XP/payout result → replacement/day-ending flow (5B), extended by between-day Click Strength purchases (6A). The tenth result ends the day on Continue. Wallet, dish mastery and upgrade rank are run-local; final visuals, full economy, Player XP and persistent progression remain unimplemented.
- [INFERRED] The intended experience combines a persistent restaurant-management loop with focused, tactile dish-completion moments.
- [TBD] Platform, session length, target audience, monetization, and final scope.

### 1.1 Designer story and experience

- [DESIGN SOURCE — FigJam] A young student cook starts with simple food in the Dormitory, improves culinary skills, buys recipes, works in progressively better venues and aims for a Michelin restaurant/star. The intended journey is novice → chef, not merely a sequence of abstract damage upgrades.
- [DESIGN SOURCE] Single-player presentation includes the protagonist's inner thoughts/voice and a culinary diary/book. Background social interactions, thought icons and narrative events are part of the design direction. Their exact scripts, triggers, characters, voice production and localization remain to be specified; do not fabricate completed story content.
- [DESIGN SOURCE] Critics and the possibility of starting a career again belong to the source concept. Section 0.1 records the approved deferral of penalty/reset details; they are not part of current runtime or an implicit prestige mechanic.

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
- [APPROVED — UI-A3 review] Each dish has HP / required click progress; remaining HP and required clicks are now visible during cooking (Section 9).
- [DESIGNED] Completion produces a rarity result.
- [DESIGNED] Daily XP formula: `Daily Earnings / 100 = XP`.
- [TBD] Final work-day transition, timing and failure. Approved prototype lifecycle and baseline rarity calculations are specified in Sections 6–7 and 10–11.

## 3. Complete Game Flow / State Machine

| State | Entry | Player actions | Exit | Known rules |
|---|---|---|---|---|
| Main Menu | Launch / return from play | Continue, New Game, Settings, Exit, Discord | Choose action | [DESIGNED] Menu items exist |
| Location Selection | New/continued session | Browse locations, inspect dishes/progress, Cook | Press Cook | [DESIGNED] Carousel/card UI exists |
| Work Day / Order Board | Start work day | Select an order, Finish Day | Select dish / finish day | [DESIGNED] Currency, remaining dishes, location-dependent capacity up to six visible orders |
| Cooking | Select order | Click dish; potentially use click-related upgrades | Dish completion | [APPROVED] Dish on the real plate, visible HP; close-up with restaurant visible |
| Dish Result | Dish reaches completion | Continue | Return to orders | [DESIGNED] Dish, rarity, value shown |
| End of Day | Finish Day | View prepared dishes/results, Upgrades, Next Day | Upgrade screen / next day | [DESIGNED] Summary wireframe |
| Upgrades | End-of-day action/navigation | Inspect/purchase upgrades | Return | [DESIGNED] Upgrade tree exists |
| Achievements | Navigation | Inspect achievement list | Return | [DESIGNED] Screen exists |
| Journal | Navigation | Browse collection | Return | [DESIGNED] Grid/list screen exists |
| Settings | Main menu/pause | Change settings/reset data | Return | [DESIGNED] Settings categories exist |
| Pause | During gameplay | Resume, Settings, return to Main Menu | Chosen action | [DESIGN SOURCE] Buttons are specified; simulation/save policy remains separate |

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
- [IMPLEMENTED — 8A owner correction2026-10-28] Completed dishes grant Dish XP immediately, while their money stays pending for that day. Finishing before10 dishes forfeits all pending money without changing the previous wallet or Dish XP. The tenth receipt credits the complete day sum once in its autosave transaction; Continue opens the immutable summary without another payment. Menu/scene exit or restarting before10 also discards pending money. Summary distinguishes actual earnings from forfeited amounts. Player XP, calendar advancement and report history remain unimplemented.
- [TBD] Day duration/timer policy, failure states, penalties, other daily reset behavior, date advancement, and earnings formula. No penalties, rewards, or XP are inferred for the prototype.

## 7. Order Board

- [APPROVED DESIGN — latest counter instruction; synchronized 2026-10-15] The Order Board is the physical noticeboard to the player's right in the existing Dormitory environment. Paper sheets are clickable order cards anchored to this scene surface, not a disconnected screen-space grid. The selected dish appears on the existing plate/tray in front of the player. Preserve environment geometry/lighting and the approved order identity/lifecycle. Final sheets need readable art/text, hover/focus feedback and non-overlapping hit areas for 2–6 orders; camera framing/readability must be verified before acceptance.
- [HISTORICAL DESIGN] Early wireframes used a right-side HUD grid; UI-A3 (2026-10-12) implemented large central cards above the plate, a centered pair/row or up to 3×2 cards. These remain historical presentation checkpoints, not the final spatial specification. The independent duplicate orders, location-dependent capacity and immediate selection rules remain current.
- [PROTOTYPE PRESENTATION CHOICE — UI-A3] Green card bars are explicitly labeled Dish mastery and display existing Dish XP / next-level threshold (full at mastery cap), not cooking HP or expiry. Screenshot wallet/progress examples are not new balancing values; final food illustrations remain pending.
- [DESIGNED] It displays `Количество блюд осталось - ..` / dishes remaining.
- [DESIGNED] Current currency is shown above it; example: `215 у.е.`
- [HISTORICAL INFERENCE] Six visible slots were interpreted as a 3×2 grid. The approved rule is a maximum of six simultaneously visible orders, not six orders at every location or a finite six-order day; final grid layout remains a presentation choice.
- [HISTORICAL INFERENCE — superseded] A completed/selected slot might become blank. Selection now reserves the order without removing it; successful completion removes that exact order and permits replacement as specified below.
- [DESIGNED] Orders are selected by the player before cooking.
- [APPROVED — Milestone 4 design] Capacity depends on location: early locations approximately 2–3 simultaneous orders, later locations up to six. Exact per-location capacities are not approved.
- [IMPLEMENTED — 8B, supersedes the unfiltered prototype pool] Generate orders uniformly from distinct valid dish IDs in selected `LocationData.DocumentedAvailableDishes` intersected with owned recipes. Initial and replacement orders use the same filter. Duplicate orders remain independent runtime instances. A zero-owned location cannot start through Cook; its UI offers recipes or another location. No rarity weighting is inferred.
- [APPROVED — selection] Clicking a populated card immediately enters cooking, without confirmation. Only one order may be active. Preserve its runtime identity, dish, slot, and location; selection is not completion and other orders cannot be selected while it is reserved.
- [ADOPTED DESIGN — Figma reconciliation, 2026-10-15] Other choice cards hide or become visibly unavailable during close-up cooking; their runtime orders are retained. Back restores the same board and exact orders. The original Figma phrase about other options disappearing is not interpreted as deleting/regenerating all unselected orders. The source's timely-choice bonus is planned as a separate optional bonus window; no forced selection/expiry is added to the existing prototype.
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
- [APPROVED — Milestone 4B baseline; UI-A3 visibility] Base dish HP equals the documented `ClickRequirement`; each unupgraded discrete click subtracts one HP, and zero completes the selected dish. All documented values are preserved, including 70,000 for the final dish. UI-A3 displays this HP; 6A upgrades affect damage.
- [IMPLEMENTED — 8B] Permanent recipe ownership by stable dish ID, individual purchase at authored RecipeCost, paginated recipe shop and schema17 persistence/migration. Starter Sandwiches is free; existing recipes with positive Dish XP migrate as owned. Dish art, Journal content UI and recipe-price modifiers remain separate work. Base Dish XP awards are approved in Section 10.

## 9. Cooking / Clicking Mechanics

- [APPROVED USER REQUEST — 2026-10-20; IMPLEMENTED / VERIFIED — 2026-10-21] Burgers (Dish ID 6) assemble from the authored Blender ingredient meshes on the actual plate. The bottom bun is initially visible; patty, cheddar, lettuce, tomato and top bun descend in order as accepted cooking damage progresses. Sesame moves with the top bun. Presentation reads `1 − remaining HP / maximum HP`; stronger clicks can cross several visual stages. A short squash/settle gives click feedback. These are visual timings, not extra clicks, ingredients to purchase or a recipe minigame. Completion/rewards use the existing runtime rules; Back resets assembly together with existing HP reset, and Result shows the assembled burger.

- [IMPLEMENTED — 6A] Unupgraded damage remains 1; purchased Click Strength ranks now apply Section 13.1 damage. Earlier 4B statements describe the baseline. Critical, hold and auto-click remain absent.
- [DESIGNED; visibility updated in UI-A3] Select an order, enter close-up cooking presentation, click the dish, reduce visible dish HP, complete dish, reveal rarity.
- [DESIGNED] The restaurant environment remains visible around the cooking presentation.
- [DESIGNED] Relevant upgrade branches: click strength, critical hit, click hold, auto-click strength, auto-click frequency.
- [APPROVED — 2026-10-15] The expanded tree includes a probabilistic extra strike on the current dish. A successful proc is another hit, not another order, completed dish, XP grant or payout. It must not chain recursively or carry unused damage into a replacement. It is not an OS/mouse double-click gesture. Section 13.2 specifies the future design scope; implementation is pending.
- [DESIGNED] Screen 2 shows a central vertical feedback area with example values `67` and `23`, plus flame/impact graphics.
- [HISTORICAL APPROVAL — Milestone 4B; visibility superseded by UI-A3] Without upgrades, hidden HP = `DishData.ClickRequirement`, click damage = 1, completion occurs at zero. The original instruction was not to display HP or a numerical progress counter. Back resets partial progress; reselecting the same order starts at full HP. The prototype uses a large clickable placeholder showing the dish name rather than final models or effects.
- [APPROVED — designer UI-A3 review, 2026-10-12] Cooking now displays dish HP as remaining / documented maximum, a decreasing HP bar and remaining clicks using current Click Strength (`ceil(remaining HP / current click damage)`). This supersedes earlier hidden-HP presentation rules, including the UI-A specification. Maximum HP, damage, Back reset, completion, payout and XP rules are unchanged.
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

- [OWNER APPROVED / IMPLEMENTED — 8C, 2026-10-29] The table contains cumulative thresholds, level10/15000 XP is the cap. Award exact decimal `full-day paid earnings / 100` once on the tenth receipt, retaining four fractional places and clamping actual XP at the cap. Early/abandoned days give0 Player XP, while Dish XP remains separate.
- [OWNER APPROVED / IMPLEMENTED — 8C] Only Dormitory at chef level1, Café at2, then sequentially; Fashion and Michelin both require10. The model and controller enforce level access in addition to recipe ownership. Schema18 stores Player XP;1–17 migrate to0 new XP without inferred retroactive awards. See ClickCook_8C_Player_Progression_Contract.md.

## 13. Upgrade System

Authority: designer ClickCook tree and FishSim reference screenshots under Section 0. The user explicitly requires a similarly broad progression/mechanics system, not only similar UI. [Upgrade Design Proposal](ClickCook_Upgrade_Design_Proposal.md) Section 12 records the expanded adaptation. Its older 16-family balance is a retained draft, except Click Strength adopted/verified in 6A. See [Production Roadmap](ClickCook_Production_Roadmap.md) for dependencies.

### Original designer tree — retained foundation, not a final size limit

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
- [ADOPTED DESIGN — 2026-10-15] Use the existing earned currency for the upgrade economy; between-day purchases extend the verified 6A flow. No second/premium currency, paid respec or monetization is inferred. Tree groups are navigation, not paid empty effects.
- [TBD / PROPOSED BALANCE] Final node-specific prices/ranks/caps, exact prerequisite graph beyond the source links, modifier stacking/rounding and respec. The 6A Click Strength subset remains the only implemented effect; other draft prices are not source-approved balance.

### 13.1 Milestone 6A — Click Strength purchases

- [IMPLEMENTED — 6A, 2026-10-12] Adopt the proposed Click Strength baseline: initial rank 0 / damage 1, maximum rank 15; damage = ceil(1.75^rank). Next-rank price = ceil(20 × 2.20^ownedRank) у.е., using decimal arithmetic. First purchase costs 20 and increases strength to 2. No unlock prerequisites, discounts or refund/respec in this slice.
- [IMPLEMENTED — 6A] Buy only between Work Days through the temporary Upgrades screen on Location Selection. Display wallet, rank/max, current/next strength, price and insufficient-funds/max states. A purchase debits the existing wallet once; a session-bound immutable quote protects against stale rank/price and repeated callbacks. All mid-day purchases are rejected.
- [IMPLEMENTED — 6A; visibility updated in UI-A3] Rank survives day cleanup within the current run and resets on a new run with wallet/mastery. Cooking subtracts the current click strength, clamps HP at zero, and pays/completes once; excess damage never transfers to another order. UI-A3 displays HP; Back reset, approved rarity and XP rules are preserved.
- [PARTIAL — 6A] One UpgradeCatalog ScriptableObject stores 16 uniquely identified definitions with provisional prices/caps; only click_strength has a purchasable runtime effect. Other definitions and their displayed names remain unavailable. Their prerequisites and effects are not implemented or implicitly approved by their catalog presence. The full connected tree/Active Skills Statistics presentation remains unfinished.
- Files: Assets/Scripts/Data/UpgradeCatalog.cs, Assets/Data/UpgradeCatalog.asset, Assets/Scripts/Runtime/PrototypeOrderSession.cs, Assets/Scripts/Runtime/PrototypeDishRewards.cs, Assets/Scripts/Runtime/ClickCookPrototypeController.cs, Assets/Scenes/SampleScene.unity. Repeatable Editor-only verification: Assets/Tests/Editor/Milestone6AVerification.cs.
- [TBD] Critical/hold/auto-click, XP/value modifiers, recipe/upgrade discounts, passive-income rules, choice bonus/time window, persistence and final tree UI. The draft 90-second income quota and 3–6-second bonus window are not implemented.

### 13.2 Broad FishSim-style progression — approved direction, future implementation

- [APPROVED DIRECTION — 2026-10-15] Build a wide, connected tree with meaningful branches, multi-rank investments, later extensions, critical hits, double strikes and automation. The 16 serialized 6A definitions are a checkpoint, not a cap on the final design. Do not mark extra branches implemented because their names exist in data or a mockup.
- [ADOPTED DESIGN] Preserve all 16 existing effect-family IDs. The expanded planning model has 27 effect families below: 16 existing plus 11 extensions. Multiple visually separate tier nodes may contribute ranks to one effect family. These counts do not require 27 assets or a generalized framework and do not authorize changing the runtime catalog in this documentation task.

| Branch | Existing families to retain | Planned extensions / purpose |
|---|---|---|
| Manual cooking | `click_strength`, `critical_chance`, `critical_multiplier`, `click_hold` | `click_multiplier` (multiplies base strength), `double_strike_chance`, `double_strike_power`, `hold_frequency` |
| Automation | `auto_strength`, `auto_frequency` | `auto_strength_multiplier`; separate strength and speed investments, operating only on the selected dish |
| Mastery / XP | `dish_xp`, `restaurant_xp`, `character_xp` | `dish_xp_flat`, `restaurant_xp_flat`, `character_xp_flat`; distinguish flat bonuses from multipliers and the three XP owners |
| Value / economy | `dish_value`, `recipe_discount`, `upgrade_discount`, `passive_unlock`, `passive_value` | `passive_flat`; source-inspired base income and multiplier paths, not only one scalar |
| Choice / rhythm | `selection_bonus`, `selection_window` | Timely-choice reward and a separate increase to its window; not a hard day timer |
| Quality / collection | None in the 6A catalog | `rarity_chance`, `rarity_multiplier`; adapt FishSim's flat/multiplicative rarity progression to the five ClickCook tiers, without copying its single rare-fish percentage |

- [ADOPTED DESIGN] Early purchases must have a working effect immediately. Do not sell an automation opener with zero effective frequency, an XP modifier before that XP system exists, or a passive-income node before accrual works. Keep available / prerequisite-locked / unaffordable / maximum / not-yet-implemented states distinct.
- [ADOPTED DESIGN] Offer parallel manual, automation and value/mastery investments instead of forcing all upgrades into one purchase sequence. Later tiers use explicit prerequisites from data; no invisible level gates. Existing Click Strength rank/cost/damage remain unchanged until a dedicated balance migration milestone.
- [ADOPTED DESIGN] Display selected node name, rank/max, concise description, current → next effect, exact price, explicit prerequisites and unavailable reason. The active-statistics panel summarizes actual combined effects. Hover, focus and selected states must not rely only on line color. Tree zoom/pan is presentation, not a new gameplay system.
- [ADOPTED DESIGN] A manual or hold trigger may produce one extra strike according to its purchased chance. Resolve only against the same still-active dish; if the first hit completes it, suppress the extra hit. Extra hits do not trigger further extra hits. Each applied manual/hold hit may independently crit; automation does not crit or trigger double strikes in the base adaptation. One order still produces at most one completion/reward transaction. Physical rapid clicks remain ordinary independent inputs.
- [PROPOSED BALANCE, not implemented] Reuse the original proposal's critical/hold/auto values as initial candidates, not proven final tuning. Choose new family prices, ranks, caps and extra-strike power only in the relevant implementation/balance slice. Additional rarity modifiers require a separately specified probability transform preserving a 100% distribution; do not mutate the approved baseline table or invent extra tiers.
- [IMPLEMENTATION BOUNDARY] Extended definition/graph validation, tree UI, critical/hold, double strike, auto-click, XP/value, timing/passives and quality modifiers are separate testable slices. See the proposal Section 12 and roadmap U0–U6. No new effect is implemented by this section.

## 14. Economy

- [IMPLEMENTED — 6A] The run-local wallet supports exact Click Strength debits between days (Section 13.1). Other spending and passive income remain unimplemented.
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
- [IMPLEMENTED — 8B] Sandwiches is granted at start; other recipes are bought individually and required before their orders spawn. Ownership is shared across serving locations. The purchase shop is implemented; the later Journal remains separate.
- [OWNER DECISION — 2026-10-28, 8B] Retain the current Unity Michelin pool: only dishes29–32. The broader source interpretations are not used by runtime. LocationData is authoritative for all11 serving pools; reverse dish→location lists remain reference data.

## 16. Achievements

- [DESIGNED] An achievements screen exists with five visible rows.
- [DESIGNED] Each row has icon, title/main text, and right-side reward/value area.
- [DESIGNED] It shares the Active Skills Statistics panel.
- [DESIGN SOURCE — FigJam, synchronized 2026-10-15] The designer supplied a table of milestone labels and effect rewards. Preserve the rows below; do not treat all achievement content as absent. Not implemented. Names/notation below retain source terminology; `+0.5` is not silently converted into percentage points, and `quality` is not silently mapped to a particular rarity.

| Source milestone label | Source reward notation |
|---|---|
| Общежитие | Сила клика +1 |
| Кафе | Умнож. ценность блюда ×1.05 |
| Закусочная | Повыш. опыт за блюдо +3 |
| Фудкорт | Повыш. шанс крита +0.5 |
| Шашлычная | Повыш. автоподсечки +1 |
| Домашний ресторан | Повыш. шанс качества +0.02 |
| Итальянский ресторан | Умнож. множитель клика ×1.1 |
| Кондитерская | Умнож. ценность блюда ×1.1 |
| Японский ресторан | Сила клика +1.2 |
| Фешн ресторан | Повыш. опыт за блюдо +5 |
| Мишленовский ресторан | Повыш. шанс качества +0.5 |
| Количество блюд 10 | Сила клика +1 |
| Количество блюд 50 | Сила клика +2 |
| Количество блюд 250 | Умнож. множитель клика ×1.1 |
| Количество блюд 700 | Умнож. множитель клика ×1.5 |
| Превосходное качество 10 | Повыш. шанс качества +0.2 |
| Превосходное качество 50 | Умнож. ценность блюда ×1.05 |
| Превосходное качество 150 | Умнож. множитель качества ×1.15 |
| Заработано денег 1000 | Сила клика +1 |
| Заработано денег 100000 | Умнож. чаевых ×1.1 |
| Заработано денег 10000000 | Умнож. чаевых ×1.5 |
| Заработано денег 1000000000 | Умнож. ценность блюда ×1.7 |
| Получено опыта 100 | Повыш. опыт за блюдо +3 |
| Получено опыта 700 | Умнож. опыт за блюдо ×1.05 |
| Получено опыта 4200 | Умнож. опыт за блюдо ×1.2 |

- [ADOPTED DESIGN] Treat the table as achievement content, not store purchases. Show permanent earned bonuses separately from purchased ranks; granting a bonus must be idempotent. The fishing word `автоподсечки` needs a ClickCook effect mapping, not a new fishing subsystem.
- [TBD BEFORE IMPLEMENTATION] Exact trigger for venue-named milestones (unlock, first shift or mastery); whether prepared/earnings/XP counters are career totals; canonical meaning of Превосходное качество/quality/tips; units and stacking of chance bonuses; fractional strength rounding; automatic versus claimed rewards; notifications and persistent recovery. Source values remain intact until these semantics are resolved.

## 17. Journal

- [DESIGNED] Journal screen exists as a collection grid/list.
- [DESIGNED] It shares top navigation with Locations, Upgrades, and Achievements.
- [DESIGN SOURCE — FigJam] The detail panel has dish illustration/name, dish level and progress, prepared count, price, five colored indicators and recipe content. The board includes a dish-index table plus authored ingredient/method texts. Recipe writing therefore has an existing source; it is not wholly TBD. Preserve source wording/content when importing it in a later content milestone; this sync does not claim a word-for-word import or culinary validation of every recipe.
- [ADOPTED DESIGN] Distinguish recipe purchase price from sale value, dish mastery from cooking HP, and authored recipe instructions from interactive cooking actions. Real-world ingredient/preparation steps do not automatically become runtime minigames.
- [TBD] Entry visibility/ownership rules, meaning/data of the five indicators, canonical display price, missing/source image assets, sorting/filtering, collection counters and persistence.

## 18. Settings

- [DESIGNED] Language selection.
- [DESIGNED] Display/window controls.
- [DESIGNED] Volume controls.
- [DESIGN SOURCE — wireframe] Display controls include resolution, fullscreen/window mode and FPS. Separate sliders are music, environment and effects volume. Examples 1920×1080 / 60 are mockup values, not a mandatory hardware/default policy.
- [DESIGNED] Reset data action.
- [TBD] Release languages, defaults/supported display choices, FPS options, audio routing, reset confirmation and storage behavior. The source audio categories above are known; implementation is not complete.

## 19. Pause

- [DESIGNED] A pause screen exists.
- [DESIGN SOURCE — wireframe] Actions: Resume, Settings, return to Main Menu; retain the version/Discord footer where appropriate. Do not leave the menu's actual button list unspecified.
- [APPROVED / IMPLEMENTED — 7B, 2026-10-22] Minimal Pause: Resume, return to Main Menu, Save and Exit; Esc or Pause button opens it, losing focus also pauses. Time scale becomes zero and controller/session guards block cooking, selection, Back, purchases and result transitions. Resume restores the previous time scale and focus. Main Menu discards unfinished orders but retains already issued rewards. Full Settings/version/Discord footer remain planned; no passive-income or automation semantics are invented.

## 20. Save / Load / New Game / Continue

- [DESIGNED] Main menu contains Continue and New Game.
- [DESIGNED] Settings contains Reset Data.
- [IMPLEMENTED / VERIFIED — 7A, 2026-10-15] One in-memory `PlayerProfile` owns the existing rewards wallet/Dish XP and upgrade ranks. `PlayerProfileRuntime` owns it for the run, independently of screen/scene instances. `PrototypeOrderSession` owns only temporary day/orders/results and references that profile. Ending a day or reloading SampleScene clears unfinished orders, not earned profile progress. Only one day per profile can be active; existing purchase/receipt guards remain intact.
- [IMPLEMENTED / VERIFIED — 7A] `PlayerProfileSnapshot` captures detached read-only decimal wallet, XP by existing numeric `DishData.DishId`, and ranks by existing `UpgradeDefinition.Id`. No Unity object/order/session references are included. The existing unique documented location levels 0–10 remain catalog lookup keys, not player-level unlock rules; there is no location progress to snapshot yet. Keep authored IDs stable; renaming display labels must not renumber identity. No static assets were changed.
- [APPROVED / IMPLEMENTED — 7B, 2026-10-22] One local profile: exact decimal wallet (invariant string), XP by current dish IDs 1–32 and implemented Click Strength rank 0–15. Save immediately after one completed receipt or purchase, retry/flush on menu/focus/quit. Orders, partial HP, reservations and pending result UI are excluded. Restart loads progress into the 7A owner and opens Main Menu; Continue never reissues a receipt or resumes an unfinished day.
- [IMPLEMENTED — 7B] Version-1 JSON envelope, SHA-256 integrity checksum, validated payload/IDs/ranges, flushed temporary file and atomic primary replacement with previous valid backup. Corrupt/missing primary can recover backup with a visible notice. Both copies invalid/unreadable or a newer schema disable Continue and preserve files; New Game requires explicit confirmation. Failed writes retain in-memory progress, show a retry notice and block menu Save and Exit until saving succeeds. New Game resets both primary and backup; it does not reset unimplemented progression systems.
- [STORAGE — 7B] Player: `Application.persistentDataPath/profile.json`, `.bak` backup; Editor: Git-ignored project `UserSettings/ClickCookProfile/profile.json` (survives Editor restarts, isolated from Player saves). Verification uses separate GUID folders in `Temp/ClickCookSaveTests`. Local only; no PlayerPrefs/cloud/multiple slots. Newer schemas are protected, not silently migrated; add a migration when a new schema actually exists. Full Settings Reset Data control remains planned; confirmed New Game currently provides the implemented reset action.

## 21. UI/UX

- [IMPLEMENTED / VERIFIED — UI-A4, 2026-10-15] Reused the six order buttons as world-space sheets on the actual Dormitory noticeboard. Selection shows a collider-clickable 3D sketch on the existing plate/tray, with visible HP and scene-camera close-up. Other sheets hide during cooking/result; Back restores the same board and resets partial HP through the existing controller. Camera FOV/parallax mode restores on return; pointer-follow is bounded outside the window. No order/reward/balance formula changed.
- [IMPLEMENTED / VERIFIED — UI-A4 input] The dish target also accepts EventSystem Submit and explicit arrow navigation to/from Back, with a visible input/temporary-art hint. Physical mouse verified carousel wrapping, Cook, sheet selection, HP 8→7, Back/reselect 8/8, eight-click completion, one 8.00 payout/+10 XP, Continue/same replacement, early Finish, fresh identities on a new day, and slot 6 selection. Physical keyboard and standalone build were NOT retested; Submit/move checks are EventSystem callbacks, not physical-key claims.
- [VERIFIED — UI-A4] Compilation passed with no C# errors; 4,685 existing model, 237 Play Mode, 240 presentation and 10 edge assertions passed. Rendered-frame checks across all 11 locations at 1280×720, 1920×1080, 2560×1440, 1920×1200 and 2560×1080 passed 880 lifecycle/input and 69,391 spatial/text/glyph/raycast assertions. Temporary resolution preset restored. Existing scene blocks removed: zero; Camera only gained a PhysicsRaycaster, authored camera pose/environment/lighting and static dish/location/catalog assets retained. Only pre-existing UVCS configuration errors remained.
- [PARTIAL PRESENTATION — UI-A4] Sandwich and mug-cake sketches use simple primitives; other dishes use a clearly labeled neutral 3D sketch. World sheets retain temporary illustrations. This is the scene-interaction prototype, not 32 final dish models, final typography/art, other location environments, animation or production UX certification. Files: `Assets/Scripts/UI/CounterScenePresentation.cs`, `Assets/Scripts/UI/CounterDishPointer.cs`, `Assets/Scripts/Editor/UiA4Setup.cs`, `Assets/Tests/Editor/CounterSceneVerification.cs`, `Assets/Scripts/Camera_Move.cs`, existing controller, `Assets/Scenes/SampleScene.unity`, `Assets/Art/Prototype/Materials/`.

- [APPROVED DESIGN PRIORITY — 2026-10-15] Follow the designer's developed Figma visuals and ClickCook descriptions before earlier GDD/developer presentation assumptions. Blue food-pattern/paper/orange styling applies to meta screens; the real 3D counter is the gameplay surface. Use a handwritten accent for short dish titles, readable body/numeric text, and genuine scene-bound sheets/dish targets. Current runtime labels/placeholders are not final art. See Section 0 for conflicts and [UI/UX Specification](ClickCook_UI_UX_Specification.md).

- [IMPLEMENTED / VERIFIED — designer UI-A3 review, 2026-10-12] Current prototype Location Selection menu includes Exit Game. Standalone code calls `Application.Quit`; Editor code stops Play Mode without closing Unity. Native Editor Exit was verified; standalone quit/build is NOT VERIFIED. This does not implement the full Main Menu/Continue/save system.
- [IMPLEMENTED / VERIFIED — UI-A3] Central large dish cards replace the earlier right-hand board presentation, with labeled real mastery bars; capacities/duplicates/lifecycle unchanged. Cooking now shows real remaining/max HP, decreasing bar and remaining clicks at current strength. Compilation, existing 237 regression and 240 presentation assertions, 30 new HP/card/Exit-guard assertions and 980 rendered layout/input checks passed. Native mouse verified selection, HP 5→4, Back/reselect reset and Exit stopping Play Mode. Final food art remains temporary.

- [IMPLEMENTED — UI-A1, 2026-10-12] Location Selection and Click Strength purchase have a blue/pergament/orange uGUI presentation, Ukrainian display copy, scalable panel/button assets, visible focus outlines and reusable component prefabs. Authored IDs, numeric data and gameplay rules are unchanged. The venue image is an explicitly labeled stylized placeholder. See [UI/UX Specification](ClickCook_UI_UX_Specification.md).
- [VERIFIED — UI-A1] C# compilation, 237 current-loop Play Mode assertions, glyph/contrast checks, and layout/raycast/focus at five rendered Canvas sizes passed through Unity MCP. Native mouse checks through Computer Use passed for carousel wrapping, upgrades/Back, disabled purchase, successful purchase, Cook and early Finish Day. Fixed focus loss after a consumed purchase offer; native retest confirmed focus returns to Back when the next offer is unaffordable. The user confirmed physical Enter and arrow navigation work in Game View. Computer Use synthetic keys did not trigger runtime Input System actions; that automation limitation remains recorded. Final art and the remaining production screens are unfinished.
- [IMPLEMENTED / VERIFIED — UI-A2, 2026-10-12] Work Day has separate wallet/completed-day/order-capacity displays and up to six paper order cards on the right. Cooking has a large clickable plate sketch, selected dish/location, Back reset warning and no hidden-HP display. Dish Result shows Ukrainian rarity labels with an additional color cue, exact receipt payout/wallet/XP/levels, and a labeled bar of actual Dish XP (full at mastery cap). Tenth-result CTA uses the existing Continue action to end the day. Candidate diagnostics stay in a hidden Editor-inspectable label; no pool or order rule changed. Shared sprites/style and explicit focus navigation reuse UI-A1; all dish illustrations remain labeled temporary sketches.
- [VERIFIED — UI-A2] Successful C# compilation, 237 existing regression assertions, 240 presentation/lifecycle assertions, ten empty-board/mastery-cap fixture checks and 795 glyph/layout/raycast/focus checks across five actual Canvas resolutions. Computer Use native mouse verified Cook, selection, Back identity preservation, five-click sandwich completion, exact result, Continue without repeated payout and early Finish. Keyboard focus paths were exercised through EventSystem; a new physical-keyboard pass over these screens and a production build were not executed.
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

- [IMPLEMENTED / VERIFIED — environment recovery, 2026-10-21] Retained the user's updated Dormitory FBX, prefab, materials and authored placement/camera. Rebound the two imported board-paper renderers and Plate after GUID replacement; board alignment now derives from the paper mesh plane rather than assuming FBX axes. Removed 152 unresolved old-import overrides while preserving all 39 environment transforms and 17 live overrides. The saved scene was reopened successfully. A pre-repair scene/environment copy is retained locally under `ArtSource/Recovery/Dormitory_BeforeRepair_2026-10-21/`.
- [IMPLEMENTED / VERIFIED — burger presentation] `Assets/Models/Food/Burger/BurgerAssembly.prefab` reuses the existing 15-mesh, 1,306-triangle FBX and 512×512 color atlas through a URP material; six ingredient groups include sesame in the top group. `BurgerAssemblyPresentation.cs` drives assembly from real HP progress. Counter presentation and input keep the existing order/session/profile behavior. The burger name panel is compact and left-aligned during its close-up to clear the ingredients. Other dishes retain their current sketches; this does not complete all dish art or animations.

- [DESIGNED] FigJam includes areas for Locations, Dishes, Characters, and `3D модели и анимации`.
- [DESIGNED] Food/reference imagery and external visual references exist.
- [APPROVED DESIGN — latest counter instruction, synchronized 2026-10-15] Retain and interact with the existing Dormitory: right-hand noticeboard for order sheets, actual tray/plate for cooking. The background must not remain a purely decorative image behind floating gameplay panels. Existing counter concepts support this spatial relationship. Final food models, transitions and other venues remain separate content work.
- [IMPLEMENTED — updated environment retained 2026-10-21] Unity contains the user's replacement `Dormitory.fbx`/variant prefab with 38 renderers and four mapped URP materials. The physical board, tray, plate and environment props remain in the scene. There are no colliders on this updated environment; the cooking target retains its own BoxCollider/PhysicsRaycaster input.
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
| Cooking | Dish, visible HP / remaining clicks, upgrades | Click to progress | Future modifiers |
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

7A implementation subset: `PlayerProfile`/`PlayerProfileRuntime`/`PlayerProfileSnapshot` under `Assets/Scripts/Runtime/`. Only wallet, Dish XP and the already playable Click Strength rank exist; the other conceptual Player fields above are still unimplemented. The existing reward implementation remains the profile-owned subcomponent, not a second wallet or second XP store.

## 28. Unity Implementation Mapping

This is a design-to-technical mapping, not existing implementation.

| System | Current Unity state | Likely Unity representation | Notes |
|---|---|---|---|
| Game flow | Partially implemented: Location Selection → board → base cooking → rarity/payout/XP result → board or day end | Existing prototype controller/session/rewards | Final summary, visuals and persistent progression remain unimplemented |
| Locations | Partially implemented: 11 static data assets, ordered catalog, placeholder carousel | Data assets/serializable location definitions | Unlocks, ownership, progress, and income runtime remain TBD/unimplemented |
| Dishes/recipes | 32 static dishes plus recipe ownership, purchase shop, order filtering and schema17 saves | Existing DishData plus RecipeCatalog/profile/session | Final dish art, Journal and recipe discounts remain unfinished |
| Orders | Partially implemented: random instances, reservation, completion/replacement, daily cap and cleanup (4B) | PrototypeOrder / PrototypeOrderSession | Final capacities and advanced eligibility remain TBD |
| Cooking | Partially implemented: base HP, 6A Click Strength, visible HP and UI-A4 collider-clickable 3D sketch on the existing plate | Existing controller/session and scene-only CounterScenePresentation/CounterDishPointer | Final art, effects and other click modifiers remain absent |
| Rarity | Baseline levels 1–18 implemented (5B) | PrototypeDishRewards / approved table | Future modifiers and final reveal presentation |
| Economy | Partially implemented: exact run-local wallet/payouts (5B), Click Strength purchases (6A) | PrototypeDishRewards / PrototypeOrderSession | Other spending, passive income and persistence |
| XP | Partially implemented: run-local Dish XP/mastery (5B) | PrototypeDishRewards | Player/restaurant XP, upgrades and persistence |
| Upgrades | Partially implemented: 16 static definitions; run-local Click Strength purchases and temporary UI (6A) | UpgradeCatalog and existing session/controller | Remaining effects, prerequisites and final tree UI unfinished |
| Save/load | Not implemented | Persistence service | Format/slots TBD |
| UI | Partially implemented: temporary uGUI Location Selection, Work Day, base cooking and rarity/XP/payout Dish Result screens | Existing Canvas/prototype controller | Final UI and remaining screens are unimplemented |
| Environment | Dormitory retained; spatial UI-A4 verified | Scene-bound board and tray/plate presentation | Other venues/final assets remain unfinished |
| Camera | One camera with cursor-follow script | Gameplay presentation camera | Existing script may be reusable |
| Input | Input System asset, direct mouse read | Central input bindings | Current asset unwired |
| Visual effects | URP/Global Volume | Presentation/post-processing profile | Must be validated against Figma |

## 29. Current Unity Project State

- [IMPLEMENTED] Unity `6000.3.14f1` / Unity 6.3 LTS.
- [IMPLEMENTED] URP `17.3.0`.
- [IMPLEMENTED] Product: ClickCook.
- [IMPLEMENTED] One enabled scene: `Assets/Scenes/SampleScene.unity`.
- [IMPLEMENTED] Scene roots: Directional Light, Dormitory, Global Volume, Camera, Directional Light (1), and `ClickCook_Prototype`.
- [IMPLEMENTED] Authored runtime scripts: `Camera_Move.cs`, `DishData.cs`, `LocationData.cs`, `LocationCatalog.cs`, `ClickCookPrototypeController.cs`, `PrototypeOrder.cs`, `PrototypeOrderSession.cs`, `PrototypeDishRewards.cs`, `UpgradeCatalog.cs` (6A), and scene-only `CounterScenePresentation.cs` / `CounterDishPointer.cs` (UI-A4).
- [IMPLEMENTED] Camera position approximately `(1.138, 1.184, -2.718)`, 60° perspective FOV, slight mouse-driven rotation.
- [IMPLEMENTED] Updated Dormitory FBX/variant prefab with four mapped URP materials; geometry, textures, authored placement and camera retained during the 2026-10-21 counter repair.
- [IMPLEMENTED] Global post-processing includes bloom, film grain, vignette, chromatic aberration, and color grading.
- [IMPLEMENTED] Static data contains 32 `DishData` assets, 11 `LocationData` assets, and one ordered `LocationCatalog` asset.
- [IMPLEMENTED] `SampleScene` contains a temporary uGUI Canvas/EventSystem and verified Location Selection → board → base click cooking → rarity/Dish XP/payout result / day end flow (5B), with Back resetting partial progress and Continue preserving replacement orders without duplicate rewards.
- [PARTIALLY IMPLEMENTED] Burger now has an assembly presentation prefab/animation; other dish art, audio, save/load, final result presentation, complete economy and Player XP/progression remain unfinished. Click Strength is the first implemented upgrade (6A); other upgrade effects remain absent.
- [APPROVED DESIGN] Retain the current Dormitory environment and adapt the gameplay presentation to its board/tray/plate; do not replace it. Additional location environments remain a separate content scope.

## 30. Contradictions and Design Risks

- [HISTORICAL — 5B report corrected by 2026-10-12 inspection/6A verification] The earlier report described an empty Dormitory pool. Current 00_Dormitory.asset contains valid Sandwiches and Mug cakes references, confirmed through Unity MCP and the 6A Play Mode flow. No static data was changed to achieve this. Empty-pool handling remains supported and tested using transient fixtures; recipe ownership/eligibility is still TBD.

1. **Rarity naming**
   - [DESIGNED] User context gives Common, Uncommon, Rare, Fantastic, Legendary.
   - [F] Result wireframe says `Обычное`.
   - [TBD] Confirm canonical localization and whether `Обычное` equals Common.

2. **Visible versus hidden cooking values**
   - [HISTORICAL DESIGN — superseded by UI-A3] Hidden HP/click progress was intended. The designer now requires visible HP and remaining clicks (Section 9).
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
   - [APPROVED DESIGN — 2026-10-15 synchronization] Retain Dormitory and make its board/plate interactive; other environments and production art remain unimplemented.

6. **Click requirement semantics**
   - [F] Dish data exposes “number of clicks.”
   - [DESIGNED] User context describes hidden dish HP.
   - [APPROVED — Milestone 4B] Values are base hidden HP with one point of damage per click in the unupgraded prototype. Future upgrade effects remain TBD.

## 31. Complete TBD / Open Decisions List

### Critical

- Work-day duration/timer, failure, order timeout, final capacity mapping, and future daily-limit progression. Baseline limit, early finish/discard, generation, selection, completion replacement, and automatic end transition are approved in Sections 6–7 but design approval alone does not establish implementation.
- Future modifiers beyond implemented 6A Click Strength and the adopted extra-strike contract. Base HP/damage and visible progress are specified in Sections 8–9.
- Critical, click-hold, and auto-click formulas.
- Baseline rarity probabilities, dish-XP relationship and payout multipliers are approved/implemented in 5B; future modifiers, persistent progression and localized terminology remain open.
- Recipe purchase/unlock rules and location unlock rules.
- Exact prices/ranks/caps/prerequisite graph beyond implemented 6A, plus probability/rounding contracts for the expanded Section 13.2 tree. The source-backed branch scope is no longer wholly TBD.
- Save/load, New Game, Continue, reset-data behavior.
- Future economic modifiers/order and spending rules; base payout timing is approved/implemented in 5B.
- Additional environmental/3D content scope; Dormitory retention and spatial board/plate use are decided.

### Important

- Character, restaurant, and dish XP gain/thresholds/rewards.
- Player-level interpretation and benefits.
- Achievement trigger/reward semantics and persistence; source rows/numeric reward notation exist in Section 16.
- Journal unlock/indicator rules and asset completeness; source recipe texts/detail-panel contents exist in Section 17.
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
4. Complete the retained Dormitory's spatial board/plate presentation and verify it before extrapolating to other locations.
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

### Designer synchronization checkpoint — 2026-10-15

- Documentation only: Section 0 establishes Figma-first authority and resolves source conflicts; Sections 1, 7, 9, 13, 16–19 and 21–23 restore designer scope. User decisions: probabilistic extra strike; defer critics' penalty/reset policy. No new Unity verification is claimed.
- [x] UI-A4 — world-space order sheets on the real board, collider/Submit dish target on the existing plate, visible HP, camera return and same-order Back. Completed and verified after source synchronization on 2026-10-15; evidence/files/remaining art and physical-keyboard/build limitations are recorded in Section 21. No new upgrade effects were added.
- [x] U1 upgrade tree presentation — 92 manifest tiles / 38 effect families plus root, details, actual statistics, pan/zoom and mouse/EventSystem keyboard navigation. Only node.click_strength.01 is purchasable; remaining effects and versioned node-state migration are pending U2–U6.
- [ ] Critical/hold, extra-strike, automation, additional economy/XP/quality modifiers, achievements, recipe Journal and narrative/critic systems remain unimplemented. Design-source completeness is not implementation completion.

### Verified foundations

- [x] Static data architecture for locations and dishes, preserving documented names, numeric values, and explicit availability relationships. — **Milestone 1**
- [x] All 11 documented `LocationData` assets exist. — **Milestone 1**
- [x] All 32 documented `DishData` assets exist. — **Milestone 1**
- [x] A read-only `LocationCatalog` references exactly the 11 locations in documented level order 0–10 and supports lookup by documented level. — **Milestone 2A**
- [x] The Dormitory environment, lighting, camera, URP volume, Input System presence, and `CameraCursorFollow` baseline remain available. — **Existing project baseline, retained through Milestone 2B**

### Game flow and UI

- [ ] Partially implemented — The core loop covers Main Menu/Continue → Location Selection → board → click cooking → rarity/Dish XP/payout → day end, between-day Click Strength purchases and local persistence. Final summary, remaining upgrade effects and date-based next-day progression remain unfinished. — **Milestones 2B–7B**
- [x] Minimal functional Main Menu: valid-profile Continue, confirmed New Game and Exit; backup recovery/error states. Full Settings/version/Discord presentation remains planned. — **7B, 2026-10-22**
- [ ] Partially implemented — Location Selection has a verified temporary uGUI carousel with one current location, Previous/Next wraparound across all 11 locations, name, documented level, placeholder progress, documented dish count, passive income/sec, and `Cook`. Unlock/ownership rules, real progress, dish-slot contents, and final visual design remain unimplemented. — **Milestone 2B**
- [ ] Partially implemented — Cook starts a fresh in-memory day/board; completion, replacement, ten-dish limit, run-local payouts/XP and cleanup work. Dates and final day summary remain unimplemented. — **Milestones 2B–5B**
- [x] Work Day displays the selected location's complete `DocumentedAvailableDishes` list as read-only documented candidates, with count, dish IDs, and English names in stored order. This is not an order board or an eligibility rule. — **Milestone 3**, supporting Sections 2, 5, and 8; files: `Assets/Scripts/Runtime/ClickCookPrototypeController.cs`, `Assets/Scenes/SampleScene.unity`
- [x] Manual Finish Day clears unfinished orders and temporary day state, returns to Location Selection, and displays a day-finished message; since 5B, ten completions trigger cleanup on result Continue. Earned run-local wallet/Dish XP survive day cleanup. No Finish Day action during cooking/results. — **Milestones 4B/5B**; files: `Assets/Scripts/Runtime/ClickCookPrototypeController.cs`, `Assets/Scripts/Runtime/PrototypeOrderSession.cs`, `Assets/Scripts/Runtime/PrototypeDishRewards.cs`, `Assets/Scenes/SampleScene.unity`
- [x] Initial Order Board population: location-configured capacity capped at six, uniform random documented candidates, independent duplicate order identities, empty-state handling and separate available/capacity/completed counters. 5B replaces the original unimplemented currency label with a real run-local wallet. — **Milestones 4A/5B**; files: `Assets/Scripts/Runtime/PrototypeOrder.cs`, `Assets/Scripts/Runtime/PrototypeOrderSession.cs`, `Assets/Scripts/Runtime/ClickCookPrototypeController.cs`, `Assets/Scenes/SampleScene.unity`
- [x] Single-order reservation and selected-order/dish/location handoff; Back preserves the board without completion/regeneration and resets partial cooking progress. — **Milestones 4A–4B**, same files as initial board population.
- [x] Base cooking: HP equals `ClickRequirement`, one damage per unupgraded discrete dish-button click, exact-instance completion at zero, replacement in the same slot before the ten-dish cap, and no replacement at the cap. HP was hidden at 4B and became visible at UI-A3; 6A applies purchased strength. — **Milestones 4B/6A/UI-A3**; files: `Assets/Scripts/Runtime/PrototypeOrder.cs`, `Assets/Scripts/Runtime/PrototypeOrderSession.cs`, `Assets/Scripts/Runtime/ClickCookPrototypeController.cs`, `Assets/Scenes/SampleScene.unity`
- [ ] Partially implemented — Order Board/basic day lifecycle works; final balancing, eligibility, economy, expiry rules and presentation remain unfinished. — **Milestones 4A–4B**
- [ ] Partially implemented — Basic click cooking works on a large temporary dish placeholder; final close-up dish art, effects, critical/hold/auto-click upgrades and final input support remain unfinished. — **Milestone 4B**
- [x] Dish Result foundation (5A) is extended in 5B: exact dish identity/name, resolved rarity, earned payout/wallet, Dish XP gain and old/new level/progress; Continue preserves the replacement board without repeat rewards. All ten dishes show results; tenth Continue ends the day. Stale text clears on Continue/end/new day. — **Milestones 5A–5B**; files: `Assets/Scripts/Runtime/ClickCookPrototypeController.cs`, `Assets/Scripts/Runtime/PrototypeDishRewards.cs`, `Assets/Scripts/Runtime/PrototypeOrderSession.cs`, `Assets/Scenes/SampleScene.unity`
- [ ] Partially implemented — Dish Result has styled uGUI receipt, rarity copy and real mastery display (UI-A2); final reveal art, audiovisual feedback and animations remain unimplemented. — **Milestones 5A–5B / UI-A2**
- [ ] End-of-Day Summary is not implemented.
- [ ] Partially implemented — UI-A1/A2 style and responsive checks cover the five existing gameplay/purchase screens; final art, production menus/remaining screens, transitions and animations are unfinished.
- [x] UI-A1 Location Selection/Click Strength visual and text layer is implemented and verified: automated/native mouse checks passed, including purchase focus recovery; physical Enter/arrows verified by the user. — **UI-A1 (2026-10-12)**. Files: `Assets/Scripts/UI/ClickCookUiCopy.cs`, `Assets/Scripts/UI/UiFocusOutline.cs`, `Assets/Scripts/Editor/UiA1Setup.cs`, `Assets/UI/Art/`, `Assets/UI/Prefabs/Components/`, `Assets/Scripts/Runtime/ClickCookPrototypeController.cs`, `Assets/Scenes/SampleScene.unity`; verification: `Assets/Tests/Editor/UiA1Verification.cs`, `Assets/Tests/Editor/Milestone6AVerification.cs`. Remaining screens retain their prototype UI; full UI/UX remains partial.
- [x] Work Day HUD/order cards, Cooking and Dish Result presentation with real XP bar and focus handoff; existing lifecycle retained. — **UI-A2 (2026-10-12)**. Files: `Assets/Scripts/Editor/UiA2Setup.cs`, `Assets/Scripts/Runtime/ClickCookPrototypeController.cs`, `Assets/Scripts/UI/ClickCookUiCopy.cs`, `Assets/UI/Art/PlateCircle.png`, `Assets/UI/Prefabs/Components/OrderCard.prefab`, `Assets/Scenes/SampleScene.unity`; verification: `Assets/Tests/Editor/UiA2Verification.cs`. Temporary art and full UI/UX remain partial.
- [x] Designer review corrections: Exit in current prototype menu (Editor tested), visible cooking HP/current-strength click count, central 2–6 dish cards with labeled real mastery. — **UI-A3 (2026-10-12)**. Files: `Assets/Scripts/Runtime/ClickCookPrototypeController.cs`, `Assets/Scripts/Editor/UiA3Setup.cs`, `Assets/Scenes/SampleScene.unity`, `Assets/UI/Prefabs/Components/OrderCard.prefab`; verification: `Assets/Tests/Editor/UiA3Verification.cs`, `Assets/Tests/Editor/UiA1Verification.cs`, existing UI-A2/6A checks. Standalone quit and final art remain unverified/unfinished.

### Runtime systems

- [x] Burger model/HP-driven ingredient assembly, click squash, same-order Back reset and assembled Result; repaired counter bindings/orientation in the user's updated Dormitory. — **Scoped presentation fix, 2026-10-21**, outside the numbered roadmap. Files: `Assets/Scripts/UI/BurgerAssemblyPresentation.cs`, existing `CounterScenePresentation.cs` / `CounterDishPointer.cs`, `Assets/Scripts/Editor/BurgerAssemblySetup.cs`, `Assets/Models/Food/Burger/`, updated Dormitory assets and `Assets/Scenes/SampleScene.unity`. Verification: `Assets/Tests/Editor/BurgerAssemblyVerification.cs`, existing CounterScene/6A/profile/UI-A2 helpers. Remaining dish models and broad animation/audio work stay pending; 7B persistence is completed separately.

- [ ] Partially implemented — Location and dish design data are available at runtime, but location unlocking/ownership, passive-income accrual, recipe ownership/gating, and order eligibility rules are not implemented. — **Milestones 1 and 2A**
- [x] Baseline rarity resolved by the approved dish-level table (1–18), before awarding Dish XP; independently tracked dish mastery, carry-over XP and level-18 cap. — **Milestone 5B**; file: `Assets/Scripts/Runtime/PrototypeDishRewards.cs`
- [x] Exact decimal payouts once per successful completion into a zero-starting run-local wallet; wallet/Dish XP survive day changes, not game restart. — **Milestone 5B**; files: `Assets/Scripts/Runtime/PrototypeDishRewards.cs`, `Assets/Scripts/Runtime/PrototypeOrderSession.cs`
- [ ] Partially implemented — Economy has persisted payouts/wallet and Click Strength purchases; other spending and passive income remain unfinished. — **Milestones 5B–7B**
- [ ] Partially implemented — Dish XP/mastery works in memory; Player/restaurant XP, XP upgrade effects and persistent progression remain unimplemented. — **Milestone 5B**
- [ ] Recipe and location unlock systems are not implemented.
- [x] Click Strength purchase/damage slice: ranks 0–15, exact decimal prices/debit, between-day UI, session-bound quote and duplicate-callback guards, cross-day retention and fresh-run reset. — **Milestone 6A**; files: `Assets/Scripts/Data/UpgradeCatalog.cs`, `Assets/Data/UpgradeCatalog.asset`, existing session/rewards/controller, `Assets/Scenes/SampleScene.unity`; checks: `Assets/Tests/Editor/Milestone6AVerification.cs`.
- [ ] Partially implemented — 16 upgrade definitions and Click Strength work; remaining 15 effects, prerequisites, connected tree/statistics UI and persistent ownership are unfinished. — **Milestone 6A**
- [x] Local save/load, Continue and confirmed New Game/reset of wallet/Dish XP/ranks plus backup. Atomic writes, corruption recovery, newer-version protection and failure/retry verified in Editor; native Windows earn→purchase→quit→restart→Continue passed. Full Settings Reset Data control remains planned. — **7B, 2026-10-22**
- [x] Run-owned in-memory player profile, stable authored IDs and detached read-only snapshot; wallet/Dish XP/Click Strength ownership survive day and SampleScene/controller replacement, while temporary orders do not. Cross-session active-day and stale-purchase guards verified; at the historical 7A checkpoint a fresh Play started empty. — **Milestone 7A (2026-10-15)**. Files: `Assets/Scripts/Runtime/PlayerProfile.cs`, `PlayerProfileRuntime.cs`, `PlayerProfileSnapshot.cs`, existing `PrototypeOrderSession.cs`, `PrototypeDishRewards.cs`, `ClickCookPrototypeController.cs`; checks: `Assets/Tests/Editor/PlayerProfileVerification.cs`, existing 6A/UI-A2/CounterScene verification. 7B adds disk persistence; the rest of Player progression remains unfinished.
- [ ] Achievements are not implemented.
- [ ] Journal is not implemented.
- [ ] Settings are not implemented.
- [x] Minimal Pause/Resume, menu return and Save and Exit; gameplay guards, previous time scale/focus restoration and focus-loss pause. Full Settings integration and hardware-keyboard acceptance remain pending. — **7B, 2026-10-22**
- [ ] Audio content and runtime audio behavior are not implemented.

Going forward, every completed and Unity-verified milestone must update this checklist and `Assets/Documentation/IMPLEMENTATION_STATUS.md`.
