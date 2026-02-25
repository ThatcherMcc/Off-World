using UnityEngine;
using OffWorld.Anatomy;

/// <summary>
/// Storage Terminal UI — browse all stored graft parts and DNA samples at the base.
/// Two tabs: Parts and Samples. Read-only viewing with item details.
///
/// OnGUI-based, no Canvas needed. Add to the Player GameObject.
/// </summary>
public class StorageTerminalUI : MonoBehaviour
{
    public static StorageTerminalUI Instance { get; private set; }
    public bool IsShowing { get; private set; }

    private enum Tab { Parts, Samples }
    private Tab currentTab = Tab.Parts;

    // Scroll positions
    private Vector2 partsScroll;
    private Vector2 samplesScroll;

    // Cursor restore
    private CursorLockMode prevLockState;
    private bool prevCursorVisible;

    // Styles
    private bool stylesReady;
    private GUIStyle panelBox, titleStyle, subtitleStyle, sectionHeader, bodyText, subText;
    private GUIStyle tabBtn, tabActiveBtn, closeBtn, cancelBtn;
    private Texture2D entryTex, entryHoverTex;

    private void Awake()
    {
        Instance = this;
    }

    private void Update()
    {
        if (IsShowing && Input.GetKeyDown(KeyCode.Escape))
            Close();
    }

    public void Show()
    {
        if (IsShowing) return;

        currentTab = Tab.Parts;
        partsScroll = Vector2.zero;
        samplesScroll = Vector2.zero;
        IsShowing = true;
        GraftMenuUI.MenuOpen = true;
        FreezePlayer(true);
    }

    public void Close()
    {
        IsShowing = false;
        GraftMenuUI.MenuOpen = false;
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

        panelBox = HarvestUIStyles.MakeBox(HarvestUIStyles.ExtractPanelTex);
        titleStyle = HarvestUIStyles.MakeLabel(24, HarvestUIStyles.TealPrimary, FontStyle.Bold, TextAnchor.MiddleCenter);
        subtitleStyle = HarvestUIStyles.MakeLabel(15, HarvestUIStyles.SeaGreen, FontStyle.Normal, TextAnchor.MiddleCenter);
        sectionHeader = HarvestUIStyles.MakeLabel(16, HarvestUIStyles.CleanWhite, FontStyle.Bold);
        bodyText = HarvestUIStyles.MakeLabel(15, HarvestUIStyles.LightGray);
        subText = HarvestUIStyles.MakeLabel(14, HarvestUIStyles.SeaGreen, FontStyle.Italic);

        tabBtn = HarvestUIStyles.MakeButton(16, HarvestUIStyles.MidGray,
            HarvestUIStyles.MakeTex(new Color(0.04f, 0.10f, 0.09f, 0.85f)),
            HarvestUIStyles.MakeTex(new Color(0.06f, 0.16f, 0.14f, 0.90f)), 36);

        tabActiveBtn = HarvestUIStyles.MakeButton(16, HarvestUIStyles.TealPrimary,
            HarvestUIStyles.MakeTex(new Color(0.06f, 0.20f, 0.17f, 0.95f)), null, 36, FontStyle.Bold);

        closeBtn = new GUIStyle(GUI.skin.button);
        closeBtn.fontSize = 16;
        closeBtn.fontStyle = FontStyle.Bold;
        closeBtn.normal.textColor = Color.white;

        cancelBtn = HarvestUIStyles.MakeButton(16, HarvestUIStyles.MidGray,
            HarvestUIStyles.CancelBtnTex, null, 40);

        entryTex = HarvestUIStyles.MakeTex(new Color(0.04f, 0.10f, 0.09f, 0.85f));
        entryHoverTex = HarvestUIStyles.MakeTex(new Color(0.06f, 0.16f, 0.14f, 0.90f));

        stylesReady = true;
    }

