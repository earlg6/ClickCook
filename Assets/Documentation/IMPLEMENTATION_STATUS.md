# ClickCook Implementation Status

**Поточний checkpoint — 2026-10-28: Upgrade batch B завершено.** Після batch A реалізовано ще 27 пізніх manual/Hold/auto вузлів: сила та множники кліку, crit, double/triple, Hold frequency, auto chance/count/strength/multiplier/frequency. Працюють 50 платних вузлів із 91 (54.9%; root окремо), повністю реалізовано ці manual/auto гілки. Це частка вузлів, не відсоток готовності всієї гри. Контракт: [Batch B](ClickCook_Upgrade_Batch_B_Contract.md). Schema 16 з backward read 1–15, власними caps/prerequisites/цінами та discount-aware quotes. Максимуми: manual 101078 HP, crit 50% ×7.75, mutually exclusive double35%/triple15%, Hold14/с, auto31×3384 HP ×10/с. Початковий баланс; pacing не доведено.

Локальні compilation/model/storage перевірки, незалежні maxima/RNG/proc/deadline/no-backlog/no-carry/receipt checks та суміжні старі manual/auto/Hold моделі пройдено. Native tier покупки й aggregate UI fit/autosave перевірено попередньою сесією; збережений Full-HD screenshot оглянуто (Temp/BatchB/tree.png). Попередня live controller completion дала payout11/XP276.75 один раз. Після втрати тимчасового Temp новим запуском повторено лише потрібний restart boundary: ізольовані покупки strengthVI/autoMultiplierII, wallet9524050000.25, damage80918/1862, дробова XP.37, restart/Continue відновлено точно. Новий Player build/широкі suites не потрібні. Play stopped, save override removed. Нижчі checkpoints історичні.

**Наступний roadmap-крок: 8A — підсумок дня**, сума фактичних receipts без повторної виплати, Next Day та Upgrades. U5 лишається частковим: recipe/passive/Player/Restaurant XP залежать від власних базових milestones; U6 quality потребує окремого числового контракту.


**Upgrade batch A виконано — 2026-10-27.** За дорученням власника продовжено апгрейди пакетами: знижка на апгрейди (до 5%), Dish XP flat II/III (+5/+14 за ранг), Dish Value II–IV (+4/+5/+6% за ранг). [Контракт](ClickCook_Upgrade_Batch_A_Contract.md). Разом 6 нових вузлів, власні caps/prices/prerequisites, агрегати flat XP до 125 та value factor до 2.20. Ціни всіх working nodes і UI враховують discount; quotes прив'язані до discount rank. Schema 15 читає 1–14. Compilation, targeted purchase/aggregate/storage та value-zero compatibility, native discount purchase/autosave, UI prices, restart/Continue і discounted strength purchase 213→211 пройдено. Full-HD discount screenshot: Temp/BatchA/discount.png. Пізні manual/auto tiers — наступний пакет, ще не завершені цим checkpoint. Build/широка регресія/pacing не виконувались.


## Current checkpoint — U5c first Dish Value multiplier — completed 2026-10-27

Implemented `node.dish_value.01` / Вигідне меню after purchased Click Strength 1. Fifteen ranks, factor `1 + 0.03N` (×1–×1.45), existing catalog `ceil(100 × 1.7^N)`, total 408779. Initial ClickCook balance, not pacing proof. Contract: ClickCook_U5c_Dish_Value_Contract.md. No authored DishData or catalog prices changed.

Shared CompleteOrder passes the profile's value factor to the receipt boundary. Payout is decimal `Round(SaleValue × qualityMultiplier × valueFactor, 2, AwayFromZero)` with one final cent rounding; rank-zero identity retains prior payouts. Manual/Hold/start/periodic completion use this same one-receipt branch. XP/fractions, pre-reward rarity, quality probabilities, damage and scheduling unchanged. Mastered dishes still pay; partial work/Back/end do not. Dependent upgrade/recipe discounts, passive unlock and tier-II value remain unavailable despite visible prerequisite progress.

Schema 14 adds only canonical dish_value.01 with cap 15/purchased prerequisite. Schemas 1–13 reject its ID even at zero; legacy money/whole XP/hundredths/all ranks/backups retain exact values without eager rewrite or free value. >14 files protected. Fraction validation/representation inherited from schema 13. UI shows factor/current→next, actual receipt/wallet and an eighth active-statistics line; statistics grows 24 px and scrollable detail begins 24 px higher.

Files: UpgradeNodeRules.cs, PrototypeOrderSession.cs, PrototypeDishRewards.cs, PlayerProfileStore.cs, UpgradeTreeView.cs; new DishValueVerification.cs and contract (+ Unity metadata). Existing DishXpFlat/Multiplier live-stat line-count expectations updated to eight, multiplier storage expectations follow CurrentVersion. Existing worktree edits preserved; no intentional scene, food, catalog or project-settings edits.

Scoped verification per AGENTS.md:

- Compilation passed. RunModelChecks passed explicit fifteen-price/cap/total/prerequisite/funds/pause/day/owner/replay guards. Three integer base prices (1, 5, 37), all five qualities and ranks 0–15 matched an independent integer rational cent oracle. Explicit 1.325→1.33 and 6.4375→6.44 examples verified final rounding/positive midpoints. Receipt, wallet cents, one notification, XP/fraction independence, old-level rarity, stale-result guards and cross-dish XP isolation passed. Hold/start/periodic sources matched the manual payout path; mastery-cap dishes still paid and partial/Back/end did not. DishXpMultiplierVerification.RunModelChecks passed at value zero for the changed shared receipt boundary. Unrelated broad evidence reused.
- RunStorageChecks passed schemas 1–13→14 with exact legacy money/whole XP/fractions/ranks/no eager rewrite/free factor, old backup recovery, rank-1/15 independent fractional XP + cent-wallet idle roundtrip, invalid cap/prerequisite/duplicate/tier/alias rejection and version 15 protection. Unchanged schema-13 fraction/reset validation reused prior evidence.
- Isolated `Temp/ClickCookSaveTests/U5c-e6f08404d63d44b98fc41b2caf1e3017/profile.json`: native ComputerUse mouse bought ranks 1/2 for 100/170, wallet 30.25, factor ×1.06 and immediate disk save. UI rank 2/15, ×1.06→×1.09/funds lock and eight-stat panel fit at actual Full HD. Tree/result screenshots reviewed and retained at Temp/U5c/tree.png and result.png. Advancing frames confirmed; native Resume handled focus-loss pause.
- Real controller Uncommon completion with base 5 and factor 1.06 computed 6.625→6.63 payout, wallet 36.88, independently 20.52 XP and level 1→2, one result and no stale repayment. Result text showed actual money/wallet; autosave held exact cents and whole 20 + 52 hundredths, other dish XP unchanged at 17. Restart/Continue restored all seventeen node entries, value rank 2/factor 1.06, XP factor 1.14, wallet 36.88, XP 20.52 and idle day/Hold/feedback.

Console contained only existing Unity VCS missing-client configuration exceptions, no ClickCook runtime exception. Play stopped, fixture clones and profile override removed, original runInBackground=false restored; scene clean and Game View layout/preset unchanged. User profile not used. No Player build, new native keyboard pass, broad regression/resolution matrix or pacing claim.

Next separate slice: **U5d — first node.upgrade_discount.01**, with its own base-price/discount stacking/rounding/quote/self-purchase/save contract. U5 remains partial; recipe discounts/passive and Player/Restaurant XP need their base milestones.


## Current checkpoint — U5b first Dish XP multiplier / fractional mastery — completed 2026-10-27

Work resumed from the implemented but unverified U5b runtime/storage/UI changes and new verification helper. Completed and verified `node.dish_xp.01` / Швидке навчання рецепта after purchased flat XP 1: fifteen ranks, factor `1 + 0.07N` (×1–×2.05), existing catalog prices `ceil(100 × 1.7^N)`, total 408779. Initial ClickCook balance, not pacing evidence. Contract: ClickCook_U5b_Dish_Xp_Multiplier_Contract.md (adopted 2026-10-26; verified 2026-10-27).

Shared reward receipt calculates `min((base rarity XP + flat) × factor, exact remaining mastery XP)` using decimal. Rarity is chosen from the pre-reward level and payout remains unchanged. Per-dish fractional hundredths accumulate without award rounding and survive save/load/Back/end. Whole-only GetTotalXp/GetLevelXp remain for compatibility; exact accessors include hundredths. Receipt XpGained is decimal. Result and board text/bar use exact XP, formatted 0.##. Mastery level 18 clamps total and clears remainder; receipt reports actual fractional XP near the cap. Other dish IDs remain isolated.

Schema 13 adds only canonical multiplier rank 0–15 with purchased-flat prerequisite and integer hundredths 0–99 per dish (missing=0). Schemas 1–12 reject multiplier IDs even at rank zero and reject nonzero hundredths; old profiles/backups retain exact wallet/integer XP/ranks with no eager rewrite or free multiplier. Fraction at mastery cap is invalid, >13 protected. Snapshot separately copies/read-only wraps fraction values. New Game/reset clears primary/backup fractions. No Player/Restaurant XP added; all later effects remain unavailable.

Files: UpgradeNodeRules.cs, PrototypeOrderSession.cs, PrototypeDishRewards.cs, PlayerProfile.cs, PlayerProfileSnapshot.cs, PlayerProfileStore.cs, ClickCookPrototypeController.cs, UpgradeTreeView.cs; new DishXpMultiplierVerification.cs and contract (+ metadata). Existing dish_xp catalog entry retained unchanged. DishXpFlatVerification future-node/storage-version expectations updated. Preexisting worktree edits preserved; no intentional scene, food or project-settings edit.

Scoped verification per AGENTS.md:

- Unity reported scripts up-to-date and compilationFailed=false. RunModelChecks passed explicit fifteen-price oracle/cap/total/prerequisite/funds/pause/day/owner/replay guards. All five rarities at ranks 0–15 matched independent rational XP oracles including preexisting .37 remainder and unchanged payout/old-level rarity. Flat-before-factor, no stale reward/cross-dish XP, Hold/start/periodic source parity, repeated fraction carry, exact level progress, multi-level crossing and near-cap actual 1/0.01 XP plus zero-at-cap passed. Back/end preserved fractions without rewards.
- RunStorageChecks passed schemas 1–12→13 preserving exact legacy wallet/two-dish XP/all ranks/no eager write/free fraction/multiplier, old backup recovery; independent rank-1/15 fractional roundtrip, immutable snapshot, current fractional backup recovery, New Game primary/backup reset, invalid node/cap/prerequisite/duplicate/tier/alias/fraction bounds/cap remainder and version 14 protection. DishXpFlatVerification.RunModelChecks passed for multiplier-zero compatibility at the shared receipt boundary. Unrelated broad gameplay/input/resolution evidence reused.
- Isolated `Temp/ClickCookSaveTests/U5b-dd650bb5592d47a7ba66a603a0d4ec6c/profile.json`: native ComputerUse mouse bought ranks 1/2 for 100/170, exact wallet 30.25, factor 1.14, no purchase XP and immediate disk save. Full-HD detail rank 2/15, ×1.14→×1.21, funds lock and seven-line flat/factor statistics fit. Current tree/result screenshots reviewed and retained at Temp/U5b/tree.png and result.png. Advancing frames confirmed.
- Actual controller Common completion awarded `(10 + 6) × 1.14 = 18.24 XP`, one payout/wallet 35.25, actual result/mastery text and fill, stale-action guard and fractional disk autosave passed. Restart/Continue restored all sixteen ranks, wallet 35.25, dish-1 whole 18 + hundredths 24, other dish XP 17, and idle day/Hold/feedback. A second real controller completion after Continue accumulated exactly 36.48 XP (whole 36 + 48 hundredths), wallet 40.25 and autosaved. An active board fixture then confirmed visible level-2 text `16,48 / 24 XP` after frame advancement.
- The first additional board probe required active cards immediately after the result transition; the texts already held the right fractional value but were inactive. Set up the same XP on an active board and waited for frames, confirming visible exact text. This was a probe timing/state assumption; no production change was needed. Native Resume handled focus-loss pause during setup.

Console review found only the existing Unity VCS missing-client configuration exceptions, no product runtime exception. Play stopped, transient fixture clones and save override removed, original runInBackground=false restored; scene clean, Game View layout/preset unchanged. User profile was not used. No new native keyboard pass, Player build, broad regression/resolution suite or economy/pacing claim.

Next separate slice: **U5c — first node.dish_value.01 (Вигідне меню)**, with its own payout multiplier/order/rounding/prices/save contract before sale. U5 remains partial; later XP tiers and recipe/Player/Restaurant systems are not marked complete.


## Current checkpoint — U5a first flat Dish XP — 2026-10-26

Implemented `node.dish_xp_flat.01` / Нотатки до рецепта after purchased Click Strength 1: five ranks, +3 XP/rank (0–15), `ceil(100 × 1.7^N)` with prices 100/170/289/492/836, total 1887. Initial ClickCook balance, not pacing proof. Contract: ClickCook_U5a_Dish_Xp_Flat_Contract.md. Manifest prerequisite makes flat the first Dish XP slice; multiplier is next, not enabled by this checkpoint.

Shared CompleteOrder passes the profile's current flat bonus into PrototypeDishRewards.Complete. Receipt XP is min(base rarity XP + flat bonus, remaining mastery XP). Rarity uses the pre-reward level, payout/probabilities/mastery thresholds unchanged. Manual/Hold/auto-start/periodic completion receive the same bonus once/order, never per hit/action/tick. Receipt reports actual XP, including zero at mastery cap; other dish IDs retain their XP. No XP granted for purchasing the node, unfinished work or Back. Flat XP is integral; multiplier/fractional semantics deferred.

Schema 12 adds only canonical flat.01 cap 5 and purchased prerequisite; schemas 1–11 reject it even at zero. Existing wallet/XP/all ranks/backups retain exact values with no eager Load write/free bonus; >12 protected. No extra XP fields or Player/Restaurant XP added. Detail shows purchased bonus/current→next/price/cap; result uses existing XpGained binding. Actual statistics has seven lines; its panel grows 24 px and scrollable detail begins 24 px higher, preserving separation.

Files: UpgradeNodeRules.cs, PrototypeOrderSession.cs, PrototypeDishRewards.cs, PlayerProfileStore.cs, UpgradeTreeView.cs; UpgradeCatalog.asset appended through Unity SerializedObject; new DishXpFlatVerification.cs and contract (+ Unity-generated metadata). Previous AutoFrequencyVerification storage version expectations follow CurrentVersion. Existing worktree changes preserved; no intentional scene/food/project-settings edits.

Scoped verification per AGENTS.md:

- Compilation passed. RunModelChecks passed exact prices/cap/total, prerequisite/funds/pause/day/owner/replay guards and future-effect unavailability. Seeded manual receipts covered all five rarities at every flat rank 0–5 with explicit base XP/payout oracles, pre-reward rarity level, exact XP addition, one profile notification and no cross-dish XP. Mastery near-cap/cap receipt clamping and level 1→3 crossing passed. Hold/start/periodic completion each gave the same +6 bonus once; stale manual/start/periodic events could not repeat rewards. Partial work/Back/end gave no XP. AutoFrequencyVerification.RunModelChecks passed at flat zero for the changed shared reward boundary; unrelated broad evidence reused.
- RunStorageChecks passed schemas 1–11→12 preserving exact wallet/two-dish XP/all ranks/no eager write/free bonus, old backup recovery, independent ranks 1/5 idle roundtrips, invalid cap/prerequisite/duplicate/tier/alias rejection and version 13 protection.
- Isolated `Temp/ClickCookSaveTests/U5a-1aa9a32082c645fd8e3fbb855b08ff4b/profile.json`: native ComputerUse mouse bought ranks 1/2 for 100/170 with exact wallet 30.25, bonus +6 and immediate disk save; XP remained zero at purchase. Full-HD tree reviewed with rank 2/5, +6→+9 XP, next price 289/funds lock and seven actual-stat lines fitting. Result screenshot reviewed with actual +16 XP (10 Common +6 bonus), level 1/16 of 20 XP and wallet 35.25. Screenshot artifacts retained at Temp/U5a/tree.png and result.png.
- Controller manual completion/result/autosave passed with one receipt, actual dish-1 XP 16, dish-2 XP unchanged at 17 and stale input blocked. Restart/Continue restored fifteen node entries, exact wallet 35.25, dish XP 16/17, flat rank 2/bonus 6, prior auto stats and idle day/Hold/feedback. Advancing frames confirmed. Play stopped, save override removed, original runInBackground=false restored; layout/preset unchanged.

