# ClickCook — UI/UX specification for Unity

Поправка власника, реалізована2026-10-28: до10 страв вартість замовлень є тимчасовим доходом дня; баланс не зростає. Достроковий Finish анулює весь цей дохід, зберігаючи попередній баланс і Dish XP. Result пояснює виплату після10 страв; кнопка Finish і Pause попереджають про втрату доходу. Ранній summary показує Earnings0, загальну анульовану суму та позначає рядки як анульовані. На десятій страві вся сума зараховується й зберігається один раз; Continue/summary не платять повторно. Нове правило замінює старі описи негайної виплати нижче. [Контракт8A](ClickCook_8A_Day_Summary_Contract.md).

Оновлення 2026-10-28 — 8B: з вибору локації доступний магазин32 рецептів,8 рядків на сторінку. Є ціна/стан володіння, продажна вартість, serving-локації, баланс і купівля з автозбереженням. Панель локації показує власні/загальні рецепти; без жодного власного рецепта Cook недоступне, є підказка, а навігація переходить тільки між активними діями. Новий профіль починає із сендвічів, Michelin лишається з4 авторськими стравами за рішенням власника. New Game явно попереджає про скидання куплених рецептів. [Контракт і локальні докази8B](ClickCook_8B_Recipe_Contract.md). Фінальний Journal з інструкціями й Player-level locks ще попереду.

Оновлення 2026-10-28 — 8A: окремий paper-екран підсумку дня показує локацію, до десяти фактичних receipts зі стравою/якістю/грошима/Dish XP та точні суми. Ранній Finish Day і Continue після десятої страви відкривають його без нових нагород; нульовий день має явний порожній стан. «Апгрейди» повертають до того самого звіту кнопкою «До підсумку», а «Наступний день» повертає до вибору локації. Pause/menu/save-warning з 7B діють поверх екрана. Календар і Player XP ще не додано. Повторно використано наявні paper/button/font ресурси; початковий focus на Next Day, явні навігаційні зв'язки між двома кнопками. Локальні Play/text-fit та native mouse перевірки: [контракт 8A](ClickCook_8A_Day_Summary_Contract.md). Попередні описи незавершеного summary нижче є історичними.

Оновлення 2026-10-22 — 7B: додано окремий Main Menu (Continue залежить від валідного профілю; New Game з явним підтвердженням; Exit), мінімальну Pause (Resume/до меню/зберегти й вийти), Esc і pause-on-focus-loss. Повторно використано uGUI paper/button/font ресурси. UI показує recovery з backup, захист новішої схеми й помилку запису з Retry; невдалий запис блокує тихий вихід. Gameplay callbacks мають окремі guards, фонова кнопка не обходить паузу/меню. Три меню перевірено після rendered frames у 1280×720, 1920×1080 і 2560×1080: bounds, glyphs, text height, topmost raycasts. Це мінімальний 7B slice; повний Settings/version/Discord footer, інші екрани й final art залишаються запланованими. Windows/нативні перевірки — IMPLEMENTATION_STATUS.md.

Оновлення 2026-10-21: прилавок перепідключено до оновленого Dormitory без заміни його нової геометрії/матеріалів. Фізична дошка й тарілка зберігають чинні взаємодії. Для Burgers (ID 6) використано авторську Blender-модель із послідовним складанням інгредієнтів за реальним HP-прогресом; кунжут рухається з верхньою булочкою, клік дає короткий squash. Підпис цієї страви компактно розміщено ліворуч, щоб він не закривав складання. Back скидає HP/анімацію; результат показує зібраний бургер. Візуальна анімація не затримує видачу чинної нагороди й не створює додаткових порцій. Інші страви, фінальні ефекти й решта UI залишаються у своїх попередніх станах. Докази перевірок та межі — IMPLEMENTATION_STATUS.md.

## Актуалізація за дизайнерським FigJam — 2026-10-15

