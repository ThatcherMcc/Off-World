using UnityEngine;
using OffWorld.Anatomy;

/// <summary>
/// Extraction Chamber UI — base station for loading DNA samples into the player's suit.
/// States:
/// 1. SampleList — shows all DNA samples in BaseStorage, player selects one
/// 2. SlotSelection — shows active (1-4) and organ (1-2) slots, player picks destination
/// 3. Result — confirmation of what was loaded
///
/// OnGUI-based, no Canvas needed. Add to the Player GameObject.
/// </summary>
public class ExtractionChamberUI : MonoBehaviour
{
    public static ExtractionChamberUI Instance { get; private set; }
    public bool IsShowing { get; private set; }

    private enum State { SampleList, SlotSelection, Result }
    private State state;

    // Selection data
    private DNASampleSO selectedSample;
    private int loadedSlotIndex;
    private string loadedSlotType;
    private DNASampleSO displacedDNA;

    // References
    private AnatomyManager anatomyManager;

    // Cursor restore
    private CursorLockMode prevLockState;
    private bool prevCursorVisible;

    // Styles
    private bool stylesReady;
    private GUIStyle panelBox, titleStyle, subtitleStyle, sectionHeader, bodyText, subText;
    private GUIStyle selectBtn, cancelBtn, closeBtn, okBtn, slotBtn, slotActiveBtn;
    private Texture2D entryTex, entryHoverTex;

    private void Awake()
    {
        Instance = this;
        anatomyManager = GetComponent<AnatomyManager>();
        if (anatomyManager == null) anatomyManager = GetComponentInParent<AnatomyManager>();
    }

    private void Update()
    {
        if (IsShowing && Input.GetKeyDown(KeyCode.Escape))
            Close();
    }

    public void Show()
    {
        if (IsShowing) return;

        selectedSample = null;
        displacedDNA = null;
        loadedSlotIndex = -1;
        state = State.SampleList;
        IsShowing = true;
        GraftMenuUI.MenuOpen = true;
        FreezePlayer(true);
    }

    public void Close()
    {
        IsShowing = false;
        GraftMenuUI.MenuOpen = false;
        selectedSample = null;
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
        subtitleStyle = HarvestUIStyles.MakeLabel(16, HarvestUIStyles.SeaGreen, FontStyle.Normal, TextAnchor.MiddleCenter);
        sectionHeader = HarvestUIStyles.MakeLabel(16, HarvestUIStyles.CleanWhite, FontStyle.Bold);
        bodyText = HarvestUIStyles.MakeLabel(15, HarvestUIStyles.LightGray);
        subText = HarvestUIStyles.MakeLabel(14, HarvestUIStyles.SeaGreen, FontStyle.Italic);

        selectBtn = HarvestUIStyles.MakeButton(18, HarvestUIStyles.CleanWhite,
            HarvestUIStyles.MakeTex(HarvestUIStyles.ExtractButtonBG),
            HarvestUIStyles.MakeTex(HarvestUIStyles.ExtractButtonHover));

        cancelBtn = HarvestUIStyles.MakeButton(16, HarvestUIStyles.MidGray,
            HarvestUIStyles.CancelBtnTex, null, 40);

        okBtn = HarvestUIStyles.MakeButton(18, HarvestUIStyles.CleanWhite,
            HarvestUIStyles.MakeTex(new Color(0.08f, 0.24f, 0.22f, 0.90f)), null, 50);

        closeBtn = new GUIStyle(GUI.skin.button);
        closeBtn.fontSize = 16;
        closeBtn.fontStyle = FontStyle.Bold;
        closeBtn.normal.textColor = Color.white;

        slotBtn = HarvestUIStyles.MakeButton(15, HarvestUIStyles.CleanWhite,
            HarvestUIStyles.MakeTex(new Color(0.06f, 0.16f, 0.14f, 0.85f)),
            HarvestUIStyles.MakeTex(new Color(0.08f, 0.24f, 0.20f, 0.90f)), 45);

        slotActiveBtn = HarvestUIStyles.MakeButton(15, HarvestUIStyles.TealPrimary,
            HarvestUIStyles.MakeTex(new Color(0.06f, 0.22f, 0.18f, 0.90f)), null, 45, FontStyle.Bold);

        entryTex = HarvestUIStyles.MakeTex(new Color(0.04f, 0.10f, 0.09f, 0.85f));
        entryHoverTex = HarvestUIStyles.MakeTex(new Color(0.06f, 0.16f, 0.14f, 0.90f));

        stylesReady = true;
    }

