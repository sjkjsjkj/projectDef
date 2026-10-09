using System;
using UnityEngine;

/// <summary>한 전투에서 사용하는 재화와 명예를 관리합니다. UI와 별도 오브젝트에 배치합니다.</summary>
public class ResourceManager : BaseMono
{
    [SerializeField, Min(0)] private int gold;
    [SerializeField, Min(0)] private int innRefreshTickets;
    [SerializeField, Range(1, 10)] private int honorLevel = 1;

    public int Gold => Mathf.Max(0, gold);
    public int InnRefreshTickets => Mathf.Max(0, innRefreshTickets);
    public int HonorLevel => Mathf.Clamp(honorLevel, 1, 10);
    public event Action OnChanged;

    public void SetHonorLevel(int level)
    {
        int next = Mathf.Clamp(level, 1, 10);
        if (honorLevel == next) return;
        honorLevel = next;
        OnChanged?.Invoke();
    }

    public void AddGold(int amount)
    {
        if (amount <= 0) return;
        gold = (int)Math.Min(int.MaxValue, (long)Gold + amount);
        OnChanged?.Invoke();
    }

    /// <summary>웨이브 보상과 이벤트 보상이 공통으로 사용하는 지급 API입니다.</summary>
    public void AddInnRefreshTickets(int amount)
    {
        if (amount <= 0) return;
        innRefreshTickets = (int)Math.Min(int.MaxValue, (long)InnRefreshTickets + amount);
        OnChanged?.Invoke();
    }

    public bool TrySpendGold(int amount)
    {
        if (amount <= 0 || Gold < amount) return false;
        gold = Gold - amount;
        OnChanged?.Invoke();
        return true;
    }

    public bool TrySpendInnRefreshTicket()
    {
        if (InnRefreshTickets < 1) return false;
        innRefreshTickets = InnRefreshTickets - 1;
        OnChanged?.Invoke();
        return true;
    }
}