Пріоритет: останні явні рішення дизайнера → ClickCook-описи/таблиці/макети Figma → адаптація механік і дерева FishSim → попередні інтерпретації GDD/прототипу → рекомендації цього документа. Див. GDD §0 і §13.2. Цей запис синхронізує дизайн, не засвідчує нові Unity-перевірки.

UI-A4 виконано й перевірено 2026-10-15: аркуші замовлень на фізичній дошці праворуч; collider-clickable страва на наявній тарілці/підносі. Камера фокусується на страві й повертається до дошки; HP правдиве, Back не замінює orders. Центральні картки UI-A3 — попередній історичний етап. П'ять rendered resolutions / усі 11 локацій і native mouse цикл пройдено; деталі — IMPLEMENTATION_STATUS.md. Страви/картки ще тимчасові, не final art.

Додано focus/Submit на самій 3D-страві та явну arrow-навігацію з Back, з підказкою про клік/Enter. EventSystem перевірки пройдено; фізичну клавіатуру й standalone build у UI-A4 не тестували. Нижчі розділи про повний UI залишаються планом, а не заявою готовності всіх екранів.

Дерево — широка механічна прогресія за FishSim, не лише його стилізація: 27 планових сімейств, паралельні гілки, tier/rank, preview ціни/ефекту й active statistics. Із них фактично працює тільки Click Strength. Інші вузли до реалізації мають чесний стан «У розробці», відмінний від gameplay-lock; не продавати ефект-заглушку.

Дата: 2026-10-12. База: завершений Milestone 6A, commit 1fa32ff.
Статус: UI-A1 і UI-A2 виконано та перевірено в межах існуючого циклу. Решта екранів — дизайн-план за п'ятьма наданими референсами.
Оновлення UI-A3 після тесту користувача: Exit у поточному меню локацій, видимий HP/click count під час готування та великі центральні картки за новим референсом. Це явна зміна попередніх hidden-HP/right-board вимог; gameplay формули збережено. Mastery смуги карток підписані й використовують фактичний Dish XP, final art — майбутній крок.
Оновлення UI-A1: Location Selection і Click Strength оформлені в Unity; component assets, copy та automated layout/input checks готові. Computer Use native mouse checks пройдено; виправлено втрату фокусу після покупки. Користувач підтвердив роботу фізичних Enter і стрілок у Game View. Синтетичні клавіші Computer Use не викликали runtime Input System дій — це обмеження автоматизації.
Оновлення UI-A2: оформлено Order Board/HUD, Cooking та Dish Result; українські назви/рідкість/receipt, окремі daily/orders counters, real mastery bar, state focus. П’ять розмірів Canvas/raycast/focus, regression і нативний цикл мишею перевірено; новий фізичний keyboard pass і build не виконано. Страви мають явно позначені тимчасові ескізи. Деталі виконаних перевірок — IMPLEMENTATION_STATUS.md. Подальші розділи описують весь план, а не готовність усіх екранів.
Джерела: ClickCook_GDD.md (§3–7, 9–11, 13, 16–25), IMPLEMENTATION_STATUS.md та актуальний production roadmap.
Попередню ієрархію «GDD вище Figma» скасовано рішенням дизайнера 2026-10-15. Однак structured ClickCook tables визначають числа, а презентаційні приклади — композицію; числа FishSim не стають автоматично балансом ClickCook. Нові інтерпретації мають явні позначки джерела/дизайн-рішення/баланс-пропозиції.

## 1. Висновок і рішення про запуск

Доцільно розпочати UI зараз як окремий обмежений UI-A: дизайн-система й оформлення вже реалізованих Location Selection, Order Board, Cooking, Dish Result і Click Strength purchase.

Повне production-оформлення всіх екранів краще завершувати разом із відповідними системами: Main Menu/Continue після save/load, підсумок дня після day report, recipe UI/Journal після ownership, повне дерево після готових effects. Інакше доведеться переробляти layout, навігацію й стани.