Test-helper recovery: the initial live fixture supplied start-roll 1 outside [0,1); corrected the helper to valid miss .99. While diagnosing that fixture, periodic auto completed its active order and saved XP 16. Read-only inspection confirmed the temporary profile's changed state; reset only that isolated profile to the already-proven rank-2 purchase checkpoint, then reran the dependent controller/result phase successfully. Native purchase/model/storage evidence reused. Console contained only known Unity VCS configuration exceptions; eval test failures were isolated to the fixture. No new native keyboard pass, Player build, broad regression/resolution matrix or economy/pacing claim.

Next separate slice: **U5b — first node.dish_xp.01 multiplier**, after specifying factor, flat/multiplier order and fractional XP retention/rounding. U5 remains partial; higher flat tiers and other XP/economy branches stay unavailable.


## Current checkpoint — U4e first auto frequency — 2026-10-26

Implemented `node.auto_frequency.01` / Темп помічника I after purchased auto_action_count.01 rank 1. Existing catalog retained: fifteen ranks, `ceil(600 × 1.8^N)`, total 5059237. Effective periodic rate `1 + 0.15N` gives 1–3.25 cycles/sec; exact interval is its reciprocal, minimum 4/13 sec. Actual rate is zero when periodic auto is disabled. Contract: ClickCook_U4e_Auto_Frequency_Contract.md. This is initial ClickCook balance, not a source cap or pacing claim.

TryCookAuto uses the effective interval for first binding and every reschedule; one bounded batch/frame with no catch-up. Existing pause/focus/system-pause/disable/Back/result/end/order-reset behavior restarts a full interval. Start chance/count/damage/multiplier/manual/Hold unchanged. Detail/current→next, count preview and six-line actual statistics show real cycles/sec. Schema 11 adds only canonical frequency.01 with cap 15 and purchased prerequisite; old schemas 1–10 reject its ID even at zero. Old wallet/XP/ranks/backups load unchanged without eager writes/free speed; >11 files protected, scheduling/day/input/feedback transient.

Files: UpgradeNodeRules.cs, PrototypeOrderSession.cs, PlayerProfileStore.cs, UpgradeTreeView.cs; new AutoFrequencyVerification.cs and contract (+ Unity-generated metadata). AutoCooking/AutoStrength/AutoMultiplier future-frequency assertions moved to tier II; multiplier storage expectations follow CurrentVersion. Catalog, scene, food and project settings not intentionally edited; existing worktree changes preserved.

Scoped verification per AGENTS.md:

- Compilation passed. RunModelChecks passed fifteen explicit prices/cap/total/purchase guards, independent rational rate/interval oracle, early/exact first deadlines and reschedule deadlines at all ranks, one delayed batch/no backlog, zero-rank one-second compatibility and disabled-auto gate. Pause/same-frame pause-resume/suspend/invalid-clock/Back/reselect/end resets passed at the fastest interval. Start/manual critical triple/Hold stats remained independent, Back did not reroll. Periodic early lethal/exact/overshoot completion gave one receipt/XP/payout/notification, with stale-result/no-carry guards. AutoCookingVerification.RunModelChecks passed at frequency zero for the changed scheduler boundary; unrelated broad tests reused prior evidence.
- RunStorageChecks passed schemas 1–10→11 with exact old money/XP/all ranks/no eager write/free speed, old backup recovery, ranks 1/15 independent idle roundtrip, invalid cap/prerequisite/duplicate/tier/alias rejection and version 12 protection.
- Isolated `Temp/ClickCookSaveTests/U4e-7dd284f71fb8488eace02bd44dea3ae6/profile.json`: native ComputerUse mouse bought frequency ranks 1/2 for 600/1080, exact wallet 320.25, real rates 1.15/1.3 and immediate disk save. Rank 2/15 preview showed 1.3→1.45, next 1944/funds lock; count detail and six-line stats fit. Actual Full-HD tree screenshot reviewed, retained at Temp/U4e/tree.png; layout/preset unchanged.
- Advancing-frame check passed. Controller start reduced 1000→992 without changing the batch. Live timer probe recorded now 32.7289844, next deadline 33.4982152 (10/13 sec); real controller ticks reduced HP to 912 with 2 actions/raw damage 8. Pause held HP 912 from unscaled time 41.2326703 to 48.8492602. Resume resumed periodic damage. Separate HP-13 fixture received start damage 8, then a real periodic batch completed the remaining 5 HP with 2 actions/raw damage 8, one result/autosave and no stale repayment.
- Restart/Continue restored fourteen node entries, exact post-receipt wallet/XP, frequency 2/rate 1.3, strength 2/multiplier 2/damage 4 and idle day/Hold/feedback. Play stopped, override/expected keys removed; original runInBackground=false restored. User profile not used.

Tool recovery: a multi-file patch retry duplicated adjacent registry additions; removed those duplicate additions before compilation. Screenshot capture rejected a parent-relative path; captured under the supported Assets path and moved the owned PNG to project Temp. Console separated that Pipeline validation error and known Unity VCS configuration exceptions from product runtime errors; no ClickCook runtime exception observed. Native Resume handled focus-loss pause during UI setup. No new native keyboard pass, Player build, broad regression/resolution matrix or economy/pacing claim.

U4a–e now implement the first nodes of all five base auto families. Later auto tiers remain unavailable and must not be counted as finished. Next chapter: **U5**, starting with an independently usable first Dish XP upgrade after its numeric/effect/receipt/save contract. Character/Restaurant XP and recipe/economy effects still depend on their base milestones.


## Current checkpoint — U4d first auto strength multiplier — 2026-10-26

Implemented `node.auto_strength_multiplier.01` / Школа су-шефа I after purchased auto_strength.01 rank 1: ten independent ranks, factor `1 + 0.1N`, price `ceil(1000 × 1.8^N)`, total 445062. Initial ClickCook balance, not source reward or pacing evidence. Contract: ClickCook_U4d_Auto_Multiplier_Contract.md. Per-action damage is decimal `ceil((1 + strengthRank) × factor)`, snapshotted by the existing shared start/periodic batch. Rank-zero identity, manual/crit/proc, action count, start chance, full-second interval and lifecycle rules retained. Maximum current batch is eleven actions × 32 HP = 352. Detail shows factor and actual current→next HP including plateaus; strength preview also uses the current multiplier; count/chance details and six-line actual statistics show effective damage.

Schema 10 adds only canonical multiplier.01 with its cap and purchased prerequisite; versions 1–9 reject the new ID even at rank zero, retain exact old money/XP/all ranks/backups and do not rewrite on Load or give free multipliers. >10 files protected. Transient day/order/timers/input/feedback are excluded. Multiplier II, frequency and later tiers remain unpurchasable.

Files: UpgradeNodeRules.cs, PrototypeOrderSession.cs, PlayerProfileStore.cs, UpgradeTreeView.cs; UpgradeCatalog.asset appended through Unity SerializedObject; new AutoMultiplierVerification.cs and contract (+ Unity-generated metadata). Previous AutoStrengthVerification's future-node assertion now refers to multiplier II; storage expectations follow CurrentVersion. Preexisting worktree changes preserved; no intentional scene, authored food or project-settings edits.

Scoped verification per AGENTS.md:

- Compilation passed. RunModelChecks passed explicit ten-price oracle/cap/total/debit/owner/replay/pause/day/funds guards, independent integer rational oracle across strength 1–15 and multiplier 0–10, identical start/periodic damage, full interval, unchanged manual triple/critical damage, Back no reroll, early lethal/exact/overshoot cases, one receipt/XP/payout/notification, stale-result/no-carry guards and maximum batch. Throwing manual RNG providers proved auto isolation. AutoStrengthVerification.RunModelChecks passed for the changed damage boundary with multiplier zero. Unchanged broad tree/input/lifecycle evidence reused.
- RunStorageChecks passed schemas 1–9→10, exact legacy wallet/XP/ranks/no eager write/free factor, old backup recovery, rank 1/10 independent idle roundtrip, invalid bounds/prerequisite/duplicate/alias/tier rejection and version 11 protection.
- Isolated `Temp/ClickCookSaveTests/U4d-51a35dc5ecd84b5598be7a6420ee0e1c/profile.json`: native ComputerUse mouse bought ranks 1 and 2 for 1000/1800, exact wallet 1200.25 and immediate disk saves. Rank 2/10, ×1.2→×1.3, 4→4 HP, next 3240/funds lock were visually inspected at actual 1920×1080. Strength/count/chance previews and six-line statistics fit. Screenshot retained at Temp/U4d/tree.png.
- Actual controller start reduced 1000→992; advancing-frame evidence and a real periodic observation showed HP 952, damage 8 and two actions. HP 5 start used two actions/raw damage 8, showed result and paid/saved once; stale input did not repay. Restart/Continue restored thirteen node entries, exact post-receipt wallet/XP, strength 2/multiplier 2/damage 4 and idle day/Hold/feedback. Play stopped, test override/expected keys removed; original runInBackground=false restored. Game View layout/preset unchanged.

The first UI helper expected a dot decimal separator; Ukrainian UI correctly used a comma. Corrected only the test expectation to current culture, recompiled and resumed the dependent live phase from the saved rank-2 fixture; purchase/model/storage evidence reused. A socket-hangup after recompilation was recovered by status inspection and a confirmed Play entry. Console contained only preexisting Unity VCS configuration exceptions, no ClickCook runtime exception. No new native keyboard pass, Player build, broad resolution/regression suite or full economy/pacing claim.

Next separate slice: **U4e — first node.auto_frequency.01**, with explicit numeric/interval/clamping/scheduler/save contract before sale. U4 remains partial.


## Current checkpoint — U4c first auto strength — 2026-10-26

Implemented node.auto_strength.01 / Навчання бригади I after auto_action_count.01 rank 1: fifteen ranks, +1 HP/action from baseline 1 to 16, existing catalog ceil(500 × 2^N). Contract: ClickCook_U4c_Auto_Strength_Contract.md. ApplyAutoBatch snapshots current count and strength for both first-entry and periodic sources. Per-action clamp, early lethal exit and one receipt remain shared; manual damage/RNG, count, start chance and interval are unchanged. Details for strength/count/chance and active statistics use real damage. Schema 9 accepts canonical strength ranks with prerequisite, reads 1–8 preserving exact legacy wallet/XP/ranks/backups without eager rewrite/free strength, and protects >9. Later strength tiers/multiplier/frequency unavailable.

Changed files: UpgradeNodeRules.cs, PrototypeOrderSession.cs, PlayerProfileStore.cs, UpgradeTreeView.cs; new AutoStrengthVerification.cs and contract (+ metas). Existing AutoCookingVerification future-node check and AutoStartVerification schema expectations updated. Catalog/scene/project settings untouched by this slice; preexisting worktree changes preserved.

Scoped verification per AGENTS.md:

- Compilation passed. RunModelChecks passed fifteen known prices/cap/purchase guards; identical start/periodic damage, independent manual triple/chance/count/timer, overshoot HP 1/3/4/6/7, single receipt/XP/notification, no carry/stale reward, maximum eleven actions × sixteen damage. AutoStartVerification.RunModelChecks passed on the affected shared batch boundary. Unchanged checks reused.
- RunStorageChecks passed schemas 1–8→9 with exact old state/backups and no eager rewrite/free strength; strength 1/15 idle roundtrips; invalid bounds/prerequisite/duplicate/alias/tier rejection and future version 10 protection.
- Unity MCP + ComputerUse on an isolated Temp/ClickCookSaveTests/U4c profile: native mouse bought ranks 1 and 2 for 500/1000, each single debit and immediate disk save (wallet 1000.25). Preview showed rank 2/15, 3→4 HP and next price 2000 with funds lock. Count/chance detail and six statistics lines reflected 2 × 3 HP and fit. Actual controller first-entry reduced 1000→994, real LateUpdate observation changed 964→958; lethal HP 4 used two actions/raw damage 6, one result/payout and autosave. Native Enter/Space did not activate the selected Buy through ComputerUse; held-key input is unsupported on Windows, so keyboard purchase remains unconfirmed. Native screen reviewed at existing Full-HD preset; no broad resolution pass.
- Restart/Continue restored twelve node entries, exact post-receipt wallet/XP, strength 2/damage 3 and idle state. Play stopped and test override/expected keys removed. User profile was not used; Game View layout/preset unchanged during this verification.

Initial checks after the unsuccessful keyboard attempt failed their purchase precondition; inspection showed rank 1 unchanged. Only the dependent live phase was resumed after the second mouse purchase passed. Existing Unity VCS configuration exception is unrelated; no Player build, broad regression or economy pacing claim. Next separate slice: U4d first auto_strength_multiplier, then frequency; U4 remains partial.

## Current checkpoint — U4b first-entry auto chance — 2026-10-25

Implemented first `node.auto_action_chance.01` / Пильний помічник I: Strength 2 prerequisite, ten independent ranks, +2 percentage points/rank to 20%, ceil(250 × 1.8^N). Initial ClickCook balance, not source rewards or pacing evidence. Contract: ClickCook_U4b_Auto_Start_Contract.md. Chance does not unlock periodic auto; without count a successful start applies the baseline one 1-HP action, with count it uses the current 1+N batch. Later chance tiers/auto strength/multiplier/frequency unavailable.

PrototypeOrder owns transient AutoStartChecked. TryCookAutoStart accepts only the active unpaused order/day, marks the first visit before rolling (including miss/zero), consumes no RNG at zero chance, and never rerolls a checked order after Back/pause/timer reset. Separate injected start-roll provider preserves existing manual/proc constructors. Controller invokes start after accepting selection/activating cooking, then drives the existing result handler immediately if lethal. A shared ApplyAutoBatch serves start and periodic sources with no manual combat RNG, per-action HP clamp, bounded actions and one shared receipt/notification. The periodic full-second deadline remains independent. Existing Back resets HP but preserves the order's consumed start chance; different/replacement order identities start fresh.

Detail shows actual chance/batch/first-entry rule, statistics distinguish disabled periodic cycle from an active start chance in the same six lines. Schema 8 adds only canonical auto_action_chance.01 rank 0–10 with purchased prerequisite, rejects it in schemas 1–7 even at rank 0, preserves exact legacy wallet/XP/ranks and backups without eager write/free chance, protects >8. Order/checked/timer state is excluded from profiles; restart retains the existing unfinished-day reset rule.

Files: UpgradeNodeRules.cs, PrototypeOrder.cs, PrototypeOrderSession.cs, ClickCookPrototypeController.cs, PlayerProfileStore.cs, UpgradeTreeView.cs; new AutoStartVerification.cs and contract (+ metas); catalog authored via Unity SerializedObject; prior AutoCookingVerification availability/current-version expectations updated. Existing worktree changes preserved; no scene, authored food or project settings edits.

Scoped verification per AGENTS.md:

- Compilation completed without errors. AutoStartVerification.RunModelChecks passed ten known prices/cap, independent count/chance ownership, prerequisites/owner/replay/pause/day/funds guards, strict 2%/20% thresholds, zero-chance no RNG, hit/miss/zero Back suppression, invalid/paused/foreign calls preserving first-visit eligibility, and same-dish distinct order eligibility. Throwing manual RNG providers proved plain start batches. Standalone/upgraded batches, independent periodic deadline, immediate bounded completion/one receipt/XP/notification, stale-result guards and fresh replacement eligibility passed. AutoCookingVerification.RunModelChecks passed on the changed shared auto-batch boundary; unchanged manual proc/tree/hold checks reused prior evidence.
- RunStorageChecks passed schema 1–7→8 retaining exact old money/XP/all ranks with no eager rewrite/free chance; old backup recovery; rank 1/10 roundtrips idle; bounds/prerequisite/duplicate/future tier/alias rejection and future version 9 protection.
- Isolated Temp/ClickCookSaveTests/U4b profile: chance button spent 250 once despite duplicate invoke, autosaved rank 1 without periodic unlock; next preview 450/2→4% and standalone cycle-off/start-2% statistics fit. Controller first selection applied one HP immediately; Back restored HP without another roll, other order rolled independently. A 1.2-second observation confirmed standalone HP unchanged. Buying existing action_count for 500 retained chance and made the first batch two HP; actual later LateUpdate changed 998→996 without a new start roll. Lethal 1-HP selection directly showed result/cooking hidden, bounded to one action with one payout and autosaved both nodes. Combined statistics fit; maximum-value line measured 346px within 426px. Actual Full-HD screenshot reviewed at Temp/U4b/final.png.
- Restart/Continue restored eleven node entries and exact post-receipt wallet/XP with no active day/Hold/auto feedback. Original Game View preset restored before restart; Play stopped and temporary override/expected-value keys removed.

