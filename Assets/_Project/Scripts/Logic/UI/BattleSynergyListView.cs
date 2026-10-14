using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>출전 장수의 시너지를 특수 관계, 활성, 대기 순서로 표시합니다.</summary>
[DisallowMultipleComponent]
public class BattleSynergyListView : BaseMono
{
    [SerializeField] private RectTransform rowsRoot;
    [SerializeField] private TMP_Text emptyText;
    [SerializeField] private ScrollRect scroll;
    [SerializeField] private TMP_FontAsset uiFont;
    private SpecialSynergyDefinition[] _specialSynergies;
    private readonly List<Unit> _units = new List<Unit>();
    private readonly List<BattleSynergyRowView> _rows = new List<BattleSynergyRowView>();
    private bool _dirty = true;
    private RectTransform _tooltip;
    private TMP_Text _tooltipText;
    private BattleSynergyRowView _hoveredRow;

    public void Configure(SpecialSynergyDefinition[] specialSynergies, TMP_FontAsset font)
    {
        _specialSynergies = specialSynergies;
        uiFont = font;
        EnsureView();
        _dirty = true;
    }

    private void EnsureView()
    {
        if (rowsRoot != null) return;
        TMP_Text title = Label("SynergyTitle", transform, uiFont, 18);
        title.text = "출전 시너지";
        title.rectTransform.anchorMin = new Vector2(0, 1);
        title.rectTransform.anchorMax = Vector2.one;
        title.rectTransform.offsetMin = new Vector2(0, -28);
        title.rectTransform.offsetMax = Vector2.zero;

        RectTransform viewport = Rect("SynergyViewport", transform);
        Stretch(viewport);
        viewport.offsetMax = new Vector2(0, -36);
        viewport.gameObject.AddComponent<Image>().color = Color.clear;
        viewport.gameObject.AddComponent<RectMask2D>();
        rowsRoot = Rect("Rows", viewport);
        rowsRoot.anchorMin = new Vector2(0, 1);
        rowsRoot.anchorMax = Vector2.one;
        rowsRoot.pivot = new Vector2(0.5f, 1);
        rowsRoot.anchoredPosition = Vector2.zero;
        rowsRoot.sizeDelta = Vector2.zero;
        VerticalLayoutGroup layout = rowsRoot.gameObject.AddComponent<VerticalLayoutGroup>();
        layout.spacing = 6;
        layout.childControlWidth = layout.childControlHeight = true;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = false;
        rowsRoot.gameObject.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        scroll = viewport.gameObject.AddComponent<ScrollRect>();
        scroll.viewport = viewport;
        scroll.content = rowsRoot;
        scroll.horizontal = false;
        scroll.vertical = true;
        scroll.movementType = ScrollRect.MovementType.Clamped;
        scroll.scrollSensitivity = 24;
        emptyText = Label("EmptySynergies", viewport, uiFont, 16);
        Stretch(emptyText.rectTransform);
        emptyText.text = "출전 장수의 시너지가 없습니다.";
    }

    private void OnEnable()
    {
        EventBus<OnBattleFormationChanged>.Subscribe(OnFormationChanged);
        if (scroll != null) scroll.onValueChanged.AddListener(OnScroll);
        _dirty = true;
    }

    private void OnDisable()
    {
        EventBus<OnBattleFormationChanged>.Unsubscribe(OnFormationChanged);
        if (scroll != null) scroll.onValueChanged.RemoveListener(OnScroll);
        HideTooltip();
    }

    private void OnDestroy()
    {
        if (_tooltip != null) Destroy(_tooltip.gameObject);
    }

    private void OnFormationChanged(OnBattleFormationChanged change)
    {
        if (change.Scene == gameObject.scene) _dirty = true;
    }

    private void LateUpdate()
    {
        if (!_dirty) return;
        _dirty = false;
        RefreshList();
    }

    private void RefreshList()
    {
        EnsureView();
        // Configure보다 OnEnable이 먼저 호출된 기존 씬의 컴포넌트도 연결합니다.
        scroll.onValueChanged.RemoveListener(OnScroll);
        scroll.onValueChanged.AddListener(OnScroll);
        HideTooltip();
        BattleFormationUtility.CollectUnits(gameObject.scene, _units);
        var units = new List<BattleSynergyUnit>(_units.Count);
        foreach (Unit unit in _units)
        {
            UnitData data = unit.Data;
            if (data != null) units.Add(new BattleSynergyUnit(data.Id, data.Region, data.WeaponType));
        }
        List<BattleSynergyEntry> entries = BattleSynergyCalculator.Calculate(units, _specialSynergies);
        for (int i = 0; i < entries.Count; i++)
        {
            if (i == _rows.Count)
            {
                BattleSynergyRowView row = Rect("SynergyRow", rowsRoot).gameObject.AddComponent<BattleSynergyRowView>();
                row.Initialize(this, uiFont);
                _rows.Add(row);
            }
            _rows[i].gameObject.SetActive(true);
            _rows[i].Bind(entries[i]);
        }
        for (int i = entries.Count; i < _rows.Count; i++) _rows[i].gameObject.SetActive(false);
        emptyText.gameObject.SetActive(entries.Count == 0);
    }

    private void OnScroll(Vector2 position) => HideTooltip();

    public void ShowTooltip(BattleSynergyRowView row, Vector2 screenPosition)
    {
        if (!isActiveAndEnabled || !row.isActiveAndEnabled) return;
        Canvas canvas = GetComponentInParent<Canvas>()?.rootCanvas;
        if (canvas == null) return;
        if (_tooltip == null)
        {
            // 마스크 밖의 Canvas 직속에 두어 패널 가장자리에서도 툴팁이 잘리지 않습니다.
            _tooltip = Rect("SynergyTooltip", canvas.transform);
            _tooltip.anchorMin = _tooltip.anchorMax = new Vector2(0.5f, 0.5f);
            _tooltip.pivot = new Vector2(0, 1);
            _tooltip.sizeDelta = new Vector2(200, 70);
            Image background = _tooltip.gameObject.AddComponent<Image>();
            background.color = new Color32(12, 16, 23, 245);
            background.raycastTarget = false;
            CanvasGroup group = _tooltip.gameObject.AddComponent<CanvasGroup>();
            group.blocksRaycasts = group.interactable = false;
            _tooltipText = Label("Text", _tooltip, uiFont, 16);
            Stretch(_tooltipText.rectTransform);
            _tooltipText.rectTransform.offsetMin = new Vector2(10, 8);
            _tooltipText.rectTransform.offsetMax = new Vector2(-10, -8);
        }
        _hoveredRow = row;
        _tooltipText.text = $"{row.Entry.Name}\n현재 개수: {row.Entry.Count}";
        RectTransform canvasRect = (RectTransform)canvas.transform;
        Camera camera = canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, screenPosition, camera, out Vector2 point);
        Rect bounds = canvasRect.rect;
        float x = Mathf.Clamp(point.x + 16, bounds.xMin + 8, bounds.xMax - _tooltip.rect.width - 8);
        float y = Mathf.Clamp(point.y - 16, bounds.yMin + _tooltip.rect.height + 8, bounds.yMax - 8);
        _tooltip.localPosition = new Vector3(x, y, 0);
        _tooltip.SetAsLastSibling();
        _tooltip.gameObject.SetActive(true);
    }

    public void HideTooltip(BattleSynergyRowView row = null)
    {
        if (row != null && _hoveredRow != row) return;
        _hoveredRow = null;
        if (_tooltip != null) _tooltip.gameObject.SetActive(false);
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

    internal static TMP_Text Label(string name, Transform parent, TMP_FontAsset font, int size)
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
