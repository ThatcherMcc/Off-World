using System.Collections.Generic;
using UnityEngine;
using OffWorld.Anatomy;

/// <summary>
/// Surgery Table UI — base station for equipping graft parts from BaseStorage.
/// States:
/// 1. BodyDiagram — shows 6 body slots with current equipment and storage counts
/// 2. PartSelection — shows stored parts for the selected slot
/// 3. Confirmation — stat preview and permanence warning
/// 4. Result — summary of what was grafted
///
/// OnGUI-based, no Canvas needed. Add to the Player GameObject.
/// </summary>
public class SurgeryTableUI : MonoBehaviour
{
    public static SurgeryTableUI Instance { get; private set; }
    public bool IsShowing { get; private set; }

    private enum State { BodyDiagram, PartSelection, Confirmation, Result }
    private State state;

    // Selection data
    private BodySlot selectedSlot;
    private GraftPartSO selectedPart;
    private GraftPartSO replacedPart;

    // Stat snapshots
    private float prevWalk, prevSprint, prevJump, prevDodge, prevAir;
    private int prevMaxHP;
    private float afterWalk, afterSprint, afterJump, afterDodge, afterAir;
    private int afterMaxHP;

    // References
    private AnatomyManager anatomyManager;
    private PlayerMovement playerMovement;
    private PlayerHealth playerHealth;

    // Cursor restore
    private CursorLockMode prevLockState;
    private bool prevCursorVisible;

    // Styles
    private bool stylesReady;
    private GUIStyle panelBox, titleStyle, subtitleStyle, sectionHeader, bodyText, subText;
    private GUIStyle graftBtn, cancelBtn, closeBtn, okBtn, backBtn;
    private GUIStyle slotEmpty, slotFilled, slotHover;
    private GUIStyle warningText;
    private GUIStyle statPositive, statNegative, statNeutral;
    private Texture2D entryTex, entryHoverTex;

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

    public void Show()
    {
        if (IsShowing) return;

        selectedPart = null;
        replacedPart = null;
        state = State.BodyDiagram;
        IsShowing = true;
        GraftMenuUI.MenuOpen = true;
        FreezePlayer(true);
    }

    public void Close()
    {
        IsShowing = false;
        GraftMenuUI.MenuOpen = false;
        selectedPart = null;
        replacedPart = null;
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
        if (playerMovement != null) playerMovement.enabled = !freeze;
        var attack = GetComponent<AttackController>();
        if (attack == null) attack = GetComponentInParent<AttackController>();
        if (attack != null) attack.enabled = !freeze;
    }

    // ---- OnGUI ----

