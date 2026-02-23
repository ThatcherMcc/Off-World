using UnityEngine;
using OffWorld.Anatomy;

/// <summary>
/// Added to a creature when it dies. Converts it into an interactable ragdoll corpse.
/// The player walks up and presses E (interact) to open the graft menu.
/// Dead bodies only offer GRAFTS — never DNA extraction.
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

    /// <summary>How many seconds until this corpse despawns.</summary>
    public float DespawnTimeRemaining => Mathf.Max(0f, despawnTime - (Time.time - spawnTime));

    /// <summary>Creature display name.</summary>
    public string CreatureName => creatureName;

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
    public void Initialize(GraftPartSO[] drops, DNASampleSO dna, float healthNorm, float despawn = 60f)
    {
        this.drops = drops;
        this.despawnTime = despawn;
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
            // Small random torque so the body topples over
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
        Debug.Log($"[CreatureCorpse] {creatureName} is now a corpse. {DropCount} parts available. Despawns in {despawnTime}s.");
#endif
    }

    /// <summary>
    /// Called when the player presses E while looking at the corpse.
    /// Opens the graft menu (kill path — grafts only, no DNA).
    /// </summary>
    public void Interact(InteractController controller)
    {
        if (DropCount == 0) return;

        var ui = GraftMenuUI.Instance;
        if (ui != null && !ui.IsShowing)
        {
            // Filter out null entries (already grafted parts)
            var available = System.Array.FindAll(drops, d => d != null);
            ui.Show(available, creatureName, this);
        }
    }

    /// <summary>
    /// Remove a part from the available drops (called after successful graft).
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
