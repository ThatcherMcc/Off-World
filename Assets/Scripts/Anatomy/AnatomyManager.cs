using UnityEngine;

namespace OffWorld.Anatomy
{
    /// <summary>
    /// Central controller for the player's anatomy system.
    /// Lives on the Player GameObject. Manages grafts, DNA suit, and applies
    /// combined stat modifiers to PlayerMovement and PlayerHealth.
    /// </summary>
    public class AnatomyManager : MonoBehaviour
    {
        public static AnatomyManager Instance { get; private set; }

        [Header("Sub-Systems")]
        [SerializeField] private GraftSystem graftSystem = new GraftSystem();
        [SerializeField] private DNASuit dnaSuit = new DNASuit();
        private SuitEnergy suitEnergy;

        [Header("Drone Pickup")]
        [SerializeField] private DronePickupConfig droneConfig;

        [Header("Base Stats (cached on Start)")]
        private float baseWalkSpeed;
        private float baseSprintSpeed;
        private float baseJumpForce;
        private float baseDodgeForce;
        private float baseAirSpeedMultiplier;
        private int   baseMaxHealth;

        [Header("Incompatibility Penalty")]
        [SerializeField] private float incompatibilityHPDrainPerSecond = 0.1f; // 1 HP per 10s
        [SerializeField] private float incompatibilityMaxHPPenalty = 0.1f; // -10% max HP

        // References
        private PlayerMovement playerMovement;
        private PlayerHealth playerHealth;

        // Public accessors
        public GraftSystem Grafts => graftSystem;
        public DNASuit Suit => dnaSuit;
        public SuitEnergy Energy => suitEnergy;
        public DronePickupConfig DroneConfig => droneConfig;

        private void Awake()
        {
            Instance = this;

            playerMovement = GetComponent<PlayerMovement>();
            playerHealth = GetComponent<PlayerHealth>();
            suitEnergy = GetComponent<SuitEnergy>();

            if (suitEnergy == null)
                suitEnergy = gameObject.AddComponent<SuitEnergy>();
        }

        private void Start()
        {
            // Cache base stats before any modifications
            baseWalkSpeed = playerMovement.walkSpeed;
            baseSprintSpeed = playerMovement.sprintSpeed;
            baseJumpForce = playerMovement.jumpForce;
            baseDodgeForce = playerMovement.DodgeForce;
            baseAirSpeedMultiplier = playerMovement.airSpeedMultiplier;
            baseMaxHealth = playerHealth.maxHealth;

            // Listen for changes
            graftSystem.OnGraftsChanged += RecalculateStats;
            dnaSuit.OnDNAChanged += RecalculateStats;
            dnaSuit.OnDNAChanged += UpdateSuitEnergyState;

            RecalculateStats();
            UpdateSuitEnergyState();
        }

        private void Update()
        {
            // Apply incompatibility HP drain
            if (graftSystem.HasIncompatibilityPenalty())
            {
                // Slow passive damage: 1 HP per 10 seconds
                // We accumulate fractionally; PlayerHealth.PlayerTakeDMG expects int,
                // so we track it here and apply when it reaches 1.
                incompatibilityDrainAccumulator += incompatibilityHPDrainPerSecond * Time.deltaTime;
                if (incompatibilityDrainAccumulator >= 1f)
                {
                    int dmg = Mathf.FloorToInt(incompatibilityDrainAccumulator);
                    incompatibilityDrainAccumulator -= dmg;
                    playerHealth.PlayerTakeDMG(dmg);
                }
            }
        }

        private float incompatibilityDrainAccumulator;

        /// <summary>
        /// Recalculate all player stats from base + grafts + DNA passives.
        /// Called whenever grafts or DNA change.
        /// </summary>
        public void RecalculateStats()
        {
            // Combine graft modifiers
            var graftMods = graftSystem.GetCombinedModifiers();
            // Combine DNA passive modifiers
            var dnaMods = dnaSuit.GetPassiveModifiers();
            // Stack them: grafts first, then DNA
            var combined = StatModifierData.Combine(graftMods, dnaMods);

            // Apply incompatibility penalty to max HP multiplier
            float incompatPenalty = graftSystem.HasIncompatibilityPenalty() ? (1f - incompatibilityMaxHPPenalty) : 1f;

            // Calculate final stats: (base + flat) * mult
            playerMovement.walkSpeed = (baseWalkSpeed + combined.walkSpeedFlat) * combined.walkSpeedMult;
            playerMovement.sprintSpeed = (baseSprintSpeed + combined.sprintSpeedFlat) * combined.sprintSpeedMult;
            playerMovement.jumpForce = (baseJumpForce + combined.jumpForceFlat) * combined.jumpForceMult;
            playerMovement.DodgeForce = (baseDodgeForce + combined.dodgeForceFlat) * combined.dodgeForceMult;
            playerMovement.airSpeedMultiplier = baseAirSpeedMultiplier * combined.airControlMult;

            int newMaxHP = Mathf.RoundToInt((baseMaxHealth + combined.maxHPFlat) * combined.maxHPMult * incompatPenalty);
            playerHealth.maxHealth = Mathf.Max(1, newMaxHP);

#if UNITY_EDITOR
            Debug.Log($"[AnatomyManager] Stats recalculated. Walk: {playerMovement.walkSpeed:F1}, " +
                      $"Sprint: {playerMovement.sprintSpeed:F1}, Jump: {playerMovement.jumpForce:F1}, " +
                      $"MaxHP: {playerHealth.maxHealth}, Resist: {combined.damageResistance:P0}, " +
                      $"Affinity: {graftSystem.GetAffinitySpecies()?.ToString() ?? "None"}, " +
                      $"Incompatible: {graftSystem.HasIncompatibilityPenalty()}");
#endif
        }

        private void UpdateSuitEnergyState()
        {
            suitEnergy.SetDNALoaded(dnaSuit.HasAnyDNA());
        }

        // --- Public API for other systems ---

        /// <summary>Graft a body part onto the player. Returns the replaced part (null if slot was empty).</summary>
        public GraftPartSO GraftPart(GraftPartSO part)
        {
            var replaced = graftSystem.EquipGraft(part);
#if UNITY_EDITOR
            Debug.Log($"[AnatomyManager] Grafted '{part.partName}' into {part.slot}. " +
                      (replaced != null ? $"Replaced '{replaced.partName}'." : "Slot was empty."));
#endif
            return replaced;
        }

        /// <summary>Load DNA into an active suit slot.</summary>
        public DNASampleSO LoadActiveDNA(int slotIndex, DNASampleSO dna)
        {
            return dnaSuit.SetActiveSlot(slotIndex, dna);
        }

        /// <summary>Load DNA into an organ suit slot.</summary>
        public DNASampleSO LoadOrganDNA(int slotIndex, DNASampleSO dna)
        {
            return dnaSuit.SetOrganSlot(slotIndex, dna);
        }

        /// <summary>Get the current damage resistance (0-0.75) from all sources.</summary>
        public float GetDamageResistance()
        {
            var graftMods = graftSystem.GetCombinedModifiers();
            var dnaMods = dnaSuit.GetPassiveModifiers();
            float total = graftMods.damageResistance + dnaMods.damageResistance;
            return Mathf.Clamp(total, 0f, 0.75f);
        }

        private void OnDestroy()
        {
            if (graftSystem != null)
                graftSystem.OnGraftsChanged -= RecalculateStats;
            if (dnaSuit != null)
            {
                dnaSuit.OnDNAChanged -= RecalculateStats;
                dnaSuit.OnDNAChanged -= UpdateSuitEnergyState;
            }
        }
    }
}