The MCP tool catalog did not refresh after detecting the open Editor, so the current official Unity CLI (1.0.0-beta.14, self-update check confirmed current) controlled the same running Editor. The sandbox denied the Store executable; scoped escalated CLI execution succeeded. An unfocused periodic observation correctly stopped at the existing focus guard; focusing/resuming and observing in one CLI session passed. Domain-reload reconnect and existing Unity VCS/CLI diagnostics were separated from product runtime errors; no ClickCook runtime exception found. No physical input pass, Player build, broad regression/resolution suite or full economy balance claim. Next separate slice: U4c first auto_strength; multiplier/frequency later, U4 remains partial.

## Current checkpoint — U4a periodic auto foundation — 2026-10-24

Implemented first `node.auto_action_count.01` / Вправний помічник I: requires Strength 2, ten ranks, ceil(500 × 1.8^N). Rank 0 periodic auto disabled; rank 1 enables one cycle/sec with two plain 1-HP actions, each later rank adds an action, cap eleven. Initial ClickCook balance, not source rewards/pacing proof. Contract: ClickCook_U4a_Auto_Contract.md. Chance/strength/multiplier/frequency and later count tiers remain unavailable; U4 is only partially implemented.

Session TryCookAuto binds the selected order on the first active cooking frame, waits a full unscaled second, applies at most one bounded batch/frame and schedules from now without catch-up. Auto consumes no critical/double/triple RNG and no manual damage modifiers. Each action clamps HP and stops on completion. Manual/auto share a new CompleteOrder helper containing the prior receipt/XP/payout/notification/replacement logic, once/order; no automatic next selection or carried actions. A manual/Hold completion suppresses the later auto call in the same controller LateUpdate. Pause (including same-frame resume), focus/system pause, disable, Back/result/end/order changes suspend binding/deadline; active cooking resumes with a fresh interval. Existing ProfileMenu additionally pauses on focus loss, requiring Resume.

Controller LateUpdate runs guarded auto after Hold; shared HandleCookingOutcome drives the existing result screen for either source. Auto HP feedback identifies the helper, actual action count and batch damage; subsequent manual feedback resets auto flags. Six actual-stat lines use 20-point text in the existing panel and include cycle actions/strength/rate. Schema 7 adds only canonical action_count.01 rank 0–10 with purchased prerequisite; schemas 1–6 reject it even at rank 0. Old files/backups retain exact wallet/XP/all prior ranks without eager writes or free auto, >7 protected; timers/input/day/feedback transient.

Files: UpgradeNodeRules.cs, PrototypeOrderSession.cs, ClickCookPrototypeController.cs, PlayerProfileStore.cs, UpgradeTreeView.cs; new AutoCookingVerification.cs and contract (+ metas); catalog entry authored through Unity SerializedObject; preceding triple storage expectations made current-version aware. Existing worktree edits preserved; no scene/authored food/project settings edits in this slice.

Local verification per AGENTS.md:

- Compilation passed. AutoCookingVerification.RunModelChecks passed ten known prices/activation/cap/debit/replay/owner/funds/pause/day guards, disabled rank 0, fresh/exact 1-second deadlines, one delayed batch/no backlog, same-frame pause/resume and explicit suspension/reset, Back/reselect/end/invalid clock. Throwing combat RNG providers proved auto has no manual rolls even with all proc upgrades owned. Mixed manual triple/auto/manual feedback and damage remained independent. Lethal/partial batches verified bounded actual actions, one profile notification/receipt/XP/payout, no stale reward/carry, manual completion before due auto and ten-completion day cap. TripleStrikeVerification.RunModelChecks passed for the affected shared manual completion boundary. Other unchanged manual math/tree/hold evidence reused.
- RunStorageChecks passed schema 1–6→7 retaining exact old money/XP/ranks without rewrite/free auto; old backup recovery; rank 1/10 roundtrips idle; invalid cap/prerequisite/duplicate/unknown tier/alias rejection and future-version-8 protection.
- Isolated Temp/ClickCookSaveTests/U4a profile: Buy spent 500 once despite repeated invoke, autosaved rank 1; next price 900, six statistics lines and auto feedback fit. Live controller LateUpdate changed HP by two without input/combat RNG. A 1.2-second observation while paused left HP unchanged. Resume/focus/system-pause/controller-disable checks confirmed fresh deadlines. Actual next auto cycle completed the armed 1-HP order, showed the result, gave one receipt and autosaved. Restart/Continue restored ten node entries and exact post-receipt wallet/XP, no day/Hold/auto feedback. Full-HD screenshot reviewed at Temp/U4a/final.png. Test override removed, Play stopped, Game View preset restored and verified temporary preset removed.

The initial live helper incorrectly assumed focus return automatically resumes the existing ProfileMenu; it was corrected to call Resume, then only the affected live phase was repeated. Purchase/model/storage evidence reused. Pipeline timeout/reconnect and multiple Game View handling required tool recovery; rendered dimensions and advancing frames were confirmed. Console distinguishes existing Unity VCS/tool diagnostics from product exceptions. No physical input pass, Player build, broad resolution/regression suite or full economy/pacing claim. Next separate slice: U4b auto_action_chance.01, with one first-cooking roll per order (Back cannot reroll) under manifest rules; periodic strength/multiplier/frequency later.

## Current checkpoint — U3b tier II and triple — 2026-10-24

Implemented `node.double_strike_chance.02` / Другий рух II: own five ranks after first chance's ten purchased ranks, +2 percentage points/rank (total double cap 30%), ceil(50000 × 1.6^N). Implemented `node.triple_strike_chance.01` / Потрійний рух I: ten ranks after II rank 1, +1 percentage point/rank (triple cap 10%), ceil(10000 × 1.8^N). Adopted initial ClickCook balance, not source rewards or pacing proof. Contract: ClickCook_U3b_Triple_Strike_Contract.md. First chance rank/cap/price preserved; later double III/power II/triple II remain unavailable.

One proc roll after a surviving primary selects exactly one outcome: u<d double, d≤u<d+t triple, otherwise single. P(2)=d and P(3)=t, maximum total proc .40; thresholds computed from integer percentage points before division, preserving the old distribution at triple 0. A bounded loop applies at most two additional strikes with the existing q/single-ceiling/independent-crit pipeline. Lethal primary suppresses proc; lethal second suppresses third/its crit; no recursion/carry or duplicate receipt. LastExtraStrikeDamage aggregates actual extras, LastStrikeCount records actual 1–3; feedback shows actual strike count, statistics show both probabilities.

UpgradeCatalog.FindForNode and explicit registry CatalogId give tier II its own canonical price entry/cap rather than the first family definition. Session quote, tree detail and tile all share that lookup; quote checks registry/catalog cap consistency. Schema 6 adds only the new II/triple IDs with independent bounds/purchased prerequisites, rejects them in schemas 1–5 even at rank 0, reads old backups/profiles without eager rewrite or free ranks, protects >6 and excludes transient input/feedback.

Files: UpgradeNodeRules.cs, UpgradeCatalog.cs, PrototypeOrderSession.cs, PlayerProfileStore.cs, ClickCookPrototypeController.cs, UpgradeTreeView.cs; UpgradeCatalog.asset authored via Unity SerializedObject; new TripleStrikeVerification.cs and contract (+ metas); previous U3 checks updated for availability/current schema/statistics. Existing dirty worktree changes preserved; no scene, authored food or project settings edits in this slice.

Scoped verification per AGENTS.md:

- Compilation passed. TripleStrikeVerification.RunModelChecks passed known node-specific price sequences (including exact decimal 32400/58320 triple prices), 5/10 caps, prerequisites, owner/replay/pause/day/insufficient-funds guards, independent rank contributions and matching catalog caps for all working nodes. Strict boundaries around .22/.23 and capped .30/.40 confirmed exclusive outcomes. Eight critical combinations verified independent rolls/single rounding. Lethal cases at primary/second/third verified skipped RNG, single profile notification/receipt/XP/payout, no stale reward/carry into replacement. One triple transaction per initial/repeated Hold trigger and Back guards passed. Previous DoubleStrikeVerification.RunModelChecks passed for the affected triple-zero compatibility/price path. Unchanged broad tree navigation/critical math/Hold lifecycle reused prior evidence.
- RunStorageChecks passed schema 1–5→6 retaining exact wallet/XP/all prior node ranks with no eager write/free new ranks; old backup recovery; first/max II+triple roundtrips idle; independent cap/duplicate/prerequisite/unknown tier/alias rejection; version 7 protection.
- Isolated Temp/ClickCookSaveTests/U3b profile: triple locked before II purchase, II button spent 50000 once with its own 0/5 cap and next price 80000; triple button spent 10000 once with next 18000, both autosaved despite repeated invokes. Detail contributions and aggregate 22%/1% statistics fit. Synthetic real pointer down/up/click and Submit each applied one three-strike transaction; critical feedback 3 movements/27 HP fit. Third-strike completion followed by stale result events gave one receipt and autosaved both new nodes. Actual 1920×1080 tree screenshot reviewed at Temp/U3b/tree.png. Temporary size restored.
- Restart/Continue restored all nine working node ranks, exact receipt wallet/XP, no active day/Hold/feedback. The first verification response timed out after the code had passed and erased its expected-value keys; blindly repeating the one-shot helper produced an expected-value assertion. Read-only inspection confirmed the completed cleanup, all expected ranks/idle state and exact match against the on-disk autosave. This was a verification transport/replay issue, not a product state loss. Play stopped and profile override removed.

Pipeline socket/reconnect and main-thread timeouts recovered. Console review distinguishes existing Unity VCS configuration/tool errors from ClickCook runtime exceptions. No physical input pass, Player build, broad resolution/regression suite or full economy/pacing claim. Next separate slice: U4 first auto subsystem, with activation/trigger/frequency/strength and save contract fixed before any new sale; later manual tiers remain separate work.

## Current checkpoint — U3 additional same-order strike — 2026-10-24

Implemented first `node.double_strike_chance.01` / Подвійний рух I and `node.double_strike_power.01` / Відточений повтор I. Both have ten ranks. Chance requires Strength 2, adds 2 percentage points/rank to 20%, costs ceil(350 × 1.7^N). Power requires chance 1, starts at ×0.5 without purchase and adds .05/rank to ×1, costs ceil(400 × 1.7^N). These are adopted initial ClickCook balance values, not source rewards or a pacing claim. Contract: ClickCook_U3_Double_Strike_Contract.md. Later tiers/triple remain unavailable.

TryCookClick now handles one bounded transaction: primary strike with its own crit; if HP remains and chance succeeds, one additional strike with its own independent crit. No proc recursion. A lethal primary consumes no double roll or extra critical roll. Additional damage uses ceil(D × q × critical multiplier) once: base 6, q .55, multiplier 2.1 yields ordinary 4 or critical 7. HP clamps per strike, and the existing completion/reward/profile notification executes once after the transaction. Hold repeats and Submit share the same path; stale/paused/result/Back/order guards remain. Feedback aggregates the actual strikes' calculated damage before clamp, records strike count/extra damage/any critical, and resets per accepted trigger/selection/Back/day end. HP copy identifies two movements and labels remaining ordinary clicks as an upper estimate without bonuses.

Statistics fit five lines by combining base strength/rank and adding actual proc chance/power. Details/tiles/quotes use each node's own ten-rank cap. Schema 5 adds the two canonical IDs and validates independent bounds/purchased prerequisites; schemas 1–4 reject them even at rank 0. Existing state and old backups are retained without eager rewrite; versions >5 protected. No transient gesture/feedback is saved.

Files: UpgradeNodeRules.cs, PrototypeOrderSession.cs, PlayerProfileStore.cs, ClickCookPrototypeController.cs, UpgradeTreeView.cs; UpgradeCatalog.asset authored through Unity SerializedObject; new DoubleStrikeVerification.cs and contract (+ metas); HoldFrequencyVerification version expectations made current-version aware. Unity serialization escaped the existing frequency display name without changing its text. Prior worktree changes preserved; no scene/authored food/project settings changes in this slice.

Scoped verification per AGENTS.md:

- Compilation passed. DoubleStrikeVerification.RunModelChecks passed both complete known price sequences/caps, prerequisites, exact wallet debit/replay/owner/pause/day guards, zero chance and strict probability boundaries, all four primary/extra crit combinations, single rounding and one proc per trigger. Lethal-primary/extra cases verified one profile notification/receipt/XP/payout, no carry into replacement, no stale RNG/reward, and one transaction for each initial/repeated Hold trigger. Existing CriticalUpgradeVerification.RunModelChecks passed for the refactored primary critical path. Unchanged full tree navigation and Hold lifecycle evidence reused.
- RunStorageChecks passed schema 1–4→5 without lost money/XP/prior ranks or free nodes/eager writes; old backup recovery; both rank 1/10 roundtrips idle; negative/over-cap/duplicate/prerequisite/alias/tier rejection; future version 6 protection.
- Isolated Temp/ClickCookSaveTests/U3 profile: chance button spent 350 once and power button 400 once despite duplicate invokes; both autosaved. Power lock before chance, refreshed current/next price/q and all statistics fit. Synthetic real pointer down/up/click produced exactly one two-strike trigger; Submit shared the pipeline. Combined critical feedback fit. Lethal extra strike followed by stale pointer/Submit events paid once and autosaved both nodes/receipt. Restart/Continue restored all seven working node ranks, exact post-receipt wallet/XP, no active day/Hold/feedback. Actual 1920×1080 tree screenshot reviewed: Temp/U3/tree.png. Temporary size restored, Play stopped, save override removed.

Pipeline timeout on initial Play sizing and socket reconnect on restart recovered; actual dimensions/advancing frames and Continue subsequently passed. Console review found existing Unity VCS configuration errors and Pipeline timeout diagnostics, no ClickCook runtime exception. No physical input pass, broad regression/resolution suite, Player build or full economy balance claim. Next separate slice: U3b second double chance tier (manifest prerequisite for triple) plus first triple, after node-specific prices/caps/contributions and the shared double/triple probability contract.

## Current checkpoint — U2c hold frequency — 2026-10-24

Implemented first node `node.hold_frequency.01` (Робочий ритм I), requiring purchased Hold 1. Ten ranks add 0.5 actions/sec each to the base 4, reaching 9/sec; prices ceil(300 × 1.6^N), total 54479. These are adopted initial ClickCook balance values, not source rewards or a pacing playtest result. Contract: ClickCook_U2c_Hold_Frequency_Contract.md. Tier .02 remains unavailable even when its :max prerequisite becomes satisfied; no numeric preview or save whitelist for it.

The session computes the effective frequency and reciprocal interval; both initial deadline and repeat rescheduling use it. Initial hit, critical damage, completion receipts and input cancellation retain the U2b pipeline. Delayed frames skip backlog, with at most one repeat/frame. Tree preview/rank/cap/price, actual statistics and unfocused counter hint reflect the frequency. Hold unlock detail labels its 4/sec as baseline. Schema 4 adds only the first frequency node (0–10), rejects frequency in old schemas even at rank 0, preserves legacy profiles without eager writes, and protects future version >4. Gesture/deadline remains transient.

Changed U2c files: UpgradeNodeRules.cs, PrototypeOrderSession.cs, PlayerProfileStore.cs, UpgradeTreeView.cs, CounterDishPointer.cs and UpgradeCatalog.asset; new HoldFrequencyVerification.cs and contract (+ metas); existing critical/Hold checks updated for the new availability/schema boundary. Prior U1/U2a/U2b and unrelated dirty worktree changes preserved; no scene or project settings changes in this slice.

Local verification per AGENTS.md, scoped to the scheduler/purchase/schema/UI risks:

