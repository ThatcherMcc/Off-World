using UnityEngine;
using OffWorld.Anatomy;

/// <summary>
/// Test UI for the harvest choice menu. Uses OnGUI so no Canvas setup is needed.
/// Opens when the player interacts with a creature corpse.
/// No time pause — the world keeps running. Player can close and reopen freely.
/// Add this to the Player.
/// </summary>
public class HarvestChoiceUI : MonoBehaviour
{
    public static HarvestChoiceUI Instance { get; private set; }

    private bool isShowing;
    private bool showResult;
    private string resultMessage;

    // Pending harvest data
    private GraftPartSO[] pendingDrops;
    private DNASampleSO pendingDNA;
    private float pendingHealthNormalized;
    private string creatureName;

    // Cached references
    private AnatomyManager anatomyManager;
    private PlayerMovement playerMovement;
    private PlayerHealth playerHealth;

    // Snapshot of stats before applying
    private float prevWalk, prevSprint, prevJump, prevDodge, prevAir;
    private int prevMaxHP;

    // Cursor state to restore
    private CursorLockMode previousLockState;
    private bool previousCursorVisible;

    public bool IsShowing => isShowing;

    private void Awake()
    {
        Instance = this;
        anatomyManager = GetComponent<AnatomyManager>();
        if (anatomyManager == null)
            anatomyManager = GetComponentInParent<AnatomyManager>();
        playerMovement = GetComponent<PlayerMovement>();
        if (playerMovement == null)
            playerMovement = GetComponentInParent<PlayerMovement>();
        playerHealth = GetComponent<PlayerHealth>();
        if (playerHealth == null)
            playerHealth = GetComponentInParent<PlayerHealth>();
    }

