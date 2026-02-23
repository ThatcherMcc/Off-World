using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace OffWorld.Anatomy
{
    /// <summary>
    /// Manages the 6 body graft slots on the player.
    /// Handles equipping, replacing, species affinity, and incompatibility.
    /// </summary>
    [Serializable]
    public class GraftSystem
    {
        [SerializeField] private GraftPartSO[] slots = new GraftPartSO[6]; // indexed by BodySlot enum

        // Affinity: 3+ parts from same species grants a bonus
        private const int AffinityThreshold = 3;
        // Incompatibility: 4+ different species causes a penalty
        private const int IncompatibilityThreshold = 4;

        public event Action OnGraftsChanged;

        /// <summary>Get the graft in a specific slot. Null if empty.</summary>
        public GraftPartSO GetGraft(BodySlot slot) => slots[(int)slot];

        /// <summary>Get all equipped grafts (non-null).</summary>
        public IEnumerable<GraftPartSO> GetAllGrafts() => slots.Where(s => s != null);

        /// <summary>
        /// Equip a graft part into its designated slot.
        /// Returns the previous graft in that slot (null if was empty). The old graft is destroyed.
        /// </summary>
        public GraftPartSO EquipGraft(GraftPartSO part)
        {
            if (part == null) return null;

            int index = (int)part.slot;
            GraftPartSO previous = slots[index];
            slots[index] = part;
            OnGraftsChanged?.Invoke();
            return previous;
        }

        /// <summary>
        /// Compute the combined stat modifiers from all equipped grafts.
        /// </summary>
        public StatModifierData GetCombinedModifiers()
        {
            var combined = StatModifierData.Identity;
            foreach (var graft in GetAllGrafts())
            {
                combined = StatModifierData.Combine(combined, graft.modifiers);
            }
            return combined;
        }

        /// <summary>
        /// Returns the species that has affinity (3+ parts), or null if none qualifies.
        /// </summary>
        public CreatureSpecies? GetAffinitySpecies()
        {
            var counts = GetSpeciesCounts();
            foreach (var kvp in counts)
            {
                if (kvp.Value >= AffinityThreshold)
                    return kvp.Key;
            }
            return null;
        }

        /// <summary>
        /// Returns true if the player has 4+ different non-Apex species equipped,
        /// triggering the biological incompatibility penalty.
        /// </summary>
        public bool HasIncompatibilityPenalty()
        {
            var uniqueSpecies = new HashSet<CreatureSpecies>();
            foreach (var graft in GetAllGrafts())
            {
                // Apex tier parts don't count toward incompatibility
                if (graft.tier == PartTier.Apex) continue;
                uniqueSpecies.Add(graft.species);
            }
            return uniqueSpecies.Count >= IncompatibilityThreshold;
        }

        /// <summary>Count of parts per species.</summary>
        public Dictionary<CreatureSpecies, int> GetSpeciesCounts()
        {
            var counts = new Dictionary<CreatureSpecies, int>();
            foreach (var graft in GetAllGrafts())
            {
                if (!counts.ContainsKey(graft.species))
                    counts[graft.species] = 0;
                counts[graft.species]++;
            }
            return counts;
        }

        /// <summary>How many slots are filled.</summary>
        public int EquippedCount => GetAllGrafts().Count();
    }
}
