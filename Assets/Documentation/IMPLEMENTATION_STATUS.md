# ClickCook Implementation Status

## Completed milestone

**Milestone 1 — ClickCook Data Foundation**

Static ScriptableObject data for Locations and Dishes is implemented. This milestone does not add gameplay, UI, save/load, economy runtime behavior, progression, rarity, or scene changes.

## Files created

- `Assets/Scripts/Data/DishData.cs`
- `Assets/Scripts/Data/LocationData.cs`
- `Assets/Data/Dishes/` — 32 `DishData` assets
- `Assets/Data/Locations/` — 11 `LocationData` assets
- `Assets/Documentation/IMPLEMENTATION_STATUS.md`

## Implemented

- `DishData`: documented numeric ID, English and Russian display names, sale value, recipe cost, click requirement, and explicitly documented location availability.
- `LocationData`: level requirement, English and Russian display names, passive income per second, and documented dish availability.
- Bidirectional references are populated from the explicit **Known locations** entries in the GDD dish table. This preserves each source relationship without adding runtime behavior.

## Known GDD ambiguities / TBDs retained

- The GDD separately describes new recipe groups and dishes served by locations; their precise relationship and recipe purchase gating are TBD. These assets model only documented dish availability, not ownership or recipe unlocks.
- Several location descriptions use incomplete phrases such as “also serves selected ... dishes.” No extra dishes were inferred from those phrases.
- The Michelin restaurant says “all dishes starting from the third restaurant,” which conflicts with / is less precise than its criteria dish list. Dishes 29–32 are linked to the Michelin location because the GDD labels their known location as “Michelin restaurant criteria list”; no additional earlier dishes are inferred from that phrase.
- The click requirement is stored exactly as the documented number of clicks. Its relation to future hidden HP/progress remains TBD.
- Location ownership/unlock state, passive-income accrual/collection, recipes, order generation, and all other runtime rules remain unimplemented by design.

## Next recommended milestone

**Define the minimal runtime session/state architecture and the player-owned recipe/location persistence boundaries after the GDD’s recipe-unlock and Michelin-availability ambiguities are decided.**
