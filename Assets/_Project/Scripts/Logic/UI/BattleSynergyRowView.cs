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
    private TMP_Text _grade;
    private CanvasGroup _group;
    public BattleSynergyEntry Entry { get; private set; }

    public void Initialize(BattleSynergyListView owner, TMP_FontAsset font)
    {
        _owner = owner;
        Image background = GetComponent<Image>();
        if (background == null) background = gameObject.AddComponent<Image>();
        background.color = new Color32(42, 48, 60, 255);
        BattleInfoSkin skin = BattleInfoSkin.Load();
        if (skin != null) BattleInfoSkin.SetImage(background, skin.synergyRow);
        _group = GetComponent<CanvasGroup>();
        if (_group == null) _group = gameObject.AddComponent<CanvasGroup>();
        LayoutElement layout = GetComponent<LayoutElement>();
        if (layout == null) layout = gameObject.AddComponent<LayoutElement>();
        layout.minHeight = layout.preferredHeight = 48;
        if (skin != null)
        {
            Image frame = BattleInfoSkin.ImageChild("IconFrame", transform, skin.emptyFrame);
            BattleInfoSkin.Place(frame.rectTransform, 8, 6, 36, 36);
        }
        _name = BattleInfoSkin.TextChild("Name", transform, font, 14);
        _name.textWrappingMode = TextWrappingModes.NoWrap;
        _name.overflowMode = TextOverflowModes.Ellipsis;
        SetBounds(_name.rectTransform, 52, 73, 5, 25);
        _count = BattleInfoSkin.TextChild("Count", transform, font, 13);
        BattleInfoSkin.Stretch(_count.rectTransform, 0, 48, 5, 23);
        _count.rectTransform.anchorMin = new Vector2(1, 0);
        _count.rectTransform.offsetMin = new Vector2(-73, 23);
        _count.alignment = TextAlignmentOptions.Midline;
        _stages = BattleInfoSkin.TextChild("Stages", transform, font, 10);
        SetBounds(_stages.rectTransform, 52, 9, 27, 43);
        _stages.richText = true;
        RectTransform badge = BattleInfoSkin.Child("GradeBadge", transform);
        if (skin != null) BattleInfoSkin.ImageChild("GradeBadge", transform, skin.gradeBadge);
        BattleInfoSkin.Stretch(badge, 0, 8, 6, 22);
        badge.anchorMin = new Vector2(1, 0);
        badge.offsetMin = new Vector2(-48, 22);
        _grade = BattleInfoSkin.TextChild("Grade", badge, font, 10);
        BattleInfoSkin.Stretch(_grade.rectTransform, 3, 3, 1, 1);
        _grade.alignment = TextAlignmentOptions.Midline;
    }

    public void Bind(BattleSynergyEntry entry)
    {
        Entry = entry;
        _name.text = entry.Name;
        _count.text = $"{entry.Count}";
        _grade.text = entry.IsActive ? $"{entry.Stage}단계" : "대기";
        Color color = entry.IsActive ? new Color32(236, 231, 215, 255) : new Color32(166, 173, 184, 255);
        _name.color = _count.color = _stages.color = _grade.color = color;
        _group.alpha = entry.IsActive ? 1f : 0.6f;
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
