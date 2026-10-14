using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>첫 메인 화면 구성. 생성된 UI는 MainScene 수명만 가집니다.</summary>
public static class MainMenuUIFactory
{
    public static MainMenuView Create(TMP_FontAsset font = null)
    {
        font = InnUIFactory.ResolveFont(font);
        var root = new GameObject("MainMenuCanvas", typeof(RectTransform), typeof(Canvas),
            typeof(CanvasScaler), typeof(GraphicRaycaster));
        root.SetActive(false);
        Canvas canvas = root.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;
        CanvasScaler scaler = root.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1280, 720);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.Expand;

        var background = new GameObject("Background", typeof(RectTransform), typeof(Image));
        background.transform.SetParent(root.transform, false);
        RectTransform bg = (RectTransform)background.transform;
        bg.anchorMin = Vector2.zero;
        bg.anchorMax = Vector2.one;
        bg.offsetMin = bg.offsetMax = Vector2.zero;
        background.GetComponent<Image>().color = new Color32(16, 22, 30, 255);

        Label("Eyebrow", root.transform, 238, 380, 32, "THREE KINGDOMS  /  DEFENSE", 16, font,
            new Color32(190, 157, 95, 255));
        Label("Title", root.transform, 174, 760, 86, "삼국지 디펜스", 58, font, Color.white);
        Label("Subtitle", root.transform, 103, 600, 35, "장수를 모으고, 밀려오는 적을 막아내세요", 19, font,
            new Color32(157, 169, 184, 255));

        Button start = Button(root.transform, "StartGame", "게임시작", 20, font, true);
        Button upgrade = Button(root.transform, "Upgrades", "능력 강화", -48, font, false);
        Button settings = Button(root.transform, "Settings", "설정", -116, font, false);
        Button quit = Button(root.transform, "Quit", "게임 종료", -184, font, false);
        TMP_Text status = Label("Status", root.transform, -264, 1000, 54, "", 17, font,
            new Color32(157, 169, 184, 255));
        MainMenuView view = root.AddComponent<MainMenuView>();
        view.Configure(start, upgrade, settings, quit, status);
        root.SetActive(true);
        return view;
    }

    private static Button Button(Transform parent, string name, string text, float y, TMP_FontAsset font, bool primary)
    {
        RectTransform rect = Rect(name, parent, y, 340, 56);
        Image image = rect.gameObject.AddComponent<Image>();
        image.color = primary ? new Color32(190, 157, 95, 255) : new Color32(37, 47, 60, 255);
        Button button = rect.gameObject.AddComponent<Button>();
        button.targetGraphic = image;
        ColorBlock colors = button.colors;
        colors.highlightedColor = new Color(1.15f, 1.15f, 1.15f);
        colors.pressedColor = new Color(0.75f, 0.75f, 0.75f);
        colors.disabledColor = new Color(0.45f, 0.45f, 0.45f);
        button.colors = colors;
        Label("Label", rect, 0, 320, 50, text, 23, font,
            primary ? new Color32(20, 25, 32, 255) : Color.white);
        return button;
    }

    private static TMP_Text Label(string name, Transform parent, float y, float width, float height,
        string text, float size, TMP_FontAsset font, Color color)
    {
        var label = Rect(name, parent, y, width, height).gameObject.AddComponent<TextMeshProUGUI>();
        label.font = font;
        label.text = text;
        label.fontSize = size;
        label.alignment = TextAlignmentOptions.Center;
        label.color = color;
        label.raycastTarget = false;
        return label;
    }

    private static RectTransform Rect(string name, Transform parent, float y, float width, float height)
    {
        var rect = (RectTransform)new GameObject(name, typeof(RectTransform)).transform;
        rect.SetParent(parent, false);
        rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = new Vector2(0, y);
        rect.sizeDelta = new Vector2(width, height);
        return rect;
    }
}
