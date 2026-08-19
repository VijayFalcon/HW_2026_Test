using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

namespace DoofusDiaries.UI
{
    /// <summary>
    /// Small helpers for building uGUI elements entirely from code. The whole
    /// project builds its UI at runtime (see UIManager/GameBootstrap) instead
    /// of relying on a hand-authored Canvas prefab, so these few methods are
    /// the single place that knows how a panel/label/button is put together.
    /// </summary>
    internal static class UIFactory
    {
        public static GameObject Panel(Transform parent, string name, Color background)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);

            var rect = go.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            var image = go.AddComponent<Image>();
            image.color = background;
            image.raycastTarget = background.a > 0f;

            return go;
        }

        public static Text Text(Transform parent, string name, string content, int fontSize, Vector2 anchoredPos)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);

            var rect = go.GetComponent<RectTransform>();
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = new Vector2(1000, 200);
            rect.anchoredPosition = anchoredPos;

            var text = go.AddComponent<Text>();
            text.text = content;
            text.font = GetBuiltinFont();
            text.fontSize = fontSize;
            text.alignment = TextAnchor.MiddleCenter;
            text.color = Color.white;
            text.horizontalOverflow = HorizontalWrapMode.Overflow;
            text.verticalOverflow = VerticalWrapMode.Overflow;

            return text;
        }

        public static Button Button(Transform parent, string name, string label, Vector2 anchoredPos, UnityAction onClick)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);

            var rect = go.GetComponent<RectTransform>();
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = new Vector2(320, 110);
            rect.anchoredPosition = anchoredPos;

            var image = go.AddComponent<Image>();
            image.color = new Color(0.2f, 0.6f, 0.9f);

            var button = go.AddComponent<Button>();
            if (onClick != null) button.onClick.AddListener(onClick);

            Text(go.transform, "Label", label, 44, Vector2.zero);

            return button;
        }

        /// <summary>
        /// Unity renamed its built-in font resource across versions
        /// ("Arial.ttf" pre-2022.2, "LegacyRuntime.ttf" from 2022.2 on).
        /// Try both and fall back to null (Unity substitutes a default)
        /// rather than letting UI construction throw on an unfamiliar version.
        /// </summary>
        private static Font GetBuiltinFont()
        {
            Font font = TryGetFont("LegacyRuntime.ttf");
            if (font == null) font = TryGetFont("Arial.ttf");
            return font;
        }

        private static Font TryGetFont(string resourceName)
        {
            try
            {
                return Resources.GetBuiltinResource<Font>(resourceName);
            }
            catch
            {
                return null;
            }
        }
    }
}
