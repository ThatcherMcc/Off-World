using UnityEngine;

/// <summary>
/// Base station for processing DNA samples from BaseStorage into the player's suit.
/// Player walks up, presses E, and the ExtractionChamberUI opens.
/// Place this on a world object in the base area.
/// Requires: Collider on this GameObject + layer set to InteractLayerMask (same as other interactables).
/// </summary>
[RequireComponent(typeof(Collider))]
public class ExtractionChamber : MonoBehaviour, IInteractable
{
    private void Start()
    {
        if (gameObject.layer == 0)
            Debug.LogWarning($"[ExtractionChamber] '{gameObject.name}' is on the Default layer. " +
                "Set it to the same layer as other interactables (check InteractController.InteractLayerMask).", this);

        if (GetComponent<Collider>() == null)
            Debug.LogWarning($"[ExtractionChamber] '{gameObject.name}' has no Collider. " +
                "A collider is required for interaction detection.", this);
    }

    public void Interact(InteractController controller)
    {
        var ui = ExtractionChamberUI.Instance;
        if (ui != null && !ui.IsShowing)
            ui.Show();
    }
}
