# U0 — контракт дерева апгрейдів

**Фінальний контракт — 2026-10-23, U0 завершено:** [92 плитки / повний node manifest](ClickCook_Upgrade_Node_Manifest.md), 38 effect families, 76 прочитаних source observations + 16 доповнень ClickCook. Назви, описи, node IDs і граф ClickCook визначено; джерельні спостереження відокремлені від прийнятого дизайну. Всі 25 раніше непрочитаних popup дочитано через Chrome Computer Use. Нижчі checkpoints з меншими counts і відкритим source gate є історичними. Наступний milestone — U1; runtime effects U2–U6 не реалізовані цим документом.

Manifest має пріоритет над старим family-only graph та family-rank контрактом нижче: майбутній purchased state — `nodeId → purchasedRank`, ефекти агрегуються за familyId. Schema-1 поки зберігає лише legacy click_strength; U1 прив’язує його до node.click_strength.01 без зміни сили/ціни/рангу. Root безкоштовний і віртуально відкритий. До продажу іншого вузла потрібна versioned node-state migration зі збереженням legacy rank, а не обнулення/подвійне застосування. Числові candidate контракти crit/hold нижче належать їхнім першим вузлам, не є cap усього сімейства.

**Продовження source audit — 2026-10-23:** Звірено й записано додаткові popup-спостереження; зарезервовано 36 сімейств. Нові відносно попереднього checkpoint: day_income_bonus, location_value, selection_precision_window, order_tracker_tier, order_tracker_frequency. На огляді 76 плиток, але детальні знімки містять додаткові ступені; повний count/graph ще не доведений. Source gate перед U1 лишається відкритим. Погоджені правила швидкого вибору та переносу часу від критів збережено. Поточна матриця/перелік непрочитаного: [FishSim audit](ClickCook_FishSim_Upgrade_Audit.md).


**Уточнення власника / source audit — 2026-10-22:** Перед U1 потрібна повна відповідність плиткам і механікам FishSim, а не лише 27 сімейств. [Матриця звірки](ClickCook_FishSim_Upgrade_Audit.md) фіксує прочитані вузли, кухонні назви та прогалини. Наразі зарезервовано 31 сімейство (+auto_action_chance, +selection_start_progress, +critical_time_bonus, +triple_strike_chance). На огляді пораховано 76 плиток; повний manifest ще уточнюється. Погоджено: бонусний таймер швидкого вибору дає стартовий прогрес, повільний вибір без штрафу. **Source gate U0 перед U1 відкритий; екран ще не впроваджувати.**


Дата: 2026-10-22. Статус: **ADOPTED DESIGN** у межах доручення перейти до наступного milestone. Числа нової гілки — початковий баланс для майбутнього U2, а не авторські числа Figma чи доведений playtest баланс. Runtime цього checkpoint не змінюється.

Джерела: [GDD §13.2](ClickCook_GDD.md), [proposal §12](ClickCook_Upgrade_Design_Proposal.md), чинні `UpgradeCatalog.cs` / `UpgradeCatalog.asset`. U0 закриває реєстр усіх сімейств і точний контракт першої гілки; пізні гілки отримують власні числові контракти перед реалізацією.

## 1. Реєстр ідентичностей

ID регістрозалежні, незмінні, не залежать від локалізації. Перші 16 точно зберігають серіалізовані ID 6A. Наступні 11 закріплюють design IDs proposal §12.2. Рядок реєстру не вмикає продаж і не додає запис до schema-1 save.

