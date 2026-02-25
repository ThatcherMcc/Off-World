using System;
using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    [SerializeField] private int health;
    [SerializeField] private Healthbar healthbar;
    [SerializeField] private CharacterInteraction bossController;
    [Tooltip("Must be true for HurtEnemy to deal damage. Boss sets this on encounter start; regular mobs should check this on.")]
    [SerializeField] private bool activated = true;
    [SerializeField] private int maxHealth = 100;

    [Header("Downed State")]
    [Tooltip("Seconds the creature stays downed before recovering.")]
    [SerializeField] private float vulnerableWindowDuration = 12f;

    private bool isIncapacitated;
    private bool hasBeenExtracted;

    /// <summary>Fired when creature collapses at 1 HP. Passes remaining HP fraction.</summary>
    public event Action<float> OnIncapacitated;
    /// <summary>Fired when creature recovers from downed state.</summary>
    public event Action OnRecovered;
    /// <summary>Fired when creature dies. Systems can hook this for loot drops.</summary>
    public event Action OnDied;

    public bool IsIncapacitated => isIncapacitated;
    public bool HasBeenExtracted => hasBeenExtracted;
    public int MaxHealth => maxHealth;

    /// <summary>Seconds remaining before the creature recovers. Updated every frame.</summary>
    public float IncapTimeRemaining { get; private set; }

    private void Awake()
    {
        if (healthbar == null)
        {
            var hbObj = GameObject.FindGameObjectWithTag("BOSSHEALTHBAR");
            if (hbObj != null)
                healthbar = hbObj.GetComponent<Healthbar>();
        }
        if (bossController == null)
        {
            bossController = GetComponent<CharacterInteraction>();
        }
    }

    private void Start()
    {
        health = maxHealth;
    }

    public void HurtEnemy(int dmg)
    {
        if (!activated) return;

        // If already downed, any additional damage kills immediately
        if (isIncapacitated)
        {
            health = 0;
            isIncapacitated = false;
            IncapTimeRemaining = 0f;
            healthbar?.SetHealth(health);

            if (HarvestFlashUI.Instance != null)
                HarvestFlashUI.Instance.ShowFlash("SPECIMEN KILLED",
                    "DNA extraction no longer possible",
                    HarvestUIStyles.DangerRed, HarvestUIStyles.BurntOrange,
                    HarvestUIStyles.FlashRedBG, 2f);

            Die();
            return;
        }

        health -= dmg;

        // Creature collapses at 1 HP instead of dying
        if (health <= 1)
        {
            health = 1;
            healthbar?.SetHealth(health);
            Incapacitate();
            return;
        }

        healthbar?.SetHealth(health);
    }

    private void Update()
    {
        if (isIncapacitated)
        {
            IncapTimeRemaining -= Time.deltaTime;
            if (IncapTimeRemaining <= 0f)
            {
                IncapTimeRemaining = 0f;
                Recover();
            }
        }
    }

    private void Incapacitate()
    {
        isIncapacitated = true;
        IncapTimeRemaining = vulnerableWindowDuration;
        OnIncapacitated?.Invoke(GetHealthNormalized());

#if UNITY_EDITOR
        Debug.Log($"[EnemyHealth] {gameObject.name} collapsed at 1 HP. Window: {vulnerableWindowDuration}s");
#endif
    }

    private void Recover()
    {
        isIncapacitated = false;
        // Heal to 25% so the creature can flee or fight
        int healTarget = Mathf.CeilToInt(maxHealth * 0.25f);
        health = Mathf.Max(health, healTarget);
        healthbar?.SetHealth(health);
        OnRecovered?.Invoke();

#if UNITY_EDITOR
        Debug.Log($"[EnemyHealth] {gameObject.name} recovered from downed state.");
#endif
    }

    /// <summary>Mark that DNA has been extracted. If killed later, parts will be degraded.</summary>
    public void MarkExtracted()
    {
        hasBeenExtracted = true;
    }

    /// <summary>
    /// Force-kill the creature. Called by the harvest menu when player chooses HARVEST PARTS.
    /// </summary>
    public void ForceKill()
    {
        health = 0;
        isIncapacitated = false;
        IncapTimeRemaining = 0f;
        healthbar?.SetHealth(health);
        Die();
    }

    private void Die()
    {
        OnDied?.Invoke();
        if (bossController != null)
            bossController.DeactivateHealthbar();
        // If IncapacitationController is present, it converts the creature to a corpse.
        // Only destroy immediately if no controller handles the death.
        if (GetComponent<IncapacitationController>() == null)
            Destroy(gameObject);
    }

    public void SetActivated(bool state)
    {
        activated = state;
    }

    /// <summary> 0 = dead, 1 = full. Use for phase logic (e.g. enrage under 0.5). </summary>
    public float GetHealthNormalized()
    {
        if (maxHealth <= 0) return 1f;
        return Mathf.Clamp01((float)health / maxHealth);
    }
}
