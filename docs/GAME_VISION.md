# Off-World: Game Vision

## Core Concept

**Off-World** is a survival-action game set on a procedurally generated alien planet. The player crash-lands and must survive by hunting the alien creatures that inhabit the world. The core mechanic revolves around **biological acquisition** -- gaining alien abilities by harvesting their anatomy.

---

## The Two Paths: Kill vs. Incapacitate

Every creature encounter presents a meaningful choice:

### Path 1: Kill -- Physical Anatomy Grafting
- **What happens:** Killing a creature allows the player to harvest its body parts (limbs, claws, wings, carapace, eyes, etc.).
- **Result:** The player physically grafts alien anatomy onto their body, visually transforming over time.
- **Tradeoff:** Powerful and immediate, but the player loses humanity. The grafted parts are permanent (or costly to remove). Mixing too many incompatible anatomies may have consequences.

### Path 2: Incapacitate -- DNA Suit Integration
- **What happens:** Subduing a creature without killing it allows the player to extract DNA samples.
- **Result:** DNA is applied to the player's suit, granting abilities without physical transformation. The suit adapts and evolves.
- **Tradeoff:** More versatile and reversible, but weaker than direct grafting. DNA abilities may require suit energy/charge.

---

## World

- Procedurally generated alien terrain with distinct biomes (sand/beach, grasslands, rocky cliffs, future: swamp, crystal caves, volcanic)
- Creatures spawn by biome/height using the existing ChunkManager spawn table system
- The world should feel hostile and alien -- the player is the outsider

---

## Existing Systems to Build On

| System | File(s) | How It Connects |
|--------|---------|-----------------|
| Player Movement | PlayerMovement.cs | Anatomy/DNA will modify movement stats (speed, jump, dodge) |
| Player Health | PlayerHealth.cs | New anatomy can modify max HP, add resistances |
| Combat | AttackController.cs, PlayerAnimMethods.cs | New attacks from grafted limbs or DNA abilities |
| Power Items | PowerItemI.cs, JumpPowerItem.cs, SpeedPowerItem.cs | Template for stat modifications -- anatomy/DNA system replaces/extends this |
| Enemy Health | EnemyHealth.cs | Needs "incapacitate" threshold alongside death |
| Enemy Grabbable | EnemyGrabbable.cs | Net capture exists -- extend for DNA extraction |
| Behavior Trees | Node.cs, Strategies.cs | Enemies already have behavior -- can add "wounded/downed" states |
| Spawn System | ChunkManager.cs | Creature variety per biome drives what parts/DNA are available |
| Terrain | ChunkGeneration.cs, TerrainGeneration.cs | Biomes determine creature populations |

---

## Design Pillars

1. **Every Kill Transforms You** -- The player's appearance and abilities change based on what they hunt.
2. **Choice Matters** -- Kill vs. incapacitate is a real decision with different rewards.
3. **The World Provides** -- Different biomes offer different creatures with different parts/DNA.
4. **Emergent Builds** -- Players can mix and match parts/DNA to create unique ability combinations.
5. **Alien Ecology** -- Creatures should feel like they belong to an ecosystem, not just be combat targets.
