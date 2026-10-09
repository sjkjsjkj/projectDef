using UnityEngine;

/// <summary>
/// 아군 유닛을 대기 필드에 배치하는 테스트입니다. 적 소환은 WaveManager가 담당합니다.
/// </summary>
public class TestManager : BaseMono
{
    #region ─────────────────────────▶ 인스펙터 ◀─────────────────────────
    [Header("유닛 생성 테스트")]
    [SerializeField] private Unit[] units;
    [SerializeField] private WaitField waitField;
    [SerializeField] private bool unitTestFlag;
    
    #endregion

    #region ─────────────────────────▶ 내부 변수 ◀─────────────────────────

    #endregion

    #region ─────────────────────────▶ 공개 멤버 ◀─────────────────────────

    #endregion

    #region ─────────────────────────▶ 내부 메서드 ◀─────────────────────────
    private void Init()
    {
        if(unitTestFlag) UnitTest();
    }

    private void UnitTest()
    {
        if (waitField == null)
        {
            Debug.LogError("테스트 유닛을 배치할 WaitField가 없습니다.", this);
        }

        for (int i = 0; i < units.Length; i++)
        {
            Unit unit = units[i];
            if (unit == null || (waitField != null && waitField.TryAddUnit(unit)))
                continue;

            // 이미 배치된 유닛은 유지하고, 새로 배치하지 못한 테스트 유닛만 비활성화합니다.
            if (unit.CurrentTarget == null)
            {
                unit.gameObject.SetActive(false);
                Debug.LogWarning($"대기 슬롯 배치에 실패하여 테스트 유닛을 비활성화했습니다: {unit.name}", unit);
            }
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
