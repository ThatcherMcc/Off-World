using UnityEngine;
using OffWorld.Anatomy;

/// <summary>
/// Graft menu UI — the kill path. 3 states:
/// 1. Part Selection (if multiple drops)
/// 2. Confirmation with warning ("This is permanent")
/// 3. Result summary
///
/// Opened by CreatureCorpse.Interact(). OnGUI-based, no Canvas needed.
/// Add to the Player GameObject.
/// </summary>
public class GraftMenuUI : MonoBehaviour
{
    public static GraftMenuUI Instance { get; private set; }
    public static bool MenuOpen { get; set; }
    public bool IsShowing { get; private set; }

    private enum State { Selection, Confirmation, Result }
    private State state;

    // Data from the corpse
    private GraftPartSO[] drops;
    private string creatureName;
    private CreatureCorpse activeCorpse;

    // Selected part for confirmation/result
    private GraftPartSO selectedPart;
    private GraftPartSO replacedPart;

    // Stat snapshots for preview
    private float prevWalk, prevSprint, prevJump, prevDodge, prevAir;
    private int prevMaxHP;
    private float afterWalk, afterSprint, afterJump, afterDodge, afterAir;
    private int afterMaxHP;

    // Species info
    private int sameSpeciesBefore, sameSpeciesAfter;
    private int speciesCountAfter;

    // References
    private AnatomyManager anatomyManager;
    private PlayerMovement playerMovement;
    private PlayerHealth playerHealth;

    // Cursor restore
    private CursorLockMode prevLockState;
    private bool prevCursorVisible;

    // Styles (lazy init)
    private bool stylesReady;
    private GUIStyle panelBox, titleStyle, subtitleStyle, sectionHeader, bodyText, subText;
    private GUIStyle warningText, partNameStyle, partInfoStyle, partDescStyle;
    private GUIStyle graftBtn, cancelBtn, closeBtn, okBtn, walkAwayBtn;
    private GUIStyle statPositive, statNegative, statNeutral;

    private void Awake()
    {
        Instance = this;
        anatomyManager = GetComponent<AnatomyManager>();
        if (anatomyManager == null) anatomyManager = GetComponentInParent<AnatomyManager>();
        playerMovement = GetComponent<PlayerMovement>();
        if (playerMovement == null) playerMovement = GetComponentInParent<PlayerMovement>();
        playerHealth = GetComponent<PlayerHealth>();
        if (playerHealth == null) playerHealth = GetComponentInParent<PlayerHealth>();
    }

    private void Update()
    {
        if (IsShowing && Input.GetKeyDown(KeyCode.Escape))
            Close();
    }

    public void Show(GraftPartSO[] availableDrops, string creature, CreatureCorpse corpse)
    {
        if (IsShowing) return;

        drops = availableDrops;
        creatureName = creature;
        activeCorpse = corpse;
        selectedPart = null;
        replacedPart = null;

        if (drops != null && drops.Length == 1)
        {
            selectedPart = drops[0];
            CalculatePreview(selectedPart);
            state = State.Confirmation;
        }
        else
        {
            state = State.Selection;
        }

        IsShowing = true;
        MenuOpen = true;
        FreezePlayer(true);
    }

    public void Close()
    {
        IsShowing = false;
        MenuOpen = false;
        drops = null;
        selectedPart = null;
        replacedPart = null;
        activeCorpse = null;
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

        // Disable player controls while menu is open
        var cam = FindObjectOfType<PlayerCam>();
        if (cam != null) cam.enabled = !freeze;
        var rot = GetComponent<RotatePlayer>();
        if (rot == null) rot = GetComponentInParent<RotatePlayer>();
        if (rot != null) rot.enabled = !freeze;
        if (playerMovement != null) playerMovement.enabled = !freeze;
        var attack = GetComponent<AttackController>();
        if (attack == null) attack = GetComponentInParent<AttackController>();
        if (attack != null) attack.enabled = !freeze;
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
            prevMaxHP = playerHealth.maxHealth;
    }

