# DNA Suit System -- Complete Design Document

## Table of Contents
1. [System Overview](#1-system-overview)
2. [Suit Architecture](#2-suit-architecture)
3. [Energy System](#3-energy-system)
4. [DNA Extraction Process](#4-dna-extraction-process)
5. [DNA Abilities by Creature](#5-dna-abilities-by-creature)
6. [Synergy System](#6-synergy-system)
7. [Suit Upgrade Path](#7-suit-upgrade-path)
8. [C# Implementation Sketch](#8-c-implementation-sketch)
9. [Balance Reference Tables](#9-balance-reference-tables)
10. [Integration Notes](#10-integration-notes-with-existing-systems)

---

## 1. System Overview

The DNA Suit System is the **reversible/tactical** progression path in Off-World. When the player crash-lands, the suit is standard-issue survival gear with no special capabilities. By incapacitating alien creatures and extracting their DNA, the player loads biological data into the suit's adaptive matrix, gaining abilities that can be swapped, stacked, and combined.

**Core design tension:** The suit path trades raw power for flexibility. A player who kills and grafts gets a permanent +40% sprint speed from a Wolf's legs. A player who extracts Wolf DNA gets +15% sprint speed passively, plus an activatable burst sprint that drains energy. The suit player is weaker at any single thing but can adapt their loadout to the situation.

**Key principles:**
- DNA can always be unslotted. Nothing is permanent.
- Every ability has an energy consideration. Free power does not exist.
- Combining DNA from different creatures produces emergent synergies.
- The suit visually communicates what DNA is loaded, but the player always looks human.

---

## 2. Suit Architecture

### 2.1 Slot Count and Unlocking

The suit has **5 DNA slots total**, unlocked progressively:

| Slot | Unlock Condition | Type |
|------|-----------------|------|
| Slot 1 | Available from game start (suit boots up after crash) | Generic |
| Slot 2 | Extract DNA for the first time (tutorial completion) | Generic |
| Slot 3 | Acquire DNA from 3 different creature species | Generic |
| Slot 4 | Defeat (incapacitate or kill) the RockBoss | Generic |
| Slot 5 | Acquire DNA from all 5 creature species | Synergy Slot |

**Rationale for generic slots:** Typed slots (offensive/defensive/mobility) were considered and rejected. Generic slots create more interesting decisions because the player must weigh "do I want three mobility DNA for maximum agility, or do I diversify?" Typed slots would make the choice obvious. Generic slots mean every slot is a real tradeoff.

**Slot 5 -- the Synergy Slot:** This final slot is special. DNA loaded into Slot 5 does not grant its own passive/active abilities. Instead, it amplifies the synergy bonuses of whatever other DNA is already loaded (see Section 6). This creates a reason to keep collecting DNA even after you have a loadout you like.

### 2.2 Stacking Rules

- **Same DNA in multiple slots: YES, with diminishing returns.** Loading Wolf DNA into two slots gives the passive at 100% for the first and 50% for the second. A third copy gives 25%. This means stacking is viable for specialization but never optimal past two copies.
- **Stacking formula:** `effectMultiplier = 1.0 / (2 ^ (copyIndex - 1))` where `copyIndex` starts at 1 for the first copy. So: 1.0, 0.5, 0.25, 0.125.
- **Active abilities do NOT stack.** You only get one instance of each active ability regardless of how many copies of that DNA are loaded. The active's power does not increase with stacking.

### 2.3 Visual Changes

The suit does not deform the player's body (that is the graft path). Instead, DNA manifests as **surface-level visual effects on the suit itself:**

| DNA Loaded | Visual Effect |
|-----------|---------------|
| Wolf | Suit panels develop a faint fur-like texture; faded grey-white color shift on arms and legs |
| Frog | Iridescent green-blue shimmer on suit surface; subtle webbing pattern between fingers on gloves |
| Bug | Chitinous plate overlay on shoulders and forearms; dark amber tint |
| JumpingAlien | Suit leg panels glow with faint bioluminescent lines (cyan); ankle joints show spring-coil pattern |
| RockBoss | Rough stone-like texture overlay on torso and back panels; cracks glow orange when energy is high |

When multiple DNA types are loaded, the effects blend. The dominant visual (most slots of one type) takes priority on the torso; secondary types appear on extremities.

**Energy indicator:** The suit's chest panel has a visible energy gauge -- a ring of bioluminescent dots that dim as energy depletes. When energy hits zero, the suit's DNA visual effects flicker and fade to grey.

---

## 3. Energy System

### 3.1 Core Stats

| Stat | Base Value | Notes |
|------|-----------|-------|
| Max Energy | 100 | Mirrors maxHealth for UI symmetry |
| Passive Drain Rate | 2 energy/sec (total, not per slot) | Flat cost while any DNA is loaded |
| Base Regeneration | 0 energy/sec | Energy does NOT passively regenerate |

**Why no passive regen:** This is the central tension of the suit system. Energy is a scarce resource that forces the player to engage with the world. If energy regenerated passively, the optimal strategy would be to hide and wait. Instead, the player must actively choose how to replenish.

### 3.2 Energy Regeneration Sources

| Source | Energy Gained | Condition |
|--------|--------------|-----------|
| **Melee hit on enemy** | +8 per hit | Must connect (PlayerHitBox triggers) |
| **Incapacitate a creature** | +30 burst | Full incapacitation, not just damage |
| **Harvesting flora** | +5 per harvest | Collecting scannable items (ItemScannable) |
| **Proximity to captured creature** | +3/sec | While holding a creature via EnemyGrabbable |
| **Standing in sunlight** (daytime) | +1/sec | Ambient trickle, only outdoors during day cycle (future system) |

**Design intent:** Combat is the primary energy source. The player who engages enemies recharges fastest. Harvesting and capturing provide slower but safer alternatives. This creates a rhythm: fight to charge up, use abilities to fight better, need to fight again to recharge.

### 3.3 Passive vs Active Energy Costs

**Passive abilities** incur the flat 2 energy/sec drain. This drain is the same whether 1 slot or 4 slots have DNA loaded -- the suit's adaptive matrix draws a fixed overhead. If energy reaches 0, all passive effects immediately deactivate. The passives reactivate the moment energy rises above 0.

**Active abilities** have individual energy costs (see Section 5). Activating an ability when you lack the energy simply fails -- the suit flashes red and emits a warning tone. There is no "going negative."

### 3.4 Energy Depletion Behavior

When energy hits 0:
1. All passive bonuses (stat modifications) are removed instantly
2. Active abilities cannot be triggered
3. The suit's visual DNA effects flicker and go grey
4. The suit enters **Recovery Mode**: the passive drain stops (it was already at 0), and any energy gained from combat/harvesting goes directly into the pool without overhead
5. Once energy reaches 10 or above, passives reactivate and the 2/sec drain resumes

**Recovery Mode** prevents a death spiral where the player gains 1 energy, loses it immediately to drain, and can never recover. The 10-energy threshold gives a small buffer before drain resumes.

---

## 4. DNA Extraction Process

### 4.1 Incapacitation Threshold

Enemies gain a new state: **Incapacitated**. This triggers when their health drops below **15% of max health** without reaching 0. The existing `EnemyHealth.cs` will be extended with an incapacitation threshold.

| Creature | Max HP | Incapacitate Threshold (15%) | Notes |
|----------|--------|------------------------------|-------|
| Wolf | 60 | 9 HP | Fast and aggressive; hard to control damage |
| Frog | 30 | 4 HP | Low HP; easy to accidentally kill |
| Bug | 20 | 3 HP | Very fragile; requires restraint |
| JumpingAlien | 40 | 6 HP | Mobile; hard to pin down |
| RockBoss | 500 | 75 HP | Long fight; incapacitation is a major achievement |

When incapacitated:
- The creature collapses (plays a downed animation)
- AI is disabled (`EnableAI(false)`)
- The creature remains downed for **12 seconds** before either dying (bleeding out) or recovering to 15% HP and fleeing
- The player must extract during this 12-second window

### 4.2 Extraction Equipment

Extraction requires the **DNA Extractor**, a tool that replaces the Net in the player's main hand slot. The Extractor is crafted or found early in the game (near the crash site as part of the ship's scattered cargo).

**The Extractor is NOT the Net.** The Net (`NetScript.cs`) is for live capture. The Extractor is a separate tool for DNA sampling. Both can exist in the player's inventory, but you can only hold one main-hand tool at a time.

### 4.3 Step-by-Step Extraction

1. **Weaken the creature.** Attack it using melee (`AttackController.cs` -- SwipeRight/SwipeLeft) or environmental hazards. Each hit deals 20 damage (existing `PlayerHitBox` value). The player must stop attacking before killing the creature.

2. **Wait for incapacitation.** When the creature's health drops below the threshold, it enters the downed state. A UI indicator ("DNA READY") appears above the creature.

3. **Equip the DNA Extractor.** The player must have it in their inventory and equip it as the held item (replaces whatever is in the main hand via `InteractController`).

4. **Approach and interact.** With the Extractor equipped, the player aims at the downed creature within `InteractRange` (3 units, matching existing interact range) and presses the interact key.

5. **Channel the extraction.** Extraction is a **channeled action lasting 3 seconds**. During this time:
   - The player cannot move (movement input is suppressed)
   - The player cannot attack or dodge
   - A circular progress bar appears on the HUD
   - The creature plays a "sampling" particle effect (needle/scan visual)
   - If the player takes damage, the channel is interrupted and must be restarted
   - If the creature's bleed-out timer expires during extraction, the extraction fails and the creature dies

6. **Receive DNA sample.** On successful extraction, the player receives one DNA sample of that creature's type. The creature then either dies (consumed by extraction) or recovers and flees, depending on a design toggle. **Recommended: the creature recovers and flees.** This reinforces the "merciful" identity of the suit path and allows re-extraction from the same species in the wild.

### 4.4 Extraction Multiplicity

- You CAN extract from the same creature species multiple times (from different individual creatures) to gain multiple copies for stacking.
- Each individual creature can only be extracted from ONCE. After extraction, that specific creature is marked and cannot be sampled again (even if it recovers and is incapacitated a second time).
- DNA samples are stored in a persistent inventory. Loading a sample into a suit slot consumes it. Unloading a slot returns the sample to inventory.

### 4.5 RockBoss Extraction (Special Case)

The RockBoss is a boss encounter. Incapacitating it (bringing it below 75 HP out of 500) is exceptionally difficult because of its behavior tree's aggression. Special rules:

- The RockBoss's incapacitation window is **20 seconds** (longer than normal creatures) to compensate for the difficulty of reaching this state.
- Extraction from the RockBoss takes **5 seconds** instead of 3 (the DNA is denser/more complex).
- Successful RockBoss extraction yields **2 DNA samples** instead of 1.
- The RockBoss does NOT recover after extraction -- it is destroyed. This makes RockBoss DNA extremely valuable and finite (one extraction per RockBoss encounter, and RockBoss spawns are rare).

---

## 5. DNA Abilities by Creature

### 5.1 Wolf DNA

**Creature identity:** Pack predator. Fast, aggressive, persistent. Chases prey relentlessly, investigates where it last saw targets.

| Ability | Type | Effect | Energy Cost |
|---------|------|--------|-------------|
| **Predator's Stride** | Passive | +15% walkSpeed, +15% sprintSpeed | Part of flat 2/sec drain |
| **Bloodrush** | Active | For 4 seconds: +60% sprintSpeed, +30% walkSpeed. Melee hits during Bloodrush restore +5 extra energy each. After Bloodrush ends, 1 second of 20% movement slow. | 25 energy, 12 sec cooldown |

**Design rationale:** The Wolf is the "speed" creature. The passive gives a modest always-on boost. The active creates a high-risk window: you sprint in fast, land hits to recoup energy, but are slowed afterward. This encourages aggressive in-and-out play.

### 5.2 Frog DNA

**Creature identity:** Skittish prey animal. Hops away from threats. Agile but fragile. Uses vertical movement to escape.

| Ability | Type | Effect | Energy Cost |
|---------|------|--------|-------------|
| **Springform Legs** | Passive | +20% jumpForce. Reduce fall damage by 40% (when fall damage system is implemented; until then, reduces landing stagger). | Part of flat 2/sec drain |
| **Toxic Leap** | Active | Perform a powered leap (2.5x jumpForce, directional -- launches toward crosshair aim point). On landing, emit a 4-unit radius toxic cloud that lasts 3 seconds, dealing 8 damage/sec to enemies inside it. The player is immune to their own cloud. | 20 energy, 8 sec cooldown |

**Design rationale:** Frog DNA is about vertical mobility and area denial. The passive makes general traversal better. The active combines a gap-closer/escape with damage, but the cloud is easy for fast enemies to leave, so it rewards positioning (chokepoints, corners).

### 5.3 Bug DNA

**Creature identity:** Tiny, fragile, but numerous. Hard to hit. Survival through being overlooked.

| Ability | Type | Effect | Energy Cost |
|---------|------|--------|-------------|
| **Chitin Weave** | Passive | Incoming damage reduced by 3 flat (after percentage reductions if any). Minimum damage taken is 1. | Part of flat 2/sec drain |
| **Swarm Shield** | Active | For 5 seconds, a cloud of holographic bug-constructs surrounds the player. Any enemy projectile (EnemyArcProjectile) that enters the 3-unit radius cloud is destroyed. Melee attacks that hit the player during Swarm Shield deal 50% reduced damage. While active, the player's movement speed is reduced by 15%. | 30 energy, 15 sec cooldown |

**Design rationale:** Bug DNA is the "defensive" option. The passive flat damage reduction is most effective against frequent small hits (like Wolf bite attacks at 10 damage) and least effective against big hits (RockBoss slam). The active provides a panic button that is specifically strong against projectiles -- making it the counter to the RockBoss's rock throw -- but slows you down so it is not a free escape.

### 5.4 JumpingAlien DNA

**Creature identity:** Strange, bouncy creature. High vertical mobility. Alien and unpredictable movement patterns.

| Ability | Type | Effect | Energy Cost |
|---------|------|--------|-------------|
| **Alien Tendons** | Passive | +35% jumpForce. Air control increased: airSpeedMultiplier changes from its base value to base * 1.5 (50% more responsive in the air). | Part of flat 2/sec drain |
| **Blink Step** | Active | Instantly teleport 8 units in the direction of current movement input (or forward if no input). 0.3-second invulnerability window during the teleport. Cannot teleport through solid geometry thicker than 1 unit (prevents wall clipping, but allows passing through fences/bars). Leave a brief afterimage at the origin point. | 22 energy, 6 sec cooldown |

**Design rationale:** JumpingAlien DNA is the "mobility" specialist. The passive overlaps with Frog's jump boost but focuses on air control rather than ground utility. The active is the suit system's premier escape/reposition tool -- short cooldown, instant, but expensive enough that you cannot spam it. The 8-unit range is exactly enough to dodge a RockBoss slam or escape a Wolf pack.

### 5.5 RockBoss DNA

**Creature identity:** Massive, powerful, territorial. Slow but devastating. The apex predator of the planet.

| Ability | Type | Effect | Energy Cost |
|---------|------|--------|-------------|
| **Titan's Constitution** | Passive | +25 maxHealth (from 100 to 125). Immunity to knockback from attacks that deal less than 15 damage. | Part of flat 2/sec drain |
| **Seismic Slam** | Active | The player slams the ground with both fists (0.8-second wind-up animation, player is locked in place). Creates a shockwave in a 6-unit radius around the player. Enemies in the radius take 40 damage and are knocked away 4 units. Destroys small environmental objects (rocks, bushes). | 40 energy, 20 sec cooldown |

**Design rationale:** RockBoss DNA is the "power" option -- expensive to acquire, expensive to use, but the most impactful. The passive +25 HP is the largest survivability boost in the suit system, and knockback immunity fundamentally changes how you fight packs. The active is a panic-button crowd clearer with the highest damage and energy cost, but the wind-up makes it punishable if timed poorly.

---

## 6. Synergy System

When two or more different DNA types are loaded simultaneously, they produce **synergy bonuses**. These bonuses are automatic -- the player does not need to do anything beyond loading the right combination.

### 6.1 Two-DNA Synergies

| Combo | Synergy Name | Effect |
|-------|-------------|--------|
| Wolf + Frog | **Pounce Predator** | After using Toxic Leap (Frog active), Bloodrush (Wolf active) cooldown is immediately reset. Can only trigger once per 30 seconds. |
| Wolf + Bug | **Relentless Swarm** | While Bloodrush is active, each melee hit also applies a 2-second slow (20% movement reduction) to the target enemy. |
| Wolf + JumpingAlien | **Pursuit Protocol** | Blink Step's distance increases from 8 to 12 units if used while sprinting. |
| Wolf + RockBoss | **Apex Momentum** | Killing an enemy while Bloodrush is active instantly restores 15 energy. (Note: "killing" here refers to a creature dying while the player has Wolf+RockBoss loaded, even on the suit path.) |
| Frog + Bug | **Toxic Carapace** | When Swarm Shield is active, it also emits the Frog's toxic cloud (4-unit radius, 5 dmg/sec) around the player for the shield's duration. |
| Frog + JumpingAlien | **Double Jump** | After a normal jump, pressing the jump key again in midair performs a second jump at 60% of the first jump's force. Costs 5 energy per double-jump. |
| Frog + RockBoss | **Quake Landing** | Toxic Leap's landing shockwave radius increases from 4 to 7 units and also knocks enemies back 2 units. |
| Bug + JumpingAlien | **Phase Shift** | Blink Step's invulnerability window increases from 0.3 seconds to 0.8 seconds. Additionally, the player is invisible for 1.5 seconds after Blink Step ends. |
| Bug + RockBoss | **Fortified Shell** | Chitin Weave's flat damage reduction increases from 3 to 6. Seismic Slam's wind-up time decreases from 0.8 to 0.5 seconds. |
| JumpingAlien + RockBoss | **Meteor Strike** | If Seismic Slam is activated while airborne, the player plummets to the ground instantly and the slam's radius increases from 6 to 9 units. Damage increases from 40 to 55. |

### 6.2 Three-DNA Synergies (selected combinations)

Only listing the most mechanically interesting three-way combinations. Other three-way combos just stack their two-way synergies without an additional bonus.

| Combo | Synergy Name | Effect |
|-------|-------------|--------|
| Wolf + Frog + JumpingAlien | **Apex Acrobat** | All movement-based active abilities (Bloodrush, Toxic Leap, Blink Step) have their cooldowns reduced by 25%. |
| Wolf + Bug + RockBoss | **Unstoppable Force** | While Bloodrush is active, the player is immune to all knockback and stagger, and Chitin Weave's reduction applies double (6 flat, or 12 with Fortified Shell). |
| Frog + Bug + JumpingAlien | **Evasion Matrix** | Double Jump no longer costs energy. Swarm Shield duration increases from 5 to 7 seconds. |
| Bug + JumpingAlien + RockBoss | **Titan's Blink** | Seismic Slam can be activated during Blink Step, detonating at the teleport destination. |

### 6.3 Slot 5 -- Synergy Amplifier

When Slot 5 is unlocked and loaded with any DNA, it does NOT grant that DNA's passive or active ability. Instead:

- All active two-DNA synergies involving the Slot 5 DNA type are **enhanced** (effects increased by ~30%, durations extended by 1-2 seconds, cooldown reductions increased).
- If the Slot 5 DNA matches a DNA already in Slots 1-4, it does NOT count as a stacking copy for passive purposes. It purely enhances synergies.
- If no synergies exist for the Slot 5 DNA (because none of the other slots have compatible types), the Slot 5 DNA provides a **10% energy cost reduction** on all active abilities as a fallback bonus.

---

## 7. Suit Upgrade Path

### 7.1 Upgrade Categories

The suit improves through **Suit Modules** -- physical components found in the world or crafted from materials. These are permanent upgrades to the suit's base capabilities, independent of which DNA is loaded.

| Module | Effect | How to Acquire |
|--------|--------|----------------|
| **Energy Cell Mk I** | Max Energy +25 (100 -> 125) | Found near crash site (early game guaranteed) |
| **Energy Cell Mk II** | Max Energy +25 (125 -> 150) | Crafted from 3 RockBoss rock projectile fragments (collect rocks the boss throws at you) |
| **Energy Cell Mk III** | Max Energy +25 (150 -> 175) | Reward for incapacitating one of every creature species |
| **Adaptive Matrix Upgrade** | Passive drain reduced from 2/sec to 1.5/sec | Acquired by loading DNA into all available slots simultaneously for the first time |
| **Combat Siphon** | Melee energy gain increased from +8 to +12 per hit | Defeat 30 creatures (kill or incapacitate) |
| **Rapid Extractor** | Extraction channel time reduced from 3 sec to 2 sec | Extract DNA from 5 different individual creatures |
| **Resonance Amplifier** | All active ability cooldowns reduced by 15% | Trigger 5 different synergy bonuses in a single play session |
| **Hardened Casing** | While below 30% energy, incoming damage reduced by 20% | Survive 10 hits at 0 energy (encourages playing through depletion rather than avoiding it) |

### 7.2 Progression Curve

The suit upgrade path is designed to follow the game's exploration arc:

**Early game (0-30 minutes):**
- Player finds the Extractor near the crash site
- Slot 1 available immediately; Slot 2 unlocks on first extraction
- Energy Cell Mk I found in crash debris
- The player learns the extraction loop on Bugs and Frogs (weakest creatures)

**Mid game (30-90 minutes):**
- Player has encountered Wolves and JumpingAliens
- Slot 3 unlocks after sampling 3 species
- Combat Siphon and Rapid Extractor earned through natural play
- The player experiments with synergy combinations
- Adaptive Matrix Upgrade encourages filling all slots

**Late game (90+ minutes):**
- Player challenges the RockBoss
- Slot 4 unlocks on RockBoss defeat; Slot 5 on full species collection
- Energy Cell Mk II requires engaging with RockBoss combat (collecting thrown rocks)
- Resonance Amplifier rewards deep synergy experimentation
- Full 5-slot loadouts with Synergy Amplifier become possible

### 7.3 Relationship to World Progression

The suit system ties into terrain/biome progression through creature availability:

| Biome (by terrain height) | Available Creatures | DNA Available |
|---------------------------|-------------------|---------------|
| Low coastal/beach | Bug, Frog | Defensive, Mobility (basic) |
| Mid grasslands | Wolf, Bug, Frog | Speed, Defense, Mobility |
| High rocky terrain | Wolf, JumpingAlien | Speed, Advanced Mobility |
| Mountain peaks/boss arena | RockBoss | Power |

This means a player exploring upward gains access to increasingly powerful DNA. A player who stays in safe coastal areas can only access Bug and Frog DNA -- functional but limited. The suit system rewards exploration.

---

## 8. C# Implementation Sketch

### 8.1 DNAType.cs -- ScriptableObject for DNA definitions

```csharp
using UnityEngine;

[CreateAssetMenu(fileName = "NewDNAType", menuName = "Off-World/DNA Type")]
public class DNAType : ScriptableObject
{
    [Header("Identity")]
    public string dnaName;           // "Wolf", "Frog", "Bug", "JumpingAlien", "RockBoss"
    public CreatureSpecies species;
    public Sprite icon;
    public Color suitTintColor;      // The color tint applied to suit visuals

    [Header("Passive Stats")]
    public float walkSpeedMultiplier;    // e.g. 1.15 for +15%
    public float sprintSpeedMultiplier;
    public float jumpForceMultiplier;
    public float airControlMultiplier;
    public int maxHealthBonus;           // Flat bonus
    public int flatDamageReduction;      // Bug's Chitin Weave
    public float fallDamageReduction;    // 0-1 range

    [Header("Active Ability Reference")]
    public DNAAbility activeAbility;     // Reference to the active ability prefab/SO

    [Header("Extraction")]
    public float extractionTime;         // Seconds to channel (3 for normal, 5 for RockBoss)
    public int samplesPerExtraction;     // 1 for normal, 2 for RockBoss
}

public enum CreatureSpecies
{
    Wolf,
    Frog,
    Bug,
    JumpingAlien,
    RockBoss
}
```

### 8.2 DNASlot.cs -- Individual slot data

```csharp
using UnityEngine;

[System.Serializable]
public class DNASlot
{
    public int slotIndex;
    public bool isUnlocked;
    public bool isSynergySlot;       // True only for Slot 5
    public DNAType loadedDNA;        // Null if empty

    /// <summary>
    /// Returns the stacking multiplier for this slot's DNA based on how many
    /// copies of the same DNA exist in earlier slots.
    /// </summary>
    public float GetStackMultiplier(DNASuit suit)
    {
        if (loadedDNA == null) return 0f;
        if (isSynergySlot) return 0f; // Synergy slot doesn't grant passives

        int copyIndex = 0;
        for (int i = 0; i < slotIndex; i++)
        {
            if (suit.slots[i].loadedDNA != null
                && suit.slots[i].loadedDNA.species == loadedDNA.species
                && !suit.slots[i].isSynergySlot)
            {
                copyIndex++;
            }
        }

        // 1.0, 0.5, 0.25, 0.125 ...
        return 1f / Mathf.Pow(2f, copyIndex);
    }

    public void LoadDNA(DNAType dna)
    {
        loadedDNA = dna;
    }

    public DNAType UnloadDNA()
    {
        DNAType removed = loadedDNA;
        loadedDNA = null;
        return removed;
    }

    public bool IsEmpty => loadedDNA == null;
}
```

### 8.3 SuitEnergy.cs -- Energy management

```csharp
using UnityEngine;
using System;

public class SuitEnergy : MonoBehaviour
{
    [Header("Energy")]
    [SerializeField] private float maxEnergy = 100f;
    [SerializeField] private float currentEnergy;

    [Header("Drain")]
    [SerializeField] private float passiveDrainRate = 2f;  // per second
    [SerializeField] private float recoveryThreshold = 10f; // energy needed to exit recovery

    [Header("Combat Regen")]
    [SerializeField] private float meleeHitGain = 8f;
    [SerializeField] private float incapacitateGain = 30f;
    [SerializeField] private float harvestGain = 5f;
    [SerializeField] private float captureProximityGain = 3f; // per second

    private bool inRecoveryMode = false;
    private bool anyDNALoaded = false;

    // Events for UI binding
    public event Action<float, float> OnEnergyChanged; // current, max
    public event Action OnEnergyDepleted;
    public event Action OnEnergyRecovered;

    public float CurrentEnergy => currentEnergy;
    public float MaxEnergy => maxEnergy;
    public bool IsInRecoveryMode => inRecoveryMode;
    public bool HasEnergy => currentEnergy > 0f;

    private void Start()
    {
        currentEnergy = maxEnergy;
    }

    private void Update()
    {
        if (!anyDNALoaded) return;

        if (!inRecoveryMode)
        {
            // Apply passive drain
            DrainEnergy(passiveDrainRate * Time.deltaTime);
        }
        else
        {
            // In recovery mode, check if we've reached the threshold to reactivate
            if (currentEnergy >= recoveryThreshold)
            {
                inRecoveryMode = false;
                OnEnergyRecovered?.Invoke();
            }
        }
    }

    public void SetDNALoaded(bool loaded)
    {
        anyDNALoaded = loaded;
        if (!loaded)
        {
            inRecoveryMode = false;
        }
    }

    /// <summary> Attempt to spend energy on an active ability. Returns false if insufficient. </summary>
    public bool TrySpendEnergy(float amount)
    {
        if (inRecoveryMode) return false;
        if (currentEnergy < amount) return false;

        currentEnergy -= amount;
        OnEnergyChanged?.Invoke(currentEnergy, maxEnergy);

        if (currentEnergy <= 0f)
        {
            currentEnergy = 0f;
            EnterRecoveryMode();
        }

        return true;
    }

    /// <summary> Add energy from combat, harvesting, etc. </summary>
    public void GainEnergy(float amount)
    {
        currentEnergy = Mathf.Min(currentEnergy + amount, maxEnergy);
        OnEnergyChanged?.Invoke(currentEnergy, maxEnergy);
    }

    public void OnMeleeHit() => GainEnergy(meleeHitGain);
    public void OnCreatureIncapacitated() => GainEnergy(incapacitateGain);
    public void OnHarvest() => GainEnergy(harvestGain);
    public void OnCaptureProximityTick() => GainEnergy(captureProximityGain * Time.deltaTime);

    public void IncreaseMaxEnergy(float amount)
    {
        maxEnergy += amount;
    }

    public void SetPassiveDrainRate(float newRate)
    {
        passiveDrainRate = newRate;
    }

    private void DrainEnergy(float amount)
    {
        currentEnergy -= amount;
        OnEnergyChanged?.Invoke(currentEnergy, maxEnergy);

        if (currentEnergy <= 0f)
        {
            currentEnergy = 0f;
            EnterRecoveryMode();
        }
    }

    private void EnterRecoveryMode()
    {
        inRecoveryMode = true;
        OnEnergyDepleted?.Invoke();
    }
}
```

### 8.4 DNAAbility.cs -- Base class for active abilities

```csharp
using UnityEngine;

public abstract class DNAAbility : ScriptableObject
{
    [Header("Ability Base")]
    public string abilityName;
    public string description;
    public Sprite icon;
    public float energyCost;
    public float cooldown;
    public KeyCode activationKey = KeyCode.F; // Default; can be rebound

    [HideInInspector] public float lastActivationTime = -999f;

    /// <summary> Whether the cooldown has elapsed. </summary>
    public bool IsReady => Time.time - lastActivationTime >= cooldown;

    /// <summary> Returns the current cooldown remaining. </summary>
    public float CooldownRemaining => Mathf.Max(0f, cooldown - (Time.time - lastActivationTime));

    /// <summary>
    /// Attempt to activate this ability. Returns true if activation succeeded.
    /// Subclasses implement the actual effect in Execute().
    /// </summary>
    public bool TryActivate(DNASuit suit)
    {
        if (!IsReady) return false;
        if (!suit.Energy.TrySpendEnergy(energyCost)) return false;

        lastActivationTime = Time.time;
        Execute(suit);
        return true;
    }

    /// <summary> Implement the actual ability effect. </summary>
    protected abstract void Execute(DNASuit suit);

    /// <summary> Called every frame while this ability is slotted. Override for ongoing effects. </summary>
    public virtual void Tick(DNASuit suit) { }

    /// <summary> Called when this ability is unslotted or the DNA is removed. Clean up any persistent effects. </summary>
    public virtual void Cleanup(DNASuit suit) { }

    /// <summary> Apply cooldown reduction (e.g. from synergies or upgrades). </summary>
    public float GetModifiedCooldown(float reductionPercent)
    {
        return cooldown * (1f - Mathf.Clamp01(reductionPercent));
    }
}
```

Example concrete ability:

```csharp
using UnityEngine;
using System.Collections;

[CreateAssetMenu(fileName = "BloodrushAbility", menuName = "Off-World/Abilities/Bloodrush")]
public class BloodrushAbility : DNAAbility
{
    [Header("Bloodrush Specifics")]
    public float duration = 4f;
    public float sprintBoost = 0.60f;   // +60%
    public float walkBoost = 0.30f;     // +30%
    public float bonusEnergyPerHit = 5f;
    public float postSlowDuration = 1f;
    public float postSlowAmount = 0.20f; // 20% slow

    protected override void Execute(DNASuit suit)
    {
        suit.StartCoroutine(BloodrushCoroutine(suit));
    }

    private IEnumerator BloodrushCoroutine(DNASuit suit)
    {
        PlayerMovement movement = suit.GetComponent<PlayerMovement>();
        float originalWalk = movement.walkSpeed;
        float originalSprint = movement.sprintSpeed;

        // Apply boost
        movement.walkSpeed *= (1f + walkBoost);
        movement.sprintSpeed *= (1f + sprintBoost);
        suit.SetBloodrushActive(true);

        yield return new WaitForSeconds(duration);

        // Revert and apply slow
        movement.walkSpeed = originalWalk;
        movement.sprintSpeed = originalSprint;
        suit.SetBloodrushActive(false);

        movement.walkSpeed *= (1f - postSlowAmount);
        movement.sprintSpeed *= (1f - postSlowAmount);

        yield return new WaitForSeconds(postSlowDuration);

        movement.walkSpeed = originalWalk;
        movement.sprintSpeed = originalSprint;
    }
}
```

### 8.5 DNASuit.cs -- Main suit controller

```csharp
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class DNASuit : MonoBehaviour
{
    [Header("Slots")]
    public DNASlot[] slots = new DNASlot[5];

    [Header("References")]
    [SerializeField] private PlayerMovement playerMovement;
    [SerializeField] private PlayerHealth playerHealth;
    private SuitEnergy suitEnergy;

    [Header("DNA Inventory")]
    public List<DNAType> dnaInventory = new List<DNAType>();

    [Header("Active Ability Keybinds")]
    public KeyCode ability1Key = KeyCode.Alpha1;
    public KeyCode ability2Key = KeyCode.Alpha2;
    public KeyCode ability3Key = KeyCode.Alpha3;
    public KeyCode ability4Key = KeyCode.Alpha4;

    // Synergy tracking
    private HashSet<string> activeSynergies = new HashSet<string>();
    private float cooldownReductionPercent = 0f;

    // Internal state
    private bool bloodrushActive = false;
    private bool passivesActive = false;

    // Cached base stats for restoration
    private float baseWalkSpeed;
    private float baseSprintSpeed;
    private float baseJumpForce;
    private float baseAirSpeedMultiplier;
    private int baseMaxHealth;

    public SuitEnergy Energy => suitEnergy;

    private void Awake()
    {
        suitEnergy = GetComponent<SuitEnergy>();

        if (playerMovement == null) playerMovement = GetComponent<PlayerMovement>();
        if (playerHealth == null) playerHealth = GetComponent<PlayerHealth>();

        // Initialize slots
        for (int i = 0; i < slots.Length; i++)
        {
            slots[i] = new DNASlot
            {
                slotIndex = i,
                isUnlocked = (i == 0), // Only Slot 0 starts unlocked
                isSynergySlot = (i == 4),
                loadedDNA = null
            };
        }
    }

    private void Start()
    {
        // Cache base stats
        baseWalkSpeed = playerMovement.walkSpeed;
        baseSprintSpeed = playerMovement.sprintSpeed;
        baseJumpForce = playerMovement.jumpForce;
        baseAirSpeedMultiplier = playerMovement.airSpeedMultiplier;
        baseMaxHealth = playerHealth.maxHealth;
    }

    private void Update()
    {
        // Handle active ability inputs
        HandleAbilityInput();

        // Tick active abilities (for ongoing effects)
        foreach (var slot in slots)
        {
            if (slot.isUnlocked && !slot.IsEmpty && !slot.isSynergySlot)
            {
                slot.loadedDNA.activeAbility?.Tick(this);
            }
        }

        // Check if passives should toggle based on energy state
        if (suitEnergy.IsInRecoveryMode && passivesActive)
        {
            DeactivatePassives();
        }
        else if (!suitEnergy.IsInRecoveryMode && !passivesActive && HasAnyDNALoaded())
        {
            ActivatePassives();
        }
    }

    // ---- Slot Management ----

    public bool LoadDNA(int slotIndex, DNAType dna)
    {
        if (slotIndex < 0 || slotIndex >= slots.Length) return false;
        if (!slots[slotIndex].isUnlocked) return false;
        if (!dnaInventory.Contains(dna)) return false;

        // Unload existing DNA if slot is occupied
        if (!slots[slotIndex].IsEmpty)
        {
            UnloadDNA(slotIndex);
        }

        dnaInventory.Remove(dna);
        slots[slotIndex].LoadDNA(dna);

        RecalculatePassives();
        RecalculateSynergies();
        suitEnergy.SetDNALoaded(HasAnyDNALoaded());

        return true;
    }

    public DNAType UnloadDNA(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= slots.Length) return null;
        if (slots[slotIndex].IsEmpty) return null;

        // Cleanup active ability state
        slots[slotIndex].loadedDNA.activeAbility?.Cleanup(this);

        DNAType removed = slots[slotIndex].UnloadDNA();
        dnaInventory.Add(removed);

        RecalculatePassives();
        RecalculateSynergies();
        suitEnergy.SetDNALoaded(HasAnyDNALoaded());

        return removed;
    }

    public void UnlockSlot(int slotIndex)
    {
        if (slotIndex >= 0 && slotIndex < slots.Length)
        {
            slots[slotIndex].isUnlocked = true;
        }
    }

    // ---- Passive Stat Calculation ----

    private void RecalculatePassives()
    {
        // Reset to base stats
        playerMovement.walkSpeed = baseWalkSpeed;
        playerMovement.sprintSpeed = baseSprintSpeed;
        playerMovement.jumpForce = baseJumpForce;
        playerMovement.airSpeedMultiplier = baseAirSpeedMultiplier;
        playerHealth.maxHealth = baseMaxHealth;

        if (suitEnergy.IsInRecoveryMode)
        {
            passivesActive = false;
            return;
        }

        // Accumulate multipliers from all loaded DNA (with stacking diminish)
        float walkMult = 1f;
        float sprintMult = 1f;
        float jumpMult = 1f;
        float airControlMult = 1f;
        int healthBonus = 0;

        foreach (var slot in slots)
        {
            if (!slot.isUnlocked || slot.IsEmpty || slot.isSynergySlot) continue;

            float stackMult = slot.GetStackMultiplier(this);
            DNAType dna = slot.loadedDNA;

            // Each multiplier is adjusted by stacking: effective = 1 + (bonus - 1) * stackMult
            // For a 1.15 multiplier (15% bonus), with stackMult 0.5: 1 + 0.15 * 0.5 = 1.075
            walkMult += (dna.walkSpeedMultiplier - 1f) * stackMult;
            sprintMult += (dna.sprintSpeedMultiplier - 1f) * stackMult;
            jumpMult += (dna.jumpForceMultiplier - 1f) * stackMult;
            airControlMult += (dna.airControlMultiplier - 1f) * stackMult;
            healthBonus += Mathf.RoundToInt(dna.maxHealthBonus * stackMult);
        }

        playerMovement.walkSpeed = baseWalkSpeed * walkMult;
        playerMovement.sprintSpeed = baseSprintSpeed * sprintMult;
        playerMovement.jumpForce = baseJumpForce * jumpMult;
        playerMovement.airSpeedMultiplier = baseAirSpeedMultiplier * airControlMult;
        playerHealth.maxHealth = baseMaxHealth + healthBonus;

        passivesActive = true;
    }

    private void ActivatePassives()
    {
        RecalculatePassives();
    }

    private void DeactivatePassives()
    {
        playerMovement.walkSpeed = baseWalkSpeed;
        playerMovement.sprintSpeed = baseSprintSpeed;
        playerMovement.jumpForce = baseJumpForce;
        playerMovement.airSpeedMultiplier = baseAirSpeedMultiplier;
        playerHealth.maxHealth = baseMaxHealth;
        passivesActive = false;
    }

    // ---- Synergy Calculation ----

    private void RecalculateSynergies()
    {
        activeSynergies.Clear();
        cooldownReductionPercent = 0f;

        // Get set of unique loaded species (excluding synergy slot for ability purposes)
        HashSet<CreatureSpecies> loadedSpecies = new HashSet<CreatureSpecies>();
        foreach (var slot in slots)
        {
            if (slot.isUnlocked && !slot.IsEmpty)
            {
                loadedSpecies.Add(slot.loadedDNA.species);
            }
        }

        // Check two-DNA synergies
        if (loadedSpecies.Contains(CreatureSpecies.Wolf) && loadedSpecies.Contains(CreatureSpecies.Frog))
            activeSynergies.Add("PouncePredator");

        if (loadedSpecies.Contains(CreatureSpecies.Wolf) && loadedSpecies.Contains(CreatureSpecies.Bug))
            activeSynergies.Add("RelentlessSwarm");

        if (loadedSpecies.Contains(CreatureSpecies.Wolf) && loadedSpecies.Contains(CreatureSpecies.JumpingAlien))
            activeSynergies.Add("PursuitProtocol");

        if (loadedSpecies.Contains(CreatureSpecies.Wolf) && loadedSpecies.Contains(CreatureSpecies.RockBoss))
            activeSynergies.Add("ApexMomentum");

        if (loadedSpecies.Contains(CreatureSpecies.Frog) && loadedSpecies.Contains(CreatureSpecies.Bug))
            activeSynergies.Add("ToxicCarapace");

        if (loadedSpecies.Contains(CreatureSpecies.Frog) && loadedSpecies.Contains(CreatureSpecies.JumpingAlien))
            activeSynergies.Add("DoubleJump");

        if (loadedSpecies.Contains(CreatureSpecies.Frog) && loadedSpecies.Contains(CreatureSpecies.RockBoss))
            activeSynergies.Add("QuakeLanding");

        if (loadedSpecies.Contains(CreatureSpecies.Bug) && loadedSpecies.Contains(CreatureSpecies.JumpingAlien))
            activeSynergies.Add("PhaseShift");

        if (loadedSpecies.Contains(CreatureSpecies.Bug) && loadedSpecies.Contains(CreatureSpecies.RockBoss))
            activeSynergies.Add("FortifiedShell");

        if (loadedSpecies.Contains(CreatureSpecies.JumpingAlien) && loadedSpecies.Contains(CreatureSpecies.RockBoss))
            activeSynergies.Add("MeteorStrike");

        // Check three-DNA synergies
        if (loadedSpecies.Contains(CreatureSpecies.Wolf) && loadedSpecies.Contains(CreatureSpecies.Frog) && loadedSpecies.Contains(CreatureSpecies.JumpingAlien))
        {
            activeSynergies.Add("ApexAcrobat");
            cooldownReductionPercent += 0.25f; // 25% CDR
        }

        if (loadedSpecies.Contains(CreatureSpecies.Frog) && loadedSpecies.Contains(CreatureSpecies.Bug) && loadedSpecies.Contains(CreatureSpecies.JumpingAlien))
            activeSynergies.Add("EvasionMatrix");

        if (loadedSpecies.Contains(CreatureSpecies.Bug) && loadedSpecies.Contains(CreatureSpecies.JumpingAlien) && loadedSpecies.Contains(CreatureSpecies.RockBoss))
            activeSynergies.Add("TitansBlink");

        // Synergy slot amplification
        if (slots[4].isUnlocked && !slots[4].IsEmpty)
        {
            CreatureSpecies ampSpecies = slots[4].loadedDNA.species;
            // Enhance synergies involving the amplified species (handled in ability code via HasAmplifiedSynergy)
        }
    }

    public bool HasSynergy(string synergyName) => activeSynergies.Contains(synergyName);

    public bool HasAmplifiedSynergy(string synergyName)
    {
        if (!activeSynergies.Contains(synergyName)) return false;
        if (slots[4].IsEmpty) return false;

        // Check if the synergy involves the Slot 5 species
        // (Implementation would check species pairs per synergy name)
        return true; // Simplified; full implementation maps synergy names to required species
    }

    public float GetCooldownReduction() => cooldownReductionPercent;

    // ---- Ability Input ----

    private void HandleAbilityInput()
    {
        // Map each non-empty, non-synergy slot to an ability key
        int abilityIndex = 0;
        KeyCode[] abilityKeys = { ability1Key, ability2Key, ability3Key, ability4Key };

        foreach (var slot in slots)
        {
            if (!slot.isUnlocked || slot.IsEmpty || slot.isSynergySlot) continue;
            if (abilityIndex >= abilityKeys.Length) break;

            if (Input.GetKeyDown(abilityKeys[abilityIndex]))
            {
                slot.loadedDNA.activeAbility?.TryActivate(this);
            }

            abilityIndex++;
        }
    }

    // ---- Utility ----

    public bool HasAnyDNALoaded()
    {
        foreach (var slot in slots)
        {
            if (slot.isUnlocked && !slot.IsEmpty) return true;
        }
        return false;
    }

    public int CountDNAOfSpecies(CreatureSpecies species)
    {
        int count = 0;
        foreach (var slot in slots)
        {
            if (slot.isUnlocked && !slot.IsEmpty && slot.loadedDNA.species == species)
                count++;
        }
        return count;
    }

    // State flags for synergy interactions
    public void SetBloodrushActive(bool active) { bloodrushActive = active; }
    public bool IsBloodrushActive() => bloodrushActive;
}
```

### 8.6 DNAExtractor.cs -- Extraction tool and process

```csharp
using System.Collections;
using UnityEngine;

public class DNAExtractor : MonoBehaviour, InteractableI
{
    [Header("Extraction Settings")]
    [SerializeField] private float baseExtractionTime = 3f;
    [SerializeField] private float bossExtractionTime = 5f;
    [SerializeField] private float extractionRange = 3f;

    [Header("References")]
    [SerializeField] private Transform fpsCam;
    [SerializeField] private LayerMask enemyLayer;

    private DNASuit suit;
    private PlayerMovement playerMovement;
    private bool isExtracting = false;
    private Coroutine extractionCoroutine;

    // UI references (assigned in Inspector or found at runtime)
    private ExtractionProgressUI progressUI;

    private void Awake()
    {
        suit = GetComponentInParent<DNASuit>();
        playerMovement = GetComponentInParent<PlayerMovement>();
    }

    /// <summary>
    /// Called when the player presses the interact key while this tool is equipped
    /// and aiming at a downed creature.
    /// </summary>
    public void Interact(InteractController controller)
    {
        // This is the pickup interaction; the extractor is an equippable tool
        if (!controller.isEquipped)
        {
            controller.heldObject = gameObject;
            controller.isEquipped = true;
            // Parent to grab point, etc. (same pattern as ObjectGrabbable)
        }
    }

    /// <summary>
    /// Called when the player uses the primary action (LMB) with the extractor equipped.
    /// Initiates extraction if aiming at an incapacitated creature.
    /// </summary>
    public void TryExtract()
    {
        if (isExtracting) return;

        if (Physics.SphereCast(fpsCam.position, 0.5f, fpsCam.forward,
            out RaycastHit hit, extractionRange, enemyLayer))
        {
            EnemyHealth enemyHealth = hit.collider.GetComponent<EnemyHealth>();
            if (enemyHealth == null)
                enemyHealth = hit.collider.GetComponentInParent<EnemyHealth>();

            if (enemyHealth != null && enemyHealth.IsIncapacitated && !enemyHealth.HasBeenExtracted)
            {
                extractionCoroutine = StartCoroutine(
                    ExtractionProcess(enemyHealth)
                );
            }
        }
    }

    private IEnumerator ExtractionProcess(EnemyHealth target)
    {
        isExtracting = true;

        // Determine extraction time based on creature type
        DNAType targetDNA = target.GetDNAType(); // New method on EnemyHealth
        float extractTime = (targetDNA.species == CreatureSpecies.RockBoss)
            ? bossExtractionTime
            : baseExtractionTime;

        // Lock player movement
        float originalWalkSpeed = playerMovement.walkSpeed;
        float originalSprintSpeed = playerMovement.sprintSpeed;
        playerMovement.walkSpeed = 0f;
        playerMovement.sprintSpeed = 0f;

        // Show progress UI
        if (progressUI != null) progressUI.Show(extractTime);

        float elapsed = 0f;

        while (elapsed < extractTime)
        {
            elapsed += Time.deltaTime;

            // Check interruption conditions
            if (target == null || !target.IsIncapacitated)
            {
                // Target died or recovered during extraction
                CancelExtraction(originalWalkSpeed, originalSprintSpeed);
                yield break;
            }

            // Progress UI update
            if (progressUI != null) progressUI.SetProgress(elapsed / extractTime);

            yield return null;
        }

        // Extraction successful
        for (int i = 0; i < targetDNA.samplesPerExtraction; i++)
        {
            suit.dnaInventory.Add(targetDNA);
        }

        target.MarkExtracted(); // Prevent re-extraction from this individual
        suit.Energy.GainEnergy(suit.Energy.MaxEnergy * 0.1f); // Small energy bonus

        // RockBoss dies after extraction; others recover and flee
        if (targetDNA.species == CreatureSpecies.RockBoss)
        {
            target.ForceKill();
        }
        else
        {
            target.RecoverAndFlee();
        }

        // Restore movement
        playerMovement.walkSpeed = originalWalkSpeed;
        playerMovement.sprintSpeed = originalSprintSpeed;

        if (progressUI != null) progressUI.Hide();

        isExtracting = false;
    }

    private void CancelExtraction(float walkSpeed, float sprintSpeed)
    {
        playerMovement.walkSpeed = walkSpeed;
        playerMovement.sprintSpeed = sprintSpeed;

        if (progressUI != null) progressUI.Hide();

        isExtracting = false;
    }

    /// <summary>
    /// Called from PlayerHealth when the player takes damage during extraction.
    /// </summary>
    public void OnPlayerDamaged()
    {
        if (isExtracting && extractionCoroutine != null)
        {
            StopCoroutine(extractionCoroutine);
            CancelExtraction(
                playerMovement.walkSpeed, // Note: these are already 0 during extraction
                playerMovement.sprintSpeed  // Need to cache originals better in production
            );
        }
    }
}
```

### 8.7 Required Changes to Existing Scripts

**EnemyHealth.cs -- additions needed:**

```csharp
// New fields to add to EnemyHealth
[Header("Incapacitation")]
[SerializeField] private float incapacitateThreshold = 0.15f; // 15% of max health
[SerializeField] private float incapacitationDuration = 12f;
[SerializeField] private DNAType dnaType; // Assign in Inspector per creature prefab
private bool isIncapacitated = false;
private bool hasBeenExtracted = false;

public bool IsIncapacitated => isIncapacitated;
public bool HasBeenExtracted => hasBeenExtracted;
public DNAType GetDNAType() => dnaType;

// Modify HurtEnemy to check for incapacitation before death:
public void HurtEnemy(int dmg)
{
    if (activated)
    {
        health -= dmg;
        healthbar.SetHealth(health);

        // Check incapacitation before death
        if (!isIncapacitated && health > 0 && health <= maxHealth * incapacitateThreshold)
        {
            Incapacitate();
        }
        else if (health <= 0)
        {
            Die();
        }
    }
}

private void Incapacitate()
{
    isIncapacitated = true;
    // Disable AI
    IEnemy enemyAI = GetComponent<IEnemy>();
    if (enemyAI != null) enemyAI.EnableAI(false);

    // Start bleed-out timer
    StartCoroutine(IncapacitationTimer());
}

private IEnumerator IncapacitationTimer()
{
    yield return new WaitForSeconds(incapacitationDuration);

    if (isIncapacitated && !hasBeenExtracted)
    {
        // Timer expired: creature either dies or recovers
        RecoverAndFlee();
    }
}

public void MarkExtracted()
{
    hasBeenExtracted = true;
}

public void RecoverAndFlee()
{
    isIncapacitated = false;
    health = Mathf.RoundToInt(maxHealth * incapacitateThreshold);

    IEnemy enemyAI = GetComponent<IEnemy>();
    if (enemyAI != null) enemyAI.EnableAI(true);
    // Creature would enter "flee" behavior here
}

public void ForceKill()
{
    health = 0;
    Die();
}
```

**PlayerHealth.cs -- additions needed:**

```csharp
// Add reference to extractor for interruption
private DNAExtractor extractor;

// In PlayerTakeDMG, after applying damage:
if (extractor != null)
{
    extractor.OnPlayerDamaged();
}
```

---

## 9. Balance Reference Tables

### 9.1 Energy Economy Per Minute

Assuming average combat engagement (hitting an enemy every 2 seconds, incapacitating one creature per minute, one harvest per minute):

| Source | Rate | Per Minute |
|--------|------|-----------|
| Passive drain | -2/sec | -120 |
| Melee hits (30 hits/min avg) | +8 each | +240 |
| Incapacitation (1/min) | +30 each | +30 |
| Harvesting (1/min) | +5 each | +5 |
| **Net per minute** | | **+155** |

This means an actively fighting player has a comfortable energy surplus. A passive/exploring player (5 hits/min, no incaps) gets: -120 + 40 + 5 = **-75/min**, depleting in about 80 seconds. The system rewards engagement.

### 9.2 Ability Cost vs Cooldown Matrix

| Ability | Energy Cost | Cooldown | Cost/Sec (if spammed on CD) |
|---------|------------|----------|-----------------------------|
| Bloodrush | 25 | 12s | 2.08/sec |
| Toxic Leap | 20 | 8s | 2.50/sec |
| Swarm Shield | 30 | 15s | 2.00/sec |
| Blink Step | 22 | 6s | 3.67/sec |
| Seismic Slam | 40 | 20s | 2.00/sec |

Blink Step has the highest sustained energy cost due to its short cooldown -- spamming it drains the pool fast. Seismic Slam is the cheapest on a per-second basis despite the highest single cost, because the long cooldown limits usage.

### 9.3 Passive Stat Modifications Summary

| DNA | walkSpeed | sprintSpeed | jumpForce | airSpeedMult | maxHealth | Damage Reduction |
|-----|-----------|-------------|-----------|-------------|-----------|-----------------|
| Wolf | +15% | +15% | -- | -- | -- | -- |
| Frog | -- | -- | +20% | -- | -- | -40% fall dmg |
| Bug | -- | -- | -- | -- | -- | 3 flat |
| JumpingAlien | -- | -- | +35% | +50% | -- | -- |
| RockBoss | -- | -- | -- | -- | +25 | Knockback immune (<15 dmg) |

---

## 10. Integration Notes with Existing Systems

### 10.1 Compatibility with Power Items

The existing `JumpPowerItem` and `SpeedPowerItem` apply permanent multipliers (`jumpForce *= 2`, `walkSpeed *= 2`). The DNA suit's `RecalculatePassives()` resets stats to cached base values, which means **Power Items consumed before the suit is equipped will be overwritten**.

**Recommended resolution:** When a PowerItem is consumed, update the suit's base stat cache as well:

```csharp
// In JumpPowerItem.Eat(), after modifying jumpForce:
DNASuit suit = player.GetComponent<DNASuit>();
if (suit != null) suit.UpdateBaseJumpForce(playerMovement.jumpForce);
```

Alternatively, remove Power Items from the game once the DNA suit system is implemented, since the suit replaces their function with a more nuanced system.

### 10.2 Compatibility with Dodge Roll

The existing `DodgeRollSequence` in `PlayerMovement.cs` grants invulnerability via `PlayerHealth.StartImmunity()`. DNA abilities like Blink Step also grant invulnerability. These should stack gracefully -- if the player dodges during Blink Step's i-frames, immunity simply continues without interruption. The `EndImmunity()` call should be refactored to use a counter rather than a boolean:

```csharp
private int immunityStackCount = 0;

public void StartImmunity()
{
    immunityStackCount++;
    immune = true;
}

public void EndImmunity()
{
    immunityStackCount = Mathf.Max(0, immunityStackCount - 1);
    if (immunityStackCount == 0) immune = false;
}
```

### 10.3 Compatibility with Net Capture

The Net (`NetScript.cs`) and `EnemyGrabbable.Capture()` system is for picking up creatures. The DNA Extractor is a separate interaction. However, there is an interesting design synergy: capturing a creature with the Net, bringing it to the scanner (`ItemScan.cs`), and scanning it currently produces a PowerItem. This pipeline could be extended so that scanning a captured creature also yields a DNA sample, providing an alternative extraction path that does not require incapacitation.

### 10.4 New Components Per Prefab

| Prefab | New Components to Add |
|--------|----------------------|
| Player | `DNASuit`, `SuitEnergy` (on same GameObject as `PlayerMovement`, `PlayerHealth`) |
| Wolf | `DNAType` ScriptableObject reference on `EnemyHealth`; incapacitation threshold = 15% of 60 = 9 HP |
| Frog | Same; threshold = 15% of 30 = 4 HP |
| Bug | Same; threshold = 15% of 20 = 3 HP |
| JumpingAlien | Same; threshold = 15% of 40 = 6 HP |
| RockBoss | Same; threshold = 15% of 500 = 75 HP; extraction time = 5s; samples = 2 |
| DNA Extractor | New prefab: `DNAExtractor` component, `ObjectGrabbable` (for pickup), `InteractableI` implementation |

### 10.5 UI Requirements

| UI Element | Location | Description |
|-----------|----------|-------------|
| Energy Bar | HUD, below health bar | Slider mirroring healthbar; shows current/max energy with color shift (blue -> grey in recovery) |
| DNA Slot Display | HUD, bottom-center | 5 slot icons showing loaded DNA type icons; locked slots show padlock; empty slots show outline |
| Active Ability Cooldowns | HUD, near slot display | Circular cooldown indicators per active ability, keybind label on each |
| Extraction Progress | HUD, center | Circular progress bar during channeled extraction |
| "DNA READY" Indicator | World-space, above incapacitated creature | Floating text/icon visible within interact range |
| Synergy Tooltips | Inventory/Suit Management screen | When hovering over a slot, show active synergies that DNA participates in |
