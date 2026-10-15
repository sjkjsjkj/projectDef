using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

/// <summary>이벤트 중에만 활성화되는 모달 UI입니다. 닫기 대신 선택 성공으로만 진행합니다.</summary>
public class RoundEventPanel : BaseMono
{
    private RoundEventManager _manager;
    private TMP_Text _title;
    private TMP_Text _description;
    private TMP_Text _gold;
    private TMP_Text _message;
    private Image _monster;
    private RoundEventCard[] _cards;
    private float _appearTime;
    private GameObject _previousSelection;

    public void Configure(RoundEventManager manager, TMP_Text title, TMP_Text description,
        TMP_Text gold, TMP_Text message, Image monster, RoundEventCard[] cards)
    {
        _manager = manager;
        _title = title;
        _description = description;
        _gold = gold;
        _message = message;
        _monster = monster;
        _cards = cards;
        for (int i = 0; i < cards.Length; i++)
        {
            int index = i;
            cards[i].Select.onClick.AddListener(() => Select(index));
            cards[i].Refresh.onClick.AddListener(() => Refresh(index));
        }
    }

    private void OnEnable()
    {
        if (_manager == null) return;
        _manager.OnChanged += RefreshView;
        if (_manager.Resources != null) _manager.Resources.OnChanged += RefreshView;
        _appearTime = 0f;
        _message.text = "원하는 효과 하나를 선택하면 다음 라운드로 진행합니다.";
        RefreshView();
        if (EventSystem.current != null)
        {
            _previousSelection = EventSystem.current.currentSelectedGameObject;
            EventSystem.current.SetSelectedGameObject(_cards[0].Select.gameObject);
        }
    }

    private void OnDisable()
    {
        if (_manager == null) return;
        _manager.OnChanged -= RefreshView;
        if (_manager.Resources != null) _manager.Resources.OnChanged -= RefreshView;
        if (EventSystem.current != null)
            EventSystem.current.SetSelectedGameObject(_previousSelection != null && _previousSelection.activeInHierarchy
                ? _previousSelection : null);
    }

    private void Update()
    {
        // 전투 일시정지와 독립적으로 등장 연출을 재생합니다.
        _appearTime += Time.unscaledDeltaTime;
        float progress = Mathf.Clamp01(_appearTime / 0.3f);
        _monster.rectTransform.localScale = Vector3.one * Mathf.Lerp(0.8f, 1f, progress);
        _monster.color = new Color(1f, 1f, 1f, progress);
    }

    private void Select(int index)
    {
        if (!_manager.TrySelect(index, out string reason)) _message.text = reason;
        RefreshView();
    }

    private void Refresh(int index)
    {
        bool result = _manager.TryRefresh(index, out string reason);
        _message.text = result ? "선택지가 변경되었습니다." : reason;
        RefreshView();
    }

    private void RefreshView()
    {
        RoundEventSession session = _manager.Session;
        if (session == null) return;
        _title.text = session.Data.Title;
        _description.text = session.Data.Description;
        _gold.text = $"보유 골드 {_manager.Resources.Gold}   ·   공통 유료 새로고침 {session.PaidRefreshCost}골드";
        _monster.sprite = session.Data.MonsterPortrait;
        _monster.enabled = _monster.sprite != null;
        for (int i = 0; i < _cards.Length; i++)
        {
            RoundEventCard card = _cards[i];
            card.Root.SetActive(i < session.ChoiceCount);
            if (i >= session.ChoiceCount) continue;
            RoundEventOptionData option = session.GetOffer(i);
            card.Title.text = option.Title;
            card.Description.text = option.Description;
            card.Icon.sprite = option.Icon;
            card.Icon.enabled = option.Icon != null;
            card.Select.interactable = session.IsActive;
            card.Refresh.interactable = session.CanRefresh(i);
            card.RefreshLabel.text = session.HasFreeRefresh(i) ? "무료 새로고침 · 1회" : $"새로고침 · {session.PaidRefreshCost}골드";
            // 키보드/패드 탐색도 모달 내부로 제한합니다.
            int previous = (i + session.ChoiceCount - 1) % session.ChoiceCount;
            int next = (i + 1) % session.ChoiceCount;
            card.Select.navigation = new Navigation { mode = Navigation.Mode.Explicit,
                selectOnLeft = _cards[previous].Select, selectOnRight = _cards[next].Select,
                selectOnDown = card.Refresh };
            card.Refresh.navigation = new Navigation { mode = Navigation.Mode.Explicit,
                selectOnLeft = _cards[previous].Refresh, selectOnRight = _cards[next].Refresh,
                selectOnUp = card.Select };
        }
    }
}

public sealed class RoundEventCard
{
    public GameObject Root;
    public TMP_Text Title;
    public TMP_Text Description;
    public Image Icon;
    public Button Select;
    public Button Refresh;
    public TMP_Text RefreshLabel;
}