    private void OnGUI()
    {
        if (!IsShowing) return;
        InitStyles();

        switch (state)
        {
            case State.SampleList: DrawSampleList(); break;
            case State.SlotSelection: DrawSlotSelection(); break;
            case State.Result: DrawResult(); break;
        }
    }

    // ---- STATE 1: Sample List ----
    private void DrawSampleList()
    {
        var storage = BaseStorage.Instance;
        int sampleCount = storage != null ? storage.SampleCount : 0;

        float w = 480, pad = 25;
        float entryH = 60;
        float h = 160 + sampleCount * (entryH + 8) + 60;
        h = Mathf.Clamp(h, 280, Screen.height * 0.85f);
        Rect panel = CenterPanel(w, h);

        GUI.Box(panel, "", panelBox);
        float y = panel.y + 20;
        float cw = w - pad * 2;

        if (GUI.Button(new Rect(panel.x + w - 40, panel.y + 5, 35, 30), "X", closeBtn)) { Close(); return; }

        GUI.Label(new Rect(panel.x, y, w, 32), "EXTRACTION CHAMBER", titleStyle);
        y += 30;
        GUI.Label(new Rect(panel.x, y, w, 22), "Select a DNA sample to process", subtitleStyle);
        y += 32;

        HarvestUIStyles.DrawSeparator(new Rect(panel.x + pad, y, cw, 1), HarvestUIStyles.SeaGreen);
        y += 14;

        if (sampleCount == 0)
        {
            GUI.Label(new Rect(panel.x + pad, y, cw, 30), "No DNA samples in storage.", subText);
        }
        else
        {
            for (int i = 0; i < storage.StoredSamples.Count; i++)
            {
                var sample = storage.StoredSamples[i];
                if (sample == null) continue;

                Rect entryRect = new Rect(panel.x + pad, y, cw, entryH);
                bool hover = entryRect.Contains(Event.current.mousePosition);
                GUI.DrawTexture(entryRect, hover ? entryHoverTex : entryTex);

                var nameStyle = HarvestUIStyles.MakeLabel(16, HarvestUIStyles.CleanWhite, FontStyle.Bold);
                GUI.Label(new Rect(entryRect.x + 12, entryRect.y + 8, cw - 24, 22), sample.sampleName, nameStyle);

                var infoStyle = HarvestUIStyles.MakeLabel(13, HarvestUIStyles.SeaGreen);
                GUI.Label(new Rect(entryRect.x + 12, entryRect.y + 32, cw - 24, 20),
                    $"{sample.species}  |  {sample.tier}  |  Energy: {sample.energyCost}", infoStyle);

                if (GUI.Button(entryRect, "", GUIStyle.none))
                {
                    selectedSample = sample;
                    state = State.SlotSelection;
                }

                y += entryH + 8;
            }
        }

        y += 15;
        float btnW = 160;
        if (GUI.Button(new Rect(panel.x + (w - btnW) / 2, y, btnW, 40), "CLOSE", cancelBtn))
            Close();
    }

