# 8A — Day summary contract

Implemented and locally verified 2026-10-28.

**Owner correction, implemented 2026-10-28:** finishing before10 dishes forfeits all order money from that day. Existing wallet money is retained. Dish XP remains awarded per completed dish. This replaces the original immediate per-order wallet credit described by historical evidence below.

- A day contains only successful `PrototypeDishResult` receipts issued by its session. All cooking sources already converge on that boundary. Duplicate dish types count as separate completions.
- Order values accumulate in session-only `PendingDayEarnings`, including existing quality and Dish Value effects. At the tenth successful receipt the sum moves into Wallet once, in the same autosave notification as that receipt's XP. Summary Earnings is that sum for a full day, otherwise0; ForfeitedEarnings is the pending receipt sum for an early day. Starting wallet, spending, unfinished HP, candidates and passive income are excluded.
- Reported XP is the exact sum of **Dish XP actually granted**, including fractional hundredths and mastery caps. It is neither Player XP nor Restaurant XP. The proposed Daily Earnings / 100 formula belongs to 8C and is not awarded here.
- Manual Finish Day is available from the board, including an empty day. It cannot finish while cooking or paused. Continue after the tenth result uses the same finish boundary. Finishing returns a detached read-only receipt list, then discards remaining orders and releases profile ownership. Repeating finish is rejected.
- Summary reports the settled or forfeited outcome. Reading, upgrade return, Next Day and replayed callbacks never pay again. Early EndDay/menu/scene cleanup discard pending money. Saving/restarting midway cannot retain it because it never enters the persistent wallet. Full payment is already final on the tenth receipt, even before Continue. Existing failed-write/retry handling is reused; no new schema is necessary.
- Upgrades may open from either location selection or summary. Close returns to its entry screen with the corresponding button label. Purchases change the profile, while the frozen day report retains its original earnings/XP.
- Next Day consumes the visible summary and returns to location selection once; Cook starts a fresh board at the selected location. A calendar date, day ordinal, timer or automatic idle payout is not inferred. Calendar progression remains an explicit future design decision.
- Main menu, scene exit and a new run dismiss the report. Progress survives through the existing save; report history and unfinished days are not persisted. Restoring a profile begins idle and cannot replay a day-end reward.

## Verification of the owner correction

Changed behavior is the cash settlement boundary. Risks are keeping early money through autosave/restart, paying twice at full completion, carrying voided income into the next day or removing old wallet/XP. Smallest checks: compilation, all early counts1–9/full tenth with isolated save roundtrips, existing scoped summary model and controller/result/summary/restart boundaries in Play.

`DaySettlementVerification.RunModelChecks` passed every early count1–9 and full tenth, exact existing balance, pending money, receipt autosave, fractional XP/value effects, replay/cleanup/no-carry and immutable settlement report. Updated DaySummary model passed. Play with three completed dishes produced Earnings0, nonzero ForfeitedEarnings and zero wallet/save money while retaining exact XP; result/summary/warning text fits. A subsequent ten-dish day settled once on its tenth receipt, with no prior-day carry, and retained its exact balance after actual stop/restart/Continue. Early summary was visually inspected in the real Game View through Computer Use. Pause and Finish warnings explicitly explain forfeiture. Play stopped, override removed and saved scene reopened clean. No Player build or unrelated suite was needed.

## Historical initial8A evidence (before the cash correction)

Before verification: changed behavior is receipt aggregation and day-end navigation. Risks are duplicate reward/XP, inclusion of abandoned work or starting wallet, stale callbacks, wrong upgrade-return destination, unreadable ten-row report and save mismatch. Smallest relevant checks were compilation, isolated model cases and the new controller/UI integration boundary.

- `DaySummaryVerification.RunModelChecks`: empty/early/ten completed receipts, duplicate dishes, decimal earnings/fraction XP, paused/active/stale guards, immutable reports across days, partial/Back exclusion, owner release and no additional profile notifications/rewards.
- `VerifyPlay` on a disposable profile: empty report, tenth-result report, exact receipt totals, stale controller callbacks, summary→upgrades→summary→Next Day, initial focus, ten-row text height and exact saved progress loaded into an idle profile. One initial MCP request timed out during Play startup; a state read showed the test had not advanced from the main menu, and the bounded retry passed.
- Native Computer Use: resumed focus-loss pause, inspected the ten-row report in the Full-HD Game View rendered at 0.45 display scale; clicked Upgrades, purchased strength rank 1 for 20, returned to the same report, clicked Next Day. Earnings90/Dish XP110 remained frozen; wallet70/rank1 matched disk. Controller calls prepared the completed dishes; this is not a claim of ten dishes cooked with physical input.
- Final contextual-label check: summary entry says «До підсумку», location entry says «До локацій». No repeat of unchanged receipt tests needed for this copy adjustment.
- Scene serialization: zero existing blocks removed; added summary UI and controller bindings. Existing canvas child list changes plus one whitespace-only UpgradeTreeView normalization. Environment/camera/lights retained.

No Player build, physical keyboard pass, broad regression or resolution matrix. Only known Unity VCS configuration exceptions and the recorded MCP startup timeout appeared; no ClickCook runtime exception was observed. Temporary save override removed and Play stopped after verification.

Next: 8B recipe ownership and availability contract, then purchase/eligibility runtime implementation.
