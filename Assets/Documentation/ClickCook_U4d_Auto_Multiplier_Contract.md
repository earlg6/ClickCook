# U4d — множник сили авто-дії

ADOPTED IMPLEMENTATION CONTRACT — 2026-10-26. `node.auto_strength_multiplier.01` / «Школа су-шефа I», prerequisite `node.auto_strength.01:1` за manifest. Початковий баланс ClickCook: 10 рангів, `ceil(1000 × 1.8^N)` за наступний ранг N=0–9. Це не source reward і не підтверджений темп економіки.

- Множник `1 + 0.1 × N` (×1–×2), окремо від purchased auto_strength. Шкода однієї авто-дії `ceil((1 + strengthRank) × multiplier)` з decimal та одним округленням після множення. Наприклад, strength 2: 3 × 1.1 → 4 HP; 3 × 1.2 → теж 4 HP. Detail чесно показує однаковий damage сусідніх рангів. Максимум 16 × 2 = 32 HP/дію, 11 × 32 = 352 HP/batch. Ручні удари/crit/proc, count, start chance і секундна частота не змінюються.
- Спільний ApplyAutoBatch бере snapshot ефективної сили для start/periodic. Lethal завершує batch: HP clamp, одна receipt/виплата/XP/notification, без carry та combat RNG. Back не поновлює стартовий roll; scheduling/пауза залишаються чинними.
- Quote/cap/prerequisite/ціна/current→next працюють через загальний purchase pipeline. Detail множника містить factor і фактичні HP; strength preview враховує придбаний множник; count/chance detail і шість рядків active statistics показують ефективну силу.
- Schema 10 додає тільки canonical ID цього вузла з cap 10 і purchased prerequisite. Schema 1–9 відхиляють новий ID навіть з rank 0. Load не переписує старі файли, не видає безкоштовного множника та зберігає wallet/XP/всі ранги/backup; звичайний Save пише 10. Версії >10 захищені. Timers/day/order/input/feedback transient.
- Multiplier II, пізні auto tiers і frequency залишаються недоступними. Наступний slice — U4e, перший `node.auto_frequency.01`, після власного контракту. U4 ще частковий.

Мінімальна перевірка за AGENTS.md: compilation для нових властивостей; model prices/cap/guards, rounding across ranks, start/periodic/manual isolation і bounded completion; storage 1–9→10/backup/invalid/future; native mouse purchase на ізольованому Temp-профілі, controller tick/result/autosave та restart/Continue; один поточний rendered size. Player build, широка регресія й pacing поза цим кроком.
