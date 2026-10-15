using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "MetaProgressionSettings", menuName = "ScriptableObjects/Meta Progression/Settings")]
public class MetaProgressionSettings : ScriptableObject
{
    [Header("게임 종료 메타포인트")]
    [Min(0)] public int pointsPerCompletedWave = 10;
    [Min(0)] public int victoryBonus = 100;
    [Header("독립 강화 목록 / 임시 밸런스")]
    public List<MetaUpgradeDefinition> upgrades = CreateDefaults();

    public bool IsValid
    {
        get
        {
            if (pointsPerCompletedWave < 0 || victoryBonus < 0 || upgrades == null || upgrades.Count == 0) return false;
            var ids = new HashSet<string>(StringComparer.Ordinal);
            foreach (MetaUpgradeDefinition item in upgrades)
                if (item == null || !item.IsValid || !ids.Add(item.id)) return false;
            return true;
        }
    }

    public int RewardFor(int completedWaves, bool victory) => (int)Math.Min(int.MaxValue,
        (long)Math.Max(0, completedWaves) * Math.Max(0, pointsPerCompletedWave) + (victory ? Math.Max(0, victoryBonus) : 0));

    private static List<MetaUpgradeDefinition> CreateDefaults() => new List<MetaUpgradeDefinition>
    {
        Make("attack", "기물 공격력", "모든 기물의 공격력을 높입니다.", MetaUpgradeCategory.Military, MetaUpgradeEffect.AttackPower, 30, 10, 5, .05f),
        Make("attack_speed", "기물 공격속도", "기본 공격 간격을 줄입니다. 스킬 쿨타임에는 영향을 주지 않습니다.", MetaUpgradeCategory.Military, MetaUpgradeEffect.AttackSpeed, 20, 15, 10, .05f),
        Make("basic_range", "기본 공격 사거리", "기본 공격의 대상 탐색 범위를 넓힙니다.", MetaUpgradeCategory.Military, MetaUpgradeEffect.BasicRange, 10, 20, 15, .05f),
        Make("skill_cooldown", "스킬 쿨타임", "스킬을 사용한 뒤 다음 사용까지의 시간을 줄입니다.", MetaUpgradeCategory.Strategy, MetaUpgradeEffect.SkillCooldown, 15, 20, 10, .03f),
        Make("skill_range", "스킬 공격 범위", "스킬의 대상 탐색 범위를 넓힙니다.", MetaUpgradeCategory.Strategy, MetaUpgradeEffect.SkillRange, 10, 20, 15, .05f),
        Make("enemy_health", "적 체력 감소", "새로 출현하는 모든 적의 최대 체력을 줄입니다.", MetaUpgradeCategory.Defense, MetaUpgradeEffect.EnemyHealth, 20, 20, 10, .02f),
        Make("enemy_capacity", "최대 적 허용 수", "패배하기 전까지 허용되는 생존 적 수를 늘립니다.", MetaUpgradeCategory.Defense, MetaUpgradeEffect.EnemyCapacity, 20, 10, 5, 5),
        Make("gold_gain", "획득 골드", "적 처치와 이벤트의 골드 보상을 늘립니다. 시작 골드와 환불은 제외됩니다.", MetaUpgradeCategory.Supply, MetaUpgradeEffect.GoldGain, 20, 15, 10, .05f),
        Make("starting_gold", "시작 골드", "매 게임 시작 시 추가 골드를 지급합니다.", MetaUpgradeCategory.Supply, MetaUpgradeEffect.StartingGold, 20, 10, 5, 5),
        Make("starting_honor", "시작 명예", "매 게임 시작 시 명예 경험치를 추가합니다. 필요 경험치를 채우면 명예 레벨도 오릅니다.", MetaUpgradeCategory.Inn, MetaUpgradeEffect.StartingHonor, 20, 15, 10, 4),
        Make("starting_refresh", "시작 객잔 초기화권", "매 게임 시작 시 객잔 목록 초기화권을 추가 지급합니다.", MetaUpgradeCategory.Inn, MetaUpgradeEffect.StartingRefreshTickets, 10, 20, 20, 1),
        Make("starting_recruitment", "시작 5급 지명영입권", "매 게임 시작 시 5급 기물을 선택할 수 있는 영입권을 추가 지급합니다.", MetaUpgradeCategory.Inn, MetaUpgradeEffect.StartingRecruitmentTickets, 3, 100, 100, 1)
    };

    private static MetaUpgradeDefinition Make(string id, string title, string description, MetaUpgradeCategory category,
        MetaUpgradeEffect effect, int max, int cost, int step, float value) => new MetaUpgradeDefinition
        { id = id, title = title, description = description, category = category, effect = effect,
            maxLevel = max, baseCost = cost, costPerLevel = step, valuePerLevel = value };
}
