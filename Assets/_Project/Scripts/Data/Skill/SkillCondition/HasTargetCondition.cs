using UnityEngine;

/// <summary>
/// SO 클래스의 설계 의도입니다.
/// </summary>
[CreateAssetMenu(fileName = "HasTargetCondition_", menuName = "ScriptableObjects/Combat/SkillCondition/HasTargetCondition", order = 2)]
public class HasTargetCondition : SkillCondition
{
    #region ─────────────────────────▶ 인스펙터 ◀─────────────────────────

    #endregion

    #region ─────────────────────────▶ 공개 멤버 ◀─────────────────────────
   
    // 값 유효성 검사
    public override bool IsValid()
    {
        base.IsValid();
        return true;
    }

    public override bool IsSatisfied(AbilityContext context)
    {
        return context.Targets.Count > 0;
    }
    #endregion

    #region ─────────────────────────▶ 메시지 함수 ◀─────────────────────────
    protected override void OnValidate()
    {
        base.OnValidate();
    }
    #endregion
}