Профіль 7A не починати до окремої команди користувача. Сам UI-A цим документом не запускається. Повна специфікація нижче охоплює фінальний продукт, а acceptance першого підкроку — лише наявні можливості.

## 2. Розбір п'яти референсів

| Референс | Що використати | Що уточнити або виправити |
|---|---|---|
| 1 — Main Menu, Settings, Pause | Проста ієрархія дій, окремі параметри звуку/екрана, версія й Discord | Текстові рядки перетворити на кнопки з focus; не додавати непрацюючі Continue/Settings до поточної гри |
| 2 — Location, Upgrades, Achievements, Journal | Carousel, дерево, statistics panel, collection grid із detail panel | Навігацію й назви узгодити; пояснювати locked/unavailable; бари мають отримувати підписане джерело даних |
| 3 — Board, Cooking, Result, Day Summary | Тарілка/клош, чіткий результат, окрема дія продовження, summary із наступним днем | HP видимі після UI-A3; 67/23 не мають визначеної семантики й не є формулою. Finish недоступний під час Cooking/Result за поточними правилами |
| 4 — оформлене меню та Location Selection | Глибокий синій food-pattern background, пергамент, теплий логотип, orange-outline buttons, ілюстрація закладу | Взяти декоративні предмети з правого варіанта меню дозовано; не перевантажувати. У прикладі «УРОВЕНЬ 1 / 5 в секунду» не відповідає нинішньому Dormitory level 0 / income 0 |
| 5 — картки замовлень у середовищі | Пергаментні картки, велика ілюстрація/назва страви, компактний HUD, видима кухня | 253 і 5/10 — демонстраційні числа. Зелені смуги під стравами не трактувати як hidden HP, таймер чи рецепт без рішення |

Модель страви/зображення на референсі не означає, що в Assets уже є відповідний production asset. Наразі потрібні окремі вихідні logo/background/panel/button/icon/food sprites; не розрізати композитні скріншоти на фінальні текстури й не обіцяти автоматичний Figma import.

## 3. Візуальна система

Характер: затишна настільна кулінарна гра; читабельність і швидка дія переважають декоративність.

Два контексти:
- Меню/метагра: синій фон з ненав'язливими food motifs, пергаментна панель і теплі ілюстрації.
- Work Day/Cooking/Result: чинне 3D середовище видиме; невеликі світлі HUD-плашки й непрозорі картки. Не перекривати кухню великим чорним прямокутником.

### Запропоновані токени, не виміряні пікселі референсу

| Токен | Значення | Використання |
|---|---|---|
| BackgroundBlue | #17366B | Зовнішній фон меню |
| BackgroundDeep | #10264D | Віньєтка фону |
| Paper | #FFF0CE | Поля з текстом |
| PaperShade | #E8C88B | Край панелі/легкий об'єм |
| Ink | #3B281B | Основний текст |
| InkMuted | #6A4B32 | Допоміжні написи |
| Accent | #FFB33E | Основна кнопка |
| AccentBorder | #A95712 | Контур/стан натискання |
| Progress | #729C3D | Підписаний mastery/progression bar |
| Focus | #FFFFFF + #17366B | Подвійний видимий focus outline |
| DisabledPaper | #DDD4C0 | Недоступні елементи з поясненням |
| ErrorInk | #8A261F | Помилки разом із текстом/іконкою |

Темний Ink на світлому Paper; текст не розміщувати безпосередньо на строкатому 3D/food-pattern фоні. Пергаментна текстура в області body text має бути малоконтрастною, краї — декоративними. Читабельність не залежить від bloom або постобробки камери.

