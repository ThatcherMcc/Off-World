# Project Restructuring Ideas (Off-World)

Ideas for organizing `Assets/Scripts` and related assets. Pick what fits your workflow; no need to do everything at once.

---

## 1. **Reduce root clutter in Scripts**

Several scripts live at `Assets/Scripts/` next to domain folders. Consider grouping by concern:

| Current (root) | Suggestion | Reason |
|----------------|------------|--------|
| `BaseAI.cs` | `Scripts/Enemies/BaseAI.cs` or `Scripts/Core/AI/BaseAI.cs` | Used only by enemy AI (Wolf, etc.); keep near `IEnemy` and other enemies. |
| `IEnemy.cs` | Keep in `Enemies/` (already there) | — |
| `InteractableI.cs` | `Scripts/Core/InteractableI.cs` or `Scripts/Interaction/` | Shared by player interaction; core contract. |
| `TeleportDoor.cs` | `Scripts/Gameplay/TeleportDoor.cs` or `Scripts/Level/` | Level/flow logic, separate from player/enemy. |
| `checkHumanoid.cs` | `Scripts/Editor/CheckHumanoid.cs` or delete if one-off | Editor/debug helper; name with PascalCase. |

**Recommendation:** Add a `Core/` (or `Shared/`) folder for interfaces and base types used across domains; move level/gameplay one-offs into `Gameplay/` or `Level/`.

---

## 2. **Naming consistency**

- **PascalCase for type names:** Rename `checkHumanoid.cs` → `CheckHumanoid.cs`, `rotatePlayer.cs` → `RotatePlayer.cs` so file names match class names and C# conventions.
- **Folder names:** Avoid spaces. `Power Items` → `PowerItems` (folder and namespace if you add one). Unity and tools handle spaces, but scripts and refs are cleaner without.
- **Typo:** `Terrain/ChuckManager.cs` → `ChunkManager.cs` (class is already `ChunkManager`; only the file name is wrong).

---

## 3. **Behavior tree placement and namespaces**

- **Current:** `BehaviorTrees/` and `Strategies.cs` use `BossFight.BehaviorTrees` and `BossFight.Strategies`; only RockBoss uses them.
- **Options:**
  - **A)** Keep at root as shared AI: `Scripts/AI/BehaviorTrees/` and `Scripts/AI/Strategies/` (or one `Scripts/AI/` with subfolders). Use a single namespace, e.g. `OffWorld.AI` or keep `BossFight` if you prefer.
  - **B)** Move under RockBoss: `Scripts/Enemies/RockBoss/BehaviorTrees/` and `Scripts/Enemies/RockBoss/Strategies/` so it’s clear they’re RockBoss-specific until another boss uses them.

Choose A if you plan more behavior-tree bosses; choose B for minimal change and clear ownership.

---

## 4. **Enemy layout**

- **Current:** Flat list under `Enemies/` plus `Enemies/RockBoss/` for one boss.
- **Idea:** One folder per “major” enemy type that has multiple scripts, e.g.:
  - `Enemies/RockBoss/` (already exists)
  - `Enemies/Wolf/` – move `WolfAI.cs`, `WolfAttack.cs` into it
  - Keep single-script enemies (`EnemyAI`, `FrogAI`, etc.) in `Enemies/` root, or group in `Enemies/Common/` for shared enemy logic.

This keeps each boss/mini-boss self-contained and scales when you add more.

---

## 5. **Shared vs domain-specific**

- **Interfaces:** `IEnemy`, `InteractableI`, `PowerItemI` – consider a single `Scripts/Core/Interfaces/` (or `Contracts/`) so all contracts live in one place.
- **Player:** Already grouped under `Player/`; good. Optionally rename `rotatePlayer.cs` to `RotatePlayer.cs` for consistency.
- **Items:** `Items/` with `Power Items/` subfolder is fine; renaming to `PowerItems` would align with “no spaces” and future code references.

---

## 6. **Editor-only and build safety**

Several runtime scripts reference `UnityEditor` (e.g. `Node.cs`, `Strategies.cs`, `PlayerHealth.cs`, `FrogAI.cs`, `TerrainGeneration.cs`, `AnimationPlayer.cs`). Those references will break in builds.

- **Fix:** Remove unused `using UnityEditor.*` or wrap editor-only code in `#if UNITY_EDITOR` and keep it in `Editor/` or clearly marked editor scripts.
- **Recommendation:** Audit all `UnityEditor` usages; for behavior tree and RockBoss, remove or guard any Editor references so the built game runs.

---

## 7. **Suggested target layout (high level)**

```
Scripts/
├── Core/                    # Shared contracts and base types
│   ├── InteractableI.cs
│   └── (optional) Interfaces/
│       ├── IEnemy.cs
│       └── ...
├── AI/                      # Or keep under Enemies/RockBoss
│   ├── BehaviorTrees/
│   └── Strategies (or same folder)
├── Camera/
├── Enemies/
│   ├── BaseAI.cs
│   ├── RockBoss/
│   ├── Wolf/                # optional grouping
│   └── ...
├── Gameplay/ or Level/
│   └── TeleportDoor.cs
├── Player/
├── Terrain/
├── UserInterface/
├── Villager/
├── Items/
├── ProceduralAttack/
└── Editor/                  # Editor-only utilities
    └── CheckHumanoid.cs
```

You can adopt this gradually: start with naming fixes and ChuckManager → ChunkManager, then move one folder at a time and fix references.

---

## Summary

- **Quick wins:** Fix `ChuckManager.cs` → `ChunkManager.cs`, `checkHumanoid.cs` → `CheckHumanoid.cs`, `rotatePlayer.cs` → `RotatePlayer.cs`; remove or guard `UnityEditor` in runtime scripts.
- **Medium:** Move root-level scripts into `Core/`, `Gameplay/`, or `Enemies/`; optionally group Wolf (and future bosses) in subfolders.
- **Larger:** Introduce `Core/` and `Editor/`, standardize namespaces (e.g. `OffWorld.*`), and optionally move behavior tree under `AI/` or keep under `Enemies/RockBoss/`.

After you choose a direction, you can do the moves in small steps and run the game after each change to avoid broken references.