- Compilation passed. HoldFrequencyVerification.RunModelChecks passed all ten known prices and effective rates, exact/early repeat deadlines, skip-backlog, critical damage, pause/day/quote-owner/replay guards, exact wallet total, cap and tier-2 unavailability. Existing Hold RunModelChecks passed for the affected base 0.25-second scheduler and completion boundary. Unchanged full critical math, input lifecycle and tree navigation reused earlier evidence.
- RunStorageChecks passed schema 1/2/3→4 without eager write/free frequency or loss of wallet/XP/existing ranks, old backup recovery, rank 1/10 roundtrips idle, invalid bounds/duplicates/prerequisites/aliases/tier rejection and version-5 protection.
- Isolated Temp/ClickCookSaveTests/U2c profile: real Buy button spends 300 exactly once despite duplicate invocation and autosaves rank 1; next detail shows 480 and 4.5→5/sec. Controller LateUpdate repeated critical hits at the new rate; matching pointer release/click adds no hit, and HP stayed unchanged through a 0.4-second observation. Counter hint/statistics rate and text fit passed. Rendered 1920×1080 screenshot reviewed at Temp/U2c/frequency.png. Restart/Continue retained wallet 700.25, Dish XP 50, all five working ranks and idle Hold/day. Temporary Game View preset restored, Play stopped and save override removed.

Pipeline main-thread timeouts around initial Play/restart recovered; actual subsequent operations/frames and Continue succeeded. Console contained existing Unity VCS configuration exceptions and those tool timeouts, with no ClickCook runtime exceptions. No physical input pass, broad regression/resolution matrix, Player build or end-to-end economy balance claim. Next separate slice: U3 chance/power of an additional same-order strike, after its local numeric/proc contract.

## Current checkpoint — U2b Hold — 2026-10-24

Implemented node.click_hold.01 only: one rank for 250, requires purchased first Click Strength rank 2. The actual counter dish accepts a left-button press as one immediate manual hit, followed by repeats at a 0.25-second minimum interval. Every applied hit uses the existing critical/completion/reward path. Matching release and its subsequent click add no hit; ordinary click-on-release is unchanged without Hold, and Enter/Submit remains discrete. Other buttons/foreign pointer-up do not release the active left hold. Pointer exit and keyboard focus leaving the dish also cancel it, as explicitly adopted in ClickCook_U2b_Hold_Contract.md.

The transient scheduler binds to one order. Pause setter cancels immediately, including pause/resume within one frame. Back, result/completion, end day, application focus/system pause, pointer/controller disable and order replacement cancel Hold. Repeats run in controller LateUpdate after EventSystem processes release/exit. A delayed frame takes at most one repeat and sets its next deadline from actual time; there is no catch-up burst or carry into another order. A fresh unowned press clears an old run's consumed-click marker, preserving the first ordinary click after a reset.

Schema 3 adds only canonical node.click_hold.01, rank 0–1, with its purchased prerequisite. Existing schema 1/2 profiles load without an immediate rewrite and retain wallet/XP/working ranks. Schema 2 cannot contain Hold; the next ordinary save writes 3, protecting the profile from a U2a client that treats version >2 as unsupported. Old backups remain readable and versions >3 protected. Hold input state/deadlines are never saved. hold_frequency nodes remain unimplemented/unpurchasable.

Tree details/tiles now use each implemented node's own maximum (including Hold 1/1), show disabled→4 actions/sec and actual Hold statistics. Counter hints explicitly distinguish left-button Hold and one-action Enter. Files: UpgradeNodeRules.cs, PrototypeOrderSession.cs, ClickCookPrototypeController.cs, CounterDishPointer.cs, PlayerProfileStore.cs, UpgradeTreeView.cs; new HoldUpgradeVerification.cs and U2b contract (+ metas); affected storage/critical test version expectations updated. No scene, authored food, catalog or project settings changed by U2b; preceding U1/U2a and unrelated worktree edits remain present.

Local verification under AGENTS.md:

- Compilation passed. HoldUpgradeVerification.RunModelChecks PASS: exact price/prerequisite/one-rank cap, insufficient funds, node-specific replay, unowned/foreign-order begin, initial/manual damage, exact 0.25 deadline, no backlog, same-frame pause/resume, Back/reselect/end cancellation, critical rolls on actual repeats and single bounded completion/receipt with no carry.
- RunStorageChecks PASS: schema 1/2→3, exact wallet/XP/existing ranks and no free Hold/eager write, rank/duplicate/prerequisite/frequency rejection, old backup reads and version-4 protection. Existing PlayerProfileStoreVerification.RunModelChecks passed in the affected atomic storage subsystem. Unchanged critical math and full U1 tree checks reused prior evidence.
- Isolated Temp/ClickCookSaveTests profiles: unowned down leaves HP alone; release/click and Submit each apply one hit. Real purchase button spends 250 once and autosaves, detail max is 1/1, actual Hold statistics fit, frequency cannot be bought. With Hold, duplicate down adds no hit; live LateUpdate repeats were observed via changing remaining HP after advancing frames. Matching release/click left HP unchanged, including a 0.4-second observation spanning the next interval. Synthetic pointer/lifecycle checks passed for foreign/right up, pointer exit, same-frame pause/resume, application focus/system pause, both disable paths, keyboard focus, Back/reselect, stale release, result/next order/end; no duplicate payout. Test-only transient clones and deterministic roll provider stayed in the isolated session.
- Play restart/Continue restored all four working ranks with Hold idle and no active day. Actual 1920×1080 visual review passed for Hold details/max/statistics; both focused and unfocused Hold hints fit. Game View preset restored. After the final unowned-press reset fix, only the relevant unowned input/purchase phase was repeated and passed; unchanged Hold lifecycle/restart evidence was reused. Play Mode stopped and temporary profile override removed.

A Pipeline main-thread timeout while setting the temporary Game View size recovered; actual 1920×1080 and advancing frames were subsequently confirmed. Console review separates existing Unity VCS configuration errors/tool diagnostics from ClickCook runtime exceptions. No physical mouse/keyboard pass, broad gameplay/resolution suite, Player build or human pacing/balance claim. Next execution slice: U2c hold_frequency, with exact values/caps/prices fixed locally before sale.


## Current checkpoint — U2a critical upgrades — 2026-10-24

Implemented only the first critical chance and multiplier nodes, following the U0 numbers and manifest prerequisites. node.critical_chance.01: 0–15 ranks, ceil(150 × 1.9^N), +2 percentage points/rank, cap 30%, requires first Click Strength rank 3. node.critical_multiplier.01: 0–15, ceil(200 × 1.9^N), ×(2 + 0.1N), cap ×3.5, requires purchased chance rank 1. The first chance rank works with the baseline ×2 multiplier. Hold/frequency, later critical tiers and other effects remain unavailable. Contract: ClickCook_U2a_Critical_Contract.md.

Each accepted manual hit rolls once using u < chance; chance zero and rejected/paused/stale hits consume no critical RNG. Critical damage rounds up once using decimal multiplication. HP clamps at zero; completion/reward remains one transaction with no carry into another order. Session reports the last accepted hit for visible critical HP feedback. With critical chance active the displayed remaining clicks explicitly means the count without critical hits.

Upgrade quotes now carry their nodeId; profile mutation validates the implemented node, purchased prerequisites, owned rank/cap and exact wallet debit. Existing per-frame replay guard, active-day and pause restrictions remain. The tree previews real effects/prices for the three implemented nodes and distinguishes unmet prerequisites from unimplemented effects. Statistics use actual independent node ranks and effective chance/multiplier. No data/catalog/scene layout/food asset changes in U2a.

Schema 2 stores canonical node IDs for only these three implemented nodes. Load accepts signed schema 1 and maps legacy click_strength once to node.click_strength.01, preserving exact wallet, Dish XP and damage. Loading does not rewrite the file. The next ordinary save uses schema 2 through the existing atomic/backup mechanism; schema-1 backups remain readable. Duplicate/unknown/alias/root/future-tier IDs, out-of-bound ranks and purchased critical ranks without prerequisites are rejected. Versions above 2 stay protected. The legacy click_strength getter/quote alias remains compatible; snapshots contain canonical node IDs.

Files: new Data/UpgradeNodeRules.cs, Tests/Editor/CriticalUpgradeVerification.cs and the U2a contract (+ metas); modified profile/store/session, controller, tree definition/view. Existing profile/store/U1 verification expectations were updated for canonical snapshot keys, schema 2 and effect copy. All earlier U1 and unrelated worktree changes remain present.

Local verification (AGENTS.md):

- Compilation passed. RunModelChecks PASS: node ownership/quote replay/foreign and paused/day guards, exact prices and prerequisites, caps, zero-chance unchanged damage/RNG, deterministic rolls immediately below/at threshold, decimal damage rounding, bounded critical completion and single receipt/no carry.
- RunStorageChecks PASS: signed schema-1 large exact decimal wallet/XP/rank migration without eager rewrite or double strength, schema-2 roundtrip of all working ranks, schema-1 backup recovery, signed invalid node/rank/prerequisite rejection and future-schema protection (including a newer file appearing after Load). Existing PlayerProfileStoreVerification.RunModelChecks passed for atomic write/backup/recovery/error/reset behavior in the affected storage subsystem.
- Isolated Temp/ClickCookSaveTests profile: RunPurchaseUi, RunMultiplierUi and RunCookingUi PASS. Real button purchase/debit/autosave, locked vs unimplemented state, pause/replay guards, new next-frame quote, Continue after Play restart, both ranks retained, actual statistics fit, and deterministic critical through the real controller/HP path. A fixed decimal-separator assumption in the test was corrected to follow the Editor locale. The first cooking fixture selected an authored 5-HP sandwich and correctly completed it immediately; the intermediate-feedback check was corrected to use a transient 30-HP clone, then passed, without editing authored food data. Test-only roll injection stayed inside the isolated session.
- Actual 1920×1080 visual inspection passed for the new critical details/statistics. Game View preset restored; Play Mode stopped and temporary profile override removed. Background-frame stalls and Pipeline reconnects were resolved before dependent next-frame checks. Console contains the existing Unity VCS configuration errors; no ClickCook runtime exception was observed.

No unrelated gameplay suite, resolution matrix, Player build or physical mouse/keyboard pass. End-to-end economy pacing and full-game balance remain unverified. Next execution slice: U2b Hold, with its input lifecycle and save whitelist checked locally before sale; hold_frequency is a later separate slice.


## Current checkpoint — U1 upgrade tree — 2026-10-24

Implemented the 92-tile U0 manifest in the existing uGUI Upgrades screen. Runtime presentation content preserves every stable nodeId, familyId, kitchen title/description and explicit prerequisite. Edges use the adopted graph; dependency depth determines columns. All tiles remain inspectable, including unavailable effects. Details distinguish the virtual free root, working rank/price/current→next strength, unaffordable/max states, unmet purchased prerequisites and unimplemented effects with unknown numeric limits/prices. Unknown predecessor maximum never counts as completed. Active statistics report actual manual damage and purchased legacy ranks, with Hide/Show.

Mouse drag pans; wheel zooms around the pointer, clamped to 30–140%; buttons provide zoom and center-on-selection. Explicit keyboard links reach every tile; Move/Submit update selection/details and reveal offscreen nodes. Details have an independently clipped scroll area with wheel/drag and Page Up/Page Down. Selection, focus and hover have visible cues. Closing/reopening retains the view without duplicating tiles. Pause blocks tree actions and purchases.

Only node.click_strength.01 maps to existing schema-1 click_strength. Its original 0–15 ranks, decimal prices, damage, quote/debit/replay guards and immediate profile save remain in place. Selecting another tile clears the displayed quote; later tiers never inherit legacy rank. Root writes no rank or reward. No other upgrade effect, save migration, catalog change or gameplay balance was introduced.

Files: new Resources/UpgradeTreeNodes.json, Data/UpgradeTreeDefinition.cs, UI/UpgradeTreeView.cs, Editor/UpgradeTreeSetup.cs and Tests/Editor/UpgradeTreeVerification.cs (+ metas); controller integration; SampleScene adds one view component and its controller binding. Existing uncommitted model/setup/test/settings changes were retained outside U1.

Local verification, following AGENTS.md:

- Changed risk: content/graph drift or rank leakage. RunContractChecks PASS: all 92 records and exact title/description/prerequisites match the manifest, 38 families plus root, valid rooted DAG, correct working/max dependencies and no legacy rank on future tiers.
- Changed risk: wrong selection spends money or misreports state. RunInitialUiChecks and RunPostPurchaseChecks PASS in an isolated Temp/ClickCookSaveTests profile: every tile's details/non-sale and keyboard reachability, readable tile text, zero-wallet/max/prerequisite states, exact one-time 20 debit, duplicate callback protection, next-frame 44 quote/strength refresh, actual statistics, schema-1 autosave with only legacy rank, pause, Back/reopen and no duplicate view. A first post-purchase attempt ran while the background Editor had not advanced; it was repeated after focusing Game View and confirming advancing frames, then passed. One test compilation typo in the snapshot property name was corrected; final compilation passed.
- Changed risk: clipped graph/input hits the wrong surface. RunInteractionChecks PASS at actual 1920×1080: topmost tile raycast, drag routing to viewport, EventSystem Move/Submit, pointer-anchored zoom, separate detail scrolling. Pan/wheel, zoom limits and Hide/Show passed. Final visual inspection at 1920×1080 passed; Game View preset restored. The final paper-sprite adjustment was inspected and interaction checks passed afterward; unchanged purchase checks were reused.
- Console review found no ClickCook runtime exception; existing unconfigured Unity VCS errors remain. A capture attempt using an unsupported parent-relative path was rejected by Pipeline; capture succeeded with its supported Assets-relative path. Captures were moved outside Assets after inspection.

No unrelated gameplay/regression suite or Player build was run. Physical mouse/keyboard input and a broad resolution matrix are not claimed; input evidence is local EventSystem/PointerEventData plus rendered inspection. Play Mode stopped and the temporary profile override removed; the user's normal save was not used.

Next separate milestone: U2, with the chosen effect's local numeric/lifecycle contract and save migration before any new node becomes purchasable.


## Current checkpoint — U0 complete after Chrome source reading — 2026-10-23

Read all 25 remaining FishSim popup images through the connected Chrome Computer Use session. The completed design manifest defines 92 kitchen nodes: 76 reference records and 16 explicitly marked ClickCook additions, covering 38 effect families plus a free root. Every node has a stable ID, kitchen title/description and adopted ClickCook prerequisites. Added preparation_efficiency and auto_action_count; corrected the earlier conflation of automatic action count with action strength. Preserved crit, double/triple, hold, automation, passive income, XP/quality/economy, optional bonus timing and the owner-approved bounded critical-time carry.

The source audit is complete for the supplied image set. One original title is physically cropped; its effect/tier is legible and its kitchen copy is defined. Source graph identity and completeness across other FishSim versions are not claimed. The manifest explicitly distinguishes observed source facts from kitchen design decisions. Tracker tiers, auto-hook trigger, location-level ownership and preparation work reduction now have concrete adopted kitchen behavior; numeric rank/price contracts remain gates before their effect slices.

U1 implementation has not started. Next: 92-node UI, detail/statistics and pan/zoom, with only legacy Click Strength purchasable. Schema-1 profile and current gameplay remain unchanged. Earlier checkpoints below are historical and superseded by this contract.

Verification PASS: 92 unique nodes, 76 distinct source image references plus 16 additions, 38 registered effect families matching the manifest; every node has kitchen copy and reaches the root through existing prerequisites, no cycles, legacy Click Strength mapping documented. The first family check omitted the mixed manual/choice category in its parser; narrowing extraction to the registry section resolved that check without changing data. Scoped tracked-document whitespace check passed. No Unity compilation, Play Mode or Player build: runtime behavior did not change. Numeric gameplay balance and perceived pacing remain untested until their effect slices.

## Current checkpoint — continued FishSim audit — 2026-10-23

Continued the source review before U1 and recorded missing observations from the end of the prior session. The audit now records 51 popup images (not 51 distinct upgrade nodes) and kitchen copy for 36 reserved effect families. Added distinct day_income_bonus, location_value, selection_precision_window, order_tracker_tier and order_tracker_frequency. The overview contains 76 visible tiles, but detail screenshots show additional tiers; 76 is a confirmed overview count/minimum breadth, not a proven complete source inventory. Source graph/tile manifest remains incomplete, and U1 implementation has not started.

