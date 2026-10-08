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

    // 타깃 선택 결과는 문맥에 보관하며 공유 SO에는 저장하지 않습니다.
    internal List<IDamageable> TargetBuffer { get; } = new();

    public AbilityContext(Unit unit)
    {
        if (unit == null)
            throw new System.ArgumentNullException(nameof(unit));

        Owner = unit;
        OwnerStats = unit.Stat;
        Origin = unit.Origin;
        Targets = TargetBuffer.AsReadOnly();
    }
    #endregion

    #region ─────────────────────────▶ 내부 메서드 ◀─────────────────────────

    #endregion
}
