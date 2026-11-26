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

- For localization edits, use the local Unity import queue described below.

- Do not run tests or checks in Unity unless the task requires them or the user explicitly requests them.
- Do not use Auto Size for text components. Set font sizes explicitly.
- Use `Scripts/UI/Universal/TextRevealView.cs` for reusable character-by-character text reveal and erase animations.
- Use `Scripts/UI/Universal/StaggeredRevealView.cs` for staggered fade and scale reveals of lists of UI elements. Configure the elements, delay, duration, stagger, and easing in the Inspector.
- Use `Scripts/UI/Universal/PanelRevealView.cs` for panels that reveal their contents downward from a header. Put the header, background, and body under the panel's dedicated `RectMask2D`. Configure the delay, duration, and easing in the Inspector. Assign the component to `SlidingPanelView` when the panel also slides, or call `HideInstant` before playing `Show` on its own.

- Use the section divider `// ===== ===== ===== ===== ===== ===== ===== ===== ===== =====` in scripts to mark meaningful boundaries between member groups. Put one after the class opening before the first group of fields/properties, and another after that group before the next logical section (for example, fields before lifecycle methods, injected dependencies before public behavior, or events before related state). Add further dividers only when they make a substantial section change easier to scan; do not put one between every method or small related group. Keep the divider on its own line and match the surrounding indentation.
- Do not add unnecessary null checks for required references or dependencies. Let configuration and programming errors surface as exceptions; use null checks only when absence is a valid state or required by object lifecycle.

- Follow existing C# naming, namespaces, event patterns, and dependency injection conventions in the code being changed. Subscribe and unsubscribe to events and input actions with matching lifetimes.
- Review callers and listeners when changing data mutations or events. In particular, `DiceData` exposes separate notifications for data, lock, and value changes; face bonuses are accounted for separately in scoring.
- Preserve the ordering of score and charm effects unless the requested change explicitly alters it.

## Local localization workflow

- Read existing keys and translations from `Settings/Localization`. For string changes, queue a UTF-8 JSON file named `LocalizationChanges.json` at the Unity project root, next to `Assets`. Let Unity write the string table assets.
- The file has the shape `{"entries":[{"key":"example_key","en":"…","ru":"…","es":"…","ja":"…","ko":"…","zhHans":"…"}]}`. Include only new or changed keys, with all six texts for each row. Preserve placeholders, rich-text tags, and sprite names where applicable. Add `"smart":true` for new Smart String entries.
- Publish the patch by writing a temporary sibling file and renaming it into place when complete. Check for a pending file first and preserve its edits. The editor merges the patch automatically in Edit Mode. `Tools → Localization → Apply Local Changes` retries a failed patch on demand.
- A successful import moves the patch to `Library/AppliedLocalization` and logs a receipt. Check the receipt and the diff of the Unity assets. If Unity is closed or still in Play Mode, report that the patch is pending. Commit the resulting Unity table assets.
