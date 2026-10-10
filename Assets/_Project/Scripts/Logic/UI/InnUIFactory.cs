using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

/// <summary>씬 편집 메뉴와 런타임에서 동일한 객잔 UI를 생성합니다.</summary>
public static class InnUIFactory
{
    private static readonly Color Background = new Color32(22, 25, 33, 255);
    private static readonly Color Surface = new Color32(35, 40, 51, 255);
    private static TMP_FontAsset _fallbackFont;

    public static TMP_FontAsset ResolveFont(TMP_FontAsset preferred)
    {
        if (preferred != null) return preferred;
        TMP_FontAsset standard = TMP_Settings.defaultFontAsset;
        if (standard != null && standard.HasCharacter('객')) return standard;
        if (!Application.isPlaying) return standard;
        if (_fallbackFont == null)
        {
            Font source = Font.CreateDynamicFontFromOSFont(
                new[] { "Malgun Gothic", "Apple SD Gothic Neo", "Noto Sans CJK KR", "Arial" }, 24);
            _fallbackFont = TMP_FontAsset.CreateFontAsset(source);
            if (_fallbackFont != null) _fallbackFont.isMultiAtlasTexturesEnabled = true;
        }
        return _fallbackFont != null ? _fallbackFont : standard;
    }

    public static InnPanel Create(InnManager manager, TMP_FontAsset font = null)
    {
        font = ResolveFont(manager != null && manager.UIFont != null ? manager.UIFont : font);
        var root = new GameObject("InnCanvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        root.SetActive(false);
        Canvas canvas = root.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;
        CanvasScaler scaler = root.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1280, 720);
        // 화면 비율이 달라도 패널 전체가 화면 안에 들어오도록 맞춥니다.
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.Expand;

        RectTransform panel = Rect("Shop", root.transform, -560, -220, 1120, 440);
        panel.anchorMin = panel.anchorMax = new Vector2(0.5f, 0.5f);
        panel.gameObject.AddComponent<Image>().color = Background;
        Label("Title", panel, 28, 374, 200, 40, "객잔", 30, font, TextAlignmentOptions.MidlineLeft);
        Label("Subtitle", panel, 28, 347, 640, 25, "함께 싸울 장수를 고용하세요", 16, font, TextAlignmentOptions.MidlineLeft);
        Button close = MakeButton("Close", panel, 988, 378, 104, 36, "닫기  ×", font);

        TMP_Text honorStatus = Label("Honor", panel, 28, 292, 470, 40, "명예", 22, font, TextAlignmentOptions.MidlineLeft);
        TMP_Text gold = Label("Gold", panel, 520, 292, 260, 40, "보유 골드", 21, font);
        TMP_Text tickets = Label("Tickets", panel, 820, 292, 272, 40, "초기화권", 21, font);

        var views = new InnOfferView[InnManager.OfferCount];
        for (int i = 0; i < views.Length; i++)
        {
            RectTransform card = Rect($"Offer_{i + 1}", panel, 28 + i * 216, 110, 200, 170);
            Image border = card.gameObject.AddComponent<Image>();
            border.color = InnOfferView.GradeColor(1);
            RectTransform inner = Rect("CardFace", card, 3, 3, 194, 164);
            Image face = inner.gameObject.AddComponent<Image>();
            face.color = Surface;
            face.raycastTarget = false;
            Button buy = card.gameObject.AddComponent<Button>();
            buy.targetGraphic = face;
            ColorBlock colors = buy.colors;
            colors.highlightedColor = new Color(1.3f, 1.3f, 1.3f);
            colors.pressedColor = new Color(0.7f, 0.7f, 0.7f);
            colors.disabledColor = new Color(0.55f, 0.55f, 0.55f);
            buy.colors = colors;
            TMP_Text name = Label("Name", card, 12, 67, 176, 66, "장수", 28, font);
            name.enableAutoSizing = true;
            name.fontSizeMin = 18;
            name.fontSizeMax = 28;
            TMP_Text grade = Label("Grade", card, 12, 128, 176, 28, "등급", 17, font);
            TMP_Text price = Label("Price", card, 12, 14, 176, 35, "고용", 20, font);
            views[i] = card.gameObject.AddComponent<InnOfferView>();
            views[i].Configure(name, grade, price, buy, border);
        }

        Button honor = MakeButton("BuyHonor", panel, 28, 48, 310, 44, "명예 획득", font);
        Button refresh = MakeButton("Refresh", panel, 768, 48, 324, 44, "목록 초기화 · 초기화권 1장", font);
        TMP_Text message = Label("Message", panel, 28, 8, 1064, 30, "", 16, font, TextAlignmentOptions.MidlineLeft);
        Button open = MakeButton("OpenShop", root.transform, -188, 28, 160, 52, "객잔 / 상점", font);
        var openRect = (RectTransform)open.transform;
        openRect.anchorMin = openRect.anchorMax = new Vector2(1, 0);

        InnPanel view = root.AddComponent<InnPanel>();
        view.Configure(manager, panel.gameObject, open, close, refresh, honor,
            honor.GetComponentInChildren<TMP_Text>(), honorStatus, gold, tickets, message, views);
        panel.gameObject.SetActive(false);
        root.SetActive(true);
        if (Application.isPlaying) EnsureEventSystem();
        return view;
    }

    public static GameObject EnsureEventSystem()
    {
        if (Object.FindAnyObjectByType<EventSystem>() != null) return null;
        var events = new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
        events.GetComponent<InputSystemUIInputModule>().AssignDefaultActions();
        return events;
    }

    private static RectTransform Rect(string name, Transform parent, float x, float y, float width, float height)
    {
        var rect = new GameObject(name, typeof(RectTransform)).GetComponent<RectTransform>();
        rect.SetParent(parent, false);
        rect.anchorMin = rect.anchorMax = rect.pivot = Vector2.zero;
        rect.anchoredPosition = new Vector2(x, y);
        rect.sizeDelta = new Vector2(width, height);
        return rect;
    }

    private static TMP_Text Label(string name, Transform parent, float x, float y, float width, float height,
        string text, int size, TMP_FontAsset font, TextAlignmentOptions alignment = TextAlignmentOptions.Center)
    {
        var label = Rect(name, parent, x, y, width, height).gameObject.AddComponent<TextMeshProUGUI>();
        if (font != null) label.font = font;
        label.text = text;
        label.fontSize = size;
        label.alignment = alignment;
        label.color = new Color32(236, 231, 215, 255);
        label.raycastTarget = false;
        return label;
    }

    private static Button MakeButton(string name, Transform parent, float x, float y, float width, float height,
        string text, TMP_FontAsset font)
    {
        RectTransform rect = Rect(name, parent, x, y, width, height);
        Image face = rect.gameObject.AddComponent<Image>();
        face.color = new Color32(88, 67, 37, 255);
        Button button = rect.gameObject.AddComponent<Button>();
        button.targetGraphic = face;
        Label("Label", rect, 6, 0, width - 12, height, text, 18, font);
        return button;
    }
}
