using System.Collections.Generic;

using UnityEngine;

/// <summary>
/// 클래스의 설계 의도입니다.
/// </summary>
public class AbilityContext
{
    #region ─────────────────────────▶ 내부 변수 ◀─────────────────────────

    #endregion

    #region ─────────────────────────▶ 공개 멤버 ◀─────────────────────────
    public UnitStats OwnerStats { get; }
    public Transform Origin { get; }
    public AbilityInstance Ability { get; }
    public IReadOnlyList<IDamageable> Targets { get; }


    public AbilityContext(
        UnitStats ownerStats,
        Transform ownerTransform,
        AbilityInstance ability,
        IReadOnlyList<IDamageable> targets)
    {
        OwnerStats = ownerStats;
        Origin = ownerTransform;
        Ability = ability;
        Targets = targets;
    }
    #endregion

    #region ─────────────────────────▶ 내부 메서드 ◀─────────────────────────

    #endregion
}
