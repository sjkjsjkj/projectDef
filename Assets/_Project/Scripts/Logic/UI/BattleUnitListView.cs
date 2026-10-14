using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>같은 씬의 전투 슬롯에 실제 배치된 장수를 보여줍니다. 슬롯 변경은 프레임 끝에 모아서 반영합니다.</summary>
[DisallowMultipleComponent]
public class BattleUnitListView : BaseMono
{
    [SerializeField] private TMP_Text countText;
    [SerializeField] private TMP_Text listText;
    private readonly List<Unit> _units = new List<Unit>();
    private bool _dirty = true;

    public IReadOnlyList<Unit> Units => _units;

    public void EnsureView(TMP_FontAsset font)
    {
        if (countText != null && listText != null) return;

        RectTransform root = Rect("DeployedUnits", transform);
        Stretch(root);
        countText = Label("Count", root, font, 18);
        RectTransform countRect = countText.rectTransform;
        countRect.anchorMin = new Vector2(0, 1);
        countRect.anchorMax = Vector2.one;
        countRect.pivot = new Vector2(0, 1);
        countRect.offsetMin = new Vector2(0, -28);
        countRect.offsetMax = Vector2.zero;
        countText.text = "출전 장수 · 0";

        RectTransform viewport = Rect("Viewport", root);
        Stretch(viewport);
        viewport.offsetMax = new Vector2(0, -36);
        Image hitArea = viewport.gameObject.AddComponent<Image>();
        hitArea.color = Color.clear;
        viewport.gameObject.AddComponent<RectMask2D>();

        listText = Label("List", viewport, font, 17);
        RectTransform content = listText.rectTransform;
        content.anchorMin = new Vector2(0, 1);
        content.anchorMax = Vector2.one;
        content.pivot = new Vector2(0, 1);
        content.anchoredPosition = Vector2.zero;
        content.sizeDelta = Vector2.zero;
        listText.lineSpacing = 16;
        listText.margin = new Vector4(0, 0, 6, 0);
        listText.text = "출전 중인 장수가 없습니다.";
        ContentSizeFitter fitter = content.gameObject.AddComponent<ContentSizeFitter>();
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        ScrollRect scroll = viewport.gameObject.AddComponent<ScrollRect>();
        scroll.viewport = viewport;
        scroll.content = content;
        scroll.horizontal = false;
        scroll.vertical = true;
        scroll.movementType = ScrollRect.MovementType.Clamped;
        scroll.scrollSensitivity = 24;
    }

    private void OnEnable()
    {
        EventBus<OnBattleFormationChanged>.Subscribe(OnFormationChanged);
        _dirty = true;
    }

    private void OnDisable()
    {
        EventBus<OnBattleFormationChanged>.Unsubscribe(OnFormationChanged);
        UnsubscribeUnits();
        _units.Clear();
    }

    private void OnFormationChanged(OnBattleFormationChanged change)
    {
        if (change.Scene == gameObject.scene) _dirty = true;
    }

    private void MarkDirty() => _dirty = true;

    private void LateUpdate()
    {
        if (!_dirty) return;
        _dirty = false;
        RefreshList();
    }

    public void RefreshList()
    {
        if (listText == null || countText == null)
            EnsureView(InnUIFactory.ResolveFont(null));
        UnsubscribeUnits();
        BattleFormationUtility.CollectUnits(gameObject.scene, _units);
        foreach (Unit unit in _units)
            unit.OnLevelChanged += MarkDirty;

        countText.text = $"출전 장수 · {_units.Count}";
        var text = new StringBuilder();
        for (int i = 0; i < _units.Count; i++)
        {
            Unit unit = _units[i];
            string unitName = unit.Data != null && !string.IsNullOrWhiteSpace(unit.Data.UnitName)
                ? unit.Data.UnitName : unit.name;
            if (i > 0) text.AppendLine();
            text.Append(unitName).Append("  ·  Lv.").Append(unit.Level);
        }
        listText.text = _units.Count == 0 ? "출전 중인 장수가 없습니다." : text.ToString();
    }

    private void UnsubscribeUnits()
    {
        foreach (Unit unit in _units)
            if (unit != null) unit.OnLevelChanged -= MarkDirty;
    }

    private static RectTransform Rect(string name, Transform parent)
    {
        var rect = new GameObject(name, typeof(RectTransform)).GetComponent<RectTransform>();
        rect.SetParent(parent, false);
        return rect;
    }

    private static void Stretch(RectTransform rect)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = rect.offsetMax = Vector2.zero;
    }

    private static TMP_Text Label(string name, Transform parent, TMP_FontAsset font, int size)
    {
        TMP_Text label = Rect(name, parent).gameObject.AddComponent<TextMeshProUGUI>();
        if (font != null) label.font = font;
        label.fontSize = size;
        label.alignment = TextAlignmentOptions.TopLeft;
        label.color = new Color32(236, 231, 215, 255);
        label.raycastTarget = false;
        label.richText = false;
        return label;
    }
}
