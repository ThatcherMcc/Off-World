using UnityEngine;
using OffWorld.Anatomy;

/// <summary>
/// Added to a creature when it dies. Converts it into an interactable ragdoll corpse.
/// The player walks up and presses E to collect all remaining parts — a drone
/// flies in and transports them to base storage.
/// Parts may be degraded if the creature was previously extracted from or killed messily.
/// The body despawns after a configurable time.
/// </summary>
public class CreatureCorpse : MonoBehaviour, IInteractable
{
    [Header("Despawn")]
    [SerializeField] private float despawnTime = 60f;

    // Harvest data stored from the death event
    private GraftPartSO[] drops;
    private string creatureName;
    private float spawnTime;
    private bool isDegraded;

    /// <summary>How many seconds until this corpse despawns.</summary>
    public float DespawnTimeRemaining => Mathf.Max(0f, despawnTime - (Time.time - spawnTime));

    /// <summary>Creature display name.</summary>
    public string CreatureName => creatureName;

    /// <summary>Whether the parts from this corpse are degraded quality.</summary>
    public bool IsDegraded => isDegraded;

    /// <summary>How many harvestable parts remain.</summary>
    public int DropCount
    {
        get
        {
            if (drops == null) return 0;
            int count = 0;
            foreach (var d in drops)
                if (d != null) count++;
            return count;
        }
    }

    /// <summary>
    /// Called by IncapacitationController when the creature dies.
    /// </summary>
    public void Initialize(GraftPartSO[] drops, DNASampleSO dna, float healthNorm,
        float despawn = 60f, bool isDegraded = false)
    {
        this.drops = drops;
        this.despawnTime = despawn;
        this.isDegraded = isDegraded;
        this.creatureName = gameObject.name.Replace("(Clone)", "").Trim();
        this.spawnTime = Time.time;

        // --- Ragdoll setup ---

        // Disable AI
        var enemy = GetComponent<IEnemy>();
        if (enemy != null)
            enemy.EnableAI(false);

        // Disable animator so Rigidbody physics takes over (ragdoll)
        var anim = GetComponentInChildren<Animator>();
        if (anim != null)
            anim.enabled = false;

        // Make sure Rigidbody has gravity and isn't kinematic
        var rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.useGravity = true;
            rb.isKinematic = false;
            rb.constraints = RigidbodyConstraints.None;
            rb.AddTorque(Random.insideUnitSphere * 3f, ForceMode.Impulse);
        }

        // Disable the enemy health bar if present
        var healthBar = GetComponent<EnemyHealthBar>();
        if (healthBar != null)
            healthBar.enabled = false;

        // Disable EnemyHealth to prevent further damage
        var enemyHealth = GetComponent<EnemyHealth>();
        if (enemyHealth != null)
            enemyHealth.enabled = false;

        // Disable the IncapacitationController
        var incapCtrl = GetComponent<IncapacitationController>();
        if (incapCtrl != null)
            incapCtrl.enabled = false;

        // Despawn after timer
        Destroy(gameObject, despawnTime);

#if UNITY_EDITOR
        string quality = isDegraded ? " (DEGRADED)" : "";
        Debug.Log($"[CreatureCorpse] {creatureName} is now a corpse{quality}. {DropCount} parts available. Despawns in {despawnTime}s.");
#endif
    }

    /// <summary>
    /// Called when the player presses E while looking at the corpse.
    /// Collects all remaining parts to base storage via drone.
    /// </summary>
    public void Interact(InteractController controller)
    {
        if (DropCount == 0) return;

        // Add all remaining drops to base storage immediately
        int count = 0;
        foreach (var drop in drops)
        {
            if (drop == null) continue;
            BaseStorage.Instance?.AddPart(drop, isDegraded);
            count++;
        }

        // Dispatch a single drone for the visual
        var config = AnatomyManager.Instance != null ? AnatomyManager.Instance.DroneConfig : null;
        if (config != null)
            DronePickup.Dispatch(gameObject, config);

        // Show notification
        string quality = isDegraded ? "DEGRADED " : "";
        string partWord = count == 1 ? "part" : "parts";
        if (HarvestFlashUI.Instance != null)
            HarvestFlashUI.Instance.ShowFlash(
                $"{quality}REMAINS COLLECTED",
                $"{count} {partWord} from {creatureName} -- drone inbound",
                isDegraded ? HarvestUIStyles.WarningAmber : HarvestUIStyles.AmberPrimary,
                HarvestUIStyles.MutedTan,
                HarvestUIStyles.FlashSiennaBG, 2.5f);

        // Give energy for harvesting
        var energy = controller.GetComponent<SuitEnergy>();
        if (energy == null)
            energy = controller.GetComponentInParent<SuitEnergy>();
        energy?.OnHarvest();

        // Clear drops so corpse can't be re-harvested
        for (int i = 0; i < drops.Length; i++)
            drops[i] = null;
    }

    /// <summary>
    /// Remove a part from the available drops.
    /// </summary>
    public void RemoveDrop(GraftPartSO part)
    {
        if (drops == null) return;
        for (int i = 0; i < drops.Length; i++)
        {
            if (drops[i] == part)
            {
                drops[i] = null;
                break;
            }
        }
    }
}