    private void OnGUI()
    {
        if (!IsShowing) return;
        InitStyles();

        var storage = BaseStorage.Instance;
        int partCount = storage != null ? storage.PartCount : 0;
        int sampleCount = storage != null ? storage.SampleCount : 0;

        float w = 520, pad = 25;
        float h = Mathf.Min(Screen.height * 0.85f, 600);
        Rect panel = CenterPanel(w, h);

        GUI.Box(panel, "", panelBox);
        float y = panel.y + 18;
        float cw = w - pad * 2;

        // Close button
        if (GUI.Button(new Rect(panel.x + w - 40, panel.y + 5, 35, 30), "X", closeBtn)) { Close(); return; }

        // Title
        GUI.Label(new Rect(panel.x, y, w, 32), "STORAGE", titleStyle);
        y += 28;
        GUI.Label(new Rect(panel.x, y, w, 20), $"{partCount} parts  |  {sampleCount} samples", subtitleStyle);
        y += 28;

        // Tabs
        float tabW = cw / 2f - 4;
        bool onParts = currentTab == Tab.Parts;
        if (GUI.Button(new Rect(panel.x + pad, y, tabW, 36), $"PARTS ({partCount})", onParts ? tabActiveBtn : tabBtn))
            currentTab = Tab.Parts;
        if (GUI.Button(new Rect(panel.x + pad + tabW + 8, y, tabW, 36), $"SAMPLES ({sampleCount})", !onParts ? tabActiveBtn : tabBtn))
            currentTab = Tab.Samples;
        y += 44;

        HarvestUIStyles.DrawSeparator(new Rect(panel.x + pad, y, cw, 1), HarvestUIStyles.SeaGreen);
        y += 8;

        // Content area (scrollable)
        float contentTop = y;
        float contentH = panel.y + h - contentTop - 60; // Leave room for close button
        Rect contentArea = new Rect(panel.x + pad, contentTop, cw, contentH);

        if (currentTab == Tab.Parts)
            DrawPartsTab(contentArea, storage);
        else
            DrawSamplesTab(contentArea, storage);

        // Close button at bottom
        float btnW = 160;
        if (GUI.Button(new Rect(panel.x + (w - btnW) / 2, panel.y + h - 50, btnW, 40), "CLOSE", cancelBtn))
            Close();
    }

    // ---- Parts Tab ----
    private void DrawPartsTab(Rect area, BaseStorage storage)
    {
        if (storage == null || storage.PartCount == 0)
        {
            GUI.Label(new Rect(area.x, area.y + 10, area.width, 24), "No graft parts in storage.", subText);
            return;
        }

        float entryH = 70;
        float totalH = storage.PartCount * (entryH + 8);
        Rect viewRect = new Rect(0, 0, area.width - 20, totalH);

        partsScroll = GUI.BeginScrollView(area, partsScroll, viewRect);
        float y = 0;

        for (int i = 0; i < storage.StoredParts.Count; i++)
        {
            var entry = storage.StoredParts[i];
            var part = entry.part;
            if (part == null) continue;

            Rect entryRect = new Rect(0, y, viewRect.width, entryH);
            bool hover = entryRect.Contains(Event.current.mousePosition - partsScroll + new Vector2(area.x, area.y));
            GUI.DrawTexture(entryRect, hover ? entryHoverTex : entryTex);

            // Part name (show DEGRADED label if applicable)
            var nameStyle = HarvestUIStyles.MakeLabel(16, HarvestUIStyles.CleanWhite, FontStyle.Bold);
            string nameLabel = entry.isDegraded ? $"{part.partName} (DEGRADED)" : part.partName;
            if (entry.isDegraded) nameStyle.normal.textColor = HarvestUIStyles.WarningAmber;
            GUI.Label(new Rect(entryRect.x + 12, entryRect.y + 6, entryRect.width - 24, 22), nameLabel, nameStyle);

            // Slot + Species + Tier
            var infoStyle = HarvestUIStyles.MakeLabel(13, HarvestUIStyles.SeaGreen);
            GUI.Label(new Rect(entryRect.x + 12, entryRect.y + 28, entryRect.width - 24, 18),
                $"{part.slot}  |  {part.species}  |  {part.tier}", infoStyle);

            // Stat preview
            string statLine = BuildPartStatLine(part);
            if (!string.IsNullOrEmpty(statLine))
            {
                var statStyle = HarvestUIStyles.MakeLabel(12, HarvestUIStyles.LightGray, FontStyle.Italic);
                GUI.Label(new Rect(entryRect.x + 12, entryRect.y + 46, entryRect.width - 24, 18), statLine, statStyle);
            }

            y += entryH + 8;
        }

        GUI.EndScrollView();
    }

