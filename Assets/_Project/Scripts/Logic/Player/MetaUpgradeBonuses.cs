using System;

/// <summary>저장한 레벨에서 계산한 효과. 공유 UnitData/AbilityData/EnemyData는 수정하지 않습니다.</summary>
public sealed class MetaUpgradeBonuses
{
    public static readonly MetaUpgradeBonuses None = new MetaUpgradeBonuses();
    public float AttackPower { get; private set; } = 1f;
    public float AttackSpeed { get; private set; } = 1f;
    public float BasicRange { get; private set; } = 1f;
    public float SkillRange { get; private set; } = 1f;
    public float SkillCooldown { get; private set; } = 1f;
    public float EnemyHealth { get; private set; } = 1f;
    public float GoldGain { get; private set; } = 1f;
    public int EnemyCapacity { get; private set; }
    public int StartingGold { get; private set; }
    public int StartingHonor { get; private set; }
    public int StartingRefreshTickets { get; private set; }
    private readonly int[] _tickets = new int[5];
    public int RecruitmentTickets(int grade) => grade >= 1 && grade <= 5 ? _tickets[grade - 1] : 0;

    public static MetaUpgradeBonuses From(MetaProgression progression)
    {
        var result = new MetaUpgradeBonuses();
        foreach (MetaUpgradeDefinition item in progression.Settings.upgrades)
        {
            float value = item.ValueAt(progression.Level(item.id));
            int count = (int)Math.Min(int.MaxValue, Math.Round((double)value));
            switch (item.effect)
            {
                case MetaUpgradeEffect.AttackPower: result.AttackPower += value; break;
                case MetaUpgradeEffect.AttackSpeed: result.AttackSpeed += value; break;
                case MetaUpgradeEffect.BasicRange: result.BasicRange += value; break;
                case MetaUpgradeEffect.SkillRange: result.SkillRange += value; break;
                case MetaUpgradeEffect.SkillCooldown: result.SkillCooldown -= value; break;
                case MetaUpgradeEffect.EnemyHealth: result.EnemyHealth -= value; break;
                case MetaUpgradeEffect.GoldGain: result.GoldGain += value; break;
                case MetaUpgradeEffect.EnemyCapacity: result.EnemyCapacity = Add(result.EnemyCapacity, count); break;
                case MetaUpgradeEffect.StartingGold: result.StartingGold = Add(result.StartingGold, count); break;
                case MetaUpgradeEffect.StartingHonor: result.StartingHonor = Add(result.StartingHonor, count); break;
                case MetaUpgradeEffect.StartingRefreshTickets: result.StartingRefreshTickets = Add(result.StartingRefreshTickets, count); break;
                case MetaUpgradeEffect.StartingRecruitmentTickets:
                    result._tickets[item.ticketGrade - 1] = Add(result._tickets[item.ticketGrade - 1], count); break;
            }
        }
        result.SkillCooldown = Math.Max(.05f, result.SkillCooldown);
        result.EnemyHealth = Math.Max(.05f, result.EnemyHealth);
        return result;
    }

    private static int Add(int a, int b) => (int)Math.Min(int.MaxValue, (long)a + b);
}
