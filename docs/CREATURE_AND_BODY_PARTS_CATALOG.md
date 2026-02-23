# Off-World: Creature and Body Parts Catalog

> Comprehensive design document for all creatures, harvestable body parts, DNA profiles, ecology, and biome mappings. Covers existing creatures (Wolf, Frog, Bug, JumpingAlien, RockBoss) and 8 new creature designs.

---

## Table of Contents

1. [Creature Roster](#1-creature-roster)
   - [Existing Creatures](#existing-creatures)
   - [New Creatures](#new-creatures)
2. [Body Parts Catalog](#2-body-parts-catalog)
3. [DNA Catalog](#3-dna-catalog)
4. [Creature Ecology](#4-creature-ecology)
5. [Biome-Creature Mapping](#5-biome-creature-mapping)

---

## 1. Creature Roster

### Existing Creatures

#### Wolf (Howler)
- **Visual:** Lean, six-legged canine with bioluminescent stripe patterns along its flanks. Jaw splits into four mandibles when attacking. Dark grey hide with electric-blue markings.
- **Biome:** Grasslands, rocky cliffs
- **Size class:** Medium
- **Behavior archetype:** Pack hunter (aggressive)
- **Combat style:** Melee rush, coordinated flanking
- **Unique mechanic:** Pack coordination -- when one Wolf spots the player, nearby Wolves share the alert (already modeled via `BaseAI` LOS + chase propagation). Wolves circle to attack from multiple directions simultaneously.
- **Implementation notes:** Uses `BaseAI` with `WolfAI` override. Chase/search/idle states with position memory stack for retracing steps. Damage via `WolfAttack` collision.

#### Frog (Gulper)
- **Visual:** Bulbous, translucent-skinned amphibian the size of a large dog. Oversized hind legs with suction-cup toes. Swollen throat sac glows amber when startled.
- **Biome:** Beach/sand (near water), grasslands (near ponds)
- **Size class:** Small-Medium
- **Behavior archetype:** Passive (flees when spotted)
- **Combat style:** Evasion -- hops away from the player using impulse-based jumps
- **Unique mechanic:** Hopping flee pattern makes it hard to catch; must be cornered or netted. Drops valuable jump-enhancing parts. The `Hop()` method fires directional impulse forces.
- **Implementation notes:** Uses `FrogAI` with hop-based Rigidbody movement (no NavMesh). Runs from player on LOS detection. Idle wandering with periodic hops.

#### Bug (Chitin Swarmling)
- **Visual:** Small, iridescent beetle-like creature with four translucent wings and a hard chitinous shell. Moves in groups. Glows faintly green.
- **Biome:** All biomes (most common in grasslands)
- **Size class:** Small
- **Behavior archetype:** Swarm (passive individually, aggressive in numbers)
- **Combat style:** Swarm melee -- individually weak, dangerous in groups
- **Unique mechanic:** Swarm behavior -- individual Bugs are trivial, but groups can overwhelm. Killing one causes others to briefly scatter before regrouping. Harvesting requires killing multiple for enough material.

#### JumpingAlien (Vaulter)
- **Visual:** Tall, bipedal creature with reverse-jointed legs (digitigrade). Long arms with hooked claws for stabilization mid-jump. Mottled brown-and-orange hide.
- **Biome:** Rocky cliffs, grasslands
- **Size class:** Medium
- **Behavior archetype:** Territorial (aggressive when approached)
- **Combat style:** Hit-and-run -- leaps in, strikes, leaps away
- **Unique mechanic:** Vertical mobility -- can leap to high terrain and attack from above. Forces the player to deal with three-dimensional combat.

#### RockBoss (Lithosaur)
- **Visual:** Massive golem-like quadruped made of layered stone and crystal. Head is a cluster of mineral growths. Arms end in boulder-fists. Dormant until provoked -- appears as part of the landscape.
- **Biome:** Rocky cliffs (boss arena)
- **Size class:** Boss
- **Behavior archetype:** Territorial boss (dormant until approached)
- **Combat style:** Mixed melee/ranged/AOE -- punch combos (left/right hooks), backspin punish, ground slam AOE, rock throw projectile, jump-slam gap closer
- **Unique mechanic:** Wake-up sequence (player must approach within `proximityDistance` to trigger `STARTSHAKE` animation). Behavior tree with priority-based attack selection: punishes players attacking from behind (BACKSPIN), chooses ranged or melee based on distance, and gap-closes with JUMPSLAM or chase. Head-look system tracks player and gates attacks on facing.
- **Implementation notes:** Full behavior tree in `CharacterInteraction.cs` using `PrioritySelector`, `RandomSelector`, `GuardedSequence`. 5 attacks + 2 movement behaviors. `EnemyHealth` with healthbar UI and `GetHealthNormalized()` for future phase logic.

---

### New Creatures

#### 1. Thornback (Spineweaver)
- **Visual:** Low-slung, armadillo-like quadruped covered in rows of retractable crystalline spines. When threatened, the spines extend outward like a sea urchin, doubling the creature's apparent size. Body is rust-red with pale underbelly. Spines refract light, creating a prismatic shimmer.
- **Biome:** Grasslands, rocky cliffs
- **Size class:** Medium
- **Behavior archetype:** Defensive/territorial -- does not flee or chase. Holds ground and punishes melee attackers.
- **Combat style:** Passive-reactive melee. Curls into a spine-ball when attacked, dealing contact damage to melee attackers. Periodically shakes to launch loose spines in a short-range radial burst.
- **Unique mechanic:** **Damage reflection.** Melee attacks against an extended Thornback deal partial damage back to the attacker. The player must either wait for the spines to retract (brief vulnerability window every ~8 seconds when it grazes), use ranged attacks, or bait the spine-burst and rush in during the cooldown. Teaches the player patience and positioning.
- **AI approach:** State machine or simple BT: `Idle (grazing) -> Threatened (spines out, contact damage aura) -> Spine Burst (AOE then brief vulnerable window) -> return to Idle`. Uses `BaseAI` with custom override. No chase behavior -- stays near spawn.

#### 2. Gloomray (Driftfin)
- **Visual:** A large, flat, manta-ray-shaped creature that hovers 2-3 meters off the ground using bioluminescent gas bladders along its wingspans. Translucent skin reveals pulsing internal organs. Underside has a cluster of dangling tendrils that trail across the ground. Deep indigo body with veins of pale violet light.
- **Biome:** Grasslands (open fields), swamp (future)
- **Size class:** Large
- **Behavior archetype:** Passive roamer -- drifts slowly across the landscape. Non-aggressive until attacked or the player walks under it.
- **Combat style:** AOE debuff/area denial. Tendrils release a toxic cloud below it that slows and damages players who linger underneath. When attacked, it rises higher and emits an electric pulse that stuns nearby targets.
- **Unique mechanic:** **Living environmental hazard.** The Gloomray is not a traditional fight -- it creates a moving danger zone. Players must decide whether to avoid it, snipe it from range, or brave the toxic cloud to get underneath for a critical weak spot (exposed gas bladders on the underside). Killing it causes a satisfying explosion of bioluminescent gas.
- **AI approach:** Simple patrol path (no `BaseAI` chase needed). Floats above terrain using a fixed Y offset + terrain-following raycast. Tendril damage is a trigger zone child object. On-hit response: rise + pulse. Uses Rigidbody with custom floating logic similar to player's `ApplyFloating()`.

#### 3. Burrower (Sandmaw)
- **Visual:** Worm-like creature with a segmented, chitinous body. Front end is a ring of hooked teeth surrounding a lamprey-like mouth. No visible eyes -- navigates by vibration. Sandy beige coloring with darker segment rings. Only the front third of the body is ever visible above ground.
- **Biome:** Beach/sand (primary), grasslands (edges near sand)
- **Size class:** Medium-Large
- **Behavior archetype:** Ambush predator
- **Combat style:** Ambush melee -- erupts from the ground beneath or near the player, bites, then re-burrows. Surface movement is telegraphed by a moving dirt/sand mound.
- **Unique mechanic:** **Subterranean ambush.** The Burrower is invisible underground. The player can detect it by watching for a particle-effect "sand trail" moving toward them. Sprinting creates more vibration (attracts it faster). Standing still causes it to lose interest. When it erupts, there is a brief wind-up (ground cracks + dust burst) giving a dodge window. After the attack, it is exposed above-ground for ~3 seconds before re-burrowing.
- **AI approach:** Custom AI (not NavMesh-based while underground). Subterranean state: move toward player using vibration detection (distance + player velocity magnitude). Surface state: brief attack animation then re-burrow timer. Mesh/renderer disabled while underground; particle system tracks position.

#### 4. Sporemother (Mycelith)
- **Visual:** A immobile, tree-sized fungal organism rooted to the ground. Massive mushroom cap (3m diameter) with bioluminescent spots. Thick stalk covered in smaller parasitic growths. Releases visible clouds of luminous spores. When threatened, the cap splits open to reveal a toothy maw underneath.
- **Biome:** Grasslands (dense areas near trees), swamp (future), crystal caves (future)
- **Size class:** Large (stationary)
- **Behavior archetype:** Stationary support/area control
- **Combat style:** Support + AOE. Constantly releases spore clouds that heal nearby creatures and debuff the player (vision obscuration, slow). When the player gets close, it can snap with its hidden maw. Killing it removes the healing/buff aura from an area, making nearby creatures weaker.
- **Unique mechanic:** **Ecosystem anchor.** The Sporemother does not move but shapes the battlefield. Creatures near a Sporemother regenerate health slowly, making fights harder. The player is incentivized to prioritize Sporemothers before engaging packs. The spore cloud is a visible area-of-effect: green spores heal creatures, yellow spores debuff the player. Destroying it creates a "cleansed" zone that is safe for a time.
- **AI approach:** No movement AI. Stationary with trigger-zone children for spore cloud (heal aura for creatures with `EnemyHealth`, debuff zone for player). Maw attack uses a proximity trigger + simple animation. Health component reuses `EnemyHealth`. Could be a behavior tree with just: `[Condition: player in maw range] -> Bite` else `[Emit spores]`.

#### 5. Mimic Stalker (Glassghast)
- **Visual:** Humanoid-proportioned creature (slightly taller than the player) with a smooth, reflective surface that shifts colors to match its surroundings. In neutral state, it is nearly invisible -- a faint heat-shimmer distortion. When it attacks, its surface fractures into sharp crystalline edges and its "face" splits open to reveal rows of glass-like teeth. Pale white body with prismatic surface reflections.
- **Biome:** All biomes (rare spawn)
- **Size class:** Medium
- **Behavior archetype:** Ambush stalker -- follows the player at a distance while camouflaged, attacks when the player is vulnerable (low health, in combat with another creature, or back turned).
- **Combat style:** High-damage single strikes. Decloaks with a lunging slash, then retreats and re-cloaks. Hit-and-run assassin.
- **Unique mechanic:** **Active camouflage + opportunistic AI.** The Mimic Stalker is nearly invisible and only attacks when conditions favor it. The player might notice environmental cues: footstep sounds with no visible source, small animals fleeing from "nothing," or a faint shimmer in peripheral vision. Scanning with the scanner item reveals its outline. Once spotted, it becomes aggressive and fights directly (losing camo advantage). A terrifying encounter that rewards awareness.
- **AI approach:** Extended `BaseAI` with additional states: `Stalking (cloaked, follow at distance, check player health/combat status)`, `Ambush (decloak + lunge)`, `Exposed (aggressive melee, no cloak for X seconds)`, `Retreat (re-cloak at distance)`. Cloak is a material swap (transparent shader) or `MeshRenderer.enabled = false` with shimmer particle effect.

#### 6. Shellcrab (Tidebreaker)
- **Visual:** A massive crustacean with an asymmetric body plan. One claw is enormous (shield-sized, used for blocking), the other is a narrow spike (used for piercing attacks). Barnacle-encrusted shell with tidal pool ecosystems growing on its back. Blue-green coloring with orange joint membranes.
- **Biome:** Beach/sand (primary), rocky cliffs (coastal areas)
- **Size class:** Large
- **Behavior archetype:** Aggressive territorial -- patrols shorelines, charges anything that enters its territory
- **Combat style:** Directional melee tank. Leads with the shield-claw to block frontal attacks, then counters with the spike-claw. Can sideways-charge to knock the player down. Shield-claw blocks reduce incoming damage from that direction.
- **Unique mechanic:** **Directional defense.** The Shellcrab's shield-claw absorbs most frontal damage. The player must flank it (attacking the spike-claw side or rear) to deal full damage. The Shellcrab will try to keep its shield-claw facing the player, creating a rotation duel. Its charge attack is telegraphed by it lowering its body and clicking its claws. After a charge, it is briefly exposed from behind.
- **AI approach:** `BaseAI` derivative with custom `Chase()` that orients shield-claw toward player. BT: `[Shield-face player] -> [Player in range? -> Random: Spike thrust / Shield bash / Sideways charge] -> [Player far? -> Charge]`. Directional damage mitigation via dot-product check in a custom `TakeDamage` override.

#### 7. Voltwasp (Stormstinger)
- **Visual:** Insectoid flyer the size of a hawk. Four crackling, electrically-charged wings that leave lightning-trail afterimages. Segmented abdomen ends in a barbed stinger that arcs with electricity. Metallic gold-and-black exoskeleton. Compound eyes glow electric blue.
- **Biome:** Rocky cliffs (high altitude), grasslands (storms)
- **Size class:** Small-Medium
- **Behavior archetype:** Pack aggressor (like Wolves but aerial)
- **Combat style:** Ranged harasser -- dive-bombs with electric sting, then retreats to altitude. In groups, they coordinate strafing runs. Electric attacks can chain between nearby players/creatures.
- **Unique mechanic:** **Aerial combat + chain lightning.** Voltwasps force the player to deal with flying enemies for the first time. They are individually fragile but attack in coordinated strafing patterns. When two or more Voltwasps hit in rapid succession, their electrical damage chains (bonus damage). They are attracted to metal equipment, and during thunderstorms their damage is doubled. Can be grounded by hitting their wings.
- **AI approach:** Custom flying AI (not NavMesh). Y-axis oscillation with terrain-following at a height offset. Simple state machine: `Circle (maintain distance at altitude) -> Dive (swoop toward player, sting) -> Retreat (climb back to altitude)`. Pack coordination: if one dives, others prepare to follow within 1-2 seconds.

#### 8. Coralgor (Reefwarden) -- BOSS
- **Visual:** A towering crustacean-plant hybrid that looks like a walking coral reef. Its "torso" is a mass of living coral and anemone-like tendrils. Two massive coral-encrusted arms that it uses as hammers. Lower body is a cluster of thick root-legs that it tears from the ground to walk. Small symbiotic creatures (Shellcrabs, fish-like parasites) live on its body and occasionally detach to fight independently. Vibrant but alien coloring: turquoise coral, pink anemone tendrils, deep red root-legs.
- **Biome:** Beach/sand (boss arena -- tidal flats that flood and drain during the fight)
- **Size class:** Boss
- **Behavior archetype:** Territorial boss -- guards a coastal region. Arena has tidal mechanic.
- **Combat style:** Heavy melee + summons + environmental. Coral hammer slams, tendril sweep (mid-range), summons small Shellcrabs from its body, and triggers tidal surges that flood the arena (creating water zones that slow the player but empower the Coralgor).
- **Unique mechanic:** **Tidal phase shifts.** The arena alternates between low tide (exposed tidal flats, Coralgor is slower, player has solid ground) and high tide (ankle-deep water floods in, Coralgor moves faster, player is slowed, electric attacks from symbiotes arc through water). The player must manage positioning around tide pools and elevated rocks. Destroying coral growths on its body reduces its armor and prevents it from summoning more symbiotes.
- **AI approach:** Full behavior tree like RockBoss. `PrioritySelector`: `[Summon Symbiotes (if count < max)] -> [Tidal Surge (phase shift, cooldown-gated)] -> [Tendril Sweep (mid-range AOE)] -> [Coral Hammer Slam (melee)] -> [Chase/Reposition]`. Phase logic via health thresholds using `GetHealthNormalized()`: Phase 1 (100-60%): normal. Phase 2 (60-30%): faster tides, more summons. Phase 3 (30-0%): permanent high tide, enraged.

---

## 2. Body Parts Catalog

For each creature, all harvestable body parts with their graft slot, ability, stat modifications, and visual change to the player.

### Wolf (Howler)

| Part Name | Slot | Ability Granted | Stat Modifications | Visual Change |
|-----------|------|-----------------|-------------------|---------------|
| Howler Fangs | Head | **Predator Bite** -- melee attack that deals bonus damage and briefly heals the player (lifesteal) | +15% melee damage, +5% lifesteal on melee hits | Player's jaw distorts; four mandible-like protrusions emerge from the lower face. Bioluminescent blue lines trace along the jawline. |
| Howler Hide | Torso | **Pack Instinct** -- nearby allied creatures (tamed or summoned) deal 20% more damage | +10% damage resistance, +10% sprint speed | Torso covered in dark grey alien fur with electric-blue bioluminescent stripes. Posture becomes slightly hunched. |
| Howler Forelegs | Arms | **Lunge Strike** -- dash-forward melee attack that covers 5m instantly | +15% attack speed, +10% sprint speed | Arms elongate slightly, covered in grey hide. Hands develop claw-like nails. Forearms have blue-glowing tendons visible under thin skin. |
| Howler Haunches | Legs | **Pursuit Sprint** -- sprinting speed increases the longer the player runs (stacks up to 40% over 5 seconds) | +20% sprint speed, -5% base walk speed | Legs become digitigrade (reverse-jointed). Thighs bulk up with alien muscle. Blue markings trail down the calves. |
| Howler Tail | Tail | **Alpha Howl** -- activatable howl that frightens small creatures in a radius, causing them to flee for 5 seconds | +10% movement speed, +5% dodge distance | A segmented, bioluminescent tail extends from the player's lower back. It sways during movement and flares bright blue when howling. |

### Frog (Gulper)

| Part Name | Slot | Ability Granted | Stat Modifications | Visual Change |
|-----------|------|-----------------|-------------------|---------------|
| Gulper Eyes | Head | **Tremor Sense** -- nearby creatures are highlighted through walls/terrain within 15m (brief pulse every 3 seconds) | +15% detection range for enemies on minimap/HUD | Eyes become large, bulbous, and amber-colored with horizontal slit pupils. They protrude slightly from the skull and can move independently. |
| Gulper Throat | Torso | **Toxin Spit** -- ranged attack that launches a glob of irritant mucus, slowing the target for 3 seconds | +10% poison/toxin resistance | Throat swells into a visible vocal sac. The player's neck thickens, with translucent skin showing amber fluid underneath. Sac inflates visibly when using the ability. |
| Gulper Tongue | Arms | **Grapple Tongue** -- launch a sticky tongue up to 8m that pulls small objects/creatures toward the player or pulls the player toward heavy objects | +10% grab range, +5% attack speed | One arm develops a long, retractable tongue-like appendage that coils around the forearm when not in use. Pink-amber coloring with sticky pad at the tip. |
| Gulper Legs | Legs | **Power Leap** -- massively enhanced jump (3x normal height). Charge-up mechanic: holding jump charges a higher leap. | +150% jump height, +30% jump force, -5% ground speed | Legs become thick and muscular with visible suction-cup pads on the feet. Thighs are disproportionately large. Skin takes on a mottled green-amber tone. Crouching shows the legs coiling. |
| Gulper Skin | Back | **Moisture Barrier** -- passive water breathing and 50% reduced damage from water/acid/toxin sources. Active: secrete slippery mucus that makes the player harder to grab and reduces friction for 5 seconds. | +20% toxin resistance, +10% swim speed | Back and shoulders become covered in smooth, moist, translucent skin. Visible fluid runs beneath the surface. Skin glistens and occasionally drips. |

### Bug (Chitin Swarmling)

| Part Name | Slot | Ability Granted | Stat Modifications | Visual Change |
|-----------|------|-----------------|-------------------|---------------|
| Swarmling Antennae | Head | **Hive Sense** -- detect all creatures of the same species within 30m when near one Bug. Passively shows resource nodes and harvestable items. | +10% loot detection range | Two segmented, chitinous antennae sprout from the player's forehead. They twitch and orient toward detected targets. Faintly green-glowing tips. |
| Swarmling Carapace | Torso | **Chitin Armor** -- lightweight natural armor that regenerates over time if not hit for 5 seconds (ablative shield). | +15% armor (regenerating ablative layer, max 25 bonus HP), -5% sprint speed | Torso develops overlapping iridescent chitin plates. Green-tinted with a beetle-shell sheen. Plates visibly crack when damaged and regrow over time with a crystallization effect. |
| Swarmling Wings | Back | **Flutter Jump** -- double jump with a brief hover (1.5 seconds of slow descent). Not true flight, but significant air control. | +30% air control, double jump enabled, +10% fall damage reduction | Four translucent insectoid wings unfold from the player's shoulder blades. They buzz audibly during flutter and fold flat against the back when not in use. Iridescent green shimmer. |
| Swarmling Mandibles | Arms | **Rend and Harvest** -- melee attacks have a chance to drop extra crafting materials from killed creatures (bonus harvest). | +10% melee damage, +25% harvest yield | Forearms develop small chitinous mandible-claws alongside the hands. They click and flex during attacks. Wrists have segmented chitin guards. |
| Swarmling Legs (set of 4) | Legs | **Wall Cling** -- the player can cling to vertical surfaces for up to 3 seconds, and climb slowly. | +15% climb speed, wall cling enabled, +10% ground speed | Lower legs develop segmented chitin with hooked tarsal claws. Feet become multi-toed with grip pads. Movement produces faint clicking sounds. |

### JumpingAlien (Vaulter)

| Part Name | Slot | Ability Granted | Stat Modifications | Visual Change |
|-----------|------|-----------------|-------------------|---------------|
| Vaulter Crest | Head | **Height Advantage** -- dealing damage from above (during a jump or from higher elevation) grants 25% bonus damage. | +10% critical hit chance from above | A bony, swept-back crest grows from the back of the skull. Mottled brown-orange coloring. The crest acts as a natural stabilizer during jumps. |
| Vaulter Ribcage | Torso | **Impact Absorb** -- landing from any height deals no fall damage and instead creates a small shockwave that staggers nearby enemies. | Fall damage immunity, +10% HP | Ribs become externally visible, protruding through the skin as curved bone-plates along the torso sides. Brown-orange coloration. The ribcage flexes visibly on hard landings, absorbing impact. |
| Vaulter Claws | Arms | **Grapple Hook** -- claw-assisted rapid climb. The player can grab ledges from further away and pull themselves up instantly. | +20% climb speed, +15% melee damage, ledge-grab range +2m | Hands develop elongated, hooked claws (3 fingers + thumb). Brown keratin with orange-tipped hooks. Arms lengthen slightly. Claws retract partially when not climbing. |
| Vaulter Legs | Legs | **Vault Launch** -- a powered jump that launches the player forward in an arc (directional long-jump, ~10m horizontal). Short cooldown. | +100% jump height, +50% horizontal jump distance, +20% sprint speed | Legs become fully digitigrade with powerful reverse-jointed structure. Thighs and calves are heavily muscled. Orange-brown hide. Feet develop three-toed raptor-like structure. |
| Vaulter Tail | Tail | **Aerial Rudder** -- full air control during jumps. The player can change direction mid-air and perform a single air-dash. | +50% air control, air-dash enabled (8m horizontal burst) | A long, rigid tail with a flat fin-like end extends from the lower spine. Brown-orange with darker bands. It visibly adjusts during jumps for balance. |

### RockBoss (Lithosaur)

| Part Name | Slot | Ability Granted | Stat Modifications | Visual Change |
|-----------|------|-----------------|-------------------|---------------|
| Lithosaur Skull | Head | **Seismic Sense** -- sense all creatures and players through the ground within 25m. Works even when enemies are behind cover or underground. No visual LOS needed. | +20% detection radius, immune to blinding effects | Head becomes encased in angular mineral growths. Crystal formations protrude from the temples and brow. Eyes glow with a deep amber light from within stone sockets. |
| Lithosaur Core | Torso | **Stone Skin** -- passive massive damage reduction. Active: briefly become immovable and invulnerable for 2 seconds (long cooldown). | +40% armor, +20% max HP, -15% movement speed | Torso becomes covered in layered stone plates with crystal veins. The player's silhouette widens. Crystals pulse faintly with each heartbeat. Cracks glow orange when damaged. |
| Lithosaur Fists | Arms | **Boulder Slam** -- charged melee attack that creates a small AOE shockwave on impact. Full charge sends a ground-traveling fissure forward. | +30% melee damage, +25% stagger chance, -10% attack speed | Arms bulk up enormously, covered in stone plating. Fists become boulder-like with crystal knuckles. Forearms have visible mineral striations. Arms hang heavier, slightly altering idle pose. |
| Lithosaur Legs | Legs | **Quake Step** -- sprinting creates minor tremors that slow nearby ground-level enemies. Stomp attack: a short-range AOE that damages and staggers. | +25% armor (legs), +15% stagger resistance, -20% jump height, -10% speed | Legs become thick stone pillars with crystal-veined joints. Feet are flat, wide, and heavy. Each step creates visible dust impact. Footsteps echo with bass. |
| Lithosaur Spine | Back | **Mineral Regeneration** -- slowly regenerate HP over time while standing still (2 HP/sec). Regeneration pauses for 5 seconds after taking damage. | +10 HP regen/sec (while stationary), +15% max HP | A ridge of interlocking mineral plates and crystal spurs runs down the player's spine. Small crystal formations grow off the shoulders. The crystals glow brighter as health regenerates. |

### Thornback (Spineweaver)

| Part Name | Slot | Ability Granted | Stat Modifications | Visual Change |
|-----------|------|-----------------|-------------------|---------------|
| Spineweaver Crown | Head | **Threat Display** -- enemies within 5m have their attack speed reduced by 15% (intimidation aura). | +10% nearby enemy attack speed reduction | A ring of short, crystalline spines grows around the crown of the head like a thorny halo. Spines refract light with a prismatic shimmer. They extend slightly when enemies are near. |
| Spineweaver Shell | Torso | **Spine Ward** -- passive: melee attackers take 20% of their own damage reflected back. Active: extend all spines for 3 seconds, reflecting 50% of melee damage and preventing grabs. | +25% armor, 20% melee damage reflection (passive) | Torso covered in rows of retractable crystalline spines over a rust-red carapace. Spines lie flat normally but visibly extend during the active ability. Underside remains pale and exposed. |
| Spineweaver Quills | Arms | **Spine Shot** -- launch a volley of 3 crystalline quills (ranged attack, 15m range, moderate damage). 5-second cooldown. | +10% ranged damage, ranged attack enabled | Forearms develop dense clusters of crystalline quills that regenerate over time. When fired, quills visibly detach with a crystal-shatter sound. Arms have a rust-red carapace layer. |
| Spineweaver Pads | Legs | **Anchor Stance** -- while crouching, the player is immune to knockback and staggers. Roots to the ground. | +15% stagger resistance, knockback immunity while crouching | Lower legs develop wide, flat footpads with gripping barbs. Legs take on the rust-red armored look. When anchoring, small crystalline roots visibly extend into the ground. |
| Spineweaver Tail | Tail | **Spine Burst** -- release a radial burst of spines (360-degree short-range AOE, 3m radius). Knocks back enemies and deals moderate damage. 10-second cooldown. | +10% armor, spine burst AOE enabled | A short, thick tail covered in densely packed crystal spines. The tail curls slightly upward. During burst, it whips in a circle and spines visibly fly outward, regrowing seconds later. |

### Gloomray (Driftfin)

| Part Name | Slot | Ability Granted | Stat Modifications | Visual Change |
|-----------|------|-----------------|-------------------|---------------|
| Driftfin Sensory Lobe | Head | **Atmospheric Read** -- sense weather changes and environmental hazards before they occur. Passive: nearby toxic/electric/elemental zones are highlighted with aura outlines. | +15% elemental damage resistance, hazard awareness | Head develops a smooth, bulbous lobe at the crown that pulses with faint violet bioluminescence. Surface is slightly translucent, revealing neural patterns beneath. |
| Driftfin Membrane | Torso | **Toxic Emission** -- active: release a cloud of toxic spores around the player (5m radius) that damages enemies for 5 seconds. 15-second cooldown. | +20% toxin resistance, +10% HP | Torso develops panels of translucent membrane between the ribs and under the arms. The membrane pulses with violet light and visibly swells before releasing toxin clouds. Internal organs are faintly visible. |
| Driftfin Tendrils | Arms | **Tendril Lash** -- mid-range whip attack (6m reach) that deals damage and applies a 2-second slow. Can hit multiple enemies in an arc. | +10% melee range, mid-range whip attack enabled | Arms develop clusters of bioluminescent tendrils that hang from the forearms and wrists. They coil and uncoil during attacks. Deep indigo with violet veining. |
| Driftfin Bladders | Back | **Glide** -- after jumping, hold jump to glide. The player slowly descends while moving forward at full speed. Duration: up to 5 seconds. Stacks with double-jump if available. | Glide enabled, +30% fall damage reduction, +10% air control | Two gas bladder sacs inflate from the player's upper back/shoulders. They glow pale violet when inflated during glide. When not gliding, they rest as deflated, vein-traced panels against the back. |
| Driftfin Tail Fin | Tail | **Electric Pulse** -- discharge a short-range electric burst (3m radius) that stuns enemies for 1.5 seconds. 12-second cooldown. If used in water, range doubles. | +15% electric damage, electric stun AOE enabled | A broad, flat tail fin extends from the lower back, resembling a manta ray's tail. Edges crackle faintly with static. The fin splays wide during the pulse and emits arcing electricity. |

### Burrower (Sandmaw)

| Part Name | Slot | Ability Granted | Stat Modifications | Visual Change |
|-----------|------|-----------------|-------------------|---------------|
| Sandmaw Teeth Ring | Head | **Gnashing Maw** -- devastating bite attack (close range only) that deals 3x normal melee damage. 6-second cooldown. Heals 10% of damage dealt. | +20% melee damage, +10% lifesteal on bite | The player's face is ringed by a circle of hooked, chitinous teeth that frame the jaw and cheeks. They retract when not biting. Mouth opens wider than human norm during the attack. Unsettling appearance. |
| Sandmaw Segments | Torso | **Tectonic Shift** -- active: burrow underground for up to 3 seconds, becoming invulnerable and untargetable. Emerge at a location up to 8m away. Long cooldown (20 sec). | +15% armor, burrow/phase ability enabled | Torso becomes segmented with chitinous plates that overlap like worm segments. Sandy beige coloring with darker ring markings. Segments visibly flex and compress during the burrow animation. |
| Sandmaw Hooks | Arms | **Drag Under** -- grapple attack that pulls a target toward the player and briefly roots them in place for 2 seconds. | +10% grab strength, grapple-pull enabled | Forearms develop rows of hooked, backward-curving barbs. Hands become claw-like with prominent hooks. Sandy-beige chitin covers the forearms. Hooks click together audibly. |
| Sandmaw Underbelly | Legs | **Tremor Walk** -- the player's footsteps generate vibrations that detect buried/hidden enemies within 10m (highlighted through terrain). Passive. | +10% ground speed, detect hidden/burrowed enemies | Legs become segmented like the torso, with ridged underbelly pads on the feet. Footfalls produce a visible vibration ripple effect on soft ground. Sandy coloring with smooth underside. |
| Sandmaw Tail Segment | Tail | **Sand Spray** -- kick up a cloud of sand/debris behind the player (4m cone), blinding enemies caught in it for 3 seconds. 8-second cooldown. | +10% dodge speed, blind/obscure ability enabled | A thick, segmented tail section extends from the lower back, terminating in a flat, shovel-like plate. The tail drags slightly and automatically sweeps when the ability activates. |

### Sporemother (Mycelith)

| Part Name | Slot | Ability Granted | Stat Modifications | Visual Change |
|-----------|------|-----------------|-------------------|---------------|
| Mycelith Cap | Head | **Spore Cloud** -- release a healing spore cloud that regenerates 5 HP/sec to the player and nearby allies for 5 seconds. 20-second cooldown. | +10% max HP, healing burst enabled | A small mushroom cap grows from the top/back of the player's skull. Bioluminescent spots dot its surface. It releases visible spore particles when the ability is activated. Organic, fungal texture. |
| Mycelith Core | Torso | **Mycorrhizal Network** -- while standing still for 3+ seconds, roots extend into the ground creating a zone (8m radius) that slows enemies by 20% and reveals their positions. | +15% HP, area slow/detection zone enabled (stationary) | Torso develops patches of fungal growth -- soft, bioluminescent tissue that spreads across the chest and abdomen. Small mushroom buds grow along the collarbone. Root-like tendrils trail down from the waistline. |
| Mycelith Tendrils | Arms | **Parasitic Grasp** -- grapple attack that infects the target with spores, dealing damage over time (3 HP/sec for 8 seconds). Stacks up to 3 times on the same target. | +10% DOT damage, parasitic DOT attack enabled | Arms develop trailing fungal tendrils that wrap around the forearms and between the fingers. Tendrils are pale with bioluminescent tips. They visibly extend and grasp during the attack, leaving glowing spore marks on targets. |
| Mycelith Roots | Legs | **Root Network** -- passive: standing on natural ground (dirt, grass, sand) regenerates 1 HP/sec. Active: root in place for 3 seconds to fully heal 30% max HP (long cooldown, interruptible). | +1 HP/sec on natural ground, rooted heal enabled | Lower legs and feet develop thick root-like structures that dig into the ground with each step. Feet become wide and root-threaded. When rooting, visible tendrils extend into the terrain and pulse with green light. |
| Mycelith Spines | Back | **Decomposer Aura** -- passive: dead creatures near the player decompose faster, yielding +50% harvest materials. Active: release a burst of decay spores (5m radius) that weakens enemy armor by 20% for 8 seconds. | +50% harvest yield (near corpses), enemy armor reduction burst enabled | Back develops a cluster of thin, spine-like fungal growths surrounded by shelf-mushrooms. The formation resembles a small fungal garden. Spines release visible golden spore clouds during the active ability. |

### Mimic Stalker (Glassghast)

| Part Name | Slot | Ability Granted | Stat Modifications | Visual Change |
|-----------|------|-----------------|-------------------|---------------|
| Glassghast Visage | Head | **Predator Vision** -- see enemy health bars, current state (idle/alert/attacking), and their detection radius as visible aura. Passive info overlay. | +20% detection range, enemy info HUD enabled | Face becomes smooth and reflective, like polished glass. Features are still visible but muted beneath a prismatic sheen. Eyes become solid reflective orbs that display faint prismatic refraction. Deeply unsettling. |
| Glassghast Skin | Torso | **Active Camo** -- become nearly invisible for 6 seconds. Moving reduces effectiveness (shimmer visible). Attacking breaks camo immediately. 25-second cooldown. | Active camouflage enabled, +10% stealth | Torso develops a smooth, reflective surface layer that shifts colors to match the environment. In bright light, it creates prismatic refractions. When camo activates, the entire body surface ripples like liquid mercury before going transparent. |
| Glassghast Claws | Arms | **Ambush Strike** -- the first melee attack after being unseen by the target for 3+ seconds deals 3x damage and applies a bleed (5 damage/sec for 4 seconds). | +25% stealth attack damage, ambush bonus enabled | Hands develop long, crystalline claws that fold flat against the forearms. The claws are translucent and razor-sharp, catching light in unnerving ways. They extend with a glass-like chime sound before attacks. |
| Glassghast Legs | Legs | **Silent Step** -- footstep sounds are eliminated. Movement no longer alerts vibration-sensitive enemies (Burrowers). Sprint does not increase detection radius. | Silent movement, +10% stealth, sprint detection penalty removed | Legs develop the same reflective surface as the torso, with padded, wide feet that make no sound on any surface. Joint membranes are visible as dark, flexible bands. Movement becomes eerily fluid. |
| Glassghast Spine | Back | **Mirror Image** -- create a holographic decoy of the player at your current position that persists for 5 seconds and attracts enemy attention. 20-second cooldown. | Decoy ability enabled, +10% dodge chance | Back develops a raised dorsal ridge of reflective crystalline plates. When the ability activates, the plates flash brightly and project a shimmering afterimage. The ridge folds flat when not in use but catches light constantly. |

### Shellcrab (Tidebreaker)

| Part Name | Slot | Ability Granted | Stat Modifications | Visual Change |
|-----------|------|-----------------|-------------------|---------------|
| Tidebreaker Eyestalks | Head | **Panoramic Vision** -- 360-degree threat awareness. Enemies that would normally be behind the player are shown on the HUD edge. Cannot be backstabbed. | 360-degree awareness, backstab immunity | Two armored eyestalks extend from the temples, each ending in a small, independently-moving eye. They constantly scan the environment. Blue-green chitinous stalks with orange joint membranes. |
| Tidebreaker Shield | Arms (off-hand) | **Living Shield** -- block incoming attacks from the front, reducing damage by 60%. Can parry: blocking within 0.3 seconds of an attack reflects 30% damage back. | +30% block efficiency, parry enabled, -10% attack speed | One arm (off-hand) develops an enormous, flat, barnacle-encrusted chitinous shield-growth that extends from wrist to shoulder. It is always visible and cannot be "unequipped." Blue-green with orange-joint membrane at the wrist. |
| Tidebreaker Spike | Arms (main hand) | **Piercing Thrust** -- melee attack that ignores 50% of target armor. Narrow hitbox but high damage. | +20% armor penetration, +15% melee damage | The dominant hand develops a long, narrow, hardened spike that extends from the fist. Orange-tipped with blue-green chitin base. The spike retracts partially during non-combat movement. |
| Tidebreaker Carapace | Torso | **Tidal Fortitude** -- in water, the player gains +30% damage resistance and +20% movement speed instead of being slowed. | +20% armor, water speed/resistance bonus | Torso becomes encased in a thick, barnacle-encrusted crustacean shell. Blue-green dominant color with small living organisms (barnacles, tiny anemones) growing on the surface. Small tidal pools of trapped water sit in shell crevices. |
| Tidebreaker Legs | Legs | **Sideways Charge** -- quick lateral dodge-dash that covers 6m and deals knockback damage to anything in the path. 5-second cooldown. | Lateral charge enabled, +10% strafe speed, +15% knockback | Legs become thick, multi-jointed crustacean limbs with four segments each. Blue-green chitin plating with orange membrane joints. Movement becomes a scuttling gait. The lateral charge is a crab-like sideways lunge. |

### Voltwasp (Stormstinger)

| Part Name | Slot | Ability Granted | Stat Modifications | Visual Change |
|-----------|------|-----------------|-------------------|---------------|
| Stormstinger Compound Eyes | Head | **Electric Field Sense** -- detect all electrical/mechanical objects within 20m (including player-made structures, traps, and enemies with electrical attacks). Passive. | +15% detection range, electrical object awareness | Eyes become faceted compound lenses with hundreds of tiny segments. They glow electric blue and refract light into rainbow patterns. Constant faint electrical crackling sounds near the eyes. |
| Stormstinger Thorax | Torso | **Charged Body** -- passive: the player builds up static charge while sprinting. After 3 seconds of sprinting, the next melee attack deals bonus electrical damage (50% bonus) and arcs to one nearby enemy. | +15% electrical damage, sprint-charged attacks enabled | Torso develops a metallic gold-and-black exoskeleton layer over the chest. Small electrical arcs are visible between plate segments when charged. The exoskeleton buzzes faintly during sprint. |
| Stormstinger Wings | Back | **Hover** -- hold jump while airborne to hover in place for up to 3 seconds. Costs stamina. Can attack while hovering. | Hover enabled, +20% air time, +15% air control | Four translucent, electrically-veined wings unfold from the shoulder blades. They beat rapidly during hover, leaving crackling afterimages. When not hovering, they fold into sleek panels against the back. Gold-black with blue electric veins. |
| Stormstinger Stinger | Tail | **Lightning Lance** -- ranged attack: launch a bolt of electricity (20m range) that deals high damage to a single target. If the target is in water or near another electrified enemy, arcs to them. 8-second cooldown. | +20% electrical damage, ranged electric attack enabled | A segmented, barbed stinger extends from the lower back. It arcs with constant electrical discharge. The stinger glows intensely blue-white when the ability is charging. It retracts partially when not in use. |
| Stormstinger Legs | Legs | **Static Cling** -- the player can walk on any surface (walls, ceilings) for up to 5 seconds using electromagnetic adhesion. 10-second cooldown. | Wall/ceiling walk enabled, +10% climb speed | Legs develop segmented metallic chitin with electromagnetic pads on the feet. Faint blue sparks trail from footsteps. The pads visibly crackle when activating the ability. Gold-and-black coloring matches the thorax. |

### Coralgor (Reefwarden) -- BOSS

| Part Name | Slot | Ability Granted | Stat Modifications | Visual Change |
|-----------|------|-----------------|-------------------|---------------|
| Reefwarden Crown | Head | **Tidal Command** -- active: summon a wave of water (10m cone in front) that pushes enemies back and deals moderate damage. In water, the wave is larger and stronger. 15-second cooldown. | +20% water damage, tidal wave attack enabled | Head becomes crowned with living coral formations -- branching turquoise and pink growths that extend upward and to the sides. Small anemone tendrils wave from between the coral. The crown drips with saltwater. |
| Reefwarden Core | Torso | **Living Reef** -- passive: the player regenerates HP in water (3 HP/sec). Small symbiotic organisms grow on the player's armor over time (purely visual). Active: harden coral armor for 4 seconds (+50% damage reduction), 20-second cooldown. | +25% armor, water regen enabled, coral harden active | Torso becomes a living coral reef ecosystem. Turquoise and pink coral plates cover the chest and back. Small anemones and barnacles grow in crevices. Tiny bioluminescent organisms pulse beneath translucent coral panels. The player's silhouette is broader and more organic. |
| Reefwarden Arms | Arms | **Coral Hammer** -- heavy overhead melee attack that creates a 3m AOE shockwave on impact. Slow but devastating. Leaves a brief "coral growth" on the ground that acts as a temporary barrier. | +35% melee damage, AOE slam enabled, -15% attack speed | Arms become massive coral-encrusted limbs ending in hammer-like fist formations. The coral is layered and textured with living growth. Anemone tendrils wrap around the upper arms. The arms are visibly heavier, affecting idle animations. |
| Reefwarden Roots | Legs | **Tidal Anchor** -- while standing in water, the player is immune to knockback and gains +20% damage. On land, stomp attack creates a small water geyser (2m radius AOE). 8-second cooldown. | +15% water damage, water knockback immunity, geyser stomp enabled | Legs become thick root-like structures with coral encrustation. Feet develop into wide, root-splayed bases. Small tidal pools collect in the joints. Visible water drips constantly from the leg joints. Movement is heavier but deliberate. |
| Reefwarden Mantle | Back | **Symbiote Swarm** -- summon 3 small symbiotic creatures (tiny crab-like organisms) that attack nearby enemies for 10 seconds, each dealing small damage. 30-second cooldown. | Summon symbiotes enabled, +10% max HP, +10% armor | Back develops a living mantle of coral, anemone, and barnacle growth that forms a hump-like protrusion. Small organisms visibly crawl across the surface. When the ability activates, they detach and scuttle toward enemies. The mantle slowly regrows its population between uses. |

---

## 3. DNA Catalog

DNA is extracted from incapacitated (not killed) creatures and applied to the player's suit. DNA provides a passive suit effect (always active), an active suit ability (costs suit energy), and synergies with other DNA types.

### DNA Overview Table

| Creature | DNA Type Name | Passive Suit Effect | Active Suit Ability | Energy Cost | Synergies |
|----------|--------------|--------------------|--------------------|-------------|-----------|
| Wolf | Lupine DNA | **Pack Bond** -- +10% damage when within 15m of an allied creature or NPC | **Frenzy** -- for 8 seconds, attack speed +30% and each hit restores 2 HP. 30-sec cooldown. | Medium | Vaulter DNA (Apex Predator: +15% bonus to both jump-attacks and Frenzy healing) |
| Frog | Amphibian DNA | **Adaptive Metabolism** -- toxins and debuffs last 40% shorter on the player | **Leap Surge** -- one supercharged jump (2x height) + soft landing (no fall damage). 10-sec cooldown. | Low | Driftfin DNA (Atmospheric Leap: Leap Surge transitions into glide automatically) |
| Bug | Hivemind DNA | **Swarm Shield** -- taking lethal damage instead leaves the player at 1 HP once per 60 seconds (passive revive) | **Swarm Cloak** -- a cloud of holographic bug-projections surrounds the player for 5 seconds, causing enemy attacks to miss 30% of the time. 25-sec cooldown. | Medium | Mycelith DNA (Living Ecosystem: Swarm Shield cooldown reduced to 40 seconds; spore creatures inherit swarm behavior) |
| JumpingAlien | Vaulter DNA | **Momentum** -- moving at sprint speed for 3+ seconds grants +15% damage on the next attack (momentum strike) | **Skyfall** -- leap 15m into the air, then slam down at target location dealing AOE damage. 15-sec cooldown. | High | Lupine DNA (Apex Predator), Stormstinger DNA (Thunderfall: Skyfall attack deals bonus electric damage and stuns) |
| RockBoss | Lithic DNA | **Stone Resolve** -- when below 25% HP, gain +25% damage resistance (last stand) | **Seismic Slam** -- pound the ground, creating a shockwave that damages and staggers all enemies within 8m. 20-sec cooldown. | Very High | Tidebreaker DNA (Tectonic Tide: Seismic Slam in water creates a tsunami wave that extends the range to 15m) |
| Thornback | Spinemind DNA | **Retribution** -- 10% of all melee damage taken is reflected back to the attacker | **Spine Nova** -- launch spines in all directions (8m radius), dealing moderate damage and applying bleed (3 damage/sec for 5 sec). 18-sec cooldown. | Medium | Sandmaw DNA (Buried Spines: enemies hit by Spine Nova are briefly rooted in place for 2 sec) |
| Gloomray | Driftfin DNA | **Buoyancy** -- fall speed reduced by 30% passively. The player can survive falls that would normally be lethal. | **Miasma** -- release a toxic cloud at current location (6m radius) that persists for 8 seconds, slowing and damaging enemies inside. 20-sec cooldown. | Medium | Amphibian DNA (Atmospheric Leap), Hivemind DNA (Toxic Swarm: Miasma cloud also spawns decoy bugs that distract enemies) |
| Burrower | Sandmaw DNA | **Tremor Detect** -- feel enemy footsteps through the ground; hidden/underground enemies within 12m appear as vibration markers on HUD | **Earthdive** -- burrow underground, become untargetable for 2 seconds, emerge up to 10m away dealing AOE damage at exit point. 20-sec cooldown. | High | Spinemind DNA (Buried Spines), Lithic DNA (Tectonic Dive: Earthdive exit creates a larger shockwave with knockback) |
| Sporemother | Mycelith DNA | **Symbiotic Growth** -- standing still for 2+ seconds begins regenerating 1 HP/sec. Stacks up to 3 HP/sec after 6 seconds of stillness. | **Spore Burst** -- release a healing pulse (8m radius) that restores 20 HP to the player and applies a 5-second regen to the player (+3 HP/sec). Also debuffs enemies in range: -15% attack speed for 5 sec. 25-sec cooldown. | Medium | Hivemind DNA (Living Ecosystem), Reefwarden DNA (Coral Bloom: Spore Burst also creates temporary healing coral patches on the ground) |
| Mimic Stalker | Glassghast DNA | **Shimmer** -- enemies take 0.5 seconds longer to detect the player (delayed aggro) | **Phase Shift** -- become translucent and untargetable for 3 seconds. Can move but not attack. Exiting Phase Shift grants a 2-second window where the next attack deals 2x damage. 30-sec cooldown. | High | Driftfin DNA (Phantom Drift: Phase Shift allows gliding during its duration), Vaulter DNA (Shadow Drop: exiting Phase Shift from above deals 3x instead of 2x) |
| Shellcrab | Tidebreaker DNA | **Shell Harden** -- blocking or crouching grants +10% damage resistance (stacks with existing block) | **Tidal Guard** -- deploy a water-shield barrier in front of the player (3m wide wall) that blocks projectiles and slows enemies passing through. Lasts 6 seconds. 20-sec cooldown. | Medium | Lithic DNA (Tectonic Tide), Reefwarden DNA (Living Barrier: Tidal Guard also heals the player while they stand behind it, 2 HP/sec) |
| Voltwasp | Stormstinger DNA | **Static Aura** -- enemies that hit the player in melee take a small jolt of electrical damage (5% of hit reflected as electric) | **Chain Lightning** -- fire a bolt that hits one target and arcs to up to 3 additional targets within 5m of each other. 12-sec cooldown. | Medium | Vaulter DNA (Thunderfall), Glassghast DNA (Lightning Ghost: Chain Lightning becomes invisible/silent, enemies don't know the source) |
| Coralgor | Reefwarden DNA | **Oceanic Vitality** -- max HP is increased by 15%. In water, increase is 25%. | **Tsunami** -- summon a directional wave (15m cone) that deals heavy damage, pushes enemies back, and briefly floods the area. 30-sec cooldown. | Very High | Mycelith DNA (Coral Bloom), Tidebreaker DNA (Living Barrier), Lithic DNA (Continental Shift: Tsunami also creates permanent terrain deformation -- raised coral barriers) |

### DNA Synergy Web (Visual Reference)

```
                    Lupine -------- Vaulter
                      |               |   \
                      |               |    Stormstinger
                      |               |        |
                      |            Glassghast   |
                      |               |         |
                 Amphibian ------- Driftfin     |
                      |               |         |
                   Hivemind ------- Mycelith    |
                      |               |         |
                  Spinemind ------ Sandmaw      |
                      |               |         |
                  Tidebreaker ---- Lithic       |
                      |               |         |
                  Reefwarden ------+--+---------+
```

Each line represents a synergy pair. Players are incentivized to experiment with different DNA combinations to discover these synergies. The suit could display a "Synergy Active" indicator when a valid pair is equipped.

---

## 4. Creature Ecology

### Food Chain

```
APEX PREDATORS (Top of chain)
    RockBoss (Lithosaur) -- territorial apex; nothing hunts it
    Coralgor (Reefwarden) -- coastal apex; dominates tidal zones
    |
LARGE PREDATORS
    Mimic Stalker (Glassghast) -- solitary apex ambush predator
    Wolf (Howler) -- pack predator, hunts medium creatures
    Shellcrab (Tidebreaker) -- coastal predator/scavenger
    |
MESOPREDATORS / SPECIALISTS
    Voltwasp (Stormstinger) -- aerial pack predator, hunts small/medium
    Burrower (Sandmaw) -- ambush predator, hunts anything on sand
    JumpingAlien (Vaulter) -- territorial omnivore, hunts small creatures
    |
HERBIVORES / DETRIVORES
    Thornback (Spineweaver) -- armored herbivore, too spiny to hunt easily
    Frog (Gulper) -- small herbivore/insectivore, prey for most predators
    Gloomray (Driftfin) -- filter feeder, drifts and consumes airborne particles
    |
DECOMPOSERS / PRIMARY PRODUCERS
    Sporemother (Mycelith) -- stationary fungal organism, decomposes dead matter
    Bug (Chitin Swarmling) -- scavenger/detritivore, consumes everything
```

### Predator-Prey Relationships

| Predator | Prey | Interaction |
|----------|------|-------------|
| Wolf (Howler) | Frog (Gulper), Bug (Swarmling) | Wolves hunt Frogs and feed on Bug swarms. Frog hopping behavior evolved as escape response. |
| Wolf (Howler) | Thornback (Spineweaver) | Wolves occasionally attempt to hunt Thornbacks but are deterred by spines. Injured wolves near Thornbacks suggest failed hunts. |
| Voltwasp (Stormstinger) | Bug (Swarmling), Frog (Gulper) | Voltwasps raid Bug swarms and pick off isolated Frogs from above. |
| Burrower (Sandmaw) | Frog (Gulper), Bug (Swarmling), Shellcrab (juvenile) | Ambushes anything walking on sand. The vibration-detection mechanic is how it actually hunts. |
| Mimic Stalker (Glassghast) | Wolf (Howler), JumpingAlien (Vaulter), Frog (Gulper) | The apex ambush predator. Stalks Wolf packs and picks off stragglers. Hunts JumpingAliens by waiting at cliff edges. |
| Shellcrab (Tidebreaker) | Bug (Swarmling), Frog (Gulper), carrion | Scavenges shoreline. Hunts small creatures opportunistically. |
| JumpingAlien (Vaulter) | Bug (Swarmling), Frog (Gulper) | Leaps onto prey from above. Territorial defender of cliff nests. |

### Symbiotic Relationships

| Relationship | Creatures | Description |
|-------------|-----------|-------------|
| **Mutualism** | Sporemother + Wolf pack | Wolf packs often den near Sporemothers. The spore healing aura keeps the pack healthy; the wolves deter creatures from damaging the Sporemother. **Gameplay:** destroying a Sporemother weakens a nearby Wolf pack. |
| **Mutualism** | Coralgor + Shellcrab | Shellcrabs live on and around the Coralgor. The boss spawns juvenile Shellcrabs during combat. In the open world, Shellcrabs near a Coralgor are more aggressive (defending their host). |
| **Commensalism** | Gloomray + Bug swarm | Bugs follow Gloomrays, feeding on organisms that fall from the toxic cloud. The Gloomray is unaffected. **Gameplay:** where you see Bugs swarming in a column, a Gloomray is overhead. |
| **Parasitism** | Sporemother + any corpse | Sporemothers send root tendrils toward nearby corpses, consuming them over ~30 seconds. If the player does not harvest a kill quickly near a Sporemother, it will consume the body (and the harvestable parts). **Gameplay:** time pressure near Sporemothers. |
| **Commensalism** | Thornback + Bug | Bugs nest in the gaps between Thornback spines, feeding on parasites. Thornbacks tolerate them. **Gameplay:** killing a Thornback causes a burst of Bugs to scatter from the corpse. |

### Territorial Conflicts

| Conflict | Creatures | Description |
|----------|-----------|-------------|
| **Territory overlap** | Wolf pack vs. JumpingAlien | Wolves and Vaulters compete for the same grassland-cliff transition zones. The player may encounter them fighting each other, creating opportunities for ambush or scavenging. |
| **Apex confrontation** | RockBoss vs. anything | The RockBoss's territory is inviolable. No creature willingly enters its zone. If a Wolf pack chases prey into RockBoss territory, the RockBoss will attack the Wolves. **Gameplay:** luring enemies into boss territory is a risky but viable strategy. |
| **Coastal dominance** | Shellcrab vs. Burrower | Shellcrabs and Burrowers compete along the sand-water boundary. They will attack each other on sight, creating chaotic three-way fights when the player is present. |
| **Aerial vs. Ground** | Voltwasp vs. JumpingAlien | Voltwasps and Vaulters have overlapping cliff-territory. Vaulters can reach Voltwasps with their leaps; Voltwasps dive-bomb Vaulter nests. |

### How Ecology Creates Interesting Gameplay

1. **Strategic target prioritization.** Sporemothers heal nearby enemies -- the player learns to identify and kill support creatures first, like taking out a healer in an MMO.

2. **Environmental storytelling.** A trail of dead Bugs leads to a Gloomray overhead. Claw marks on a cliff face reveal a Vaulter nest above. A Thornback covered in quills missing from one side suggests a recent fight with a Wolf pack.

3. **Emergent three-way fights.** Predator-prey interactions mean the player regularly stumbles into ongoing creature conflicts. A Wolf pack chasing a Frog through a Burrower's territory creates a chaotic, dynamic encounter.

4. **Risk-reward territory management.** Killing a Sporemother weakens everything in the area but removes a debuff source. Killing an apex predator may cause prey populations to surge in that area (more Frogs and Bugs, but less danger).

5. **Hunt planning.** If the player wants Wolf parts, they can find a Wolf pack hunting near a Sporemother (the wolves will be healthier and harder). Or they can wait until the pack leaves the aura, ambush stragglers, or use a Thornback encounter to soften the pack up.

6. **Biome transition danger.** The most dangerous areas are biome transitions where multiple creature types overlap: the sand-grass boundary (Burrowers + Wolves + Frogs), cliff edges (Vaulters + Voltwasps + Glassghasts), and shorelines (Shellcrabs + Burrowers + Coralgor territory).

---

## 5. Biome-Creature Mapping

### Current Biomes

| Biome | Terrain Type | Elevation Range | Characteristics |
|-------|-------------|-----------------|-----------------|
| **Beach/Sand** | Sand texture, near water level | `waterLevel` to `waterLevel + 20` | Flat shoreline, tidal areas |
| **Grasslands** | Grass texture, rolling terrain | `waterLevel + 20` to `waterLevel + 60` | Open fields, tree clusters |
| **Rocky Cliffs** | Cliff rock texture, steep slopes | `waterLevel + 60` and above | Steep terrain, cliff faces, peaks |

### Future Biomes (planned)

| Biome | Description | Unique Mechanics |
|-------|-------------|------------------|
| **Swamp** | Low-elevation wetlands with standing water | Slowed movement, visibility reduction, toxin pools |
| **Crystal Caves** | Underground caverns accessed through cliff openings | Enclosed spaces, crystal light sources, echo mechanics |
| **Volcanic** | High-temperature zones near volcanic vents | Heat damage over time, obsidian terrain, fire hazards |

### Creature Spawn Table

| Creature | Beach/Sand | Grasslands | Rocky Cliffs | Swamp (future) | Crystal Caves (future) | Volcanic (future) |
|----------|-----------|------------|-------------|------|--------|---------|
| Wolf (Howler) | -- | **Common** | **Uncommon** | Uncommon | -- | -- |
| Frog (Gulper) | **Common** | **Common** | -- | **Common** | -- | -- |
| Bug (Swarmling) | Uncommon | **Common** | Uncommon | **Common** | Uncommon | -- |
| JumpingAlien (Vaulter) | -- | **Uncommon** | **Common** | -- | Uncommon | -- |
| RockBoss (Lithosaur) | -- | -- | **Boss (unique)** | -- | -- | -- |
| Thornback (Spineweaver) | -- | **Common** | **Uncommon** | -- | -- | -- |
| Gloomray (Driftfin) | -- | **Uncommon** | -- | **Common** | -- | -- |
| Burrower (Sandmaw) | **Common** | Rare (edges) | -- | -- | -- | -- |
| Sporemother (Mycelith) | -- | **Uncommon** | -- | **Common** | **Common** | -- |
| Mimic Stalker (Glassghast) | Rare | Rare | Rare | Rare | Rare | Rare |
| Shellcrab (Tidebreaker) | **Common** | -- | Uncommon (coast) | -- | -- | -- |
| Voltwasp (Stormstinger) | -- | Uncommon | **Common** | -- | Uncommon | -- |
| Coralgor (Reefwarden) | **Boss (unique)** | -- | -- | -- | -- | -- |

Spawn rarity key:
- **Common** = regular spawns, multiple per chunk
- **Uncommon** = occasional spawns, 1-2 per chunk
- **Rare** = low chance spawn, drives exploration
- **Boss (unique)** = single fixed spawn, specific arena

### Biome-Exclusive Creatures

These creatures drive exploration by only appearing in specific biomes:

| Creature | Exclusive Biome(s) | Why It Drives Exploration |
|----------|-------------------|--------------------------|
| Burrower (Sandmaw) | Beach/Sand only (rare at edges) | Players wanting burrow/ambush abilities must venture to the dangerous shoreline |
| Coralgor (Reefwarden) | Beach/Sand boss arena | The beach boss requires dedicated expedition to the coast |
| Shellcrab (Tidebreaker) | Beach/Sand primarily | Shield/armor builds require coastal hunting |
| Voltwasp (Stormstinger) | Rocky Cliffs primarily | Electrical abilities require climbing to high-altitude zones |
| RockBoss (Lithosaur) | Rocky Cliffs boss arena | The cliff boss requires ascending to the peaks |

### Rare and Legendary Variants

Each creature has a chance to spawn as a rare **Alpha** variant or an extremely rare **Legendary** variant. These provide enhanced parts and unique DNA.

| Variant Tier | Spawn Rate | Visual Indicator | Bonus |
|-------------|-----------|------------------|-------|
| **Alpha** | 5% of spawns | Larger size (+50%), bioluminescent glow, more aggressive behavior | Parts grant +25% stat bonuses. DNA provides +1 synergy slot. |
| **Legendary** | 0.5% of spawns (or 1 per biome) | Unique coloration (inverted/spectral), visible particle aura, unique idle animations | Parts grant a unique bonus ability in addition to the normal one. DNA has a third passive effect. |

#### Legendary Variant Concepts

| Creature | Legendary Name | Unique Coloring | Bonus Ability |
|----------|---------------|-----------------|---------------|
| Wolf | **Howler Patriarch** | Pure white with gold bioluminescence | Howler Fangs additionally grant **Pack Call**: summon 2 spectral wolves that fight alongside the player for 15 seconds (long cooldown) |
| Frog | **Elder Gulper** | Deep crimson with black spots | Gulper Legs additionally grant **Meteor Drop**: landing from a Power Leap creates an AOE shockwave |
| Bug | **Hive Queen** | Iridescent gold with royal purple wings | Swarmling Wings additionally grant **Swarm Cloud**: passively surrounded by 3 orbiting spectral bugs that block incoming projectiles |
| JumpingAlien | **Sky Vaulter** | Iridescent blue with silver crest | Vaulter Legs additionally grant **Triple Jump**: three consecutive jumps with increasing height |
| Thornback | **Crystal Monarch** | Pure crystal (transparent with rainbow refraction) | Spineweaver Shell additionally grants **Diamond Skin**: once every 60 seconds, next hit that would kill instead shatters the crystal armor (absorbed) and reforms over 10 seconds |
| Gloomray | **Storm Drifter** | Dark thundercloud purple with lightning veins | Driftfin Bladders additionally grant **Thunderglide**: gliding generates chain lightning below the player |
| Burrower | **Abyssal Maw** | Jet black with red bioluminescent rings | Sandmaw Segments additionally grant **Earthshatter**: emerging from burrow creates a 10m fissure that persists for 5 seconds, damaging enemies who cross |
| Sporemother | **Ancient Mycelith** | Bone-white with gold bioluminescence | Mycelith Core additionally grants **Fungal Resurrection**: upon death, the player respawns at the Sporemother's location with 50% HP (once per 5 minutes, must be near one) |
| Mimic Stalker | **Void Phantom** | Completely transparent even when "visible"; only eyes are seen (two floating red orbs) | Glassghast Skin additionally grants **True Invisibility**: Active Camo no longer produces a shimmer when moving |
| Shellcrab | **Ancient Tidebreaker** | Deep coral red with gold barnacles | Tidebreaker Shield additionally grants **Counter Crush**: successful parries stun the attacker for 2 seconds |
| Voltwasp | **Tempest Queen** | White lightning body with violet wings | Stormstinger Wings additionally grant **Storm Mantle**: while hovering, all nearby enemies are continuously hit by weak lightning |
| RockBoss | **Primordial Lithosaur** | Obsidian black with magma-vein cracks | Lithosaur Core additionally grants **Eruption**: Stone Skin active ability ends with a volcanic explosion dealing massive AOE damage |
| Coralgor | **Leviathan Reefwarden** | Deep ocean blue-black with bioluminescent coral | Reefwarden Core additionally grants **Depth Pressure**: all enemies within 10m move 20% slower (permanent aura, like being deep underwater) |

### Spawn Integration with Existing Systems

Based on the current codebase, creature spawning can integrate with the existing terrain and chunk systems:

**Height-based spawning** (using `TerrainGeneration.cs` height values):
- Creatures check the terrain height at their spawn point against `waterLevel` offsets from `ChunkGeneration`
- Beach creatures: spawn where `y < waterLevel + 20`
- Grassland creatures: spawn where `waterLevel + 20 < y < waterLevel + 60`
- Cliff creatures: spawn where `y > waterLevel + 60`

**Spawn density** (using noise, similar to existing tree spawning):
- Use a separate Perlin noise layer with creature-specific seed offsets to determine spawn points
- Reference the existing `treeThreshold` pattern from `TerrainGeneration.cs` for consistent density control
- Each creature type gets its own threshold value exposed in the `ChunkManager`

**Chunk-based management**:
- The existing `ChunkManager` system can be extended with a spawn table (creature type, biome, rarity, max count per chunk)
- Creatures spawn when chunks load and despawn when chunks unload (using the chunk lifecycle already in `ChunkGeneration`)

---

## Appendix: Implementation Priority

Suggested order for adding new creatures, based on the systems already in place:

### Phase 1: Uses existing AI patterns directly
1. **Thornback** -- reskin/modify `EnemyAI` (no chase, just idle + spine state)
2. **Shellcrab** -- extend `BaseAI` like Wolf but with directional blocking
3. **Voltwasp** -- new flying AI but simple state machine (circle/dive/retreat)

### Phase 2: Requires new mechanics but manageable scope
4. **Burrower** -- underground state with particle trail; custom AI (no NavMesh underground)
5. **Sporemother** -- stationary; trigger zones + healing aura (relatively simple)
6. **Gloomray** -- floating patrol AI with hazard zones

### Phase 3: Complex AI and interactions
7. **Mimic Stalker** -- requires camo shader + opportunistic behavior logic
8. **Coralgor (Boss)** -- full behavior tree like RockBoss + tidal arena mechanic

### Parallel: Body Part and DNA Systems
- Build the grafting/DNA systems independently of creature AI
- Use existing `PowerItemI` pattern as foundation (already has `Eat()` for stat modification)
- Extend `PlayerMovement` with modifier slots that parts/DNA can write to
- Create a `BodyPartSlot` component system on the player that handles visual swaps + stat stacking
