using UnityEngine;

/// <summary>
/// SO 클래스의 설계 의도입니다.
/// </summary>
[CreateAssetMenu(fileName = "SkillRankChange_", menuName = "ScriptableObjects/SkillSO", order = 1)]
public class SkillRankChangeSO : ScriptableObject
{
    #region ─────────────────────────▶ 인스펙터 ◀─────────────────────────
    [Header("랭크업/다운")]
    [SerializeField] private EType _type;
    [SerializeField] private int _rank;
    #endregion

    #region ─────────────────────────▶ 공개 멤버 ◀─────────────────────────
    public void Excute(CharacterBaseMono target)
    {
        //todo target의 스탯 랭크 변화
    }
    #endregion

    #region ─────────────────────────▶ 메시지 함수 ◀─────────────────────────

    #endregion
}