    private void CalculatePreview(GraftPartSO part)
    {
        SnapshotStats();

        if (anatomyManager != null)
        {
            var currentInSlot = anatomyManager.Grafts.GetGraft(part.slot);

            afterWalk = prevWalk;
            afterSprint = prevSprint;
            afterJump = prevJump;
            afterDodge = prevDodge;
            afterAir = prevAir;
            afterMaxHP = prevMaxHP;

            if (currentInSlot != null)
            {
                var oldM = currentInSlot.modifiers;
                var newM = part.modifiers;
                float walkMultRatio = oldM.walkSpeedMult != 0 ? newM.walkSpeedMult / oldM.walkSpeedMult : newM.walkSpeedMult;
                float sprintMultRatio = oldM.sprintSpeedMult != 0 ? newM.sprintSpeedMult / oldM.sprintSpeedMult : newM.sprintSpeedMult;
                float jumpMultRatio = oldM.jumpForceMult != 0 ? newM.jumpForceMult / oldM.jumpForceMult : newM.jumpForceMult;
                float dodgeMultRatio = oldM.dodgeForceMult != 0 ? newM.dodgeForceMult / oldM.dodgeForceMult : newM.dodgeForceMult;
                float hpMultRatio = oldM.maxHPMult != 0 ? newM.maxHPMult / oldM.maxHPMult : newM.maxHPMult;

                afterWalk = (prevWalk + newM.walkSpeedFlat - oldM.walkSpeedFlat) * walkMultRatio;
                afterSprint = (prevSprint + newM.sprintSpeedFlat - oldM.sprintSpeedFlat) * sprintMultRatio;
                afterJump = (prevJump + newM.jumpForceFlat - oldM.jumpForceFlat) * jumpMultRatio;
                afterDodge = (prevDodge + newM.dodgeForceFlat - oldM.dodgeForceFlat) * dodgeMultRatio;
                afterMaxHP = Mathf.RoundToInt((prevMaxHP + newM.maxHPFlat - oldM.maxHPFlat) * hpMultRatio);
            }
            else
            {
                var m = part.modifiers;
                afterWalk = prevWalk + m.walkSpeedFlat;
                if (m.walkSpeedMult != 1f) afterWalk *= m.walkSpeedMult;
                afterSprint = prevSprint + m.sprintSpeedFlat;
                if (m.sprintSpeedMult != 1f) afterSprint *= m.sprintSpeedMult;
                afterJump = prevJump + m.jumpForceFlat;
                if (m.jumpForceMult != 1f) afterJump *= m.jumpForceMult;
                afterDodge = prevDodge + m.dodgeForceFlat;
                if (m.dodgeForceMult != 1f) afterDodge *= m.dodgeForceMult;
                afterMaxHP = Mathf.RoundToInt((prevMaxHP + m.maxHPFlat) * m.maxHPMult);
            }

            var counts = anatomyManager.Grafts.GetSpeciesCounts();
            sameSpeciesBefore = counts.ContainsKey(part.species) ? counts[part.species] : 0;
            sameSpeciesAfter = sameSpeciesBefore + 1;
            if (currentInSlot != null && currentInSlot.species == part.species)
                sameSpeciesAfter = sameSpeciesBefore;

            speciesCountAfter = counts.Count;
            if (!counts.ContainsKey(part.species))
                speciesCountAfter++;
            if (currentInSlot != null && currentInSlot.species != part.species)
            {
                int oldSpeciesCount = counts.ContainsKey(currentInSlot.species) ? counts[currentInSlot.species] : 0;
                if (oldSpeciesCount <= 1) speciesCountAfter--;
            }

            replacedPart = currentInSlot;
        }
    }

