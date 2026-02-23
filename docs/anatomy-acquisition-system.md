# Anatomy Acquisition System -- Design Document

**Game:** Off-World
**Version:** 1.0
**Status:** Design Specification

---

## Table of Contents

1. [System Overview](#1-system-overview)
2. [Body Part Slot System](#2-body-part-slot-system)
3. [Creature Harvest Table](#3-creature-harvest-table)
4. [Stat Modification Rules](#4-stat-modification-rules)
5. [Incapacitation Mechanic](#5-incapacitation-mechanic)
6. [DNA Suit Mechanics](#6-dna-suit-mechanics)
7. [Ability Catalog](#7-ability-catalog)
8. [Progression Design](#8-progression-design)
9. [C# Architecture Sketch](#9-c-architecture-sketch)

---

## 1. System Overview

The Anatomy Acquisition System is the central progression mechanic of Off-World. Every alien creature the player encounters is both a threat and a resource. The player faces a fundamental choice each time they defeat a creature:

- **KILL (Physical Grafting):** Destroy the creature, harvest its body parts, and physically graft alien anatomy onto your own body. Grafts are permanent stat mutations. They are powerful, always-on, and free to use -- but they change what you are. Mixing incompatible biology carries risk. You become less human with every graft.

- **INCAPACITATE (DNA Suit Integration):** Weaken the creature without killing it, extract a DNA sample, and load it into your survival suit. Suit abilities are flexible and swappable but cost energy to activate, and they never match the raw power of a physical graft. You stay human. You stay versatile.

Neither path is strictly better. Grafting gives permanent power at the cost of flexibility and humanity. DNA gives versatility at the cost of raw strength and energy management. The most interesting builds will mix both.

### Design Pillars

1. **Every creature matters.** No trash mobs. Each species offers a unique mechanical reward worth pursuing.
2. **Meaningful choice under pressure.** The kill-vs-subdue decision happens in combat, not in a menu. Missing the incapacitation window means you only get parts.
3. **Visual identity.** The player's appearance reflects their choices. A player who grafts Wolf legs and a Bug carapace looks fundamentally different from a suited-up DNA collector.
4. **Buildcraft depth.** Slot limitations and compatibility rules create real tradeoffs. You cannot graft everything onto yourself. You have to pick a direction.

---

## 2. Body Part Slot System

### 2.1 Graft Slots (Physical Path)

The player body has **6 graft slots**. Each slot accepts one alien body part at a time. Grafting a new part into an occupied slot **destroys** the previous graft permanently -- this is surgery, not swapping gear.

| Slot | Location | What It Governs | Default (Human) |
|------|----------|-----------------|-----------------|
| **HEAD** | Skull/face | Perception, awareness, special senses | Normal vision, no special senses |
| **TORSO** | Chest/core | Max HP, damage resistance, overall constitution | 100 HP, no resistance |
| **LEFT ARM** | Left upper limb | Off-hand attack, utility abilities | Normal left swipe (20 dmg) |
| **RIGHT ARM** | Right upper limb | Main-hand attack, primary offense | Normal right swipe (20 dmg) |
| **LEGS** | Both lower limbs (paired) | Movement speed, jump, dodge, locomotion | Walk 7, Sprint 14, Jump 13 |
| **BACK** | Spine/shoulder blades | Traversal abilities, passive defense, carried weight | No special traversal |

### 2.2 Slot Rules

**One part per slot.** No stacking. A Wolf Jaw in the HEAD slot means you cannot also have Bug Antennae in the HEAD slot.

**Grafting is permanent until overwritten.** You cannot remove a graft and go back to "human." You can only replace it with a different graft. This creates commitment.

**Species Affinity bonus.** If 3 or more of your 6 slots contain parts from the **same species**, you gain a Species Affinity bonus -- a powerful passive that represents your body adapting to that creature's biology. This is the reward for specializing instead of mixing and matching.

**Incompatibility penalty.** If you have parts from 4 or more **different** species simultaneously, you suffer a Biological Rejection debuff: -10% max HP, passive HP drain of 1 HP every 10 seconds. Your body is fighting itself. This discourages collecting one part from everything. It rewards focus.

**Boss parts are unique.** Boss creature parts occupy the same slot as regular parts but are flagged as "Apex" tier. They cannot trigger incompatibility penalties with other parts -- boss biology is dominant enough to coexist. They can still contribute to Species Affinity.

### 2.3 DNA Suit Slots

The DNA Suit has **4 active slots** and **2 passive slots** (covered in section 6). DNA is non-destructive to swap. You can overwrite a suit slot freely at any crafting station or rest point.

---

## 3. Creature Harvest Table

### 3.1 Overview

Every creature drops specific body parts on death and yields specific DNA on incapacitation. Not every creature fills every slot -- a Wolf has no interesting arms to harvest, but its legs and head are valuable.

### 3.2 Harvest Table

#### Wolf

| Drop Type | Slot | Item Name | Drop Rate |
|-----------|------|-----------|-----------|
| Graft Part | HEAD | Wolf Jaw | 100% |
| Graft Part | LEGS | Wolf Haunches | 100% |
| Graft Part | TORSO | Wolf Hide | 40% |
| DNA Sample | -- | Wolf DNA | 100% (on incapacitate) |

**Lore:** Pack predators. Fast, aggressive, relentless. Their biology is built for pursuit and tearing.

#### Frog

| Drop Type | Slot | Item Name | Drop Rate |
|-----------|------|-----------|-----------|
| Graft Part | LEGS | Frog Legs | 100% |
| Graft Part | TORSO | Frog Membrane | 60% |
| Graft Part | HEAD | Frog Tongue | 50% |
| DNA Sample | -- | Frog DNA | 100% (on incapacitate) |

**Lore:** Skittish prey creatures. Evolved for explosive jumps and escape. Their skin secretes a mild toxin.

#### Bug

| Drop Type | Slot | Item Name | Drop Rate |
|-----------|------|-----------|-----------|
| Graft Part | TORSO | Bug Carapace | 100% |
| Graft Part | HEAD | Bug Antennae | 100% |
| Graft Part | BACK | Bug Wing Casings | 40% |
| DNA Sample | -- | Bug DNA | 100% (on incapacitate) |

**Lore:** Armored insects. Hard exoskeleton, sensitive feelers, vestigial flight structures. Tough but slow.

#### JumpingAlien

| Drop Type | Slot | Item Name | Drop Rate |
|-----------|------|-----------|-----------|
| Graft Part | LEGS | Alien Spring Legs | 100% |
| Graft Part | LEFT ARM | Alien Grasper | 60% |
| Graft Part | BACK | Alien Dorsal Fin | 50% |
| DNA Sample | -- | JumpingAlien DNA | 100% (on incapacitate) |

**Lore:** Agile ambush predators. Enormous vertical leap, prehensile gripping limbs, stabilizing dorsal structure.

#### RockBoss (Apex)

| Drop Type | Slot | Item Name | Drop Rate |
|-----------|------|-----------|-----------|
| Graft Part | RIGHT ARM | Rockfist | 100% |
| Graft Part | LEFT ARM | Boulder Buckler | 100% |
| Graft Part | TORSO | Stone Heart | 100% |
| Graft Part | BACK | Geode Spine | 100% |
| DNA Sample | -- | RockBoss DNA | 100% (on incapacitate) |

**Lore:** An ancient mineral-organic hybrid. Its body is living stone. Every part it drops is Apex tier -- no incompatibility penalties.

### 3.3 Drop Mechanics

- Parts drop as physical world objects (new prefabs) at the creature's death position.
- Parts persist for 120 seconds before despawning. A pulsing glow VFX signals urgency.
- The player picks up parts using the existing `InteractController` system (E key). Parts go into a new **Inventory** panel, not directly onto the body.
- DNA is extracted via a special interaction on incapacitated creatures (section 5).

---

## 4. Stat Modification Rules

### 4.1 Base Player Stats (Reference)

These are the values currently set in `PlayerMovement` and `PlayerHealth`:

| Stat | Field | Base Value | Source |
|------|-------|------------|--------|
| Max HP | `PlayerHealth.maxHealth` | 100 | PlayerHealth.cs |
| Walk Speed | `PlayerMovement.walkSpeed` | Inspector-set | PlayerMovement.cs |
| Sprint Speed | `PlayerMovement.sprintSpeed` | Inspector-set | PlayerMovement.cs |
| Jump Force | `PlayerMovement.jumpForce` | Inspector-set | PlayerMovement.cs |
| Dodge Force | `PlayerMovement.DodgeForce` | Inspector-set | PlayerMovement.cs |
| Dodge Duration | `PlayerMovement.DodgeDuration` | Inspector-set | PlayerMovement.cs |
| Attack Damage | `PlayerHitBox` hardcoded | 20 | PlayerHurtBox.cs |
| Immunity Duration | `PlayerHealth.immunityDuration` | 1.0s | PlayerHealth.cs |

### 4.2 How Grafts Modify Stats

Grafts apply **flat modifiers** and **multipliers** to the base stats. The final stat calculation is:

```
FinalStat = (BaseStat + FlatSum) * MultiplierProduct
```

Where `FlatSum` is the sum of all flat bonuses from grafts and `MultiplierProduct` is the product of all multiplier bonuses.

Example: Wolf Haunches (LEGS) gives +0 flat speed, x1.4 speed multiplier. If base walkSpeed is 7:
```
FinalWalkSpeed = (7 + 0) * 1.4 = 9.8
```

Grafts never modify the Inspector-set base values directly. Instead, `AnatomyManager` computes the effective value each time a graft changes, and `PlayerMovement`/`PlayerHealth` read from the manager.

### 4.3 How DNA Suit Modifies Stats

DNA suit abilities are **active effects** -- they do not passively change stats (with exceptions for passive DNA slots). When a suit ability is activated, it applies a temporary buff. When it expires or is deactivated, stats return to their graft-modified baseline.

Passive DNA slots apply small, always-on bonuses:
```
FinalStat = (BaseStat + GraftFlat + DNAPassiveFlat) * GraftMult * DNAPassiveMult
```

### 4.4 Stat Modification Summary Table

| Modifier Source | Application | Duration | Stacks With |
|----------------|-------------|----------|-------------|
| Graft Part | Permanent while equipped | Until replaced | Other grafts, DNA passives |
| DNA Active Ability | Temporary on activation | Ability duration | Grafts, DNA passives |
| DNA Passive Slot | Permanent while slotted | Until swapped | Grafts, DNA actives |
| Species Affinity | Permanent while 3+ same-species parts equipped | While condition met | Everything |
| Incompatibility | Permanent while 4+ different species parts equipped | While condition met | Stacks negatively |

### 4.5 Damage Resistance

A new stat not currently in the codebase. Damage resistance is a percentage reduction applied before `PlayerTakeDMG`:

```
ActualDamage = IncomingDamage * (1 - DamageResistance)
```

Capped at 0.75 (75% reduction). Only certain torso and back grafts provide resistance.

---

## 5. Incapacitation Mechanic

### 5.1 Core Concept

Incapacitation is the act of weakening a creature to a vulnerable state without killing it. This is the only way to obtain DNA samples. It is deliberately harder than killing -- a reward for restraint and skill.

### 5.2 The Incapacitation Window

Every creature has an **Incapacitation Threshold** -- a percentage of max HP. When the creature's HP drops into the threshold range, it enters a short **Vulnerable Window**. If the player interacts with the creature during this window, they extract DNA. If the window expires, the creature either dies (HP too low) or recovers and becomes enraged.

| Creature | Max HP | Incapacitation Threshold | Vulnerable Window Duration |
|----------|--------|--------------------------|---------------------------|
| Wolf | 60 | 10-20% (6-12 HP) | 4 seconds |
| Frog | 30 | 15-30% (5-9 HP) | 6 seconds |
| Bug | 50 | 10-25% (5-13 HP) | 5 seconds |
| JumpingAlien | 70 | 10-20% (7-14 HP) | 3.5 seconds |
| RockBoss | 100 | 5-15% (5-15 HP) | 8 seconds |

### 5.3 How Incapacitation Works Step-by-Step

1. **Combat as normal.** The player attacks the creature using standard left/right swipes (20 damage per hit via `PlayerHitBox`).

2. **Threshold reached.** When HP drops into the incapacitation range, `EnemyHealth` fires an `OnIncapacitationReady` event. The creature enters a stunned state:
   - Its AI is disabled (`IEnemy.EnableAI(false)`), same as the existing net capture system.
   - It plays a stagger/collapse animation.
   - A distinct VFX (pulsing DNA helix particle) appears above it.
   - A UI prompt appears: "Press [E] to Extract DNA".

3. **Extraction.** The player walks up and presses Interact (E). This triggers a 2-second extraction animation (player kneels, suit extends a tendril). On completion:
   - The player receives the creature's DNA sample in their DNA Inventory.
   - The creature is released -- it staggers away and despawns after leaving the player's notice radius. It is not killed.
   - If the creature is a boss, it enters a special defeated-but-alive state (collapses, cutscene plays).

4. **Window expires.** If the player does not extract within the window:
   - Regular enemies: the creature dies as if HP reached 0. The player gets body part drops but NO DNA.
   - This means hesitation is punished -- you had your chance.

5. **Overkill.** If the player hits the creature while it is in the vulnerable state, it dies immediately. No DNA. Restraint matters.

### 5.4 Integration with Existing Net System

The existing `EnemyGrabbable` / `NetScript` / `InteractController.UseAction()` net capture system is repurposed:

- **Net + Weaken combo:** Using the net on a creature at ANY HP now applies a "Restrained" debuff. Restrained creatures take 50% less damage from attacks, making it easier to carefully chip them into the incapacitation threshold without overkilling. The net also extends the Vulnerable Window by 3 seconds.
- This gives the net real mechanical purpose beyond the capture demo currently in the codebase.

### 5.5 Visual/Audio Feedback

- **Threshold approaching (25% HP):** Creature starts limping/slowing. Audio: labored breathing SFX.
- **Threshold entered:** Creature collapses. Camera briefly shakes. A DNA-helix particle emits from the body. Audio: a resonant "bio-ping" sound.
- **Window expiring:** The DNA helix VFX flickers and fades. Audio: warning beep.
- **Successful extraction:** Suit glows briefly with the creature's color. DNA sample icon appears in HUD. Audio: satisfying tech-chirp.
- **Failed (overkill):** Bone-crunch SFX. No DNA helix. Creature drops parts normally.

---

## 6. DNA Suit Mechanics

### 6.1 Suit Overview

The player's survival suit is a piece of human technology brought from Earth. It has bio-integration ports that can process alien DNA and temporarily express alien traits. Unlike grafting, suit abilities:

- Are swappable at any rest point or crafting station.
- Cost **Suit Energy** to activate.
- Have cooldowns.
- Are generally weaker than equivalent grafts but far more flexible.

### 6.2 Suit Slots

| Slot Type | Count | Description |
|-----------|-------|-------------|
| **Active Slots** | 4 | Abilities the player manually activates. Each has a keybind (1, 2, 3, 4). |
| **Passive Slots** | 2 | Always-on minor stat bonuses. No activation needed, no energy cost. |

### 6.3 Suit Energy

**Maximum Energy:** 100 units. Displayed as a bar beneath the HP bar.

**Recharge:**
- Passive recharge: 5 energy/second while not using abilities.
- Recharge pauses for 3 seconds after any ability activation.
- Power Items (existing `PowerItemI` system) can be extended: eating a new "Energy Fruit" restores 50 energy instantly.

**Energy costs per ability:** Range from 10 (minor buffs) to 40 (powerful transformations). See Ability Catalog (section 7).

### 6.4 Slot Management

- DNA samples are stored in a **DNA Inventory** (unlimited capacity, persists across sessions).
- At a rest point or crafting station, the player opens the **Suit Configuration UI** and drags DNA from inventory into active/passive slots.
- Swapping is instant and free. No cost, no cooldown. Experimentation is encouraged.
- The same DNA sample can be used to fill multiple slots if the player has multiple copies. Each slot consumes one DNA sample when loaded. Removing DNA from a slot returns the sample to inventory.

### 6.5 DNA Tier System

DNA quality depends on the creature's remaining HP when incapacitated. Lower HP (closer to the bottom of the threshold) means the sample was taken under more stress, yielding weaker DNA.

| Extraction HP Range | DNA Tier | Ability Power |
|---------------------|----------|---------------|
| Upper 50% of threshold | **Prime** | 100% ability values |
| Lower 50% of threshold | **Degraded** | 70% ability values |

Example: Wolf threshold is 10-20% HP (6-12 HP out of 60).
- Extract at 10-12 HP: Prime Wolf DNA.
- Extract at 6-9 HP: Degraded Wolf DNA.

This rewards precision. Getting the creature into the threshold without going too deep gives better DNA.

### 6.6 Combining DNA (Late-Game)

At a late-game crafting station (unlocked after defeating RockBoss), the player can combine **3 DNA samples of the same species** into a **Hybrid DNA** sample. Hybrid DNA unlocks a more powerful version of the suit ability that is unavailable from a single sample.

---

## 7. Ability Catalog

### 7.1 Wolf

#### Graft Abilities (Physical Path)

| Part | Slot | Passive Effect | Active Ability |
|------|------|----------------|----------------|
| **Wolf Jaw** | HEAD | +30% attack damage on both swipes | **Lunge Bite:** When sprinting, the next attack deals 2x damage and lunges the player forward 3m. 8s internal cooldown. |
| **Wolf Haunches** | LEGS | +40% sprint speed, +20% walk speed | **Pursuit Mode:** While sprinting, gain 10% speed every 2 seconds (max +50%). Resets when you stop sprinting. No dodge height penalty. |
| **Wolf Hide** | TORSO | +15 Max HP, 10% damage resistance | No active. Pure defense. |

#### Wolf Species Affinity (3+ Wolf parts)
**Pack Hunter:** When within 20m of any enemy, gain +15% move speed and +10% attack damage. The Wolf parts feed off proximity to prey.

#### DNA Suit Abilities

| Slot Type | Ability Name | Energy Cost | Duration | Effect |
|-----------|-------------|-------------|----------|--------|
| Active | **Predator Dash** | 20 | Instant | Dash forward 8m at extreme speed, dealing 15 damage to all enemies you pass through. 6s cooldown. |
| Active | **Howl** | 15 | 6s | All enemies within 12m have their movement speed reduced by 30%. |
| Passive | **Keen Nose** | -- | Permanent | Enemy awareness indicators appear on HUD when enemies are within 25m. Directional arrows. |

---

### 7.2 Frog

#### Graft Abilities (Physical Path)

| Part | Slot | Passive Effect | Active Ability |
|------|------|----------------|----------------|
| **Frog Legs** | LEGS | +80% jump force, -10% walk speed | **Power Leap:** Hold jump key to charge. Release for a jump up to 3x normal height. Charge time: 1.5s max. No fall damage from Power Leap landings. |
| **Frog Membrane** | TORSO | +10 Max HP, take 50% reduced fall damage | **Toxin Sweat:** When hit, 30% chance to poison the attacker for 5 damage/second for 3 seconds. |
| **Frog Tongue** | HEAD | +20% interact range | **Tongue Lash:** Press interact at range to grab items/small enemies from 8m away (triple normal interact range). Works like a grapple for pickups only. |

#### Frog Species Affinity (3+ Frog parts)
**Amphibian Adaptation:** Take zero fall damage. Move 20% faster in water/rain. Jump cooldown reduced by 50%.

#### DNA Suit Abilities

| Slot Type | Ability Name | Energy Cost | Duration | Effect |
|-----------|-------------|-------------|----------|--------|
| Active | **Leap Boost** | 15 | 5s | Next jump has 2.5x force. Includes a soft-landing (no fall damage). |
| Active | **Toxic Cloud** | 25 | 8s | Drop a poison cloud at your position. 4m radius. Enemies inside take 8 damage/second. |
| Passive | **Sticky Grip** | -- | Permanent | Reduces slide on slopes. Can walk up slopes 15 degrees steeper than normal (modifies `maxSlopeAngle`). |

---

### 7.3 Bug

#### Graft Abilities (Physical Path)

| Part | Slot | Passive Effect | Active Ability |
|------|------|----------------|----------------|
| **Bug Carapace** | TORSO | +30 Max HP, 20% damage resistance, -15% move speed | **Harden:** Activate to gain 50% damage resistance for 4 seconds. Cannot attack during Harden. 12s cooldown. |
| **Bug Antennae** | HEAD | Enemies within 15m appear as blips on a minimap overlay | **Tremor Sense:** Activate to reveal ALL creatures within 30m for 6 seconds, even through terrain. 15s cooldown. |
| **Bug Wing Casings** | BACK | +10% dodge distance | **Glide:** After jumping, hold jump to glide. Reduces fall speed by 70%, maintains horizontal momentum. Drains stamina while active (future stamina system). Duration: up to 4 seconds per jump. |

#### Bug Species Affinity (3+ Bug parts)
**Hive Mind:** When you kill or incapacitate any Bug-type creature, heal 15 HP. Your Bug Carapace regenerates: +2 HP/second when not taking damage for 5 seconds.

#### DNA Suit Abilities

| Slot Type | Ability Name | Energy Cost | Duration | Effect |
|-----------|-------------|-------------|----------|--------|
| Active | **Chitin Shield** | 20 | 6s | Gain a temporary 25 HP shield that absorbs damage before your real HP. Does not regenerate. |
| Active | **Pheromone Decoy** | 30 | 10s | Drop a decoy at your position. Enemies within 15m are drawn to the decoy instead of you. Decoy has 30 HP. |
| Passive | **Exoskeleton** | -- | Permanent | +10 Max HP, +5% damage resistance. |

---

### 7.4 JumpingAlien

#### Graft Abilities (Physical Path)

| Part | Slot | Passive Effect | Active Ability |
|------|------|----------------|----------------|
| **Alien Spring Legs** | LEGS | +50% jump force, +15% move speed, +25% dodge force | **Double Jump:** Gain one additional jump in midair. The second jump has 80% of the first jump's force. |
| **Alien Grasper** | LEFT ARM | +10% attack damage (left swipe), +15% interact range | **Grapple Pull:** Left-click at range to pull small/medium enemies toward you (8m range). Large enemies: you are pulled toward them instead. 6s cooldown. |
| **Alien Dorsal Fin** | BACK | +10% air control (`airSpeedMultiplier`), reduces fall damage by 30% | **Air Dash:** While airborne, press dodge to dash horizontally 5m in your movement direction. Resets on landing. |

#### JumpingAlien Species Affinity (3+ JumpingAlien parts)
**Apex Predator:** After landing from a jump of sufficient height (3m+), deal 20 AOE damage in a 3m radius around your landing point. Your air speed multiplier doubles.

#### DNA Suit Abilities

| Slot Type | Ability Name | Energy Cost | Duration | Effect |
|-----------|-------------|-------------|----------|--------|
| Active | **Propulsion Jump** | 20 | Instant | Launches you upward with 3x jump force. Can be used in midair. 4s cooldown. |
| Active | **Gravity Tether** | 25 | 8s | Reduces your gravity by 60%. You float, jump higher, and fall slower. Good for exploration. |
| Passive | **Stabilizer** | -- | Permanent | +15% air control. Eliminates the first 10 points of fall damage. |

---

### 7.5 RockBoss (Apex Tier)

#### Graft Abilities (Physical Path)

All RockBoss parts are **Apex tier**: they do not trigger incompatibility penalties and provide the strongest effects in the game.

| Part | Slot | Passive Effect | Active Ability |
|------|------|----------------|----------------|
| **Rockfist** | RIGHT ARM | +50% right-swipe attack damage, attacks cause 0.3s stagger on enemies | **Seismic Punch:** Charge right-click for 1s. Release to deliver a punch that deals 60 damage and sends the target flying backward 5m. Creates a small shockwave (2m radius, 20 damage). 10s cooldown. |
| **Boulder Buckler** | LEFT ARM | 15% damage resistance (stacks with torso), block with left-click for 40% damage reduction while held | **Stone Wall:** Slam the ground to raise a temporary rock barrier (3m wide, 2m tall) that blocks projectiles and enemy movement for 6 seconds. 20s cooldown. |
| **Stone Heart** | TORSO | +50 Max HP, 25% damage resistance, -20% move speed | **Petrify:** Become stone for 3 seconds. Immune to all damage. Cannot move or attack. Heals 20 HP during petrification. 30s cooldown. |
| **Geode Spine** | BACK | +10% damage resistance, enemies that hit you in melee take 5 recoil damage | **Eruption:** Slam backward, dealing 40 damage in a 4m cone behind you. Mirrors the RockBoss's own BACKSPIN punish mechanic. 12s cooldown. |

#### RockBoss Species Affinity (3+ RockBoss parts)
**Living Mountain:** +20% damage resistance (total can exceed normal cap up to 85%). Your attacks cause the ground to tremor -- enemies within 5m of your melee hits are slowed by 20% for 2 seconds.

#### DNA Suit Abilities

| Slot Type | Ability Name | Energy Cost | Duration | Effect |
|-----------|-------------|-------------|----------|--------|
| Active | **Quake Slam** | 35 | Instant | Slam the ground. All enemies within 6m take 30 damage and are knocked down for 2 seconds. 15s cooldown. |
| Active | **Rock Armor** | 30 | 10s | Gain 30% damage resistance and immune to knockback. Move speed reduced by 10% during effect. |
| Active | **Boulder Toss** | 25 | Instant | Summon and throw a rock projectile. Arc trajectory matching `EnemyArcProjectile` physics. Deals 40 damage on impact, 15 AOE damage in 2m radius. 8s cooldown. |
| Passive | **Stone Skin** | -- | Permanent | +15 Max HP, +10% damage resistance. |

---

### 7.6 Hybrid DNA Abilities (Late-Game Combinations)

Created by combining 3 DNA of the same species at an advanced crafting station.

| Hybrid DNA | Ability Name | Energy Cost | Effect |
|-----------|-------------|-------------|--------|
| **Wolf Hybrid** | **Alpha Strike** | 35 | Dash through all enemies in a 12m line. Each enemy hit takes 25 damage and is slowed 50% for 3 seconds. You heal 5 HP per enemy hit. |
| **Frog Hybrid** | **Plague Leap** | 30 | Leap to a target location (15m range). On landing, release a toxic shockwave: 6m radius, 15 damage, poison for 10 damage over 5 seconds. |
| **Bug Hybrid** | **Swarm Shield** | 35 | Summon a swarm of alien insects around you for 12 seconds. The swarm deals 5 damage/second to nearby enemies (4m radius) and grants 15% damage resistance. |
| **JumpingAlien Hybrid** | **Orbital Drop** | 40 | Launch yourself 20m into the air, then slam down on a targeted location (10m targeting range). Deals 50 damage on impact, 25 damage in 5m radius. |
| **RockBoss Hybrid** | **Tectonic Wrath** | 40 | For 8 seconds, every attack creates a ground fissure that travels 6m forward, dealing 20 damage to all enemies in its path. |

---

## 8. Progression Design

### 8.1 Progression Phases

#### Early Game (First 15-30 minutes)

**Available creatures:** Wolf, Frog, Bug (common spawns on starting terrain chunks).

**Player state:** No grafts, no DNA, base stats. The player is weak and must learn combat fundamentals.

**Key moments:**
- First kill of any creature drops parts. The player discovers they can graft.
- First time encountering the incapacitation window (creature limping at low HP). Discovering they can extract DNA from wounded creatures rather than finishing them off.
- UI tutorial prompt on first graft: explains slot system, warns about permanence.
- UI tutorial prompt on first DNA extraction: explains suit slots and energy.

**Available abilities are modest:** Wolf gives speed. Frog gives jumping. Bug gives toughness. These address the player's core survival needs on an alien world without being overpowered.

**Progression tension:** The player has 6 graft slots but only 3 creature types. They can fill most slots but must decide which creature's LEGS they want: Wolf Haunches (speed), Frog Legs (jump), or wait. The first graft choice matters because replacement costs losing the previous part.

#### Mid Game (30-60 minutes)

**New creature:** JumpingAlien appears in more advanced terrain chunks (procedurally generated further from spawn).

**Player state:** 2-4 grafts equipped, a few DNA abilities in suit slots. The player has a "build" forming.

**Key moments:**
- JumpingAlien parts introduce mobility options that compete with Wolf/Frog legs. The player must decide their traversal identity.
- The Species Affinity system starts mattering. A player with Wolf Jaw + Wolf Haunches is one part away from the Pack Hunter bonus.
- The incompatibility penalty creates real tension. A player who grabbed Bug Carapace, Wolf Jaw, Frog Legs, and Alien Grasper (4 species) starts suffering rejection. Time to specialize or find a replacement.
- DNA collection ramps up. The player can now fill all 4 active suit slots and both passive slots, enabling interesting combinations of graft permanence with suit flexibility.

**Build archetypes begin emerging:**
- **Predator build:** Wolf-heavy grafts (Jaw + Haunches + Hide) for the Pack Hunter affinity. Fast, aggressive, high damage. Low defense.
- **Tank build:** Bug-heavy grafts (Carapace + Antennae + Wing Casings) for the Hive Mind affinity. Slow, tough, great awareness. Low damage.
- **Acrobat build:** Frog/JumpingAlien hybrid grafts. Massive vertical mobility, air control, exploration-focused.

#### Late Game (60+ minutes, RockBoss encounter)

**Boss creature:** RockBoss is the game's current boss with full behavior tree AI.

**Player state:** 4-6 grafts, multiple DNA samples, a refined build.

**Key moments:**
- RockBoss is a genuine skill check. Its behavior tree (PunishBack, Attacks, Movement) demands the player use their build intelligently.
- **Kill vs. Incapacitate on a boss** is the hardest and most meaningful version of the choice. The RockBoss incapacitation threshold (5-15% HP, or 5-15 HP) is extremely narrow. One extra hit kills it.
- Killing the RockBoss gives Apex-tier graft parts that are the strongest in the game.
- Incapacitating the RockBoss gives access to the most powerful DNA abilities AND unlocks the Hybrid DNA crafting station.
- The most dedicated players will fight the RockBoss multiple times -- once to kill for parts, once to incapacitate for DNA.

#### Endgame / New Game+

- The Hybrid DNA crafting station enables combining 3 DNA of the same species into Hybrid DNA with abilities unavailable otherwise.
- Players can re-fight respawned creatures to farm specific DNA tiers (Prime vs. Degraded).
- The goal is a "perfect build" -- optimized graft loadout with Species Affinity active, all suit slots filled with Prime DNA, and Hybrid abilities unlocked.

### 8.2 Power Curve

```
 Power
   ^
   |                                          /--- Apex grafts + Hybrid DNA
   |                                        /
   |                                   ___/
   |                              ___/      ^-- RockBoss defeated
   |                         ___/
   |                    ___/
   |               ___/    ^-- Species Affinity unlocked
   |          ___/
   |     ___/       ^-- First DNA suit abilities
   |  __/
   | /  ^-- First graft
   |/
   +-----------------------------------------> Time
     Early          Mid            Late
```

### 8.3 Difficulty Scaling

- Early creatures (Wolf, Frog, Bug) have wide incapacitation thresholds and long vulnerable windows. Forgiving for learning.
- JumpingAlien has a tighter threshold and shorter window. Requires precision.
- RockBoss has the tightest threshold in the game (5-15%) but the longest window (8s) because the boss encounter itself is the challenge. Getting it to threshold without overkill is the hard part.

### 8.4 Compatibility with Existing Power Items

The existing `JumpPowerItem` (2x jump) and `SpeedPowerItem` (2x speed) work as temporary, consumable boosts that stack multiplicatively with grafts and DNA:

```
FinalStat = (BaseStat + GraftFlat + DNAPassiveFlat) * GraftMult * DNAPassiveMult * PowerItemMult
```

Power Items become more valuable mid/late game because they multiply an already-modified base. A player with Wolf Haunches (+40% sprint) who eats a SpeedPowerItem gets: `sprintSpeed * 1.4 * 2.0 = 2.8x sprint speed`. This is intentional -- finding a Power Item when you already have grafts feels amazing.

---

## 9. C# Architecture Sketch

### 9.1 System Diagram

```
                        AnatomyManager (MonoBehaviour, on Player)
                       /                    \
                      /                      \
          GraftSystem                    DNASuitSystem
         /     |     \                  /      |       \
   BodyPartSlot[]  GraftPart     SuitSlot[]  DNASample  SuitEnergy
   (6 slots)       (ScriptableObject)  (6 slots)  (ScriptableObject)
        |                                   |
        v                                   v
   StatModifierStack  <------>  PlayerStatProvider
        |                           |
        v                           v
   PlayerMovement              PlayerHealth
   (reads effective stats)     (reads effective stats)
```

### 9.2 Core Interfaces and Classes

#### IStatModifier.cs
```csharp
/// Anything that modifies player stats implements this.
public interface IStatModifier
{
    StatModifierData GetModifiers();
    int Priority { get; } // Order of application. Grafts = 0, DNA Passive = 1, DNA Active = 2, PowerItem = 3.
}
```

#### StatModifierData.cs
```csharp
[System.Serializable]
public struct StatModifierData
{
    // Flat additions (applied before multipliers)
    public int   maxHPFlat;
    public float walkSpeedFlat;
    public float sprintSpeedFlat;
    public float jumpForceFlat;
    public float dodgeForceFlat;
    public float attackDamageFlat;

    // Multipliers (applied after flats; multiply together if multiple sources)
    public float maxHPMult;          // default 1.0
    public float walkSpeedMult;      // default 1.0
    public float sprintSpeedMult;    // default 1.0
    public float jumpForceMult;      // default 1.0
    public float dodgeForceMult;     // default 1.0
    public float attackDamageMult;   // default 1.0
    public float damageResistance;   // 0.0 to 0.75, additive across sources then capped
    public float airControlMult;     // default 1.0

    /// Returns a StatModifierData with all multipliers at 1.0 and flats at 0.
    public static StatModifierData Identity => new StatModifierData
    {
        maxHPMult = 1f, walkSpeedMult = 1f, sprintSpeedMult = 1f,
        jumpForceMult = 1f, dodgeForceMult = 1f, attackDamageMult = 1f,
        damageResistance = 0f, airControlMult = 1f
    };
}
```

#### GraftPartSO.cs (ScriptableObject)
```csharp
using UnityEngine;

public enum BodySlot { Head, Torso, LeftArm, RightArm, Legs, Back }
public enum CreatureSpecies { Wolf, Frog, Bug, JumpingAlien, RockBoss }
public enum PartTier { Standard, Apex }

[CreateAssetMenu(fileName = "NewGraftPart", menuName = "Off-World/Graft Part")]
public class GraftPartSO : ScriptableObject, IStatModifier
{
    [Header("Identity")]
    public string partName;
    public string description;
    public BodySlot slot;
    public CreatureSpecies species;
    public PartTier tier;
    public Sprite icon;
    public GameObject visualPrefab; // Mesh swap or additive mesh for the player model

    [Header("Stat Modifiers")]
    public StatModifierData modifiers = StatModifierData.Identity;

    [Header("Active Ability (optional)")]
    public GraftAbilitySO activeAbility; // null if this part has no active

    public int Priority => 0; // Grafts apply first

    public StatModifierData GetModifiers() => modifiers;
}
```

#### GraftAbilitySO.cs (ScriptableObject)
```csharp
using UnityEngine;

[CreateAssetMenu(fileName = "NewGraftAbility", menuName = "Off-World/Graft Ability")]
public class GraftAbilitySO : ScriptableObject
{
    public string abilityName;
    [TextArea] public string description;
    public float cooldown;
    public Sprite icon;

    // The actual ability logic is handled by a MonoBehaviour component
    // that is added/removed when the graft is equipped/unequipped.
    // This SO just holds data. The component type is determined by a string
    // that maps to a concrete class via reflection or a registry.
    public string abilityComponentType; // e.g. "LungeBiteAbility", "PowerLeapAbility"
}
```

#### DNASampleSO.cs (ScriptableObject)
```csharp
using UnityEngine;

public enum DNATier { Prime, Degraded }

[CreateAssetMenu(fileName = "NewDNASample", menuName = "Off-World/DNA Sample")]
public class DNASampleSO : ScriptableObject
{
    [Header("Identity")]
    public string sampleName;
    public CreatureSpecies species;
    public DNATier tier;
    public Sprite icon;

    [Header("Active Ability")]
    public string activeAbilityComponentType; // Component class name
    public float energyCost;
    public float cooldown;
    public float duration; // 0 = instant

    [Header("Passive Bonus (only applies in passive suit slots)")]
    public StatModifierData passiveModifiers = StatModifierData.Identity;

    [Header("Tier Scaling")]
    [Range(0f, 1f)] public float degradedPowerMultiplier = 0.7f;

    /// Returns effective modifiers accounting for tier.
    public StatModifierData GetEffectivePassiveModifiers()
    {
        if (tier == DNATier.Prime) return passiveModifiers;

        var m = passiveModifiers;
        m.maxHPFlat = Mathf.RoundToInt(m.maxHPFlat * degradedPowerMultiplier);
        m.walkSpeedFlat *= degradedPowerMultiplier;
        m.sprintSpeedFlat *= degradedPowerMultiplier;
        m.jumpForceFlat *= degradedPowerMultiplier;
        m.damageResistance *= degradedPowerMultiplier;
        return m;
    }
}
```

#### BodyPartSlot.cs
```csharp
[System.Serializable]
public class BodyPartSlot
{
    public BodySlot slotType;
    public GraftPartSO equippedPart; // null = empty (human default)

    /// Equip a new part. Returns the old part (null if slot was empty).
    /// The old part is DESTROYED (not returned to inventory).
    public GraftPartSO Equip(GraftPartSO newPart)
    {
        if (newPart.slot != slotType)
        {
            Debug.LogError($"Cannot equip {newPart.partName} in {slotType} slot.");
            return null;
        }
        var old = equippedPart;
        equippedPart = newPart;
        return old; // Caller can log/discard this; it is destroyed narratively
    }

    public void Clear() => equippedPart = null;
    public bool IsEmpty => equippedPart == null;
}
```

#### SuitSlot.cs
```csharp
public enum SuitSlotType { Active, Passive }

[System.Serializable]
public class SuitSlot
{
    public SuitSlotType slotType;
    public int slotIndex; // 0-3 for Active, 0-1 for Passive
    public DNASampleSO loadedDNA; // null = empty

    /// Load DNA into this slot. Returns the old DNA (returned to inventory).
    public DNASampleSO Load(DNASampleSO newDNA)
    {
        var old = loadedDNA;
        loadedDNA = newDNA;
        return old; // Returned to inventory, not destroyed
    }

    public void Unload() => loadedDNA = null;
    public bool IsEmpty => loadedDNA == null;
}
```

#### SuitEnergy.cs (MonoBehaviour)
```csharp
using UnityEngine;

public class SuitEnergy : MonoBehaviour
{
    [Header("Energy")]
    public float maxEnergy = 100f;
    public float currentEnergy;
    public float passiveRechargeRate = 5f; // per second
    public float rechargePauseAfterUse = 3f;

    private float rechargePauseTimer;

    // UI reference (similar pattern to Healthbar)
    [SerializeField] private Healthbar energyBar; // Reuse the Slider-based Healthbar component

    private void Start()
    {
        currentEnergy = maxEnergy;
    }

    private void Update()
    {
        if (rechargePauseTimer > 0)
        {
            rechargePauseTimer -= Time.deltaTime;
        }
        else
        {
            currentEnergy = Mathf.Min(currentEnergy + passiveRechargeRate * Time.deltaTime, maxEnergy);
        }
        energyBar?.SetHealth(Mathf.RoundToInt(currentEnergy));
    }

    /// Attempt to spend energy. Returns true if successful.
    public bool TrySpend(float amount)
    {
        if (currentEnergy < amount) return false;
        currentEnergy -= amount;
        rechargePauseTimer = rechargePauseAfterUse;
        return true;
    }

    public void Restore(float amount)
    {
        currentEnergy = Mathf.Min(currentEnergy + amount, maxEnergy);
    }
}
```

#### GraftSystem.cs (MonoBehaviour)
```csharp
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GraftSystem : MonoBehaviour
{
    [Header("Body Slots")]
    [SerializeField] private BodyPartSlot[] slots = new BodyPartSlot[6];

    // Events
    public event Action OnGraftsChanged; // Fired when any slot changes
    public event Action<CreatureSpecies> OnAffinityActivated;
    public event Action OnIncompatibilityTriggered;

    private void Awake()
    {
        // Initialize slots
        slots[0] = new BodyPartSlot { slotType = BodySlot.Head };
        slots[1] = new BodyPartSlot { slotType = BodySlot.Torso };
        slots[2] = new BodyPartSlot { slotType = BodySlot.LeftArm };
        slots[3] = new BodyPartSlot { slotType = BodySlot.RightArm };
        slots[4] = new BodyPartSlot { slotType = BodySlot.Legs };
        slots[5] = new BodyPartSlot { slotType = BodySlot.Back };
    }

    /// Graft a part into the appropriate slot.
    public void GraftPart(GraftPartSO part)
    {
        var slot = GetSlot(part.slot);
        if (slot == null) return;

        var old = slot.Equip(part);
        if (old != null)
        {
            // Remove old ability component if it had one
            RemoveAbilityComponent(old);
        }

        // Add new ability component if it has one
        if (part.activeAbility != null)
        {
            AddAbilityComponent(part);
        }

        OnGraftsChanged?.Invoke();
        CheckAffinity();
        CheckIncompatibility();
    }

    public BodyPartSlot GetSlot(BodySlot slotType) => slots.FirstOrDefault(s => s.slotType == slotType);

    /// Collect all stat modifiers from equipped grafts.
    public List<StatModifierData> GetAllModifiers()
    {
        var mods = new List<StatModifierData>();
        foreach (var slot in slots)
        {
            if (!slot.IsEmpty)
            {
                mods.Add(slot.equippedPart.GetModifiers());
            }
        }
        return mods;
    }

    /// Returns the active affinity species, or null if none.
    public CreatureSpecies? GetActiveAffinity()
    {
        var speciesCounts = new Dictionary<CreatureSpecies, int>();
        foreach (var slot in slots)
        {
            if (slot.IsEmpty) continue;
            var sp = slot.equippedPart.species;
            speciesCounts.TryGetValue(sp, out int count);
            speciesCounts[sp] = count + 1;
        }
        foreach (var kvp in speciesCounts)
        {
            if (kvp.Value >= 3) return kvp.Key;
        }
        return null;
    }

    /// Returns true if 4+ distinct species are grafted (incompatibility).
    public bool HasIncompatibility()
    {
        var species = new HashSet<CreatureSpecies>();
        foreach (var slot in slots)
        {
            if (slot.IsEmpty) continue;
            if (slot.equippedPart.tier == PartTier.Apex) continue; // Apex parts exempt
            species.Add(slot.equippedPart.species);
        }
        return species.Count >= 4;
    }

    /// Returns count of distinct non-Apex species grafted.
    public int GetDistinctSpeciesCount()
    {
        var species = new HashSet<CreatureSpecies>();
        foreach (var slot in slots)
        {
            if (slot.IsEmpty) continue;
            if (slot.equippedPart.tier == PartTier.Apex) continue;
            species.Add(slot.equippedPart.species);
        }
        return species.Count;
    }

    private void CheckAffinity()
    {
        var affinity = GetActiveAffinity();
        if (affinity.HasValue) OnAffinityActivated?.Invoke(affinity.Value);
    }

    private void CheckIncompatibility()
    {
        if (HasIncompatibility()) OnIncompatibilityTriggered?.Invoke();
    }

    private void AddAbilityComponent(GraftPartSO part)
    {
        // Use a component registry or reflection to add the correct MonoBehaviour
        var typeName = part.activeAbility.abilityComponentType;
        var type = System.Type.GetType(typeName);
        if (type != null)
        {
            gameObject.AddComponent(type);
        }
    }

    private void RemoveAbilityComponent(GraftPartSO part)
    {
        if (part.activeAbility == null) return;
        var typeName = part.activeAbility.abilityComponentType;
        var type = System.Type.GetType(typeName);
        if (type == null) return;
        var comp = gameObject.GetComponent(type);
        if (comp != null) Destroy(comp);
    }
}
```

#### DNASuitSystem.cs (MonoBehaviour)
```csharp
using System;
using System.Collections.Generic;
using UnityEngine;

public class DNASuitSystem : MonoBehaviour
{
    [Header("Suit Slots")]
    [SerializeField] private SuitSlot[] activeSlots = new SuitSlot[4];
    [SerializeField] private SuitSlot[] passiveSlots = new SuitSlot[2];

    [Header("References")]
    [SerializeField] private SuitEnergy suitEnergy;

    // Keybinds for active abilities
    private KeyCode[] activeKeys = { KeyCode.Alpha1, KeyCode.Alpha2, KeyCode.Alpha3, KeyCode.Alpha4 };
    private float[] cooldownTimers = new float[4];

    // Events
    public event Action OnSuitChanged;

    private void Awake()
    {
        for (int i = 0; i < 4; i++)
            activeSlots[i] = new SuitSlot { slotType = SuitSlotType.Active, slotIndex = i };
        for (int i = 0; i < 2; i++)
            passiveSlots[i] = new SuitSlot { slotType = SuitSlotType.Passive, slotIndex = i };
    }

    private void Update()
    {
        // Tick cooldowns
        for (int i = 0; i < 4; i++)
        {
            if (cooldownTimers[i] > 0)
                cooldownTimers[i] -= Time.deltaTime;
        }

        // Check for active ability input
        for (int i = 0; i < 4; i++)
        {
            if (Input.GetKeyDown(activeKeys[i]))
            {
                TryActivateSlot(i);
            }
        }
    }

    private void TryActivateSlot(int index)
    {
        var slot = activeSlots[index];
        if (slot.IsEmpty) return;
        if (cooldownTimers[index] > 0) return;

        var dna = slot.loadedDNA;
        float cost = dna.energyCost;
        if (dna.tier == DNATier.Degraded) cost *= 1.2f; // Degraded DNA costs more energy

        if (!suitEnergy.TrySpend(cost)) return; // Not enough energy

        // Activate the ability (add temporary component or call static method)
        ActivateAbility(dna);
        cooldownTimers[index] = dna.cooldown;
    }

    private void ActivateAbility(DNASampleSO dna)
    {
        var typeName = dna.activeAbilityComponentType;
        var type = System.Type.GetType(typeName);
        if (type == null) return;

        // Add a temporary ability component that self-destructs after duration
        var comp = gameObject.AddComponent(type) as ISuitAbility;
        comp?.Activate(dna.tier == DNATier.Degraded ? dna.degradedPowerMultiplier : 1f, dna.duration);
    }

    /// Load DNA into an active slot. Returns displaced DNA (returned to inventory).
    public DNASampleSO LoadActive(int slotIndex, DNASampleSO dna)
    {
        var old = activeSlots[slotIndex].Load(dna);
        OnSuitChanged?.Invoke();
        return old;
    }

    /// Load DNA into a passive slot.
    public DNASampleSO LoadPassive(int slotIndex, DNASampleSO dna)
    {
        var old = passiveSlots[slotIndex].Load(dna);
        OnSuitChanged?.Invoke();
        return old;
    }

    /// Collect all passive stat modifiers from passive slots.
    public List<StatModifierData> GetPassiveModifiers()
    {
        var mods = new List<StatModifierData>();
        foreach (var slot in passiveSlots)
        {
            if (!slot.IsEmpty)
            {
                mods.Add(slot.loadedDNA.GetEffectivePassiveModifiers());
            }
        }
        return mods;
    }
}
```

#### ISuitAbility.cs
```csharp
/// Interface for DNA suit ability components.
/// These are MonoBehaviours added at runtime when an ability is activated.
public interface ISuitAbility
{
    /// Called when the ability is activated.
    /// powerMultiplier: 1.0 for Prime, 0.7 for Degraded.
    /// duration: how long the effect lasts. 0 = instant (component should self-destroy after execution).
    void Activate(float powerMultiplier, float duration);
}
```

#### AnatomyManager.cs (MonoBehaviour -- Central Hub)
```csharp
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Central manager on the Player. Coordinates GraftSystem, DNASuitSystem,
/// and feeds final computed stats to PlayerMovement and PlayerHealth.
/// </summary>
public class AnatomyManager : MonoBehaviour
{
    [Header("Subsystems")]
    public GraftSystem grafts;
    public DNASuitSystem suit;

    [Header("Player References")]
    public PlayerMovement playerMovement;
    public PlayerHealth playerHealth;

    [Header("Base Stats (set these to match Inspector values on PlayerMovement)")]
    public float baseWalkSpeed;
    public float baseSprintSpeed;
    public float baseJumpForce;
    public float baseDodgeForce;
    public int baseMaxHP = 100;
    public float baseAttackDamage = 20f;
    public float baseAirSpeedMultiplier;

    // Computed effective stats (read by other systems)
    [HideInInspector] public float effectiveWalkSpeed;
    [HideInInspector] public float effectiveSprintSpeed;
    [HideInInspector] public float effectiveJumpForce;
    [HideInInspector] public float effectiveDodgeForce;
    [HideInInspector] public int effectiveMaxHP;
    [HideInInspector] public float effectiveAttackDamage;
    [HideInInspector] public float effectiveDamageResistance;
    [HideInInspector] public float effectiveAirSpeedMultiplier;

    // Incompatibility
    private bool incompatibilityActive;
    private float incompatibilityDrainTimer;

    private void Start()
    {
        grafts.OnGraftsChanged += RecalculateStats;
        suit.OnSuitChanged += RecalculateStats;
        grafts.OnIncompatibilityTriggered += () => incompatibilityActive = true;
        RecalculateStats();
    }

    private void Update()
    {
        // Incompatibility HP drain
        if (incompatibilityActive && grafts.HasIncompatibility())
        {
            incompatibilityDrainTimer += Time.deltaTime;
            if (incompatibilityDrainTimer >= 10f)
            {
                playerHealth.PlayerTakeDMG(1);
                incompatibilityDrainTimer = 0f;
            }
        }
        else
        {
            incompatibilityActive = false;
            incompatibilityDrainTimer = 0f;
        }
    }

    /// <summary>
    /// Recomputes all effective stats from base + grafts + DNA passives.
    /// Called whenever any graft or suit slot changes.
    /// </summary>
    public void RecalculateStats()
    {
        // Gather all modifiers
        var allMods = new List<StatModifierData>();
        allMods.AddRange(grafts.GetAllModifiers());
        allMods.AddRange(suit.GetPassiveModifiers());

        // Compute flats
        float walkFlat = allMods.Sum(m => m.walkSpeedFlat);
        float sprintFlat = allMods.Sum(m => m.sprintSpeedFlat);
        float jumpFlat = allMods.Sum(m => m.jumpForceFlat);
        float dodgeFlat = allMods.Sum(m => m.dodgeForceFlat);
        int hpFlat = allMods.Sum(m => m.maxHPFlat);
        float atkFlat = allMods.Sum(m => m.attackDamageFlat);
        float airFlat = 0f; // No flat air control

        // Compute multipliers (product of all)
        float walkMult = allMods.Aggregate(1f, (acc, m) => acc * m.walkSpeedMult);
        float sprintMult = allMods.Aggregate(1f, (acc, m) => acc * m.sprintSpeedMult);
        float jumpMult = allMods.Aggregate(1f, (acc, m) => acc * m.jumpForceMult);
        float dodgeMult = allMods.Aggregate(1f, (acc, m) => acc * m.dodgeForceMult);
        float hpMult = allMods.Aggregate(1f, (acc, m) => acc * m.maxHPMult);
        float atkMult = allMods.Aggregate(1f, (acc, m) => acc * m.attackDamageMult);
        float airMult = allMods.Aggregate(1f, (acc, m) => acc * m.airControlMult);

        // Compute damage resistance (additive, capped)
        float resistance = allMods.Sum(m => m.damageResistance);
        float resistanceCap = 0.75f;

        // Species Affinity bonus
        var affinity = grafts.GetActiveAffinity();
        if (affinity.HasValue)
        {
            var affinityMod = GetAffinityModifier(affinity.Value);
            walkMult *= affinityMod.walkSpeedMult;
            sprintMult *= affinityMod.sprintSpeedMult;
            jumpMult *= affinityMod.jumpForceMult;
            atkMult *= affinityMod.attackDamageMult;
            resistance += affinityMod.damageResistance;
        }

        // Incompatibility penalty
        if (grafts.HasIncompatibility())
        {
            hpMult *= 0.9f; // -10% max HP
        }

        // RockBoss affinity raises cap
        if (affinity.HasValue && affinity.Value == CreatureSpecies.RockBoss)
        {
            resistanceCap = 0.85f;
        }

        resistance = Mathf.Clamp(resistance, 0f, resistanceCap);

        // Apply final values
        effectiveWalkSpeed = (baseWalkSpeed + walkFlat) * walkMult;
        effectiveSprintSpeed = (baseSprintSpeed + sprintFlat) * sprintMult;
        effectiveJumpForce = (baseJumpForce + jumpFlat) * jumpMult;
        effectiveDodgeForce = (baseDodgeForce + dodgeFlat) * dodgeMult;
        effectiveMaxHP = Mathf.RoundToInt((baseMaxHP + hpFlat) * hpMult);
        effectiveAttackDamage = (baseAttackDamage + atkFlat) * atkMult;
        effectiveDamageResistance = resistance;
        effectiveAirSpeedMultiplier = (baseAirSpeedMultiplier + airFlat) * airMult;

        // Push stats to existing systems
        playerMovement.walkSpeed = effectiveWalkSpeed;
        playerMovement.sprintSpeed = effectiveSprintSpeed;
        playerMovement.jumpForce = effectiveJumpForce;
        playerMovement.DodgeForce = effectiveDodgeForce;
        playerMovement.airSpeedMultiplier = effectiveAirSpeedMultiplier;
        playerHealth.maxHealth = effectiveMaxHP;
    }

    /// Returns the Species Affinity bonus for a given species.
    private StatModifierData GetAffinityModifier(CreatureSpecies species)
    {
        var m = StatModifierData.Identity;
        switch (species)
        {
            case CreatureSpecies.Wolf:
                // Pack Hunter: +15% speed, +10% attack when near enemies (simplified: always-on for stat calc)
                m.walkSpeedMult = 1.15f;
                m.sprintSpeedMult = 1.15f;
                m.attackDamageMult = 1.10f;
                break;
            case CreatureSpecies.Frog:
                // Amphibian Adaptation: zero fall damage (handled elsewhere), +20% speed in water
                m.jumpForceMult = 1.15f; // General jump boost as proxy
                break;
            case CreatureSpecies.Bug:
                // Hive Mind: regen (handled elsewhere), stat proxy
                m.maxHPMult = 1.10f;
                m.damageResistance = 0.05f;
                break;
            case CreatureSpecies.JumpingAlien:
                // Apex Predator: landing damage (handled elsewhere), double air speed
                m.airControlMult = 2.0f;
                m.jumpForceMult = 1.10f;
                break;
            case CreatureSpecies.RockBoss:
                // Living Mountain: +20% resistance, tremor slow (handled elsewhere)
                m.damageResistance = 0.20f;
                m.attackDamageMult = 1.10f;
                break;
        }
        return m;
    }
}
```

#### CreatureLootTable.cs (ScriptableObject)
```csharp
using UnityEngine;

[CreateAssetMenu(fileName = "NewCreatureLoot", menuName = "Off-World/Creature Loot Table")]
public class CreatureLootTable : ScriptableObject
{
    public CreatureSpecies species;

    [System.Serializable]
    public struct LootEntry
    {
        public GraftPartSO part;
        [Range(0f, 1f)] public float dropChance;
    }

    [Header("Kill Drops")]
    public LootEntry[] killDrops;

    [Header("DNA (on incapacitate)")]
    public DNASampleSO primeDNA;
    public DNASampleSO degradedDNA;

    [Header("Incapacitation Settings")]
    [Range(0f, 1f)] public float incapThresholdLow;  // e.g. 0.10
    [Range(0f, 1f)] public float incapThresholdHigh; // e.g. 0.20
    public float vulnerableWindowDuration;            // seconds

    /// Roll drops. Returns the parts that dropped.
    public GraftPartSO[] RollDrops()
    {
        var drops = new System.Collections.Generic.List<GraftPartSO>();
        foreach (var entry in killDrops)
        {
            if (Random.value <= entry.dropChance)
            {
                drops.Add(entry.part);
            }
        }
        return drops.ToArray();
    }

    /// Determine DNA tier based on normalized HP at extraction.
    public DNASampleSO GetDNA(float normalizedHP)
    {
        float midpoint = (incapThresholdLow + incapThresholdHigh) / 2f;
        return normalizedHP >= midpoint ? primeDNA : degradedDNA;
    }
}
```

#### IncapacitationController.cs (MonoBehaviour -- on enemy)
```csharp
using System;
using UnityEngine;

/// <summary>
/// Attach to any enemy alongside EnemyHealth. Monitors HP and triggers
/// the incapacitation state when HP enters the threshold window.
/// </summary>
[RequireComponent(typeof(EnemyHealth))]
public class IncapacitationController : MonoBehaviour
{
    [Header("Loot")]
    [SerializeField] private CreatureLootTable lootTable;

    [Header("Drop Spawn")]
    [SerializeField] private GameObject partDropPrefab; // Generic "loot orb" that holds a GraftPartSO reference
    [SerializeField] private float dropSpawnHeight = 1.5f;

    // State
    private EnemyHealth enemyHealth;
    private IEnemy enemyAI;
    private bool isVulnerable;
    private float vulnerableTimer;
    private bool wasExtracted;

    // Events
    public event Action OnBecameVulnerable;
    public event Action OnVulnerableExpired;
    public event Action<DNASampleSO> OnDNAExtracted;

    private void Awake()
    {
        enemyHealth = GetComponent<EnemyHealth>();
        enemyAI = GetComponent<IEnemy>();
    }

    private void Update()
    {
        if (isVulnerable)
        {
            vulnerableTimer -= Time.deltaTime;
            if (vulnerableTimer <= 0f && !wasExtracted)
            {
                // Window expired -- creature dies, drop parts only
                isVulnerable = false;
                OnVulnerableExpired?.Invoke();
                SpawnPartDrops();
                Destroy(gameObject);
            }
        }
        else if (!wasExtracted)
        {
            CheckThreshold();
        }
    }

    private void CheckThreshold()
    {
        float hp = enemyHealth.GetHealthNormalized();
        if (hp <= lootTable.incapThresholdHigh && hp >= lootTable.incapThresholdLow)
        {
            EnterVulnerableState();
        }
    }

    private void EnterVulnerableState()
    {
        isVulnerable = true;
        vulnerableTimer = lootTable.vulnerableWindowDuration;

        // Disable AI
        enemyAI?.EnableAI(false);

        // Visual/audio feedback (trigger animation, VFX, etc.)
        // TODO: Play stagger animation, spawn DNA helix particle

        OnBecameVulnerable?.Invoke();
    }

    /// Called by InteractController when the player presses E on a vulnerable creature.
    public void ExtractDNA()
    {
        if (!isVulnerable || wasExtracted) return;

        wasExtracted = true;
        isVulnerable = false;

        float hp = enemyHealth.GetHealthNormalized();
        DNASampleSO dna = lootTable.GetDNA(hp);

        OnDNAExtracted?.Invoke(dna);

        // Creature staggers away and despawns
        enemyAI?.EnableAI(true);
        // TODO: Set flee behavior, then Destroy after delay
        Destroy(gameObject, 8f);
    }

    /// Called when the creature is killed (HP <= 0). Drop parts.
    public void OnDeath()
    {
        if (wasExtracted) return; // Already handled
        SpawnPartDrops();
    }

    private void SpawnPartDrops()
    {
        var drops = lootTable.RollDrops();
        foreach (var part in drops)
        {
            Vector3 spawnPos = transform.position + Vector3.up * dropSpawnHeight;
            spawnPos += UnityEngine.Random.insideUnitSphere * 0.5f;
            spawnPos.y = transform.position.y + dropSpawnHeight;

            // Instantiate a drop prefab and assign the part data to it
            var dropObj = Instantiate(partDropPrefab, spawnPos, Quaternion.identity);
            var dropComp = dropObj.GetComponent<PartDropPickup>();
            if (dropComp != null)
            {
                dropComp.Initialize(part);
            }
        }
    }
}
```

#### PartDropPickup.cs (MonoBehaviour -- on dropped part prefab)
```csharp
using UnityEngine;

/// <summary>
/// A world-space pickup for a graft part. Player interacts with it to pick up.
/// Self-destructs after despawnTime seconds.
/// </summary>
public class PartDropPickup : MonoBehaviour, InteractableI
{
    [HideInInspector] public GraftPartSO partData;
    public float despawnTime = 120f;

    public void Initialize(GraftPartSO part)
    {
        partData = part;
        // TODO: Set mesh/material to match part visuals
        // TODO: Start pulsing glow VFX
        Destroy(gameObject, despawnTime);
    }

    public void Interact(InteractController controller)
    {
        // Add to player's part inventory
        var manager = controller.GetComponentInParent<AnatomyManager>();
        if (manager != null)
        {
            // For now, auto-graft (later: add to inventory UI)
            manager.grafts.GraftPart(partData);
        }
        Destroy(gameObject);
    }
}
```

### 9.3 Integration Points with Existing Code

#### Modifications to EnemyHealth.cs

The existing `EnemyHealth.Die()` method needs to notify `IncapacitationController` before destroying the gameObject:

```csharp
// In EnemyHealth.cs, modify Die():
private void Die()
{
    var incap = GetComponent<IncapacitationController>();
    if (incap != null)
    {
        incap.OnDeath(); // Spawn part drops before destroying
    }

    // Existing logic
    if (bossController != null) bossController.DeactivateHealthbar();
    Destroy(gameObject);
}
```

The existing `HurtEnemy()` method needs a guard to prevent overkilling a vulnerable creature through the incapacitation system. The `IncapacitationController` handles the threshold check independently via `GetHealthNormalized()`, so no change is needed there. However, the existing hardcoded `health <= 0` check in `HurtEnemy` naturally handles the overkill case: if the player keeps hitting a vulnerable creature, it dies and drops parts but not DNA.

#### Modifications to PlayerHealth.cs

The `PlayerTakeDMG` method needs to apply damage resistance from `AnatomyManager`:

```csharp
// In PlayerHealth.cs, modify PlayerTakeDMG():
public void PlayerTakeDMG(int damage)
{
    if (immune == false)
    {
        immune = true;

        // Apply damage resistance from anatomy system
        var anatomy = GetComponent<AnatomyManager>();
        if (anatomy != null)
        {
            damage = Mathf.RoundToInt(damage * (1f - anatomy.effectiveDamageResistance));
            damage = Mathf.Max(1, damage); // Always take at least 1 damage
        }

        health -= damage;
        healthbar.SetHealth(health);

        Invoke("EndImmunity", immunityDuration);

        if (health <= 0)
        {
            health = 0;
            Die();
        }
    }
}
```

#### Modifications to InteractController.cs

Add DNA extraction as an interact action:

```csharp
// In InteractController.cs, add to the Interact() method:
private void Interact()
{
    if (Physics.SphereCast(fpsCam.position, InteractRadius, fpsCam.forward,
        out RaycastHit raycastHit, InteractRange, InteractLayerMask))
    {
        // Existing interactable check
        if (raycastHit.transform.TryGetComponent(out InteractableI newInteractable))
        {
            newInteractable.Interact(this);
        }
    }

    // NEW: Check for vulnerable creatures (on enemyInteractLayerMask)
    if (Physics.SphereCast(fpsCam.position, InteractRadius, fpsCam.forward,
        out RaycastHit enemyHit, InteractRange, enemyInteractLayerMask))
    {
        if (enemyHit.transform.TryGetComponent(out IncapacitationController incap))
        {
            incap.ExtractDNA();
        }
    }
}
```

#### Modifications to PlayerMovement.cs

`PlayerMovement` already uses public fields (`walkSpeed`, `sprintSpeed`, `jumpForce`, `DodgeForce`, `airSpeedMultiplier`). The `AnatomyManager.RecalculateStats()` writes directly to these fields, so `PlayerMovement` requires no structural changes. The values are overwritten by `AnatomyManager` whenever grafts or suit configuration changes.

Important: The existing `PowerItemI` implementations (`JumpPowerItem`, `SpeedPowerItem`) directly multiply these fields. Since `AnatomyManager` recalculates from base stats + modifiers on every graft change, a Power Item's multiplication will persist until the next recalculation. To handle this correctly, Power Items should instead register as temporary `IStatModifier` sources with `AnatomyManager`. This is a future refactor -- for the initial implementation, Power Items can continue to work as-is with the caveat that swapping grafts after eating a Power Item will reset the Power Item's bonus.

### 9.4 File Structure

```
Assets/Scripts/
    AnatomySystems/
        Core/
            AnatomyManager.cs
            StatModifierData.cs
            IStatModifier.cs
        Grafting/
            GraftSystem.cs
            BodyPartSlot.cs
            GraftPartSO.cs
            GraftAbilitySO.cs
        DNA/
            DNASuitSystem.cs
            SuitSlot.cs
            DNASampleSO.cs
            SuitEnergy.cs
            ISuitAbility.cs
        Loot/
            CreatureLootTable.cs
            IncapacitationController.cs
            PartDropPickup.cs
        Abilities/
            Graft/
                LungeBiteAbility.cs
                PowerLeapAbility.cs
                HardenAbility.cs
                DoubleJumpAbility.cs
                GrapplePullAbility.cs
                SeismicPunchAbility.cs
                ... (one MonoBehaviour per graft active ability)
            Suit/
                PredatorDashAbility.cs
                LeapBoostAbility.cs
                ChitinShieldAbility.cs
                QuakeSlamAbility.cs
                ... (one MonoBehaviour per suit active ability)
    Enums/
        BodySlot.cs
        CreatureSpecies.cs
        PartTier.cs
        DNATier.cs

Assets/ScriptableObjects/
    GraftParts/
        Wolf/
            WolfJaw.asset
            WolfHaunches.asset
            WolfHide.asset
        Frog/
            FrogLegs.asset
            FrogMembrane.asset
            FrogTongue.asset
        Bug/
            BugCarapace.asset
            BugAntennae.asset
            BugWingCasings.asset
        JumpingAlien/
            AlienSpringLegs.asset
            AlienGrasper.asset
            AlienDorsalFin.asset
        RockBoss/
            Rockfist.asset
            BoulderBuckler.asset
            StoneHeart.asset
            GeodeSpine.asset
    DNASamples/
        WolfDNA_Prime.asset
        WolfDNA_Degraded.asset
        FrogDNA_Prime.asset
        FrogDNA_Degraded.asset
        ... (Prime and Degraded for each species)
    LootTables/
        WolfLootTable.asset
        FrogLootTable.asset
        BugLootTable.asset
        JumpingAlienLootTable.asset
        RockBossLootTable.asset
    GraftAbilities/
        LungeBite.asset
        PowerLeap.asset
        Harden.asset
        ... (one per active ability)
```

### 9.5 Implementation Priority Order

1. **Phase 1 -- Foundation:** `StatModifierData`, `GraftPartSO`, `BodyPartSlot`, `GraftSystem`, `AnatomyManager`. Get stat modification working with one test part (Wolf Haunches). Verify `PlayerMovement` stats actually change.

2. **Phase 2 -- Loot:** `CreatureLootTable`, `IncapacitationController`, `PartDropPickup`. Get parts dropping on kill. Wire into `EnemyHealth.Die()`.

3. **Phase 3 -- DNA:** `DNASampleSO`, `SuitSlot`, `DNASuitSystem`, `SuitEnergy`. Get DNA extraction on incapacitate. Wire into `InteractController`.

4. **Phase 4 -- Abilities:** Implement concrete ability MonoBehaviours one at a time, starting with the simplest (Wolf Haunches passive speed boost is already handled by stats; first real ability to implement is Lunge Bite or Power Leap).

5. **Phase 5 -- Polish:** Species Affinity, Incompatibility, DNA tiers, Hybrid DNA, VFX, UI panels.

---

## Appendix A: Quick Reference Stat Tables

### Graft Stat Modifiers

| Part | Slot | HP Flat | HP Mult | Walk Mult | Sprint Mult | Jump Mult | Dodge Mult | Atk Mult | Resist | Air Mult |
|------|------|---------|---------|-----------|-------------|-----------|------------|----------|--------|----------|
| Wolf Jaw | HEAD | 0 | 1.0 | 1.0 | 1.0 | 1.0 | 1.0 | 1.3 | 0 | 1.0 |
| Wolf Haunches | LEGS | 0 | 1.0 | 1.2 | 1.4 | 1.0 | 1.0 | 1.0 | 0 | 1.0 |
| Wolf Hide | TORSO | +15 | 1.0 | 1.0 | 1.0 | 1.0 | 1.0 | 1.0 | 0.10 | 1.0 |
| Frog Legs | LEGS | 0 | 1.0 | 0.9 | 1.0 | 1.8 | 1.0 | 1.0 | 0 | 1.0 |
| Frog Membrane | TORSO | +10 | 1.0 | 1.0 | 1.0 | 1.0 | 1.0 | 1.0 | 0 | 1.0 |
| Frog Tongue | HEAD | 0 | 1.0 | 1.0 | 1.0 | 1.0 | 1.0 | 1.0 | 0 | 1.0 |
| Bug Carapace | TORSO | +30 | 1.0 | 0.85 | 0.85 | 1.0 | 1.0 | 1.0 | 0.20 | 1.0 |
| Bug Antennae | HEAD | 0 | 1.0 | 1.0 | 1.0 | 1.0 | 1.0 | 1.0 | 0 | 1.0 |
| Bug Wing Casings | BACK | 0 | 1.0 | 1.0 | 1.0 | 1.0 | 1.1 | 1.0 | 0 | 1.0 |
| Alien Spring Legs | LEGS | 0 | 1.0 | 1.15 | 1.15 | 1.5 | 1.25 | 1.0 | 0 | 1.0 |
| Alien Grasper | L_ARM | 0 | 1.0 | 1.0 | 1.0 | 1.0 | 1.0 | 1.1 | 0 | 1.0 |
| Alien Dorsal Fin | BACK | 0 | 1.0 | 1.0 | 1.0 | 1.0 | 1.0 | 1.0 | 0 | 1.1 |
| Rockfist | R_ARM | 0 | 1.0 | 1.0 | 1.0 | 1.0 | 1.0 | 1.5 | 0 | 1.0 |
| Boulder Buckler | L_ARM | 0 | 1.0 | 1.0 | 1.0 | 1.0 | 1.0 | 1.0 | 0.15 | 1.0 |
| Stone Heart | TORSO | +50 | 1.0 | 0.8 | 0.8 | 1.0 | 1.0 | 1.0 | 0.25 | 1.0 |
| Geode Spine | BACK | 0 | 1.0 | 1.0 | 1.0 | 1.0 | 1.0 | 1.0 | 0.10 | 1.0 |

### DNA Suit Ability Quick Reference

| DNA Source | Active Ability | Energy | Cooldown | Passive Bonus |
|-----------|---------------|--------|----------|---------------|
| Wolf | Predator Dash | 20 | 6s | Keen Nose (enemy radar) |
| Wolf | Howl | 15 | 10s | -- |
| Frog | Leap Boost | 15 | 8s | Sticky Grip (+slope angle) |
| Frog | Toxic Cloud | 25 | 12s | -- |
| Bug | Chitin Shield | 20 | 15s | Exoskeleton (+10 HP, +5% resist) |
| Bug | Pheromone Decoy | 30 | 20s | -- |
| JumpingAlien | Propulsion Jump | 20 | 4s | Stabilizer (+15% air control) |
| JumpingAlien | Gravity Tether | 25 | 15s | -- |
| RockBoss | Quake Slam | 35 | 15s | Stone Skin (+15 HP, +10% resist) |
| RockBoss | Rock Armor | 30 | 20s | -- |
| RockBoss | Boulder Toss | 25 | 8s | -- |
