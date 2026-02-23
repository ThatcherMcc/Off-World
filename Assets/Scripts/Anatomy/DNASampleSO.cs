using UnityEngine;

namespace OffWorld.Anatomy
{
    /// <summary>
    /// ScriptableObject defining a DNA sample extracted from an incapacitated creature.
    /// DNA can be loaded into suit slots for active/passive abilities.
    /// Create via Assets > Create > Off-World > DNA Sample.
    /// </summary>
    [CreateAssetMenu(fileName = "NewDNASample", menuName = "Off-World/DNA Sample")]
    public class DNASampleSO : ScriptableObject
    {
        [Header("Identity")]
        public string sampleName;
        public CreatureSpecies species;
        public DNATier tier;
        public Sprite icon;

        [Header("Active Ability")]
        [Tooltip("Full class name of the MonoBehaviour that implements this DNA's active ability.")]
        public string activeAbilityComponentType;
        public float energyCost = 20f;
        public float cooldown = 5f;
        public float duration; // 0 = instant

        [Header("Passive Bonus")]
        [Tooltip("Stat modifiers applied when this DNA is in a passive suit slot.")]
        public StatModifierData passiveModifiers = StatModifierData.Identity;

        [Header("Tier Scaling")]
        [Range(0f, 1f)]
        [Tooltip("Degraded DNA has its effects multiplied by this factor.")]
        public float degradedPowerMultiplier = 0.7f;

        /// <summary>Returns the effective modifiers accounting for DNA tier.</summary>
        public StatModifierData GetEffectivePassiveModifiers()
        {
            if (tier == DNATier.Prime)
                return passiveModifiers;

            // Scale down flats for degraded DNA
            float m = degradedPowerMultiplier;
            var d = passiveModifiers;
            d.maxHPFlat        = Mathf.RoundToInt(d.maxHPFlat * m);
            d.walkSpeedFlat    *= m;
            d.sprintSpeedFlat  *= m;
            d.jumpForceFlat    *= m;
            d.dodgeForceFlat   *= m;
            d.attackDamageFlat *= m;
            d.damageResistance *= m;
            // Multipliers: lerp toward 1.0 (neutral)
            d.maxHPMult        = Mathf.Lerp(1f, d.maxHPMult, m);
            d.walkSpeedMult    = Mathf.Lerp(1f, d.walkSpeedMult, m);
            d.sprintSpeedMult  = Mathf.Lerp(1f, d.sprintSpeedMult, m);
            d.jumpForceMult    = Mathf.Lerp(1f, d.jumpForceMult, m);
            d.dodgeForceMult   = Mathf.Lerp(1f, d.dodgeForceMult, m);
            d.attackDamageMult = Mathf.Lerp(1f, d.attackDamageMult, m);
            d.airControlMult   = Mathf.Lerp(1f, d.airControlMult, m);
            return d;
        }
    }
}
