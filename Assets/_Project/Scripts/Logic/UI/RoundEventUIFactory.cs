using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>씬 프리팹 없이도 연결 즉시 사용할 수 있는 기본 이벤트 화면입니다.</summary>
public static class RoundEventUIFactory
{
    public static RoundEventPanel Create(RoundEventManager manager, TMP_FontAsset preferredFont)
    {
        TMP_FontAsset font = InnUIFactory.ResolveFont(preferredFont);
        var root = new GameObject("RoundEventCanvas", typeof(RectTransform), typeof(Canvas),
            typeof(CanvasScaler), typeof(GraphicRaycaster));
        root.SetActive(false);
        Canvas canvas = root.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 500;
        CanvasScaler scaler = root.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1280, 720);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.Expand;

        RectTransform shade = InnUIFactory.Rect("InputBlocker", root.transform, 0, 0, 0, 0);
        shade.anchorMax = Vector2.one;
        shade.gameObject.AddComponent<Image>().color = new Color(0.02f, 0.03f, 0.05f, 0.92f);
        RectTransform panel = InnUIFactory.Rect("Event", shade, -600, -320, 1200, 640);
        panel.anchorMin = panel.anchorMax = new Vector2(0.5f, 0.5f);
        panel.gameObject.AddComponent<Image>().color = new Color32(22, 25, 33, 255);
        TMP_Text title = InnUIFactory.Label("Title", panel, 36, 568, 1128, 48, "", 32, font);
        TMP_Text description = InnUIFactory.Label("Description", panel, 36, 526, 1128, 36, "", 19, font);
        Image monster = InnUIFactory.Rect("Monster", panel, 50, 162, 290, 330).gameObject.AddComponent<Image>();
        monster.preserveAspect = true;
        monster.raycastTarget = false;
        TMP_Text gold = InnUIFactory.Label("Gold", panel, 36, 68, 1128, 36, "", 19, font);
        TMP_Text message = InnUIFactory.Label("Message", panel, 36, 25, 1128, 34, "", 17, font);

        RectTransform cardsRoot = InnUIFactory.Rect("Choices", panel, 370, 136, 794, 360);
        var layout = cardsRoot.gameObject.AddComponent<HorizontalLayoutGroup>();
        layout.spacing = 12;
        layout.childControlWidth = layout.childControlHeight = true;
        layout.childForceExpandWidth = layout.childForceExpandHeight = true;
        var cards = new RoundEventCard[4];
        for (int i = 0; i < cards.Length; i++)
        {
            RectTransform card = InnUIFactory.Rect($"Choice_{i + 1}", cardsRoot, 0, 0, 190, 360);
            card.gameObject.AddComponent<Image>().color = new Color32(39, 44, 56, 255);
            card.gameObject.AddComponent<LayoutElement>().flexibleWidth = 1;
            Image icon = StretchRect("Icon", card, 0.25f, 0.76f, 0.75f, 0.96f).gameObject.AddComponent<Image>();
            icon.preserveAspect = true;
            icon.raycastTarget = false;
            TMP_Text name = Label("Title", card, 0.05f, 0.61f, 0.95f, 0.75f, font, 24);
            TMP_Text detail = Label("Description", card, 0.07f, 0.29f, 0.93f, 0.60f, font, 18);
            Button select = InnUIFactory.MakeButton("Select", card, 0, 0, 180, 44, "선택", font);
            Stretch((RectTransform)select.transform, 0.06f, 0.16f, 0.94f, 0.28f);
            Stretch(select.GetComponentInChildren<TMP_Text>().rectTransform, 0.03f, 0, 0.97f, 1);
            Button refresh = InnUIFactory.MakeButton("Refresh", card, 0, 0, 180, 40, "", font);
            Stretch((RectTransform)refresh.transform, 0.06f, 0.03f, 0.94f, 0.14f);
            TMP_Text refreshLabel = refresh.GetComponentInChildren<TMP_Text>();
            Stretch(refreshLabel.rectTransform, 0.03f, 0, 0.97f, 1);
            refreshLabel.enableAutoSizing = true;
            refreshLabel.fontSizeMin = 11;
            refreshLabel.fontSizeMax = 17;
            cards[i] = new RoundEventCard { Root = card.gameObject, Title = name, Description = detail,
                Icon = icon, Select = select, Refresh = refresh, RefreshLabel = refreshLabel };
        }
        RoundEventPanel view = root.AddComponent<RoundEventPanel>();
        view.Configure(manager, title, description, gold, message, monster, cards);
        InnUIFactory.EnsureEventSystem();
        // 활성화는 세션 준비가 끝난 매니저가 담당합니다.
        return view;
    }

    private static RectTransform StretchRect(string name, Transform parent, float xMin, float yMin, float xMax, float yMax)
    {
        RectTransform rect = InnUIFactory.Rect(name, parent, 0, 0, 0, 0);
        Stretch(rect, xMin, yMin, xMax, yMax);
        return rect;
    }

    private static void Stretch(RectTransform rect, float xMin, float yMin, float xMax, float yMax)
    {
        rect.anchorMin = new Vector2(xMin, yMin);
        rect.anchorMax = new Vector2(xMax, yMax);
        rect.offsetMin = rect.offsetMax = Vector2.zero;
    }

    private static TMP_Text Label(string name, Transform parent, float xMin, float yMin, float xMax,
        float yMax, TMP_FontAsset font, int size)
    {
        var label = StretchRect(name, parent, xMin, yMin, xMax, yMax).gameObject.AddComponent<TextMeshProUGUI>();
        if (font != null) label.font = font;
        label.fontSize = label.fontSizeMax = size;
        label.fontSizeMin = 12;
        label.enableAutoSizing = true;
        label.alignment = TextAlignmentOptions.Center;
        label.raycastTarget = false;
        label.color = new Color32(236, 231, 215, 255);
        return label;
    }
}
