using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace GTP
{
    /// <summary>Helpers para construir la UI greybox por código (uGUI + fuente integrada).</summary>
    public static class UIFactory
    {
        private static Font font;

        public static Font DefaultFont
        {
            get
            {
                if (font == null)
                    font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                return font;
            }
        }

        public static Canvas CreateCanvas(Transform parent, string name, int sortOrder)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            go.transform.SetParent(parent, false);
            var canvas = go.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = sortOrder;
            var scaler = go.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;
            return canvas;
        }

        public static RectTransform Rect(Transform parent, string name, Vector2 anchorMin, Vector2 anchorMax,
                                         Vector2 offsetMin = default, Vector2 offsetMax = default)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var rt = (RectTransform)go.transform;
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.offsetMin = offsetMin;
            rt.offsetMax = offsetMax;
            return rt;
        }

        public static Image Panel(Transform parent, string name, Color color, Vector2 anchorMin, Vector2 anchorMax,
                                  Vector2 offsetMin = default, Vector2 offsetMax = default)
        {
            RectTransform rt = Rect(parent, name, anchorMin, anchorMax, offsetMin, offsetMax);
            var image = rt.gameObject.AddComponent<Image>();
            image.color = color;
            return image;
        }

        public static Text Label(Transform parent, string name, string text, int size, TextAnchor align, Color color,
                                 Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin = default, Vector2 offsetMax = default)
        {
            RectTransform rt = Rect(parent, name, anchorMin, anchorMax, offsetMin, offsetMax);
            var label = rt.gameObject.AddComponent<Text>();
            label.font = DefaultFont;
            label.text = text;
            label.fontSize = size;
            label.alignment = align;
            label.color = color;
            label.horizontalOverflow = HorizontalWrapMode.Wrap;
            label.verticalOverflow = VerticalWrapMode.Overflow;
            label.raycastTarget = false;
            return label;
        }

        public static Button Button(Transform parent, string name, string text, Vector2 center, Vector2 size, UnityAction onClick)
        {
            Image bg = Panel(parent, name, new Color(0.12f, 0.16f, 0.3f, 0.95f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f));
            var rt = bg.rectTransform;
            rt.sizeDelta = size;
            rt.anchoredPosition = center;
            var button = bg.gameObject.AddComponent<Button>();
            ColorBlock colors = button.colors;
            colors.highlightedColor = new Color(0.45f, 0.6f, 1f);
            colors.pressedColor = new Color(0.3f, 0.4f, 0.8f);
            colors.selectedColor = new Color(0.35f, 0.5f, 0.95f);
            button.colors = colors;
            button.onClick.AddListener(onClick);
            Label(rt, "Texto", text, 30, TextAnchor.MiddleCenter, Color.white, Vector2.zero, Vector2.one);
            return button;
        }
    }
}
