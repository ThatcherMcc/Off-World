using UnityEngine;
using OffWorld.Anatomy;

/// <summary>
/// Attach to creature prefabs alongside EnemyHealth and CreatureLootTable.
/// Handles the incapacitation visual state (disabling AI, playing downed animation)
/// and spawning loot drops on death.
/// </summary>
public class IncapacitationController : MonoBehaviour
{
    private EnemyHealth enemyHealth;
    private CreatureLootTable lootTable;
    private IEnemy enemyAI;

    [Header("Incapacitated State")]
    [Tooltip("If true, the creature is visually collapsed/downed when incapacitated.")]
    [SerializeField] private bool useDownedAnimation = true;
    private Animator animator;
    private Rigidbody rb;
    private MonoBehaviour aiComponent; // The actual AI script (FrogBT, WolfBT, etc.)
    private RigidbodyConstraints savedConstraints;
    private bool hadRootMotion;
    private static readonly int DownedHash = Animator.StringToHash("DOWNED");
    private static readonly int RecoverHash = Animator.StringToHash("RECOVER");

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
        Debug.Log($"[IncapacitationController] {gameObject.name} is now incapacitated. " +
                  $"Player can extract DNA for the next {lootTable?.vulnerableWindowDuration ?? 12f}s.");
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
    }

    [Header("Corpse Settings")]
    [Tooltip("How long the dead body stays in the world before despawning.")]
    [SerializeField] private float corpseDespawnTime = 60f;

    private void HandleDied()
    {
        if (lootTable == null) return;

        // Roll for graft part drops
        var drops = lootTable.RollDrops();

        // Convert creature into an interactable ragdoll corpse
        var corpse = gameObject.AddComponent<CreatureCorpse>();
        float healthNorm = enemyHealth != null ? enemyHealth.GetHealthNormalized() : 0f;
        corpse.Initialize(drops, lootTable.dnaSample, healthNorm, corpseDespawnTime);
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

        // Configure the pickup
        var pickup = dropObj.GetComponent<PartDropPickup>();
        if (pickup != null)
        {
            pickup.Initialize(part);
        }
    }
}
