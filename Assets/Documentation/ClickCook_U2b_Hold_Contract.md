# U2b — Hold

ADOPTED IMPLEMENTATION CONTRACT — 2026-10-24. Джерела: U0 contract, node manifest, roadmap U2. Окремий execution slice після криту, без hold_frequency.

- node.click_hold.01: один придбаний ранг, 250 у.е.; prerequisite node.click_strength.01:2. Hold rank 0 вимкнено, rank 1 — повтор кожні 0.25 с (4/с). Купівля через чинний quote/debit/autosave; інші ефекти/ціни не змінюються.
- З Hold натискання ЛКМ на активній страві відразу робить один ручний удар. Повтори йдуть через чинний controller/manual hit pipeline, з одним critical roll на фактичний удар. PointerUp/наступний Click не додає удару. Без Hold залишається звичайний click-on-release. Enter/Submit — одна звичайна дія; утримання клавіатури не створюється цим slice.
- Прийняте уточнення lifecycle: вихід pointer за страву або зміна UI focus теж скасовує Hold. Один pointer/order за раз; інші кнопки й чужий pointer-up не завершують активну ЛКМ. Продовження після скасування потребує нового натискання.
- Pause (навіть pause/resume в одному кадрі), втрата фокусу/системна пауза, Back, result/completion, end day, меню, disable і новий order скасовують Hold. Стан прив'язаний до конкретного order і не зберігається у профілі. LateUpdate повторює після обробки EventSystem release/exit; не може залишитися release-frame зайвий удар через порядок Update.
- Не накопичуються прострочені повтори: максимум один повтор за кадр, наступний deadline = фактичний час +0.25 с; повільний кадр не створює burst. Цільові 4/с — максимальний стабільний темп при достатній частоті кадрів. Completion один/order, без carry/reward дублювання.
- Schema 3 додає тільки canonical node.click_hold.01 з рангом 0–1 і перевіреною передумовою до чотирьох робочих вузлів. Читаються schema 1 (legacy mapping) і 2 (перші три node IDs) без негайного переписування. Hold у schema 2 не приймається; наступний звичайний save пише 3. Це захищає новий профіль від старої U2a-версії, яка бачить version>2. Backup 1/2 читабельний; version>3 захищений.
- Дерево показує 0/1, 250 у.е., вимкнено → 4 рухи/с, max після покупки та фактичний Hold у статистиці. Підказка на страві розрізняє Hold ЛКМ і дискретний Enter. Вузли hold_frequency лишаються недоступними.

Локальні ризики/перевірки: точні deadlines/no backlog; initial/up/click без подвоєння; lifecycle та same-order binding; крит і completion через той самий шлях; купівля/cap/передумова; schema 1/2→3, whitelist і restart. UI/PointerEventData/кадри на окремому Temp-профілі; без широкого gameplay/resolution suite або build. Native фізичний input та pacing не прирівнюються до синтетичних подій.
