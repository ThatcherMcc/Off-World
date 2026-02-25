using UnityEngine;

namespace OffWorld.Anatomy
{
    /// <summary>
    /// Attach to any creature prefab to define what it drops on death (graft parts)
    /// and what DNA it yields when harvested.
    /// </summary>
    public class CreatureLootTable : MonoBehaviour
    {
        [Header("Creature Identity")]
        public CreatureSpecies species;

        [Header("Graft Drops (on kill)")]
        [Tooltip("Each entry has a GraftPartSO and inherits the drop rate from the SO.")]
        public GraftPartSO[] possibleGraftDrops;

        [Header("DNA Yield (on extract)")]
        public DNASampleSO dnaSample;

        [Header("Downed State")]
        [Tooltip("Seconds the creature stays downed before recovering.")]
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
    }
}
