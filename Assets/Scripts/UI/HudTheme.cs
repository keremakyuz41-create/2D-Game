using System.Collections.Generic;
using UnityEngine;

namespace Cernobil.UI
{
    public static class HudTheme
    {
        public static readonly Color Background = new Color(0.035f, 0.052f, 0.061f, 0.94f);
        public static readonly Color Text = new Color(0.94f, 0.96f, 0.94f);
        public static readonly Color Muted = new Color(0.73f, 0.80f, 0.80f);
        public static readonly Color Mint = new Color(0.30f, 0.89f, 0.72f);
        public static readonly Color Amber = new Color(1f, 0.73f, 0.30f);
        public static readonly Color Danger = new Color(1f, 0.32f, 0.32f);
        private static readonly Dictionary<int, GUIStyle> styles = new Dictionary<int, GUIStyle>();
        private static Texture2D circle;
        private static Font font;

        // Layout may scale, but glyphs are rasterized once at their actual screen size.
        // This avoids shrinking/zooming a low-resolution glyph bitmap through GUI.matrix.
        private static Rect PixelRect(Rect rect)
        {
            Vector3 a = GUI.matrix.MultiplyPoint3x4(new Vector3(rect.x, rect.y, 0));
            Vector3 b = GUI.matrix.MultiplyPoint3x4(new Vector3(rect.xMax, rect.yMax, 0));
            return Rect.MinMaxRect(Mathf.Round(a.x), Mathf.Round(a.y), Mathf.Round(b.x), Mathf.Round(b.y));
        }

        private static int PixelSize(int size, int minimum = 11)
            => Mathf.Max(minimum, Mathf.RoundToInt(size * Mathf.Abs(GUI.matrix.m00)));

        private static GUIStyle TextStyle(int pixels, Color color, bool bold, TextAnchor alignment, bool wrap)
        {
            EnsureResources();
            int key = pixels + (bold ? 1000 : 0) + (int)alignment * 2000 + (wrap ? 40000 : 0);
            if (!styles.TryGetValue(key, out var style))
            {
                style = new GUIStyle { font = font, fontSize = pixels, fontStyle = bold ? FontStyle.Bold : FontStyle.Normal,
                    alignment = alignment, wordWrap = wrap, clipping = TextClipping.Clip, richText = false };
                styles.Add(key, style);
            }
            style.normal.textColor = color;
            return style;
        }

        private static void EnsureResources()
        {
            if (font == null)
            {
                font = Font.CreateDynamicFontFromOSFont(new[] { "Segoe UI", "Arial" }, 16);
                if (font == null) font = GUI.skin.font;
                styles.Clear();
            }
            if (circle != null) return;
            circle = new Texture2D(64, 64, TextureFormat.RGBA32, false)
            { name = "HUD corners", filterMode = FilterMode.Bilinear, wrapMode = TextureWrapMode.Clamp, hideFlags = HideFlags.HideAndDontSave };
            var pixels = new Color[4096];
            for (int y = 0; y < 64; y++)
            for (int x = 0; x < 64; x++)
                pixels[y * 64 + x] = new Color(1, 1, 1,
                    Mathf.Clamp01(32 - Vector2.Distance(new Vector2(x + 0.5f, y + 0.5f), new Vector2(32, 32))));
            circle.SetPixels(pixels);
            circle.Apply(false, true);
        }

        public static void Fill(Rect rect, Color color)
        {
            Color previous = GUI.color;
            GUI.color = color;
            GUI.DrawTexture(rect, Texture2D.whiteTexture);
            GUI.color = previous;
        }

        public static void Rounded(Rect rect, Color color, float radius = 12)
        {
            EnsureResources();
            if (rect.width <= 0 || rect.height <= 0) return;
            radius = Mathf.Min(radius, rect.width / 2, rect.height / 2);
            Fill(new Rect(rect.x + radius, rect.y, rect.width - radius * 2, rect.height), color);
            Fill(new Rect(rect.x, rect.y + radius, radius, rect.height - radius * 2), color);
            Fill(new Rect(rect.xMax - radius, rect.y + radius, radius, rect.height - radius * 2), color);
            Color previous = GUI.color;
            GUI.color = color;
            GUI.DrawTextureWithTexCoords(new Rect(rect.x, rect.y, radius, radius), circle, new Rect(0, 0.5f, 0.5f, 0.5f));
            GUI.DrawTextureWithTexCoords(new Rect(rect.xMax - radius, rect.y, radius, radius), circle, new Rect(0.5f, 0.5f, 0.5f, 0.5f));
            GUI.DrawTextureWithTexCoords(new Rect(rect.x, rect.yMax - radius, radius, radius), circle, new Rect(0, 0, 0.5f, 0.5f));
            GUI.DrawTextureWithTexCoords(new Rect(rect.xMax - radius, rect.yMax - radius, radius, radius), circle, new Rect(0.5f, 0, 0.5f, 0.5f));
            GUI.color = previous;
        }

