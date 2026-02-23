using UnityEngine;
using OffWorld.Anatomy;

/// <summary>
/// A world pickup for a graft body part dropped by a killed creature.
/// The player interacts with it to graft the part onto themselves.
/// Implements IInteractable so the existing InteractController can pick it up.
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
    /// Grafts the part onto the player via AnatomyManager.
    /// </summary>
    public void Interact(InteractController controller)
    {
        if (graftPart == null) return;

        // Find the AnatomyManager on the player
        var anatomyManager = controller.GetComponent<AnatomyManager>();
        if (anatomyManager == null)
        {
            anatomyManager = controller.GetComponentInParent<AnatomyManager>();
        }

        if (anatomyManager != null)
        {
            anatomyManager.GraftPart(graftPart);

            // Give energy for harvesting
            var energy = controller.GetComponent<OffWorld.Anatomy.SuitEnergy>();
            if (energy == null)
                energy = controller.GetComponentInParent<OffWorld.Anatomy.SuitEnergy>();
            energy?.OnHarvest();

#if UNITY_EDITOR
            Debug.Log($"[PartDropPickup] Player grafted '{graftPart.partName}' into {graftPart.slot}.");
#endif

            Destroy(gameObject);
        }
        else
        {
#if UNITY_EDITOR
            Debug.LogWarning("[PartDropPickup] No AnatomyManager found on player.");
#endif
        }
    }
}