The approved no-penalty timely-choice starting progress and bounded critical-time carry remain intact. Triple strikes stay in scope as a separate later slice. Tracker II/III source previews are ambiguous; location-level ownership, pulse units and numeric balance remain future effect gates, not fabricated source facts. Runtime, scene, static gameplay assets and save schema are unchanged. Figma MCP reading resumed, then returned the Starter call limit again. Chrome extension was absent from browser inventory when checked. Next action is to read the remaining exact image IDs listed in the audit and complete node/edge mapping; no Unity tests or build are warranted for documentation-only work.

Local documentation check PASS: 36 unique registry IDs match 36 kitchen-copy rows; 51 unique source image records; scoped tracked-document whitespace check passed. Historical 27/29/31-family checkpoints below are superseded by this inventory and do not close the remaining source gate.


## Current checkpoint — FishSim source audit before U1 — 2026-10-22

Owner requires FishSim-equivalent breadth and mechanics with kitchen names/descriptions. U0's one-node-per-family assumption is superseded; U1 source gate is open. See ClickCook_FishSim_Upgrade_Audit.md: 13 source popup images and one overview read, kitchen copy for 29 reserved families, separate tier node identity/rank and effect-family identity, source facts distinguished from adopted design. auto_action_chance and selection_start_progress added to design registry only. Owner approved timely-choice starting progress with no slow-choice penalty; no cooking expiry/spoilage. 78 selected reference objects are not an upgrade count. Exact source tile count, unread effects and full prerequisite mapping remain unverified.

Figma MCP returned Starter call limit. Chrome Browser Use successfully read the live board via the extension; large selection copy/export stalled the tab/tool, reload recovered it once, then export timed out and the reconnected tab returned Debugger unattached. No UI tree implementation or runtime changes were made. Only local documentation consistency checks are appropriate; no Unity build or regression suite is required.


## Current checkpoint — U0 — 2026-10-22

Completed the documentation/data-contract milestone in ClickCook_U0_Upgrade_Contract.md: reserved all 27 family IDs, retained all 16 serialized IDs, fixed the initial manual-branch graph and adopted starting price/effect/rounding contracts, separated purchased ranks from future achievement source grants, and recorded the save-schema boundary. Only Click Strength remains implemented/purchasable. Next milestone: U1; U2+ effects remain future work.

Owner verification policy is persisted repository-wide in root AGENTS.md: checks must address a concrete changed risk, remain local, reuse unchanged evidence, and avoid unnecessary repeats/builds. U0 validation is limited to ID/catalog consistency, prerequisite bounds/reachability/cycle checks and a few price/damage examples. No C# or runtime assets changed, so compilation, Play Mode, regression suites and Player build are unnecessary for this checkpoint.

Local validation PASS: 27 unique IDs, all 16 current catalog IDs preserved, 3 valid prerequisite edges with all 4 initial-branch families reachable and acyclic; documented third-rank price and damage examples match decimal calculations. Scoped tracked-document whitespace check PASS. No runtime verification was run or claimed.


## Current checkpoint — Milestone 7B — 2026-10-22

7B local save/load, Main Menu/Continue/New Game and minimal Pause are completed and verified in Editor and a Windows x64 Player. The owner explicitly approved one local profile + backup, immediate persistence of earned rewards/purchases, discarding interrupted days without new rewards, confirmed New Game reset and blocked gameplay while paused. Earlier sections are historical reports; their statements that persistence is absent no longer describe the current implementation.

### Implemented

- `PlayerProfileStore.cs`: schema-1 JSON envelope with exact invariant decimal wallet, XP for stable dish IDs 1–32, implemented Click Strength rank 0–15; SHA-256 integrity validation, duplicate/unknown ID and range checks. Orders, partial HP, reservation/result UI and unimplemented progression fields are excluded.
- Writes flush a temporary file before atomic replacement; the previous valid primary becomes backup. Initial save also creates a backup. Recovery never rotates a corrupt primary over a valid backup. New Game replaces both copies. A newer schema is protected even if its file appears after initial load. There is no migration until a subsequent schema is defined.
- `PlayerProfileRuntime` loads the profile once per run, subscribes to completed reward/purchase changes and autosaves the complete wallet/XP/rank transaction. Failed writes retain in-memory progress/dirty status; Retry persists it without another reward/debit. Player storage is `Application.persistentDataPath/profile.json`; Editor storage is Git-ignored project `UserSettings/ClickCookProfile/profile.json`, which survives Editor restarts. Verification uses isolated GUID folders under project Temp and restores its override afterward.
- `ProfileMenu.cs` + `ProfileMenuSetup.cs`: existing uGUI resources provide Main Menu, valid-profile Continue, confirmed New Game/reset, Exit, minimal Pause/Resume/Main Menu/Save and Exit, status/recovery/error messages and a Retry notice. Both-invalid/unavailable/newer saves disable Continue and preserve existing files. Failed save prevents a silent menu exit. Full Settings/version/Discord footer and the Settings Reset Data control remain planned.
- Esc/Pause button/focus loss pause gameplay, preserve previous focus/time scale, and guard controller/session cooking, Back, selections, purchases and result transitions. Menu return ends temporary orders and retains rewards; Continue starts no day automatically. The existing counter presentation stops updating while paused.
- Modified `PlayerProfile`, `PlayerProfileSnapshot`, `PrototypeDishRewards`, `PrototypeOrderSession`, `ClickCookPrototypeController`, `CounterScenePresentation`, and SampleScene. Added `Assets/Tests/Editor/PlayerProfileStoreVerification.cs`; Unity-generated metadata is present. GDD, roadmap and UI/UX specification synchronized.

### Verification executed

- Unity 6000.3.14f1 compilation PASS, no C# errors.
- Save filesystem/model: **41 PASS** — exact large fractional wallet, all 32 XP entries, actual rank purchase, detached fresh load, atomic backup rotation, locked-file failure, recovery/repair, both-corrupt protection, unknown/newer schema, explicit reset + reset-backup behavior, uncommitted temp rejection, checksum corruption and signed invalid payload values/IDs/ranks/XP.
- Play integration: **24 PASS** — startup/hidden callbacks, confirmed New Game, pause/direct-callback guards, real reward autosave/no duplicate payout, menu cleanup/cancel/Continue, real earned-money purchase with debit+rank saved together, and partial-day exclusion.
- Actual stopped/fresh Play Mode entry: **8 PASS**, retained wallet/Dish XP/Click Strength, no active day/order/result, no rewritten save or replayed reward; Continue restored strength 2.
- Locked-file/retry/reset Play integration: **12 PASS**, retained one receipt in memory, blocked silent quit, successful retry, failed reset retaining old session/progress, successful reset clearing wallet/XP/rank and backup.
- Existing regression: **4,685 model**, **58 profile**, **237 Play**, **240 presentation**, **10 empty/mastery edge** assertions PASS. These are Editor/MCP checks, not registered NUnit tests.
- New menu checks after rendered frames: **206 bounds/text/glyph/topmost-raycast assertions per size**, PASS at **1280×720, 1920×1080, 2560×1080** (618 total). Main Menu screenshot visually inspected. An initial immediate-after-activation raycast check ran before Canvas graphic depth was rendered; the test was corrected to wait for actual frames and all three panels passed. No assertion was removed.
- Temporary Game View size/maximization and runtime `runInBackground` were restored; Play Mode stopped; test storage override removed. Console contains only the known pre-existing Plastic/UVCS client-configuration exceptions.
- Final Windows x64 build **Succeeded**, **0 errors**, two environment/service warnings: Pipeline is disabled in Player because no runtime configuration asset exists; native-symbol upload to Unity services returned HTTP 403. Pipeline runtime setting was also checked: `enableInBuilds=false`. Output: `Builds/Milestone7B/ClickCook.exe` (whole build folder required), 138,568,546 bytes. The final build was rerun after resolving a hidden save-scene modal; an intermediate build captured the inspection-request timeout/uncompiled-change advisory, so it is not the final clean build evidence.
- Native Windows Player mouse acceptance **PASS** using Computer Use: initial Continue disabled; confirmed New Game; Sandwich Common (5.00/+10 XP), Mug cake Uncommon (10.00/+12 XP), Sandwich Common (5.00/+10 XP) = 20.00 total; Pause→Main Menu→Continue retained earnings; Click Strength purchase debited 20.00 and granted rank 1; Save and Exit closed the process. Relaunched the final build: Continue enabled, wallet 0.00, rank 1/current strength 2/next price 44.00, Sandwich mastery level 2 (0/24 XP), Mug cake 12/20 XP retained. A fresh day displayed completed=0; no old reward/result was replayed. Native Pause/Save and Exit worked again; Player closed. This used 18 native cooking clicks and no debug funding. The pause button initially overlapped the Location Selection header; it was moved below the header and the final Player view confirmed separation. Injected Escape did not produce an observable transition, consistent with prior automation limitations; hardware-keyboard acceptance remains unclaimed.
- Final purchase/paused-refresh correction: pending next-frame upgrade quote refresh remains pending across paused rendered frames and resumes with the correct rank; both checks PASS. Save-model 41 and Play integration 24 were rerun after that correction. Editor profile path is separate and persistent under UserSettings; test overrides are cleared. The build cleaned its temporary PerformanceTestRunInfo/Settings resources; none remain in Assets. Scoped script/document diff whitespace check PASS; no static gameplay data changed.

### Remaining scope and next milestone

Full Settings, summary/date progression, recipe ownership/unlocks, Player/Restaurant XP, all other upgrade effects, final UI/art/audio and production release QA remain unfinished. No gameplay balance/static dish/location/catalog data was changed. Existing uncommitted order-selection/sandwich/environment/art/settings work was preserved; the scene also contains those pre-existing changes. This checkpoint is local; no mixed working tree was committed or pushed.

Next roadmap step after 7B acceptance: **U0 — graph/balance contract for the first new branch, stable family IDs and rank/achievement source separation**, then U1 tree UI. Do not label the 27-family runtime tree complete or automatically implement U2–U6.

## Designer-source synchronization — 2026-10-15 (documentation only)

- The designer explicitly prioritizes ClickCook Figma/FigJam over previous GDD interpretations. GDD Section 0 now records source precedence and reconciled conflicts; FishSim is a mechanics/progression reference as well as a UI reference.
- Restored designer story/inner thoughts/culinary diary direction, detailed Pause/Settings contents, existing recipe Journal content and the 25 source achievement rows/reward notation. Achievement units, exact triggers and stacking remain unresolved; no rewards were granted.
- Expanded upgrade design: 27 planned effect families (existing 16 plus 11 extensions), connected/tiered progression, manual/critical/hold/extra-strike/automation/economy/XP/quality paths. Only Click Strength remains implemented and verified. The original 16-family draft is not a final content cap; its unimplemented numbers are still proposed balance.
- User approved: double strike is a chance of one extra hit on the same dish, without another portion/reward; critics remain part of the design but activation and career penalties/reset scope require separate approval. No reset behavior was authorized.
- Spatial presentation target: actual right-hand noticeboard sheets and cooking on the existing tray/plate, superseding UI-A3's floating central-card layout as the final target. UI-A3 verification below remains historical evidence, not verification of the new presentation.
- At source synchronization UI-A4 had pre-existing local script/scene/art/test work, not a complete acceptance pass. It was subsequently completed/verified in the separate UI-A4 report below; the documentation-only synchronization itself did not certify it. Existing unrelated settings changes were preserved.
- Documentation changed: ClickCook_GDD.md, ClickCook_Upgrade_Design_Proposal.md, ClickCook_UI_UX_Specification.md, ClickCook_Production_Roadmap.md, IMPLEMENTATION_STATUS.md. No gameplay code, data, scene, Figma or settings were changed by this synchronization task.
- The next checkpoint at synchronization was UI-A4; after its completion below, the next separate implementation is 7A profile/stable IDs. Do not automatically implement U0–U6 or mark the whole tree complete.
- Documentation verification: scoped `git diff --check` passed; original location/dish/rarity tables in GDD Sections 5/8/11 compare unchanged against HEAD; expanded tree contains 27 unique family IDs; achievement table contains 25 entries; Markdown fences/local document links passed checks. No Unity compilation/Play Mode test was run for this documentation-only task. Existing scene-diff whitespace warnings were not edited.

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

- Current upgrade status (6A): Click Strength purchases/damage are implemented; critical/hold/auto-click and all other effects remain unimplemented. Older milestone sections below retain their historical scope.
- Location unlocking and ownership.
- Recipe ownership/gating.
- Final end-of-day summary and date-based next-day progression.
- Full economy/passive income, Player/restaurant XP, save/load, persistence, critical/hold/auto-click, timers, scoring, final UI, and final animations. Baseline rarity, Dish XP and run-local payouts/wallet are implemented in 5B; Click Strength purchases/damage are implemented in 6A.

## Known GDD ambiguities / TBDs retained

- The GDD separately describes new recipe groups and dishes served by locations; their precise relationship and recipe purchase gating are TBD.
- Several location descriptions use incomplete phrases such as “also serves selected ... dishes.” No extra dishes were inferred.
- Correction on 2026-10-12: current Dormitory LocationData has valid Sandwiches and Mug cakes references, confirmed through Unity MCP and 6A Play Mode. Earlier zero-candidate statements describe a historical verification report, not current data. No static asset was changed. Empty pools remain covered by transient test fixtures; final recipe eligibility is still TBD.
- The Michelin restaurant says “all dishes starting from the third restaurant,” which conflicts with / is less precise than its criteria dish list. No additional earlier dishes are inferred.
- Base hidden HP equals ClickRequirement, with one damage per unupgraded click (4B); 6A applies purchased Click Strength. Remaining upgrade modifiers are TBD.
- Location ownership/unlock state, passive-income accrual/collection, recipes, Player/restaurant progression and persistent progression remain unimplemented by design. Base rarity, Dish XP and payouts use the approved 5B rules.

## Files created

- `Assets/Scripts/Data/DishData.cs`
- `Assets/Scripts/Data/LocationData.cs`
- `Assets/Scripts/Runtime/Locations/LocationCatalog.cs`
- `Assets/Scripts/Runtime/ClickCookPrototypeController.cs`
- `Assets/Scripts/Runtime/PrototypeDishRewards.cs` and `.meta` — Milestone 5B
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

- Core game flow: Location Selection → Work Day board → base click cooking → rarity/Dish XP/payout result → replacement / day end exists (5B). The tenth result ends on Continue; final summary remains absent.
- Location system: static data and browsing exist; unlocking, ownership, real progress, passive-income accrual, and recipe gating do not.
- Dish system: static data, base click cooking, approved rarity and independent run-local Dish XP/mastery exist; runtime recipes, persistent progression, final dish art and effects do not.
- Work Day: documented candidates, dynamic orders, base cooking/completion/replacement, ten-dish limit, payouts, early finish and cleanup exist; daily summary, dates and date-based next-day progression do not.
- UI: temporary prototype screens exist; final navigation, styling, responsiveness, feedback, and all other screens do not.
- Dish Result: dish name, rolled rarity, payout/wallet, earned Dish XP, level/progress and Continue exist (5B); final presentation does not.

### Not implemented

- Main Menu, final result presentation, full economy, Player/restaurant XP/progression, unlock systems, upgrades, save/load, achievements, journal, settings, pause, final animations, and audio content.

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

### Remaining limitations at the Milestone 4A checkpoint

Historical plan below; the designer subsequently approved actual base cooking, automatic ending at ten, and return to Location Selection. The completed 4B section supersedes the simulated-completion proposal.

Actual cooking, order completion/replacement, daily counting/limit enforcement, end-of-day cleanup/summary, economy, progression, rarity, and persistence remain unimplemented. Recommend a separate 4B prototype lifecycle slice for explicit simulated completion, exact-instance removal/counting/replacement up to ten, and early-day order cleanup, without cooking formulas or rewards.

Before 4B, approve the prototype end-state behavior at ten completions and the Finish Day destination. Final capacity balancing, future daily-limit increases, final recipe eligibility/availability, timers, click/HP math, rarity, and rewards remain open. No 4B implementation was started.

## Completed Milestone 4B — Base Click Cooking and Order/Day Lifecycle

### Approved rules recorded in GDD Sections 6–9 and 30–31

