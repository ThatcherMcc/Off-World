using UnityEngine;
using OffWorld.Anatomy;

/// <summary>
/// Unified harvest choice menu — appears when the player presses E on a downed creature.
/// Offers two choices:
///   EXTRACT DNA — creature lives, DNA to base storage, creature flees
///   HARVEST PARTS — creature dies, graft parts to base storage
///
/// OnGUI-based, no Canvas needed. Add to the Player GameObject.
/// </summary>
public class HarvestChoiceMenuUI : MonoBehaviour
{
    public static HarvestChoiceMenuUI Instance { get; private set; }
    public bool IsShowing { get; private set; }

    private enum State { Choice, Result }
    private State state;

    // Target creature
    private EnemyHealth targetHealth;
    private CreatureLootTable targetLoot;
    private IncapacitationController targetIncapCtrl;
    private string creatureName;

    // Result data
    private string resultTitle;
    private string resultSubtitle;
    private Color resultTitleColor;

    // Cursor restore
    private CursorLockMode prevLockState;
    private bool prevCursorVisible;

    // Styles
    private bool stylesReady;
    private GUIStyle panelBox, titleStyle, subtitleStyle, sectionHeader, bodyText, subText;
    private GUIStyle extractBtn, harvestBtn, cancelBtn, closeBtn, okBtn;
    private GUIStyle timerStyle;
    private Texture2D extractColumnTex, harvestColumnTex;

    private void Awake()
    {
        Instance = this;
    }

    private void Update()
    {
        if (!IsShowing) return;

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Close();
            return;
        }

        // Auto-close if creature recovered, was destroyed, or player moved too far
        if (targetHealth == null || !targetHealth.gameObject.activeInHierarchy)
        {
            Close();
            return;
        }