    /// <summary>
    /// Show the harvest choice menu. Called when player interacts with a creature corpse.
    /// </summary>
    public void Show(GraftPartSO[] drops, DNASampleSO dna, float healthNormalized, string name)
    {
        if (isShowing) return; // Already open

        pendingDrops = drops;
        pendingDNA = dna;
        pendingHealthNormalized = healthNormalized;
        creatureName = name;
        showResult = false;
        resultMessage = "";
        isShowing = true;

        SnapshotStats();

        // Free cursor so player can click buttons
        previousLockState = Cursor.lockState;
        previousCursorVisible = Cursor.visible;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    private void SnapshotStats()
    {
        if (playerMovement != null)
        {
            prevWalk = playerMovement.walkSpeed;
            prevSprint = playerMovement.sprintSpeed;
            prevJump = playerMovement.jumpForce;
            prevDodge = playerMovement.DodgeForce;
            prevAir = playerMovement.airSpeedMultiplier;
        }
        if (playerHealth != null)
        {
            prevMaxHP = playerHealth.maxHealth;
        }
    }

    private string BuildStatDiff()
    {
        if (playerMovement == null || playerHealth == null) return "Could not read player stats.";

        var sb = new System.Text.StringBuilder();
        sb.AppendLine("<b>Stat Changes:</b>\n");

        AppendDiff(sb, "Walk Speed", prevWalk, playerMovement.walkSpeed);
        AppendDiff(sb, "Sprint Speed", prevSprint, playerMovement.sprintSpeed);
        AppendDiff(sb, "Jump Force", prevJump, playerMovement.jumpForce);
        AppendDiff(sb, "Dodge Force", prevDodge, playerMovement.DodgeForce);
        AppendDiff(sb, "Air Control", prevAir, playerMovement.airSpeedMultiplier);
        AppendDiffInt(sb, "Max HP", prevMaxHP, playerHealth.maxHealth);

        if (sb.ToString().Contains("+") || sb.ToString().Contains("-"))
            return sb.ToString();

        return sb + "No stat changes (modifiers are at default values).";
    }

    private void AppendDiff(System.Text.StringBuilder sb, string label, float before, float after)
    {
        float delta = after - before;
        if (Mathf.Abs(delta) < 0.001f) return;
        string sign = delta > 0 ? "+" : "";
        sb.AppendLine($"  {label}: {before:F2} → {after:F2}  ({sign}{delta:F2})");
    }

    private void AppendDiffInt(System.Text.StringBuilder sb, string label, int before, int after)
    {
        int delta = after - before;
        if (delta == 0) return;
        string sign = delta > 0 ? "+" : "";
        sb.AppendLine($"  {label}: {before} → {after}  ({sign}{delta})");
    }

    private void OnGraftChosen()
    {
        if (anatomyManager == null || pendingDrops == null || pendingDrops.Length == 0)
        {
            resultMessage = "No graft parts available or AnatomyManager not found.";
            showResult = true;
            return;
        }

        var part = pendingDrops[0];
        var replaced = anatomyManager.GraftPart(part);

        resultMessage = $"<b>Grafted: {part.partName}</b>\n";
        resultMessage += $"Slot: {part.slot}    Species: {part.species}\n";
        if (replaced != null)
            resultMessage += $"Replaced: {replaced.partName}\n";
        resultMessage += "\n" + BuildStatDiff();

        showResult = true;

        Debug.Log($"[HarvestChoiceUI] Grafted '{part.partName}' into {part.slot}");
    }

    private void OnExtractChosen()
    {
        if (anatomyManager == null || pendingDNA == null)
        {
            resultMessage = "No DNA sample available or AnatomyManager not found.";
            showResult = true;
            return;
        }

        DNATier tier = DNATier.Degraded;

        bool loaded = false;
        for (int i = 0; i < anatomyManager.Suit.UnlockedPassiveSlots; i++)
        {
            if (anatomyManager.Suit.GetPassiveSlot(i) == null)
            {
                anatomyManager.LoadPassiveDNA(i, pendingDNA);
                loaded = true;
                break;
            }
        }
        if (!loaded)
        {
            anatomyManager.LoadPassiveDNA(0, pendingDNA);
        }

        resultMessage = $"<b>Extracted: {pendingDNA.sampleName}</b>\n";
        resultMessage += $"Tier: {tier}    Species: {pendingDNA.species}\n";
        resultMessage += "\n" + BuildStatDiff();

        showResult = true;

        Debug.Log($"[HarvestChoiceUI] Extracted {tier} '{pendingDNA.sampleName}'");
    }

    public void Close()
    {
        isShowing = false;
        showResult = false;
        pendingDrops = null;
        pendingDNA = null;

        // Restore cursor
        Cursor.lockState = previousLockState;
        Cursor.visible = previousCursorVisible;
    }

    // ---- OnGUI rendering ----

    private GUIStyle boxStyle;
    private GUIStyle titleStyle;
    private GUIStyle bodyStyle;
    private GUIStyle buttonStyle;
    private GUIStyle closeBtnStyle;
    private bool stylesInitialized;

    private void InitStyles()
    {
        if (stylesInitialized) return;

        boxStyle = new GUIStyle(GUI.skin.box);
        boxStyle.normal.background = MakeTex(2, 2, new Color(0.08f, 0.08f, 0.12f, 0.95f));

        titleStyle = new GUIStyle(GUI.skin.label);
        titleStyle.fontSize = 22;
        titleStyle.fontStyle = FontStyle.Bold;
        titleStyle.alignment = TextAnchor.MiddleCenter;
        titleStyle.normal.textColor = new Color(0.9f, 0.85f, 0.6f);
        titleStyle.richText = true;

        bodyStyle = new GUIStyle(GUI.skin.label);
        bodyStyle.fontSize = 16;
        bodyStyle.wordWrap = true;
        bodyStyle.normal.textColor = Color.white;
        bodyStyle.richText = true;

        buttonStyle = new GUIStyle(GUI.skin.button);
        buttonStyle.fontSize = 18;
        buttonStyle.fontStyle = FontStyle.Bold;
        buttonStyle.fixedHeight = 50;
        buttonStyle.normal.textColor = Color.white;

        closeBtnStyle = new GUIStyle(GUI.skin.button);
        closeBtnStyle.fontSize = 16;
        closeBtnStyle.fontStyle = FontStyle.Bold;
        closeBtnStyle.normal.textColor = Color.white;

        stylesInitialized = true;
    }

    private Texture2D MakeTex(int width, int height, Color col)
    {
        var pix = new Color[width * height];
        for (int i = 0; i < pix.Length; i++) pix[i] = col;
        var tex = new Texture2D(width, height);
        tex.SetPixels(pix);
        tex.Apply();
        return tex;
    }

    private void OnGUI()
    {
        if (!isShowing) return;

        InitStyles();

        float panelW = 460;
        float panelH = showResult ? 420 : 320;
        float panelX = (Screen.width - panelW) / 2f;
        float panelY = (Screen.height - panelH) / 2f;
        Rect panelRect = new Rect(panelX, panelY, panelW, panelH);

        GUI.Box(panelRect, "", boxStyle);

        float y = panelY + 15;
        float pad = 20;
        float contentW = panelW - pad * 2;

        // Close X button (top-right corner)
        if (GUI.Button(new Rect(panelX + panelW - 40, panelY + 5, 35, 30), "X", closeBtnStyle))
        {
            Close();
            return;
        }

        // Title
        GUI.Label(new Rect(panelX, y, panelW, 35), $"Creature Killed: {creatureName}", titleStyle);
        y += 45;

        if (!showResult)
        {
            // Description
            string desc = "";
            if (pendingDrops != null && pendingDrops.Length > 0)
                desc += $"Body Part: {pendingDrops[0].partName} ({pendingDrops[0].slot})\n";
            if (pendingDNA != null)
                desc += $"DNA Available: {pendingDNA.sampleName}";

            GUI.Label(new Rect(panelX + pad, y, contentW, 60), desc, bodyStyle);
            y += 70;

            // Graft button
            bool hasGraft = pendingDrops != null && pendingDrops.Length > 0;
            GUI.enabled = hasGraft;
            if (GUI.Button(new Rect(panelX + pad, y, contentW, 50),
                hasGraft ? $"Graft: {pendingDrops[0].partName}" : "No Parts Dropped", buttonStyle))
            {
                OnGraftChosen();
            }
            GUI.enabled = true;
            y += 60;

            // Extract button
            bool hasDNA = pendingDNA != null;
            GUI.enabled = hasDNA;
            if (GUI.Button(new Rect(panelX + pad, y, contentW, 50),
                hasDNA ? $"Extract DNA: {pendingDNA.sampleName}" : "No DNA Available", buttonStyle))
            {
                OnExtractChosen();
            }
            GUI.enabled = true;
        }
        else
        {
            // Result display
            GUI.Label(new Rect(panelX + pad, y, contentW, 280), resultMessage, bodyStyle);
            y = panelY + panelH - 70;

            if (GUI.Button(new Rect(panelX + pad, y, contentW, 50), "OK", buttonStyle))
            {
                Close();
            }
        }
    }
}
