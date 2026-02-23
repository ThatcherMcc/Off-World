using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Floating world-space health bar above a creature.
/// Creates its own Canvas from code — no prefab setup needed.
/// Add this to any creature alongside EnemyHealth.
/// The bar only appears once the creature has taken damage, and hides when full or dead.
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

    private EnemyHealth enemyHealth;
    private Camera mainCam;
    private Canvas canvas;
    private GameObject canvasObj;
    private RectTransform fillRect;
    private Image fillImage;
    private float lastHealthNorm = 1f;
    private bool hasBeenDamaged;

    private void Start()
    {
        enemyHealth = GetComponent<EnemyHealth>();
        mainCam = Camera.main;

        if (enemyHealth == null)
        {
            enabled = false;
            return;
        }

        CreateHealthBar();
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

        // Billboard: face camera
        if (mainCam != null)
        {
            canvasObj.transform.rotation = mainCam.transform.rotation;
        }
    }

    private void OnDisable()
    {
        // Hide when disabled (e.g. creature becomes a corpse)
        if (canvasObj != null)
            canvasObj.SetActive(false);
    }
}