- Hidden HP = documented `DishData.ClickRequirement`; each discrete click deals one damage; zero completes the exact selected order. Documented values are unchanged, including 70,000 clicks for the Imperial tasting set.
- Back resets partial cooking progress but retains the exact order and the unchanged board. Reselecting starts at full HP.
- A large clickable temporary dish replaces the nonfunctional 4A placeholder; successful completion returns directly to the board, without rarity/result/reward screens.
- Completion marks/removes the exact order, increments completed dishes and generates a fresh identity in the same slot using the approved uniform pool/duplicate rules. No replacement after ten completions.
- Ten completions automatically end the day. Manual Finish Day is available on the board before ten, not during cooking. Both clear unfinished orders/current day state, return to Location Selection and display a message with the finished count; no penalties, rewards, XP or date advancement.

### Implemented files (modified only; no new files)

- `Assets/Scripts/Runtime/PrototypeOrder.cs` — completion flag and hidden remaining click progress.
- `Assets/Scripts/Runtime/PrototypeOrderSession.cs` — exact-instance base clicks, Back reset, slot-aware replacement, real completed count, cap and cleanup.
- `Assets/Scripts/Runtime/ClickCookPrototypeController.cs` — dish-button callbacks, automatic/manual end transitions and message; no hidden HP displayed.
- `Assets/Scenes/SampleScene.unity` — temporary clickable dish, cooking layout and existing finish-message label moved to Location Selection.
- `Assets/Documentation/ClickCook_GDD.md`
- `Assets/Documentation/IMPLEMENTATION_STATUS.md`

### Executed verification

- Unity 6000.3.14f1 recompiled successfully with no C# compilation failures.
- Play Mode button-callback suite checked all 11 locations: random pool/capacity, one active order, one-point damage, no premature completion, Back reset and full-HP reselection, exact last-click completion, replacement identity/slot/location, unchanged other orders, completed counter, early Finish cleanup, and fresh-day reset.
- Actual dish-button callbacks completed ten orders and verified automatic return, `10 / 10` message and empty inactive session; extra/stale callbacks cannot complete another order.
- A transient single-dish fixture tested all 32 documented click requirements, including 70,000, exactly at the final click; independent duplicates, wrong-order clicks and repeated completion guards passed. Model tests also proved no replacement at ten and no further selection at the limit.
- Empty/mixed-null pools, missing location and a pool becoming empty during cooking were tested without modifying static assets. Slot lookup remained correct when a completed slot had no replacement.
- The full suite passed again after a fresh Play Mode session. Automated high-click-count tests verify mechanics, not human pacing; the 60–90-second target has NOT been validated.
- EventSystem raycast/pointer dispatch hit the dish and dealt exactly one damage; cooking labels fit, the dish does not overlap Back, and both early/automatic completion messages fit. Physical mouse interaction was NOT tested.
- Rendered cooking and day-finished views were inspected. No ClickCook runtime exceptions observed; only the pre-existing Plastic/UVCS configuration error remains.
- Serialized scene diff contains only prototype UI/controller changes and added dish UI, with no existing block removed. Dormitory, camera, lighting, URP, static dish/location data, catalog and project settings are unchanged.
- SampleScene is saved, clean, left out of Play Mode, and retains the new serialized dish-button/label bindings.

### Intentionally unfinished and recommended next step at the Milestone 4B checkpoint

This is base cooking and a temporary day-ending flow, not the complete game. Final models, animation/effects/audio, rarity/result presentation, economy/rewards, XP, upgrades, recipes/unlocks, save/load and final End-of-Day summary remain unimplemented. Capacity values remain the existing provisional proposal; no timing or click-count balancing was invented.

Recommended next step: approve the minimal rarity/result and payout contract in GDD Sections 11 and 14 before implementing those systems. Questions include base rarity probabilities, dish-XP influence, currency multipliers and payout timing. Do not infer them from the now-working base cooking loop.

## Completed Milestone 5A — Informational Dish Result

### Approved scope (GDD Sections 7 and 11)

- For completed dishes 1–9, show the exact completed dish name and base `DishData.SaleValue`, explicitly reference-only, not a payout.
- Display `Rarity: not implemented` and `No currency or XP awarded.` Do not assign a fixed Common tier or invent rarity probabilities/multipliers.
- Continue returns to the existing replacement board. Completion/counting/replacement still occur exactly once at the final cooking click; Continue does not modify order state.
- The tenth dish immediately ends the day as in 4B, without a Dish Result screen. No additional Finish Day/Back/navigation action is exposed during the result screen.

### Implementation and files (modified only; no new files)

- `Assets/Scripts/Runtime/ClickCookPrototypeController.cs` — captures the completed order before clearing cooking state, populates the informational result, guards Continue by screen/day state and clears stale text on Continue/end/new day.
- `Assets/Scenes/SampleScene.unity` — adds `ClickCook_Prototype_DishResult` under the existing Canvas with title, details and Continue button, using the project's legacy uGUI Text. No TMP resources or dependencies added.
- `Assets/Documentation/ClickCook_GDD.md`
- `Assets/Documentation/IMPLEMENTATION_STATUS.md`

The existing order/session classes, static data, capacity configuration and all cooking/replacement formulas are unchanged. No economy, XP or rarity code introduced.

### Executed verification

- Unity 6000.3.14f1 recompiled successfully, without C# compilation failures.
- Play Mode suite passed twice across fresh sessions. Each run checked all 11 locations, 52 informational result screens, and exact name/base-value text for all 32 real dish assets using transient unsaved fixtures.
- Results appear only after the exact final cooking click. While a result is visible, hidden/stale order, Cook, navigation, Finish Day, Back and dish-click callbacks do not change the session.
- Continue returns to the same replacement instances/count; repeated Continue does not increment completion or regenerate orders. All untouched slots, pool/capacity rules, Back/full-HP reset, manual finish, fresh-day clearing, empty pools and carousel wrapping passed regression checks.
- Real cooking button callbacks tested the complete ten-dish path: results 1–9, then immediate automatic end/cleanup at ten with no result screen or stale result text.
- Continue EventSystem raycast/pointer dispatch and label-height bounds passed. The initial pointer check failed immediately after the UI/test transition; subsequent inspection and retest after Game View updated passed without code changes. Physical mouse input was NOT tested.
- Rendered result screen was inspected. SampleScene is saved, clean, stopped, and retains all result bindings. No ClickCook runtime exception observed; the pre-existing Plastic/UVCS configuration exception remains.
- Scene serialization comparison shows only the prototype Canvas child list/controller bindings changed, plus new result UI blocks; no existing blocks removed. Dormitory, camera, lighting/URP, static data, catalog and project settings remain unchanged.

### Remaining limitations and recommended next step

This is a partial Dish Result system, not implemented rarity or economy. Base value is informational only. Final graphics/animations/audio, actual rarity, rewards, XP, upgrades, recipes/unlocks, persistence and final End-of-Day summary remain absent. The provisional capacities and 70,000-click values were not rebalanced; human pacing remains unverified.

Recommended next step: a read-only design decision checkpoint for rarity/payout (GDD Sections 11 and 14): base probabilities or non-random progression policy, dish-XP influence, currency multipliers, payout timing and starting wallet. Do not implement these systems without approved values.

## Approved post-5A design decisions — historical approval checkpoint, implemented by 5B below

- GDD Section 11.1 records the designer-approved rarity probability table. Fantastic is canonical for the historical Epic column. Level 3 Common is corrected to 57%, level 11 Legendary to 24%; levels 12/14/16 explicitly repeat 11/13/15. All rows total 100%. No interpolation or runtime rarity implementation was performed.
- Random rarity uses the specific dish's level; Dish XP awards depend on rarity. XP-to-level thresholds, initial/max level, XP awards and calculation timing still need approval.
- Wallet starts at 0 у.е.; payout is SaleValue × the approved future rarity multiplier, once at successful completion, with amounts to two decimal places. Multiplier values and any finer-precision rounding remain unresolved. No wallet/reward code implemented.
- The future tenth-dish result will remain visible until Continue ends the day. This supersedes the 5A bypass only when implemented and verified; current runtime behavior is unchanged.
- Next step: approve remaining numeric progression/reward rules before implementation. Only GDD and this tracker changed; no new Unity verification or gameplay milestone completion claimed.

## Completed Milestone 5B — Dish Rarity, Mastery and Run-Local Payouts

### Approved scope and balance

- Implement GDD Sections 10, 11.1 and 14 together. No further roadmap system is included.
- Per-dish initial level 1 / 0 XP, maximum 18. Transition cost is round(20 × 1.18^(L − 1)), half away from zero; carry excess XP. Total XP cap is 1,744; no additional XP at mastery, payouts continue.
- Common / Uncommon / Rare / Fantastic / Legendary award 10 / 12 / 16 / 22 / 30 Dish XP and pay SaleValue × 1 / 1.25 / 1.75 / 2.5 / 4. XP gains are truncated to the mastery cap.
- Use the approved chance row for the dish level before awarding new XP. Historical levels 19–21 remain in the GDD, not implemented levels. No extra rarity weighting, pity system or click-based XP.
- Zero-starting wallet and Dish XP persist between days in the current run, not across stopping/restarting game/Play Mode. Unfinished orders/Back pay nothing; completed dishes pay exactly once at completion. All ten results are shown; tenth Continue ends the day. No Player XP or save/load.

### Files

- Created: `Assets/Scripts/Runtime/PrototypeDishRewards.cs` and Unity-generated `.meta` — approved probability table/resolver, mastery thresholds, per-dish XP dictionary, decimal wallet and immutable completion receipt.
- Modified: `Assets/Scripts/Runtime/PrototypeOrderSession.cs` — rewards only in the exact completion branch; retain mastery/wallet when day orders/receipt clear.
- Modified: `Assets/Scripts/Runtime/ClickCookPrototypeController.cs` — show real receipt/mastery/wallet, tenth result Continue ends day; stale-screen guards retained.
- Modified: `Assets/Scenes/SampleScene.unity` — only result details font size 28 → 24 for the additional text. Existing hierarchy/bindings, environment, camera, lighting and static data unchanged.
- Modified: `Assets/Documentation/ClickCook_GDD.md` and this tracker — approved rules and verified partial-system progress.

### Executed verification

- Unity 6000.3.14f1 recompiled successfully; no C# compilation failures.
- Play Mode controller suite passed twice in fresh sessions: all 11 locations, carousel wrapping, Cook, candidate pools/capacities, empty Dormitory, Back/full-HP reset, exact click completion, hidden/double/stale callbacks, replacement preservation, early Finish, retained wallet/XP and all ten result screens. The tenth has no replacement and ends only on Continue; repeated Continue does not award again.
- Enumerated all 100 integer rolls at each level 1–18 (1,800 checks) against the designer's exact table. Zero-probability tiers cannot appear. This tests the resolver mapping, not statistical player enjoyment.
- Separate Play Mode model suite passed 160 full-cooking completions: all 32 actual DishData assets × all five tiers using seeded rolls and transient unsaved candidate fixtures. Exact decimal payouts and XP awards, pre-award rarity, duplicate-completion protection and cross-day retention passed.
- Every XP level boundary, overflow carry, cap truncation, no XP at maximum with continued payout, independent dish progress, invalid resolver inputs and fresh session zero-state passed. All game-run progress reset on a fresh Play Mode session.
- Result text preferred-height checks passed across all location results. Continue raycast/EventSystem pointer dispatch passed without changing wallet/count. Physical mouse input was NOT tested.
- SampleScene is saved/clean outside Play Mode; bindings retained and new script recognized by Unity. No ClickCook runtime exceptions observed. Pre-existing Plastic/UVCS NotConfiguredClientException remains unrelated.
- A later Unity MCP request encountered a transport disconnect/server error; subsequent Editor/eval calls recovered. This is not a ClickCook gameplay exception. Game View capture showed Location Selection, but result-screen screenshot requests did not reliably capture the active result; visual screenshot verification of that screen is NOT claimed. Its active state, text contents, layout bounds and EventSystem interaction were verified separately.

### Limitations and next step

This is baseline rarity/mastery and payout, not a complete economy or persistent progression system. Spending, passive income, Player/restaurant XP, saves, final result art/audio/animations and End-of-Day summary remain unimplemented. No DishData/LocationData/catalog values or click requirements were changed. Human pacing and the engagement of this initial balance are NOT verified; high click requirements still need separately approved upgrade mechanics.

Recommended next step: a scoped design checkpoint for the documented upgrade/cooking-speed dependency (GDD Sections 9 and 13), including cost, click-strength effect and purchase rules, before coding an upgrade slice. No next milestone implementation started.

## Completed Milestone 6A — Click Strength Purchases (2026-10-12)

The user authorized the next roadmap step and delegated routine design choices. Adopted the Click Strength subset of the existing proposal: 15 purchasable ranks, initial strength 1, damage ceil(1.75^rank), next price ceil(20 × 2.20^ownedRank), run-local ownership and purchases between days. Remaining proposal mechanics are not implicitly approved.

### Implemented files

- Created `Assets/Scripts/Data/UpgradeCatalog.cs` and `Assets/Data/UpgradeCatalog.asset` (+ Unity metas): one ScriptableObject with 16 serialized definitions (stable IDs/names/base prices/growth/caps). Only Click Strength is enabled for runtime purchases. Other prices remain provisional; no prerequisite/effect framework was introduced for unavailable nodes.
- Modified `Assets/Scripts/Runtime/PrototypeOrderSession.cs`: immutable session/rank/price quote, guarded purchase, persistent-within-run rank and upgraded hidden-HP damage.
- Modified `Assets/Scripts/Runtime/PrototypeDishRewards.cs`: internal positive-amount/exact-balance debit; reward and rarity formulas unchanged.
- Modified `Assets/Scripts/Runtime/ClickCookPrototypeController.cs`: between-day Upgrades/Back/Buy, wallet/rank/current-next strength/price/status, unavailable-node labels and per-frame quote consumption.
- Modified `Assets/Scenes/SampleScene.unity`: prototype upgrade panel/bindings, navigation button and adjacent Cook position. No existing object removed; four existing serialized blocks changed (controller, two parent child lists, Cook RectTransform), plus 55 new blocks. Dormitory/camera/lights/URP unchanged.
- Created `Assets/Tests/Editor/Milestone6AVerification.cs` (+ folder/metas): repeatable Editor-only model and Play Mode verification harness. It is invoked through MCP eval, not registered NUnit tests. Fixture wallet funding stays in Editor-only code.
- Updated GDD, this tracker, upgrade proposal and production roadmap. Previously prepared design documents are included in this checkpoint as related context.

### Executed verification

- Unity 6000.3.14f1 recompiled successfully with compilationFailed=false and no C# errors.
- `Milestone6AVerification.RunModelChecks()` passed 4,685 assertions, including fixed expected prices/damage, all 32 real dish definitions × ranks 0–15 (512 exact-completion cases), debit/insufficient cents/max rank, stale/cross-session quotes, unavailable nodes, mid-day rejection, Back reset, replacement identity, exact payout, empty pools and cross-day/new-session behavior.
- In fresh Play Mode, `RunPlayChecks()` passed 236 assertions: all 11 locations before/after upgrades, carousel wrapping, actual button callbacks, two purchases, repeated callback protection, exact upgraded cooking, result/Continue, early Finish and a full ten-dish day. Upgrade labels fit their bounds.
- A separate fresh Play Mode confirmed wallet/rank reset to zero. Five real UI-cooked dishes earned 31.00 in that sampled run; Buy through EventSystem raycast/pointer dispatch produced rank 1 and wallet 11.00. Repeated pointer dispatch did not buy twice. After the next frame, UI showed next price 44.00 and insufficient balance.
- `RunInteractionChecks()` passed three Back topmost-raycast/pointer assertions. Initial raycasts failed while the background Editor remained at frame 1 and new graphics had depth -1; advancing one frame with EditorApplication.Step resolved this. Run interaction checks after a rendered/advanced frame, not immediately after changing active panels. Physical mouse input was NOT tested.
- Inspected rendered upgrade panel and location navigation. Screen capture can lag state: exact post-purchase values were verified from live UI fields rather than claiming the screenshot depicted them.
- No ClickCook runtime exception observed. Existing Plastic/UVCS NotConfiguredClientException recurred on domain reload; unrelated and not changed. One transient MCP transport failure during entering Play Mode recovered on the next status/eval call.
- Existing DishData/LocationData/LocationCatalog and project settings have no diff. Scene saved; verification leaves Play Mode stopped. Current Dormitory valid two-dish pool confirmed; previous zero-pool documentation corrected as historical.