    private void OnGraftConfirmed()
    {
        if (anatomyManager == null || selectedPart == null) return;

        SnapshotStats();
        replacedPart = anatomyManager.GraftPart(selectedPart);

        afterWalk = playerMovement.walkSpeed;
        afterSprint = playerMovement.sprintSpeed;
        afterJump = playerMovement.jumpForce;
        afterDodge = playerMovement.DodgeForce;
        afterAir = playerMovement.airSpeedMultiplier;
        afterMaxHP = playerHealth.maxHealth;

        if (activeCorpse != null)
        {
            activeCorpse.RemoveDrop(selectedPart);
            Destroy(activeCorpse.gameObject);
            activeCorpse = null;
        }

        state = State.Result;
    }

    // ---- OnGUI ----

    private void InitStyles()
    {
        if (stylesReady) return;

        panelBox = HarvestUIStyles.MakeBox(HarvestUIStyles.GraftPanelTex);
        titleStyle = HarvestUIStyles.MakeLabel(24, HarvestUIStyles.AmberPrimary, FontStyle.Bold, TextAnchor.MiddleCenter);
        subtitleStyle = HarvestUIStyles.MakeLabel(16, HarvestUIStyles.BurntOrange, FontStyle.BoldAndItalic, TextAnchor.MiddleCenter);
        sectionHeader = HarvestUIStyles.MakeLabel(16, HarvestUIStyles.BoneWhite, FontStyle.Bold);
        bodyText = HarvestUIStyles.MakeLabel(15, HarvestUIStyles.LightGray);
        subText = HarvestUIStyles.MakeLabel(14, HarvestUIStyles.MutedTan, FontStyle.Italic);
        warningText = HarvestUIStyles.MakeLabel(16, HarvestUIStyles.BurntOrange, FontStyle.Bold);

        partNameStyle = HarvestUIStyles.MakeLabel(18, HarvestUIStyles.BoneWhite, FontStyle.Bold);
        partInfoStyle = HarvestUIStyles.MakeLabel(14, HarvestUIStyles.MutedTan);
        partDescStyle = HarvestUIStyles.MakeLabel(14, HarvestUIStyles.LightGray);

        graftBtn = HarvestUIStyles.MakeButton(18, HarvestUIStyles.BoneWhite,
            HarvestUIStyles.MakeTex(HarvestUIStyles.GraftButtonBG),
            HarvestUIStyles.MakeTex(HarvestUIStyles.GraftButtonHover));

        cancelBtn = HarvestUIStyles.MakeButton(16, HarvestUIStyles.MidGray,
            HarvestUIStyles.CancelBtnTex, null, 50);

        okBtn = HarvestUIStyles.MakeButton(18, HarvestUIStyles.BoneWhite,
            HarvestUIStyles.MakeTex(new Color(0.24f, 0.20f, 0.16f, 0.90f)), null, 50);

        walkAwayBtn = HarvestUIStyles.MakeButton(16, HarvestUIStyles.MidGray,
            HarvestUIStyles.CancelBtnTex, null, 40);

        closeBtn = new GUIStyle(GUI.skin.button);
        closeBtn.fontSize = 16;
        closeBtn.fontStyle = FontStyle.Bold;
        closeBtn.normal.textColor = Color.white;

        statPositive = HarvestUIStyles.MakeLabel(15, HarvestUIStyles.PositiveGreen, FontStyle.Normal, TextAnchor.UpperLeft, false, false);
        statNegative = HarvestUIStyles.MakeLabel(15, HarvestUIStyles.DangerRed, FontStyle.Normal, TextAnchor.UpperLeft, false, false);
        statNeutral = HarvestUIStyles.MakeLabel(15, HarvestUIStyles.DimGray, FontStyle.Normal, TextAnchor.UpperLeft, false, false);

        stylesReady = true;
    }

    private void OnGUI()
    {
        if (!IsShowing) return;
        InitStyles();

        if (activeCorpse == null && state != State.Result)
        {
            Close();
            if (HarvestFlashUI.Instance != null)
                HarvestFlashUI.Instance.ShowFlash("REMAINS DECOMPOSED", "The body has decayed",
                    HarvestUIStyles.DarkSienna, HarvestUIStyles.MutedTan, HarvestUIStyles.FlashSiennaBG, 2.5f);
            return;
        }

        switch (state)
        {
            case State.Selection: DrawSelection(); break;
            case State.Confirmation: DrawConfirmation(); break;
            case State.Result: DrawResult(); break;
        }
    }

