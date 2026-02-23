using UnityEngine;

namespace OffWorld.Anatomy
{
    /// <summary>
    /// ScriptableObject defining a harvestable body part that can be grafted onto the player.
    /// Create via Assets > Create > Off-World > Graft Part.
    /// </summary>
    [CreateAssetMenu(fileName = "NewGraftPart", menuName = "Off-World/Graft Part")]
    public class GraftPartSO : ScriptableObject
    {
        [Header("Identity")]
        public string partName;
        [TextArea] public string description;
        public BodySlot slot;
        public CreatureSpecies species;
        public PartTier tier;
        public Sprite icon;

        [Header("Visual")]
        [Tooltip("Mesh prefab to attach to the player model when this graft is equipped.")]
        public GameObject visualPrefab;

        [Header("Stat Modifiers")]
        public StatModifierData modifiers = StatModifierData.Identity;

        [Header("Active Ability (optional)")]
        [Tooltip("If this graft grants an active ability, reference its SO here.")]
        public GraftAbilitySO activeAbility;

        [Header("Drop Settings")]
        [Range(0f, 1f)]
        public float dropRate = 1f;
    }
}
