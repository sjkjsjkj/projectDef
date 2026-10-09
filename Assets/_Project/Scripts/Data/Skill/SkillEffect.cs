using UnityEngine;

/// <summary>
/// 스킬의 효과로 들어갈 수 있는 효과들을 쪼개 놓은 SO
/// </summary>
public abstract class SkillEffect : ScriptableObject 
{
    #region ─────────────────────────▶ 인스펙터 ◀─────────────────────────
    [Header("정보")]
    [SerializeField] private string id;
    #endregion

    #region ─────────────────────────▶ 공개 멤버 ◀─────────────────────────
    public string Id => id;

    public abstract void Apply(AbilityContext context);
    // 값 유효성 검사
    public virtual bool IsValid()
    {
        if (id.IsEmpty()) return false;
        return true;
    }
    #endregion

    #region ─────────────────────────▶ 메시지 함수 ◀─────────────────────────
    protected virtual void OnValidate()
    {
        if (!IsValid())
        {
            UDebug.PrintOnce($"SO 인스턴스({this.name})의 값이 올바르지 않습니다. (ID = {id}, Type = {this.GetType().Name})", LogType.Warning);
        }
    }
    #endregion
}
