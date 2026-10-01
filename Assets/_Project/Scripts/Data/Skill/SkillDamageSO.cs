using UnityEngine;

/// <summary>
/// SO 클래스의 설계 의도입니다.
/// </summary>
[CreateAssetMenu(fileName = "SkillDamage_", menuName = "ScriptableObjects/SkillSO", order = 1)]
public class SkillDamageSO : SkillContextSO
{
    #region ─────────────────────────▶ 인스펙터 ◀─────────────────────────
    [Header("기본 정보")]
    [SerializeField] protected int _damage;
    #endregion

    #region ─────────────────────────▶ 공개 멤버 ◀─────────────────────────
    public int Damage => _damage;

    // 값 유효성 검사
    public virtual bool IsValid()
    {
        if (_damage == 0) return false;
        return true;
    }
    public override void UseSkill(params CharacterBaseMono[] targets)
    {
        
    }
    #endregion

    #region ─────────────────────────▶ 메시지 함수 ◀─────────────────────────
    protected virtual void OnValidate()
    {
        if (!IsValid())
        {
            UDebug.PrintOnce($"SO 인스턴스({this.name})의 값이 올바르지 않습니다. Type = {this.GetType().Name})", LogType.Warning);
        }
    }
    #endregion
}
