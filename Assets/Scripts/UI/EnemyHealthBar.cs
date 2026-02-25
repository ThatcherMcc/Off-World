using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Floating world-space health bar above a creature.
/// Creates its own Canvas from code — no prefab setup needed.
/// Add this to any creature alongside EnemyHealth.
/// The bar only appears once the creature has taken damage, and hides when full or dead.
/// When the creature is incapacitated, shows a recovery timer bar and countdown text.
/// </summary>
public class EnemyHealthBar : MonoBehaviour
{
    [Header("Position")]
    [Tooltip("Offset above the creature's pivot.")]
    [SerializeField] private Vector3 offset = new Vector3(0f, 2f, 0f);

    [Header("Size")]
    [SerializeField] private float barWidth = 1.2f;
    [SerializeField] private float barHeight = 0.15f;

    [Header("Colors")]
    [SerializeField] private Color backgroundColor = new Color(0.15f, 0.15f, 0.15f, 0.85f);
    [SerializeField] private Color fillColor = new Color(0.85f, 0.2f, 0.2f, 1f);
    [SerializeField] private Color fillLowColor = new Color(1f, 0.6f, 0.1f, 1f);

    [Header("Incap Timer Colors")]
    [SerializeField] private Color timerFullColor = new Color(0.2f, 0.8f, 0.7f, 1f);
    [SerializeField] private Color timerWarningColor = new Color(1f, 0.7f, 0.2f, 1f);
    [SerializeField] private Color timerCriticalColor = new Color(0.9f, 0.25f, 0.2f, 1f);

    private EnemyHealth enemyHealth;
    private Camera mainCam;
    private Canvas canvas;
    private GameObject canvasObj;
    private RectTransform fillRect;
    private Image fillImage;
    private float lastHealthNorm = 1f;
    private bool hasBeenDamaged;

    // Incap timer elements
    private GameObject timerGroup;
    private RectTransform timerFillRect;
    private Image timerFillImage;
    private Text timerText;
    private float vulnerableWindowDuration;

    private void Start()
    {
        enemyHealth = GetComponent<EnemyHealth>();
        mainCam = Camera.main;

        if (enemyHealth == null)
        {
            enabled = false;
            return;
        }

        // Cache the vulnerable window duration from CreatureLootTable if available
        var loot = GetComponent<OffWorld.Anatomy.CreatureLootTable>();
        vulnerableWindowDuration = loot != null ? loot.vulnerableWindowDuration : 12f;

        CreateHealthBar();
        CreateIncapTimer();
        canvasObj.SetActive(false); // Hidden until damaged
    }

    private void CreateHealthBar()
    {
        // World-space canvas
        canvasObj = new GameObject("EnemyHealthBarCanvas");
        canvasObj.transform.SetParent(transform);
        canvasObj.transform.localPosition = offset;

        canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        canvas.sortingOrder = 100;

        var rt = canvasObj.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(barWidth * 100f, barHeight * 100f);
        rt.localScale = Vector3.one * 0.01f; // 100 canvas units = 1 world unit

        // Background
        var bgObj = new GameObject("BG");
        bgObj.transform.SetParent(canvasObj.transform, false);
        var bgRect = bgObj.AddComponent<RectTransform>();
        bgRect.anchorMin = Vector2.zero;
        bgRect.anchorMax = Vector2.one;
        bgRect.offsetMin = Vector2.zero;
        bgRect.offsetMax = Vector2.zero;
        var bgImage = bgObj.AddComponent<Image>();
        bgImage.color = backgroundColor;

        // Fill
        var fillObj = new GameObject("Fill");
        fillObj.transform.SetParent(canvasObj.transform, false);
        fillRect = fillObj.AddComponent<RectTransform>();
        fillRect.anchorMin = Vector2.zero;
        fillRect.anchorMax = Vector2.one;
        fillRect.pivot = new Vector2(0f, 0.5f);
        fillRect.offsetMin = Vector2.zero;
        fillRect.offsetMax = Vector2.zero;
        fillImage = fillObj.AddComponent<Image>();
        fillImage.color = fillColor;
    }

