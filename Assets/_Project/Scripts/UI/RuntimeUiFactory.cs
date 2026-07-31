using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

namespace Necrocis
{
    /// <summary>
    /// Shared primitives for menus that build Unity UI at runtime.
    /// Screen-specific layout and styling remain in each controller.
    /// </summary>
    internal static class RuntimeUiFactory
    {
        public static void EnsureEventSystem()
        {
            if (Object.FindFirstObjectByType<EventSystem>() != null)
            {
                return;
            }

            GameObject eventSystem = new GameObject("EventSystem");
            eventSystem.AddComponent<EventSystem>();
            eventSystem.AddComponent<InputSystemUIInputModule>();
        }

        public static GameObject CreateUiObject(string objectName, Transform parent)
        {
            GameObject uiObject = new GameObject(objectName, typeof(RectTransform));
            uiObject.transform.SetParent(parent, false);
            return uiObject;
        }

        public static Image CreateImage(string objectName, Transform parent, Color color)
        {
            return CreateImage(objectName, parent, null, color);
        }

        public static Image CreateImage(string objectName, Transform parent, Sprite sprite, Color color)
        {
            GameObject imageObject = CreateUiObject(objectName, parent);
            Image image = imageObject.AddComponent<Image>();
            image.sprite = sprite;
            image.color = color;
            return image;
        }

        public static Text CreateText(
            string objectName,
            Transform parent,
            string value,
            Font font,
            int fontSize,
            Color color)
        {
            GameObject textObject = CreateUiObject(objectName, parent);
            Text text = textObject.AddComponent<Text>();
            text.text = value;
            text.font = font;
            text.fontSize = fontSize;
            text.fontStyle = FontStyle.Normal;
            text.color = color;
            text.supportRichText = false;
            text.raycastTarget = false;
            text.horizontalOverflow = HorizontalWrapMode.Overflow;
            text.verticalOverflow = VerticalWrapMode.Overflow;

            Shadow shadow = textObject.AddComponent<Shadow>();
            shadow.effectColor = new Color(0.08f, 0.005f, 0.012f, 0.95f);
            shadow.effectDistance = new Vector2(3f, -3f);
            return text;
        }

        public static Slider CreateVolumeSlider(string objectName, Transform parent, Vector2 position)
        {
            GameObject sliderObject = CreateUiObject(objectName, parent);
            SetRect(
                sliderObject.GetComponent<RectTransform>(),
                new Vector2(0.5f, 0.5f),
                new Vector2(360f, 42f),
                position);

            Slider slider = sliderObject.AddComponent<Slider>();
            slider.minValue = 0f;
            slider.maxValue = 1f;
            slider.direction = Slider.Direction.LeftToRight;

            Image background = CreateImage(
                "Background",
                sliderObject.transform,
                new Color(0.14f, 0.04f, 0.055f, 1f));
            Stretch(background.rectTransform, new Vector2(0f, 13f), new Vector2(0f, -13f));

            GameObject fillArea = CreateUiObject("Fill Area", sliderObject.transform);
            Stretch(fillArea.GetComponent<RectTransform>(), new Vector2(8f, 13f), new Vector2(-8f, -13f));
            Image fill = CreateImage("Fill", fillArea.transform, new Color(0.88f, 0.29f, 0.1f, 1f));
            Stretch(fill.rectTransform);

            GameObject handleArea = CreateUiObject("Handle Slide Area", sliderObject.transform);
            Stretch(handleArea.GetComponent<RectTransform>(), new Vector2(12f, 0f), new Vector2(-12f, 0f));
            Image handle = CreateImage("Handle", handleArea.transform, new Color(1f, 0.76f, 0.33f, 1f));
            SetRect(handle.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(24f, 38f), Vector2.zero);

            slider.fillRect = fill.rectTransform;
            slider.handleRect = handle.rectTransform;
            slider.targetGraphic = handle;
            return slider;
        }

        public static void Stretch(RectTransform rect)
        {
            Stretch(rect, Vector2.zero, Vector2.zero);
        }

        public static void Stretch(RectTransform rect, Vector2 offsetMin, Vector2 offsetMax)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.offsetMin = offsetMin;
            rect.offsetMax = offsetMax;
        }

        public static void SetRect(RectTransform rect, Vector2 anchor, Vector2 size, Vector2 position)
        {
            rect.anchorMin = anchor;
            rect.anchorMax = anchor;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = size;
            rect.anchoredPosition = position;
        }

        public static void SetVerticalNavigation(Selectable selectable, Selectable up, Selectable down)
        {
            Navigation navigation = selectable.navigation;
            navigation.mode = Navigation.Mode.Explicit;
            navigation.selectOnUp = up;
            navigation.selectOnDown = down;
            selectable.navigation = navigation;
        }
    }
}
