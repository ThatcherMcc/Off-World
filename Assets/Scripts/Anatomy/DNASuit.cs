using System;
using System.Linq;
using UnityEngine;

namespace OffWorld.Anatomy
{
    /// <summary>
    /// Manages the DNA suit system: active slots (abilities) and passive slots (stat bonuses).
    /// DNA can be freely swapped unlike permanent grafts.
    /// </summary>
    [Serializable]
    public class DNASuit
    {
        [Header("Active Slots (keys 1-4)")]
        [SerializeField] private DNASampleSO[] activeSlots = new DNASampleSO[4];

        [Header("Passive Slots")]
        [SerializeField] private DNASampleSO[] passiveSlots = new DNASampleSO[2];

        [Header("Slot Unlocking")]
        [SerializeField] private int unlockedActiveSlots = 2;  // Start with 2, unlock more via progression
        [SerializeField] private int unlockedPassiveSlots = 1; // Start with 1

        public event Action OnDNAChanged;

        // --- Active Slots ---

        public DNASampleSO GetActiveSlot(int index)
        {
            if (index < 0 || index >= activeSlots.Length) return null;
            return activeSlots[index];
        }

        /// <summary>Load DNA into an active slot. Returns the previous DNA (null if empty).</summary>
        public DNASampleSO SetActiveSlot(int index, DNASampleSO dna)
        {
            if (index < 0 || index >= unlockedActiveSlots) return null;
            var previous = activeSlots[index];
            activeSlots[index] = dna;
            OnDNAChanged?.Invoke();
            return previous;
        }

        /// <summary>Remove DNA from an active slot.</summary>
        public DNASampleSO ClearActiveSlot(int index)
        {
            return SetActiveSlot(index, null);
        }

        // --- Passive Slots ---

        public DNASampleSO GetPassiveSlot(int index)
        {
            if (index < 0 || index >= passiveSlots.Length) return null;
            return passiveSlots[index];
        }

        /// <summary>Load DNA into a passive slot. Returns the previous DNA.</summary>
        public DNASampleSO SetPassiveSlot(int index, DNASampleSO dna)
        {
            if (index < 0 || index >= unlockedPassiveSlots) return null;
            var previous = passiveSlots[index];
            passiveSlots[index] = dna;
            OnDNAChanged?.Invoke();
            return previous;
        }

        // --- Stat Modifiers ---

        [Header("Suit Efficiency")]
        [Range(0f, 1f)]
        [Tooltip("DNA suit can't replicate biological effects perfectly. Scales all DNA buffs.")]
        [SerializeField] private float suitEfficiency = 0.5f;

        /// <summary>
        /// Compute combined passive stat modifiers from ALL loaded DNA (active + passive slots).
        /// DNA buffs are always active but weaker than grafts due to suit efficiency scaling.
        /// </summary>
        public StatModifierData GetPassiveModifiers()
        {
            var combined = StatModifierData.Identity;

            // Gather from all active slots
            for (int i = 0; i < unlockedActiveSlots && i < activeSlots.Length; i++)
            {
                if (activeSlots[i] != null)
                {
                    combined = StatModifierData.Combine(combined, activeSlots[i].GetEffectivePassiveModifiers());
                }
            }

            // Gather from all passive slots
            for (int i = 0; i < unlockedPassiveSlots && i < passiveSlots.Length; i++)
            {
                if (passiveSlots[i] != null)
                {
                    combined = StatModifierData.Combine(combined, passiveSlots[i].GetEffectivePassiveModifiers());
                }
            }

            // Apply suit efficiency — DNA can't match real biology
            combined = ApplySuitEfficiency(combined);
            return combined;
        }

        /// <summary>Scale stat modifiers by suit efficiency (flats scaled, mults lerped toward 1).</summary>
        private StatModifierData ApplySuitEfficiency(StatModifierData mods)
        {
            float e = suitEfficiency;
            mods.maxHPFlat        = Mathf.RoundToInt(mods.maxHPFlat * e);
            mods.walkSpeedFlat    *= e;
            mods.sprintSpeedFlat  *= e;
            mods.jumpForceFlat    *= e;
            mods.dodgeForceFlat   *= e;
            mods.attackDamageFlat *= e;
            mods.damageResistance *= e;
            mods.maxHPMult        = Mathf.Lerp(1f, mods.maxHPMult, e);
            mods.walkSpeedMult    = Mathf.Lerp(1f, mods.walkSpeedMult, e);
            mods.sprintSpeedMult  = Mathf.Lerp(1f, mods.sprintSpeedMult, e);
            mods.jumpForceMult    = Mathf.Lerp(1f, mods.jumpForceMult, e);
            mods.dodgeForceMult   = Mathf.Lerp(1f, mods.dodgeForceMult, e);
            mods.attackDamageMult = Mathf.Lerp(1f, mods.attackDamageMult, e);
            mods.airControlMult   = Mathf.Lerp(1f, mods.airControlMult, e);
            return mods;
        }

        // --- Queries ---

        /// <summary>True if any slot (active or passive) has DNA loaded.</summary>
        public bool HasAnyDNA()
        {
            return activeSlots.Any(s => s != null) || passiveSlots.Any(s => s != null);
        }

        /// <summary>Count of unique species across all loaded DNA.</summary>
        public int UniqueSpeciesCount()
        {
            return activeSlots.Concat(passiveSlots)
                .Where(s => s != null)
                .Select(s => s.species)
                .Distinct()
                .Count();
        }

        // --- Slot Unlocking ---

        public int UnlockedActiveSlots => unlockedActiveSlots;
        public int UnlockedPassiveSlots => unlockedPassiveSlots;

        public void UnlockActiveSlot()
        {
            if (unlockedActiveSlots < activeSlots.Length)
                unlockedActiveSlots++;
        }

        public void UnlockPassiveSlot()
        {
            if (unlockedPassiveSlots < passiveSlots.Length)
                unlockedPassiveSlots++;
        }
    }
}
