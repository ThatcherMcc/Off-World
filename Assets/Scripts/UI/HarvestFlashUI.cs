using UnityEngine;

/// <summary>
/// Displays temporary center-screen flash messages for harvest edge cases:
/// - "SPECIMEN KILLED" when killing an incapacitated creature
/// - "SPECIMEN RECOVERED" when the recovery timer expires
/// - "REMAINS DECOMPOSED" when a corpse despawns
///
/// Queue-based: multiple flashes display sequentially.
/// Add to the Player GameObject.
/// </summary>
public class HarvestFlashUI : MonoBehaviour
{
    public static HarvestFlashUI Instance { get; private set; }

    private struct FlashData
    {
        public string line1, line2;
        public Color line1Color, line2Color, bgColor;
        public float duration, elapsed;
    }

    private FlashData? current;
    private readonly System.Collections.Generic.Queue<FlashData> queue = new System.Collections.Generic.Queue<FlashData>();

    // Styles
    private bool stylesReady;
    private GUIStyle line1Style, line2Style;
    private Texture2D bgTex;
    private Color lastBGColor;

    private void Awake()
    {
        Instance = this;
    }

    /// <summary>Show a temporary flash message.</summary>
    public void ShowFlash(string title, string subtitle, Color titleColor, Color subtitleColor, Color bgColor, float duration = 2f)
    {
        var flash = new FlashData
        {
            line1 = title,
            line2 = subtitle,
            line1Color = titleColor,
            line2Color = subtitleColor,
            bgColor = bgColor,
            duration = duration,
            elapsed = 0f
        };

        if (current == null)
            current = flash;
        else
            queue.Enqueue(flash);
    }

    private void Update()
    {
        if (current == null) return;

        var c = current.Value;
        c.elapsed += Time.deltaTime;

        if (c.elapsed >= c.duration)
        {
            current = queue.Count > 0 ? (FlashData?)queue.Dequeue() : null;
        }
        else
        {
            current = c;
        }
    }

    private void InitStyles()
    {
        if (stylesReady) return;
        line1Style = new GUIStyle(GUI.skin.label);
        line1Style.fontSize = 20;
        line1Style.fontStyle = FontStyle.Bold;
        line1Style.alignment = TextAnchor.MiddleCenter;
        line1Style.richText = false;

        line2Style = new GUIStyle(GUI.skin.label);
        line2Style.fontSize = 14;
        line2Style.alignment = TextAnchor.MiddleCenter;
        line2Style.richText = false;

        stylesReady = true;
    }

    private void OnGUI()
    {
        if (current == null) return;
        InitStyles();

        var c = current.Value;

        // Fade out during last 0.5s
        float alpha = 1f;
        float fadeStart = c.duration - 0.5f;
        if (c.elapsed > fadeStart && c.duration > 0.5f)
            alpha = 1f - (c.elapsed - fadeStart) / 0.5f;

        float w = 350, h = 70;
        Rect rect = new Rect((Screen.width - w) / 2f, (Screen.height - h) / 2f - 40f, w, h);

        // Background
        Color bg = c.bgColor;
        bg.a *= alpha;
        var prevColor = GUI.color;
        GUI.color = bg;
        GUI.DrawTexture(rect, Texture2D.whiteTexture);
        GUI.color = prevColor;

        // Line 1
        Color c1 = c.line1Color;
        c1.a *= alpha;
        line1Style.normal.textColor = c1;
        GUI.Label(new Rect(rect.x, rect.y + 8, rect.width, 28), c.line1, line1Style);

        // Line 2
        Color c2 = c.line2Color;
        c2.a *= alpha;
        line2Style.normal.textColor = c2;
        GUI.Label(new Rect(rect.x, rect.y + 38, rect.width, 22), c.line2, line2Style);
    }
}
