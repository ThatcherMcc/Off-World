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

    [Header("Incapacitation")]
    [Tooltip("If true, this creature can be incapacitated instead of killed.")]
    [SerializeField] private bool canBeIncapacitated = true;
    [Range(0.05f, 0.4f)]
    [Tooltip("HP fraction at which incapacitation triggers (e.g. 0.15 = 15% HP).")]
    [SerializeField] private float incapacitateThreshold = 0.15f;
    [Tooltip("Seconds the creature stays incapacitated before recovering.")]
    [SerializeField] private float vulnerableWindowDuration = 12f;

    private bool isIncapacitated;
    private bool hasBeenExtracted; // Prevents double-extraction

    /// <summary>Fired when creature is incapacitated (HP hits threshold). Passes remaining HP fraction.</summary>
    public event Action<float> OnIncapacitated;
    /// <summary>Fired when creature recovers from incapacitation.</summary>
    public event Action OnRecovered;
    /// <summary>Fired when creature dies. Systems can hook this for loot drops.</summary>
    public event Action OnDied;

    public bool IsIncapacitated => isIncapacitated;
    public bool HasBeenExtracted => hasBeenExtracted;
    public int MaxHealth => maxHealth;

    /// <summary>Seconds remaining before the creature recovers from incapacitation. Updated every frame.</summary>
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
        if (activated)
        {
            // If already incapacitated, additional damage kills
            if (isIncapacitated)
            {
                health -= dmg;
                healthbar?.SetHealth(health);
                if (health <= 0)
                {
                    isIncapacitated = false;
                    IncapTimeRemaining = 0f;

                    // Flash: player killed the specimen instead of extracting
                    if (HarvestFlashUI.Instance != null)
                        HarvestFlashUI.Instance.ShowFlash("SPECIMEN KILLED",
                            "DNA extraction no longer possible",
                            HarvestUIStyles.DangerRed, HarvestUIStyles.BurntOrange,
                            HarvestUIStyles.FlashRedBG, 2f);

                    Die();
                }
                return;
            }

            health -= dmg;
            healthbar?.SetHealth(health);

            // Check for incapacitation before death
            if (canBeIncapacitated && !hasBeenExtracted && health > 0 && GetHealthNormalized() <= incapacitateThreshold)
            {
                Incapacitate();
                return;
            }

            if (health <= 0)
            {
                Die();
            }
        }
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
        Debug.Log($"[EnemyHealth] {gameObject.name} incapacitated at {GetHealthNormalized():P0} HP. Window: {vulnerableWindowDuration}s");
#endif
    }

    private void Recover()
    {
        isIncapacitated = false;
        // Heal to just above threshold so the creature can fight again
        int healTarget = Mathf.CeilToInt(maxHealth * (incapacitateThreshold + 0.1f));
        health = Mathf.Max(health, healTarget);
        healthbar?.SetHealth(health);
        OnRecovered?.Invoke();

#if UNITY_EDITOR
        Debug.Log($"[EnemyHealth] {gameObject.name} recovered from incapacitation.");
#endif
    }

    /// <summary>Mark that DNA has been extracted. Prevents re-incapacitation.</summary>
    public void MarkExtracted()
    {
        hasBeenExtracted = true;
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