    private void InitStyles()
    {
        if (stylesReady) return;

        panelBox = HarvestUIStyles.MakeBox(HarvestUIStyles.GraftPanelTex);
        titleStyle = HarvestUIStyles.MakeLabel(24, HarvestUIStyles.AmberPrimary, FontStyle.Bold, TextAnchor.MiddleCenter);
        subtitleStyle = HarvestUIStyles.MakeLabel(16, HarvestUIStyles.BurntOrange, FontStyle.Normal, TextAnchor.MiddleCenter);
        sectionHeader = HarvestUIStyles.MakeLabel(16, HarvestUIStyles.BoneWhite, FontStyle.Bold);
        bodyText = HarvestUIStyles.MakeLabel(15, HarvestUIStyles.LightGray);
        subText = HarvestUIStyles.MakeLabel(14, HarvestUIStyles.MutedTan, FontStyle.Italic);
        warningText = HarvestUIStyles.MakeLabel(16, HarvestUIStyles.BurntOrange, FontStyle.Bold);

        graftBtn = HarvestUIStyles.MakeButton(18, HarvestUIStyles.BoneWhite,
            HarvestUIStyles.MakeTex(HarvestUIStyles.GraftButtonBG),
            HarvestUIStyles.MakeTex(HarvestUIStyles.GraftButtonHover));

        cancelBtn = HarvestUIStyles.MakeButton(16, HarvestUIStyles.MidGray,
            HarvestUIStyles.CancelBtnTex, null, 50);

        okBtn = HarvestUIStyles.MakeButton(18, HarvestUIStyles.BoneWhite,
            HarvestUIStyles.MakeTex(new Color(0.24f, 0.20f, 0.16f, 0.90f)), null, 50);

        backBtn = HarvestUIStyles.MakeButton(16, HarvestUIStyles.MidGray,
            HarvestUIStyles.CancelBtnTex, null, 40);

        closeBtn = new GUIStyle(GUI.skin.button);
        closeBtn.fontSize = 16;
        closeBtn.fontStyle = FontStyle.Bold;
        closeBtn.normal.textColor = Color.white;

        slotEmpty = HarvestUIStyles.MakeButton(14, HarvestUIStyles.MutedTan,
            HarvestUIStyles.MakeTex(new Color(0.08f, 0.06f, 0.05f, 0.80f)),
            HarvestUIStyles.MakeTex(new Color(0.14f, 0.10f, 0.08f, 0.90f)), 70);

        slotFilled = HarvestUIStyles.MakeButton(14, HarvestUIStyles.BoneWhite,
            HarvestUIStyles.MakeTex(new Color(0.14f, 0.10f, 0.07f, 0.85f)),
            HarvestUIStyles.MakeTex(new Color(0.20f, 0.14f, 0.10f, 0.90f)), 70);

        entryTex = HarvestUIStyles.MakeTex(HarvestUIStyles.GraftEntryBG);
        entryHoverTex = HarvestUIStyles.MakeTex(HarvestUIStyles.GraftEntryHover);

        statPositive = HarvestUIStyles.MakeLabel(15, HarvestUIStyles.PositiveGreen, FontStyle.Normal, TextAnchor.UpperLeft, false, false);
        statNegative = HarvestUIStyles.MakeLabel(15, HarvestUIStyles.DangerRed, FontStyle.Normal, TextAnchor.UpperLeft, false, false);
        statNeutral = HarvestUIStyles.MakeLabel(15, HarvestUIStyles.DimGray, FontStyle.Normal, TextAnchor.UpperLeft, false, false);

        stylesReady = true;
    }

    private void OnGUI()
    {
        if (!IsShowing) return;
        InitStyles();

        switch (state)
        {
            case State.BodyDiagram: DrawBodyDiagram(); break;
            case State.PartSelection: DrawPartSelection(); break;
            case State.Confirmation: DrawConfirmation(); break;
            case State.Result: DrawResult(); break;
        }
    }

    // ---- STATE 1: Body Diagram ----
    private void DrawBodyDiagram()
    {
        float w = 500, pad = 25;
        float h = 520;
        Rect panel = CenterPanel(w, h);

        GUI.Box(panel, "", panelBox);
        float y = panel.y + 20;
        float cw = w - pad * 2;

        if (GUI.Button(new Rect(panel.x + w - 40, panel.y + 5, 35, 30), "X", closeBtn)) { Close(); return; }

        GUI.Label(new Rect(panel.x, y, w, 32), "SURGERY TABLE", titleStyle);
        y += 30;
        GUI.Label(new Rect(panel.x, y, w, 22), "Select a body slot to modify", subtitleStyle);
        y += 36;

        HarvestUIStyles.DrawSeparator(new Rect(panel.x + pad, y, cw, 1), HarvestUIStyles.MutedTan);
        y += 18;

        // Body diagram layout
        float slotW = 180, slotH = 70;
        float centerX = panel.x + w / 2f;
        float halfSlot = slotW / 2f;

        // HEAD (centered)
        DrawSlotButton(new Rect(centerX - halfSlot, y, slotW, slotH), BodySlot.Head);
        y += slotH + 8;

        // BODY (centered)
        DrawSlotButton(new Rect(centerX - halfSlot, y, slotW, slotH), BodySlot.Body);
        y += slotH + 8;

        // LEFT ARM + RIGHT ARM (side by side)
        float armW = (cw - 16) / 2f;
        DrawSlotButton(new Rect(panel.x + pad, y, armW, slotH), BodySlot.LeftArm);
        DrawSlotButton(new Rect(panel.x + pad + armW + 16, y, armW, slotH), BodySlot.RightArm);
        y += slotH + 8;

        // LEGS (centered)
        DrawSlotButton(new Rect(centerX - halfSlot, y, slotW, slotH), BodySlot.Legs);
        y += slotH + 8;

        // BACK (centered)
        DrawSlotButton(new Rect(centerX - halfSlot, y, slotW, slotH), BodySlot.Back);
        y += slotH + 15;

        float btnW = 160;
        if (GUI.Button(new Rect(panel.x + (w - btnW) / 2, y, btnW, 40), "CLOSE", backBtn))
            Close();
    }

