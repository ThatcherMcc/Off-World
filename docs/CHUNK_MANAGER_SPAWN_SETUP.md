# ChunkManager: How to Set Up Creature Spawning

ChunkManager spawns creatures in a ring around the player, using **Spawn Tables** to pick **what** spawns **where** (by height and distance). No code needed — configure in the Inspector.

---

## 1. Make sure the manager can run

- **Player** and **Ship** are set automatically when you use procedural terrain (ChunkGeneration calls `SetPlayer` / `SetShip` after spawn).
- For a non-procedural scene, assign **Player Pos** and optionally **Ship Pos** in the Inspector.
- If **Player Pos** is null, ChunkManager does nothing (no per-frame lookup).

---

## 2. Add at least one Spawn Table

In the Inspector, under **Spawn Tables**, click **+** to add an element.

Each **Spawn Table** is a “biome” or “zone”: when a random spawn point is chosen, ChunkManager checks each table in order. The **first** table whose filters match the spawn position (height + distance from player) is used; one creature from that table’s entries is then chosen by weight.

### Table fields

| Field | What it does |
|-------|----------------|
| **Table Name** | Label only (e.g. "Lowland", "Hills"). |
| **Min Height** | Spawn only if ground Y ≥ this. Land is above water; use e.g. **20** for “on land”. |
| **Max Height** | Spawn only if ground Y ≤ this. Use **500** (or high) for no upper limit. |
| **Min Distance From Player** | Spawn only at least this far from player (default 0). |
| **Max Distance From Player** | Spawn only at most this far from player (default 9999 = no limit). |
| **Entries** | List of creature prefabs + weight + max count (see below). |

### Height hint (procedural terrain)

- Water level is about **0–15** (seed-dependent).
- Land is above that; player spawns around **waterLevel + 15** to **waterLevel + 40**.
- So for “on land” tables, use e.g. **Min Height = 20**, **Max Height = 500**.

---

## 3. Add Spawn Entries to the table

Inside a Spawn Table, under **Entries**, click **+** for each creature type.

| Field | What it does |
|-------|----------------|
| **Name** | Label (e.g. "Wolf", "Bug") — for you only. |
| **Prefab** | Drag a creature prefab (e.g. from `Assets/Prefab/Mobs/`: Wolf, JumpingAlien, Bug). |
| **Weight** | Relative chance in this table. E.g. Wolf **2**, Bug **1** → Wolf twice as likely as Bug. |
| **Max Count** | Max of this creature type alive at once (e.g. **10** Wolves). |

You can add several entries (e.g. Wolf, Bug, JumpingAlien) with different weights and max counts.

---

## 4. Tune global spawning (optional)

| Field | What it does |
|-------|----------------|
| **Global Mob Cap** | Max total creatures (all types). Default **40**. |
| **Spawn Check Interval** | Seconds between spawn/despawn passes. Default **1**. |
| **Spawn Attempts Per Tick** | Tries per check. Default **3**; increase to refill faster. |
| **Ground Layer Mask** | Layers used for ground raycast. Set to your **terrain layer** so spawns land on the mesh. |

---

## 5. Example: one table, two creatures

1. Add one **Spawn Table**.
2. Set **Table Name** = `"Land"`.
3. Set **Min Height** = **20**, **Max Height** = **500** (on land).
4. Add two **Entries**:
   - Name `"Wolf"`, Prefab = Wolf, Weight **2**, Max Count **8**.
   - Name `"Bug"`, Prefab = Bug, Weight **1**, Max Count **15**.
5. Leave distance filters at 0 and 9999.

Result: When a spawn point on land is chosen, ChunkManager picks Wolf 2/3 of the time and Bug 1/3, up to 8 Wolves and 15 Bugs total (and global cap 40).

---

## 6. Enemies and the player

If a spawned prefab has a component that implements **IEnemy** (e.g. WolfAI, FrogAI, EnemyAI), ChunkManager sets `enemy.player = playerPos` so the AI can track the player. No extra setup needed.

---

## 7. Ship no-spawn zone

Nothing spawns within **Ship No Spawn Radius** of the ship (default **30**). Ship transform is set by ChunkGeneration when the ship spawns, or you can assign **Ship Pos** manually.

---

## Quick checklist

- [ ] ChunkManager on scene (same GameObject as ChunkGeneration or referenced).
- [ ] Player (and optionally Ship) assigned — automatic with procedural terrain.
- [ ] At least one **Spawn Table** with **Min/Max Height** matching your terrain (e.g. 20–500 for land).
- [ ] At least one **Entry** per table: **Prefab** assigned, **Weight** > 0, **Max Count** > 0.
- [ ] **Ground Layer Mask** set to terrain layer so raycasts hit ground.
