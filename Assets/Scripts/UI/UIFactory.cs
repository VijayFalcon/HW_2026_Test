// Helper methods for building uGUI panels, labels, and buttons entirely
// from code, since the project has no hand-authored Canvas/prefab.

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

namespace DoofusDiaries.UI
{
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

        public static Text Text(Transform parent, string name, string content, int fontSize, Vector2 anchoredPos, Vector2? anchor = null)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);

            var rect = go.GetComponent<RectTransform>();
            rect.anchorMin = rect.anchorMax = anchor ?? new Vector2(0.5f, 0.5f);
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

        public static Button Button(Transform parent, string name, string label, Vector2 anchoredPos, UnityAction onClick, Vector2? size = null, int labelFontSize = 44)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);

            var rect = go.GetComponent<RectTransform>();
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = size ?? new Vector2(320, 110);
            rect.anchoredPosition = anchoredPos;

            var image = go.AddComponent<Image>();
            image.color = new Color(0.2f, 0.6f, 0.9f);

            var button = go.AddComponent<Button>();
            if (onClick != null) button.onClick.AddListener(onClick);

            Text(go.transform, "Label", label, labelFontSize, Vector2.zero);

            return button;
        }

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
