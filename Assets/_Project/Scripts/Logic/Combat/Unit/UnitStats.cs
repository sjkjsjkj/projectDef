/// <summary>
/// 공유 데이터를 변경하지 않고 유닛별 현재 레벨의 능력치를 관리합니다.
/// </summary>
public class UnitStats
{
    #region ─────────────────────────▶ 내부 변수 ◀─────────────────────────

    #endregion

    #region ─────────────────────────▶ 공개 멤버 ◀─────────────────────────
    public float MaxHp { get; private set; }
    public float CurrentHp { get; private set; }

    public float AttackPower { get; private set; }
    public float Defense { get; private set; }
    public float AttackRange { get; private set; }
    public float AttackSpd { get; private set; }

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

        float hpRatio = MaxHp > 0f ? CurrentHp / MaxHp : 1f;
        float multiplier = UnitLevelProgression.GetStatMultiplier(data.Grade, level);
        MaxHp = data.MaxHp * multiplier;
        CurrentHp = MaxHp * hpRatio;
        AttackPower = data.Attack * multiplier;
        Defense = data.Defense * multiplier;
        AttackRange = data.Range * multiplier;
        AttackSpd = data.AttackSpd * multiplier;
    }
    #endregion

    #region ─────────────────────────▶ 내부 메서드 ◀─────────────────────────

    #endregion
}
