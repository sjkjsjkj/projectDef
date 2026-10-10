using System;
using UnityEngine;

/// <summary>한 전투에서 사용하는 재화와 명예를 관리합니다. UI와 별도 오브젝트에 배치합니다.</summary>
public class ResourceManager : BaseMono
{
    [SerializeField, Min(0)] private int gold;
    [SerializeField, Min(0)] private int innRefreshTickets;
    [SerializeField, Range(1, 10)] private int honorLevel = 1;
    [SerializeField, Min(0)] private int honorExperience;
    [SerializeField] private HonorProgression honorProgression = new HonorProgression();

    public int Gold => Mathf.Max(0, gold);
    public int InnRefreshTickets => Mathf.Max(0, innRefreshTickets);
    public int HonorLevel => Mathf.Clamp(honorLevel, 1, 10);
    public int HonorExperience => Mathf.Max(0, honorExperience);
    public int RequiredHonorExperience => honorProgression?.RequiredExperience(HonorLevel) ?? 0;
    public int HonorPurchaseCost => honorProgression?.PurchaseCost ?? 0;
    public int HonorPerPurchase => honorProgression?.ExperiencePerPurchase ?? 0;
    public bool IsHonorMax => HonorLevel == HonorProgression.MaxLevel;
    public bool CanPurchaseHonor => honorProgression != null && honorProgression.IsValid && !IsHonorMax && Gold >= HonorPurchaseCost;
    public event Action OnChanged;

    public void SetHonorLevel(int level)
    {
        int next = Mathf.Clamp(level, 1, 10);
        if (honorLevel == next) return;
        honorLevel = next;
        honorExperience = 0;
        OnChanged?.Invoke();
    }

    public bool TryPurchaseHonor(out string reason)
    {
        reason = null;
        if (honorProgression == null || !honorProgression.IsValid)
        { reason = "명예 성장 설정을 확인하세요."; return false; }
        if (IsHonorMax) { reason = "명예가 최고 레벨입니다."; return false; }
        if (Gold < HonorPurchaseCost) { reason = "골드가 부족합니다."; return false; }
        // 골드와 경험치를 함께 갱신한 후 한 번만 통지합니다.
        gold = Gold - HonorPurchaseCost;
        honorProgression.AddExperience(ref honorLevel, ref honorExperience, HonorPerPurchase);
        OnChanged?.Invoke();
        return true;
    }

    public void AddHonorExperience(int amount)
    {
        if (amount <= 0 || IsHonorMax || honorProgression == null || !honorProgression.IsValid) return;
        honorProgression.AddExperience(ref honorLevel, ref honorExperience, amount);
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
