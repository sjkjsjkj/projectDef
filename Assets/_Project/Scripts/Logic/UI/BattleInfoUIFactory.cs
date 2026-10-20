using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>좌상단 전투 정보 UI를 생성합니다. 각 Content 아래에 실제 정보 UI를 추가하세요.</summary>
public static class BattleInfoUIFactory
{
    public static BattleInfoPanel Create(TMP_FontAsset font = null)
    {
        // 에디터에서 임시 기본 폰트를 사용해도 런타임에는 한글 폰트를 다시 확인합니다.
        TMP_FontAsset displayFont = InnUIFactory.ResolveFont(font);
        var root = new GameObject("BattleInfoCanvas", typeof(RectTransform), typeof(Canvas),
            typeof(CanvasScaler), typeof(GraphicRaycaster));
        root.SetActive(false);
        Canvas canvas = root.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 50;
        CanvasScaler scaler = root.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1280, 720);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.Expand;

        RectTransform panel = Rect("BattleInfo", root.transform, 20, 20, 260, 340);
        panel.gameObject.AddComponent<Image>().color = new Color32(22, 25, 33, 245);
        Label("Title", panel, 16, 12, 160, 30, "전투 정보", 22, displayFont, TextAlignmentOptions.MidlineLeft);
        TMP_Text hint = Label("ShortcutHint", panel, 184, 14, 180, 26, "Tab · 다음 항목", 14, displayFont,
            TextAlignmentOptions.MidlineRight);
        hint.color = new Color32(163, 173, 190, 255);

        string[] names = { "Units", "Synergies" };
        var tabs = new BattleInfoPanel.Tab[names.Length];
        RectTransform body = Rect("Body", panel, 12, 100, 356, 228);
        body.gameObject.AddComponent<Image>().color = new Color32(30, 35, 45, 255);
        for (int i = 0; i < tabs.Length; i++)
        {
            RectTransform tabRect = Rect(names[i] + "Tab", panel, 12 + i * 120, 54, 116, 36);
            Image face = tabRect.gameObject.AddComponent<Image>();
            face.color = i == 0 ? new Color32(116, 87, 43, 255) : new Color32(45, 51, 65, 255);
            Button button = tabRect.gameObject.AddComponent<Button>();
            button.targetGraphic = face;
            // 탭의 키보드 순환은 BattleInfoPanel이 전담합니다.
            button.navigation = new Navigation { mode = Navigation.Mode.None };

            RectTransform content = Rect(names[i] + "Content", body, 0, 0, 0, 0);
            content.anchorMin = Vector2.zero;
            content.anchorMax = Vector2.one;
            content.offsetMin = new Vector2(12, 12);
            content.offsetMax = new Vector2(-12, -12);
            content.gameObject.SetActive(i == 0);
            tabs[i] = new BattleInfoPanel.Tab { button = button, content = content.gameObject };
        }

        BattleInfoPanel view = root.AddComponent<BattleInfoPanel>();
        view.Configure(tabs, font);
        ApplyCompactLayout(view);
        view.EnsureUnitList(displayFont);
        view.EnsureSynergyList(displayFont);
        root.SetActive(true);
        if (Application.isPlaying) InnUIFactory.EnsureEventSystem();
        return view;
    }

    /// <summary>기존 본문과 탭 참조를 유지하며 폭과 테스트 아이콘을 갱신합니다.</summary>
    public static void ApplyCompactLayout(BattleInfoPanel view)
    {
        BattleInfoSkin skin = BattleInfoSkin.Load();
        if (skin != null)
        {
            skin.ApplyPanel(view);
            return;
        }
        RectTransform panel = view.transform.Find("BattleInfo") as RectTransform;
        if (panel == null) return;
        panel.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 260);

        TMP_Text title = panel.Find("Title")?.GetComponent<TMP_Text>();
        if (title != null)
        {
            title.rectTransform.sizeDelta = new Vector2(120, 30);
            title.fontSize = 20;
        }
        TMP_Text hint = panel.Find("ShortcutHint")?.GetComponent<TMP_Text>();
        if (hint != null)
        {
            hint.text = "Tab · 다음";
            hint.fontSize = 12;
            RectTransform rect = hint.rectTransform;
            rect.anchorMin = rect.anchorMax = rect.pivot = Vector2.one;
            rect.anchoredPosition = new Vector2(-16, -14);
            rect.sizeDelta = new Vector2(100, 26);
        }

        RectTransform body = panel.Find("Body") as RectTransform;
        if (body != null)
        {
            body.anchorMin = Vector2.zero;
            body.anchorMax = Vector2.one;
            body.offsetMin = new Vector2(12, 12);
            body.offsetMax = new Vector2(-12, -100);
        }

        string[] names = { "Units", "Synergies", "DFS" };
        // Unity의 기본 UI에서 사용하는 원형 손잡이, 사각형, 삼각형 화살표입니다.
        string[] spritePaths = { "UI/Skin/Knob.psd", "UI/Skin/UISprite.psd", "UI/Skin/DropdownArrow.psd" };
        for (int i = 0; i < names.Length; i++)
        {
            RectTransform tab = panel.Find(names[i] + "Tab") as RectTransform;
            if (tab == null) continue;
            tab.anchorMin = new Vector2(i / 3f, 1);
            tab.anchorMax = new Vector2((i + 1) / 3f, 1);
            tab.offsetMin = new Vector2(i == 0 ? 12 : 4, -94);
            tab.offsetMax = new Vector2(i == 2 ? -12 : -4, -54);
            Transform oldLabel = tab.Find("Label");
            if (oldLabel != null) oldLabel.gameObject.SetActive(false);

            RectTransform iconRect = tab.Find("Icon") as RectTransform;
            if (iconRect == null) iconRect = Rect("Icon", tab, 0, 0, 22, 22);
            iconRect.anchorMin = iconRect.anchorMax = iconRect.pivot = new Vector2(0.5f, 0.5f);
            iconRect.anchoredPosition = Vector2.zero;
            iconRect.sizeDelta = new Vector2(22, 22);
            iconRect.localRotation = i == 2 ? Quaternion.Euler(0, 0, 180) : Quaternion.identity;
            Image icon = iconRect.GetComponent<Image>();
            if (icon == null) icon = iconRect.gameObject.AddComponent<BattleInfoTabIcon>();
            if (icon is BattleInfoTabIcon testIcon) testIcon.ShapeIndex = i;
#if UNITY_EDITOR
            // Unity의 UI 생성 메뉴와 같은 내장 에셋을 사용하며 씬에 참조가 저장됩니다.
            icon.sprite = UnityEditor.AssetDatabase.GetBuiltinExtraResource<Sprite>(spritePaths[i]);
#endif
            icon.preserveAspect = true;
            icon.color = new Color32(236, 231, 215, 255);
            icon.raycastTarget = false;
        }
    }

    private static RectTransform Rect(string name, Transform parent, float x, float y, float width, float height)
    {
        var rect = new GameObject(name, typeof(RectTransform)).GetComponent<RectTransform>();
        rect.SetParent(parent, false);
        rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(0, 1);
        rect.anchoredPosition = new Vector2(x, -y);
        rect.sizeDelta = new Vector2(width, height);
        return rect;
    }

    private static TMP_Text Label(string name, Transform parent, float x, float y, float width, float height,
        string text, int size, TMP_FontAsset font, TextAlignmentOptions alignment)
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
}
