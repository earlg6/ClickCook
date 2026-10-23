# ClickCook project instructions

## Verification policy (owner instruction, 2026-10-22)

- Before any check, test, Play Mode run or build, identify the changed behavior, the concrete risk and the smallest check that can resolve it. Skip checks that provide no relevant new evidence.
- Keep verification local to the changed files, subsystem and affected integration boundary. Do not rerun unrelated gameplay, UI, resolutions or regression suites merely because they exist.
- Reuse existing valid evidence for unchanged behavior. Repeat a passing check only after a relevant change, a failure or a newly identified unresolved concern.
- Documentation/data-contract changes normally need only consistency and targeted data checks; do not trigger Unity compilation, Play Mode or a Player build without a runtime reason.
- Build only when the change requires Player-specific evidence or the owner requests a build. Broad release QA belongs to its own milestone.
- Report briefly what was checked and any material gap. Do not inflate assertion counts or verification scope.

These instructions apply throughout this repository and to future milestones.
