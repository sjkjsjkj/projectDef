using System;

/// <summary>유닛 레벨 상한, 스킬 단계 및 등급별 성장률을 정의합니다.</summary>
public static class UnitLevelProgression
{
    public const int MaxLevel = 10;

    // 밸런스 확정 시 등급별 값을 각각 변경합니다. 0.1은 기본 능력치의 10%입니다.
    public static float GetGrowthRate(int grade)
    {
        return grade switch
        {
            1 => 0.1f,
            2 => 0.1f,
            3 => 0.1f,
            4 => 0.1f,
            5 => 0.1f,
            _ => throw new ArgumentOutOfRangeException(nameof(grade))
        };
    }

    public static float GetStatMultiplier(int grade, int level)
    {
        int clampedLevel = Math.Max(1, Math.Min(MaxLevel, level));
        return 1f + (clampedLevel - 1) * GetGrowthRate(grade);
    }

    public static int GetSkillLevel(int level) => level >= 7 ? 3 : level >= 4 ? 2 : 1;
}
