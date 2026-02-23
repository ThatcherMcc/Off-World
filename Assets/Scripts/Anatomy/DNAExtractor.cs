using System.Collections;
using UnityEngine;
using OffWorld.Anatomy;

/// <summary>
/// Tool component on the player that handles DNA extraction from incapacitated creatures.
/// The player must look at an incapacitated creature and hold the extract key to channel.
/// </summary>
public class DNAExtractor : MonoBehaviour
{
    [Header("Extraction Settings")]
    [SerializeField] private float extractionChannelTime = 3f;
    [SerializeField] private float extractionRange = 4f;
    [SerializeField] private KeyCode extractKey = KeyCode.F;
    [SerializeField] private LayerMask enemyLayerMask;

    [Header("References")]
    [SerializeField] private Transform fpsCam;

    private bool isExtracting;
    private float extractionProgress;
    private EnemyHealth targetEnemy;
    private CreatureLootTable targetLootTable;
    private AnatomyManager anatomyManager;

    /// <summary>0-1 extraction progress for UI.</summary>
    public float ExtractionProgress => extractionProgress;
    public bool IsExtracting => isExtracting;

    private void Start()
    {
        anatomyManager = GetComponent<AnatomyManager>();
        if (anatomyManager == null)
            anatomyManager = GetComponentInParent<AnatomyManager>();

        if (fpsCam == null)
        {
            var cam = Camera.main;
            if (cam != null)
                fpsCam = cam.transform;
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(extractKey) && !isExtracting)
        {
            TryStartExtraction();
        }

        if (Input.GetKeyUp(extractKey) && isExtracting)
        {
            CancelExtraction();
        }

        if (isExtracting)
        {
            ContinueExtraction();
        }
    }

    private void TryStartExtraction()
    {
        if (fpsCam == null) return;

        if (Physics.SphereCast(fpsCam.position, 0.5f, fpsCam.forward, out RaycastHit hit, extractionRange, enemyLayerMask))
        {
            var enemyHealth = hit.collider.GetComponent<EnemyHealth>();
            if (enemyHealth == null)
                enemyHealth = hit.collider.GetComponentInParent<EnemyHealth>();

            if (enemyHealth != null && enemyHealth.IsIncapacitated && !enemyHealth.HasBeenExtracted)
            {
                var lootTable = enemyHealth.GetComponent<CreatureLootTable>();
                if (lootTable != null && lootTable.dnaSample != null)
                {
                    StartExtraction(enemyHealth, lootTable);
                }
            }
        }
    }

    private void StartExtraction(EnemyHealth enemy, CreatureLootTable lootTable)
    {
        isExtracting = true;
        extractionProgress = 0f;
        targetEnemy = enemy;
        targetLootTable = lootTable;

#if UNITY_EDITOR
        Debug.Log($"[DNAExtractor] Starting extraction from {enemy.gameObject.name}...");
#endif
    }

    private void ContinueExtraction()
    {
        // Check if target is still valid
        if (targetEnemy == null || !targetEnemy.IsIncapacitated)
        {
            CancelExtraction();
            return;
        }

        // Check if player took damage (interrupts extraction)
        // This would need a callback from PlayerHealth -- for now, we just check distance
        if (fpsCam != null)
        {
            float dist = Vector3.Distance(transform.position, targetEnemy.transform.position);
            if (dist > extractionRange * 1.5f)
            {
                CancelExtraction();
                return;
            }
        }

        extractionProgress += Time.deltaTime / extractionChannelTime;

        if (extractionProgress >= 1f)
        {
            CompleteExtraction();
        }
    }

    private void CompleteExtraction()
    {
        if (targetEnemy == null || targetLootTable == null)
        {
            CancelExtraction();
            return;
        }

        // Determine DNA quality based on precision of incapacitation
        DNATier tier = targetLootTable.DetermineDNATier(targetEnemy.GetHealthNormalized());

        // Mark creature as extracted
        targetEnemy.MarkExtracted();

        // Give energy for successful extraction
        var energy = GetComponent<SuitEnergy>();
        if (energy == null) energy = GetComponentInParent<SuitEnergy>();
        energy?.OnIncapacitate();

        // Create a copy of the DNA with the determined tier
        var dnaSample = targetLootTable.dnaSample;

#if UNITY_EDITOR
        Debug.Log($"[DNAExtractor] Extracted {tier} {dnaSample.sampleName} from {targetEnemy.gameObject.name}!");
#endif

        // Find first empty active slot and load it, or notify player to manage slots
        if (anatomyManager != null)
        {
            bool loaded = false;
            for (int i = 0; i < anatomyManager.Suit.UnlockedActiveSlots; i++)
            {
                if (anatomyManager.Suit.GetActiveSlot(i) == null)
                {
                    anatomyManager.LoadActiveDNA(i, dnaSample);
                    loaded = true;
                    break;
                }
            }

            if (!loaded)
            {
                // All slots full -- load into first slot (replace oldest)
                anatomyManager.LoadActiveDNA(0, dnaSample);
            }
        }

        isExtracting = false;
        extractionProgress = 0f;
        targetEnemy = null;
        targetLootTable = null;
    }

    private void CancelExtraction()
    {
        isExtracting = false;
        extractionProgress = 0f;
        targetEnemy = null;
        targetLootTable = null;

#if UNITY_EDITOR
        Debug.Log("[DNAExtractor] Extraction cancelled.");
#endif
    }
}
