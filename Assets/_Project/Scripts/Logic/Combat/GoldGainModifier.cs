using System;

/// <summary>플레이어의 처치 골드 보정입니다. 배율 1은 변화 없음, 1.2는 20% 증가입니다.</summary>
public readonly struct GoldGainModifier
{
    public float Flat { get; }
    public float Multiplier { get; }

    public GoldGainModifier(float flat, float multiplier)
    {
        if (float.IsNaN(flat) || float.IsInfinity(flat))
            throw new ArgumentOutOfRangeException(nameof(flat));
        if (float.IsNaN(multiplier) || float.IsInfinity(multiplier) || multiplier < 0f)
            throw new ArgumentOutOfRangeException(nameof(multiplier));
        Flat = flat;
        Multiplier = multiplier;
    }
}
