# RockBoss Directory Overview & Finalized Behavior

## 1. RockBoss Directory – Usable Parts

| File | Purpose | Usable for 5-attack design |
|------|--------|----------------------------|
| **CharacterInteraction.cs** | Main controller: wake-up, healthbar, behavior tree. Builds the BT in `Start()`, runs `tree.Process()` in `Update()` when boss is up and player in notice range. | **Core.** All attack/movement logic lives here as tree nodes. Add conditions (e.g. `PlayerIsBehind`) and new `GuardedSequence`s for Stomp and Slam. |
| **RockBossHeadLook.cs** | Procedural head look + root rotation toward player. `IsLookingAtPlayer()` = true when within `maxAngToTarget`. Turn head off during certain anims via `enabled = false`. | **Keep.** Used for “facing player” gates on front attacks. Add “player behind” in CharacterInteraction (dot/angle) for Stomp. |
| **AnimationMethodsRockBoss.cs** | Animation events: enables/disables punch colliders (`ActivateLeftPunch` / `ActivateRightPunch` and matching `Deactivate`), `LaunchRock()` instantiates the rock prefab (which uses **EnemyArcProjectile**). | **Extend.** Add `ActivateStompBack` / `DeactivateStompBack` (and optional `ActivateSlam` / `DeactivateSlam`) for new attacks; hook from animator events. |
| **EnemyArcProjectile.cs** | Used on the **rock prefab**. On `Start()` computes arc to player and applies velocity; destroys after `lifeDuration`. | **Keep as-is** for rock throw. |

**Dependencies:** `EnemyHealth` (same GameObject), `BossFight.BehaviorTrees` + `BossFight.Strategies` (Node, Leaf, Condition, AnimationWaitStrategy, ChasePlayerStrategy, JumpOnPlayerStrategy, etc.).

---

## 2. Your 5 Attacks (90% of behavior)

| Attack | Range / position | Anim trigger (suggested) | Notes |
|--------|-------------------|---------------------------|--------|
| **Slam** | Melee, AOE | `SLAM` | Standing AOE (ground pound). Use when player in melee and you want area damage. |
| **Right hook** | Front / front-right | `PUNCHRIGHT` | Already wired; left/right punch colliders in AnimationMethodsRockBoss. |
| **Left hook** | Front / front-left | `PUNCHLEFT` | Same. |
| **BACKSPIN** (stomp/kick back) | Behind boss | `BACKSPIN` | Punish players attacking from behind. Condition: `PlayerIsBehind()`. |
| **Rock throw** | Ranged | `THROWROCK` | Ranged only; `LaunchRock()` + EnemyArcProjectile. |

**Movement (remaining ~10%):** Use **JUMPSLAM** (close distance when player out of melee) and **Chase** (triggers **STARTWALKING** / **STOPWALKING**) so the boss can get in range. All animation trigger names are UPPERCASE.

---

## 3. Finalized Behavior Tree (high level)

- **Root** = `PrioritySelector` (higher priority = tried first).
  1. **PunishBack** (priority 6) – If `PlayerIsBehind()` and in short range → **BACKSPIN**. So the boss reacts to back attacks first.
  2. **Attacks** (priority 5) – When in range and facing player:
     - **Ranged:** Rock throw (**THROWROCK**).
     - **Melee:** Random among: **SLAM**, **PUNCHLEFT**, **PUNCHRIGHT**. All require melee range + facing.
  3. **Movement** (priority 1) – When out of melee:
     - **JUMPSLAM** – Leap at player (gap closer).
     - **Chase** – **STARTWALKING** / **STOPWALKING**, run toward player.

So: **Punish back → Attack (ranged or melee) → Move.**

---

## 4. Conditions (all implemented)

- **PlayerWithinRange(float)** – in `CharacterInteraction`.
- **PlayerOutOfRange(float)** – in `CharacterInteraction`.
- **FacingPlayer** – `rockBossHeadLook.IsLookingAtPlayer()`.
- **PlayerIsBehind()** – in `CharacterInteraction`: dot(forward, toPlayer) < -0.3f and uses `backStompRange` for stomp.

---

## 5. What You Add in Unity

- **Animator:** All trigger names UPPERCASE (e.g. `BACKSPIN`, `SLAM`). Animation events on BACKSPIN/SLAM clips:
  - SLAM: call `ActivateSlam` / `DeactivateSlam` (or re-use a single “slam” hitbox that’s active during the hit frame).
  - BACKSPIN: call `ActivateBackspin` / `DeactivateBackspin` on **AnimationMethodsRockBoss**.
- **Colliders:** Assign `backspinCollider` and `slamCollider` on the RockBoss prefab’s **AnimationMethodsRockBoss**; enable/disable from the animation events above.
- **Optional:** Phase logic using `EnemyHealth.GetHealthNormalized()` (e.g. enrage or different weights at low HP).

---

## 6. Summary

- **RockBoss folder** gives you: BT controller, head look/facing, animation-event damage (punches, rock launch), and arc projectile for rocks.
- **Finalized behavior:** PunishBack (BACKSPIN) first, then Attacks (THROWROCK or melee: SLAM, PUNCHLEFT, PUNCHRIGHT), then Movement (JUMPSLAM, Chase). All anim triggers are UPPERCASE.
