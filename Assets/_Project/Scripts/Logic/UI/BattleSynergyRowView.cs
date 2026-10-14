using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>시너지 이름/인원/단계를 표시하고 마우스 진입을 목록의 툴팁에 전달합니다.</summary>
public class BattleSynergyRowView : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerMoveHandler
{
    private BattleSynergyListView _owner;
    private TMP_Text _name;
    private TMP_Text _count;
    private TMP_Text _stages;
    private CanvasGroup _group;
    public BattleSynergyEntry Entry { get; private set; }

    public void Initialize(BattleSynergyListView owner, TMP_FontAsset font)
    {
        _owner = owner;
        Image background = gameObject.AddComponent<Image>();
        background.color = new Color32(42, 48, 60, 255);
        _group = gameObject.AddComponent<CanvasGroup>();
        LayoutElement layout = gameObject.AddComponent<LayoutElement>();
        layout.minHeight = layout.preferredHeight = 60;
        _name = BattleSynergyListView.Label("Name", transform, font, 16);
        _name.textWrappingMode = TextWrappingModes.NoWrap;
        _name.overflowMode = TextOverflowModes.Ellipsis;
        SetBounds(_name.rectTransform, 8, 44, 5, 29);
        _count = BattleSynergyListView.Label("Count", transform, font, 16);
        SetBounds(_count.rectTransform, 0, 8, 5, 29);
        _count.rectTransform.anchorMin = new Vector2(0.8f, 1);
        _count.alignment = TextAlignmentOptions.TopRight;
        _stages = BattleSynergyListView.Label("Stages", transform, font, 12);
        SetBounds(_stages.rectTransform, 8, 8, 34, 54);
        _stages.richText = true;
    }

    public void Bind(BattleSynergyEntry entry)
    {
        Entry = entry;
        _name.text = $"[{entry.Name}]";
        _count.text = $"[{entry.Count}]";
        Color color = entry.IsActive ? new Color32(236, 231, 215, 255) : new Color32(166, 173, 184, 255);
        _name.color = _count.color = _stages.color = color;
        _group.alpha = entry.IsActive ? 1f : 0.45f;
        if (entry.Category == BattleSynergyCategory.Special)
        {
            _stages.text = "특수 관계";
            return;
        }
        string stages = entry.Category == BattleSynergyCategory.Faction ? "진영  " : "병종  ";
        for (int stage = 1; stage <= 4; stage++)
        {
            if (stage > 1) stages += " / ";
            string tint = stage <= entry.Stage ? "E4BD76" : "858C99";
            stages += $"<color=#{tint}>{stage * 2}</color>";
        }
        _stages.text = stages;
    }

    public void OnPointerEnter(PointerEventData eventData) => _owner.ShowTooltip(this, eventData.position);
    public void OnPointerMove(PointerEventData eventData) => _owner.ShowTooltip(this, eventData.position);
    public void OnPointerExit(PointerEventData eventData) => _owner.HideTooltip(this);
    private void OnDisable()
    {
        if (_owner != null) _owner.HideTooltip(this);
    }

    private static void SetBounds(RectTransform rect, float left, float right, float top, float bottom)
    {
        rect.anchorMin = new Vector2(0, 1);
        rect.anchorMax = Vector2.one;
        rect.offsetMin = new Vector2(left, -bottom);
        rect.offsetMax = new Vector2(-right, -top);
    }
}
