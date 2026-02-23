# Off-World: Progression & Gameplay Loop Design Document

> Comprehensive design for gameplay loops, progression systems, risk/reward mechanics,
> emergent combos, world objectives, multiplayer potential, and novel mechanics.
> Grounded in the existing codebase: PlayerMovement, AttackController, PlayerHealth,
> EnemyHealth, ChunkManager spawn tables, BehaviorTree bosses, PowerItemI system,
> net capture (EnemyGrabbable), ItemScan/ItemScannable pipeline, and procedural terrain
> with height-based biomes.

---

## Table of Contents

1. [Core Gameplay Loop](#1-core-gameplay-loop)
2. [Progression Systems](#2-progression-systems)
3. [Risk/Reward Mechanics](#3-riskreward-mechanics)
4. [Emergent Mechanics & Combos](#4-emergent-mechanics--combos)
5. [World Objectives & Endgame](#5-world-objectives--endgame)
6. [Multiplayer Potential](#6-multiplayer-potential)
7. [Novel Ideas](#7-novel-ideas)
8. [Implementation Mapping](#8-implementation-mapping)

---

## 1. Core Gameplay Loop

### 1.1 Moment-to-Moment (30 seconds)

The player is always doing one of three things: **moving through terrain**, **engaging a creature**, or **harvesting a result**.

```
SPOT creature  ->  DECIDE (kill or incapacitate?)  ->  FIGHT / TRAP  ->  HARVEST
     |                                                                       |
     +--<--- equip new part / DNA --<--- return to ship for integration --<---+
```

**Moving through terrain** is not empty time. The procedural world creates constant micro-decisions:
- "That cliff has a creature silhouette on it -- do I have the jump height to reach it?"
- "The terrain drops toward water -- I need amphibian DNA or I turn back."
- "I hear a heavy footstep pattern -- that is a high-tier creature. Am I ready?"

**Engaging a creature** follows the existing combat model (swipe attacks from AttackController, dodge roll from PlayerMovement) but is expanded by the player's current build. A player with grafted mantis arms fights differently than one relying on suit-based wolf speed. The key tension: creatures you want to harvest are also the ones that can kill you.

**Harvesting** is the payoff. Every kill or capture immediately presents a question: "What do I do with this?" That question keeps the loop from ever feeling like grinding.

### 1.2 Minute-to-Minute (5-15 minutes)

A typical gameplay "cycle" is a **sortie** from the crash ship:

1. **Gear up** -- Select active DNA loadout, check graft status, grab tools (net, scanner from existing ItemScan system)
2. **Set an objective** -- "I need a Cliff Stalker carapace for heat resistance" or "I need 2 more frog DNA samples to unlock the wall-climb suit module"
3. **Trek to biome** -- Navigate terrain, encounter common creatures along the way (wolves, bugs from existing spawn tables), make opportunistic kills/captures
4. **Engage target** -- Find the creature you came for. Execute the fight. Make the kill/incapacitate decision in real time based on how the fight went
5. **Return or push deeper** -- Carry your harvest home, or risk going further with what you have

The loop naturally creates tension: the further you go from the ship, the better the creatures, but the more you risk losing if you die.

### 1.3 Session-to-Session (1-3 hours)

Each session should end with the player in a **meaningfully different state** than when they started:

- **New capability unlocked**: "I can now double-jump, so the cliff biome is reachable"
- **Build identity sharpened**: "I committed to the tank archetype by grafting rock armor"
- **Map knowledge expanded**: "I found a cave system under the crystal ridge"
- **Story advanced**: "I discovered that the villagers (existing Villager system) know about a way off-planet"

**The "One More Run" Hook**: The player just died carrying a rare part, OR they are one component away from completing a synergy combo, OR they spotted a creature they have never seen before. The game generates these hooks procedurally through the spawn table system and the combinatorial nature of the part system.

### 1.4 Kill vs. Incapacitate: Why Both Matter

This is not a binary "good vs. evil" system. Both paths are strategically valid and create replay value:

| Aspect | Kill (Graft) | Incapacitate (DNA) |
|--------|-------------|-------------------|
| **Power** | Stronger raw stats | Weaker but stackable |
| **Flexibility** | Permanent; locked in | Swappable at ship |
| **Visual** | Physically transforms player | Suit glows/patterns change |
| **Cost** | Humanity meter decreases | Suit energy capacity used |
| **Gameplay feel** | "I AM the monster" | "I have a tool for every situation" |
| **Replay incentive** | Different graft paths = different playthroughs | Same character, different loadouts per session |

A player who kills everything becomes a terrifying hybrid but is locked into that build. A player who incapacitates everything stays human but has a Swiss Army knife of abilities. The best players will mix both, creating hybrid builds with hard tradeoffs.

---

## 2. Progression Systems

### 2.1 Power Curve

The player's power comes from three sources that scale at different rates:

```
POWER
  ^
  |                                          ________  Grafts (permanent, high ceiling)
  |                                    _____/
  |                              _____/
  |                  ___________/
  |           ______/  ___________________  DNA Loadout (flexible, medium ceiling)
  |     _____/   _____/
  |    /   _____/
  |   / __/
  |  /_/____________________________  Base Skills (dodge, combat timing, map knowledge)
  | /
  |/
  +-------------------------------------------------> TIME
```

**Base Skills (Player Mastery)**: The dodge roll (already implemented with i-frames via `PlayerHealth.StartImmunity`), attack timing, and spatial awareness. These never go away and matter throughout the entire game. A skilled player with zero grafts and basic DNA can beat content that a poorly-played tank build cannot.

**DNA Loadout (Suit Integration)**: Reversible, energy-gated abilities. The player unlocks DNA "modules" by capturing creatures and scanning them (extending the existing `ItemScan` + `ItemScannable` + `EnemyGrabbable` pipeline). DNA modules slot into the suit at the ship. This is the primary progression path for the first few hours and remains relevant throughout.

**Grafts (Anatomy)**: Permanent stat boosts and new abilities gained from killing creatures. Each graft modifies the player's physical form. Grafts are the high-power-ceiling option but carry risk (see Corruption in Section 3). This is mid-to-late game power.

#### Progression Milestones

| Phase | Hours | What Unlocks | What Gates It |
|-------|-------|-------------|---------------|
| **Castaway** | 0-1 | Basic movement, swipe attacks, first creature encounters near ship | Tutorial area; only bugs and passive creatures spawn nearby |
| **Survivor** | 1-3 | Net crafted, first DNA module, ship scanner operational | Need net to incapacitate; need scanner to extract DNA |
| **Hunter** | 3-6 | First graft, 3-4 DNA modules, can reach second biome | Terrain traversal (need jump boost or climb ability); creature difficulty |
| **Apex** | 6-12 | Multiple grafts or full DNA loadout, boss encounters viable | Boss creatures gate access to deeper biomes / endgame areas |
| **Transcendent** | 12+ | Hybrid builds, endgame content, optional bosses | Corruption management, rare creature spawns, environment mastery |

### 2.2 Anatomy Slot System

The player body has **8 graft slots**. Each can hold one creature part (kill) or remain human (open for DNA suit bonus):

```
        [HEAD]          - Sensory (vision, hearing, detection)
    [LEFT ARM] [RIGHT ARM]  - Combat (attack type, damage, reach)
        [TORSO]         - Defense (HP, armor, resistances)
    [LEFT LEG] [RIGHT LEG]  - Mobility (speed, jump, dodge distance)
        [BACK]          - Utility (wings, shell, extra limbs)
        [TAIL]          - Balance/special (new attack, passive ability)
```

**Graft Rules**:
- Each slot accepts parts from specific creature categories (you cannot put a frog leg on your head)
- Grafting is done at the ship using a **Grafting Station** (equivalent to the existing PowerItem.Eat() but with a dedicated UI flow)
- Grafts are permanent within a run. Removing a graft requires a rare consumable or costs max HP permanently
- Each graft visually changes the player model (meshes swap on the first-person arm rig and third-person model)

**DNA Suit Slots**: The suit has **6 module slots**. DNA modules do NOT conflict with graft slots -- they occupy different systems. However:
- Each DNA module costs **Suit Energy** to keep active
- Suit energy is a finite pool (upgradeable) -- you cannot run every ability at once
- Modules can be swapped freely at the ship

### 2.3 Hybrid Builds and Tradeoffs

The tension between grafts and DNA creates a spectrum of viable builds:

**Pure Graft ("The Monster")**:
- All 8 slots grafted
- Maximum raw power
- No suit energy needed (suit is vestigial)
- High corruption risk (see Section 3)
- Locked into one playstyle

**Pure DNA ("The Operator")**:
- No grafts, all 6 DNA modules active
- Maximum flexibility; swap loadouts between sorties
- Lower raw power; dependent on suit energy
- Zero corruption
- Can adapt to any encounter

**Hybrid ("The Chimera")**:
- Mix of grafts and DNA modules
- Most interesting decision space
- Example: Graft wolf legs (permanent sprint speed) + DNA module for frog wall-climb (swappable) + graft mantis right arm (permanent melee damage)
- Must balance corruption from grafts against suit energy for DNA
- The "intended" optimal path for experienced players

### 2.4 Archetypes

Players naturally gravitate toward one of these build identities:

| Archetype | Key Grafts | Key DNA | Playstyle |
|-----------|-----------|---------|-----------|
| **Juggernaut** | Rock torso, beetle back, heavy legs | Regen DNA, tremor-sense DNA | Walk into fights, absorb damage, hit hard |
| **Striker** | Mantis arms, wolf legs | Speed burst DNA, crit DNA | Fast, dodge-heavy, burst damage, paper-thin |
| **Stalker** | Chameleon head, spider back | Silent-step DNA, lure DNA | Stealth approach, one-hit kills, avoid all direct combat |
| **Ranger** | Spitter head, hawk tail | Glide DNA, toxin DNA | Keep distance, poison/acid ranged attacks, fly over terrain |
| **Chimera** | Mixed across all slots | Situational swaps | Jack of all trades, adapts per encounter |

### 2.5 Soft Gating

Biomes do NOT have level requirements. Instead, they gate through **capability requirements** and **creature difficulty**:

**Terrain Gates** (extend existing terrain height/slope system):
- **Cliff biome**: Requires enhanced jump (graft or DNA) to reach upper plateaus. Height > 60 in ChunkManager terms
- **Deep water biome**: Requires amphibian DNA or gill graft to survive submersion. Below waterLevel
- **Crystal caves**: Requires light source (bioluminescent part) or sonar (bat head DNA) to navigate. Underground, accessed through specific terrain features
- **Volcanic rim**: Requires heat resistance (rock creature torso graft or thermal suit DNA). Highest elevation spawns

**Creature Difficulty Gates**:
- Biome-specific creatures are harder. Cliff Stalkers hit harder and faster than lowland wolves
- Boss creatures guard biome transitions (extending the existing RockBoss pattern)
- Some creatures flee unless the player has specific parts (herbivores spook at predator grafts; predators challenge players with prey grafts)

**Resource Gates**:
- Advanced crafting requires materials from multiple biomes
- Net upgrades (needed for tougher captures) require components from at least 2 biomes
- Ship repairs (main objective) require parts from 4+ biomes

---

## 3. Risk/Reward Mechanics

### 3.1 Death Consequences

The current system (`PlayerHealth.Die()`) teleports the player to respawn and heals them. This needs stakes:

**On Death**:
1. **DNA Samples Drop**: Any unharvested DNA samples in inventory are lost. Installed DNA modules are safe
2. **Graft Stress**: Each graft takes 1 point of stress damage. At 3 stress, the graft degrades (reduced stats). At 5, it breaks and must be re-grafted
3. **Location Reset**: Player respawns at ship. Must trek back to where they died
4. **Corpse Run (optional)**: A portion of dropped materials remains at the death location for a limited time, marked on the HUD. Adds "one more try" tension

**What is NOT lost on death**:
- Installed grafts (damaged but not removed)
- Installed DNA modules
- Discovered codex entries
- Ship upgrade progress

This creates a risk curve: early game deaths are cheap (you have little to lose). Late game deaths are expensive (stressed grafts, lost rare samples). This naturally teaches players to respect the difficulty curve without punishing new players.

### 3.2 Corruption System

Grafting alien anatomy onto a human body is not without consequence.

**Corruption Meter**: 0-100. Each graft adds corruption based on the creature's tier:

| Creature Tier | Corruption per Graft | Example |
|--------------|---------------------|---------|
| Common (T1) | 5-8 | Bug leg, frog tongue |
| Uncommon (T2) | 10-15 | Wolf arm, beetle shell |
| Rare (T3) | 18-25 | Crystal mantis blade, deep worm hide |
| Boss (T4) | 30-40 | Rock Boss arm, Hive Queen carapace |

**Corruption Effects**:

| Corruption Level | Effect | Gameplay Impact |
|-----------------|--------|-----------------|
| 0-20 | **Stable** | No effect. Cosmetic vein patterns appear on skin |
| 21-40 | **Unstable** | Occasional screen distortion. -5% suit energy max. Predators are less aggressive toward you |
| 41-60 | **Volatile** | Periodic involuntary mutations (random stat spikes/dips for 10s). -15% suit energy max. Some creatures recognize you as kin |
| 61-80 | **Feral** | Villagers refuse to trade. Passive creatures flee on sight. Combat damage +20%. Max HP -10%. Uncontrollable rage attacks during dodge (dodge deals damage but no i-frames) |
| 81-100 | **Apex Predator** | All creatures are hostile. No suit DNA can be used. Grafts are at maximum power (+40% stats). Permanent night-vision. The world treats you as the final boss |

**Corruption Management**:
- Corruption decreases slowly over time if you have fewer than 3 grafts (-1/hour)
- The ship has a **Purification Chamber** that reduces corruption by 10 per use (limited uses, requires rare materials to recharge)
- Certain DNA modules can slow or pause corruption gain
- Some players will WANT high corruption for the power boost -- this is intentional

### 3.3 Rare Creature Encounters

The ChunkManager spawn table system already supports weighted spawning. Rare creatures layer on top:

**Apex Variants**: Any creature has a small chance (1-3%) to spawn as an Apex variant. Visual tells: larger size, different coloration, particle effects. Apex creatures:
- Have 3x HP and deal 2x damage
- Drop a guaranteed **Apex Part** (superior stats to normal parts of the same type)
- Cannot be incapacitated with a standard net (requires upgraded net)
- Have 1-2 additional attacks in their behavior tree

**Migratory Creatures**: Some creatures do not spawn from tables. They traverse the world on set paths (or semi-random routes between biomes). The player must track them by following environmental clues (tracks, droppings, damaged terrain). Killing/capturing a migratory creature yields parts/DNA not available from any stationary spawn.

**Night Spawns**: Certain creatures only appear at night. The day/night cycle (a future system) would gate access to nocturnal creatures. Night creatures tend to be more dangerous but yield parts related to sensory abilities (night vision, echolocation, silent movement).

### 3.4 Environmental Hazards

Hazards test specific builds and create pressure to diversify:

| Hazard | Biome | What It Tests | Counter |
|--------|-------|--------------|---------|
| **Acid pools** | Swamp | Mobility; can you jump over or fly? | Frog legs, wing graft, amphibian DNA |
| **Thermal vents** | Volcanic | Heat resistance | Rock creature torso, thermal DNA |
| **Crystal resonance** | Caves | Sonar navigation; otherwise take damage from bumping | Bat head DNA, crystal creature graft |
| **High winds** | Cliff peaks | Weight/grip; light builds get blown off | Heavy graft (rock legs), grip DNA |
| **Deep water pressure** | Ocean depths | Pressure resistance + oxygen | Full aquatic build required |
| **Spore clouds** | Mushroom forest | Poison filter | Filter head graft, antitoxin DNA |

---

## 4. Emergent Mechanics & Combos

### 4.1 Synergy System

When parts from specific creature families are combined, they create **Synergies** -- bonus effects beyond the sum of their parts.

**Movement Synergies**:

| Combo | Parts | Effect | Why It's Fun |
|-------|-------|--------|-------------|
| **Apex Sprinter** | Wolf legs + wolf tail | Sprint speed +50%, sprint no longer costs stamina | You feel like a predator chasing prey |
| **Sky Dancer** | Frog legs + hawk back (wings) | Double jump + short glide; hold jump to extend airtime | Completely changes traversal; reach any point on the map |
| **Tremor Dash** | Rock legs + beetle torso | Dodge roll creates a shockwave that staggers nearby enemies | Defensive move becomes offensive |
| **Wall Runner** | Spider legs + gecko back | Run along vertical surfaces for 3 seconds | Opens entirely new routes through cliffs and caves |
| **Deep Diver** | Amphibian legs + fish tail | Full swim mobility + underwater breathing | Opens an entire new gameplay layer (underwater biome) |

**Combat Synergies**:

| Combo | Parts | Effect | Why It's Fun |
|-------|-------|--------|-------------|
| **Mantis Style** | Mantis right arm + mantis left arm | Attack speed doubled, attacks have 15% bleed chance | Glass cannon; feels like a fighting game character |
| **Living Fortress** | Rock torso + beetle back + rock arms | Damage taken -40%, counter-attack on block (auto-swipe when hit) | Stand your ground against anything, including bosses |
| **Venomous** | Spider arms + snake head | Attacks apply stacking poison (3% max HP/sec, stacks 5x) | Patient, tactical combat; whittle down bosses |
| **Pack Tactics** | Wolf head (howl) + any wolf part | Nearby common creatures become temporarily allied (30s) | Turn the ecosystem against your target |
| **Crystal Cannon** | Crystal arm + crystal torso | Charged attack fires a crystal shard (ranged) that shatters for AOE | Ranged build from a melee system |

**Environmental Synergies**:

| Combo | Parts | Effect | Why It's Fun |
|-------|-------|--------|-------------|
| **Bioluminescence** | Deep fish head + crystal back | Permanent light aura; reveals hidden creatures and cave paths | Exploration tool that makes caves viable |
| **Thermal Vision** | Snake head + any heat-resistant part | See creature heat signatures through walls/terrain | Hunting tool; never get ambushed |
| **Seismic Sense** | Rock legs + worm torso | Feel creature footsteps as HUD pulses; range 50m | Works in any biome; makes you the ultimate tracker |
| **Camouflage** | Chameleon head + chameleon back | Standing still for 2s makes you invisible; move to break | Stealth approach for assassination builds |

### 4.2 DNA Module Interactions

DNA modules can also synergize, but with a twist: **DNA synergies cost extra suit energy**, so the player must make hard choices about which synergies to run.

| Module Combo | Effect | Energy Cost |
|-------------|--------|-------------|
| Speed Burst + Frog Jump | Launch: massive forward leap (3x dodge distance) | 2 module slots worth of energy |
| Regen + Thick Skin | Passive: heal 1 HP/sec while not in combat | Constant drain; limits other active modules |
| Echolocation + Tremor Sense | Map overlay: shows all creatures within 100m as pings | Constant drain; no energy for combat modules |
| Toxin Coat + Spike Skin | Attackers take poison + physical damage when they hit you | Only active during combat; moderate drain |

### 4.3 Build-Defining Combinations

Some combos are so powerful they fundamentally change how the game plays. These are the "build-around" discoveries that make players say "I need to try this":

**The Monarch** (Hive Queen head + Hive drone back + any insect arms):
Your attacks spawn temporary drone allies (tiny flying creatures that harass your target). At 3 insect parts, drones last longer and deal more damage. You become a summoner in what is otherwise a melee action game.

**The Leviathan** (Full aquatic build: fish legs, amphibian torso, eel arms, shark head):
You are nearly unstoppable in water. You move 3x speed underwater, can breathe indefinitely, and your attacks create pressure waves. On land you are slow and vulnerable. This build makes the underwater biome your home territory and land a hostile environment -- a complete inversion of the default game.

**The Ghost** (Chameleon head + spider back + moth wings + snake tail):
Permanent stealth while crouching. Silent movement. Your attacks from stealth deal 4x damage but you have 40% less HP. You play the game as a stealth-action title where combat is about setup and one-hit kills, not sustained fighting.

**The Berserker** (80+ corruption + wolf parts + rock torso):
Maximum corruption turns you feral. Your dodge has no i-frames but deals damage. Your attacks are faster and stronger. You cannot use the ship's purification chamber (too far gone). Every creature attacks you on sight. You ARE the boss encounter. The game becomes pure combat at all times.

---

## 5. World Objectives & Endgame

### 5.1 Primary Objective: Escape the Planet

The player's ship is damaged. To repair it and leave, they need **5 Core Components** scattered across the world's biomes. Each component is guarded by a **Biome Boss** (extending the existing RockBoss pattern to multiple bosses).

**Ship Repair Sequence**:

| Component | Biome | Boss | What It Unlocks on Ship |
|-----------|-------|------|------------------------|
| **Navigation Crystal** | Crystal Caves | Crystal Wyrm | Star map (shows biome locations on HUD) |
| **Thermal Core** | Volcanic Rim | Magma Titan | Purification Chamber (corruption management) |
| **Bio-Processor** | Swamp/Fungal Forest | Spore Mother | Advanced DNA Synthesis (tier 2 DNA modules) |
| **Gravity Coil** | Cliff Peaks | Storm Raptor | Gravity Dampener (fall damage immunity near ship) |
| **Warp Cell** | Deep Ocean | Abyssal Leviathan | Ship launch capability (endgame trigger) |

The player can tackle these in any order. Each boss requires different build strategies, encouraging the player to diversify their abilities between attempts.

**The Escape Choice**: When all 5 components are installed, the player can launch the ship. But the game presents a final choice:
- **Leave as Human**: Remove all grafts (lose them permanently) and launch. Ending focuses on what you learned and who you were
- **Leave as Hybrid**: Keep your grafts and launch. Ending focuses on what you have become and what that means for humanity
- **Stay**: Do not launch. Remain on the planet as the apex predator. Unlocks endgame mode

### 5.2 Boss Encounters as Progression Milestones

Each biome boss follows the existing behavior tree pattern (PrioritySelector root, condition-gated attacks, movement nodes) but tests different player capabilities:

**Crystal Wyrm** (Caves):
- Arena is dark; bioluminescent parts help, but the wyrm attacks from shadows
- Phase 1: Charges through tunnels (dodge timing test)
- Phase 2: Shatters crystal pillars for AOE (positioning test; pillars are cover)
- Phase 3: Burroughs underground, emerging from random points (tremor-sense or sonar helps)
- **Part reward**: Crystal Wyrm Fang (arm graft) -- attacks shatter armor, ignoring 50% defense

**Magma Titan** (Volcanic):
- Arena has rising lava that shrinks the fighting area over time
- Phase 1: Slow melee (like RockBoss) but leaves fire trails
- Phase 2: Eruption attack (must be behind cover)
- Phase 3: Molten armor breaks off, becomes faster but takes more damage
- **Part reward**: Molten Core (torso graft) -- fire immunity, burning aura damages nearby enemies

**Spore Mother** (Fungal):
- Arena filled with spore clouds that damage and confuse (screen effects)
- Phase 1: Spawns mycelium minions (swarm fight)
- Phase 2: Envelops in spore cloud, player must find and hit weak points using sound/tremor sense
- Phase 3: Roots player in place periodically; must break free before slam attack
- **Part reward**: Mycelium Network (back graft) -- health regen from dealing damage (lifesteal)

**Storm Raptor** (Cliff Peaks):
- Arena on a cliff; high winds push player toward edges
- Phase 1: Dive bombs (similar to JumpOnPlayerStrategy but faster, from above)
- Phase 2: Creates tornados that pull player; must use weight/grip to resist
- Phase 3: Grabs player and drops from height; must have fall mitigation
- **Part reward**: Storm Wings (back graft) -- true flight for 5 seconds on cooldown

**Abyssal Leviathan** (Deep Ocean):
- Arena is underwater; requires full aquatic build OR special breathing apparatus from ship
- Phase 1: Tentacle swipes from darkness (reaction time test)
- Phase 2: Ink cloud blinds player; echolocation required
- Phase 3: Swallowed; fight from inside (cramped melee, timed escape)
- **Part reward**: Abyssal Eye (head graft) -- see through all obscuration, reveals invisible creatures, X-ray vision through thin walls

### 5.3 Endgame Content

After the primary objective is complete (or if the player chooses to stay), the endgame opens:

**Apex Hunts**: Ultra-rare creature variants spawn at random across all biomes. These are the hardest fights in the game and drop "Apex" tier parts (the best possible stats in each slot). There are 8 Apex creatures, one for each graft slot. Completing the full set is the endgame grind.

**Corruption Gauntlet**: At 100 corruption, a unique questline opens. The planet's ecosystem recognizes you as the new apex predator and sends waves of creatures to challenge you. Surviving all waves unlocks the "Apex Predator" ending and a unique cosmetic transformation.

**Ship Log Reconstruction**: Scattered data fragments across the world tell the story of previous crash survivors. Finding all fragments reveals the planet's true nature and unlocks a hidden sixth boss encounter.

**Build Optimization**: The codex tracks your "best time" for each boss, encouraging replays with optimized builds. Leaderboards (future) track fastest boss kills by build archetype.

### 5.4 Discovery / Codex System

The codex is the player's journal and progression tracker. It extends the existing scanner mechanic (`ItemScan`):

**Creature Codex**:
- Every creature scanned adds an entry
- Entry starts with silhouette and basic stats
- Killing fills in: anatomy details, part stats, graft location
- Incapacitating fills in: DNA modules available, behavior patterns, weaknesses
- Doing both completes the entry and reveals **hidden synergies** involving that creature's parts

**World Codex**:
- Biome discoveries
- Environmental hazard locations
- Resource node locations
- POI markers (cave entrances, boss arenas, village locations)

**Build Codex**:
- Tracks all synergy combos the player has discovered
- Shows undiscovered synergies as "???" with a hint about what parts are involved
- Tracks total creatures killed, captured, and scanned

---

## 6. Multiplayer Potential (Future)

### 6.1 Co-op (2-4 players)

**Shared World, Independent Builds**: Each player has their own graft/DNA state. The world is shared, creatures are shared, but harvesting is individual (each player gets their own drop from a kill/capture).

**Role Specialization**: The archetype system naturally creates co-op roles:
- Juggernaut tanks aggro
- Striker deals burst damage
- Stalker scouts and marks targets
- Ranger provides cover fire and crowd control

**Combo Attacks**: Two players can trigger special attacks if they have complementary parts:
- Wolf howl (head graft) + Pack Tactics = both players' damage boosted for 10s
- Crystal cannon (arm graft) fires through a player with prism back graft = split-shot AOE

**Shared Corruption**: When co-op players are near each other with high corruption, their mutations sync temporarily. Both get a random shared buff/debuff. This creates moments of wild power spikes.

### 6.2 PvP

**The Hunt**: Asymmetric PvP mode. One player is the "Apex" (full build, high power) and 3 players are "Castaways" (starting builds). The Apex must hunt the Castaways before they can build up enough power to fight back. The Castaways win by killing the Apex or escaping via ship.

**Arena**: Symmetric PvP in enclosed arenas. Players enter with their current build. This tests build optimization and combat skill. Parts are not lost on PvP death.

**Territorial**: Persistent servers where players claim biomes. Holding a biome gives exclusive access to its spawn tables. Other players must fight the owner (and any creature allies from Pack Tactics) to access those creatures.

### 6.3 Trading

**Part Barter**: Players can trade harvested parts at the ship. DNA samples can be duplicated (at a cost) and traded. Grafted parts CANNOT be traded (they are part of your body).

**The Market**: A neutral zone (the Villager settlement from the existing Villager system) serves as a trading hub. Villagers also sell rare materials for creature parts, creating a barter economy.

---

## 7. Novel Ideas

### 7.1 Procedural Anatomy

**The creatures themselves are procedurally generated.**

Instead of a fixed set of creatures, the game generates creature species by combining body templates with trait sets. The existing creature prefabs (Wolf, Bug, JumpingAlien, Frog) become **base templates**. The procedural system modifies them:

- **Body plan**: Quadruped, biped, insectoid, serpentine, avian
- **Size**: 0.5x to 3x base size (affects HP, damage, part quality)
- **Traits**: 2-4 traits from a pool (venomous, armored, burrowing, pack, solitary, nocturnal, amphibious, flying, bioluminescent, magnetic, electric, thermal, acoustic)
- **Color/pattern**: Determined by biome (desert creatures are sandy, crystal creatures shimmer)

**What this means for gameplay**: Every player's world has a unique creature roster. "I found a bioluminescent armored biped in the caves" means something different in every playthrough. Parts harvested from procedurally generated creatures have stats influenced by the creature's traits, so two "wolf leg" grafts from different wolves might have different stat profiles.

**Why this is novel**: Monster Hunter has fixed monsters. Spore has procedural creatures but no combat depth. Off-World combines deep combat with procedural ecology, meaning guides and wikis cannot solve the game for you -- you must learn YOUR world's creatures.

### 7.2 The Echo System

**Your kills haunt you.**

Every creature you kill leaves an "Echo" -- a ghostly afterimage that appears in the world at the location where you killed it. Echoes are visible only to you. They are passive at first, but as corruption increases, Echoes become hostile.

At high corruption (60+), Echoes of creatures you killed will attack you. They have the same behavior tree as the original creature but are translucent and cannot be killed -- only driven off temporarily. The more creatures you have killed, the more Echoes haunt you.

**Gameplay impact**: Kill-heavy players face an escalating secondary threat. The world remembers your violence. This creates a real cost to the "kill everything" approach that is more interesting than a simple morality meter.

**Counterplay**: Echoes do not appear near the ship (your safe zone). Certain DNA modules (psychic dampener from brain-type creatures) can suppress Echoes temporarily. The Purification Chamber reduces Echo intensity alongside corruption. Completing a creature's full codex entry (both kill and capture) lays that creature's Echo to rest permanently.

### 7.3 Biological Crafting

**Combine parts at the molecular level to create new ones.**

Instead of only using parts as-is, the player can break down harvested parts into **Bio-Components** (muscle fiber, chitin plates, neural tissue, venom glands, etc.) and recombine them at the ship's Bio-Lab (unlocked after the Bio-Processor ship component).

**Examples**:
- Wolf muscle fiber + frog neural tissue = **Reflex Enhancer** (suit module, reduces dodge cooldown by 30%)
- Crystal chitin + beetle chitin = **Prismatic Armor** (torso graft, reflects 10% of damage as light-based AOE)
- Venom glands (snake) + spore sacs (fungal) = **Neurotoxin Cloud** (throwable item, confuses creatures in an area)

**Why this matters**: It creates a reason to hunt creatures you do not need parts from directly. Every creature becomes a potential ingredient. It also solves the "I already have a better arm graft" problem -- surplus parts become crafting materials, never useless.

### 7.4 Living Architecture

**Build structures from creature remains.**

Using the bones, chitin, and organic materials from kills, the player can construct:
- **Bone Fences**: Block creature pathing; create safe zones outside the ship
- **Chitin Shelters**: Temporary save points; reduce corruption while resting inside
- **Nerve Wire Traps**: Stun creatures that walk through them (assist in incapacitation)
- **Organ Beacons**: Attract specific creature types to a location (farm specific parts)
- **Muscle Bridges**: Span gaps between cliffs; living, pulsing bridges that grow over time

These structures are alive. They pulse, grow, and decay. A bone fence left unattended will sprout new growth, eventually becoming a wall of alien vegetation. A chitin shelter, if fed creature remains, will grow into a secondary base with limited graft/DNA station capabilities.

**Why this is novel**: Combining Subnautica's base-building with biological horror aesthetics. The player is not building with metal and glass -- they are growing a living base from the creatures they have killed. It reinforces the transformation theme: you are not just changing yourself, you are changing the world.

### 7.5 The Metamorphosis Event

**At 50 corruption, the player enters a cocoon.**

This is a one-time, irreversible event that occurs the first time the player reaches 50 corruption. The screen fades. The player is encased in a biological cocoon for a real-time duration (2-3 minutes). During this time, they see visions of the planet's history and the alien ecology.

When they emerge, their grafts have **fused**. Instead of 8 separate slots, they now have a unified hybrid form. The specific form depends on which grafts they had when the cocoon triggered:
- Majority predator parts = **Hunter Form** (streamlined, fast, all attacks enhanced)
- Majority defensive parts = **Fortress Form** (bulky, slow, near-invulnerable)
- Majority sensory parts = **Seeker Form** (slight, fragile, can sense everything in a 200m radius)
- Mixed = **Chimera Form** (unstable, randomly shifts between stat profiles every 60 seconds)

**Why this is novel**: It is a permanent transformation based on choices the player made throughout the game up to that point, without them knowing exactly when it would trigger. The cocoon is the game saying "you have changed enough that you are no longer human." It is a point of no return that recontextualizes the entire game.

**Mechanical impact**: Post-metamorphosis, the player cannot add or remove individual grafts. Their form is set. DNA modules still work. New grafts can be consumed to "evolve" the form (small stat boosts) but the shape is permanent. This is the "New Game Plus" hook: play again and reach metamorphosis with a different set of grafts to unlock a different form.

---

## 8. Implementation Mapping

How each proposed system maps to existing code:

### Immediate (builds directly on existing systems)

| Feature | Existing System | Extension Needed |
|---------|----------------|-----------------|
| **DNA extraction from captured creatures** | `EnemyGrabbable.Capture()` + `ItemScan.ScanItem()` | Add DNA drop table to `ItemScannable`; create `DNAModule` ScriptableObject |
| **Graft application** | `PowerItemI.Eat()` modifies `PlayerMovement` stats | Create `GraftSlot` component on player; `GraftItem` replaces `PowerItemI` with slot targeting and permanent effect |
| **Corruption meter** | `PlayerHealth` already tracks `health` and `maxHealth` | Add `corruption` field to `PlayerHealth`; graft application calls `AddCorruption(int)` |
| **Creature tier spawning** | `ChunkManager.SpawnTable` with height/distance filters | Add `creatureTier` field to `SpawnEntry`; tier determines part quality on harvest |
| **Incapacitate threshold** | `EnemyHealth.HurtEnemy()` checks `health <= 0` | Add `incapacitateThreshold` (e.g., 20% max HP); when health drops below, trigger downed state instead of Die() |
| **Codex entries from scanning** | `ItemScan` + `ItemScannable` | Create `Codex` singleton with Dictionary of creature IDs to discovery state |

### Medium-term (new systems, moderate scope)

| Feature | Dependencies | Architecture Notes |
|---------|-------------|-------------------|
| **Anatomy slot system** | GraftItem ScriptableObjects, player model mesh swaps | `AnatomyManager` component on player; 8 `GraftSlot` instances; each slot references a `GraftData` ScriptableObject |
| **DNA module loadout** | DNAModule ScriptableObjects, suit energy system | `SuitManager` component on player; 6 `ModuleSlot` instances; `suitEnergy` float with drain rates per active module |
| **Synergy detection** | AnatomyManager, SuitManager | `SynergyDatabase` ScriptableObject listing all synergy rules; `SynergyChecker` evaluates on graft/module change |
| **Corruption effects** | PlayerHealth corruption field, PlayerMovement, AttackController | `CorruptionEffects` component reads corruption level and applies modifiers to movement/combat/AI perception |
| **Biome bosses** | Existing `CharacterInteraction` BT pattern, `EnemyHealth` | One `BossController` per boss inheriting the BT structure; new `IStrategy` implementations for unique attacks |
| **Ship repair progression** | New ShipManager component | `ShipManager` tracks 5 component booleans; unlocks ship features (purifier, bio-lab, etc.) per component installed |

### Long-term (complex systems, post-MVP)

| Feature | Scope | Notes |
|---------|-------|-------|
| **Procedural anatomy** | Large | Creature generator combining body templates + trait sets; modifies prefab stats at spawn time via `ChunkManager` |
| **Echo system** | Medium-Large | `EchoManager` tracks kill locations; spawns ghost prefab variants at those positions; behavior scales with corruption |
| **Biological crafting** | Medium | `BioLab` UI + `BioComponent` ScriptableObjects; recipe system combining components into new items |
| **Living architecture** | Large | Buildable prefab system with growth/decay simulation; structural integrity from organic materials |
| **Metamorphosis event** | Medium | One-time event trigger at corruption 50; form determined by graft majority; creates unified `MetamorphForm` replacing slot system |
| **Multiplayer** | Very Large | Networking layer, shared world state, sync for creature spawns/despawns, PvP balance pass |

---

## Appendix A: Creature Roster (Initial)

Mapped to existing prefabs and proposed new creatures across biomes:

| Creature | Biome | Tier | Existing Prefab | Key Part | Key DNA |
|----------|-------|------|----------------|----------|---------|
| **Skitterbug** | Lowland | T1 | Bug.prefab | Chitin plates (back) | Hive-mind sense |
| **Thornfrog** | Lowland/Swamp | T1 | JumpingAlien.prefab | Spring legs (legs) | Wall-climb |
| **Direwolf** | Grassland | T2 | Wolf.prefab | Fangs (arms), Pack howl (head) | Sprint burst |
| **Rock Golem** | Cliffs | T3 (Boss) | RockBossAnims.prefab | Stone arms, Stone torso | Tremor-sense |
| Cliff Stalker | Cliffs | T2 | New | Grip claws (arms) | Glide |
| Crystal Mantis | Caves | T2 | New | Crystal blades (arms) | Refraction (light bending) |
| Spore Crawler | Fungal | T2 | New | Mycelium tendrils (back) | Spore cloud |
| Thermal Beetle | Volcanic | T2 | New | Heat carapace (torso) | Heat resistance |
| Abyssal Eel | Ocean | T2 | New | Electric coils (arms) | Bioelectric pulse |
| Shadow Moth | Night-only | T2 | New | Dust wings (back) | Silent movement |
| Crystal Wyrm | Caves | T4 (Boss) | New | Crystal Fang (arm) | Resonance sight |
| Magma Titan | Volcanic | T4 (Boss) | New | Molten Core (torso) | Lava walk |
| Spore Mother | Fungal | T4 (Boss) | New | Mycelium Net (back) | Lifesteal aura |
| Storm Raptor | Peaks | T4 (Boss) | New | Storm Wings (back) | Updraft ride |
| Abyssal Leviathan | Ocean | T4 (Boss) | New | Abyssal Eye (head) | Pressure immunity |

## Appendix B: Stat Modification Reference

How grafts and DNA modules modify existing code values:

```
PlayerMovement.walkSpeed      <- leg grafts, speed DNA
PlayerMovement.sprintSpeed    <- leg grafts, speed DNA
PlayerMovement.jumpForce      <- leg grafts (frog), jump DNA
PlayerMovement.DodgeForce     <- tail grafts, dodge DNA
PlayerMovement.DodgeDuration  <- tail grafts
PlayerHealth.maxHealth         <- torso grafts, vitality DNA
PlayerHealth.immunityDuration  <- back grafts (shell), resilience DNA
AttackController (damage)      <- arm grafts, combat DNA
   [new] attackDamage          <- base + graft modifier + DNA modifier
   [new] attackSpeed           <- base + graft modifier + DNA modifier
   [new] attackRange           <- base + graft modifier
```

All modifiers are additive within a category (grafts stack with grafts) and multiplicative between categories (graft bonus * DNA bonus * base). This prevents any single source from being dominant while rewarding investment in both systems.

## Appendix C: Design Principles Summary

1. **No dead choices**: Every creature encounter, every part harvest, and every build decision should feel like it matters. If a part is objectively worse than another in the same slot with no situational upside, it needs a rework.

2. **Tension over punishment**: The game should create tension (will I survive this fight? Can I make it back to the ship?) not frustration (I lost 4 hours of progress). Death costs should be recoverable within 15-30 minutes.

3. **Discovery over prescription**: The synergy and codex systems reward exploration and experimentation. The game should never tell the player "equip these parts." It should say "something interesting happens when..."

4. **Transformation is the story**: The player's body IS the narrative. A player with 7 grafts and 80 corruption has told a story through gameplay without a single cutscene. The game's systems should always reinforce this theme.

5. **Procedural means personal**: Every player's world, creature roster, and build path should feel like THEIRS. Shared wikis can give general advice, but the specific creatures and part combinations in each world should be unique enough that players share discoveries rather than follow guides.
