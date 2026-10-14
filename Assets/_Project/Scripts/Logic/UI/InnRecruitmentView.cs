using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>등급별 영입권 버튼과 이름으로 선택하는 지명영입 목록입니다.</summary>
public class InnRecruitmentView : MonoBehaviour
{
    [SerializeField] private InnPanel owner;
    [SerializeField] private Button[] ticketButtons;
    [SerializeField] private Image[] ticketIcons;
    [SerializeField] private TMP_Text[] ticketCounts;
    [SerializeField] private GameObject overlay;
    [SerializeField] private Button dismissButton;
    [SerializeField] private Button closeButton;
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text messageText;
    [SerializeField] private ScrollRect scroll;

    private readonly List<Button> _candidateButtons = new List<Button>();
    private int _grade;
    private bool _bound;
    private InnManager Manager => owner != null ? owner.Manager : null;

    public void Configure(InnPanel panel, Button[] buttons, Image[] icons, TMP_Text[] counts,
        GameObject popup, Button dismiss, Button close, TMP_Text title, TMP_Text message, ScrollRect list)
    {
        owner = panel;
        ticketButtons = buttons;
        ticketIcons = icons;
        ticketCounts = counts;
        overlay = popup;
        dismissButton = dismiss;
        closeButton = close;
        titleText = title;
        messageText = message;
        scroll = list;
    }

    public void Bind()
    {
        if (_bound) return;
        _bound = true;
        for (int i = 0; i < ticketButtons.Length; i++)
        {
            int grade = i + 1;
            ticketButtons[i].onClick.AddListener(() => Open(grade));
        }
        dismissButton.onClick.AddListener(Close);
        closeButton.onClick.AddListener(Close);
    }

    public void Refresh()
    {
        ResourceManager resources = Manager != null ? Manager.Resources : null;
        for (int i = 0; i < ticketButtons.Length; i++)
        {
            int count = resources != null ? resources.GetUnitSelectionTickets(i + 1) : 0;
            ticketButtons[i].interactable = count > 0;
            ticketIcons[i].color = count > 0 ? InnOfferView.GradeColor(i + 1) : new Color32(65, 68, 74, 255);
            ticketCounts[i].text = $"{count}장";
            ticketCounts[i].color = count > 0 ? new Color32(236, 231, 215, 255) : new Color32(112, 114, 120, 255);
        }
        bool canRecruit = resources != null && resources.GetUnitSelectionTickets(_grade) > 0;
        foreach (Button button in _candidateButtons)
            button.interactable = canRecruit;
    }

    private void Open(int grade)
    {
        if (Manager == null || Manager.Resources == null || Manager.Resources.GetUnitSelectionTickets(grade) < 1) return;
        _grade = grade;
        foreach (Button button in _candidateButtons)
        {
            button.gameObject.SetActive(false);
            Destroy(button.gameObject);
        }
        _candidateButtons.Clear();
        List<UnitData> candidates = Manager.GetRecruitmentCandidates(grade);
        TMP_FontAsset font = InnUIFactory.ResolveFont(Manager.UIFont);
        for (int i = 0; i < candidates.Count; i++)
        {
            UnitData data = candidates[i];
            Button button = InnUIFactory.MakeButton($"Recruit_{data.Id}", scroll.content,
                10 + i % 5 * 142, -78 - i / 5 * 78, 132, 68, data.UnitName, font);
            var rect = (RectTransform)button.transform;
            rect.anchorMin = rect.anchorMax = new Vector2(0, 1);
            TMP_Text label = button.GetComponentInChildren<TMP_Text>();
            label.enableAutoSizing = true;
            label.fontSizeMin = 12;
            label.fontSizeMax = 22;
            button.onClick.AddListener(() => Recruit(data));
            _candidateButtons.Add(button);
        }
        scroll.content.sizeDelta = new Vector2(720, Mathf.Max(306, ((candidates.Count + 4) / 5) * 78 + 10));
        scroll.StopMovement();
        scroll.content.anchoredPosition = Vector2.zero;
        titleText.text = $"{grade}급 지명영입 · 장수 선택";
        messageText.text = candidates.Count == 0 ? "이 등급에 등록된 장수가 없습니다." : "장수를 선택하면 지명영입권 1장을 사용합니다. · 스크롤로 목록 보기";
        overlay.transform.SetAsLastSibling();
        overlay.SetActive(true);
        Refresh();
    }

    private void Recruit(UnitData data)
    {
        if (Manager == null) return;
        bool success = Manager.TryRecruit(_grade, data, out string reason);
        if (success)
        {
            owner.ShowRecruitmentMessage($"{data.UnitName} 지명영입 완료! 보유 장수는 레벨이 상승합니다.");
            Close();
        }
        else messageText.text = reason;
        Refresh();
    }

    public void Close()
    {
        if (overlay != null) overlay.SetActive(false);
    }

    private void OnDisable() => Close();
}