Контрастний орієнтир: 4.5:1 для звичайного тексту, 3:1 для великого тексту; перевірити фінальні пари й background textures, а не лише HEX. Це запозичений accessibility benchmark, не заява про WCAG сертифікацію гри: [W3C Contrast](https://www.w3.org/WAI/WCAG21/Understanding/contrast-minimum).

### Типографіка та відступи

Reference layout 1920×1080:
- Заголовок екрана 44–52, назва страви/локації 36–42.
- Body і кнопки 28–32; допоміжні написи 24–26. На 1280×720 основний текст має залишатися близько 18–21 фізичних px.
- Logo — окремий asset; handwritten accent дозволений для коротких заголовків страв. Ціни, XP, меню, опис — читабельний звичайний шрифт.
- Кирилиця/латиниця, українські Ґ/Є/І/Ї, апостроф, довге тире, цифри й currency мають бути в перевіреному font coverage.
- Не використовувати тонкий рукописний шрифт для всього HUD; не вміщувати довгі назви автоматичним зменшенням до дрібного тексту.
- Базова сітка відступів 8; зовнішній margin 48, inner padding 24–32; між секціями 24–40.
- Інтерактивний контроль має висоту 64–72 reference px; primary CTA 280–360 wide.
- Дані валюти — з розділенням тисяч і двома десятковими знаками. Currency «у.е.» зберігається як термін GDD до окремого localization рішення.

Перший UI-A може зберегти UnityEngine.UI.Text з перевіреним шрифтом. TMP-міграція не є передумовою оновлення UI; якщо пізніше потрібен TMP, робити окремий перевірений підкрок без Examples/Extras. Шрифт і його ліцензія мають бути визначені перед імпортом.

## 4. Карта навігації

Зараз: Location Selection ↔ Upgrades; Location Selection → Work Day → Cooking ↔ Board; Cooking → Result → Board або завершення дня → Location Selection.

Фінальний маршрут: Main Menu → Location Selection → Work Day → Cooking → Result → Board → End-of-Day → Upgrades/Next Day. Journal/Achievements/Settings доступні з погоджених metagame screens. Pause — overlay під час активної гри після визначення pause політики.

Під час Cooking/Result не показувати доступні metagame-вкладки; випадкова навігація не повинна втрачати чи змінювати нагороду/замовлення. Перехід на фінальний summary виконувати разом із 8A; стилізація UI-A не змінює поточний return to Location Selection.

## 5. Специфікація кожного екрана

### Main Menu — після 7B

Композиція: синій pattern, центральний пергамент до 960×720, логотип угорі, чотири однакові кнопки вертикально; декоративні кухонні предмети по нижніх кутах. Footer: версія ліворуч, Discord праворуч, destination тільки після визначення URL.

Кнопки: «Продовжити», «Нова гра», «Налаштування», «Вийти».
Continue enabled тільки за наявності валідного save; недоступний має пояснення «Збереженої гри ще немає». New Game при наявності progress потребує підтвердження наслідків згідно майбутньої save політики. Наявність Continue не підміняти новою грою.

### Location Selection — UI-A

Header: Wallet зліва; міжденні вкладки/Upgrades по центру, settings іконка праворуч лише після готової поведінки. Непрацюючі глобальні кнопки у UI-A не створювати.

Основна panel приблизно 1240×720, carousel arrows поза її боками. Ліворуч ілюстрація закладу (460–520 wide), праворуч назва, documented level, income/sec і dish strip. Bottom primary CTA: «Готувати!».

Поля UI-A:
- «Гуртожиток» — запропонований localized display label для Dormitory; стабільний ID/дані не перейменовуються.
- «Рівень локації: 0»; не показувати Player level замість LevelRequirement.
- «Дохід локації: 0 у.е./с» та для ненульових локацій tooltip «Нарахування пасивного доходу ще не реалізовано» до 9B.
- Dish strip: відомі DocumentedAvailableDishes; ілюстрація/назва; порожній стан «У цій локації немає доступних замовлень».
- Поки meaning location progress не визначений, бар не заповнювати вигаданим відсотком. Можна прибрати його з production-style UI-A або явно залишити «Прогрес локації — незабаром».
- All 11 browsable як зараз; кнопки wrap. Locked presentation додається з 8C; знак «?» не означає ownership до 8B.

Зберігати current location при виході з Upgrades. CTA видимий, arrows мають цілі hit areas, довгі назви максимум два рядки.

### Order Board — UI-A

Топ HUD: wallet ліворуч, «Приготовано 3 / 10» по центру, «Завершити день» праворуч. За потреби допоміжний рядок «Замовлення: 2 / 2» з назвою локації. Daily count і місткість не змішувати.

Картки як у референсі 5: пергамент, велика food illustration, назва, чітка рамка наведення. Кожна card — одна велика кнопка. Візуальний нахил дозволений для art layer; hit area передбачувана і картки не перекривають одна одну.

Актуальне GDD §7 / UI-A4: аркуші прив'язані до реальної дошки оголошень праворуч. Перспектива, камера та масштаб забезпечують читабельність 2–6 окремих замовлень без перекритих hit areas. Не замінювати це screen-space grid, не перекривати дошку HUD-панелями. Потрібні помітні hover/focus і надійна взаємодія pointer/raycast. UI-A3 central pair/row/3×2 — попередня перевірена композиція, яку не видавати за фінальний spatial дизайн.

Повторна страва — окрема картка/slot; не зливати дублікати. При виборі картки — одразу Cooking. Немає кнопки підтвердження чи заміни при selection. Інші варіанти приховані або недоступні під час close-up, але їхні order instances зберігаються для Back. Початкове Figma «пропадають» означає presentation, не видалення замовлень. Майбутнє bonus window не є строком придатності карток.

UI-A3 включає зелені смуги з явним підписом «Майстерність» і джерелом фактичного Dish XP / next-level threshold. Це presentation choice для вже реалізованих даних, не нова механіка. На cooking окремо показувати HP помаранчевою шкалою.

Діагностичні GUID, IDs, provisional capacities і candidate lists прибрати з основного player HUD; залишити в Editor/debug view при потребі. Це зміна presentation, не eligibility.

### Cooking — UI-A

Центр/нижня зона: вибрана страва з'являється на справжній тарілці на підносі Dormitory. Не малювати другу пласку тарілку поверх реальної. Страва — фізична точка кліку; камера може керовано змінити ракурс/крупність, але залишається в тому самому середовищі й повертається без накопиченого зміщення. Угорі коротка назва; праворуч жодні інші замовлення не доступні до натискання під час close-up.

Підказка «Натискай на страву, щоб готувати» показується ненав'язливо. Target покриває саму страву з достатнім допуском, а не великий невидимий прямокутник ресторану; decorative feedback не змінює надійність натискання. Клавіатурний шлях до дії перевіряти окремо від mouse/raycast; перехід на 3D не дає права називати keyboard доступність перевіреною без тесту.

Back: «До замовлень»; tooltip/підказка «Прогрес приготування цієї страви буде скинуто». Зберігати чинну identity/Back логіку. Finish не показувати як активну кнопку всередині Cooking.

UI-A3 supersedes hidden-HP rule: показувати «HP: remaining / max», спадну шкалу та «Залишилось кліків: ceil(remaining HP / current damage)». Back/reselect відновлює повне HP. Значення 67/23 з ранніх референсів залишаються непоясненими. Короткий visual press/tint допустимий; sparks/flames, screen shake, damage numbers та нові механіки — окремий art/effects підкрок. Не додавати fake crit feedback.

### Dish Result — UI-A

Страва на тарілці/клош і одна світла result card:
- Назва.
- Рідкість: Common/Uncommon/Rare/Fantastic/Legendary за наявним результатом; запропоновані UA labels: Звичайна / Незвичайна / Рідкісна / Фантастична / Легендарна.
- «Зароблено: 12,50 у.е.» і менше «Баланс: 37,50 у.е.».
- «Досвід страви: +12 XP».
- «Рівень страви: 2 → 3», підписаний mastery bar із поточними XP / transition cost; на cap «Майстерність: максимум».
- Primary «Продовжити»; для десятого результату label «Завершити день» може викликати ТУ САМУ існуючу Continue дію.

Rarity-color як додатковий cue, завжди разом із назвою/іконкою. Payout береться з immutable receipt; анімація цифр не змінює wallet. Result не зникає автоматично й не пропускає десяту страву.

### End-of-Day Summary — після 8A

Пергаментний report поверх видимого середовища; список prepared dishes ліворуч, підсумки праворуч. Показати кількість, earnings і лише реально реалізовану XP; Upgrades / «Наступний день» унизу.

Дата, Player/Restaurant progress, total earnings calculation не вигадуються для дизайну. Empty day: «Сьогодні жодної страви не приготовано». Усі виплати вже здійснені; report/Next Day не платять вдруге.

До 8A у UI-A стилізувати лише існуюче повідомлення завершення на Location Selection.

### Upgrades — UI-A, потім повне дерево

UI-A: велика робоча card Click Strength з рангом 0/15, current → next power, wallet, price та Buy. «Сила кліку», «Придбати за 20,00 у.е.», «Недостатньо коштів», «Максимальний ранг». Середина — майбутнє дерево; недоступні гілки чітко мають «Незабаром».

Один active node не повинен виглядати як 16 робочих upgrades. Disabled node не видавати за locked prerequisite; tooltip пояснює статус. Купівля показує новий ранг/баланс; повторна команда не купує зайве.

Повний варіант за Figma/GDD §13.2: широке зв'язане дерево, readable zoom/pan, detail panel вибраного вузла й «Активні покращення» з Hide/Show. Картка вузла містить опис, rank/max, current → next effect, ціну й конкретні передумови. Гілки: сила/множник, crit chance/multiplier, double strike chance/power, hold/frequency, auto strength/multiplier/frequency, flat/multiplier XP, економіка/знижки/пасиви, вікно вибору, quality. За потреби I/II/III — окремі видимі ступені; не застосовувати один maxRank до всіх вузлів тільки через ранній draft. Візуальні ребра беруться з явних prerequisite даних, не з довільної геометрії макета.

Double strike — випадковий додатковий удар, не вимога швидкого подвійного натискання. Іконка/feedback відрізняються від криту й не обіцяють другу страву/виплату. Коли цей ефект реалізують, показувати шанс і силу окремо; поточна HP-шкала лишається правдивою, оцінка кліків — звичайні ручні кліки без випадкових бонусів/auto.

У UI-A можна показати поточну силу кліку в компактній statistics area; крит/auto/XP bonus не показувати як активні.

### Journal — після 8B/10A

Grid із фільтрами/scroll ліворуч; selected dish detail справа. Title, food illustration, mastery, кількість реально приготованих, reference SaleValue/actual policy, рецепт/ownership із погодженого runtime.

На референсі п'ять colored bars є відомими полями UI, але їхня точна семантика не визначена; не вводити rarity counts без collection рішення й даних. Відокремити ціну рецепта від вартості продажу. FigJam уже має авторські ingredient/method texts і таблицю рецептів; це джерело контенту, не доказ готової runtime купівлі або інтерактивного minigame. Recipe action з'являється лише після системи купівлі.

Empty state: «Страви з'являтимуться тут у міру відкриття». Sorting/filtering правила — implementation рекомендації, погодити з 10A.

### Achievements — після 10B

Список rows: icon, title, коротка умова, progress, reward/status. Completed має текст/значок; колір не є єдиною ознакою. Active statistics panel reusable.

Не створювати фіктивні назви/вимоги/нагороди. Collect reward action лише якщо погоджено ручне отримання; автоматична видача потребує іншого статусу UI. Обидва варіанти не вибираються через вигляд wireframe.

### Settings / Pause — 7B та UI-C

Settings: заголовок, вкладки «Екран», «Звук», «Мова»; values поряд із controls, підказки та Back. Window/fullscreen, resolution, FPS беруться з реально підтриманого списку; запропоновані labels «Режим екрана», «Роздільність», «Обмеження FPS».

Гучність: «Музика», «Оточення», «Ефекти», slider з числовим %. Runtime routing після Audio системи; не продавати неробочі sliders як settings. Reset Data у відокремленій секції з майбутньою явною confirmation.

Pause overlay: затемнення середовища, «Пауза», «Продовжити», «Налаштування», «Головне меню». Escape — запропонована desktop shortcut, але поведінку pause/focus й recovery закрити в 7B. Закриття overlay повертає попередній стан; воно не запускає новий день.

## 6. Текстова система та states

Мова цієї специфікації — українська, запропонована copy база. Мови релізу ще відкриті; UA copy не перейменовує англійські ID/російські authored names у static data. Для runtime тексти зберігати централізовано за keys; placeholders не concatenation, щоб підтримати майбутню локалізацію.

| Key / ситуація | Запропонований текст |
|---|---|
| menu.continue / new / settings / exit | Продовжити / Нова гра / Налаштування / Вийти |
| location.title / cook | Локації / Готувати! |
| day.completed | Приготовано {completed} / {limit} |
| day.orders | Замовлення: {available} / {capacity} |
| day.finish / empty | Завершити день / У цій локації немає доступних замовлень |
| cook.hint / back | Натискай на страву, щоб готувати / До замовлень |
| cook.backHint | Прогрес приготування цієї страви буде скинуто |
| result.payout / xp | Зароблено: {amount} / Досвід страви: +{xp} XP |
| upgrade.purchase / insufficient / max | Придбати за {price} / Недостатньо коштів / Максимальний ранг |
| feature.unavailable | Незабаром |
| save.missing / failed | Збереженої гри ще немає / Не вдалося зберегти гру |
| confirmation.cancel / accept | Скасувати / Підтвердити |

Тексти помилок повідомляють дію й можливість повторити, не показують stacktrace. Debug IDs відокремлені від player UI.

Кожна кнопка: Normal, Hover, Pressed, Focused, Disabled; додатковий Busy лише під час реально асинхронної операції. Hover змінює контур/яскравість; Pressed не зміщує hit area. Disabled explanation доступне як допоміжний label/tooltip, не приховане на непрацюючій button event.

Escape не «завершує день». Keyboard focus не активує gameplay action сам по собі. Картки/CTA мають зрозумілий focus order; Enter/Space підтримати як UI activation у погодженому input кроці. Mouse й keyboard перевіряти фізично, а не лише callbacks.

## 7. Unity implementation contract — майбутня робота

Reuse: поточний Canvas/EventSystem/InputSystemUIInputModule, LocationCatalog, controller/session/rewards і UpgradeCatalog. Framework лишається uGUI.

Layout:
- CanvasScaler Scale With Screen Size, reference 1920×1080, Match Width/Height 0.5 як стартова рекомендація.
- Anchors, layout groups і окремі safe margins; 9-slice panels/buttons; не масштабувати весь композитний screenshot.
- Layout variants для 16:9, 16:10 і 21:9: центральний content має обмеження ширини; HUD прив'язаний до safe bounds, scene/background заповнює екран.
- Мінімальний validation target 1280×720, також 1920×1080, 2560×1440, 1920×1200, 2560×1080. Остаточний supported range залежить від release brief.
- На низькій висоті переносити/scroll довгі detail/list screens, не робити все дрібнішим. Для ScrollRect при реалізації використати відповідний uGUI reference.

Unity Canvas масштабування описано в [Unity Learn: screen size and anchors](https://learn.unity.com/pathway/creative-core/unit/creative-core-ui/tutorial/manage-screen-size-and-anchors?version=6.3).

Плановані області файлів:
- Assets/Documentation/ClickCook_UI_UX_Specification.md — цей документ.
- Assets/UI/Art/ — очищені logo/background/paper/buttons/icons; separate dish/location thumbnails.
- Assets/UI/Fonts/ — погоджений readable font/license, glyph coverage.
- Assets/UI/Prefabs/Components/ — PaperPanel, PrimaryButton, SecondaryButton, WalletBadge, OrderCard, StatRow, Tooltip.
- Assets/UI/Prefabs/Screens/ — відповідні екрани по мірі готовності.
- Assets/Scripts/UI/ — мінімальні view/style/text bindings за потреби; не будувати новий загальний UI framework.
- SampleScene/controller — точкові зміни presentation/bindings; orders/rewards math не змінювати для стилізації.
- Existing verification harness — оновити references тільки якщо змінився спосіб binding, зберегти gameplay assertions.

Навмисно приховані screen roots допустимі. Будь-яке переприв'язування controller перевірити на missing references. Нові graphics мають ненульові bounds; decorative images/text raycastTarget=false; одна система вводу й робочий GraphicRaycaster.

## 8. Asset readiness та scope UI-A

Потрібні assets для першого UI-A:
- Paper panel/button states як окремі scalable sprites.
- Іконки coin, arrows, cooking/back і upgrade.
- Читабельний font з UA/Latin coverage.
- Обрані перші food/location illustrations як тест стилю.

Screenshot reference лише задає напрям. Якщо оригінальні source assets відсутні, у першому кроці використовувати чисті тимчасові іконки/силуети в погодженому стилі й явно зафіксувати рівень fidelity; не називати їх фінальним арт-пакетом. Генерація/створення фінальних sprites — окреме завдання після початку UI реалізації.

UI-A можна поділити:
- UI-A1: токени/типографіка/component prefabs + Location Selection і робоча purchase card.
- UI-A2: OrderCard/HUD + Cooking/Result над чинним середовищем.
Кожний — окреме виконання з compilation, Play Mode й regression. Жоден із них цим дизайн-кроком не реалізовано.

## 9. Критерії готовності UI-A

- П'ять наявних станів мають узгоджений стиль; останні два референси задають visual direction.
- Основний текст читабельний на 1280×720, довгі назви/валюта/cap не обрізаються; contrast перевірений із textures.
- Carousel переглядає всі 11, дані беруться з runtime; level0/income0 Dormitory не змінені через референс.
- Orders 2–6 вписуються, дублікати незалежні; daily count показано окремо.
- HP та mastery мають окремі явні підписи й фактичні дані; немає непідписаних fake bars, чужих balance values або працюючих кнопок неготових систем.
- Click Strength purchase, exact completion, Back, rarity/XP/payout, tenth Continue і early Finish проходять існуючу regression suite.
- Перевірені topmost raycast, реальна mouse взаємодія, focus order та принаймні gameplay build на цільовій платформі, якщо його включено в конкретний UI підкрок.
- Captures зроблені після оновлення кадру, для всіх змінених станів на різних resolutions. Screenshot lag у MCP не вважається доказом відображення поточного значення.
- Камера/lighting/URP/Dormitory збережені; scene і prefabs saved, немає нових exceptions/missing references.
- GDD/трекер оновлені лише на фактично виконані/перевірені пункти; Git checkpoint після реалізації.

## 10. Що ще потребує рішення

Не блокує UI-A: save policy, recipe ownership, Restaurant/Player XP, full upgrade effects, achievement rewards, Journal data, progress semantics. Їхні елементи не отримують фіктивної поведінки.

До production потрібні: source art/license, мови/terminology, зміст location progress, значення colored bars референсів, settings defaults/pause policy, final dishes/environment scope. До отримання відповідних даних застосовувати явно підписані чи приховані поля.

UI-A1/A2/A3/A4 завершені у своїх межах; full UI/UX і final art залишаються частковими. Наступний окремий implementation checkpoint — 7A profile/stable IDs, потім 7B save та U0–U6 широкого дерева за залежностями; full UI-C screens завершувати з відповідними runtime systems. Жоден із цих майбутніх кроків ще не виконано.