| ID | Група | Форма ефекту / одиниця | Потрібна базова система | Етап |
|---|---|---|---|---|
| click_strength | manual | базова сила, HP/удар | ручне готування | 6A працює |
| critical_chance | manual | шанс криту, частка 0–1 | критичний удар | U2 |
| critical_multiplier | manual | множник криту | критичний удар | U2 |
| click_hold | manual | відкриття повторів | pointer hold lifecycle | U2 |
| auto_strength | auto | базова авто-сила, HP/tick | автоготування | U4 |
| auto_frequency | auto | tick/секунду | автоготування | U4 |
| dish_value | economy | множник вартості | receipt продажу | U5 |
| selection_bonus | choice | бонус виплати, частка | бонусне вікно | U5 |
| recipe_discount | economy | знижка, частка | покупка рецептів | U5 |
| upgrade_discount | economy | знижка, частка | purchase quote | U5 |
| passive_value | economy | множник доходу | пасивне нарахування | U5 |
| character_xp | xp | множник Player XP | базова Player XP | U5 |
| restaurant_xp | xp | множник Restaurant XP | базова Restaurant XP | U5 |
| dish_xp | xp | множник Dish XP | receipt Dish XP | U5 |
| passive_unlock | economy | відкриття доходу | час/стани нарахування | U5 |
| selection_window | choice | тривалість, секунди | бонусне вікно | U5 |
| click_multiplier | manual | множник базової сили | ручний damage pipeline | окремий slice |
| double_strike_chance | manual | шанс додаткового удару, частка | same-order proc | U3 |
| double_strike_power | manual | множник сили додаткового удару | same-order proc | U3 |
| hold_frequency | manual | повторів/секунду | click_hold | окремий slice |
| auto_action_chance | auto | шанс автоматичної дії, частка | source-to-kitchen trigger contract | source audit |
| auto_action_count | auto | кількість дій за tick, ціле | bounded same-order batch | U4 |
| preparation_efficiency | preparation | зменшення роботи, частка | immutable order work snapshot | U4 / окремий slice |
| selection_start_progress | choice | стартовий прогрес, частка HP | бонусне вікно вибору | U5 |
| selection_precision_window | choice | окреме ідеальне вікно, секунди | optional timely-choice contract | U5 |
| day_income_bonus | economy | бонус за завершений день, частка | day summary receipt | 8A / U5 |
| location_value | economy | внесок рівня локації у вартість | definition of location level owner | U5 |
| order_tracker_tier | information | рівень підказок | tracker contract | design gate |
| order_tracker_frequency | information | швидкість імпульсу, units TBD | tracker contract | design gate |
| critical_time_bonus | manual / choice | секунди за крит у завершеному order | crit + бонусне вікно, bounded carry | U5 |
| triple_strike_chance | manual | шанс двох додаткових ударів | same-order bounded proc | U3b |
| auto_strength_multiplier | auto | множник авто-сили | auto_strength | U4 |
| dish_xp_flat | xp | додаткова Dish XP | receipt Dish XP | U5 |
| restaurant_xp_flat | xp | додаткова Restaurant XP | базова Restaurant XP | U5 |
| character_xp_flat | xp | додаткова Player XP | базова Player XP | U5 |
| passive_flat | economy | додаткові у.е./секунду | пасивне нарахування | U5 |
| rarity_chance | quality | плоский quality-модифікатор, оператор TBD | погоджений probability transform | U6 |
| rarity_multiplier | quality | множник quality, оператор TBD | погоджений probability transform | U6 |

Для quality форма оператора й одиниці source rewards ще потребують уточнення: наведені назви не перетворюють `+0.02` чи `+0.5` на ймовірність. Для пізніх сімейств prices/caps/prerequisites лишаються draft у proposal; U1 показує їх недоступними, без вигаданого числового preview.

## 2. Точний граф першої гілки

Безкоштовний presentation-root `group.manual` не є сімейством, не має рангу/ціни і не пишеться у save. Він веде до `click_strength`. Нижче prerequisites — AND, рахуються лише за **придбаними** рангами. Для стартової гілки немає додаткових level/location gates.

| Сімейство | Prerequisite family | Мінімальний purchased rank |
|---|---|---:|
| click_strength | — | 0 |
| click_hold | click_strength | 2 |
| critical_chance | click_strength | 3 |
| critical_multiplier | critical_chance | 1 |

Граф: `click_strength → click_hold`; `click_strength → critical_chance → critical_multiplier`. Досяжність не залежить від жодної пізньої/непрацюючої гілки. Два виходи Click Strength — паралельні інвестиції. Перший critical rank одразу має базовий ×2 крит, без обов'язкової покупки множника. Hold одразу повторює звичайні удари.

Історичне рішення «один вузол на сімейство / I–III лише діапазони одного лічильника» скасовано уточненням власника: окремі source tier-плитки мають власні nodeId/ранги/назви; familyId зберігає ідентичність ефекту. Точний manifest див. source audit. Пізні групи manual/auto/xp/economy/choice/quality — навігація. Поза таблицею вище нові gameplay ребра не вважати затвердженими.

## 3. Баланс, округлення й activation gate

