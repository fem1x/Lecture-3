# Project guidance

This Unity project is a dice game. Before changing behavior that depends on Inspector assignments, inspect the relevant scripts, scenes, prefabs, and configuration assets. Preserve serialized fields, asset GUIDs, references, and Unity `.meta` files unless a change requires otherwise.

## Code map

- `Scripts/Managers/GameLifetimeScope.cs` registers configuration assets, scene components, services, and the `Bootstrap` entry point with VContainer. Register new injected dependencies here when appropriate; assign scene references in the Inspector.
- `Scripts/Managers/TurnFlowController.cs` coordinates throws, rerolls, scoring, code-lock attempts, and end-of-turn sequences. Preserve interaction locks and lifecycle cancellation in UniTask flows.
- `Scripts/Managers/LevelFlowController.cs` starts levels, checks defeat, and advances or ends the game after a win.
- `Scripts/Dices/DiceData.cs` holds die state, face upgrades, durability, damage, and change events. `Dice.cs` connects it to physics, input, and `DiceView`; `DiceTableController` manages table dice. Face indices assume six faces.
- `Scripts/Managers/ScoreCalculator.cs` builds a `ScoreSequencePlan` from selected dice, combination, face bonuses, and charm triggers; `ScoreSequenceController` plays it. Keep calculation and presentation consistent.
- `Scripts/Combinations/CombinationEvaluator.cs` returns the first matching active combination, so ordering matters. Configuration and rules are under `Scripts/Combinations`.
- `Scripts/Charms/Instances/CharmBase.cs` defines roll, scoring, damage, and degradation hooks; concrete charms and configs are under `Scripts/Charms`.
- `Scripts/Configs` contains gameplay configurations, `Scripts/UI` contains views and sequences, and `Scripts/Editor` contains editor-only tooling.

## Working conventions

- Reuse existing architecture, components, and utilities. Keep changes focused; avoid unrelated refactoring and unnecessary abstractions.
- Skip generated directories such as `Library`, `Temp`, `Logs`, `obj`, and build outputs unless the task requires them.
- Prefer targeted validation when sufficient. Follow the existing restriction on running checks in Unity.
- Do not generate large object hierarchies at runtime. Prepare cameras, lights, models, and other scene objects in a prefab or scene and assign references in the Inspector.

- Do not run tests or checks in Unity unless the task requires them or the user explicitly requests them.
- Do not use Auto Size for text components. Set font sizes explicitly.
- Build repeated UI elements with the same design, such as buttons, as instances of a shared prefab so their common appearance and behavior can be updated centrally. Use prefab variants or instance overrides for intentional differences.
- Use `Scripts/UI/Universal/TextRevealView.cs` for reusable character-by-character reveal and erase animations.
- Use `Scripts/UI/Universal/StaggeredRevealView.cs` for staggered fade and scale reveals of UI lists. Configure elements, delay, duration, stagger, and easing in the Inspector.
- Use `Scripts/UI/Universal/PanelRevealView.cs` for panels revealing downward from a header. Put the header, background, and body under its dedicated `RectMask2D`; configure delay, duration, and easing in the Inspector. Assign it to `SlidingPanelView` for sliding panels, or call `HideInstant` before `Show` when used alone.

- Use `// ===== ===== ===== ===== ===== ===== ===== ===== ===== =====` on its own line with matching indentation as a section divider. Place one after the class opening before fields/properties and another after that group. Add more only at substantial boundaries between member groups, not between every method or small related group.
- Do not add null checks for required references or dependencies; let configuration and programming errors surface as exceptions. Check for null only when absence is valid or required by lifecycle.

- Follow existing C# naming, namespaces, event patterns, and dependency injection conventions. Match event and input-action subscriptions with unsubscriptions over the same lifetime.
- Review callers and listeners when changing mutations or events. `DiceData` has separate data, lock, and value notifications; face bonuses are scored separately.
- Preserve the ordering of score and charm effects unless the requested change explicitly alters it.

## Performance and references

- In frequently called code, avoid unnecessary allocations, expensive lookups, and redundant work. Cache results when their lifetime and invalidation are clear.
- Do not use Unity's `Find...` APIs to locate objects in gameplay code. Use Inspector references or VContainer injection instead. This restriction does not apply to editor-only tooling.
- Use `GetComponent` in gameplay code only when clearly justified; prefer Inspector references or VContainer injection. Cache the result when reused, especially in frequently called code.
- Use `AddComponent` in gameplay code only when runtime creation is clearly justified. Otherwise, prepare components in the scene or prefab.
- Keep code readable; pursue broader optimizations when profiling or other evidence identifies a bottleneck. Preserve gameplay behavior and effect ordering.

## DOTween Rules

- **Safety:** Always append `.SetLink(gameObject)` to every Tween and Sequence to prevent `MissingReferenceException` on object destruction.
- **No Leaks:** Never create tweens inside `Update()`, `FixedUpdate()`, or `LateUpdate()`. Before starting a tween, kill any previous tween that would conflict with it. Prefer a stored tween reference (e.g., `tween?.Kill()`) so unrelated animations on the same target continue.
- **UI & Time:** For UI animations that should continue while the game is paused, explicitly append `.SetUpdate(true)` so they ignore `Time.timeScale = 0`. Let other UI animations follow game time when appropriate.

## Adding sound effects

- Put new clips under `Audio/` in the appropriate category. Add a uniquely named entry to `!Configs/SfxConfig.asset` and assign the clips to a layer, along with the volume and the existing SFX mixer group. Each layer picks one of its clips at random.
- Inject `ISfxPlayer` through VContainer and call it when the action starts. For UI sounds, set `is3D` and `useGlobalSpatialBlend` to false, leave `customSpatialBlend` at zero, and call `Play("SoundName")`. For positional sounds, enable `is3D` and call `Play("SoundName", worldPosition)`.

## Local localization workflow

- Read existing keys and translations from `Settings/Localization`. For string changes, queue a UTF-8 JSON file named `LocalizationChanges.json` at the Unity project root, next to `Assets`. Let Unity write the string table assets.
- The file has the shape `{"entries":[{"key":"example_key","en":"…","ru":"…","es":"…","ja":"…","ko":"…","zhHans":"…"}]}`. Include only new or changed keys, with all six texts for each row. Preserve placeholders, rich-text tags, and sprite names where applicable. Add `"smart":true` for new Smart String entries.
- Publish the patch by writing a temporary sibling file and renaming it into place when complete. Check for a pending file first and preserve its edits. The editor merges the patch automatically in Edit Mode. `Tools → Localization → Apply Local Changes` retries a failed patch on demand.
- A successful import moves the patch to `Library/AppliedLocalization` and logs a receipt. Check the receipt and the diff of the Unity assets. If Unity is closed or still in Play Mode, report that the patch is pending. Include the resulting Unity table assets in the changes; commit them only when a commit is requested.
