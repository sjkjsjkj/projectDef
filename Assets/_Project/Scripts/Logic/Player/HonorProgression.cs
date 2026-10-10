using System;
using UnityEngine;

/// <summary>명예 구매 및 레벨업 규칙. 수치는 임시 밸런스이며 인스펙터에서 조정합니다.</summary>
[Serializable]
public class HonorProgression
{
    public const int MaxLevel = 10;
    [SerializeField, Min(1)] private int purchaseCost = 4;
    [SerializeField, Min(1)] private int experiencePerPurchase = 4;
    [Tooltip("1→2부터 9→10까지 필요한 경험치 9개입니다.")]
    [SerializeField] private int[] requiredExperience = { 4, 8, 12, 20, 32, 48, 64, 80, 100 };

    public int PurchaseCost => purchaseCost;
    public int ExperiencePerPurchase => experiencePerPurchase;
    public bool IsValid
    {
        get
        {
            if (purchaseCost <= 0 || experiencePerPurchase <= 0 || requiredExperience == null || requiredExperience.Length != 9)
                return false;
            foreach (int amount in requiredExperience) if (amount <= 0) return false;
            return true;
        }
    }

    public int RequiredExperience(int level) => IsValid && level >= 1 && level < MaxLevel ? requiredExperience[level - 1] : 0;

    public void AddExperience(ref int level, ref int experience, int amount)
    {
        if (!IsValid || amount <= 0) return;
        level = Math.Max(1, Math.Min(MaxLevel, level));
        long total = (long)Math.Max(0, experience) + amount;
        while (level < MaxLevel && total >= requiredExperience[level - 1])
        {
            total -= requiredExperience[level - 1];
            level++;
        }
        experience = level == MaxLevel ? 0 : (int)total;
    }
}