    // ---- STATE 2: Slot Selection ----
    private void DrawSlotSelection()
    {
        float w = 440, pad = 25;
        float h = 400;
        Rect panel = CenterPanel(w, h);

        GUI.Box(panel, "", panelBox);
        float y = panel.y + 20;
        float cw = w - pad * 2;

        if (GUI.Button(new Rect(panel.x + w - 40, panel.y + 5, 35, 30), "X", closeBtn)) { Close(); return; }

        GUI.Label(new Rect(panel.x, y, w, 32), "LOAD DNA", titleStyle);
        y += 30;

        var sampleStyle = HarvestUIStyles.MakeLabel(16, HarvestUIStyles.SeaGreen, FontStyle.Normal, TextAnchor.MiddleCenter);
        GUI.Label(new Rect(panel.x, y, w, 22), selectedSample.sampleName, sampleStyle);
        y += 32;

        HarvestUIStyles.DrawSeparator(new Rect(panel.x + pad, y, cw, 1), HarvestUIStyles.SeaGreen);
        y += 14;

        // Active slots
        GUI.Label(new Rect(panel.x + pad, y, cw, 22), "ACTIVE SLOTS", sectionHeader);
        y += 26;

        var suit = anatomyManager.Suit;
        for (int i = 0; i < suit.UnlockedActiveSlots; i++)
        {
            var existing = suit.GetActiveSlot(i);
            string label = existing != null
                ? $"Active {i + 1}: {existing.sampleName}"
                : $"Active {i + 1}: (empty)";

            var btnStyle = existing != null ? slotBtn : slotActiveBtn;
            if (GUI.Button(new Rect(panel.x + pad, y, cw, 45), label, btnStyle))
            {
                LoadIntoSlot("Active", i);
                return;
            }
            y += 50;
        }

        y += 8;

        // Organ slots
        GUI.Label(new Rect(panel.x + pad, y, cw, 22), "ORGAN SLOTS", sectionHeader);
        y += 26;

        for (int i = 0; i < suit.UnlockedOrganSlots; i++)
        {
            var existing = suit.GetOrganSlot(i);
            string label = existing != null
                ? $"Organ {i + 1}: {existing.sampleName}"
                : $"Organ {i + 1}: (empty)";

            var btnStyle = existing != null ? slotBtn : slotActiveBtn;
            if (GUI.Button(new Rect(panel.x + pad, y, cw, 45), label, btnStyle))
            {
                LoadIntoSlot("Organ", i);
                return;
            }
            y += 50;
        }

        y += 10;
        float btnW = 160;
        if (GUI.Button(new Rect(panel.x + (w - btnW) / 2, y, btnW, 40), "BACK", cancelBtn))
            state = State.SampleList;
    }

    private void LoadIntoSlot(string slotType, int index)
    {
        loadedSlotType = slotType;
        loadedSlotIndex = index + 1;
        displacedDNA = null;

        if (slotType == "Active")
            displacedDNA = anatomyManager.LoadActiveDNA(index, selectedSample);
        else
            displacedDNA = anatomyManager.LoadOrganDNA(index, selectedSample);

        // Remove from storage
        BaseStorage.Instance?.RemoveSample(selectedSample);

        state = State.Result;
    }

    // ---- STATE 3: Result ----
    private void DrawResult()
    {
        float w = 400, pad = 25;
        float h = 280;
        Rect panel = CenterPanel(w, h);

        GUI.Box(panel, "", panelBox);
        float y = panel.y + 20;
        float cw = w - pad * 2;

        GUI.Label(new Rect(panel.x, y, w, 32), "DNA LOADED", titleStyle);
        y += 30;

        var sampleStyle = HarvestUIStyles.MakeLabel(16, HarvestUIStyles.SeaGreen, FontStyle.Normal, TextAnchor.MiddleCenter);
        GUI.Label(new Rect(panel.x, y, w, 22), $"{selectedSample.sampleName} -- {selectedSample.tier}", sampleStyle);
        y += 34;

        HarvestUIStyles.DrawSeparator(new Rect(panel.x + pad, y, cw, 1), HarvestUIStyles.SeaGreen);
        y += 14;

        GUI.Label(new Rect(panel.x + pad, y, cw, 22), $"Loaded into: {loadedSlotType} Slot {loadedSlotIndex}", sectionHeader);
        y += 28;

        if (displacedDNA != null)
        {
            var dispStyle = HarvestUIStyles.MakeLabel(15, HarvestUIStyles.WarningAmber);
            GUI.Label(new Rect(panel.x + pad, y, cw, 22), $"Replaced: {displacedDNA.sampleName}", dispStyle);
            y += 26;
        }

        var buffStyle = HarvestUIStyles.MakeLabel(15, HarvestUIStyles.TealPrimary);
        GUI.Label(new Rect(panel.x + pad, y, cw, 22), "Suit buffs now active", buffStyle);

        float btnW = 200;
        if (GUI.Button(new Rect(panel.x + (w - btnW) / 2, panel.y + h - 65, btnW, 50), "OK", okBtn))
            Close();
    }

    // ---- Helpers ----

    private Rect CenterPanel(float w, float h)
    {
        return new Rect((Screen.width - w) / 2f, (Screen.height - h) / 2f, w, h);
    }
}
