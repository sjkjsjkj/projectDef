using UnityEngine;

/// <summary>
/// 메인 액션 턴에서 데미지 후에 발동하는 효과들
/// </summary>
[CreateAssetMenu(fileName = "SkillPost_", menuName = "ScriptableObjects/SkillSO", order = 1)]
public abstract class SkillPostProgressingSO : ScriptableObject
{
    #region ─────────────────────────▶ 인스펙터 ◀─────────────────────────

    #endregion

    #region ─────────────────────────▶ 공개 멤버 ◀─────────────────────────
    public abstract void Excute();
    #endregion

    #region ─────────────────────────▶ 메시지 함수 ◀─────────────────────────
  
    #endregion
}
