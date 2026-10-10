using System;

/// <summary>스탯 하나의 고정값과 배율입니다. 배율 1은 변화 없음, 1.2는 20% 증가입니다.</summary>
public readonly struct UnitStatModifier
{
    public UnitStatType Stat { get; }
    public float Flat { get; }
    public float Multiplier { get; }

    public UnitStatModifier(UnitStatType stat, float flat = 0f, float multiplier = 1f)
    {
        if (!Enum.IsDefined(typeof(UnitStatType), stat))
            throw new ArgumentOutOfRangeException(nameof(stat));
        if (float.IsNaN(flat) || float.IsInfinity(flat))
            throw new ArgumentOutOfRangeException(nameof(flat));
        if (float.IsNaN(multiplier) || float.IsInfinity(multiplier) || multiplier < 0f)
            throw new ArgumentOutOfRangeException(nameof(multiplier));

        Stat = stat;
        Flat = flat;
        Multiplier = multiplier;
    }

    /// <summary>최종 스탯은 음수가 되지 않도록 제한합니다.</summary>
    public float Apply(float baseValue) => Math.Max(0f, baseValue * Multiplier + Flat);
}