    // ---- STATE 1: Part Selection ----
    private void DrawSelection()
    {
        float w = 480, pad = 25;
        int partCount = drops != null ? drops.Length : 0;
        float entryH = 75;
        float h = 180 + partCount * (entryH + 10) + 60;
        h = Mathf.Max(h, 300);
        Rect panel = CenterPanel(w, h);

        GUI.Box(panel, "", panelBox);
        float y = panel.y + 20;
        float cw = w - pad * 2;

        if (GUI.Button(new Rect(panel.x + w - 40, panel.y + 5, 35, 30), "X", closeBtn)) { Close(); return; }

        GUI.Label(new Rect(panel.x, y, w, 32), "BIOLOGICAL HARVEST", titleStyle);
        y += 34;
        var creatureStyle = HarvestUIStyles.MakeLabel(18, HarvestUIStyles.MutedTan, FontStyle.Normal, TextAnchor.MiddleCenter);
        GUI.Label(new Rect(panel.x, y, w, 25), creatureName, creatureStyle);
        y += 35;

        HarvestUIStyles.DrawSeparator(new Rect(panel.x + pad, y, cw, 1), HarvestUIStyles.MutedTan);
        y += 15;

        if (partCount == 0)
        {
            GUI.Label(new Rect(panel.x + pad, y, cw, 30), "No harvestable parts remain.", subText);
        }
        else
        {
            GUI.Label(new Rect(panel.x + pad, y, cw, 25), "Select a body part to graft:", bodyText);
            y += 35;

            for (int i = 0; i < drops.Length; i++)
            {
                if (drops[i] == null) continue;
                var part = drops[i];

                Rect entryRect = new Rect(panel.x + pad, y, cw, entryH);
                bool hover = entryRect.Contains(Event.current.mousePosition);
                GUI.DrawTexture(entryRect, hover ? HarvestUIStyles.GraftEntryHoverTex : HarvestUIStyles.GraftEntryTex);

                GUI.Label(new Rect(entryRect.x + 12, entryRect.y + 8, cw - 24, 24), part.partName.ToUpper(), partNameStyle);
                GUI.Label(new Rect(entryRect.x + 12, entryRect.y + 32, cw - 24, 20),
                    $"Slot: {part.slot}   |   {part.species}   |   {part.tier}", partInfoStyle);
                if (!string.IsNullOrEmpty(part.description))
                    GUI.Label(new Rect(entryRect.x + 12, entryRect.y + 52, cw - 24, 20), part.description, partDescStyle);

                if (GUI.Button(entryRect, "", GUIStyle.none))
                {
                    selectedPart = part;
                    CalculatePreview(part);
                    state = State.Confirmation;
                }

                y += entryH + 10;
            }
        }

        y += 15;
        float btnW = 160;
        if (GUI.Button(new Rect(panel.x + (w - btnW) / 2, y, btnW, 40), "WALK AWAY", walkAwayBtn))
            Close();
    }

