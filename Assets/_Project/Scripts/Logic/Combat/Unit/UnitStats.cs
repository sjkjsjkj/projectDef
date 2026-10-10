using System.Collections.Generic;

/// <summary>
/// 공유 데이터를 변경하지 않고 레벨 성장과 전체 효과를 반영한 능력치를 관리합니다.
/// </summary>
public class UnitStats
{
    #region ─────────────────────────▶ 내부 변수 ◀─────────────────────────
    private UnitData _data;
    private int _level;
    private float _hpRatio = 1f;
    // 관리자는 합계가 바뀔 때 새 사전을 전달하며, 전달한 사전은 이후 변경하지 않습니다.
    private IReadOnlyDictionary<UnitStatType, UnitStatModifier> _globalModifiers;
    #endregion

    #region ─────────────────────────▶ 공개 멤버 ◀─────────────────────────
    public float MaxHp { get; private set; }
    public float CurrentHp { get; private set; }

    public float AttackPower { get; private set; }
    public float Defense { get; private set; }
    public float AttackRange { get; private set; }
    public float AttackSpd { get; private set; }
    public float GoldGain { get; private set; }

    public UnitStats(UnitData data, int level = 1)
    {
        ApplyLevel(data, level);
        CurrentHp = MaxHp;
    }

    /// <summary>동일한 스탯 객체를 갱신해 기존 AbilityContext의 참조를 유지합니다.</summary>
    internal void ApplyLevel(UnitData data, int level)
    {
        if (data == null)
            throw new System.ArgumentNullException(nameof(data));

        _data = data;
        _level = level;
        Recalculate();
    }

    internal void ApplyGlobalModifiers(IReadOnlyDictionary<UnitStatType, UnitStatModifier> modifiers)
    {
        _globalModifiers = modifiers;
        Recalculate();
    }
    #endregion

    #region ─────────────────────────▶ 내부 메서드 ◀─────────────────────────
    private void Recalculate()
    {
        // 최대 체력이 일시적으로 0이 되어도 마지막 체력 비율을 보존합니다.
        if (MaxHp > 0f) _hpRatio = CurrentHp / MaxHp;
        float growth = UnitLevelProgression.GetStatMultiplier(_data.Grade, _level);
        // 이미 보정된 현재 값에 재적용하지 않고, 항상 레벨별 기본값부터 계산합니다.
        MaxHp = Calculate(UnitStatType.MaxHp, _data.MaxHp * growth);
        CurrentHp = MaxHp * _hpRatio;
        AttackPower = Calculate(UnitStatType.AttackPower, _data.Attack * growth);
        Defense = Calculate(UnitStatType.Defense, _data.Defense * growth);
        AttackRange = Calculate(UnitStatType.AttackRange, _data.Range * growth);
        AttackSpd = Calculate(UnitStatType.AttackSpd, _data.AttackSpd * growth);
        // 획득 금화는 전투 능력치의 레벨 성장과 독립적으로 보정합니다.
        GoldGain = Calculate(UnitStatType.GoldGain, _data.GoldGain);
    }

    private float Calculate(UnitStatType stat, float baseValue)
    {
        return _globalModifiers != null && _globalModifiers.TryGetValue(stat, out UnitStatModifier modifier)
            ? modifier.Apply(baseValue) : baseValue;
    }
    #endregion
}
