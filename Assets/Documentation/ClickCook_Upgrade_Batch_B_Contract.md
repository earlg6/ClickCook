# Upgrade batch B — remaining manual and auto tiers, 2026-10-27

Owner authorized sustained upgrade implementation. Extends only existing bounded manual/Hold/start/periodic systems. Every tier has its own paid cap, catalog price and manifest predecessor max. Initial ClickCook balance; no pacing claim. Base tiers and legacy profiles retain their exact behavior when new ranks are zero.

| Node | Family | Cap | Parent (max) | Base price | Growth % | Step per rank |
|---|---|---:|---|---:|---:|---:|
| node.click_strength.02 | click_strength | 13 | node.click_strength.01 | 250000 | 160 | 10 |
| node.click_strength.03 | click_strength | 10 | node.click_strength.02 | 2500000 | 160 | 17 |
| node.click_strength.04 | click_strength | 5 | node.click_strength.03 | 25000000 | 180 | 80 |
| node.click_strength.05 | click_strength | 5 | node.click_strength.04 | 100000000 | 180 | 180 |
| node.click_strength.06 | click_strength | 5 | node.click_strength.05 | 500000000 | 180 | 400 |
| node.click_multiplier.01 | click_multiplier | 5 | node.click_strength.04 | 100000000 | 180 | 0.08 |
| node.click_multiplier.02 | click_multiplier | 5 | node.click_multiplier.01 | 250000000 | 180 | 0.12 |
| node.click_multiplier.03 | click_multiplier | 5 | node.click_multiplier.02 | 500000000 | 180 | 0.3 |
| node.click_multiplier.04 | click_multiplier | 5 | node.click_multiplier.03 | 1000000000 | 180 | 0.25 |
| node.critical_chance.02 | critical_chance | 5 | node.critical_chance.01 | 100000 | 180 | 0.02 |
| node.critical_chance.03 | critical_chance | 5 | node.critical_chance.02 | 1000000 | 180 | 0.02 |
| node.critical_multiplier.02 | critical_multiplier | 5 | node.critical_multiplier.01 | 250000 | 180 | 0.35 |
| node.critical_multiplier.03 | critical_multiplier | 5 | node.critical_multiplier.02 | 2500000 | 180 | 0.5 |
| node.double_strike_chance.03 | double_strike_chance | 5 | node.double_strike_chance.02 | 2000000 | 180 | 0.01 |
| node.double_strike_power.02 | double_strike_power | 10 | node.double_strike_power.01 | 1000000 | 180 | 0.05 |
| node.triple_strike_chance.02 | triple_strike_chance | 5 | node.triple_strike_chance.01 | 2000000 | 180 | 0.01 |
| node.hold_frequency.02 | hold_frequency | 10 | node.hold_frequency.01 | 100000 | 180 | 0.5 |
| node.auto_action_chance.02 | auto_action_chance | 5 | node.auto_action_chance.01 | 50000 | 180 | 0.02 |
| node.auto_action_chance.03 | auto_action_chance | 5 | node.auto_action_chance.02 | 250000 | 180 | 0.03 |
| node.auto_action_count.02 | auto_action_count | 5 | node.auto_action_count.01 | 250000 | 180 | 2 |
| node.auto_action_count.03 | auto_action_count | 5 | node.auto_action_count.02 | 2500000 | 180 | 2 |
| node.auto_frequency.02 | auto_frequency | 5 | node.auto_frequency.01 | 5000000 | 180 | 0.3 |
| node.auto_frequency.03 | auto_frequency | 5 | node.auto_frequency.02 | 25000000 | 180 | 0.45 |
| node.auto_frequency.04 | auto_frequency | 5 | node.auto_frequency.03 | 100000000 | 180 | 0.6 |
| node.auto_strength.02 | auto_strength | 5 | node.auto_strength.01 | 25000000 | 180 | 36 |
| node.auto_strength.03 | auto_strength | 5 | node.auto_strength.02 | 100000000 | 180 | 130 |
| node.auto_strength_multiplier.02 | auto_strength_multiplier | 10 | node.auto_strength_multiplier.01 | 1000000 | 180 | 0.1 |
Additive family steps sum. Probability steps are absolute probability (0.02=2 percentage points). Each click/auto multiplier tier contributes its own `(1 + step×rank)` factor, factors multiply. XP/value/discount behavior from batch A retained. Effective manual damage `ceil((legacy ClickDamageAtRank(first) + later flat strength) × product(click multiplier tiers))`; crit/extra-strike rounding then uses the existing manual pipeline. First Click Strength's legacy rounding is deliberately preserved. Later critical multipliers and extra-strike power add their steps to existing factors.

Max chance: critical 50%, double 35%, triple 15%; double+triple=50%, still one mutually exclusive bounded proc roll, ≤3 actual strikes with independent crits. Hold maximum 14 actions/sec. Auto max: start chance 45%, 31 actions/cycle, base power 846, auto multiplier 4, per-action damage 3384, batch 104904, frequency 10 cycles/sec. One batch/frame and no catch-up/carry/order auto-selection; pause/Back/focus/disable reset timers. These bounded maxima fit int safely. Shared one-receipt completion unchanged.

Schema 16 admits only these 27 new IDs (batch A IDs remain introduced at 15). Per-tier cap/prerequisites validated; earlier schemas reject new IDs even at zero, legacy state/fractions retained without eager writes. Effective discount pricing/self-purchase/discount-bound quote validation applies to every new tier. UI tier detail shows own contribution/factor and statistics show aggregates, including current→next base previews with later contributions. No new statistic lines needed; nine-line panel retained. All 92 authored tiles/graph unchanged.

Minimal verification: compilation; each new cap/price/purchase/prerequisite and shared schema checks; independent aggregate maxima/probability boundaries, manual crit/proc damage, auto/Hold rate/deadline/no-backlog/lifecycle/completion checks; old first-tier compatibility where shared boundaries changed; isolated live tier purchases/details/aggregate fit/autosave/Continue at current Full HD. No Player build/broad resolution regression.