    // ---- STATE 2: Confirmation ----
    private void DrawConfirmation()
    {
        float w = 480, pad = 25;
        float h = 480;
        Rect panel = CenterPanel(w, h);

        GUI.Box(panel, "", panelBox);
        float y = panel.y + 20;
        float cw = w - pad * 2;

        if (GUI.Button(new Rect(panel.x + w - 40, panel.y + 5, 35, 30), "X", closeBtn)) { Close(); return; }

        // Header
        GUI.Label(new Rect(panel.x, y, w, 32), $"GRAFT: {selectedPart.partName.ToUpper()}", titleStyle);
        y += 30;
        GUI.Label(new Rect(panel.x, y, w, 22), "This is permanent.", subtitleStyle);
        y += 32;

        HarvestUIStyles.DrawSeparator(new Rect(panel.x + pad, y, cw, 1), HarvestUIStyles.MutedTan);
        y += 14;

        // Slot + what you're replacing
        string currentText = replacedPart != null
            ? $"Replacing: {replacedPart.partName} ({replacedPart.species})"
            : $"Slot: {selectedPart.slot} (empty)";
        GUI.Label(new Rect(panel.x + pad, y, cw, 22), currentText, bodyText);
        y += 28;

        // Description
        if (!string.IsNullOrEmpty(selectedPart.description))
        {
            GUI.Label(new Rect(panel.x + pad, y, cw, 22), selectedPart.description, bodyText);
            y += 26;
        }

        HarvestUIStyles.DrawSeparator(new Rect(panel.x + pad, y, cw, 1), HarvestUIStyles.MutedTan);
        y += 12;

        // Stat Preview - only show changed stats
        GUI.Label(new Rect(panel.x + pad, y, cw, 22), "STAT CHANGES:", sectionHeader);
        y += 26;
        bool anyChange = false;
        anyChange |= DrawStatCheck(panel.x + pad + 10, cw - 10, "Walk Speed", prevWalk, afterWalk, ref y);
        anyChange |= DrawStatCheck(panel.x + pad + 10, cw - 10, "Sprint", prevSprint, afterSprint, ref y);
        anyChange |= DrawStatCheck(panel.x + pad + 10, cw - 10, "Jump", prevJump, afterJump, ref y);
        anyChange |= DrawStatCheck(panel.x + pad + 10, cw - 10, "Dodge", prevDodge, afterDodge, ref y);
        anyChange |= DrawStatCheckInt(panel.x + pad + 10, cw - 10, "Max HP", prevMaxHP, afterMaxHP, ref y);
        if (!anyChange)
        {
            GUI.Label(new Rect(panel.x + pad + 10, y, cw - 10, 20), "No stat changes.", statNeutral);
            y += 22;
        }
        y += 8;

        // Species (condensed to one line)
        bool dangerIncompat = speciesCountAfter >= 4;
        if (dangerIncompat)
        {
            var dangerStyle = HarvestUIStyles.MakeLabel(14, HarvestUIStyles.DangerRed, FontStyle.Bold);
            GUI.Label(new Rect(panel.x + pad, y, cw, 20), $"WARNING: {speciesCountAfter} species -- rejection active!", dangerStyle);
            y += 24;
        }
        else
        {
            var spStyle = HarvestUIStyles.MakeLabel(14, sameSpeciesAfter >= 3 ? HarvestUIStyles.PositiveGreen : HarvestUIStyles.MutedTan);
            string spText = sameSpeciesAfter >= 3
                ? $"AFFINITY UNLOCKED: {selectedPart.species} (3/3)"
                : $"{selectedPart.species}: {sameSpeciesAfter}/3 for affinity";
            GUI.Label(new Rect(panel.x + pad, y, cw, 20), spText, spStyle);
            y += 24;
        }

        // Warning block
        Rect warningRect = new Rect(panel.x + pad, y, cw, 52);
        GUI.DrawTexture(warningRect, HarvestUIStyles.WarningBGTex);
        GUI.Label(new Rect(warningRect.x + 12, warningRect.y + 6, cw - 24, 22),
            "! Are you sure you want to do this to yourself?", warningText);
        string warnLine2 = replacedPart != null
            ? $"! {replacedPart.partName} will be destroyed forever."
            : "! This cannot be undone.";
        var warn2Style = HarvestUIStyles.MakeLabel(14, HarvestUIStyles.BurntOrange);
        GUI.Label(new Rect(warningRect.x + 12, warningRect.y + 28, cw - 24, 20), warnLine2, warn2Style);
        y += 60;

        // Buttons
        float graftW = cw * 0.58f;
        float cancelW = cw * 0.35f;
        float gap = cw - graftW - cancelW;

        if (GUI.Button(new Rect(panel.x + pad, y, graftW, 50), "GRAFT ONTO BODY", graftBtn))
            OnGraftConfirmed();
        if (GUI.Button(new Rect(panel.x + pad + graftW + gap, y, cancelW, 50), "CANCEL", cancelBtn))
        {
            if (drops != null && drops.Length > 1)
                state = State.Selection;
            else
                Close();
        }
    }

