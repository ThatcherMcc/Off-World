using UnityEngine;

/// <summary>
/// Base station for browsing all stored graft parts and DNA samples.
/// Player walks up, presses E, and the StorageTerminalUI opens.
/// Place this on a world object in the base area.
/// Requires: Collider on this GameObject + layer set to InteractLayerMask (same as other interactables).
/// </summary>
[RequireComponent(typeof(Collider))]
public class StorageTerminal : MonoBehaviour, IInteractable
{
    private void Start()
    {
        if (gameObject.layer == 0)
            Debug.LogWarning($"[StorageTerminal] '{gameObject.name}' is on the Default layer. " +
                "Set it to the same layer as other interactables (check InteractController.InteractLayerMask).", this);
    }

    public void Interact(InteractController controller)
    {
        var ui = StorageTerminalUI.Instance;
        if (ui != null && !ui.IsShowing)
            ui.Show();
    }
}
