using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 시전자 정보와 이번 실행의 타깃을 전달하는 런타임 문맥입니다.
/// 기본 공격과 스킬은 각각 별도의 문맥을 사용합니다.
/// </summary>
public class AbilityContext
{
    #region ─────────────────────────▶ 내부 변수 ◀─────────────────────────

    #endregion

    #region ─────────────────────────▶ 공개 멤버 ◀─────────────────────────
    public Unit Owner { get; }
    public UnitStats OwnerStats { get; }
    public Transform Origin { get; }
    public IReadOnlyList<IDamageable> Targets { get; }
    public bool IsBasicAttack { get; }
    public Vector3 AimPosition { get; internal set; }
    private readonly float? _attackPowerAtLaunch;
    public float AttackPower => _attackPowerAtLaunch ?? OwnerStats.AttackPower;

    // 타깃 선택 결과는 문맥에 보관하며 공유 SO에는 저장하지 않습니다.
    internal List<IDamageable> TargetBuffer { get; } = new();

    public AbilityContext(Unit unit, bool isBasicAttack = false)
    {
        if (unit == null)
            throw new System.ArgumentNullException(nameof(unit));

        Owner = unit;
        IsBasicAttack = isBasicAttack;
        OwnerStats = unit.Stat;
        Origin = unit.Origin;
        Targets = TargetBuffer.AsReadOnly();
    }

    private AbilityContext(AbilityContext source, IDamageable target)
    {
        Owner = source.Owner;
        OwnerStats = source.OwnerStats;
        Origin = source.Origin;
        IsBasicAttack = source.IsBasicAttack;
        AimPosition = source.AimPosition;
        _attackPowerAtLaunch = source.AttackPower;
        TargetBuffer.Add(target);
        Targets = TargetBuffer.AsReadOnly();
    }

    internal AbilityContext ForTarget(IDamageable target) => new AbilityContext(this, target);
    #endregion

    #region ─────────────────────────▶ 내부 메서드 ◀─────────────────────────

    #endregion
}