        public static void Card(Rect rect)
        {
            Rounded(new Rect(rect.x, rect.y + 4, rect.width, rect.height), new Color(0, 0, 0, 0.22f), 14);
            Rounded(rect, new Color(0.29f, 0.36f, 0.36f, 0.65f), 14);
            Rounded(new Rect(rect.x + 1, rect.y + 1, rect.width - 2, rect.height - 2), Background, 13);
        }

        public static void Label(Rect rect, string text, int size, Color color, bool bold = false,
            TextAnchor alignment = TextAnchor.MiddleLeft, bool wrap = false)
        {
            Rect pixels = PixelRect(rect);
            int fontSize = PixelSize(size, size >= 14 ? 13 : 11);
            var style = TextStyle(fontSize, color, bold, alignment, wrap);
            pixels.height = Mathf.Max(pixels.height, fontSize + 4);
            Matrix4x4 previous = GUI.matrix;
            try
            {
                GUI.matrix = Matrix4x4.identity;
                GUI.Label(pixels, text, style);
            }
            finally { GUI.matrix = previous; }
        }

        public static void History(Rect rect, IReadOnlyList<string> lines, ref Vector2 scroll)
        {
            Rect pixels = PixelRect(rect);
            var style = TextStyle(PixelSize(18, 14), Text, false, TextAnchor.UpperLeft, true);
            float width = Mathf.Max(40, pixels.width - 24);
            float total = 0;
            foreach (string line in lines) total += style.CalcHeight(new GUIContent(line), width) + 14;
            Matrix4x4 previous = GUI.matrix;
            try
            {
                GUI.matrix = Matrix4x4.identity;
                scroll = GUI.BeginScrollView(pixels, scroll, new Rect(0, 0, width, Mathf.Max(pixels.height, total)));
                float y = 0;
                foreach (string line in lines)
                {
                    float height = style.CalcHeight(new GUIContent(line), width);
                    GUI.Label(new Rect(0, y, width, height), line, style);
                    y += height + 14;
                }
                GUI.EndScrollView();
            }
            finally { GUI.matrix = previous; }
        }

        public static string InputField(Rect rect, string value, string controlName, bool focus)
        {
            EnsureResources();
            Rect pixels = PixelRect(rect);
            var style = new GUIStyle(GUI.skin.textField) {
                font = font, fontSize = PixelSize(19, 14), richText = false,
                padding = new RectOffset(9, 9, 5, 4), alignment = TextAnchor.MiddleLeft
            };
            style.normal.textColor = Text;
            style.focused.textColor = Text;
            Matrix4x4 previous = GUI.matrix;
            try
            {
                GUI.matrix = Matrix4x4.identity;
                GUI.SetNextControlName(controlName);
                string result = GUI.TextField(pixels, value, 400, style);
                if (focus) GUI.FocusControl(controlName);
                return result;
            }
            finally { GUI.matrix = previous; }
        }

        public static void Key(Rect rect, string key, Color accent)
        {
            Rounded(rect, new Color(accent.r, accent.g, accent.b, 0.15f), 6);
            Label(rect, key, 13, accent, true, TextAnchor.MiddleCenter);
        }

        public static void DrawSprite(Rect box, Sprite sprite)
        {
            if (sprite == null || sprite.texture == null) return;
            Rect source = sprite.textureRect;
            float fit = Mathf.Min(box.width / source.width, box.height / source.height);
            Rect target = new Rect(box.center.x - source.width * fit / 2, box.center.y - source.height * fit / 2,
                source.width * fit, source.height * fit);
            Color previous = GUI.color;
            GUI.color = Color.white;
            GUI.DrawTextureWithTexCoords(target, sprite.texture, new Rect(source.x / sprite.texture.width,
                source.y / sprite.texture.height, source.width / sprite.texture.width, source.height / sprite.texture.height));
            GUI.color = previous;
        }

        public static bool Button(Rect rect, string title)
        {
            Rounded(rect, rect.Contains(Event.current.mousePosition) ? new Color(0.40f, 0.96f, 0.81f) : Mint, 10);
            Label(rect, title, 17, new Color(0.035f, 0.10f, 0.08f), true, TextAnchor.MiddleCenter);
            return GUI.Button(rect, GUIContent.none, GUIStyle.none);
        }
    }
}
