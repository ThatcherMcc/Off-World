using UnityEngine;
using OffWorld.Anatomy;

/// <summary>
/// Attach to creature prefabs alongside EnemyHealth and CreatureLootTable.
/// Handles the downed visual state (disabling AI, playing downed animation),
/// the unified harvest choice (extract DNA or harvest parts), and spawning
/// loot drops on death.
/// </summary>
public class IncapacitationController : MonoBehaviour
{
    private EnemyHealth enemyHealth;
    private CreatureLootTable lootTable;
    private IEnemy enemyAI;

    [Header("Downed State")]
    [Tooltip("If true, the creature plays a downed animation when collapsed.")]
    [SerializeField] private bool useDownedAnimation = true;
    private Animator animator;
    private Rigidbody rb;
    private MonoBehaviour aiComponent; // The actual AI script (FrogBT, WolfBT, etc.)
    private RigidbodyConstraints savedConstraints;
    private bool hadRootMotion;
    private static readonly int DownedHash = Animator.StringToHash("DOWNED");
    private static readonly int RecoverHash = Animator.StringToHash("RECOVER");

    [Header("Corpse Settings")]
    [Tooltip("How long the dead body stays in the world before despawning.")]
    [SerializeField] private float corpseDespawnTime = 60f;

    // Tracks whether the kill came from the harvest menu (clean kill = full quality parts)
    private bool cleanKill;

    private void Awake()
    {
        enemyHealth = GetComponent<EnemyHealth>();
        lootTable = GetComponent<CreatureLootTable>();
        enemyAI = GetComponent<IEnemy>();
        aiComponent = enemyAI as MonoBehaviour;
        animator = GetComponentInChildren<Animator>();
        rb = GetComponent<Rigidbody>();
    }

    private void OnEnable()
    {
        if (enemyHealth != null)
        {
            enemyHealth.OnIncapacitated += HandleIncapacitated;
            enemyHealth.OnRecovered += HandleRecovered;
            enemyHealth.OnDied += HandleDied;
        }
    }

    private void OnDisable()
    {
        if (enemyHealth != null)
        {
            enemyHealth.OnIncapacitated -= HandleIncapacitated;
            enemyHealth.OnRecovered -= HandleRecovered;
            enemyHealth.OnDied -= HandleDied;
        }
    }

    private void HandleIncapacitated(float healthFraction)
    {
        // Disable AI so the creature stops attacking
        if (enemyAI != null)
            enemyAI.EnableAI(false);

        // Disable the AI script entirely so its Update() can't run
        if (aiComponent != null)
            aiComponent.enabled = false;

        // Freeze all rotation via Rigidbody constraints
        if (rb != null)
        {
            savedConstraints = rb.constraints;
            rb.constraints = savedConstraints | RigidbodyConstraints.FreezeRotation;
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        // Disable root motion so the downed animation can't rotate the creature
        if (animator != null)
        {
            hadRootMotion = animator.applyRootMotion;
            animator.applyRootMotion = false;
        }

        // Play downed animation if available
        if (useDownedAnimation && animator != null)
            animator.SetTrigger(DownedHash);

#if UNITY_EDITOR
        Debug.Log($"[IncapacitationController] {gameObject.name} collapsed. " +
                  $"Player can harvest for the next {lootTable?.vulnerableWindowDuration ?? 12f}s.");
#endif
    }

    private void HandleRecovered()
    {
        // Re-enable the AI script
        if (aiComponent != null)
            aiComponent.enabled = true;

        // Re-enable AI logic
        if (enemyAI != null)
            enemyAI.EnableAI(true);

        // Restore Rigidbody constraints and root motion
        if (rb != null)
            rb.constraints = savedConstraints;
        if (animator != null)
            animator.applyRootMotion = hadRootMotion;

        // Play recovery animation
        if (useDownedAnimation && animator != null)
            animator.SetTrigger(RecoverHash);

        // If DNA was extracted while downed, flee now that the creature woke up
        if (enemyHealth != null && enemyHealth.HasBeenExtracted)
            StartFlee();
    }

    private void HandleDied()
    {
        if (lootTable == null) return;

        // Determine if parts should be degraded:
        // - cleanKill = true (from harvest menu HARVEST PARTS) → full quality
        // - hasBeenExtracted = true (extracted then chased down) → degraded
        // - neither (creature hit while downed without menu) → degraded (messy kill)
        bool isDegraded = !cleanKill || enemyHealth.HasBeenExtracted;

        var drops = lootTable.RollDrops();

        var corpse = gameObject.AddComponent<CreatureCorpse>();
        float healthNorm = enemyHealth != null ? enemyHealth.GetHealthNormalized() : 0f;
        corpse.Initialize(drops, lootTable.dnaSample, healthNorm, corpseDespawnTime, isDegraded);

        // Reset for safety
        cleanKill = false;
    }

    // ---- Harvest Menu Entry Points ----

    /// <summary>
    /// Called by HarvestChoiceMenuUI when the player chooses HARVEST PARTS.
    /// Kills the creature cleanly — full quality parts.
    /// </summary>
    public void HarvestKill()
    {
        cleanKill = true;
        enemyHealth.ForceKill();
    }

    /// <summary>
    /// Called by HarvestChoiceMenuUI when the player chooses EXTRACT DNA.
    /// DNA goes to base storage via drone. Creature stays downed until the
    /// recovery timer expires naturally, then gets up and flees.
    /// </summary>
    public void MercifulExtract()
    {
        enemyHealth.MarkExtracted();

        // Add DNA to base storage immediately (drone is cosmetic)
        if (lootTable.dnaSample != null && BaseStorage.Instance != null)
            BaseStorage.Instance.AddSample(lootTable.dnaSample);

        // Dispatch drone visual — use a temporary marker so the drone doesn't
        // hide/destroy the creature itself (the creature stays downed)
        var config = AnatomyManager.Instance != null ? AnatomyManager.Instance.DroneConfig : null;
        if (config != null)
        {
            var marker = new GameObject("ExtractDroneMarker");
            marker.transform.position = transform.position + Vector3.up * 0.5f;
            DronePickup.Dispatch(marker, config);
        }

        // Creature stays downed — HandleRecovered() will trigger flee when
        // the incap timer expires naturally in EnemyHealth.Update()

#if UNITY_EDITOR
        Debug.Log($"[IncapacitationController] {gameObject.name} extracted mercifully. DNA sent to base. Creature will flee when timer expires.");
#endif
    }

    private void StartFlee()
    {
        var fleeable = aiComponent as IFleeable;
        if (fleeable != null)
        {
            fleeable.StartFlee(10f);
        }
        else
        {
            // Fallback: destroy after delay if no flee behavior implemented
            Destroy(gameObject, 10f);
        }
    }

    private void SpawnPartDrop(GraftPartSO part)
    {
        if (lootTable.partDropPickupPrefab == null)
        {
#if UNITY_EDITOR
            Debug.LogWarning($"[IncapacitationController] No partDropPickupPrefab on {gameObject.name}. " +
                             $"Cannot spawn drop for '{part.partName}'.");
#endif
            return;
        }

        Vector3 spawnPos = transform.position + lootTable.dropSpawnOffset;
        var dropObj = Instantiate(lootTable.partDropPickupPrefab, spawnPos, Quaternion.identity);

        var pickup = dropObj.GetComponent<PartDropPickup>();
        if (pickup != null)
        {
            pickup.Initialize(part);
        }
    }
}
