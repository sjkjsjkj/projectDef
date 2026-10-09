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

    private InnPanel _panel;
    private int _index;

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
            portrait.sprite = data != null ? data.Image : null;
            portrait.enabled = portrait.sprite != null;
        }
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
