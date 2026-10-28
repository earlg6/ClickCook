# 8A — Day summary contract

Implemented and locally verified 2026-10-28.

- A day contains only successful `PrototypeDishResult` receipts issued by its session. All cooking sources already converge on that boundary. Duplicate dish types count as separate completions.
- Earnings are the exact decimal sum of receipt payouts, including existing quality and Dish Value effects. Starting wallet, spending, unfinished HP, candidate dishes and future passive income are excluded.
- Reported XP is the exact sum of **Dish XP actually granted**, including fractional hundredths and mastery caps. It is neither Player XP nor Restaurant XP. The proposed Daily Earnings / 100 formula belongs to 8C and is not awarded here.
- Manual Finish Day is available from the board, including an empty day. It cannot finish while cooking or paused. Continue after the tenth result uses the same finish boundary. Finishing returns a detached read-only receipt list, then discards remaining orders and releases profile ownership. Repeating finish is rejected.
- Summary is a report of completed transactions. Opening, reading, returning from upgrades, Next Day, menu/scene cleanup and replayed callbacks never pay or save an additional reward. Existing receipt/purchase autosave remains authoritative; a failed write uses the existing retry notice. No new schema is necessary.
- Upgrades may open from either location selection or summary. Close returns to its entry screen with the corresponding button label. Purchases change the profile, while the frozen day report retains its original earnings/XP.
- Next Day consumes the visible summary and returns to location selection once; Cook starts a fresh board at the selected location. A calendar date, day ordinal, timer or automatic idle payout is not inferred. Calendar progression remains an explicit future design decision.
- Main menu, scene exit and a new run dismiss the report. Progress survives through the existing save; report history and unfinished days are not persisted. Restoring a profile begins idle and cannot replay a day-end reward.

## Local evidence

Before verification: changed behavior is receipt aggregation and day-end navigation. Risks are duplicate reward/XP, inclusion of abandoned work or starting wallet, stale callbacks, wrong upgrade-return destination, unreadable ten-row report and save mismatch. Smallest relevant checks were compilation, isolated model cases and the new controller/UI integration boundary.

- `DaySummaryVerification.RunModelChecks`: empty/early/ten completed receipts, duplicate dishes, decimal earnings/fraction XP, paused/active/stale guards, immutable reports across days, partial/Back exclusion, owner release and no additional profile notifications/rewards.
- `VerifyPlay` on a disposable profile: empty report, tenth-result report, exact receipt totals, stale controller callbacks, summary→upgrades→summary→Next Day, initial focus, ten-row text height and exact saved progress loaded into an idle profile. One initial MCP request timed out during Play startup; a state read showed the test had not advanced from the main menu, and the bounded retry passed.
- Native Computer Use: resumed focus-loss pause, inspected the ten-row report in the Full-HD Game View rendered at 0.45 display scale; clicked Upgrades, purchased strength rank 1 for 20, returned to the same report, clicked Next Day. Earnings90/Dish XP110 remained frozen; wallet70/rank1 matched disk. Controller calls prepared the completed dishes; this is not a claim of ten dishes cooked with physical input.
- Final contextual-label check: summary entry says «До підсумку», location entry says «До локацій». No repeat of unchanged receipt tests needed for this copy adjustment.
- Scene serialization: zero existing blocks removed; added summary UI and controller bindings. Existing canvas child list changes plus one whitespace-only UpgradeTreeView normalization. Environment/camera/lights retained.

No Player build, physical keyboard pass, broad regression or resolution matrix. Only known Unity VCS configuration exceptions and the recorded MCP startup timeout appeared; no ClickCook runtime exception was observed. Temporary save override removed and Play stopped after verification.

Next: 8B recipe ownership and availability contract, then purchase/eligibility runtime implementation.
