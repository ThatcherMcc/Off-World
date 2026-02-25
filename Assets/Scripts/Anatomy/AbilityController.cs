using UnityEngine;
using OffWorld.Anatomy;

/// <summary>
/// Handles scroll-wheel cycling through active DNA abilities.
/// Lives on the Player GameObject alongside AnatomyManager.
/// </summary>
public class AbilityController : MonoBehaviour
{
    private AnatomyManager anatomyManager;

    private void Start()
    {
        anatomyManager = GetComponent<AnatomyManager>();
        if (anatomyManager == null)
            anatomyManager = GetComponentInParent<AnatomyManager>();
    }

    private void Update()
    {
        if (anatomyManager == null) return;

        // Don't cycle when a menu is open
        if (GraftMenuUI.MenuOpen) return;

        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll > 0.01f)
            anatomyManager.Suit.CycleActiveSlot(1);
        else if (scroll < -0.01f)
            anatomyManager.Suit.CycleActiveSlot(-1);
    }
}
