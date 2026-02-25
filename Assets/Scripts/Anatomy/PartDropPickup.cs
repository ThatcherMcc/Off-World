using UnityEngine;
using OffWorld.Anatomy;

/// <summary>
/// A world pickup for a graft body part dropped by a killed creature.
/// The player interacts with it to collect the part — a drone flies in,
/// grabs it, and transports it to base storage.
/// Implements IInteractable so the existing InteractController can detect it.
/// </summary>
public class PartDropPickup : MonoBehaviour, IInteractable
{
    [Header("Drop Data")]
    [SerializeField] private GraftPartSO graftPart;

    [Header("Lifetime")]
    [Tooltip("Seconds before the drop despawns. 0 = never.")]
    [SerializeField] private float despawnTime = 120f;

    [Header("Visual")]
    [SerializeField] private float bobSpeed = 2f;
    [SerializeField] private float bobHeight = 0.3f;
    [SerializeField] private float rotateSpeed = 45f;
    private Vector3 startPos;

    public GraftPartSO GraftPart => graftPart;

    /// <summary>Called by IncapacitationController when spawning the drop.</summary>
    public void Initialize(GraftPartSO part)
    {
        graftPart = part;

        if (despawnTime > 0)
            Destroy(gameObject, despawnTime);
    }

    private void Start()
    {
        startPos = transform.position;
    }

    private void Update()
    {
        // Bob up and down
        float newY = startPos.y + Mathf.Sin(Time.time * bobSpeed) * bobHeight;
        transform.position = new Vector3(transform.position.x, newY, transform.position.z);

        // Rotate
        transform.Rotate(Vector3.up, rotateSpeed * Time.deltaTime);
    }

    /// <summary>
    /// Called when the player presses interact (E key) while looking at this pickup.
    /// Sends the part to base storage via drone instead of grafting directly.
    /// </summary>
    public void Interact(InteractController controller)
    {
        if (graftPart == null) return;

        // Add to base storage immediately (drone is cosmetic)
        if (BaseStorage.Instance != null)
            BaseStorage.Instance.AddPart(graftPart);

        // Dispatch drone for visual pickup
        var config = AnatomyManager.Instance != null ? AnatomyManager.Instance.DroneConfig : null;
        if (config != null)
            DronePickup.Dispatch(gameObject, config);

        // Show collection notification
        if (HarvestFlashUI.Instance != null)
            HarvestFlashUI.Instance.ShowFlash(
                "PART COLLECTED",
                $"{graftPart.partName} -- drone inbound",
                HarvestUIStyles.AmberPrimary, HarvestUIStyles.MutedTan,
                HarvestUIStyles.FlashSiennaBG, 2.5f);

        // Give energy for harvesting
        var energy = controller.GetComponent<SuitEnergy>();
        if (energy == null)
            energy = controller.GetComponentInParent<SuitEnergy>();
        energy?.OnHarvest();

        // Disable further interaction (drone handles destruction)
        var collider = GetComponent<Collider>();
        if (collider != null) collider.enabled = false;

#if UNITY_EDITOR
        Debug.Log($"[PartDropPickup] '{graftPart.partName}' sent to base storage via drone.");
#endif
    }
}
