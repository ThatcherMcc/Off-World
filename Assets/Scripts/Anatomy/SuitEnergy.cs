using UnityEngine;

namespace OffWorld.Anatomy
{
    /// <summary>
    /// Manages the DNA suit's energy pool. DNA active abilities consume energy.
    /// Energy is earned through combat and creature interaction, not passive regen.
    /// </summary>
    public class SuitEnergy : MonoBehaviour
    {
        [Header("Energy Pool")]
        [SerializeField] private float maxEnergy = 100f;
        [SerializeField] private float currentEnergy = 100f;

        [Header("Passive Drain")]
        [Tooltip("Energy drained per second while any DNA is loaded.")]
        [SerializeField] private float passiveDrainRate = 2f;

        [Header("Combat Recharge")]
        [Tooltip("Energy gained per melee hit on an enemy.")]
        [SerializeField] private float energyPerMeleeHit = 8f;
        [Tooltip("Energy burst on successful incapacitation.")]
        [SerializeField] private float energyOnIncapacitate = 30f;
        [Tooltip("Energy gained on creature harvest.")]
        [SerializeField] private float energyOnHarvest = 5f;

        [Header("Recovery Mode")]
        [Tooltip("When energy hits 0, drain pauses until this threshold is reached.")]
        [SerializeField] private float recoveryThreshold = 10f;
        private bool inRecoveryMode;

        private bool hasDNALoaded;

        public float CurrentEnergy => currentEnergy;
        public float MaxEnergy => maxEnergy;
        public float EnergyNormalized => maxEnergy > 0 ? currentEnergy / maxEnergy : 0f;
        public bool InRecoveryMode => inRecoveryMode;

        /// <summary>Set whether any DNA is loaded in the suit (controls passive drain).</summary>
        public void SetDNALoaded(bool loaded)
        {
            hasDNALoaded = loaded;
        }

        private void Update()
        {
            if (!hasDNALoaded) return;

            // Recovery mode: no drain until threshold
            if (inRecoveryMode)
            {
                if (currentEnergy >= recoveryThreshold)
                    inRecoveryMode = false;
                return;
            }

            // Passive drain
            currentEnergy -= passiveDrainRate * Time.deltaTime;
            if (currentEnergy <= 0f)
            {
                currentEnergy = 0f;
                inRecoveryMode = true;
            }
        }

        /// <summary>Try to spend energy. Returns true if successful.</summary>
        public bool TrySpend(float amount)
        {
            if (inRecoveryMode || currentEnergy < amount)
                return false;

            currentEnergy -= amount;
            if (currentEnergy <= 0f)
            {
                currentEnergy = 0f;
                inRecoveryMode = true;
            }
            return true;
        }

        /// <summary>Add energy (from combat, harvest, etc). Capped at max.</summary>
        public void AddEnergy(float amount)
        {
            currentEnergy = Mathf.Min(currentEnergy + amount, maxEnergy);
        }

        /// <summary>Call when player lands a melee hit.</summary>
        public void OnMeleeHit() => AddEnergy(energyPerMeleeHit);

        /// <summary>Call when player successfully incapacitates a creature.</summary>
        public void OnIncapacitate() => AddEnergy(energyOnIncapacitate);

        /// <summary>Call when player harvests a body part.</summary>
        public void OnHarvest() => AddEnergy(energyOnHarvest);

        /// <summary>Increase max energy (suit upgrades).</summary>
        public void UpgradeMaxEnergy(float bonus)
        {
            maxEnergy += bonus;
            currentEnergy = Mathf.Min(currentEnergy + bonus, maxEnergy);
        }
    }
}
