using UnityEngine;

/// <summary>
/// 모든 SO 클래스가 상속받아야하는 기본 데이터입니다.
/// </summary>
public abstract class BaseSO : ScriptableObject
{
    #region ─────────────────────────▶ 인스펙터 ◀─────────────────────────
    [Header("기본 정보")]
    [SerializeField] protected string id;
    [SerializeField] protected EResourceType type;
    #endregion

    #region ─────────────────────────▶ 공개 멤버 ◀─────────────────────────
    public string Id => id;
    
    // 정상 값을 가지는지 검사
    public virtual bool IsValid()
    {
        if (id.IsEmpty()) return false;
    
        return true;
    }
    #endregion

    #region ─────────────────────────▶ 메시지 함수 ◀─────────────────────────
    // 인스펙터 변수 유효성 검사
    protected virtual void OnValidate()
    {
        if (!IsValid())
        {
            UDebug.PrintOnce($"SO 인스턴스({this.name})의 값이 올바르지 않습니다. (ID = {id}, Type = {this.GetType().Name})", LogType.Warning);
        }
    }
    #endregion
}
