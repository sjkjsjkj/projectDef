using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 스킬들이 타겟을 선택하는 방법
/// </summary>
public abstract class TargetSelector : ScriptableObject
{
    #region ─────────────────────────▶ 인스펙터 ◀─────────────────────────

    #endregion

    #region ─────────────────────────▶ 공개 멤버 ◀─────────────────────────

    public abstract void SelectTargets(
        AbilityContext context,
        AbilityData data,
        List<IDamageable> results);
    #endregion

    #region ─────────────────────────▶ 메시지 함수 ◀─────────────────────────

    #endregion
}
