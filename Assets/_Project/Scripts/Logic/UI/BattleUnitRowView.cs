using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>초상화 자리 / 이름 / 레벨을 독립적으로 표시하는 출전 장수 행입니다.</summary>
public class BattleUnitRowView : MonoBehaviour
{
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text levelText;

    public void Initialize(TMP_FontAsset font)
    {
        BattleInfoSkin skin = BattleInfoSkin.Load();
        Image background = GetComponent<Image>();
        if (background == null) background = gameObject.AddComponent<Image>();
        background.color = new Color32(42, 48, 60, 255);
        if (skin != null) BattleInfoSkin.SetImage(background, skin.unitRow);
        LayoutElement layout = GetComponent<LayoutElement>();
        if (layout == null) layout = gameObject.AddComponent<LayoutElement>();
        layout.minHeight = layout.preferredHeight = 48;
        if (skin != null)
        {
            Image frame = BattleInfoSkin.ImageChild("PortraitFrame", transform, skin.emptyFrame);
            BattleInfoSkin.Place(frame.rectTransform, 8, 6, 36, 36);
        }
        nameText = BattleInfoSkin.TextChild("Name", transform, font, 15);
        BattleInfoSkin.Stretch(nameText.rectTransform, 52, 57, 6, 6);
        levelText = BattleInfoSkin.TextChild("Level", transform, font, 13);
        BattleInfoSkin.Stretch(levelText.rectTransform, 0, 9, 6, 6);
        levelText.rectTransform.anchorMin = new Vector2(1, 0);
        levelText.rectTransform.offsetMin = new Vector2(-55, 6);
        levelText.alignment = TextAlignmentOptions.MidlineRight;
    }

    public void Bind(Unit unit)
    {
        nameText.text = unit.Data != null && !string.IsNullOrWhiteSpace(unit.Data.UnitName)
            ? unit.Data.UnitName : unit.name;
        levelText.text = $"Lv.{unit.Level}";
    }
}
