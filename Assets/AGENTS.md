# Project guidance

This Unity project is a dice game. Inspect the relevant scene, prefab, and configuration assets as well as scripts before changing behavior that depends on Inspector assignments. Preserve existing serialized fields and Unity `.meta` files unless a change requires otherwise.

## Code map

- `Scripts/Managers/GameLifetimeScope.cs` registers configuration assets, scene components, services, and the `Bootstrap` entry point with VContainer. Register new injected dependencies here when appropriate; scene references also require Inspector setup.
- `Scripts/Managers/TurnFlowController.cs` coordinates throwing, rerolls, scoring, code-lock attempts, and end-of-turn sequences. Async flows use UniTask; preserve interaction locks and lifecycle cancellation when changing them.
- `Scripts/Managers/LevelFlowController.cs` starts levels, checks defeat, and advances or ends the game after a win.
- `Scripts/Dices/DiceData.cs` holds per-die state, face upgrades, durability, damage, and change events. `Dice.cs` connects that data to physics, input, and `DiceView`; `DiceTableController` manages dice on the table. Face indices assume six faces.
- `Scripts/Managers/ScoreCalculator.cs` builds a `ScoreSequencePlan` from the selected dice, combination, face bonuses, and charm triggers. `ScoreSequenceController` plays the resulting sequence. Keep the calculation and presentation steps consistent.
- `Scripts/Combinations/CombinationEvaluator.cs` returns the first matching active combination, so ordering matters. Combination configuration and rules live under `Scripts/Combinations`.
- `Scripts/Charms/Instances/CharmBase.cs` defines hooks for rolls, scoring, damage, and degradation; concrete charms and their configs live under `Scripts/Charms`.
- `Scripts/Configs` contains gameplay configuration assets; `Scripts/UI` contains views and UI sequences. Editor-only tooling lives in `Scripts/Editor`.

## Working conventions

- Do not use Auto Size for text components. Set font sizes explicitly.
- Use `Scripts/UI/Universal/TextRevealView.cs` for reusable character-by-character text reveal and erase animations.

- Use the section divider `// ===== ===== ===== ===== ===== ===== ===== ===== ===== =====` in scripts to mark meaningful boundaries between member groups. Put one after the class opening before the first group of fields/properties, and another after that group before the next logical section (for example, fields before lifecycle methods, injected dependencies before public behavior, or events before related state). Add further dividers only when they make a substantial section change easier to scan; do not put one between every method or small related group. Keep the divider on its own line and match the surrounding indentation.
- Do not add unnecessary null checks for required references or dependencies. Let configuration and programming errors surface as exceptions; use null checks only when absence is a valid state or required by object lifecycle.

- Follow existing C# naming, namespaces, event patterns, and dependency injection conventions in the code being changed. Subscribe and unsubscribe to events and input actions with matching lifetimes.
- Review callers and listeners when changing data mutations or events. In particular, `DiceData` exposes separate notifications for data, lock, and value changes; face bonuses are accounted for separately in scoring.
- Preserve the ordering of score and charm effects unless the requested change explicitly alters it.