### Limits and next step

Only Click Strength is playable. No saves, recipe/location gates, other upgrade effects, final tree UI, passive/time-bonus policy, new rarity rules or production build are claimed. Human pacing/full economy balance remains unverified. Next roadmap slice: 7A, one in-memory player profile with stable IDs and separation from day/screen lifetime, preparing 7B save/load. Stop after the 6A Git checkpoint.

### Upgrade design checkpoint — 2026-10-08, not implemented

Prepared `Assets/Documentation/ClickCook_Upgrade_Design_Proposal.md`: all 16 GDD leaf nodes, provisional prices/effects/caps/prerequisites, purchase safety, subsystem gating and staged implementation recommendations. Referenced from GDD Section 13 without changing its original design requirements or marking upgrades complete.

The draft uses the available FishSim screenshots and primary Kongregate/Orteil references; the remaining Figma screenshots were unavailable due to MCP quota. Executed 1,152 mathematical price checks and conditional economic simulations, NOT Unity tests or human pacing validation. Full economy balance depends on unresolved recipe/location progression. Bonus-choice windows, passive-income accrual policy and the numeric upgrade package require approval. Recommended first implementation slice after approval: 6A, run-local multilevel Click Strength and safe between-day purchases; other unsupported nodes remain unavailable. No runtime/scene/data changes, implementation commit or push performed by this design checkpoint.

## UI/UX planning checkpoint — 2026-10-12 (not implemented)

The user paused the next gameplay step and supplied five screenshots: three screen-flow prototypes and two visual references. Created `ClickCook_UI_UX_Specification.md` covering all screens, visual/text tokens, Ukrainian copy proposals, component states, layout/input/accessibility, data bindings and the distinction between illustrative screenshot values and approved GDD mechanics.

Updated `ClickCook_Production_Roadmap.md`: recommend UI-A1 (shared visual components + Location Selection/Click Strength purchase), then UI-A2 (Board/Cooking/Result), before returning to 7A/7B. Complete other screens alongside their runtime dependencies. Both UI-A steps remain not started; no gameplay, Unity scenes/assets, Figma, commit or push changed by this planning checkpoint. Primary GDD rules and implementation checkboxes remain unchanged. Sample bars are not interpreted as hidden HP or new timers.

## Completed UI-A1 — Location Selection / Click Strength UI (2026-10-12)

### Implemented subset

- Reused SampleScene Canvas/EventSystem and all controller bindings. Styled only Location Selection and Click Strength purchase with a blue backdrop, paper panels, orange controls, larger readable type, Ukrainian presentation copy and explicit focus navigation/outline. Disabled purchase is skipped by navigation. Gameplay math/ownership/order rules are unchanged.
- Created three original procedural sprites with sliced paper/button borders (not cropped screenshots), and four reusable component prefabs. Shared venue shape is labeled as a stylized placeholder, not final location art. Used Unity's existing LegacyRuntime font after checking Ukrainian glyphs; no external font import, TMP resources or packages.
- Location data fields show actual level, income and candidate count; display at most three candidate names plus remaining count. Income is explicitly unimplemented, progress bars are not invented. Static IDs/names/numeric assets are unchanged. Only these two screens and the day-finished message have Ukrainian copy; other screens remain for UI-A2.

### Files

- Created `Assets/Scripts/UI/ClickCookUiCopy.cs`, `UiFocusOutline.cs` (+ metas/folder meta).
- Created `Assets/Scripts/Editor/UiA1Setup.cs` (+ metas): explicit one-time live-Editor authoring helper; refuses an already authored screen, clean-scene check, procedural art generation, component prefabs and scene layout. Not player runtime code.
- Created `Assets/UI/Art/MenuBlue.png`, `PaperPanel.png`, `PrimaryButton.png` (+ metas).
- Created `Assets/UI/Prefabs/Components/PaperPanel.prefab`, `PrimaryButton.prefab`, `SecondaryButton.prefab`, `WalletBadge.prefab` (+ metas/folder metas).
- Modified `Assets/Scripts/Runtime/ClickCookPrototypeController.cs`: presentation copy, formatted price labels, focus entry/return and disabled-control navigation; existing transaction/completion calls preserved.
- Modified `Assets/Scenes/SampleScene.unity`: only prototype UI authoring. Serialization comparison: zero existing blocks removed, 60 existing blocks changed, 94 new blocks. Static data/project settings/packages have no diff. Environment/camera/lights/URP content retained.
- Modified `Assets/Tests/Editor/Milestone6AVerification.cs`: price-text expectation uses the presentation formatter instead of a hardcoded English decimal string; gameplay assertions retained.
- Created `Assets/Tests/Editor/UiA1Verification.cs` (+ meta): Editor-only assets/font/contrast, rendered Canvas bounds/text, topmost raycast, focus and temporary Game View resolution checks. Invoke SetResolution only after entering Play Mode, resume/focus Game View so it renders, and RestoreResolution before leaving Play Mode.
- Updated GDD, UI/UX specification, roadmap and this tracker. Previous planning changes are preserved.

### Executed verification

- Unity recompiled successfully, compilationFailed=false; saved SampleScene, clean and stopped after testing.
- 12 asset/font/palette contrast checks passed. LegacyRuntime includes UA glyphs; text-on-paper, secondary text, header-on-blue and primary-button text meet the tested 4.5:1 color-pair target. Rendered-texture inspection was separate.
- Current-loop regression passed with 237 Play Mode assertions, including all 11 locations, wrapping, purchases, base/upgraded cooking, result/Continue, early Finish and a ten-dish day. Repeated after final focus/copy changes.
- Glyph/label-height/button-bounds/focus/topmost-raycast checks passed for both screens at actual rendered Canvas sizes 1280×720, 1920×1080, 2560×1440, 1920×1200 and 2560×1080. Checked the longest Michelin title and large wallet/price states; rendered Location Selection and purchase screen inspected. Initial matrix attempts failed because paused/background Game View still had the previous Canvas size; resuming/focusing rendered the requested sizes and successful checks replaced those failed attempts. Resized screenshot dimensions alone were not counted as validation.
- Max-rank fixture: rank-14 price 1,244,365.00, large wallet display, EventSystem pointer purchase to rank 15 and repeated dispatch protection passed; max-state text/button bounds passed. Fixture funding/rank setup occurred only in Play Mode verification; new-run progress is unchanged.
- Back EventSystem raycast/pointer dispatch passed three assertions. Physical mouse/keyboard input is NOT VERIFIED.
- Restored the original Game View selection and removed only the temporary test preset. Removed only eight generated failed-resolution captures and their metas from Assets/Temp/UI-A1, then its newly created empty parent; no user assets deleted.
- No ClickCook runtime exception observed. Existing Plastic/UVCS NotConfiguredClientException recurs on domain reload. An early MCP main-thread timeout recovered; no gameplay fix was needed.

### Initial blocker history (resolved below)

Computer Use was explicitly attempted using the installed skill and @oai/sky through node_repl. Initialization failed with `failed to write kernel assets: The system cannot find the path specified. (os error 3)`; retry and kernel reset/reinitialize produced the same failure. No native inputs were issued and no substitute Windows automation was used.

At that initial checkpoint, physical input acceptance remained pending and commit/push was deferred. This historical blocker was subsequently resolved by the recovery and acceptance checks below. No profile/save implementation started.

### UI-A1 recovery and native-input review — 2026-10-12

Computer Use initialized successfully and targeted the ClickCook Unity window. Native mouse checks passed for carousel 0→10→0 wrapping and long title/correct fields, opening/closing upgrades, disabled zero-wallet purchase, Cook into Dormitory Work Day, and early Finish with cleared day state. On a Play Mode-only wallet fixture of 60, a native purchase produced rank 1, damage 2 and wallet 40; the next price 44 was correctly unavailable. No fixture values were saved.

Native purchase exposed a focus defect: disabling the consumed offer cleared EventSystem selection before the existing refresh guard ran. Updated controller LateUpdate to restore selection only when absent, choosing the next available Buy or Back. Recompiled successfully, reran all 237 Play Mode regression assertions and 12 static checks; native purchase retest confirmed Back focus and visible outline. SampleScene remains clean; six original root objects remain, static data/packages/project settings are unchanged.

Computer Use keyboard chords operate Editor menus, but Right/Down/Left/D/Enter did not trigger runtime Navigate/Submit even with Game View focus and enabled correct bindings. This is not evidence that a physical keyboard works or fails. Requested a physical keyboard check from the user; keyboard acceptance and commit/push remain pending. Left Play Mode open on Upgrades with Back selected for that check; wallet 40/rank 1 are temporary fixture state and reset when Play Mode stops. No workaround input implementation or project-settings changes were introduced. One MCP observation timed out at 5000ms and logged a pipeline error, then recovered; one post-domain-reload connection reset also recovered. Existing UVCS configuration errors remain unrelated. No ClickCook exception observed.

### Final acceptance — 2026-10-12

The user confirmed physical Enter and arrow navigation work in the Unity Game View. Together with the executed native mouse, automated regression and layout checks, this closes UI-A1 acceptance. Keyboard verification is user-executed, not claimed as successful Computer Use key injection. Stopped Play Mode afterward: temporary wallet/rank/input-observation fixtures were discarded; SampleScene is saved/clean, with the six original roots intact and no C# compilation failures. Known unrelated UVCS configuration errors remain.

UI-A1 is complete; full UI/UX, final art and remaining screens are still unfinished. The checkpoint includes the related UI planning documents, scene/UI code/assets/metas and verification helpers. Recommended next separate step: UI-A2, Order Board/HUD, Cooking and Dish Result presentation using these shared components. No UI-A2 or 7A implementation started.

## Completed UI-A2 — Work Day / Cooking / Dish Result presentation (2026-10-12)

### Implemented

- Reused the existing three screens, buttons, Canvas/EventSystem, UI-A1 sprites/font/focus outlines and current order/reward session. Work Day shows wallet, completed dishes / 10 and orders / actual capacity separately; six possible paper cards occupy a non-overlapping right-hand 3×2 area. Full documented candidate text remains populated but hidden in the Editor-inspectable component, separate from active orders. No eligibility or capacity values changed.
- Cooking shows the selected dish/location, a large clickable plate sketch, a click instruction and an explicit Back reset warning. No HP, damage numbers, fake progress or new effects. Finish remains a board-only action.
- Dish Result shows Ukrainian rarity copy plus color cue, exact immutable receipt payout, actual wallet, XP award and level change. A labeled mastery bar uses actual level XP / transition cost; cap reads maximum/full. Continue uses existing lifecycle; tenth-result label becomes Finish Day. New UI state clears with the existing result lifetime.
- Focus transfers to the first available order (Finish for empty board), dish, Continue or Cook on state entry. Board navigation excludes disabled slots. All art is explicitly temporary; final food illustrations/models/audio/animations are not introduced.

### Files created / modified

- Created `Assets/Scripts/Editor/UiA2Setup.cs` (+ meta): explicit stopped-Editor authoring helper, preserving existing object bindings; paper-backed contrast corrections and draw order.
- Created `Assets/Tests/Editor/UiA2Verification.cs` (+ meta): Editor-only MCP-invoked real-controller presentation/lifecycle, rendered-layout/raycast/focus and transient empty/mastery fixture checks; not registered NUnit tests.
- Created `Assets/UI/Art/PlateCircle.png` and `Assets/UI/Prefabs/Components/OrderCard.prefab` (+ metas); circle is original procedural UI art, not a screenshot crop.
- Modified `Assets/Scripts/Runtime/ClickCookPrototypeController.cs`: five presentation references, Ukrainian day/cooking/receipt copy, real mastery fill, tenth CTA and focus/navigation only; order identity/completion/replacement/payout/XP/upgrade formulas unchanged.
- Modified `Assets/Scripts/UI/ClickCookUiCopy.cs`: Ukrainian display labels for the five canonical rarity tiers; canonical enum/data IDs unchanged.
- Modified `Assets/Scenes/SampleScene.unity`: UI authoring only. Serialization review found zero existing blocks removed; 83 modified existing blocks all belong to ClickCook prototype UI/controller. Dormitory/camera/lights/URP preserved; Data/ProjectSettings/Packages have no diff.
- Updated GDD, this tracker, roadmap and UI/UX specification. No profile/save implementation started.

### Verification actually executed

- Unity 6000.3.14f1 compiled successfully with no C# errors. Existing 6A regression passed 237 assertions: all 11 locations before/after purchases, exact cooking, Back, rewards/replacement and full ten-result day. UI-A2 presentation checks passed 240 assertions across 11 boards and ten receipts/Continue; empty-pool/mastery-cap fixtures passed ten more. Fixtures were transient Play Mode state, never saved gameplay assets.
- All three screens passed 795 total glyph/height/button-bounds/non-overlap/topmost-raycast/focus assertions at actual rendered Canvas 1280×720, 1920×1080, 2560×1440, 1920×1200 and 2560×1080. Game View preset was restored before stopping Play Mode. A zoomed Editor viewport screenshot during the fixed-resolution matrix was cropped by Game View scaling; this is not claimed as a complete visual inspection of every target resolution. Canvas checks and final fitted native-view inspection are separate evidence.
- Computer Use native mouse executed Cook → order → cooking. Back after a partial click preserved that exact order identity and zero counter/wallet. In a fresh run, native 2+2+1 sandwich clicks completed on the fifth click: Common, payout/wallet 5.00, +10 XP, level 1 and 10/20 mastery. Native Continue preserved wallet 5, showed 1/10 and two orders with a new slot-0 identity; native Finish returned to Location Selection with the correct one-dish message. These are real pointer inputs, not callback-only claims.
- Visually corrected low-contrast headings over the ceiling, cooking instruction obscured by the decorative plate, and tiny icon copy clipping. Retested after corrections. Keyboard focus/navigation used EventSystem move tests; physical keyboard input on the new screens was NOT tested (UI-A1 physical Enter/arrows were user-verified previously). Production build/human pacing were not tested.
- No ClickCook runtime exceptions observed. Existing Plastic/UVCS client configuration errors remain; transient MCP network/domain-reload and main-thread timeout errors recovered, successful explicit reruns replace inconclusive tool calls.
- Final SampleScene saved clean outside Play Mode, original six roots retained. Temporary run state reset, no test captures or test presets left in Assets.

### Next separate step

UI-A2 is complete for the defined presentation slice. Full UI, final dish/location art, End-of-Day report, saves and release polish remain unfinished. Return to roadmap 7A: one in-memory player profile with stable IDs and wallet/dish mastery/Click Strength ownership separated from day/screen lifetime; 7B save/load follows later. Do not implement 7A in this execution.

## Completed UI-A3 — Designer playtest corrections (2026-10-12)

The user explicitly requested Exit in the menu, visible dish HP/click progress and central dish selection cards following a new gameplay screenshot. These decisions supersede prior hidden-HP/right-board presentation constraints; original HP/damage/XP/payout/capacity formulas and authored data stay intact.

- Current prototype Location Selection menu now has «Вийти з гри». Native mouse test stopped Play Mode without closing Unity. Standalone implementation uses Application.Quit; a standalone build/quit test was NOT executed. Full Main Menu/save/load remains 7B.
- Cooking shows remaining/max HP, a decreasing orange bar and ceil(remaining/current Click Strength) remaining clicks. Native sandwich click changed 5/5 and five clicks to 4/5 and four; Back/reselect restored 5/5. Back still preserves the order identity; no new damage or reward rule.
- Orders are large central cards above the plate: centered pairs/three-card row, or three columns × two rows for 4–6. No overlap/tilted hit areas; pool/capacity/duplicate rules unchanged. Green bars are explicitly Dish mastery, using real XP/threshold and cap; they are not cooking HP/expiry. Food art remains clearly labeled temporary sketches.
- Modified controller/SampleScene/OrderCard.prefab; created `Assets/Scripts/Editor/UiA3Setup.cs` and `Assets/Tests/Editor/UiA3Verification.cs` (+ metas). Reused UI-A2 authoring primitives through internal helpers in `UiA2Setup.cs`; no new gameplay framework/art dependencies. Updated `UiA1Verification.cs` to target the focused Game View when multiple views exist, avoiding ambiguous resolution tests. Updated GDD, roadmap, UI/UX specification and this tracker.
- Compilation passed. Existing 237 Play Mode regression and 240 presentation/lifecycle assertions passed. New review checks passed 15 assertions at base strength and 15 at purchased strength 2 (temporary Play Mode-only wallet funding), covering HP/fraction/click count/reset, central layout, real mastery and hidden Exit guard. 980 glyph/layout/bounds/non-overlap/topmost-raycast/focus assertions passed for two-card board, six-card board and cooking across 1280×720, 1920×1080, 2560×1440, 1920×1200, 2560×1080.
- First resolution check was inconclusive because two Game Views selected different rendering contexts. The temporary preset was restored, helper corrected, intended Game View focused, and all target checks explicitly rerun successfully. Original preset restored afterward; temporary maximized Game View returned to original dock layout on leaving Play Mode. No production build/physical keyboard retest claimed. Existing UVCS errors remain unrelated; no ClickCook runtime exception observed.
- SampleScene saved clean outside Play Mode; serialization comparison found zero existing blocks removed and 55 modified existing blocks, all within prototype UI/controller. Dormitory, camera and lights retained. Pre-existing unrelated changes in `Assets/Settings/PC_RPAsset.asset`, `Assets/Settings/UniversalRenderPipelineGlobalSettings.asset`, `ProjectSettings/ProjectSettings.asset`, `ProjectSettings/UnityConnectSettings.asset` were preserved and excluded from the UI checkpoint. DishData/LocationData/catalogs were not modified.

