using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>한 전투에서 사용하는 재화와 명예를 관리합니다. UI와 별도 오브젝트에 배치합니다.</summary>
public class ResourceManager : BaseMono
{
    [SerializeField, Min(0)] private int gold;
    [SerializeField, Min(0)] private int innRefreshTickets;
    [Tooltip("인덱스 0부터 1~5등급 유닛 선택권의 보유 수량입니다.")]
    [SerializeField] private int[] unitSelectionTickets = new int[5];
    [SerializeField, Range(1, 10)] private int honorLevel = 1;
    [SerializeField, Min(0)] private int honorExperience;
    [SerializeField] private HonorProgression honorProgression = new HonorProgression();

    private readonly Dictionary<string, GoldGainModifier> _goldGainEffects =
        new Dictionary<string, GoldGainModifier>(StringComparer.Ordinal);
    private GoldGainModifier _goldGainModifier = new GoldGainModifier(0f, 1f);

    public int Gold => Mathf.Max(0, gold);
    public int InnRefreshTickets => Mathf.Max(0, innRefreshTickets);
    public float GoldGainMultiplier => _goldGainModifier.Multiplier;
    public float GoldGainFlat => _goldGainModifier.Flat;
    public int HonorLevel => Mathf.Clamp(honorLevel, 1, 10);
    public int HonorExperience => Mathf.Max(0, honorExperience);
    public int RequiredHonorExperience => honorProgression?.RequiredExperience(HonorLevel) ?? 0;
    public int HonorPurchaseCost => honorProgression?.PurchaseCost ?? 0;
    public int HonorPerPurchase => honorProgression?.ExperiencePerPurchase ?? 0;
    public bool IsHonorMax => HonorLevel == HonorProgression.MaxLevel;
    public bool CanPurchaseHonor => honorProgression != null && honorProgression.IsValid && !IsHonorMax && Gold >= HonorPurchaseCost;
    public event Action OnChanged;

    /// <summary>
    /// 적 한 마리의 보상 묶음을 지급합니다. 골드 합계에 사망 시점의 보정을 한 번 적용하고
    /// 소수점은 버립니다. 아이템 수량과 AddGold를 통한 환불에는 보정을 적용하지 않습니다.
    /// </summary>
    public void GrountEnemyBounty(List<Bounty> bounties)
    {
        if (bounties == null || bounties.Count == 0) return;

        // 설정 오류로 보상 일부만 지급되지 않도록 전체 목록을 먼저 검사합니다.
        foreach (Bounty bounty in bounties)
        {
            if (bounty == null || !bounty.IsValid ||
                !(bounty is GoldBounty || bounty is ItemBounty))
            {
                Debug.LogError("유효하지 않거나 지원하지 않는 적 보상이 있습니다.", this);
                return;
            }
        }

        long baseGold = 0;
        bool changed = false;
        foreach (Bounty bounty in bounties)
        {
            if (bounty is GoldBounty)
            {
                baseGold += bounty.Amount;
            }
            else if (bounty is ItemBounty item)
            {
                if (item.ItemType == BountyItemType.InnRefreshTicket)
                    innRefreshTickets = AddClamped(InnRefreshTickets, item.Amount);
                else
                {
                    EnsureSelectionTicketStorage();
                    int index = item.UnitGrade - 1;
                    unitSelectionTickets[index] = AddClamped(GetUnitSelectionTickets(item.UnitGrade), item.Amount);
                }
                changed = true;
            }
        }

        if (baseGold > 0)
        {
            int amount = CalculateGoldBounty(baseGold);
            gold = AddClamped(Gold, amount);
            changed |= amount > 0;
        }
        // 골드와 모든 아이템이 반영된 상태를 한 번만 알립니다.
        if (changed) OnChanged?.Invoke();
    }

    private int CalculateGoldBounty(long baseGold)
    {
        // float의 표현 오차 때문에 10 * 1.4가 13골드로 버려지지 않도록 십진수로 계산합니다.
        try
        {
            decimal adjusted = baseGold * (decimal)GoldGainMultiplier + (decimal)GoldGainFlat;
            return (int)Math.Min(int.MaxValue, Math.Max(0m, decimal.Floor(adjusted)));
        }
        catch (OverflowException)
        {
            // decimal 범위보다 큰 보정도 기존 골드와 같은 int 상한으로 제한합니다.
            double adjusted = baseGold * (double)GoldGainMultiplier + GoldGainFlat;
            return (int)Math.Min(int.MaxValue, Math.Max(0d, Math.Floor(adjusted)));
        }
    }

    /// <summary>동일 출처는 교체합니다. 다른 출처의 고정값은 합산, 배율은 곱산합니다.</summary>
    public void SetGoldGainEffect(string sourceId, float flat = 0f, float multiplier = 1f)
    {
        if (string.IsNullOrWhiteSpace(sourceId))
            throw new ArgumentException("효과의 출처 ID가 필요합니다.", nameof(sourceId));
        var modifier = new GoldGainModifier(flat, multiplier);
        GoldGainModifier combined = CombineGoldGainEffects(sourceId, modifier);
        _goldGainEffects[sourceId] = modifier;
        _goldGainModifier = combined;
        OnChanged?.Invoke();
    }

    public bool RemoveGoldGainEffect(string sourceId)
    {
        if (sourceId == null || !_goldGainEffects.ContainsKey(sourceId)) return false;
        GoldGainModifier combined = CombineGoldGainEffects(sourceId, new GoldGainModifier(0f, 1f));
        _goldGainEffects.Remove(sourceId);
        _goldGainModifier = combined;
        OnChanged?.Invoke();
        return true;
    }

    public void ClearGoldGainEffects()
    {
        _goldGainEffects.Clear();
        _goldGainModifier = new GoldGainModifier(0f, 1f);
        OnChanged?.Invoke();
    }

    private GoldGainModifier CombineGoldGainEffects(string replacedId, GoldGainModifier replacement)
    {
        float flat = replacement.Flat;
        float multiplier = replacement.Multiplier;
        foreach (KeyValuePair<string, GoldGainModifier> effect in _goldGainEffects)
        {
            if (effect.Key == replacedId) continue;
            flat += effect.Value.Flat;
            multiplier *= effect.Value.Multiplier;
        }
        return new GoldGainModifier(flat, multiplier);
    }

    public int GetUnitSelectionTickets(int grade)
    {
        if (grade < 1 || grade > 5 || unitSelectionTickets == null || grade > unitSelectionTickets.Length)
            return 0;
        return Mathf.Max(0, unitSelectionTickets[grade - 1]);
    }

    private void EnsureSelectionTicketStorage()
    {
        if (unitSelectionTickets == null || unitSelectionTickets.Length != 5)
            Array.Resize(ref unitSelectionTickets, 5);
    }

    private static int AddClamped(int current, int amount) =>
        (int)Math.Min(int.MaxValue, (long)current + amount);

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