    private void DrawSlotButton(Rect rect, BodySlot slot)
    {
        var currentGraft = anatomyManager.Grafts.GetGraft(slot);
        var storage = BaseStorage.Instance;
        int storedCount = storage != null ? storage.GetPartsBySlot(slot).Count : 0;

        string slotName = slot.ToString().ToUpper();
        string line1, line2;

        if (currentGraft != null)
        {
            line1 = $"{slotName}: {currentGraft.partName}";
            line2 = $"{currentGraft.species}  |  {storedCount} in storage";
        }
        else
        {
            line1 = $"{slotName}: (empty)";
            line2 = $"{storedCount} in storage";
        }

        var style = currentGraft != null ? slotFilled : slotEmpty;
        style.alignment = TextAnchor.MiddleCenter;

        if (GUI.Button(rect, $"{line1}\n{line2}", style))
        {
            if (storedCount > 0)
            {
                selectedSlot = slot;
                state = State.PartSelection;
            }
        }
    }

    // ---- STATE 2: Part Selection ----
    private void DrawPartSelection()
    {
        var storage = BaseStorage.Instance;
        List<StoredPart> parts = storage != null ? storage.GetPartsBySlot(selectedSlot) : new List<StoredPart>();

        float w = 480, pad = 25;
        float entryH = 65;
        float h = 180 + parts.Count * (entryH + 8) + 60;
        h = Mathf.Clamp(h, 280, Screen.height * 0.85f);
        Rect panel = CenterPanel(w, h);

        GUI.Box(panel, "", panelBox);
        float y = panel.y + 20;
        float cw = w - pad * 2;

        if (GUI.Button(new Rect(panel.x + w - 40, panel.y + 5, 35, 30), "X", closeBtn)) { Close(); return; }

        GUI.Label(new Rect(panel.x, y, w, 32), $"SELECT {selectedSlot.ToString().ToUpper()} PART", titleStyle);
        y += 30;

        var currentGraft = anatomyManager.Grafts.GetGraft(selectedSlot);
        string currentText = currentGraft != null
            ? $"Currently equipped: {currentGraft.partName}"
            : "Slot is empty";
        GUI.Label(new Rect(panel.x, y, w, 22), currentText, subtitleStyle);
        y += 32;

        HarvestUIStyles.DrawSeparator(new Rect(panel.x + pad, y, cw, 1), HarvestUIStyles.MutedTan);
        y += 14;

        for (int i = 0; i < parts.Count; i++)
        {
            var entry = parts[i];
            var part = entry.part;
            if (part == null) continue;

            Rect entryRect = new Rect(panel.x + pad, y, cw, entryH);
            bool hover = entryRect.Contains(Event.current.mousePosition);
            GUI.DrawTexture(entryRect, hover ? entryHoverTex : entryTex);

            var nameStyle = HarvestUIStyles.MakeLabel(16, HarvestUIStyles.BoneWhite, FontStyle.Bold);
            string nameLabel = entry.isDegraded ? $"{part.partName.ToUpper()} (DEGRADED)" : part.partName.ToUpper();
            Color nameColor = entry.isDegraded ? HarvestUIStyles.WarningAmber : HarvestUIStyles.BoneWhite;
            nameStyle.normal.textColor = nameColor;
            GUI.Label(new Rect(entryRect.x + 12, entryRect.y + 8, cw - 24, 22), nameLabel, nameStyle);

            var infoStyle = HarvestUIStyles.MakeLabel(13, HarvestUIStyles.MutedTan);
            GUI.Label(new Rect(entryRect.x + 12, entryRect.y + 30, cw - 24, 20),
                $"{part.species}  |  {part.tier}", infoStyle);

            if (!string.IsNullOrEmpty(part.description))
            {
                var descStyle = HarvestUIStyles.MakeLabel(12, HarvestUIStyles.LightGray);
                GUI.Label(new Rect(entryRect.x + 12, entryRect.y + 46, cw - 24, 18), part.description, descStyle);
            }

            if (GUI.Button(entryRect, "", GUIStyle.none))
            {
                selectedPart = part;
                CalculatePreview(part);
                state = State.Confirmation;
            }

            y += entryH + 8;
        }

        y += 15;
        float btnW = 160;
        if (GUI.Button(new Rect(panel.x + (w - btnW) / 2, y, btnW, 40), "BACK", backBtn))
            state = State.BodyDiagram;
    }

