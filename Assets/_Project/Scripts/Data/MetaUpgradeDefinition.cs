using System;
using UnityEngine;

public enum MetaUpgradeCategory { Military, Strategy, Defense, Supply, Inn }
public enum MetaUpgradeEffect
{
    AttackPower, AttackSpeed, BasicRange, SkillCooldown, SkillRange,
    EnemyHealth, EnemyCapacity, GoldGain, StartingGold, StartingHonor,
    StartingRefreshTickets, StartingRecruitmentTickets
}

/// <summary>ID는 저장 키이므로 출시 후 바꾸지 않습니다. 항목 사이에는 선행 조건이 없습니다.</summary>
[Serializable]
public class MetaUpgradeDefinition
{
    public string id;
    public string title;
    [TextArea] public string description;
    public MetaUpgradeCategory category;
    public MetaUpgradeEffect effect;
    [Min(1)] public int maxLevel = 20;
    [Min(1)] public int baseCost = 10;
    [Min(0)] public int costPerLevel = 5;
    [Tooltip("비율 효과는 0.05 = 5%. 나머지는 정수 수량입니다.")]
    [Min(0)] public float valuePerLevel = 0.05f;
    [Range(1, 5)] public int ticketGrade = 5;

    public bool IsPercentage => effect <= MetaUpgradeEffect.EnemyHealth || effect == MetaUpgradeEffect.GoldGain;
    public bool IsReduction => effect == MetaUpgradeEffect.SkillCooldown || effect == MetaUpgradeEffect.EnemyHealth;
    public bool IsValid => !string.IsNullOrWhiteSpace(id) && !string.IsNullOrWhiteSpace(title) &&
        Enum.IsDefined(typeof(MetaUpgradeCategory), category) && Enum.IsDefined(typeof(MetaUpgradeEffect), effect) &&
        maxLevel > 0 && baseCost > 0 && costPerLevel >= 0 && valuePerLevel > 0 &&
        !float.IsNaN(valuePerLevel) && !float.IsInfinity(valuePerLevel) &&
        (double)valuePerLevel * maxLevel <= int.MaxValue &&
        (IsPercentage || valuePerLevel == Mathf.Floor(valuePerLevel)) &&
        (!IsReduction || (double)valuePerLevel * maxLevel <= 0.95 + 0.000001) &&
        ticketGrade >= 1 && ticketGrade <= 5;

    public int CostAt(int level) => (int)Math.Min(int.MaxValue,
        (long)baseCost + (long)costPerLevel * Math.Max(0, level));
    public float ValueAt(int level) => valuePerLevel * Mathf.Clamp(level, 0, maxLevel);
    public string FormatValue(int level)
    {
        float value = ValueAt(level);
        return IsPercentage ? $"{(IsReduction ? "-" : "+")}{value * 100f:0.#}%" : $"+{value:0}";
    }

    public static string CategoryName(MetaUpgradeCategory category) => category switch
    {
        MetaUpgradeCategory.Military => "군사 훈련",
        MetaUpgradeCategory.Strategy => "병법 연구",
        MetaUpgradeCategory.Defense => "방어 지원",
        MetaUpgradeCategory.Supply => "군수 지원",
        _ => "객잔 지원"
    };
}