        if (state == State.Choice)
        {
            if (!targetHealth.IsIncapacitated)
            {
                Close();
                if (HarvestFlashUI.Instance != null)
                    HarvestFlashUI.Instance.ShowFlash("SPECIMEN RECOVERED",
                        "The creature escaped",
                        HarvestUIStyles.WarningAmber, HarvestUIStyles.MutedTan,
                        HarvestUIStyles.FlashAmberBG, 2.5f);
                return;
            }

            float dist = Vector3.Distance(transform.position, targetHealth.transform.position);
            if (dist > 8f)
            {
                Close();
                return;
            }
        }
    }

    public void Show(EnemyHealth enemy, CreatureLootTable loot)
    {
        if (IsShowing) return;

        targetHealth = enemy;
        targetLoot = loot;
        targetIncapCtrl = enemy.GetComponent<IncapacitationController>();
        creatureName = enemy.gameObject.name.Replace("(Clone)", "").Trim();

        state = State.Choice;
        IsShowing = true;
        GraftMenuUI.MenuOpen = true;
        FreezePlayer(true);
    }

    public void Close()
    {
        IsShowing = false;
        GraftMenuUI.MenuOpen = false;
        targetHealth = null;
        targetLoot = null;
        targetIncapCtrl = null;
        FreezePlayer(false);
    }

    private void FreezePlayer(bool freeze)
    {
        if (freeze)
        {
            prevLockState = Cursor.lockState;
            prevCursorVisible = Cursor.visible;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else
        {
            Cursor.lockState = prevLockState;
            Cursor.visible = prevCursorVisible;
        }

        var cam = FindObjectOfType<PlayerCam>();
        if (cam != null) cam.enabled = !freeze;
        var rot = GetComponent<RotatePlayer>();
        if (rot == null) rot = GetComponentInParent<RotatePlayer>();
        if (rot != null) rot.enabled = !freeze;
        var move = GetComponent<PlayerMovement>();
        if (move == null) move = GetComponentInParent<PlayerMovement>();
        if (move != null) move.enabled = !freeze;
        var attack = GetComponent<AttackController>();
        if (attack == null) attack = GetComponentInParent<AttackController>();
        if (attack != null) attack.enabled = !freeze;
    }

    // ---- OnGUI ----

    private void InitStyles()
    {
        if (stylesReady) return;

        panelBox = HarvestUIStyles.MakeBox(HarvestUIStyles.MakeTex(new Color(0.05f, 0.05f, 0.06f, 0.92f)));

        titleStyle = HarvestUIStyles.MakeLabel(24, HarvestUIStyles.CleanWhite, FontStyle.Bold, TextAnchor.MiddleCenter);
        subtitleStyle = HarvestUIStyles.MakeLabel(16, HarvestUIStyles.MidGray, FontStyle.Normal, TextAnchor.MiddleCenter);
        sectionHeader = HarvestUIStyles.MakeLabel(16, HarvestUIStyles.CleanWhite, FontStyle.Bold);
        bodyText = HarvestUIStyles.MakeLabel(14, HarvestUIStyles.LightGray);
        subText = HarvestUIStyles.MakeLabel(13, HarvestUIStyles.MidGray, FontStyle.Italic);
        timerStyle = HarvestUIStyles.MakeLabel(13, HarvestUIStyles.CleanWhite, FontStyle.Bold, TextAnchor.MiddleCenter);

        extractBtn = HarvestUIStyles.MakeButton(18, HarvestUIStyles.CleanWhite,
            HarvestUIStyles.MakeTex(HarvestUIStyles.ExtractButtonBG),
            HarvestUIStyles.MakeTex(HarvestUIStyles.ExtractButtonHover));

        harvestBtn = HarvestUIStyles.MakeButton(18, HarvestUIStyles.CleanWhite,
            HarvestUIStyles.MakeTex(HarvestUIStyles.GraftButtonBG),
            HarvestUIStyles.MakeTex(HarvestUIStyles.GraftButtonHover));

        cancelBtn = HarvestUIStyles.MakeButton(16, HarvestUIStyles.MidGray,
            HarvestUIStyles.CancelBtnTex, null, 40);

        okBtn = HarvestUIStyles.MakeButton(18, HarvestUIStyles.CleanWhite,
            HarvestUIStyles.MakeTex(new Color(0.08f, 0.08f, 0.10f, 0.90f)), null, 50);

        closeBtn = new GUIStyle(GUI.skin.button);
        closeBtn.fontSize = 16;
        closeBtn.fontStyle = FontStyle.Bold;
        closeBtn.normal.textColor = Color.white;

        extractColumnTex = HarvestUIStyles.MakeTex(new Color(0.03f, 0.08f, 0.07f, 0.80f));
        harvestColumnTex = HarvestUIStyles.MakeTex(new Color(0.08f, 0.06f, 0.04f, 0.80f));

        stylesReady = true;
    }

    private void OnGUI()
    {
        if (!IsShowing) return;
        InitStyles();

        switch (state)
        {
            case State.Choice: DrawChoice(); break;
            case State.Result: DrawResult(); break;
        }
    }

    // ---- STATE 1: Choice ----
    private void DrawChoice()
    {
        if (targetHealth == null || targetLoot == null) { Close(); return; }

        float w = 560, pad = 25;
        float h = 420;
        Rect panel = CenterPanel(w, h);

        GUI.Box(panel, "", panelBox);
        float y = panel.y + 18;
        float cw = w - pad * 2;

        // Close X
        if (GUI.Button(new Rect(panel.x + w - 40, panel.y + 5, 35, 30), "X", closeBtn)) { Close(); return; }

        // Title
        GUI.Label(new Rect(panel.x, y, w, 32), "DOWNED SPECIMEN", titleStyle);
        y += 28;
        GUI.Label(new Rect(panel.x, y, w, 22), $"{creatureName}  --  1 HP", subtitleStyle);
        y += 30;

        // Recovery timer bar
        float timeRemaining = targetHealth.IncapTimeRemaining;
        float totalDuration = targetLoot.vulnerableWindowDuration;
        float fill = timeRemaining / totalDuration;

        Color timerBarColor;
        string timerLabel;
        if (timeRemaining > 6f)
        {
            timerBarColor = HarvestUIStyles.TealPrimary;
            timerLabel = $"RECOVERING IN {timeRemaining:F0}s";
        }
        else if (timeRemaining > 4f)
        {
            timerBarColor = HarvestUIStyles.WarningAmber;
            timerLabel = $"ACT NOW -- {timeRemaining:F0}s";
        }
        else
        {
            timerBarColor = HarvestUIStyles.DangerRed;
            timerLabel = "RECOVERING SOON";
        }

        float barX = panel.x + pad + 40, barW = cw - 80, barH = 8;
        HarvestUIStyles.DrawBar(new Rect(barX, y, barW, barH), fill, timerBarColor, HarvestUIStyles.TimerEmpty);
        y += 12;
        timerStyle.normal.textColor = timerBarColor;
        GUI.Label(new Rect(panel.x, y, w, 18), timerLabel, timerStyle);
        y += 26;

        HarvestUIStyles.DrawSeparator(new Rect(panel.x + pad, y, cw, 1), HarvestUIStyles.DimGray);
        y += 14;

        // Two columns
        float colW = (cw - 16) / 2f;
        float colX1 = panel.x + pad;
        float colX2 = colX1 + colW + 16;
        float colTop = y;
        float colH = 180;

        // ---- EXTRACT column (teal) ----
        Rect extractCol = new Rect(colX1, colTop, colW, colH);
        GUI.DrawTexture(extractCol, extractColumnTex);

        var extractHeader = HarvestUIStyles.MakeLabel(16, HarvestUIStyles.TealPrimary, FontStyle.Bold, TextAnchor.MiddleCenter);
        GUI.Label(new Rect(colX1, colTop + 8, colW, 22), "EXTRACT DNA", extractHeader);

        float ey = colTop + 34;
        if (targetLoot.dnaSample != null)
        {
            var nameStyle = HarvestUIStyles.MakeLabel(14, HarvestUIStyles.CleanWhite, FontStyle.Bold);
            GUI.Label(new Rect(colX1 + 12, ey, colW - 24, 20), targetLoot.dnaSample.sampleName, nameStyle);
            ey += 22;

            var infoStyle = HarvestUIStyles.MakeLabel(12, HarvestUIStyles.SeaGreen);
            GUI.Label(new Rect(colX1 + 12, ey, colW - 24, 18), $"{targetLoot.species}  |  Full quality", infoStyle);
            ey += 22;

            var noteStyle = HarvestUIStyles.MakeLabel(12, HarvestUIStyles.SeaGreen, FontStyle.Italic);
            GUI.Label(new Rect(colX1 + 12, ey, colW - 24, 18), "Creature lives and flees", noteStyle);
            ey += 20;

            if (targetLoot.dnaSample.energyCost > 0)
            {
                GUI.Label(new Rect(colX1 + 12, ey, colW - 24, 18),
                    $"Energy: {targetLoot.dnaSample.energyCost}", infoStyle);
            }
        }
        else
        {
            GUI.Label(new Rect(colX1 + 12, ey, colW - 24, 20), "No DNA available", subText);
        }

        // Extract button
        bool hasDNA = targetLoot.dnaSample != null;
        if (hasDNA)
        {
            if (GUI.Button(new Rect(colX1 + 10, colTop + colH - 55, colW - 20, 45), "EXTRACT", extractBtn))
            {
                OnExtractChosen();
                return;
            }
        }

        // ---- HARVEST column (amber) ----
        Rect harvestCol = new Rect(colX2, colTop, colW, colH);
        GUI.DrawTexture(harvestCol, harvestColumnTex);

        var harvestHeader = HarvestUIStyles.MakeLabel(16, HarvestUIStyles.AmberPrimary, FontStyle.Bold, TextAnchor.MiddleCenter);
        GUI.Label(new Rect(colX2, colTop + 8, colW, 22), "HARVEST PARTS", harvestHeader);

        float hy = colTop + 34;
        if (targetLoot.possibleGraftDrops != null && targetLoot.possibleGraftDrops.Length > 0)
        {
            int shown = 0;
            foreach (var part in targetLoot.possibleGraftDrops)
            {
                if (part == null) continue;
                if (shown >= 4) { break; } // Max 4 visible

                var partStyle = HarvestUIStyles.MakeLabel(13, HarvestUIStyles.BoneWhite);
                GUI.Label(new Rect(colX2 + 12, hy, colW - 24, 18),
                    $"{part.partName} ({part.slot})", partStyle);
                hy += 20;
                shown++;
            }

            if (shown == 0)
            {
                GUI.Label(new Rect(colX2 + 12, hy, colW - 24, 18), "No parts available", subText);
                hy += 20;
            }

            hy += 4;
            var noteStyle = HarvestUIStyles.MakeLabel(12, HarvestUIStyles.BurntOrange, FontStyle.Italic);
            GUI.Label(new Rect(colX2 + 12, hy, colW - 24, 18), "Creature dies", noteStyle);
        }
        else
        {
            GUI.Label(new Rect(colX2 + 12, hy, colW - 24, 18), "No parts available", subText);
        }

        // Harvest button
        bool hasParts = targetLoot.possibleGraftDrops != null && targetLoot.possibleGraftDrops.Length > 0;
        if (hasParts)
        {
            if (GUI.Button(new Rect(colX2 + 10, colTop + colH - 55, colW - 20, 45), "HARVEST", harvestBtn))
            {
                OnHarvestChosen();
                return;
            }
        }

        // Walk Away button
        y = colTop + colH + 16;
        float btnW = 160;
        if (GUI.Button(new Rect(panel.x + (w - btnW) / 2, y, btnW, 40), "WALK AWAY", cancelBtn))
            Close();
    }

    // ---- Actions ----

    private void OnExtractChosen()
    {
        if (targetIncapCtrl == null || targetLoot == null) { Close(); return; }

        string sampleName = targetLoot.dnaSample != null ? targetLoot.dnaSample.sampleName : "DNA";

        // Give energy
        var energy = GetComponent<SuitEnergy>();
        if (energy == null) energy = GetComponentInParent<SuitEnergy>();
        energy?.OnIncapacitate();

        // Perform extraction — creature lives, DNA to storage, creature flees
        targetIncapCtrl.MercifulExtract();

        resultTitle = "DNA EXTRACTED";
        resultSubtitle = $"{sampleName} -- drone inbound";
        resultTitleColor = HarvestUIStyles.TealPrimary;
        state = State.Result;
    }

    private void OnHarvestChosen()
    {
        if (targetIncapCtrl == null) { Close(); return; }

        // Give energy
        var energy = GetComponent<SuitEnergy>();
        if (energy == null) energy = GetComponentInParent<SuitEnergy>();
        energy?.OnHarvest();

        int partCount = 0;
        if (targetLoot != null && targetLoot.possibleGraftDrops != null)
        {
            foreach (var p in targetLoot.possibleGraftDrops)
                if (p != null) partCount++;
        }

        // Kill creature — parts go to corpse
        targetIncapCtrl.HarvestKill();

        string partWord = partCount == 1 ? "part" : "parts";
        resultTitle = "PARTS HARVESTED";
        resultSubtitle = $"{partCount} {partWord} from {creatureName} -- collect from corpse";
        resultTitleColor = HarvestUIStyles.AmberPrimary;
        state = State.Result;
    }

    // ---- STATE 2: Result ----
    private void DrawResult()
    {
        float w = 400, pad = 25;
        float h = 200;
        Rect panel = CenterPanel(w, h);

        GUI.Box(panel, "", panelBox);
        float y = panel.y + 25;

        var rTitleStyle = HarvestUIStyles.MakeLabel(22, resultTitleColor, FontStyle.Bold, TextAnchor.MiddleCenter);
        GUI.Label(new Rect(panel.x, y, w, 30), resultTitle, rTitleStyle);
        y += 36;

        var rSubStyle = HarvestUIStyles.MakeLabel(15, HarvestUIStyles.LightGray, FontStyle.Normal, TextAnchor.MiddleCenter);
        GUI.Label(new Rect(panel.x + pad, y, w - pad * 2, 22), resultSubtitle, rSubStyle);
        y += 36;

        float btnW = 200;
        if (GUI.Button(new Rect(panel.x + (w - btnW) / 2, panel.y + h - 60, btnW, 45), "OK", okBtn))
            Close();
    }

    // ---- Helpers ----

    private Rect CenterPanel(float w, float h)
    {
        return new Rect((Screen.width - w) / 2f, (Screen.height - h) / 2f, w, h);
    }
}