    // ---- STATE 3: Confirmation ----
    private void DrawConfirmation()
    {
        float w = 480, pad = 25;
        float h = 440;
        Rect panel = CenterPanel(w, h);

        GUI.Box(panel, "", panelBox);
        float y = panel.y + 20;
        float cw = w - pad * 2;

        if (GUI.Button(new Rect(panel.x + w - 40, panel.y + 5, 35, 30), "X", closeBtn)) { Close(); return; }

        GUI.Label(new Rect(panel.x, y, w, 32), $"GRAFT: {selectedPart.partName.ToUpper()}", titleStyle);
        y += 30;

        var permStyle = HarvestUIStyles.MakeLabel(16, HarvestUIStyles.BurntOrange, FontStyle.BoldAndItalic, TextAnchor.MiddleCenter);
        GUI.Label(new Rect(panel.x, y, w, 22), "This is permanent.", permStyle);
        y += 32;

        HarvestUIStyles.DrawSeparator(new Rect(panel.x + pad, y, cw, 1), HarvestUIStyles.MutedTan);
        y += 14;

        // Slot info
        string slotText = replacedPart != null
            ? $"Replacing: {replacedPart.partName} ({replacedPart.species})"
            : $"Slot: {selectedPart.slot} (empty)";
        GUI.Label(new Rect(panel.x + pad, y, cw, 22), slotText, bodyText);
        y += 28;

        if (!string.IsNullOrEmpty(selectedPart.description))
        {
            GUI.Label(new Rect(panel.x + pad, y, cw, 22), selectedPart.description, bodyText);
            y += 26;
        }

        HarvestUIStyles.DrawSeparator(new Rect(panel.x + pad, y, cw, 1), HarvestUIStyles.MutedTan);
        y += 12;

        // Stat preview
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

        // Warning
        Rect warningRect = new Rect(panel.x + pad, y, cw, 30);
        GUI.DrawTexture(warningRect, HarvestUIStyles.WarningBGTex);
        string warnText = replacedPart != null
            ? $"! {replacedPart.partName} will be destroyed forever."
            : "! This cannot be undone.";
        var wStyle = HarvestUIStyles.MakeLabel(14, HarvestUIStyles.BurntOrange, FontStyle.Bold);
        GUI.Label(new Rect(warningRect.x + 12, warningRect.y + 5, cw - 24, 20), warnText, wStyle);
        y += 40;

        // Buttons
        float graftW = cw * 0.58f;
        float cancelW = cw * 0.35f;
        float gap = cw - graftW - cancelW;

        if (GUI.Button(new Rect(panel.x + pad, y, graftW, 50), "GRAFT ONTO BODY", graftBtn))
            OnGraftConfirmed();
        if (GUI.Button(new Rect(panel.x + pad + graftW + gap, y, cancelW, 50), "CANCEL", cancelBtn))
            state = State.PartSelection;
    }

    // ---- STATE 4: Result ----
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

        var changedStyle = HarvestUIStyles.MakeLabel(16, HarvestUIStyles.BurntOrange, FontStyle.BoldAndItalic, TextAnchor.MiddleCenter);
        GUI.Label(new Rect(panel.x, y, w, 22), "Your body has changed.", changedStyle);
        y += 34;

        HarvestUIStyles.DrawSeparator(new Rect(panel.x + pad, y, cw, 1), HarvestUIStyles.MutedTan);
        y += 14;

        GUI.Label(new Rect(panel.x + pad, y, cw, 22),
            $"{selectedPart.slot}: {selectedPart.partName} ({selectedPart.species})", sectionHeader);
        y += 28;

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

    // ---- Logic ----

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

        if (anatomyManager == null) return;

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

        replacedPart = currentInSlot;
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

        // Remove from storage
        if (BaseStorage.Instance != null)
        {
            var stored = BaseStorage.Instance.GetPartsBySlot(selectedPart.slot);
            for (int i = 0; i < stored.Count; i++)
            {
                if (stored[i].part == selectedPart)
                {
                    BaseStorage.Instance.RemovePart(stored[i]);
                    break;
                }
            }
        }

        state = State.Result;
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
