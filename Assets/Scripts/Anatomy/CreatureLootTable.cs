using UnityEngine;

namespace OffWorld.Anatomy
{
    /// <summary>
    /// Attach to any creature prefab to define what it drops on death (graft parts)
    /// and what DNA it yields on incapacitation.
    /// </summary>
    public class CreatureLootTable : MonoBehaviour
    {
        [Header("Creature Identity")]
        public CreatureSpecies species;

        [Header("Graft Drops (on kill)")]
        [Tooltip("Each entry has a GraftPartSO and inherits the drop rate from the SO.")]
        public GraftPartSO[] possibleGraftDrops;

        [Header("DNA Yield (on incapacitate)")]
        public DNASampleSO dnaSample;

        [Header("Incapacitation Settings")]
        [Range(0.05f, 0.4f)]
        [Tooltip("HP fraction at which this creature becomes incapacitated instead of dying.")]
        public float incapacitateThreshold = 0.15f;

        [Tooltip("Seconds the creature stays in the incapacitated state before recovering.")]
        public float vulnerableWindowDuration = 12f;

        [Header("Drop Spawn")]
        [Tooltip("Offset from creature position where drops spawn.")]
        public Vector3 dropSpawnOffset = Vector3.up;

        [Tooltip("Prefab for the physical pickup that appears when parts drop.")]
        public GameObject partDropPickupPrefab;

        /// <summary>
        /// Roll for graft drops. Returns an array of parts that actually drop based on their drop rates.
        /// </summary>
        public GraftPartSO[] RollDrops()
        {
            var drops = new System.Collections.Generic.List<GraftPartSO>();
            foreach (var part in possibleGraftDrops)
            {
                if (part != null && Random.value <= part.dropRate)
                {
                    drops.Add(part);
                }
            }
            return drops.ToArray();
        }

        /// <summary>
        /// Determines DNA tier based on how precisely the creature was incapacitated.
        /// If the creature's remaining HP fraction is within the "sweet spot" (lower half of
        /// the threshold window), it yields Prime DNA. Otherwise, Degraded.
        /// </summary>
        public DNATier DetermineDNATier(float currentHealthNormalized)
        {
            float sweetSpot = incapacitateThreshold * 0.5f;
            if (currentHealthNormalized <= sweetSpot)
                return DNATier.Prime;
            return DNATier.Degraded;
        }
    }
}
