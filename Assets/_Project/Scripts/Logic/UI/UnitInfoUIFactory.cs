using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>드래그 컨트롤러가 있는 씬에 유닛 상세 UI를 한 번만 생성합니다.</summary>
public static class UnitInfoUIFactory
{
    public static UnitInfoPanel Ensure(Scene scene)
    {
        foreach (GameObject root in scene.GetRootGameObjects())
        {
            UnitInfoPanel existing = root.GetComponentInChildren<UnitInfoPanel>(true);
            if (existing != null) return existing;
        }

        var canvas = new GameObject("UnitInfoCanvas", typeof(RectTransform), typeof(Canvas),
            typeof(CanvasScaler), typeof(GraphicRaycaster));
        canvas.SetActive(false);
        SceneManager.MoveGameObjectToScene(canvas, scene);
        canvas.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.GetComponent<Canvas>().sortingOrder = 60;
        CanvasScaler scaler = canvas.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1280, 720);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.Expand;
        UnitInfoPanel view = canvas.AddComponent<UnitInfoPanel>();
        view.EnsureView();
        canvas.SetActive(true);
        if (Application.isPlaying) InnUIFactory.EnsureEventSystem();
        return view;
    }

    internal static RectTransform Rect(string name, Transform parent)
    {
        var rect = new GameObject(name, typeof(RectTransform)).GetComponent<RectTransform>();
        rect.SetParent(parent, false);
        return rect;
    }

    internal static void Stretch(RectTransform rect, float left = 0, float bottom = 0, float right = 0, float top = 0)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = new Vector2(left, bottom);
        rect.offsetMax = new Vector2(-right, -top);
    }

    internal static TMP_Text Label(string name, Transform parent, TMP_FontAsset font, int size, string text = "")
    {
        TMP_Text label = Rect(name, parent).gameObject.AddComponent<TextMeshProUGUI>();
        if (font != null) label.font = font;
        label.text = text;
        label.fontSize = size;
        label.alignment = TextAlignmentOptions.TopLeft;
        label.color = new Color32(236, 231, 215, 255);
        label.raycastTarget = false;
        label.richText = false;
        return label;
    }

    internal static void VerticalContent(RectTransform rect, float spacing)
    {
        VerticalLayoutGroup layout = rect.gameObject.AddComponent<VerticalLayoutGroup>();
        layout.spacing = spacing;
        layout.childControlWidth = layout.childControlHeight = true;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = false;
    }

    internal static Image Icon(string name, Transform parent, float size, TMP_FontAsset font, out TMP_Text fallback)
    {
        RectTransform box = Rect(name, parent);
        box.anchorMin = box.anchorMax = box.pivot = new Vector2(0, 1);
        box.anchoredPosition = Vector2.zero;
        box.sizeDelta = new Vector2(size, size);
        box.gameObject.AddComponent<Image>().color = new Color32(45, 51, 65, 255);
        fallback = Label("Fallback", box, font, 14, "이미지\n없음");
        Stretch(fallback.rectTransform);
        fallback.alignment = TextAlignmentOptions.Center;
        RectTransform picture = Rect("Image", box);
        Stretch(picture, 3, 3, 3, 3);
        Image image = picture.gameObject.AddComponent<Image>();
        image.preserveAspect = true;
        image.raycastTarget = false;
        return image;
    }
}