N — уже куплені ранги. До появи окремого discount slice ціна наступного рангу: `ceil(B × G^N)` у decimal, кінцеве округлення один раз до цілих у.е.; UI показує дві десяткові цифри. Не застосовувати неготову знижку.

| ID | B | G | Max | Ефект на ранзі N | Готовність зараз |
|---|---:|---:|---:|---|---|
| click_strength | 20 | 2.20 | 15 | ceil(1.75^N) HP/звичайний удар | працює, без зміни 6A |
| click_hold | 250 | 1.00 | 1 | N=0: вимкнено; N=1: повтор кожні 0.25 с | недоступно до U2 hold |
| critical_chance | 150 | 1.90 | 15 | 0.02 × N, максимум 0.30 | недоступно до U2 crit |
| critical_multiplier | 200 | 1.90 | 15 | 2 + 0.1 × N, максимум 3.5 | недоступно до U2 crit |

Click Strength rank 0 = 1 HP; перші три покупки 20/44/97, результат 2/4/6 HP. Critical chance rank 0 = 0; множник rank 0 = 2 не створює критів самостійно. Перші три ціни chance: 150/285/542; multiplier: 200/380/722. Hold має лише одну покупку за 250. Критичний удар: `ceil(D × M)`; звичайний: D. HP обмежується нулем; один order дає один receipt. D уже включатиме лише реально підключені модифікатори.

Crit roll — для кожного реально застосованого manual/hold удару, `u < chance` для u у [0,1); шанс 0 ніколи не критує. Auto не критує. Hold: початковий удар у момент натискання, повтори через 0.25 с; release не додає удар. Пауза/втрата фокусу/Back/result/end скасовують hold; накопичених повторів на наступне замовлення немає. Double strike лишається окремим U3 з правилами proposal §12.3.

Готовність ефекту перевіряється до покупки незалежно від виконання prerequisites/балансу. U1 продає тільки чинний Click Strength; усі інші вузли мають явну причину недоступності. Стани effect-unavailable, prerequisite-locked, max, unaffordable і buyable розрізняються. Панель може показувати prerequisites навіть для effect-unavailable. Quotes перевіряються повторно при Buy; paused/active-day/stale quote не проводить транзакцію.

Ці кандидати зберігають початкову таблицю proposal. Темп окупності не доведений: 20 у.е. першого рангу досягається на чинних стравах; подальший crit/hold баланс перевіряється на відповідному U2 slice, без повного проходження гри в U0.

## 4. Purchased ranks і source grants

- Purchased state: `familyId → purchasedRank`. Ранг збільшується тільки успішною покупкою; змінює next price, cap і purchased prerequisites. Зміна displayName або вузла дерева не змінює familyId.
- Achievement state у майбутньому: множина отриманих `sourceId` і grants з `sourceId`, `familyId`, `operation`, `value`, `unit`. SourceId має бути стабільним ID конкретного досягнення, не текстом/номером рядка; конкретні ID закриваються при імпорті 10B.
- Grant ніколи не підвищує purchasedRank, не здешевлює наступний ранг через фальшивий rank і не відкриває purchased prerequisites. Повторний sourceId не нараховується вдруге; grant і позначення ownership зберігаються однією транзакцією.
- Єдиний розрахунок effective stats поєднує purchased contributions і унікальні grants. Типовий порядок плоске → множник приймається лише після визначення одиниць конкретного сімейства. Для ще не визначених source units/UI не вигадувати бонус. Receipts використовують один snapshot effective stats, а не повторно додають досягнення.
- Schema-1 7B приймає лише чинний click_strength. Цей реєстр не розширює whitelist save: перед першим продажем нового сімейства потрібні окремі version/migration/load bounds і перевірка збереження цього сімейства. Старий профіль/Click Strength не скидаються. Achievements не реалізуються через підробку upgradeRanks.

## 5. Межа завершення U0

Результат: 27 незмінних family IDs, узгоджені джерела, граф і числовий контракт стартової manual-гілки, source ownership contract. Локальна перевірка: унікальність/збіг 16 чинних IDs, наявність вузлів/меж prerequisites, відсутність циклу, досяжність і контрольні ціни. Runtime, assets каталогу, сцена і save schema не змінені; Play Mode/build не потрібні для цього документаційного кроку.

Наступний milestone — **U1**, широке дерево, detail/statistics, zoom/pan; продаж лише готового Click Strength. Нові gameplay ефекти реалізуються окремо U2–U6.
