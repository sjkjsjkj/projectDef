using UnityEngine;

/// <summary>
/// 클래스의 설계 의도입니다.
/// </summary>
public class TestManager : BaseMono
{
    #region ─────────────────────────▶ 인스펙터 ◀─────────────────────────
    [Header("유닛 생성 테스트")]
    [SerializeField] private Unit[] units;
    [SerializeField] private WaitField waitField;
    [SerializeField] private bool _unitTestFlag;
    #endregion

    #region ─────────────────────────▶ 내부 변수 ◀─────────────────────────

    #endregion

    #region ─────────────────────────▶ 공개 멤버 ◀─────────────────────────

    #endregion

    #region ─────────────────────────▶ 내부 메서드 ◀─────────────────────────
    private void Init()
    {
        if(_unitTestFlag) UnitTest();
    }

    private void UnitTest()
    {
        for (int i = 0; i < units.Length; i++)
        {
            waitField.TryAddUnit(units[i]);
        }
    }
    #endregion

    #region ─────────────────────────▶ 메시지 함수 ◀─────────────────────────
    protected override void Awake()
    {
        
    }
    private void Start()
    {
        Init();
    }
    #endregion

    #region ─────────────────────────▶ 중첩 타입 ◀─────────────────────────

    #endregion
}
