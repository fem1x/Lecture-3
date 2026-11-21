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

- Follow existing C# naming, namespaces, event patterns, and dependency injection conventions in the code being changed. Subscribe and unsubscribe to events and input actions with matching lifetimes.
- Review callers and listeners when changing data mutations or events. In particular, `DiceData` exposes separate notifications for data, lock, and value changes; face bonuses are accounted for separately in scoring.
- Preserve the ordering of score and charm effects unless the requested change explicitly alters it.
- Validate changes in Unity when available, including compilation and the relevant gameplay flow. If Unity or suitable automated tests are unavailable, state what was not verified rather than claiming a runtime check.
