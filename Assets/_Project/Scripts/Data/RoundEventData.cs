using System.Collections.Generic;
using UnityEngine;

/// <summary>라운드 종료 후 선택 이벤트의 정의입니다. 무료 횟수와 비용은 실행마다 별도로 관리합니다.</summary>
[CreateAssetMenu(fileName = "RoundEvent_", menuName = "ScriptableObjects/Round Event/Event")]
public class RoundEventData : BaseSO
{
    [SerializeField] private string title = "새로운 기회";
    [SerializeField, TextArea] private string description = "이번 전투를 이끌 힘을 선택하세요.";
    [Tooltip("전투에 참여하지 않는 UI 연출용 몬스터 이미지입니다.")]
    [SerializeField] private Sprite monsterPortrait;
    [SerializeField, Range(3, 4)] private int choiceCount = 3;
    [SerializeField, Min(1)] private int initialRefreshCost = 20;
    [SerializeField, Min(1)] private int refreshCostIncrease = 10;
    [SerializeField] private List<RoundEventOptionData> options = new List<RoundEventOptionData>();

    public string Title => title;
    public string Description => description;
    public Sprite MonsterPortrait => monsterPortrait;
    public int ChoiceCount => choiceCount;
    public int InitialRefreshCost => initialRefreshCost;
    public int RefreshCostIncrease => refreshCostIncrease;
    public IReadOnlyList<RoundEventOptionData> Options => options;

    public override bool IsValid()
    {
        if (!base.IsValid() || choiceCount < 3 || choiceCount > 4 || initialRefreshCost < 1 ||
            refreshCostIncrease < 1 || options == null) return false;
        var ids = new HashSet<string>();
        foreach (RoundEventOptionData option in options)
            if (option == null || !option.IsValid() || !ids.Add(option.Id)) return false;
        // 현재 카드와 다른 결과로 새로고침할 수 있도록 여분 후보가 필요합니다.
        return ids.Count > choiceCount;
    }
}
