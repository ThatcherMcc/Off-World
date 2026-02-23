using UnityEngine;

/// <summary>
/// Renders context-aware interact prompts when the player looks at:
/// - A creature corpse: amber "[E] HARVEST REMAINS" with part count
/// - An incapacitated creature: teal "[E] EXTRACT DNA" with recovery timer
///
/// Reads from InteractController.LookedAtCorpse / LookedAtIncapCreature.
/// Add to the Player GameObject alongside InteractController.
/// </summary>
public class HarvestPromptUI : MonoBehaviour
{
    private InteractController interactController;

    // Styles
    private bool stylesReady;
    private GUIStyle corpseTitle, corpseSubtitle, corpseDecompose;
    private GUIStyle incapTitle, incapSubtitle, incapTimer;

    private void Start()
    {
        interactController = GetComponent<InteractController>();
        if (interactController == null)
            interactController = GetComponentInParent<InteractController>();
    }

    private void InitStyles()
    {
        if (stylesReady) return;

        corpseTitle = HarvestUIStyles.MakeLabel(20, HarvestUIStyles.AmberPrimary, FontStyle.Bold, TextAnchor.MiddleCenter);
        corpseSubtitle = HarvestUIStyles.MakeLabel(15, HarvestUIStyles.MutedTan, FontStyle.Normal, TextAnchor.MiddleCenter);
        corpseDecompose = HarvestUIStyles.MakeLabel(13, HarvestUIStyles.DarkSienna, FontStyle.Italic, TextAnchor.MiddleRight);

        incapTitle = HarvestUIStyles.MakeLabel(20, HarvestUIStyles.TealPrimary, FontStyle.Bold, TextAnchor.MiddleCenter);
        incapSubtitle = HarvestUIStyles.MakeLabel(15, HarvestUIStyles.SeaGreen, FontStyle.Normal, TextAnchor.MiddleCenter);
        incapTimer = HarvestUIStyles.MakeLabel(13, HarvestUIStyles.TealPrimary, FontStyle.Bold, TextAnchor.MiddleCenter);

        stylesReady = true;
    }

    private void OnGUI()
    {
        if (interactController == null) return;

        // Don't show prompts when a menu is open
        if ((GraftMenuUI.Instance != null && GraftMenuUI.Instance.IsShowing) ||
            (ExtractMenuUI.Instance != null && ExtractMenuUI.Instance.IsShowing))
            return;

        if (interactController.LookedAtCorpse != null)
            DrawCorpsePrompt(interactController.LookedAtCorpse);
        else if (interactController.LookedAtIncapCreature != null)
            DrawIncapPrompt(interactController.LookedAtIncapCreature);
    }

    private void DrawCorpsePrompt(CreatureCorpse corpse)
    {
        InitStyles();

        float w = 300, h = 80;
        float x = (Screen.width - w) / 2f;
        float y = Screen.height / 2f + 60f;
        Rect bg = new Rect(x, y, w, h);

        // Background
        var prevColor = GUI.color;
        GUI.color = HarvestUIStyles.CorpsePromptBG;
        GUI.DrawTexture(bg, Texture2D.whiteTexture);
        GUI.color = prevColor;

        // Title: [E] HARVEST REMAINS
        GUI.Label(new Rect(x, y + 8, w, 26), "[E] HARVEST REMAINS", corpseTitle);

        // Subtitle: CreatureName -- N parts available
        int count = corpse.DropCount;
        string parts = count == 1 ? "part" : "parts";
        GUI.Label(new Rect(x, y + 34, w, 22), $"{corpse.CreatureName}  --  {count} {parts} available", corpseSubtitle);

        // Decomposing countdown (final 15 seconds)
        float timeLeft = corpse.DespawnTimeRemaining;
        if (timeLeft < 15f)
        {
            // Pulse alpha in final 5 seconds
            float alpha = 1f;
            if (timeLeft < 5f)
                alpha = Mathf.Lerp(0.5f, 1f, Mathf.Sin(Time.time * 2f * Mathf.PI * 2f) * 0.5f + 0.5f);

            Color decompColor = HarvestUIStyles.DarkSienna;
            decompColor.a = alpha;
            corpseDecompose.normal.textColor = decompColor;
            GUI.Label(new Rect(x + 10, y + 58, w - 20, 18), $"Decomposing... {timeLeft:F0}s", corpseDecompose);
        }
    }

    private void DrawIncapPrompt(EnemyHealth enemy)
    {
        InitStyles();

        float w = 300, h = 100;
        float x = (Screen.width - w) / 2f;
        float y = Screen.height / 2f + 60f;
        Rect bg = new Rect(x, y, w, h);

        float timeRemaining = enemy.IncapTimeRemaining;

        // Pulse background when < 4s
        Color bgColor = HarvestUIStyles.IncapPromptBG;
        if (timeRemaining < 4f)
        {
            float pulse = Mathf.Sin(Time.time * 3f * Mathf.PI * 2f) * 0.1f + 0.9f;
            bgColor.a *= pulse;
        }

        var prevColor = GUI.color;
        GUI.color = bgColor;
        GUI.DrawTexture(bg, Texture2D.whiteTexture);
        GUI.color = prevColor;

        // Title: [E] EXTRACT DNA
        GUI.Label(new Rect(x, y + 6, w, 26), "[E] EXTRACT DNA", incapTitle);

        // Subtitle
        string creatureName = enemy.gameObject.name.Replace("(Clone)", "").Trim();
        GUI.Label(new Rect(x, y + 30, w, 22), $"{creatureName}  --  specimen alive", incapSubtitle);

        // Timer bar
        float totalDuration = 12f; // Default, could read from CreatureLootTable
        var loot = enemy.GetComponent<OffWorld.Anatomy.CreatureLootTable>();
        if (loot != null) totalDuration = loot.vulnerableWindowDuration;
        float fill = timeRemaining / totalDuration;

        Color timerBarColor;
        string timerLabel;
        if (timeRemaining > 6f)
        {
            timerBarColor = HarvestUIStyles.TealPrimary;
            timerLabel = $"{timeRemaining:F0}s remaining";
        }
        else if (timeRemaining > 4f)
        {
            timerBarColor = HarvestUIStyles.WarningAmber;
            timerLabel = $"Act now -- {timeRemaining:F0}s";
        }
        else
        {
            timerBarColor = HarvestUIStyles.DangerRed;
            timerLabel = $"RECOVERING SOON";
        }

        // Bar
        float barX = x + 50, barW = 200, barH = 8;
        float barY = y + 58;
        HarvestUIStyles.DrawBar(new Rect(barX, barY, barW, barH), fill, timerBarColor, HarvestUIStyles.TimerEmpty);

        // Timer text
        incapTimer.normal.textColor = timerBarColor;
        GUI.Label(new Rect(x, barY + 12, w, 18), timerLabel, incapTimer);
    }
}
