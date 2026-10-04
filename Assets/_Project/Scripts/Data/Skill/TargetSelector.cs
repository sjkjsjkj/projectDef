using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// SO 클래스의 설계 의도입니다.
/// </summary>
public abstract class TargetSelector : ScriptableObject
{
    #region ─────────────────────────▶ 인스펙터 ◀─────────────────────────
    [Header("기본 정보")]
    [SerializeField] protected string id = "";
 
    #endregion

    #region ─────────────────────────▶ 공개 멤버 ◀─────────────────────────
    public string Id => id;
   
    // 값 유효성 검사
    public virtual bool IsValid()
    {
        if (id.IsEmpty()) return false;
       
        return true;
    }

    public abstract void SelectTargets(
        AbilityContext context,
        List<IDamageable> results);
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
