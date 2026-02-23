using UnityEngine;

/// <summary>
/// Shared color palette, texture helpers, and GUIStyle builders for the harvest UI system.
/// Graft path = organic/visceral (amber, bone, burnt orange).
/// Extract path = clinical/technical (teal, cyan, sea green).
/// </summary>
public static class HarvestUIStyles
{
    // ---- Graft palette (organic / visceral) ----
    public static readonly Color AmberPrimary   = new Color32(212, 168, 67, 255);
    public static readonly Color BurntOrange    = new Color32(204, 102, 68, 255);
    public static readonly Color BoneWhite      = new Color32(232, 216, 184, 255);
    public static readonly Color MutedTan       = new Color32(168, 144, 112, 255);
    public static readonly Color DarkSienna     = new Color32(139, 69, 19, 255);
    public static readonly Color GraftPanelBG   = new Color(0.06f, 0.047f, 0.039f, 0.92f);
    public static readonly Color GraftButtonBG  = new Color(0.39f, 0.20f, 0.08f, 0.90f);
    public static readonly Color GraftButtonHover = new Color(0.55f, 0.27f, 0.12f, 0.95f);
    public static readonly Color GraftEntryBG   = new Color(0.12f, 0.086f, 0.07f, 0.90f);
    public static readonly Color GraftEntryHover = new Color(0.20f, 0.14f, 0.10f, 0.95f);
    public static readonly Color WarningBG      = new Color(0.24f, 0.08f, 0.06f, 0.50f);

    // ---- Extract palette (clinical / technical) ----
    public static readonly Color TealPrimary    = new Color32(68, 204, 170, 255);
    public static readonly Color SeaGreen       = new Color32(120, 184, 160, 255);
    public static readonly Color CleanWhite     = new Color32(232, 232, 232, 255);
    public static readonly Color ExtractPanelBG = new Color(0.031f, 0.071f, 0.063f, 0.92f);
    public static readonly Color ExtractButtonBG = new Color(0.08f, 0.31f, 0.27f, 0.90f);
    public static readonly Color ExtractButtonHover = new Color(0.12f, 0.43f, 0.37f, 0.95f);
    public static readonly Color TimerEmpty     = new Color32(26, 51, 51, 255);

    // ---- Shared ----
    public static readonly Color DangerRed      = new Color32(204, 68, 68, 255);
    public static readonly Color PositiveGreen  = new Color32(68, 204, 102, 255);
    public static readonly Color WarningAmber   = new Color32(204, 170, 68, 255);
    public static readonly Color DimGray        = new Color32(102, 102, 102, 255);
    public static readonly Color MidGray        = new Color32(136, 136, 136, 255);
    public static readonly Color LightGray      = new Color32(204, 204, 204, 255);
    public static readonly Color CancelBtnBG    = new Color(0.16f, 0.16f, 0.16f, 0.80f);

    // ---- Prompt colors ----
    public static readonly Color CorpsePromptBG = new Color(0.06f, 0.047f, 0.039f, 0.70f);
    public static readonly Color IncapPromptBG  = new Color(0.031f, 0.071f, 0.063f, 0.70f);

    // ---- Flash colors ----
    public static readonly Color FlashRedBG     = new Color(0.16f, 0.04f, 0.04f, 0.60f);
    public static readonly Color FlashAmberBG   = new Color(0.16f, 0.12f, 0.04f, 0.60f);
    public static readonly Color FlashSiennaBG  = new Color(0.12f, 0.08f, 0.04f, 0.60f);

    /// <summary>Create a solid-color Texture2D for GUI backgrounds.</summary>
    public static Texture2D MakeTex(Color col)
    {
        var tex = new Texture2D(2, 2);
        var pix = new Color[] { col, col, col, col };
        tex.SetPixels(pix);
        tex.Apply();
        return tex;
    }

    // ---- Cached textures (lazy-init) ----
    private static Texture2D _graftPanelTex;
    public static Texture2D GraftPanelTex => _graftPanelTex ?? (_graftPanelTex = MakeTex(GraftPanelBG));

    private static Texture2D _extractPanelTex;
    public static Texture2D ExtractPanelTex => _extractPanelTex ?? (_extractPanelTex = MakeTex(ExtractPanelBG));

    private static Texture2D _graftEntryTex;
    public static Texture2D GraftEntryTex => _graftEntryTex ?? (_graftEntryTex = MakeTex(GraftEntryBG));

    private static Texture2D _graftEntryHoverTex;
    public static Texture2D GraftEntryHoverTex => _graftEntryHoverTex ?? (_graftEntryHoverTex = MakeTex(GraftEntryHover));

    private static Texture2D _warningBGTex;
    public static Texture2D WarningBGTex => _warningBGTex ?? (_warningBGTex = MakeTex(WarningBG));

    private static Texture2D _cancelBtnTex;
    public static Texture2D CancelBtnTex => _cancelBtnTex ?? (_cancelBtnTex = MakeTex(CancelBtnBG));

    // ---- Style Builders ----

    public static GUIStyle MakeLabel(int fontSize, Color color, FontStyle fontStyle = FontStyle.Normal,
        TextAnchor alignment = TextAnchor.UpperLeft, bool richText = true, bool wordWrap = true)
    {
        var style = new GUIStyle(GUI.skin.label);
        style.fontSize = fontSize;
        style.fontStyle = fontStyle;
        style.alignment = alignment;
        style.normal.textColor = color;
        style.richText = richText;
        style.wordWrap = wordWrap;
        return style;
    }

    public static GUIStyle MakeButton(int fontSize, Color textColor, Texture2D bg, Texture2D hoverBg = null,
        int height = 50, FontStyle fontStyle = FontStyle.Bold)
    {
        var style = new GUIStyle(GUI.skin.button);
        style.fontSize = fontSize;
        style.fontStyle = fontStyle;
        style.fixedHeight = height;
        style.normal.textColor = textColor;
        style.hover.textColor = textColor;
        style.active.textColor = textColor;
        if (bg != null)
        {
            style.normal.background = bg;
            style.hover.background = hoverBg ?? bg;
            style.active.background = hoverBg ?? bg;
        }
        return style;
    }

    public static GUIStyle MakeBox(Texture2D bg)
    {
        var style = new GUIStyle(GUI.skin.box);
        style.normal.background = bg;
        return style;
    }

    /// <summary>Draw a horizontal separator line.</summary>
    public static void DrawSeparator(Rect area, Color color)
    {
        var prev = GUI.color;
        GUI.color = color;
        GUI.DrawTexture(area, Texture2D.whiteTexture);
        GUI.color = prev;
    }

    /// <summary>Draw a filled bar (like a timer or health bar).</summary>
    public static void DrawBar(Rect area, float fillNormalized, Color fillColor, Color emptyColor)
    {
        // Empty background
        var prev = GUI.color;
        GUI.color = emptyColor;
        GUI.DrawTexture(area, Texture2D.whiteTexture);

        // Filled portion
        if (fillNormalized > 0f)
        {
            var fillRect = new Rect(area.x, area.y, area.width * Mathf.Clamp01(fillNormalized), area.height);
            GUI.color = fillColor;
            GUI.DrawTexture(fillRect, Texture2D.whiteTexture);
        }

        GUI.color = prev;
    }
}
