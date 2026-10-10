using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>객잔의 판매 칸 하나를 표시합니다.</summary>
public class InnOfferView : UI
{
    [SerializeField] private Image portrait;
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text gradeText;
    [SerializeField] private TMP_Text priceText;
    [SerializeField] private Button purchaseButton;
    [SerializeField] private Image gradeBorder;

    private InnPanel _panel;
    private int _index;

    public void Configure(TMP_Text name, TMP_Text grade, TMP_Text price, Button button, Image border)
    {
        nameText = name;
        gradeText = grade;
        priceText = price;
        purchaseButton = button;
        gradeBorder = border;
    }

    public static Color GradeColor(int grade) => grade switch
    {
        2 => new Color32(65, 145, 255, 255),
        3 => new Color32(174, 95, 235, 255),
        4 => new Color32(255, 210, 64, 255),
        5 => new Color32(239, 68, 68, 255),
        _ => new Color32(150, 156, 165, 255)
    };

    public void Bind(InnPanel panel, int index)
    {
        if (purchaseButton != null) purchaseButton.onClick.RemoveListener(Purchase);
        _panel = panel;
        _index = index;
        if (purchaseButton != null) purchaseButton.onClick.AddListener(Purchase);
    }

    public void Refresh(UnitData data, bool sold, int gold)
    {
        if (portrait != null)
        {
            // 현재 UI는 초상화 대신 이름을 사용합니다.
            portrait.enabled = false;
        }
        if (gradeBorder != null) gradeBorder.color = GradeColor(data != null ? data.Grade : 1);
        if (nameText != null) nameText.text = data != null ? data.UnitName : "준비 중";
        if (gradeText != null) gradeText.text = data != null ? $"{data.Grade}등급" : "";
        if (priceText != null) priceText.text = sold ? "품절" : data != null ? $"{data.Price}원 · 고용" : "";
        if (purchaseButton != null) purchaseButton.interactable = data != null && !sold && gold >= data.Price;
    }

    private void Purchase()
    {
        if (_panel != null) _panel.Purchase(_index);
    }

    private void OnDestroy()
    {
        if (purchaseButton != null) purchaseButton.onClick.RemoveListener(Purchase);
    }
}
