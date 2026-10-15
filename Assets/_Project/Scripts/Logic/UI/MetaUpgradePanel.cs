using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>분류는 표시용이며 모든 항목을 독립적으로 구매합니다.</summary>
public class MetaUpgradePanel : MonoBehaviour
{
    private sealed class Card
    {
        public MetaUpgradeDefinition Definition;
        public TMP_Text Level, Value, Cost;
        public Button Buy;
    }
    private readonly List<Card> _cards = new List<Card>();
    private MetaProgressionManager _manager;
    private TMP_FontAsset _font;
    private TMP_Text _points, _message;
    private RectTransform _content;
    private ScrollRect _scroll;
    private Button _retry;
    private readonly Button[] _tabs = new Button[5];
    private MetaUpgradeCategory _category;

    public static MetaUpgradePanel Create(TMP_FontAsset font)
    {
        GameObject root = CreateCanvas("MetaUpgrades", 600);
        var view = root.AddComponent<MetaUpgradePanel>();
        view._manager = MetaProgressionManager.Ins;
        view._font = InnUIFactory.ResolveFont(font);
        RectTransform panel = CreateSurface(root.transform, 1160, 660);
        InnUIFactory.Label("Title", panel, 28, 592, 550, 42, "능력 강화", 32, view._font, TextAlignmentOptions.MidlineLeft);
        view._points = InnUIFactory.Label("Points", panel, 600, 595, 360, 38, "", 23, view._font, TextAlignmentOptions.MidlineRight);
        Button close = InnUIFactory.MakeButton("Close", panel, 990, 596, 140, 38, "메인으로", view._font);
        close.onClick.AddListener(() => root.SetActive(false));
        InnUIFactory.Label("Description", panel, 28, 552, 1104, 30,
            "원하는 능력을 자유롭게 강화하세요. 효과는 매 게임 적용되며, 영구 저장됩니다.", 18, view._font, TextAlignmentOptions.MidlineLeft);
        for (int i = 0; i < view._tabs.Length; i++)
        {
            var category = (MetaUpgradeCategory)i;
            Button tab = InnUIFactory.MakeButton($"Category_{i}", panel, 28 + i * 224, 498, 208, 42,
                MetaUpgradeDefinition.CategoryName(category), view._font);
            tab.onClick.AddListener(() => view.SelectCategory(category));
            view._tabs[i] = tab;
        }

        RectTransform viewport = InnUIFactory.Rect("Viewport", panel, 28, 92, 1104, 390);
        viewport.gameObject.AddComponent<Image>().color = new Color32(22, 28, 38, 255);
        viewport.gameObject.AddComponent<RectMask2D>();
        view._content = InnUIFactory.Rect("Cards", viewport, 0, 0, 1104, 390);
        view._content.anchorMin = view._content.anchorMax = view._content.pivot = new Vector2(0, 1);
        view._scroll = viewport.gameObject.AddComponent<ScrollRect>();
        view._scroll.viewport = viewport;
        view._scroll.content = view._content;
        view._scroll.horizontal = false;
        view._scroll.movementType = ScrollRect.MovementType.Clamped;
        view._scroll.scrollSensitivity = 35;
        view._message = InnUIFactory.Label("Message", panel, 28, 16, 910, 62, "", 17, view._font, TextAlignmentOptions.MidlineLeft);
        view._retry = InnUIFactory.MakeButton("RetrySave", panel, 962, 28, 170, 40, "보상 저장 재시도", view._font);
        view._retry.onClick.AddListener(() => { view._manager.TrySettlePending(); view.Refresh(); });
        root.SetActive(true);
        return view;
    }

    private void OnEnable()
    {
        if (_manager == null) return;
        _manager.OnChanged += Refresh;
        SelectCategory(_category);
    }

    private void OnDisable()
    {
        if (_manager != null) _manager.OnChanged -= Refresh;
    }

    private void SelectCategory(MetaUpgradeCategory category)
    {
        _category = category;
        foreach (Transform child in _content)
        {
            child.gameObject.SetActive(false);
            Destroy(child.gameObject);
        }
        _cards.Clear();
        for (int i = 0; i < _tabs.Length; i++)
            _tabs[i].GetComponent<Image>().color = i == (int)category
                ? new Color32(160, 122, 65, 255) : new Color32(49, 58, 73, 255);
        if (_manager.Progression != null)
        {
            foreach (MetaUpgradeDefinition definition in _manager.Progression.Settings.upgrades)
                if (definition.category == category) AddCard(definition, _cards.Count);
        }
        _content.sizeDelta = new Vector2(1104, Mathf.Max(390, ((_cards.Count + 1) / 2) * 190));
        _scroll.verticalNormalizedPosition = 1f;
        Refresh();
    }