Next separate roadmap step remains 7A (run-local player profile/stable IDs), unless the user chooses another UI refinement. Final dish illustrations and standalone Quit verification are explicitly pending.

## UI-A4 — Scene-bound board and plate interaction (2026-10-15)

Completed the existing local work rather than rebuilding the prototype. Six existing order buttons now belong to a world-space Canvas anchored to the actual Dormitory noticeboard. Board capacity/pool/duplicate rules are unchanged. Selecting a sheet displays the exact selected dish on the existing plate/tray; sheets hide during close-up/result and return unchanged on Back. The old floating plate/cooking button remains inactive, with serialized bindings preserved.

- `CounterScenePresentation` handles only scene presentation: environment anchors, 2–6 sheet layout, temporary dish variant, camera look-at/FOV and parallax suspension/restoration. Board framing uses a 12-degree presentation yaw, leaving the authored camera pose unchanged. Mouse-follow normalization clamps outside-window coordinates to keep the counter bounded.
- `CounterDishPointer` routes native collider pointer clicks and EventSystem Submit through the existing cooking guard. Explicit arrow navigation connects the dish target with Back; the hint distinguishes temporary 3D art and shows keyboard input while focused. No new damage/reward/order architecture or upgrade effect.
- Simple sandwich and mug-cake sketches reuse six new URP materials; other dishes use a labeled neutral sketch. Final food models/card illustrations and other location environments remain unimplemented.

### Files

- Created `Assets/Scripts/UI/CounterScenePresentation.cs`, `Assets/Scripts/UI/CounterDishPointer.cs`, `Assets/Scripts/Editor/UiA4Setup.cs`, `Assets/Tests/Editor/CounterSceneVerification.cs` and their `.meta` files. Authoring helpers are explicit stopped-Editor operations; tests are MCP-invoked Editor checks, not registered NUnit tests.
- Created six materials and `.meta` files under `Assets/Art/Prototype/Materials/`, plus folder metadata for Art/Prototype/Materials.
- Modified `Assets/Scripts/Runtime/ClickCookPrototypeController.cs` (world-space layout bypass, board hint, safe initial focus), `Assets/Scripts/Camera_Move.cs` (read-only cursor rotation and normalized input bounds), and `Assets/Scenes/SampleScene.unity`.
- Updated GDD, this tracker, roadmap, UI/UX specification and the upgrade proposal's next-step notes. No changes to DishData, LocationData, catalogs, environment meshes/materials, Packages or project settings in this milestone. Pre-existing settings diffs remain excluded.

### Verification executed

- Unity 6000.3.14f1 compilation PASS, no C# failures. Existing regression: 4,685 model assertions (32 dishes × 16 strength ranks), 237 Play Mode, 240 presentation/lifecycle and 10 empty/mastery edge assertions PASS.
- Frame-settled world-space verification ran all 11 locations at every rendered/input size below. It tests paper corners, topmost rays, text/glyphs, HUD bounds, exact selected instance, one active order, Submit/arrow event routing, Back/no regeneration/reset HP, Finish cleanup, transient empty/independent-duplicate fixtures, ten results/replacements and stale callbacks. These are automated events, NOT physical input claims.

| Actual render size | Lifecycle/input assertions | Spatial/text/glyph/raycast assertions | Result |
|---|---:|---:|---|
| 1280×720 | 176 | 13,812 | PASS |
| 1920×1080 | 176 | 13,851 | PASS |
| 2560×1440 | 176 | 13,918 | PASS |
| 1920×1200 | 176 | 13,905 | PASS |
| 2560×1080 | 176 | 13,905 | PASS |

- Native Computer Use mouse PASS: carousel 0→10→0, Cook, physical sheet → correct mug-cake order, actual 3D dish HP 8→7, Back preserving both IDs and completed=0, reselect HP=8, eight physical clicks → Common result/8.00 wallet/+10 XP/one completion, Continue retaining the replacement IDs without payment, Finish clearing orders and retaining earned wallet, and a fresh day with fresh IDs/completed=0. A six-card location was prepared through MCP, then native mouse selected slot 6/Imperial (70,000 HP), returned with Back and finished; no physical late-dish completion is claimed.
- Initial diagnostics found an offscreen third-sheet edge at fixed 720p; adjusted the board-only camera yaw and reran the complete five-size matrix successfully. Two simultaneous Game Views initially produced conflicting camera/input aspect contexts; final tests targeted the intended maximized Game View and checked actual render/input dimensions. Original resolution preset was restored before stopping Play Mode.
- Existing scene blocks removed: zero; 23 existing blocks changed, all prototype presentation except the Camera GameObject gaining a PhysicsRaycaster. Original Dormitory objects, camera Transform, lights and URP volume retained. No static gameplay asset diff. Scene saved outside Play Mode; Play Mode stopped.
- Console contains only the known Plastic/UVCS client configuration error; no new ClickCook runtime exception observed. No standalone build/quit or physical-keyboard retest was executed. Runtime Submit/move was tested through EventSystem, not synthetic keys presented as real hardware. Full production typography/art/performance acceptance remains later work.

Next separate milestone: 7A — in-memory player profile/stable IDs, followed by 7B persistence. Expanded tree U0–U6 remains planned, not implemented.

## Milestone 7A — run-owned player profile and stable IDs — completed 2026-10-15

### Implemented scope and files

- New `Assets/Scripts/Runtime/PlayerProfile.cs`: owns one existing `PrototypeDishRewards` component (wallet/Dish XP) and ranks keyed by stable upgrade ID. A profile-wide active-day guard prevents concurrent days and purchases through another session while a day is active.
- New `Assets/Scripts/Runtime/PlayerProfileRuntime.cs`: run-lifetime owner, not a screen/controller owner. SubsystemRegistration resets it for a new run; no disk I/O or persistent Unity object is required.
- New `Assets/Scripts/Runtime/PlayerProfileSnapshot.cs`: detached read-only copies of exact decimal wallet, numeric dish-ID→XP and upgrade-ID→rank. No Unity asset references, active orders, results or location ownership. This is an in-memory boundary, not a save/restore format.
- Modified `Assets/Scripts/Runtime/PrototypeOrderSession.cs`: injected profile, existing UI/rewards facade retained, Click Strength rank delegated to profile; day cleanup releases only its own active-day ownership. Default constructor still creates an isolated fresh profile for fixtures.
- Modified `Assets/Scripts/Runtime/PrototypeDishRewards.cs`: internal XP-copy helper only; probabilities, rewards and XP formulas unchanged.
- Modified `Assets/Scripts/Runtime/ClickCookPrototypeController.cs`: binds the run profile in Awake; OnDestroy clears its temporary day without deleting earned progress.
- New `Assets/Tests/Editor/PlayerProfileVerification.cs`; Unity generated `.meta` for each of the four new scripts. Documentation updated: GDD, this tracker and Production Roadmap.
- Identity policy reuses existing authored DishId, upgrade string IDs and catalog level keys 0–10; no assets renumbered. Location level remains a lookup key, not an inferred player-level gate. Future location/recipe/achievement progress is not invented or stored now.

### Executed Unity verification

- Unity MCP compilation: succeeded, no C# failures.
- New isolated profile tests: **58 PASS** — all 32 dish IDs, 11 unique ordered location keys, unique upgrade definitions, fresh profile, same-profile sessions, concurrent-day/purchase guards, foreign/stale quotes, single receipt, exact debit, day replacement, snapshot isolation/mutation rejection, null profile/location, discarded-order callbacks.
- Existing model regression: **4,685 PASS**, all 32 dishes × 16 strength ranks (512 exact cooking cases).
- Fresh Play Mode regression: **237 PASS** — all 11 locations before/after purchases, ten-dish day, carousel wrap, Cook/Back/Continue/Finish and repeat callback guards. Existing Back raycast/pointer test: **3 PASS**.
- Actual SampleScene reload in Play Mode: **14 PASS** — new controller/session retained the same funded profile and purchased ranks/XP; old unfinished order invalidated; a fresh day started and finished without additional rewards. No scene asset write occurred.
- Existing UI presentation **240 PASS**, empty-pool/mastery-cap fixtures **10 PASS**.
- Existing rendered-frame scene runner: **176 lifecycle/input + 14,036 spatial/text/glyph/raycast assertions PASS**, all 11 locations, physical board/plate targets, same-order Back, exact completion/replacement, ten receipts and cleanup at the current Game View size. This was not a repeat of the five-resolution UI-A4 matrix. The runner was initially stalled by Unity's normal unfocused Game View behavior; `Application.runInBackground` was temporarily enabled at runtime and restored to false before stopping. No project setting was edited.
- A second fresh Play Mode entry explicitly verified zero wallet, empty XP/rank snapshots, no active day and the controller bound to the run owner. Play Mode then stopped; SampleScene clean. The domain-reload-disabled configuration itself was not toggled/tested; the SubsystemRegistration reset hook is present.
- No new ClickCook runtime exceptions observed. Only the pre-existing Plastic/UVCS missing client configuration exception remains. These tests used Unity MCP, model calls and EventSystem/button callbacks; no new physical keyboard/mouse or standalone build test is claimed.
- `SampleScene`, static data, environment, packages and UI assets unchanged. Four pre-existing settings edits remain preserved and excluded from the checkpoint: PC_RPAsset, UniversalRenderPipelineGlobalSettings, ProjectSettings and UnityConnectSettings.

### Intentionally unfinished / next step

7A is complete only for run-local progress ownership. No save file, restore, autosave, New Game/reset UI, Player/Restaurant XP, new upgrade effects or balance changes. Expanded tree and other systems remain pending. Next separate milestone: **7B — local save/load and associated menu/recovery behavior**, after explicitly documenting the interrupted-day/New Game/reset/pause policies. Do not start 7B automatically.

## Scoped burger presentation and updated Dormitory recovery — completed 2026-10-21

### Preserved scene / repair

- The user manually replaced `Assets/Models/Snene_Dormitory/Dormitory.fbx`, its GUID/import mappings and materials, created a variant prefab, and moved the scene camera/environment. The old GUID left the board's two paper renderers and Plate unbound, causing CounterScenePresentation's required-reference guard to disable scene interaction.
- Before editing, copied the exact saved SampleScene/metadata and entire current environment folder to local `ArtSource/Recovery/Dormitory_BeforeRepair_2026-10-21/`; README records the snapshot. This recovery copy is outside Assets and excluded from the Git checkpoint. The new FBX is retained byte-for-byte: SHA-256 `C9C44279EC2325D619DBAD0C95B2C0CE58805334B3077616F55E6DAFC82F3149` matches the backup.
- Rebound `boardPaperLeft`, `boardPaperRight` and `plate` to the replacement model's Sticker_1, Sticker_2 and Plate. CounterScenePresentation derives board orientation from the paper mesh's thinnest axis, facing the existing camera; the new FBX uses different local axes. Real world-space sheets and plate-based dish positioning work again.
- Unity's RemoveUnusedOverrides removed 152 unresolved modifications targeting the deleted import, retaining 17 live overrides. All 39 environment transforms were compared unchanged. New model/prefab/materials, camera position `(0.95, 3.96, -3.22)` and authored rotation, lighting/volume and existing UI hierarchy preserved. Saved SampleScene reopened with valid bindings, zero missing scripts; all 38 environment renderers have valid meshes/materials/supported shaders.

### Previously interrupted burger request completed

- Preserved the existing Blender-authored FBX and color atlas. New `Assets/Models/Food/Burger/BurgerAssembly.prefab` groups 15 meshes / 1,306 triangles into six stages, with all nine sesame meshes traveling with the top bun. `Burger_URP.mat` uses the shared 512×512 atlas.
- New `Assets/Scripts/UI/BurgerAssemblyPresentation.cs`: bottom bun initially present; subsequent layers descend from above as accepted HP damage advances. Short settle/squash feedback is presentation only. Back resets the visual, result completes it, stronger damage skips visual thresholds correctly; no extra reward/cooking action is generated.
- Modified `CounterScenePresentation.cs` and `CounterDishPointer.cs`: Dish ID 6 uses the burger; other dishes retain their current sketches. A compact left-side burger name panel and slightly adjusted burger look-at clear the ingredient area. The original large title initially obscured the falling top bun; the targeted presentation adjustment resolved it.
- New explicit authoring helper `Assets/Scripts/Editor/BurgerAssemblySetup.cs`, new `Assets/Tests/Editor/BurgerAssemblyVerification.cs` and script metadata. Existing `CounterSceneVerification.cs` now includes the burger as the fourth visual variant; its former three-sketch-only assertion was obsolete and updated.
- Static DishData/LocationData/catalogs, HP/damage, rarity, payout, XP and profile formulas unchanged. This is an out-of-roadmap presentation/repair checkpoint; 7B remains unstarted.

### Verification executed

- Unity compilation PASS; no C# failures. Existing model **4,685 PASS**, profile/IDs **58 PASS**, fresh Play Mode loop **237 PASS**, UI presentation **240 PASS**, empty/mastery edge **10 PASS**.
- New burger checks **48 PASS** in the updated environment: six stages/sesame grouping, accepted pointer click, HP-driven motion, Back/reset/same identity, one completion/payment, stale Submit guard, cleanup/fallback collider and max-strength one-hit assembly. Max-strength coverage used a disposable Editor-only wallet/purchase fixture; no asset/player save was edited.
- All 11 locations passed frame-settled CounterScene checks at actual input/render sizes: 1280×720 (**176 + 14,140**), 1920×1080 (**176 + 14,133**), 2560×1440 (**176 + 14,123**), 1920×1200 (**176 + 14,107**) and 2560×1080 (**176 + 14,188**). Total **880 lifecycle/input + 70,691 spatial/text/glyph/raycast assertions**. The first 720p run reported a one-off sheet raycast failure; subsequent inspection found all sheet rays valid and a complete repeated 720p run passed. No assertion was removed or relaxed for it. Temporary size preset restored before stopping; runtime-only runInBackground restored to false.
- Native Computer Use mouse PASS in the new scene: a location-2 board prepared through MCP; native burger sheet selection, HP 42→41/descending patty, Back with all three IDs unchanged and no payment, native reselect resetting HP/assembly, final native click after 41 MCP cooking callbacks, Common burger result/40.00 wallet/+10 XP/one completion, native Continue preserving replacement/other IDs and wallet, native Finish clearing the day and retaining earned wallet. This was mixed MCP/native verification, not 42 physical clicks. The new counter, ingredient poses, unobscured name layout and result were visually inspected. Physical keyboard and standalone build were not retested.
- Only the known pre-existing Plastic/UVCS client configuration exceptions remain; no new ClickCook runtime exception observed. Play Mode stopped, scene saved clean. Existing four settings edits, other-chat art sources/reference documents and the local recovery snapshot remain separate from the runtime checkpoint.

Documentation synchronized: GDD, this tracker, Production Roadmap and UI/UX Specification. Next numbered milestone stays **7B**; do not start automatically.

## Documentation policy (ongoing)

After every completed and Unity-verified milestone, update both `ClickCook_GDD.md` and this file. Do not mark an entire gameplay system complete when only static data or placeholder UI exists.
