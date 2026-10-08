/// <summary>
/// 유닛의 스탯을 관리할 기본클래스
/// </summary>
public class UnitStats
{
    #region ─────────────────────────▶ 내부 변수 ◀─────────────────────────

    #endregion

    #region ─────────────────────────▶ 공개 멤버 ◀─────────────────────────
    public float MaxHp { get; private set; }
    public float CurrentHp { get; private set; }

    public float AttackPower { get; private set; }
    public float AttackRange { get; private set; }
    public float AttackSpd { get; private set; }

    public UnitStats(UnitData data)
    {
        MaxHp = data.MaxHp;
        CurrentHp = MaxHp;

        AttackPower = data.Attack;
        AttackRange = data.Range;
        AttackSpd = data.AttackSpd;
    }
    #endregion

    #region ─────────────────────────▶ 내부 메서드 ◀─────────────────────────

    #endregion
}