    // ---- STATE 3: Result ----
    private void DrawResult()
    {
        float w = 440, pad = 25;
        float h = 320;
        Rect panel = CenterPanel(w, h);

        GUI.Box(panel, "", panelBox);
        float y = panel.y + 20;
        float cw = w - pad * 2;

        GUI.Label(new Rect(panel.x, y, w, 32), $"GRAFTED: {selectedPart.partName.ToUpper()}", titleStyle);
        y += 30;
        GUI.Label(new Rect(panel.x, y, w, 22), "Your body has changed.", subtitleStyle);
        y += 34;

        HarvestUIStyles.DrawSeparator(new Rect(panel.x + pad, y, cw, 1), HarvestUIStyles.MutedTan);
        y += 14;

        GUI.Label(new Rect(panel.x + pad, y, cw, 22),
            $"{selectedPart.slot}: {selectedPart.partName} ({selectedPart.species})", sectionHeader);
        y += 28;

        // Only show changed stats
        bool anyChange = false;
        anyChange |= DrawStatCheck(panel.x + pad + 10, cw - 10, "Walk Speed", prevWalk, afterWalk, ref y);
        anyChange |= DrawStatCheck(panel.x + pad + 10, cw - 10, "Sprint", prevSprint, afterSprint, ref y);
        anyChange |= DrawStatCheck(panel.x + pad + 10, cw - 10, "Jump", prevJump, afterJump, ref y);
        anyChange |= DrawStatCheck(panel.x + pad + 10, cw - 10, "Dodge", prevDodge, afterDodge, ref y);
        anyChange |= DrawStatCheckInt(panel.x + pad + 10, cw - 10, "Max HP", prevMaxHP, afterMaxHP, ref y);
        if (!anyChange)
        {
            GUI.Label(new Rect(panel.x + pad + 10, y, cw - 10, 20), "No stat changes.", statNeutral);
            y += 22;
        }
        y += 8;

        if (replacedPart != null)
        {
            var replStyle = HarvestUIStyles.MakeLabel(14, HarvestUIStyles.BurntOrange);
            GUI.Label(new Rect(panel.x + pad, y, cw, 20), $"Replaced: {replacedPart.partName} (destroyed)", replStyle);
            y += 24;
        }

        float btnW = 200;
        if (GUI.Button(new Rect(panel.x + (w - btnW) / 2, panel.y + h - 65, btnW, 50), "OK", okBtn))
            Close();
    }

    // ---- Helpers ----

    private Rect CenterPanel(float w, float h)
    {
        return new Rect((Screen.width - w) / 2f, (Screen.height - h) / 2f, w, h);
    }

    private bool DrawStatCheck(float x, float w, string label, float before, float after, ref float y)
    {
        float diff = after - before;
        if (Mathf.Abs(diff) < 0.01f) return false;

        GUIStyle style = diff > 0 ? statPositive : statNegative;
        string sign = diff > 0 ? "+" : "";
        GUI.Label(new Rect(x, y, w, 20), $"{label}:  {before:F1}  -->  {after:F1}  ({sign}{diff:F1})", style);
        y += 22;
        return true;
    }

    private bool DrawStatCheckInt(float x, float w, string label, int before, int after, ref float y)
    {
        int diff = after - before;
        if (diff == 0) return false;

        GUIStyle style = diff > 0 ? statPositive : statNegative;
        string sign = diff > 0 ? "+" : "";
        GUI.Label(new Rect(x, y, w, 20), $"{label}:  {before}  -->  {after}  ({sign}{diff})", style);
        y += 22;
        return true;
    }
}
