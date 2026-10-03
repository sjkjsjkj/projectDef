using UnityEditor;
using UnityEngine;

/// <summary>
/// 클래스의 설계 의도입니다.
/// </summary>
public class UnitCombatController : BaseMono
{
    #region ─────────────────────────▶ 인스펙터 ◀─────────────────────────
    [Header("주제")]
    [SerializeField] private UnitStats unitStats;

    [SerializeField] private AbilityData basicAttack;
    [SerializeField] private AbilityData skill;
    #endregion

    #region ─────────────────────────▶ 내부 변수 ◀─────────────────────────
    private AbilityInstance _basicAttackInstance;
    private AbilityInstance _skillInstance;
    #endregion

    #region ─────────────────────────▶ 공개 멤버 ◀─────────────────────────

    #endregion

    #region ─────────────────────────▶ 내부 메서드 ◀─────────────────────────

    #endregion

    #region ─────────────────────────▶ 메시지 함수 ◀─────────────────────────
    protected override void Awake()
    {
        _basicAttackInstance =
            new AbilityInstance(basicAttack);

        _skillInstance =
            new AbilityInstance(skill);
    }

    private void Update()
    {
        _basicAttackInstance.Tick(Time.deltaTime);
        _skillInstance.Tick(Time.deltaTime);

        if (_skillInstance.TryActivate(unitStats,transform))
            return;

        _basicAttackInstance.TryActivate(unitStats, transform);
    }
    #endregion

    #region ─────────────────────────▶ 중첩 타입 ◀─────────────────────────

    #endregion
}
