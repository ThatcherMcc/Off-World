using UnityEngine;
using OffWorld.Anatomy;

/// <summary>
/// Extract menu UI — the incapacitate path. States:
/// 1. Confirmation (with recovery timer)
/// 2. Channeling (extraction in progress)
/// 3. Result (DNA acquired)
///
/// Opened when the player interacts with an incapacitated creature.
/// OnGUI-based, no Canvas needed. Add to the Player GameObject.
/// </summary>
public class ExtractMenuUI : MonoBehaviour
{
    public static ExtractMenuUI Instance { get; private set; }
    public bool IsShowing { get; private set; }

    private enum State { Confirmation, Channeling, Result }
    private State state;

    // Creature data
    private EnemyHealth targetHealth;
    private CreatureLootTable targetLoot;
    private string creatureName;
    private DNASampleSO dnaSample;
    private DNATier dnaTier;

    // Channeling
    private float channelProgress;
    private float channelTime = 3f;
    private bool channelInterrupted;
    private float interruptFlashTimer;

    // Result data
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
    private GUIStyle extractBtn, leaveBtn, closeBtn, okBtn;
    private GUIStyle timerText, channelText;

    private void Awake()
    {
        Instance = this;
        anatomyManager = GetComponent<AnatomyManager>();
        if (anatomyManager == null) anatomyManager = GetComponentInParent<AnatomyManager>();
    }

    private void Update()
    {
        if (!IsShowing) return;

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Close();
            return;
        }

        // Check if creature recovered
        if (state != State.Result)
        {
            if (targetHealth == null || !targetHealth.IsIncapacitated)
            {
                Close();
                if (HarvestFlashUI.Instance != null)
                    HarvestFlashUI.Instance.ShowFlash("SPECIMEN RECOVERED", "The creature escaped",
                        HarvestUIStyles.WarningAmber, HarvestUIStyles.MutedTan, HarvestUIStyles.FlashAmberBG, 2.5f);
                return;
            }
        }

        // Channeling
        if (state == State.Channeling)
        {
            channelProgress += Time.deltaTime / channelTime;

            if (targetHealth != null)
            {
                float dist = Vector3.Distance(transform.position, targetHealth.transform.position);
                if (dist > 6f)
                {
                    InterruptChannel();
                    return;
                }
            }

            if (channelProgress >= 1f)
                CompleteExtraction();
        }