    // ---- Samples Tab ----
    private void DrawSamplesTab(Rect area, BaseStorage storage)
    {
        if (storage == null || storage.SampleCount == 0)
        {
            GUI.Label(new Rect(area.x, area.y + 10, area.width, 24), "No DNA samples in storage.", subText);
            return;
        }

        float entryH = 70;
        float totalH = storage.SampleCount * (entryH + 8);
        Rect viewRect = new Rect(0, 0, area.width - 20, totalH);

        samplesScroll = GUI.BeginScrollView(area, samplesScroll, viewRect);
        float y = 0;

        for (int i = 0; i < storage.StoredSamples.Count; i++)
        {
            var sample = storage.StoredSamples[i];
            if (sample == null) continue;

            Rect entryRect = new Rect(0, y, viewRect.width, entryH);
            bool hover = entryRect.Contains(Event.current.mousePosition - samplesScroll + new Vector2(area.x, area.y));
            GUI.DrawTexture(entryRect, hover ? entryHoverTex : entryTex);

            // Sample name
            var nameStyle = HarvestUIStyles.MakeLabel(16, HarvestUIStyles.CleanWhite, FontStyle.Bold);
            GUI.Label(new Rect(entryRect.x + 12, entryRect.y + 6, entryRect.width - 24, 22), sample.sampleName, nameStyle);

            // Species + Tier + Energy cost
            var infoStyle = HarvestUIStyles.MakeLabel(13, HarvestUIStyles.SeaGreen);
            GUI.Label(new Rect(entryRect.x + 12, entryRect.y + 28, entryRect.width - 24, 18),
                $"{sample.species}  |  {sample.tier}  |  Energy: {sample.energyCost}", infoStyle);

            // Ability / passive info
            string detailLine = BuildSampleDetailLine(sample);
            if (!string.IsNullOrEmpty(detailLine))
            {
                var detailStyle = HarvestUIStyles.MakeLabel(12, HarvestUIStyles.LightGray, FontStyle.Italic);
                GUI.Label(new Rect(entryRect.x + 12, entryRect.y + 46, entryRect.width - 24, 18), detailLine, detailStyle);
            }

            y += entryH + 8;
        }

        GUI.EndScrollView();
    }

    // ---- Helpers ----

    private string BuildPartStatLine(GraftPartSO part)
    {
        var m = part.modifiers;
        var bits = new System.Collections.Generic.List<string>();

        if (m.maxHPFlat != 0) bits.Add($"HP {m.maxHPFlat:+#;-#;0}");
        if (!Mathf.Approximately(m.maxHPMult, 1f)) bits.Add($"HP x{m.maxHPMult:F2}");
        if (m.attackDamageFlat != 0) bits.Add($"ATK {m.attackDamageFlat:+#;-#;0}");
        if (!Mathf.Approximately(m.attackDamageMult, 1f)) bits.Add($"ATK x{m.attackDamageMult:F2}");
        if (m.walkSpeedFlat != 0) bits.Add($"SPD {m.walkSpeedFlat:+0.#;-0.#;0}");
        if (m.damageResistance != 0) bits.Add($"RES {m.damageResistance:+0.#;-0.#;0}");
        if (m.jumpForceFlat != 0) bits.Add($"JMP {m.jumpForceFlat:+0.#;-0.#;0}");

        if (part.activeAbility != null)
            bits.Add($"Ability: {part.activeAbility.abilityName}");

        return bits.Count > 0 ? string.Join("  |  ", bits) : null;
    }

    private string BuildSampleDetailLine(DNASampleSO sample)
    {
        var bits = new System.Collections.Generic.List<string>();

        if (sample.cooldown > 0) bits.Add($"CD: {sample.cooldown:F0}s");
        if (sample.duration > 0) bits.Add($"Dur: {sample.duration:F0}s");

        var m = sample.passiveModifiers;
        if (m.maxHPFlat != 0) bits.Add($"HP {m.maxHPFlat:+#;-#;0}");
        if (m.attackDamageFlat != 0) bits.Add($"ATK {m.attackDamageFlat:+#;-#;0}");
        if (m.walkSpeedFlat != 0) bits.Add($"SPD {m.walkSpeedFlat:+0.#;-0.#;0}");
        if (m.damageResistance != 0) bits.Add($"RES {m.damageResistance:+0.#;-0.#;0}");

        return bits.Count > 0 ? string.Join("  |  ", bits) : null;
    }

    private Rect CenterPanel(float w, float h)
    {
        return new Rect((Screen.width - w) / 2f, (Screen.height - h) / 2f, w, h);
    }
}
