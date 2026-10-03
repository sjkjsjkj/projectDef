using UnityEngine;

/// <summary>
/// 클래스의 설계 의도입니다.
/// </summary>
public class Unit : CharacterBaseMono, IDraggable
{
    #region ─────────────────────────▶ 인스펙터 ◀─────────────────────────
    [Header("테스트용 외부 인스펙터 노출 멤버")]
    [SerializeField] private float attackPower = 10f;
    #endregion

    #region ─────────────────────────▶ 내부 변수 ◀─────────────────────────
    private UnitStats _stats;
    #endregion

    #region ─────────────────────────▶ 공개 멤버 ◀─────────────────────────
    public Transform Transform => transform;
    public float AttackPower => attackPower;
    public UnitStats Stats => _stats;
    public IDropTarget CurrentTarget { get; set; }
    #endregion

    #region ─────────────────────────▶ 내부 메서드 ◀─────────────────────────

    #endregion

    #region ─────────────────────────▶ 메시지 함수 ◀─────────────────────────
    protected override void Awake()
    {
        _stats = new UnitStats(attackPower);
    }
    #endregion

    #region ─────────────────────────▶ 중첩 타입 ◀─────────────────────────

    #endregion
}
