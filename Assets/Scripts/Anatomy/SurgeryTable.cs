using UnityEngine;

/// <summary>
/// Base station for equipping graft parts from BaseStorage onto the player.
/// Player walks up, presses E, and the SurgeryTableUI opens with a body diagram.
/// Place this on a world object in the base area.
/// Requires: Collider on this GameObject + layer set to InteractLayerMask (same as other interactables).
/// </summary>
[RequireComponent(typeof(Collider))]
public class SurgeryTable : MonoBehaviour, IInteractable
{
    private void Start()
    {
        if (gameObject.layer == 0)
            Debug.LogWarning($"[SurgeryTable] '{gameObject.name}' is on the Default layer. " +
                "Set it to the same layer as other interactables (check InteractController.InteractLayerMask).", this);

        if (GetComponent<Collider>() == null)
            Debug.LogWarning($"[SurgeryTable] '{gameObject.name}' has no Collider. " +
                "A collider is required for interaction detection.", this);
    }

    public void Interact(InteractController controller)
    {
        var ui = SurgeryTableUI.Instance;
        if (ui != null && !ui.IsShowing)
            ui.Show();
    }
}