    private void AddCard(MetaUpgradeDefinition definition, int index)
    {
        RectTransform face = InnUIFactory.Rect(definition.id, _content, index % 2 * 560, -(index / 2 + 1) * 190, 544, 178);
        face.anchorMin = face.anchorMax = new Vector2(0, 1);
        face.gameObject.AddComponent<Image>().color = new Color32(37, 46, 59, 255);
        InnUIFactory.Label("Name", face, 18, 132, 345, 34, definition.title, 23, _font, TextAlignmentOptions.MidlineLeft);
        TMP_Text level = InnUIFactory.Label("Level", face, 363, 132, 163, 34, "", 18, _font, TextAlignmentOptions.MidlineRight);
        TMP_Text description = InnUIFactory.Label("Description", face, 18, 75, 508, 54, definition.description, 17, _font, TextAlignmentOptions.MidlineLeft);
        description.enableAutoSizing = true;
        description.fontSizeMin = 14;
        description.fontSizeMax = 17;
        TMP_Text value = InnUIFactory.Label("Value", face, 18, 20, 302, 44, "", 19, _font, TextAlignmentOptions.MidlineLeft);
        Button buy = InnUIFactory.MakeButton("Buy", face, 338, 20, 188, 44, "", _font);
        TMP_Text cost = buy.GetComponentInChildren<TMP_Text>();
        cost.enableAutoSizing = true;
        cost.fontSizeMin = 12;
        cost.fontSizeMax = 18;
        var card = new Card { Definition = definition, Level = level, Value = value, Cost = cost, Buy = buy };
        _cards.Add(card);
        buy.onClick.AddListener(() =>
        {
            bool purchased = _manager.TryPurchase(definition.id, out string reason);
            Refresh();
            _message.text = purchased ? $"{definition.title} 강화 완료! 다음 게임부터 적용됩니다." : reason;
        });
    }

    private void Refresh()
    {
        MetaProgression progression = _manager.Progression;
        _points.text = $"메타포인트  {progression?.Points ?? 0:N0}";
        _retry.gameObject.SetActive(_manager.HasPendingReward);
        _message.text = _manager.LastError ?? _manager.LoadNotice ?? "강화 비용은 레벨마다 증가합니다. 카드에 현재 효과와 다음 효과가 표시됩니다.";
        foreach (Card card in _cards)
        {
            int level = progression.Level(card.Definition.id);
            bool max = level >= card.Definition.maxLevel;
            card.Level.text = $"Lv.{level} / {card.Definition.maxLevel}";
            card.Value.text = max ? $"누적 효과  {card.Definition.FormatValue(level)}"
                : $"현재 {card.Definition.FormatValue(level)} / 다음 {card.Definition.FormatValue(level + 1)}";
            card.Cost.text = max ? "최대 강화" : $"강화  {card.Definition.CostAt(level):N0} P";
            card.Buy.interactable = _manager.IsReady && progression.CanPurchase(card.Definition.id, out _);
        }
    }

    internal static GameObject CreateCanvas(string name, int order)
    {
        var root = new GameObject(name, typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        root.SetActive(false);
        Canvas canvas = root.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = order;
        CanvasScaler scaler = root.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1280, 720);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.Expand;
        RectTransform shade = InnUIFactory.Rect("Shade", root.transform, 0, 0, 0, 0);
        shade.anchorMax = Vector2.one;
        shade.gameObject.AddComponent<Image>().color = new Color(0.025f, 0.035f, 0.05f, 0.96f);
        InnUIFactory.EnsureEventSystem();
        return root;
    }

    internal static RectTransform CreateSurface(Transform parent, float width, float height)
    {
        RectTransform panel = InnUIFactory.Rect("Panel", parent, -width / 2, -height / 2, width, height);
        panel.anchorMin = panel.anchorMax = new Vector2(.5f, .5f);
        panel.gameObject.AddComponent<Image>().color = new Color32(22, 28, 38, 255);
        return panel;
    }
}
