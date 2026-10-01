using UnityEngine;

/// <summary>
/// 스킬 효과들의 베이스 SO
/// </summary>
public abstract class SkillContextSO : ScriptableObject
{
    #region ─────────────────────────▶ 인스펙터 ◀─────────────────────────

    #endregion

    #region ─────────────────────────▶ 공개 멤버 ◀─────────────────────────
    public abstract void UseSkill(params CharacterBaseMono[] targets);
    #endregion

    #region ─────────────────────────▶ 메시지 함수 ◀─────────────────────────
  
    #endregion
}
