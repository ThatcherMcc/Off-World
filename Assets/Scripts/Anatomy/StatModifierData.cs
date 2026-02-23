using System;

namespace OffWorld.Anatomy
{
    /// <summary>
    /// Flat and multiplicative stat modifiers that can come from grafts, DNA, or power items.
    /// Flats are added first, then multipliers are applied.
    /// </summary>
    [Serializable]
    public struct StatModifierData
    {
        // Flat additions (applied before multipliers)
        public int   maxHPFlat;
        public float walkSpeedFlat;
        public float sprintSpeedFlat;
        public float jumpForceFlat;
        public float dodgeForceFlat;
        public float attackDamageFlat;

        // Multipliers (applied after flats; multiple sources multiply together)
        public float maxHPMult;
        public float walkSpeedMult;
        public float sprintSpeedMult;
        public float jumpForceMult;
        public float dodgeForceMult;
        public float attackDamageMult;
        public float airControlMult;

        // Additive (capped at 0.75)
        public float damageResistance;

        /// <summary>Returns a neutral modifier (no change to any stat).</summary>
        public static StatModifierData Identity => new StatModifierData
        {
            maxHPMult       = 1f,
            walkSpeedMult   = 1f,
            sprintSpeedMult = 1f,
            jumpForceMult   = 1f,
            dodgeForceMult  = 1f,
            attackDamageMult = 1f,
            airControlMult  = 1f,
            damageResistance = 0f
        };

        /// <summary>Combine two modifiers: flats add, multipliers multiply, resistance adds.</summary>
        public static StatModifierData Combine(StatModifierData a, StatModifierData b)
        {
            return new StatModifierData
            {
                maxHPFlat        = a.maxHPFlat + b.maxHPFlat,
                walkSpeedFlat    = a.walkSpeedFlat + b.walkSpeedFlat,
                sprintSpeedFlat  = a.sprintSpeedFlat + b.sprintSpeedFlat,
                jumpForceFlat    = a.jumpForceFlat + b.jumpForceFlat,
                dodgeForceFlat   = a.dodgeForceFlat + b.dodgeForceFlat,
                attackDamageFlat = a.attackDamageFlat + b.attackDamageFlat,

                maxHPMult        = a.maxHPMult * b.maxHPMult,
                walkSpeedMult    = a.walkSpeedMult * b.walkSpeedMult,
                sprintSpeedMult  = a.sprintSpeedMult * b.sprintSpeedMult,
                jumpForceMult    = a.jumpForceMult * b.jumpForceMult,
                dodgeForceMult   = a.dodgeForceMult * b.dodgeForceMult,
                attackDamageMult = a.attackDamageMult * b.attackDamageMult,
                airControlMult   = a.airControlMult * b.airControlMult,

                damageResistance = a.damageResistance + b.damageResistance
            };
        }
    }
}