    private void CreateIncapTimer()
    {
        // Timer group sits below the health bar
        timerGroup = new GameObject("IncapTimer");
        timerGroup.transform.SetParent(canvasObj.transform, false);

        var groupRect = timerGroup.AddComponent<RectTransform>();
        float canvasW = barWidth * 100f;
        float canvasH = barHeight * 100f;

        // Position below the health bar with a small gap
        float timerBarH = canvasH * 0.6f;
        float gap = canvasH * 0.3f;
        groupRect.anchorMin = new Vector2(0f, 0f);
        groupRect.anchorMax = new Vector2(0f, 0f);
        groupRect.pivot = new Vector2(0f, 1f);
        groupRect.anchoredPosition = new Vector2(0f, -gap);
        groupRect.sizeDelta = new Vector2(canvasW, timerBarH + 100f); // Extra height for text

        // Timer bar background
        var timerBgObj = new GameObject("TimerBG");
        timerBgObj.transform.SetParent(timerGroup.transform, false);
        var timerBgRect = timerBgObj.AddComponent<RectTransform>();
        timerBgRect.anchorMin = new Vector2(0f, 1f);
        timerBgRect.anchorMax = new Vector2(1f, 1f);
        timerBgRect.pivot = new Vector2(0.5f, 1f);
        timerBgRect.anchoredPosition = Vector2.zero;
        timerBgRect.sizeDelta = new Vector2(0f, timerBarH);
        var timerBgImage = timerBgObj.AddComponent<Image>();
        timerBgImage.color = new Color(0.1f, 0.1f, 0.1f, 0.7f);

        // Timer bar fill
        var timerFillObj = new GameObject("TimerFill");
        timerFillObj.transform.SetParent(timerBgObj.transform, false);
        timerFillRect = timerFillObj.AddComponent<RectTransform>();
        timerFillRect.anchorMin = Vector2.zero;
        timerFillRect.anchorMax = Vector2.one;
        timerFillRect.pivot = new Vector2(0f, 0.5f);
        timerFillRect.offsetMin = Vector2.zero;
        timerFillRect.offsetMax = Vector2.zero;
        timerFillImage = timerFillObj.AddComponent<Image>();
        timerFillImage.color = timerFullColor;

        // Timer countdown text below the bar
        var textObj = new GameObject("TimerText");
        textObj.transform.SetParent(timerGroup.transform, false);
        var textRect = textObj.AddComponent<RectTransform>();
        textRect.anchorMin = new Vector2(0f, 1f);
        textRect.anchorMax = new Vector2(1f, 1f);
        textRect.pivot = new Vector2(0.5f, 1f);
        textRect.anchoredPosition = new Vector2(0f, -(timerBarH + 4f));
        textRect.sizeDelta = new Vector2(0f, 80f);

        timerText = textObj.AddComponent<Text>();
        timerText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        timerText.fontSize = 60;
        timerText.fontStyle = FontStyle.Bold;
        timerText.alignment = TextAnchor.UpperCenter;
        timerText.horizontalOverflow = HorizontalWrapMode.Overflow;
        timerText.color = timerFullColor;

        timerGroup.SetActive(false);
    }

    private void LateUpdate()
    {
        if (enemyHealth == null || canvasObj == null) return;

        float healthNorm = enemyHealth.GetHealthNormalized();

        // Show bar only after taking damage
        if (!hasBeenDamaged && healthNorm < 1f)
        {
            hasBeenDamaged = true;
            canvasObj.SetActive(true);
        }

        // Hide when dead
        if (healthNorm <= 0f)
        {
            canvasObj.SetActive(false);
            return;
        }

        // Update fill width
        if (Mathf.Abs(healthNorm - lastHealthNorm) > 0.001f)
        {
            fillRect.anchorMax = new Vector2(healthNorm, 1f);
            // Lerp color toward orange/yellow when low
            fillImage.color = Color.Lerp(fillLowColor, fillColor, healthNorm);
            lastHealthNorm = healthNorm;
        }

        // Incap timer
        UpdateIncapTimer();

        // Billboard: face camera
        if (mainCam != null)
        {
            canvasObj.transform.rotation = mainCam.transform.rotation;
        }
    }

    private void UpdateIncapTimer()
    {
        if (timerGroup == null) return;

        bool showTimer = enemyHealth.IsIncapacitated;
        if (timerGroup.activeSelf != showTimer)
            timerGroup.SetActive(showTimer);

        if (!showTimer) return;

        float timeLeft = enemyHealth.IncapTimeRemaining;
        float fill = Mathf.Clamp01(timeLeft / vulnerableWindowDuration);

        // Update fill bar
        timerFillRect.anchorMax = new Vector2(fill, 1f);

        // Color based on urgency
        Color barColor;
        if (timeLeft > 6f)
            barColor = timerFullColor;
        else if (timeLeft > 3f)
            barColor = timerWarningColor;
        else
            barColor = timerCriticalColor;

        // Pulse in final 3 seconds
        if (timeLeft <= 3f)
        {
            float pulse = Mathf.Sin(Time.time * 4f * Mathf.PI) * 0.3f + 0.7f;
            barColor.a = pulse;
        }

        timerFillImage.color = barColor;
        timerText.color = barColor;
        timerText.text = $"{timeLeft:F0}s";
    }

    private void OnDisable()
    {
        // Hide when disabled (e.g. creature becomes a corpse)
        if (canvasObj != null)
            canvasObj.SetActive(false);
    }
}
