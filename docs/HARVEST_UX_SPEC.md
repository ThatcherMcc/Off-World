# Creature Harvesting UX Specification

**Game:** Off-World
**System:** Creature Encounter & Harvesting Flow
**Scope:** All player-facing UI from creature death/incapacitation through harvest completion
**Implementation:** Unity OnGUI (text, boxes, buttons -- no Canvas/shaders required)

---

## Table of Contents

1. [Flow State Machine](#1-flow-state-machine)
2. [World-Space Interact Prompts](#2-world-space-interact-prompts)
3. [Graft Menu (Kill Path)](#3-graft-menu-kill-path)
4. [Extract Menu (Incapacitate Path)](#4-extract-menu-incapacitate-path)
5. [Edge Cases](#5-edge-cases)
6. [Color Palette & Style Constants](#6-color-palette--style-constants)
7. [Implementation Notes](#7-implementation-notes)

---

## 1. Flow State Machine

Every creature encounter resolves into one of these terminal states. The UI reacts to the creature's `EnemyHealth` state and the `IncapacitationController`/`CreatureCorpse` components.

```
                    COMBAT
                      |
         +------------+------------+
         |                         |
    HP hits 0                HP hits threshold
    (creature dies)          (creature incapacitated)
         |                         |
    CORPSE STATE              INCAP STATE
    [CreatureCorpse]          [EnemyHealth.IsIncapacitated]
         |                         |
    Player looks at:          Player looks at:
    GRAFT PROMPT              EXTRACT PROMPT
         |                         |
    Press E:                  Press E:
    GRAFT MENU                EXTRACT MENU
         |                         |
    Confirm:                  Confirm:
    GRAFT RESULT              EXTRACT RESULT
         |                         |
      (Close)                   (Close)
                                   |
                    +--------------+-------------+
                    |                            |
              Player finishes            Creature recovers
              or walks away              (timer expires)
                                               |
                                         CREATURE FLEES
                                         (no harvest)
```

Additional transitions:
- **INCAP STATE -> CORPSE STATE**: Player hits incapacitated creature. `EnemyHealth.HurtEnemy` reduces HP to 0, fires `OnDied`. Prompt changes from Extract to Graft.
- **CORPSE STATE -> despawn**: 60 second timer. Corpse fades and is destroyed.
- **INCAP STATE -> recovery**: 12 second timer. Creature stands up, AI re-enables, flees.

---

## 2. World-Space Interact Prompts

These are the on-screen prompts that appear when the player's crosshair/camera raycast hits a harvestable target within `InteractController.InteractRange` (currently 3m). These replace the generic "E" prompt from the existing `UIInteract` CanvasGroup system with context-specific text.

### 2.1 Corpse Prompt (Kill Path)

**Trigger:** Player looks at a `CreatureCorpse` component within interact range.

```
+------------------------------------------+
|                                          |
|       [E] HARVEST REMAINS               |
|       Wolf  --  2 parts available        |
|                                          |
+------------------------------------------+
```

**Exact text strings:**
- Line 1: `"[E] HARVEST REMAINS"`
- Line 2: `"{CreatureName}  --  {dropCount} part{s} available"`

**Colors:**
- Line 1 text: `#D4A843` (dull amber/bone -- dead things, dried organic matter)
- Line 2 text: `#A89070` (muted tan -- subdued, secondary info)
- Background: `rgba(15, 12, 10, 0.70)` -- dark brown-black, 70% opacity

**Position:** Screen center, offset 60px below crosshair.

**Font:** Line 1 at 20pt bold. Line 2 at 15pt normal.

**Despawn countdown (bottom-right of prompt box):**
When corpse has less than 15 seconds remaining before despawn, show:
- Text: `"Decomposing... {seconds}s"` in `#8B4513` (dark sienna), 13pt, italic
- This pulses between 100% and 50% alpha at 2Hz during final 5 seconds

**No prompt if:** `CreatureCorpse.harvested == true` (already harvested), or corpse is outside interact range.

### 2.2 Incapacitated Creature Prompt (Incapacitate Path)

**Trigger:** Player looks at an `EnemyHealth` where `IsIncapacitated == true` and `HasBeenExtracted == false`, within interact range.

```
+------------------------------------------+
|                                          |
|       [E] EXTRACT DNA                    |
|       Wolf  --  specimen alive           |
|                                          |
|       [====-------] 8s remaining         |
|                                          |
+------------------------------------------+
```

**Exact text strings:**
- Line 1: `"[E] EXTRACT DNA"`
- Line 2: `"{CreatureName}  --  specimen alive"`
- Line 3: Recovery timer bar + `"{seconds}s remaining"`

**Colors:**
- Line 1 text: `#44CCAA` (teal-cyan -- clinical, scientific, alive)
- Line 2 text: `#78B8A0` (muted sea green -- alive but subdued)
- Timer bar filled: `#44CCAA` (same teal)
- Timer bar empty: `#1A3333` (dark teal void)
- Timer bar when < 4s: filled portion shifts to `#CC4444` (warning red), text becomes `"RECOVERING SOON"`
- Background: `rgba(8, 18, 16, 0.70)` -- dark blue-green, 70% opacity

**Position:** Same as corpse prompt. Screen center, 60px below crosshair.

**Font:** Line 1 at 20pt bold. Line 2 at 15pt normal. Timer text at 13pt bold.

**Timer bar dimensions:** 200px wide, 8px tall, centered below the text.

**Urgency escalation:**
- > 6s remaining: Steady teal, no urgency
- 4-6s remaining: Timer text becomes `"Act now -- {seconds}s"` in `#CCAA44` (amber warning)
- < 4s remaining: Timer bar turns red, text becomes `"RECOVERING SOON -- {seconds}s"` in `#CC4444`, prompt box pulses alpha between 100% and 80% at 3Hz
- 0s: Prompt disappears. Creature begins recovery animation.

**No prompt if:** `HasBeenExtracted == true`, creature is not incapacitated, or creature is outside interact range.

### 2.3 Prompt Switching

If the player kills an incapacitated creature (hitting it while downed), the prompt must transition:

1. Teal EXTRACT prompt disappears instantly (single frame).
2. `EnemyHealth.HurtEnemy` fires `OnDied`, `IncapacitationController.HandleDied` adds `CreatureCorpse`.
3. Amber HARVEST prompt appears on the next frame the player looks at the now-corpse object.

There is no transition animation. The hard cut communicates: "You killed it. The option changed."

---

## 3. Graft Menu (Kill Path)

Opened when the player presses E while looking at a `CreatureCorpse`. This is the permanent, body-altering choice. The UI must communicate weight, consequence, and irreversibility.

### 3.1 Visual Tone: Visceral / Organic

The graft menu uses a dark organic palette. Backgrounds are near-black with warm undertones (like dried blood under chitin). Text is stark white and amber. The framing should feel like looking at an alien autopsy report carved in bone.

### 3.2 State: Part Selection

If the creature dropped multiple parts (e.g., RockBoss drops 4), the player first picks which part to graft. If only one part dropped, skip to 3.3.

```
+==========================================================+
|  [X]                                                     |
|                                                          |
|            BIOLOGICAL HARVEST                            |
|            Wolf                                          |
|                                                          |
|  --------------------------------------------------------|
|                                                          |
|  Select a body part to graft:                            |
|                                                          |
|  +----------------------------------------------------+  |
|  |  WOLF JAW                                          |  |
|  |  Slot: HEAD     Species: Wolf     Tier: Standard   |  |
|  |  +30% attack damage on both swipes                 |  |
|  +----------------------------------------------------+  |
|                                                          |
|  +----------------------------------------------------+  |
|  |  WOLF HAUNCHES                                     |  |
|  |  Slot: LEGS     Species: Wolf     Tier: Standard   |  |
|  |  +40% sprint speed, +20% walk speed                |  |
|  +----------------------------------------------------+  |
|                                                          |
|             [ WALK AWAY ]                                |
|                                                          |
+==========================================================+
```

**Panel dimensions:** 520px wide, dynamic height (min 320px, grows with part count). Centered on screen.

**Strings:**
- Title: `"BIOLOGICAL HARVEST"` -- 24pt bold, color `#D4A843` (amber)
- Subtitle: `"{CreatureName}"` -- 18pt normal, color `#A89070` (tan)
- Instruction: `"Select a body part to graft:"` -- 16pt normal, color `#CCCCCC` (light gray)

**Part entry boxes:**
- Background: `rgba(30, 22, 18, 0.90)` (dark brown)
- Hover background: `rgba(50, 35, 25, 0.95)` (warmer brown on hover)
- Part name: 18pt bold, `#E8D8B8` (pale bone)
- Slot/Species/Tier line: 14pt normal, `#A89070` (tan)
- Description: 14pt normal, `#CCCCCC`
- Each entry is a clickable button. Clicking advances to 3.3.

**Walk Away button:** `"WALK AWAY"` -- 16pt bold, `#888888` (gray). Closes the menu. No harvest occurs.

### 3.3 State: Graft Confirmation

This is the "point of no return" screen. It shows exactly what will happen to the player's body.

```
+==========================================================+
|  [X]                                                     |
|                                                          |
|            GRAFT: WOLF JAW                               |
|            This is permanent.                            |
|                                                          |
|  --------------------------------------------------------|
|                                                          |
|  TARGET SLOT: HEAD                                       |
|                                                          |
|  CURRENT:   [Empty -- Human Default]                     |
|         or: [Bug Antennae (Bug, Standard)]               |
|                                                          |
|  REPLACING WITH:                                         |
|    Wolf Jaw (Wolf, Standard)                             |
|    +30% attack damage on both swipes                     |
|    Ability: Lunge Bite -- sprinting attack deals 2x      |
|             damage and lunges 3m forward                  |
|                                                          |
|  --------------------------------------------------------|
|                                                          |
|  STAT PREVIEW:                                           |
|    Attack Damage:   20.0  -->  26.0  (+6.0)              |
|    Walk Speed:      7.0   -->  7.0   (no change)         |
|    Sprint Speed:    14.0  -->  14.0  (no change)         |
|    Max HP:          100   -->  100   (no change)         |
|                                                          |
|  --------------------------------------------------------|
|                                                          |
|  SPECIES STATUS:                                         |
|    Wolf parts equipped: 0 --> 1 of 3 needed for          |
|    [Pack Hunter] affinity                                |
|    Distinct species: 1 (safe -- penalty at 4+)           |
|                                                          |
|  ========================================================|
|                                                          |
|  ! WARNING: This will permanently alter your body.       |
|  ! The replaced part is destroyed forever.               |
|                                                          |
|      [ GRAFT ONTO BODY ]          [ CANCEL ]             |
|                                                          |
+==========================================================+
```

**Panel dimensions:** 520px wide, ~540px tall. Centered.

**Section-by-section:**

**Header:**
- `"GRAFT: {PART_NAME}"` -- 22pt bold, `#D4A843` (amber)
- `"This is permanent."` -- 16pt bold italic, `#CC6644` (burnt orange-red -- mild alarm without full red)

**Target Slot:**
- `"TARGET SLOT: {SLOT_NAME}"` -- 16pt bold, `#E8D8B8`
- `"CURRENT:  {currentPartName} ({species}, {tier})"` -- 15pt, `#A89070`
- If slot is empty: `"CURRENT:  [Empty -- Human Default]"` in `#666666` (dim gray)

**Replacing With:**
- `"REPLACING WITH:"` -- 15pt bold, `#D4A843`
- Part name and description: 15pt, `#CCCCCC`
- Ability line (if part has an active ability): 14pt, `#CCAA44` (gold -- abilities are special)
- If part has no active ability, omit the Ability line entirely.

**Stat Preview:**
- Header: `"STAT PREVIEW:"` -- 15pt bold, `#E8D8B8`
- Each stat line: `"  {StatName}:  {before}  -->  {after}  ({diff})"` -- 14pt monospace-style
- Positive diff: `#44CC66` (green) with `+` prefix
- Negative diff: `#CC4444` (red) with `-` prefix (no explicit minus sign needed since the number is negative)
- No change: `#666666` (dim), text `"(no change)"`
- Only show stats that the part actually modifies, plus Max HP always. Do not show all 8 stats if most are unchanged -- clutter kills readability. Show the changed ones first, then a collapsed `"All other stats: no change"` line in gray.

**Species Status:**
- `"SPECIES STATUS:"` -- 15pt bold, `#E8D8B8`
- `"  {species} parts equipped: {current} --> {new} of 3 needed for [{AffinityName}] affinity"` -- 14pt, `#A89070`
- If gaining affinity (hitting 3): this line becomes `#44CC66` (green), text changes to `"AFFINITY UNLOCKED: [{AffinityName}]"`
- If losing affinity (replacing a species-matching part): this line becomes `#CC4444` (red), text: `"WARNING: Losing [{AffinityName}] affinity"`
- Distinct species line: `"  Distinct species: {count} (safe -- penalty at 4+)"` -- 14pt, `#A89070`
- If count would hit 4+: `#CC4444`, text: `"DANGER: {count} species -- biological rejection will activate (-10% HP, passive drain)"`

**Warning Block:**
- Background: `rgba(60, 20, 15, 0.50)` (dark red tint behind the warning text)
- `"! WARNING: This will permanently alter your body."` -- 15pt bold, `#CC6644`
- `"! The replaced part is destroyed forever."` -- 14pt normal, `#CC6644`
- If the current slot is empty (no part being destroyed), the second line changes to: `"! This cannot be undone."` -- less alarming since nothing is lost.

**Buttons:**
- `"GRAFT ONTO BODY"` -- 18pt bold. Background: `rgba(100, 50, 20, 0.90)` (dark burnt sienna). Text: `#E8D8B8`. On hover: background brightens to `rgba(140, 70, 30, 0.95)`.
- `"CANCEL"` -- 16pt bold. Background: `rgba(40, 40, 40, 0.80)` (neutral dark). Text: `#888888`. On hover: text becomes `#CCCCCC`.
- Button height: 50px. GRAFT button is 60% width left-aligned, CANCEL is 35% width right-aligned.

### 3.4 State: Graft Result

Shown after the player confirms the graft. The graft has already been applied via `AnatomyManager.GraftPart()`.

```
+==========================================================+
|                                                          |
|            GRAFTED: WOLF JAW                             |
|            Your body has changed.                        |
|                                                          |
|  --------------------------------------------------------|
|                                                          |
|  HEAD slot: Wolf Jaw (Wolf)                              |
|                                                          |
|  Stat Changes:                                           |
|    Attack Damage:   20.0  -->  26.0  (+6.0)              |
|                                                          |
|  Replaced: Bug Antennae (destroyed)                      |
|            or: [Slot was empty]                           |
|                                                          |
|  Species: Wolf (1/3 for Pack Hunter affinity)            |
|                                                          |
|  --------------------------------------------------------|
|                                                          |
|                       [ OK ]                             |
|                                                          |
+==========================================================+
```

**Panel:** 480px wide, ~360px tall. Centered.

**Strings:**
- Title: `"GRAFTED: {PART_NAME}"` -- 22pt bold, `#D4A843`
- Subtitle: `"Your body has changed."` -- 16pt italic, `#CC6644`
- Slot line: `"{SLOT} slot: {partName} ({species})"` -- 16pt, `#E8D8B8`
- Stat changes: Same format as preview, but now showing actual before/after from the `SnapshotStats()` diff.
- Replaced line: `"Replaced: {oldPartName} (destroyed)"` in `#CC6644` if a part was replaced, or `"Slot was empty"` in `#666666` if not.
- Species progress: `"{species} ({count}/3 for {affinityName} affinity)"` in `#A89070`, or `"AFFINITY ACTIVE: {affinityName}"` in `#44CC66` if threshold met.

**OK button:** `"OK"` -- 18pt bold, `#E8D8B8`. Background: `rgba(60, 50, 40, 0.90)`. Centered. 200px wide, 50px tall. Closes menu.

**Auto-close:** The result screen does NOT auto-close. The player must acknowledge it. The world continues running (no time pause), so there is implicit pressure from the environment, but the player controls when they dismiss it.

---

## 4. Extract Menu (Incapacitate Path)

Opened when the player presses E while looking at an incapacitated (alive, downed) creature. This is the reversible, scientific path. The UI communicates precision, data collection, and urgency.

### 4.1 Visual Tone: Clinical / Technical

The extract menu uses a dark blue-green palette. Text is white and teal. The framing should feel like a suit-mounted bio-scanner readout. Clean lines, monospace stat readouts, a sense of efficient data capture under time pressure.

### 4.2 State: Extract Confirmation

Because extraction is swappable and non-destructive, we skip the multi-screen confirmation flow used for grafts. One screen, one button, a timer creating urgency.

```
+==========================================================+
|  [X]                                                     |
|                                                          |
|            DNA EXTRACTION                                |
|            Wolf -- specimen alive                        |
|                                                          |
|  [===============---------] RECOVERING IN 8s             |
|                                                          |
|  --------------------------------------------------------|
|                                                          |
|  SAMPLE: Wolf DNA                                        |
|  TIER:   Prime                                           |
|  (Precise incapacitation -- full potency)                |
|            or                                            |
|  TIER:   Degraded                                        |
|  (Imprecise -- 70% potency)                              |
|                                                          |
|  --------------------------------------------------------|
|                                                          |
|  ACTIVE ABILITY: Predator Dash                           |
|    Dash 8m forward, 15 dmg to enemies in path            |
|    Energy: 20  |  Cooldown: 6s                           |
|                                                          |
|  PASSIVE BONUS (if placed in passive slot):              |
|    Keen Nose: enemy indicators within 25m                |
|                                                          |
|  --------------------------------------------------------|
|                                                          |
|  SUIT STATUS:                                            |
|    Active slots:  [Wolf DNA] [Empty] [Empty] [Locked]    |
|    Passive slots: [Empty] [Locked]                       |
|    --> Will load into: Active Slot 2                     |
|                                                          |
|  --------------------------------------------------------|
|                                                          |
|        [ EXTRACT DNA ]            [ LEAVE IT ]           |
|                                                          |
+==========================================================+
```

**Panel:** 520px wide, ~480px tall. Centered.

**Header:**
- `"DNA EXTRACTION"` -- 22pt bold, `#44CCAA` (teal)
- `"{CreatureName} -- specimen alive"` -- 16pt normal, `#78B8A0` (muted teal)

**Recovery Timer Bar (top of panel, always visible):**
- Full width of the content area (480px). 10px tall.
- Filled color: `#44CCAA` (teal). Depletes left-to-right as time runs out.
- Empty color: `#1A3333` (dark void).
- Text right-aligned next to bar: `"RECOVERING IN {seconds}s"` -- 14pt bold.
- Color transitions:
  - > 6s: `#44CCAA` (calm teal)
  - 4-6s: `#CCAA44` (amber), text: `"ACT NOW -- {seconds}s"`
  - < 4s: `#CC4444` (red), text: `"RECOVERING SOON -- {seconds}s"`, bar pulses alpha at 3Hz
  - 0s: Panel auto-closes. Creature recovers. Extraction failed.

**Sample Info:**
- `"SAMPLE: {sampleName}"` -- 16pt bold, `#E8E8E8`
- `"TIER: Prime"` -- 15pt bold, `#44CC66` (green for Prime)
  - Subtext: `"(Precise incapacitation -- full potency)"` -- 13pt italic, `#44CC66`
- `"TIER: Degraded"` -- 15pt bold, `#CCAA44` (amber for Degraded)
  - Subtext: `"(Imprecise -- 70% potency)"` -- 13pt italic, `#CCAA44`

**Tier is calculated from:** `CreatureLootTable.DetermineDNATier(enemyHealth.GetHealthNormalized())`. The UI reads this in real-time from the creature's current HP when the menu opens.

**Active Ability:**
- `"ACTIVE ABILITY: {abilityName}"` -- 15pt bold, `#44CCAA`
- Description: 14pt, `#CCCCCC`
- `"Energy: {cost}  |  Cooldown: {cooldown}s"` -- 14pt, `#78B8A0`
- If DNA has no active ability (edge case), show `"No active ability"` in `#666666`.

**Passive Bonus:**
- `"PASSIVE BONUS (if placed in passive slot):"` -- 15pt bold, `#44CCAA`
- Passive effect description: 14pt, `#CCCCCC`
- Stat modifiers in same format as graft stat preview but with teal coloring for positive values.
- If DNA has no meaningful passive modifiers, show `"Minimal passive effect"` in `#666666`.

**Suit Status:**
- `"SUIT STATUS:"` -- 15pt bold, `#E8E8E8`
- Active slots rendered as a row of boxes: `[{name}]` for occupied, `[Empty]` for open, `[Locked]` for not yet unlocked.
  - Occupied: `#44CCAA` text on `rgba(20, 50, 45, 0.80)` background
  - Empty: `#78B8A0` text on `rgba(15, 30, 28, 0.60)` background
  - Locked: `#444444` text on `rgba(20, 20, 20, 0.60)` background
- Same for passive slots.
- `"--> Will load into: {slot type} Slot {index}"` -- 14pt bold, `#44CCAA`
- The auto-target slot is the first empty active slot. If all active slots are full, targets the first empty passive slot. If ALL slots are full, see edge case 5.3.

**Buttons:**
- `"EXTRACT DNA"` -- 18pt bold. Background: `rgba(20, 80, 70, 0.90)` (dark teal). Text: `#E8E8E8`. Hover: `rgba(30, 110, 95, 0.95)`.
- `"LEAVE IT"` -- 16pt bold. Background: `rgba(40, 40, 40, 0.80)` (neutral). Text: `#888888`. Hover: text `#CCCCCC`.
- Same sizing as graft buttons: EXTRACT is 60% width, LEAVE IT is 35% width.

### 4.3 State: Extraction Channeling

After pressing EXTRACT DNA, the player must channel for `DNAExtractor.extractionChannelTime` (currently 3 seconds). During channeling:

**The menu panel stays open but the buttons are replaced by a channeling bar:**

```
|  --------------------------------------------------------|
|                                                          |
|  EXTRACTING...                                           |
|  [==============                        ]                |
|  Hold position. Do not take damage.                      |
|                                                          |
```

- `"EXTRACTING..."` -- 16pt bold, `#44CCAA`, pulses at 1Hz
- Channel bar: 400px wide, 14px tall. Fill color: `#44CCAA`. Background: `#1A3333`.
- `"Hold position. Do not take damage."` -- 13pt italic, `#78B8A0`
- If the player moves too far from the creature (`extractionRange * 1.5`) or takes damage, channeling is interrupted. The bar resets. Text briefly flashes `"INTERRUPTED"` in `#CC4444` for 1 second, then returns to the confirmation state.
- The recovery timer bar at the top CONTINUES to count down during channeling. If it hits 0 during channeling, extraction fails and the panel closes.
- Channeling progress is read from `DNAExtractor.ExtractionProgress` (0-1).

### 4.4 State: Extract Result

Shown after channeling completes successfully.

```
+==========================================================+
|                                                          |
|            DNA ACQUIRED                                  |
|            Wolf DNA -- Prime                             |
|                                                          |
|  --------------------------------------------------------|
|                                                          |
|  Loaded into: Active Slot 2                              |
|                                                          |
|  Active: Predator Dash                                   |
|    Press [2] in combat to activate                       |
|    Energy: 20  |  Cooldown: 6s                           |
|                                                          |
|  Passive: Keen Nose                                      |
|    (Move to a passive slot to activate)                  |
|                                                          |
|  --------------------------------------------------------|
|                                                          |
|  Stat Changes (passive, if moved to passive slot):       |
|    No immediate stat changes.                            |
|    Passive bonuses apply when DNA is in a passive slot.  |
|                                                          |
|  --------------------------------------------------------|
|                                                          |
|                       [ OK ]                             |
|                                                          |
+==========================================================+
```

**Panel:** 480px wide, ~380px tall. Centered.

**Header:**
- `"DNA ACQUIRED"` -- 22pt bold, `#44CCAA`
- `"{sampleName} -- {tier}"` -- 16pt, `#78B8A0`. Tier word is colored: Prime = `#44CC66`, Degraded = `#CCAA44`.

**Slot assignment:**
- `"Loaded into: {slotType} Slot {index}"` -- 16pt bold, `#E8E8E8`
- If a previous DNA was displaced: `"Replaced: {oldSampleName} (returned to inventory)"` in `#CCAA44`. Note the language: "returned" not "destroyed" -- DNA is never lost, only swapped.

**Ability info:** Same format as the confirmation screen.

**Keybind reminder:**
- `"Press [{keyNumber}] in combat to activate"` -- 14pt, `#44CCAA`
- This maps to `activeKeys[slotIndex]` (Alpha1-Alpha4).

**Passive note:**
- `"(Move to a passive slot to activate)"` -- 13pt italic, `#78B8A0`
- This reminds the player that passive bonuses only work in passive slots. The DNA was auto-loaded into an active slot.

**OK button:** `"OK"` -- 18pt bold, `#E8E8E8`. Background: `rgba(20, 60, 55, 0.90)`. Centered. 200px wide, 50px tall.

---

## 5. Edge Cases

### 5.1 Killing an Incapacitated Creature

**Scenario:** Creature is incapacitated (downed, alive). Player hits it again instead of extracting.

**What happens in code:**
1. `EnemyHealth.HurtEnemy` detects `isIncapacitated == true`, applies damage.
2. If `health <= 0`, sets `isIncapacitated = false` and calls `Die()`.
3. `IncapacitationController.HandleDied()` fires, adds `CreatureCorpse` component, rolls graft drops.
4. The creature is now a corpse. AI is already disabled. Ragdoll physics take over.

**What happens in UI:**
1. If the Extract menu was open (player pressed E, then attacked -- only possible if they closed the menu first since cursor is freed during menu), the menu was already closed.
2. The interact prompt switches from teal EXTRACT to amber HARVEST on the next look-at.
3. The player now gets the GRAFT menu when pressing E.
4. The recovery timer stops. The corpse despawn timer (60s) starts.

**Important:** The DNA extraction option is permanently lost for this creature. The player made their choice by killing it. No DNA from a corpse, ever. This is the core design tension.

**Feedback text (brief on-screen flash, not a menu):**
- When the incapacitated creature is killed, flash center-screen for 2 seconds:
- `"SPECIMEN KILLED"` -- 20pt bold, `#CC4444` (red)
- `"DNA extraction no longer possible"` -- 14pt normal, `#CC6644`
- Background: `rgba(40, 10, 10, 0.60)` -- dark red tint, 60% opacity
- This fades out over the last 0.5 seconds.

### 5.2 Timer Expiration During Menu

**Scenario (Incap):** Player has the Extract menu open. The creature's recovery timer reaches 0.

**What happens:**
1. `EnemyHealth` fires `OnRecovered`.
2. `IncapacitationController.HandleRecovered()` re-enables AI, plays recovery animation.
3. The Extract menu CLOSES AUTOMATICALLY with a flash message:

```
   "SPECIMEN RECOVERED"     (#CCAA44, amber)
   "The creature escaped"   (#A89070, tan)
```

Displayed for 2.5 seconds, centered, same flash format as 5.1.

4. The creature stands up and the player must fight or flee.

**Scenario (Corpse):** Player has the Graft menu open. The corpse despawn timer reaches 0.

**What happens:**
1. `Destroy(gameObject, despawnTime)` fires.
2. The Graft menu CLOSES AUTOMATICALLY with a flash message:

```
   "REMAINS DECOMPOSED"     (#8B4513, dark sienna)
   "The body has decayed"   (#A89070, tan)
```

Displayed for 2.5 seconds. The `CreatureCorpse` object is destroyed, and with it, the pending drops.

### 5.3 All Slots Full

**Graft path (all 6 graft slots occupied):**
- The graft menu still works normally. The player can always overwrite a graft slot. The confirmation screen shows what will be replaced and destroyed.
- Additional emphasis: The "Replaced" line in the warning becomes: `"! {oldPartName} will be PERMANENTLY DESTROYED"` in `#CC4444` bold.

**Extract path (all active + passive DNA slots occupied):**
- The suit status section shows all slots filled.
- The auto-target line changes to: `"--> Will REPLACE: {existingSampleName} in Active Slot 1"` in `#CCAA44` (amber warning).
- An additional line: `"Displaced DNA is returned to inventory -- nothing is lost."` in `#78B8A0`.
- The EXTRACT button still works. The oldest/first-slot DNA is replaced.
- The extract result screen shows: `"Replaced: {oldName} (returned to inventory)"`.

**Extract path (all active slots full, passive slots available):**
- Auto-target selects the first empty passive slot.
- `"--> Will load into: Passive Slot 1 (stat bonus always active)"` in `#44CC66` (green -- this is arguably a better slot).

**Extract path (no unlocked slots at all -- edge case for early game):**
- Should not happen since `DNASuit` starts with 2 active and 1 passive unlocked.
- Defensive: if somehow all unlocked slots are zero, the EXTRACT button is grayed out.
- Text: `"No suit slots available. Unlock more suit capacity."` in `#CC4444`.

### 5.4 Creature With No Drops

**Scenario:** `CreatureLootTable.RollDrops()` returns an empty array (bad luck on all drop rates).

**Graft menu:**
- Title changes to: `"BIOLOGICAL HARVEST"` (same)
- Body shows: `"No harvestable parts remain on this creature."` in `#666666`
- No part selection buttons. Only `[ WALK AWAY ]`.
- The corpse can still be interacted with (pressing E again shows the same empty screen). It despawns on timer as normal.

**Extract menu:** DNA extraction is always 100% drop rate per the `CreatureLootTable.dnaSample` field, so this edge case should not occur for extraction. If `dnaSample` is null (misconfigured creature), the EXTRACT button is disabled with text: `"No DNA signature detected"` in `#CC4444`.

### 5.5 Multiple Parts Dropped -- Partial Harvest

**Scenario:** RockBoss drops 4 parts. Player grafts one and closes the menu.

**Behavior:** The `CreatureCorpse` retains all ungrafted parts. The player can press E again to reopen the menu and graft another part. Each graft is a separate confirm flow. The corpse tracks which parts have been taken (mark as null in the drops array after grafting).

**Implementation detail:** After each graft, remove the grafted part from `CreatureCorpse.drops`. When the player reopens the menu, only remaining parts are shown. When all parts are gone or the corpse despawns, the opportunity is lost.

### 5.6 Player Takes Damage While Menu Is Open

**Behavior:** The menu does NOT close on damage. The game does not pause. If the player is attacked while browsing the graft/extract menu, they take damage normally. The cursor is freed (for clicking buttons), so the player cannot fight back until they close the menu.

**This is intentional.** The harvest happens in a dangerous world. The player must clear the area or accept the risk of browsing a menu while enemies are nearby. This creates natural tension, especially for incapacitation where the timer is also ticking.

**Exception:** During DNA extraction channeling (4.3), taking damage interrupts the channel (as implemented in `DNAExtractor`). The menu returns to the confirmation state, not closed.

---

## 6. Color Palette & Style Constants

### 6.1 Graft Path Colors (Organic/Visceral)

| Element | Hex | RGB | Usage |
|---------|-----|-----|-------|
| Amber Primary | `#D4A843` | 212, 168, 67 | Titles, part names, key text |
| Burnt Orange | `#CC6644` | 204, 102, 68 | Warnings, permanence indicators |
| Bone White | `#E8D8B8` | 232, 216, 184 | Slot labels, section headers |
| Muted Tan | `#A89070` | 168, 144, 112 | Secondary info, descriptions |
| Panel Background | `rgba(15, 12, 10, 0.92)` | -- | Dark brown-black, near opaque |
| Button Background | `rgba(100, 50, 20, 0.90)` | -- | Dark sienna (confirm) |
| Button Hover | `rgba(140, 70, 30, 0.95)` | -- | Warmer sienna (hover) |
| Danger Red | `#CC4444` | 204, 68, 68 | Incompatibility, loss warnings |
| Positive Green | `#44CC66` | 68, 204, 102 | Stat increases, affinity gains |
| Dark Sienna | `#8B4513` | 139, 69, 19 | Decomposition, corpse decay |

### 6.2 Extract Path Colors (Clinical/Technical)

| Element | Hex | RGB | Usage |
|---------|-----|-----|-------|
| Teal Primary | `#44CCAA` | 68, 204, 170 | Titles, ability names, bars |
| Muted Sea Green | `#78B8A0` | 120, 184, 160 | Secondary info, subtitles |
| Clean White | `#E8E8E8` | 232, 232, 232 | Slot labels, sample names |
| Panel Background | `rgba(8, 18, 16, 0.92)` | -- | Dark blue-green, near opaque |
| Button Background | `rgba(20, 80, 70, 0.90)` | -- | Dark teal (confirm) |
| Button Hover | `rgba(30, 110, 95, 0.95)` | -- | Brighter teal (hover) |
| Timer Calm | `#44CCAA` | 68, 204, 170 | Timer bar > 6s |
| Timer Warning | `#CCAA44` | 204, 170, 68 | Timer bar 4-6s |
| Timer Danger | `#CC4444` | 204, 68, 68 | Timer bar < 4s |
| Prime Tier | `#44CC66` | 68, 204, 102 | Prime DNA quality indicator |
| Degraded Tier | `#CCAA44` | 204, 170, 68 | Degraded DNA quality indicator |

### 6.3 Shared Colors

| Element | Hex | Usage |
|---------|-----|-------|
| Dim Gray | `#666666` | Empty slots, no-change stats, disabled text |
| Mid Gray | `#888888` | Cancel buttons, walk-away text |
| Light Gray | `#CCCCCC` | General body text, descriptions |
| Neutral Button BG | `rgba(40, 40, 40, 0.80)` | Cancel/Leave buttons |
| Close Button (X) | White text on transparent | Top-right corner, all panels |

### 6.4 Font Sizes

| Context | Size | Weight |
|---------|------|--------|
| Panel title | 22-24pt | Bold |
| Panel subtitle | 16-18pt | Normal or Italic |
| Section header | 15-16pt | Bold |
| Body text | 14-15pt | Normal |
| Sub-info / footnotes | 13pt | Normal or Italic |
| Buttons (confirm) | 18pt | Bold |
| Buttons (cancel) | 16pt | Bold |
| Timer text | 13-14pt | Bold |

### 6.5 Panel Sizing

| Panel | Width | Height | Notes |
|-------|-------|--------|-------|
| Part Selection (3.2) | 520px | Dynamic (min 320) | Grows with part count |
| Graft Confirmation (3.3) | 520px | ~540px | Fixed layout |
| Graft Result (3.4) | 480px | ~360px | Smaller -- just results |
| Extract Confirmation (4.2) | 520px | ~480px | Includes timer bar |
| Extract Result (4.4) | 480px | ~380px | Smaller -- just results |
| World prompts (2.x) | 300px | ~80px | Floating, semi-transparent |
| Flash messages (5.x) | 350px | ~70px | Centered, temporary |

---

## 7. Implementation Notes

### 7.1 Refactoring HarvestChoiceUI

The existing `HarvestChoiceUI.cs` at `Assets/Scripts/UI/HarvestChoiceUI.cs` is a working prototype that combines both harvest paths into one panel with generic Graft/Extract buttons. This spec replaces that combined panel with two separate flows:

**New components needed:**

1. **`HarvestPromptUI`** (MonoBehaviour on Player) -- Handles the world-space interact prompts (Section 2). Reads from `InteractController`'s raycast to determine what the player is looking at. Renders via OnGUI.

2. **`GraftMenuUI`** (MonoBehaviour on Player) -- Handles the graft menu flow (Section 3). States: PartSelection, Confirmation, Result. Opened by `CreatureCorpse.Interact()`.

3. **`ExtractMenuUI`** (MonoBehaviour on Player) -- Handles the extract menu flow (Section 4). States: Confirmation, Channeling, Result. Opened when player interacts with an incapacitated creature.

4. **`HarvestFlashUI`** (MonoBehaviour on Player) -- Handles the temporary center-screen flash messages for edge cases (Section 5). Queue-based: multiple flashes display sequentially.

### 7.2 Interaction Routing

The current `CreatureCorpse.Interact()` calls `HarvestChoiceUI.Instance.Show()`. Under the new system:

- **Corpse interaction** (E on `CreatureCorpse`): Opens `GraftMenuUI` with the corpse's drop data. Corpse only offers grafts, never extraction.
- **Incap interaction** (E on incapacitated creature): Opens `ExtractMenuUI` with the creature's DNA data. Incapacitated creatures only offer extraction, never grafts.

This means the routing is clean: the type of interactable determines which menu opens. No branching inside a single menu.

**For incapacitated creatures**, the interaction needs a new component. Currently, `EnemyHealth.IsIncapacitated` is true but the creature has no `IInteractable` component. Options:
- Add a `CreatureIncapInteractable : MonoBehaviour, IInteractable` that is added by `IncapacitationController.HandleIncapacitated()` and removed by `HandleRecovered()`.
- Or: `ExtractMenuUI` opens directly via a check in `InteractController.Interact()` that tests for `EnemyHealth.IsIncapacitated` on the hit target.

The second approach is simpler and consistent with how `DNAExtractor` already works (it sphere-casts for `EnemyHealth` on the enemy layer mask).

### 7.3 Data Flow

**Graft menu needs from CreatureCorpse:**
- `GraftPartSO[] drops` (already stored)
- `string creatureName` (already stored)
- `float despawnTimeRemaining` (needs to be exposed -- add a public property)

**Graft menu needs from AnatomyManager:**
- Current graft in the target slot: `anatomyManager.Grafts.GetGraft(part.slot)`
- Species counts: `anatomyManager.Grafts.GetSpeciesCounts()`
- Affinity check: `anatomyManager.Grafts.GetAffinitySpecies()`
- Incompatibility check: `anatomyManager.Grafts.HasIncompatibilityPenalty()`
- Stat snapshot (before): read from `PlayerMovement` and `PlayerHealth`
- Stat snapshot (after): call `AnatomyManager.GraftPart()`, then read new values

**Extract menu needs from the incapacitated creature:**
- `DNASampleSO` from `CreatureLootTable.dnaSample`
- `DNATier` from `CreatureLootTable.DetermineDNATier(enemyHealth.GetHealthNormalized())`
- Recovery time remaining: from `EnemyHealth` (needs a public `float IncapTimeRemaining` property, or track it externally from the `vulnerableWindowDuration` and time since incapacitation)
- Creature name: `gameObject.name`

**Extract menu needs from AnatomyManager:**
- Current DNA in all slots: `anatomyManager.Suit.GetActiveSlot(i)`, `GetPassiveSlot(i)`
- Unlocked slot counts: `anatomyManager.Suit.UnlockedActiveSlots`, `UnlockedPassiveSlots`
- First available slot (computed by the UI)

### 7.4 OnGUI Style Helper

To avoid repeating `MakeTex()` and style construction in multiple UI components, extract a shared `HarvestUIStyles` static class:

```csharp
public static class HarvestUIStyles
{
    // Graft palette
    public static readonly Color AmberPrimary = new Color32(212, 168, 67, 255);
    public static readonly Color BurntOrange = new Color32(204, 102, 68, 255);
    public static readonly Color BoneWhite = new Color32(232, 216, 184, 255);
    public static readonly Color MutedTan = new Color32(168, 144, 112, 255);
    public static readonly Color GraftPanelBG = new Color(0.06f, 0.047f, 0.039f, 0.92f);

    // Extract palette
    public static readonly Color TealPrimary = new Color32(68, 204, 170, 255);
    public static readonly Color SeaGreen = new Color32(120, 184, 160, 255);
    public static readonly Color CleanWhite = new Color32(232, 232, 232, 255);
    public static readonly Color ExtractPanelBG = new Color(0.031f, 0.071f, 0.063f, 0.92f);

    // Shared
    public static readonly Color DangerRed = new Color32(204, 68, 68, 255);
    public static readonly Color PositiveGreen = new Color32(68, 204, 102, 255);
    public static readonly Color WarningAmber = new Color32(204, 170, 68, 255);
    public static readonly Color DimGray = new Color32(102, 102, 102, 255);

    public static Texture2D MakeTex(int w, int h, Color col) { ... }
}
```

### 7.5 Cursor and Input Management

Both menus free the cursor (`Cursor.lockState = CursorLockMode.None; Cursor.visible = true`) and restore it on close. The existing `HarvestChoiceUI` already does this correctly.

During menu display, the player should NOT be able to:
- Attack (left/right click consumed by menu buttons)
- Interact with other objects (E key consumed by menu)
- Sprint or dodge (optional -- could allow movement for fleeing)

The simplest approach: set a static flag `HarvestMenuOpen` that `AttackController`, `InteractController`, and `PlayerMovement` check before processing input. The `rotatePlayer` look script should also check this flag to prevent camera rotation while the cursor is free.

### 7.6 Timer Synchronization

The recovery timer on the extract menu must exactly match the creature's actual recovery state. The creature's timer runs in `EnemyHealth` via the `RecoveryTimer()` coroutine. The UI needs to read this value every frame.

**Recommended approach:** Add a public property to `EnemyHealth`:
```csharp
public float IncapTimeRemaining { get; private set; }
```
Decrement it in `Update()` when `isIncapacitated` is true. The coroutine can be replaced with this Update-based approach for easier synchronization, or the property can run in parallel as a display value.

### 7.7 Stat Preview Calculation (Non-Destructive)

For the graft confirmation screen (3.3), we need to show stat changes BEFORE actually applying the graft. Two approaches:

**Option A (snapshot + revert):**
1. Snapshot current stats.
2. Call `AnatomyManager.GraftPart()` to apply.
3. Read new stats.
4. Revert by re-grafting the old part (or clearing the slot).
5. Display the diff. Only apply for real when the player confirms.

This is messy and has side effects (events fire twice).

**Option B (calculate preview without applying):**
1. Read the candidate part's `StatModifierData`.
2. Read the current slot's part's `StatModifierData` (if any).
3. Compute the diff: `newMods - oldMods` for each stat.
4. Apply the diff to current effective stats to get the preview.

This is cleaner. The `StatModifierData` struct makes this straightforward:

```csharp
public static StatModifierData Diff(StatModifierData oldMods, StatModifierData newMods)
{
    // Flats: simple subtraction
    // Mults: divide new by old to get the ratio change
    // Apply ratio to current effective stat
}
```

**Recommended: Option B.** The UI calculates the preview independently from the actual anatomy system. The graft is only applied when the player confirms.
