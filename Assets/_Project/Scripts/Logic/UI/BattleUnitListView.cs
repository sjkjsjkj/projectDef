using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>같은 씬의 전투 슬롯에 실제 배치된 장수를 보여줍니다. 슬롯 변경은 프레임 끝에 모아서 반영합니다.</summary>
[DisallowMultipleComponent]
public class BattleUnitListView : BaseMono
{
    [SerializeField] private TMP_Text countText;
    [SerializeField] private TMP_Text listText;
    [SerializeField] private RectTransform rowsRoot;
    [SerializeField] private TMP_Text emptyText;
    [SerializeField] private TMP_FontAsset uiFont;
    private readonly List<BattleUnitRowView> _rows = new List<BattleUnitRowView>();
    private readonly List<Unit> _units = new List<Unit>();
    private bool _dirty = true;

    public IReadOnlyList<Unit> Units => _units;

    public void EnsureView(TMP_FontAsset font)
    {
        uiFont = font;
        // 이전 씬에 저장된 텍스트 목록을 숨기고 기존 루트/스크롤을 재사용합니다.
        if (listText != null) listText.gameObject.SetActive(false);
        RectTransform root = BattleInfoSkin.Child("DeployedUnits", transform);
        Stretch(root);
        BattleInfoSkin skin = BattleInfoSkin.Load();
        if (skin != null)
        {
            Image strip = BattleInfoSkin.ImageChild("CountStrip", root, skin.countStrip);
            BattleInfoSkin.Stretch(strip.rectTransform, 0, 0, 0, 0);
            strip.rectTransform.anchorMin = new Vector2(0, 1);
            strip.rectTransform.offsetMin = new Vector2(0, -26);
            strip.transform.SetAsFirstSibling();
        }
        countText = BattleInfoSkin.TextChild("Count", root, font, 14);
        BattleInfoSkin.Stretch(countText.rectTransform, 10, 10, 0, 0);
        countText.rectTransform.anchorMin = new Vector2(0, 1);
        countText.rectTransform.offsetMin = new Vector2(10, -26);
        if (string.IsNullOrEmpty(countText.text)) countText.text = "출전 장수 · 0";

        RectTransform viewport = BattleInfoSkin.Child("Viewport", root);
        Stretch(viewport);
        viewport.offsetMax = new Vector2(0, -32);
        Image hitArea = viewport.GetComponent<Image>();
        if (hitArea == null) hitArea = viewport.gameObject.AddComponent<Image>();
        hitArea.color = Color.clear;
        if (viewport.GetComponent<RectMask2D>() == null) viewport.gameObject.AddComponent<RectMask2D>();
        rowsRoot = BattleInfoSkin.Child("Rows", viewport);
        rowsRoot.anchorMin = new Vector2(0, 1);
        rowsRoot.anchorMax = Vector2.one;
        rowsRoot.pivot = new Vector2(0.5f, 1);
        rowsRoot.anchoredPosition = rowsRoot.sizeDelta = Vector2.zero;
        VerticalLayoutGroup layout = rowsRoot.GetComponent<VerticalLayoutGroup>();
        if (layout == null) layout = rowsRoot.gameObject.AddComponent<VerticalLayoutGroup>();
        layout.spacing = 6;
        layout.childControlWidth = layout.childControlHeight = true;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = false;
        ContentSizeFitter fitter = rowsRoot.GetComponent<ContentSizeFitter>();
        if (fitter == null) fitter = rowsRoot.gameObject.AddComponent<ContentSizeFitter>();
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        ScrollRect scroll = viewport.GetComponent<ScrollRect>();
        if (scroll == null) scroll = viewport.gameObject.AddComponent<ScrollRect>();
        scroll.viewport = viewport;
        scroll.content = rowsRoot;
        scroll.horizontal = false;
        scroll.vertical = true;
        scroll.movementType = ScrollRect.MovementType.Clamped;
        scroll.scrollSensitivity = 24;
        emptyText = BattleInfoSkin.TextChild("EmptyUnits", viewport, font, 14);
        Stretch(emptyText.rectTransform);
        emptyText.alignment = TextAlignmentOptions.TopLeft;
        emptyText.text = "출전 중인 장수가 없습니다.";
        if (_rows.Count == 0)
            _rows.AddRange(rowsRoot.GetComponentsInChildren<BattleUnitRowView>(true));
        foreach (BattleUnitRowView row in _rows) row.Initialize(font);
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
        if (rowsRoot == null || countText == null)
            EnsureView(InnUIFactory.ResolveFont(null));
        UnsubscribeUnits();
        BattleFormationUtility.CollectUnits(gameObject.scene, _units);
        foreach (Unit unit in _units)
            unit.OnLevelChanged += MarkDirty;

        countText.text = $"출전 장수 · {_units.Count}";
        for (int i = 0; i < _units.Count; i++)
        {
            if (i == _rows.Count)
            {
                BattleUnitRowView row = Rect("UnitRow", rowsRoot).gameObject.AddComponent<BattleUnitRowView>();
                row.Initialize(uiFont);
                _rows.Add(row);
            }
            _rows[i].gameObject.SetActive(true);
            _rows[i].Bind(_units[i]);
        }
        for (int i = _units.Count; i < _rows.Count; i++) _rows[i].gameObject.SetActive(false);
        emptyText.gameObject.SetActive(_units.Count == 0);
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

}
