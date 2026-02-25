using UnityEngine;
using OffWorld.Anatomy;

/// <summary>
/// Minimal OnGUI hotbar showing the currently selected DNA active ability.
/// Displays a horizontal strip at the bottom-center of the screen with
/// unlocked active slots. The current slot is highlighted and larger.
/// Only visible when DNA is loaded.
/// </summary>
public class AbilityHotbarUI : MonoBehaviour
{
    private AnatomyManager anatomyManager;

    // Styles
    private bool stylesReady;
    private GUIStyle slotBox, activeSlotBox;
    private GUIStyle slotLabel, activeSlotLabel;
    private GUIStyle abilityName;
    private Texture2D slotBgTex, activeSlotBgTex, emptySlotBgTex;

    private void Start()
    {
        anatomyManager = GetComponent<AnatomyManager>();
        if (anatomyManager == null)
            anatomyManager = GetComponentInParent<AnatomyManager>();
    }

    private void InitStyles()
    {
        if (stylesReady) return;

        slotBgTex = HarvestUIStyles.MakeTex(new Color(0.08f, 0.12f, 0.14f, 0.85f));
        activeSlotBgTex = HarvestUIStyles.MakeTex(new Color(0.06f, 0.22f, 0.20f, 0.95f));
        emptySlotBgTex = HarvestUIStyles.MakeTex(new Color(0.06f, 0.06f, 0.08f, 0.60f));

        slotBox = new GUIStyle(GUI.skin.box);
        slotBox.normal.background = slotBgTex;
        slotBox.border = new RectOffset(2, 2, 2, 2);

        activeSlotBox = new GUIStyle(GUI.skin.box);
        activeSlotBox.normal.background = activeSlotBgTex;
        activeSlotBox.border = new RectOffset(2, 2, 2, 2);

        slotLabel = HarvestUIStyles.MakeLabel(11, HarvestUIStyles.MidGray, FontStyle.Normal, TextAnchor.MiddleCenter);
        activeSlotLabel = HarvestUIStyles.MakeLabel(13, HarvestUIStyles.TealPrimary, FontStyle.Bold, TextAnchor.MiddleCenter);
        abilityName = HarvestUIStyles.MakeLabel(14, HarvestUIStyles.CleanWhite, FontStyle.Bold, TextAnchor.MiddleCenter);

        stylesReady = true;
    }

    private void OnGUI()
    {
        if (anatomyManager == null) return;
        var suit = anatomyManager.Suit;
        if (!suit.HasAnyDNA()) return;

        // Don't show when a menu is open
        if (GraftMenuUI.MenuOpen) return;

        InitStyles();

        int unlocked = suit.UnlockedActiveSlots;
        if (unlocked <= 0) return;

        float slotSize = 46f;
        float activeSize = 56f;
        float gap = 6f;

        // Calculate total width
        float totalW = 0f;
        for (int i = 0; i < unlocked; i++)
        {
            totalW += (i == suit.CurrentActiveIndex) ? activeSize : slotSize;
            if (i < unlocked - 1) totalW += gap;
        }

        float startX = (Screen.width - totalW) / 2f;
        float baseY = Screen.height - 75f;
        float x = startX;

        for (int i = 0; i < unlocked; i++)
        {
            bool isCurrent = (i == suit.CurrentActiveIndex);
            float size = isCurrent ? activeSize : slotSize;
            float y = isCurrent ? baseY - 5f : baseY;

            var dna = suit.GetActiveSlot(i);
            var style = isCurrent ? activeSlotBox : slotBox;

            if (dna == null)
            {
                // Empty slot
                var emptyStyle = new GUIStyle(slotBox);
                emptyStyle.normal.background = emptySlotBgTex;
                GUI.Box(new Rect(x, y, size, size), "", emptyStyle);

                var emptyLabel = HarvestUIStyles.MakeLabel(10, new Color(0.4f, 0.4f, 0.4f, 0.6f), FontStyle.Normal, TextAnchor.MiddleCenter);
                GUI.Label(new Rect(x, y, size, size), "--", emptyLabel);
            }
            else
            {
                // Filled slot
                GUI.Box(new Rect(x, y, size, size), "", style);

                // Slot number
                var label = isCurrent ? activeSlotLabel : slotLabel;
                GUI.Label(new Rect(x, y + 2, size, 16), $"{i + 1}", label);

                // Species initial as icon placeholder
                string initial = dna.species.ToString().Substring(0, 1);
                var initialStyle = HarvestUIStyles.MakeLabel(
                    isCurrent ? 20 : 16,
                    isCurrent ? HarvestUIStyles.TealPrimary : HarvestUIStyles.SeaGreen,
                    FontStyle.Bold, TextAnchor.MiddleCenter);
                GUI.Label(new Rect(x, y + 14, size, size - 16), initial, initialStyle);
            }

            // Selection indicator for current slot
            if (isCurrent)
            {
                var indicatorColor = HarvestUIStyles.TealPrimary;
                var prevColor = GUI.color;
                GUI.color = indicatorColor;
                GUI.DrawTexture(new Rect(x, y + size + 2, size, 2), Texture2D.whiteTexture);
                GUI.color = prevColor;
            }

            x += size + gap;
        }

        // Show ability name below hotbar for current slot
        var currentDNA = suit.CurrentActiveDNA;
        if (currentDNA != null)
        {
            string displayName = currentDNA.sampleName;
            GUI.Label(new Rect(0, baseY + activeSize + 8, Screen.width, 20), displayName, abilityName);
        }
    }
}