        if (interruptFlashTimer > 0f)
            interruptFlashTimer -= Time.deltaTime;
    }

    public void Show(EnemyHealth enemy, CreatureLootTable loot)
    {
        if (IsShowing) return;
        if (enemy == null || loot == null || loot.dnaSample == null) return;

        targetHealth = enemy;
        targetLoot = loot;
        creatureName = enemy.gameObject.name.Replace("(Clone)", "").Trim();
        dnaSample = loot.dnaSample;
        dnaTier = DNATier.Prime;
        channelProgress = 0f;
        channelInterrupted = false;
        interruptFlashTimer = 0f;
        displacedDNA = null;
        state = State.Confirmation;
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

    private void InterruptChannel()
    {
        channelProgress = 0f;
        channelInterrupted = true;
        interruptFlashTimer = 1f;
        state = State.Confirmation;
    }

    private void CompleteExtraction()
    {
        if (targetHealth == null) return;

        targetHealth.MarkExtracted();

        var energy = GetComponent<SuitEnergy>();
        if (energy == null) energy = GetComponentInParent<SuitEnergy>();
        energy?.OnIncapacitate();

        // Add to base storage instead of equipping directly
        if (BaseStorage.Instance != null)
            BaseStorage.Instance.AddSample(dnaSample);

        // Dispatch drone for visual
        if (anatomyManager != null && anatomyManager.DroneConfig != null && targetHealth != null)
            DronePickup.Dispatch(targetHealth.gameObject, anatomyManager.DroneConfig);

        state = State.Result;
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

        extractBtn = HarvestUIStyles.MakeButton(18, HarvestUIStyles.CleanWhite,
            HarvestUIStyles.MakeTex(HarvestUIStyles.ExtractButtonBG),
            HarvestUIStyles.MakeTex(HarvestUIStyles.ExtractButtonHover));

        leaveBtn = HarvestUIStyles.MakeButton(16, HarvestUIStyles.MidGray,
            HarvestUIStyles.CancelBtnTex, null, 50);

        okBtn = HarvestUIStyles.MakeButton(18, HarvestUIStyles.CleanWhite,
            HarvestUIStyles.MakeTex(new Color(0.08f, 0.24f, 0.22f, 0.90f)), null, 50);

        closeBtn = new GUIStyle(GUI.skin.button);
        closeBtn.fontSize = 16;
        closeBtn.fontStyle = FontStyle.Bold;
        closeBtn.normal.textColor = Color.white;

        timerText = HarvestUIStyles.MakeLabel(14, HarvestUIStyles.TealPrimary, FontStyle.Bold, TextAnchor.MiddleRight);
        channelText = HarvestUIStyles.MakeLabel(18, HarvestUIStyles.TealPrimary, FontStyle.Bold, TextAnchor.MiddleCenter);

        stylesReady = true;
    }

    private void OnGUI()
    {
        if (!IsShowing) return;
        InitStyles();

        switch (state)
        {
            case State.Confirmation: DrawConfirmation(); break;
            case State.Channeling: DrawChanneling(); break;
            case State.Result: DrawResult(); break;
        }
    }

    private float GetTimeRemaining()
    {
        if (targetHealth == null) return 0f;
        return targetHealth.IncapTimeRemaining;
    }

    // ---- STATE 1: Confirmation ----
    private void DrawConfirmation()
    {
        float w = 440, pad = 25;
        float h = 360;
        Rect panel = CenterPanel(w, h);

        GUI.Box(panel, "", panelBox);
        float y = panel.y + 20;
        float cw = w - pad * 2;

        if (GUI.Button(new Rect(panel.x + w - 40, panel.y + 5, 35, 30), "X", closeBtn)) { Close(); return; }

        // Header
        GUI.Label(new Rect(panel.x, y, w, 32), "DNA EXTRACTION", titleStyle);
        y += 30;
        GUI.Label(new Rect(panel.x, y, w, 22), $"{creatureName} -- specimen alive", subtitleStyle);
        y += 32;

        // Recovery Timer Bar
        float timeRemaining = GetTimeRemaining();
        float timeFraction = targetLoot != null ? timeRemaining / targetLoot.vulnerableWindowDuration : 0f;

        Color timerColor;
        string timerLabel;
        if (timeRemaining > 6f)
        {
            timerColor = HarvestUIStyles.TealPrimary;
            timerLabel = $"RECOVERING IN {timeRemaining:F0}s";
        }
        else if (timeRemaining > 4f)
        {
            timerColor = HarvestUIStyles.WarningAmber;
            timerLabel = $"ACT NOW -- {timeRemaining:F0}s";
        }
        else
        {
            timerColor = HarvestUIStyles.DangerRed;
            timerLabel = $"RECOVERING SOON -- {timeRemaining:F0}s";
            float pulse = Mathf.Sin(Time.time * 3f * Mathf.PI * 2f) * 0.1f + 0.9f;
            timerColor.a = pulse;
        }

        HarvestUIStyles.DrawBar(new Rect(panel.x + pad, y, cw, 10), timeFraction, timerColor, HarvestUIStyles.TimerEmpty);
        y += 14;
        timerText.normal.textColor = timerColor;
        GUI.Label(new Rect(panel.x + pad, y, cw, 20), timerLabel, timerText);
        y += 28;

        HarvestUIStyles.DrawSeparator(new Rect(panel.x + pad, y, cw, 1), HarvestUIStyles.SeaGreen);
        y += 14;

        // Interrupt flash
        if (channelInterrupted && interruptFlashTimer > 0f)
        {
            var flashStyle = HarvestUIStyles.MakeLabel(16, HarvestUIStyles.DangerRed, FontStyle.Bold, TextAnchor.MiddleCenter);
            GUI.Label(new Rect(panel.x, y, w, 22), "INTERRUPTED -- Try again", flashStyle);
            y += 26;
        }

        // Sample + Tier (condensed)
        bool isPrime = dnaTier == DNATier.Prime;
        Color tierColor = isPrime ? HarvestUIStyles.PositiveGreen : HarvestUIStyles.WarningAmber;
        GUI.Label(new Rect(panel.x + pad, y, cw, 22), $"SAMPLE: {dnaSample.sampleName}", sectionHeader);
        y += 26;
        var tierStyle = HarvestUIStyles.MakeLabel(15, tierColor, FontStyle.Bold);
        string tierDesc = isPrime ? "PRIME -- full potency" : "DEGRADED -- 70% potency";
        GUI.Label(new Rect(panel.x + pad, y, cw, 22), tierDesc, tierStyle);
        y += 30;

        // Target slot (one line)
        string targetLine = FindTargetSlotDescription();
        var targetStyle = HarvestUIStyles.MakeLabel(15, HarvestUIStyles.TealPrimary, FontStyle.Bold);
        GUI.Label(new Rect(panel.x + pad, y, cw, 22), targetLine, targetStyle);
        y += 30;

        // Buttons
        float extractW = cw * 0.58f;
        float leaveW = cw * 0.35f;
        float gap = cw - extractW - leaveW;

        if (GUI.Button(new Rect(panel.x + pad, y, extractW, 50), "EXTRACT DNA", extractBtn))
        {
            state = State.Channeling;
            channelProgress = 0f;
        }

        if (GUI.Button(new Rect(panel.x + pad + extractW + gap, y, leaveW, 50), "LEAVE IT", leaveBtn))
            Close();
    }

    // ---- STATE 2: Channeling ----
    private void DrawChanneling()
    {
        float w = 440, pad = 25;
        float h = 200;
        Rect panel = CenterPanel(w, h);

        GUI.Box(panel, "", panelBox);
        float y = panel.y + 20;
        float cw = w - pad * 2;

        GUI.Label(new Rect(panel.x, y, w, 32), "EXTRACTING...", channelText);
        y += 36;

        // Recovery timer bar (still ticking)
        float timeRemaining = GetTimeRemaining();
        float timeFraction = targetLoot != null ? timeRemaining / targetLoot.vulnerableWindowDuration : 0f;
        Color timerColor = timeRemaining > 6f ? HarvestUIStyles.TealPrimary
            : timeRemaining > 4f ? HarvestUIStyles.WarningAmber : HarvestUIStyles.DangerRed;
        HarvestUIStyles.DrawBar(new Rect(panel.x + pad, y, cw, 8), timeFraction, timerColor, HarvestUIStyles.TimerEmpty);
        y += 20;

        // Channel bar
        float pulse = Mathf.Sin(Time.time * Mathf.PI * 2f) * 0.15f + 0.85f;
        Color barColor = HarvestUIStyles.TealPrimary;
        barColor.a = pulse;
        HarvestUIStyles.DrawBar(new Rect(panel.x + pad, y, cw, 16), channelProgress,
            barColor, HarvestUIStyles.TimerEmpty);
        y += 28;

        var instrStyle = HarvestUIStyles.MakeLabel(15, HarvestUIStyles.SeaGreen, FontStyle.Italic, TextAnchor.MiddleCenter);
        GUI.Label(new Rect(panel.x, y, w, 22), "Hold position. Do not take damage.", instrStyle);
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

        GUI.Label(new Rect(panel.x, y, w, 32), "DNA ACQUIRED", titleStyle);
        y += 30;

        var tierColor = dnaTier == DNATier.Prime ? HarvestUIStyles.PositiveGreen : HarvestUIStyles.WarningAmber;
        var sampleStyle = HarvestUIStyles.MakeLabel(16, HarvestUIStyles.SeaGreen, FontStyle.Normal, TextAnchor.MiddleCenter);
        GUI.Label(new Rect(panel.x, y, w, 22), $"{dnaSample.sampleName} -- {dnaTier}", sampleStyle);
        y += 34;

        HarvestUIStyles.DrawSeparator(new Rect(panel.x + pad, y, cw, 1), HarvestUIStyles.SeaGreen);
        y += 14;

        GUI.Label(new Rect(panel.x + pad, y, cw, 22), "Sent to base via drone", sectionHeader);
        y += 28;

        var buffStyle = HarvestUIStyles.MakeLabel(15, HarvestUIStyles.TealPrimary);
        GUI.Label(new Rect(panel.x + pad, y, cw, 22), "Use Extraction Chamber at base to load into suit", buffStyle);
        y += 26;

        float btnW = 200;
        if (GUI.Button(new Rect(panel.x + (w - btnW) / 2, panel.y + h - 65, btnW, 50), "OK", okBtn))
            Close();
    }

    // ---- Helpers ----

    private Rect CenterPanel(float w, float h)
    {
        return new Rect((Screen.width - w) / 2f, (Screen.height - h) / 2f, w, h);
    }

    private string FindTargetSlotDescription()
    {
        if (anatomyManager == null) return "No suit available";

        for (int i = 0; i < anatomyManager.Suit.UnlockedActiveSlots; i++)
        {
            if (anatomyManager.Suit.GetActiveSlot(i) == null)
                return $"--> Active Slot {i + 1}";
        }
        for (int i = 0; i < anatomyManager.Suit.UnlockedOrganSlots; i++)
        {
            if (anatomyManager.Suit.GetOrganSlot(i) == null)
                return $"--> Organ Slot {i + 1}";
        }
        var existing = anatomyManager.Suit.GetActiveSlot(0);
        string name = existing != null ? existing.sampleName : "DNA";
        return $"--> Replaces {name} in Slot 1";
    }
}
