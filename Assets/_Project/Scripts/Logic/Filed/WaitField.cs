using UnityEngine;

/// <summary>
/// 새로 획득한 유닛을 빈 대기 슬롯에 등록합니다.
/// </summary>
public class WaitField : BaseMono
{
    #region ─────────────────────────▶ 인스펙터 ◀─────────────────────────
    //[Header("주제")]
    //[SerializeField] private Class _class;
    #endregion

    #region ─────────────────────────▶ 내부 변수 ◀─────────────────────────
    private Slot[] _slots;
    #endregion

    #region ─────────────────────────▶ 공개 멤버 ◀─────────────────────────
    public Slot[] Slots => _slots;

    public Slot GetEmptySlot()
    {
        if (_slots == null)
            return null;

        foreach (Slot slot in _slots)
        {
            if (slot != null && slot.IsEmpty)
            {
                return slot;
            }
        }

        return null;
    }

    /// <summary>
    /// 아직 배치되지 않은 유닛만 등록합니다.
    /// 실패 시 상태를 변경하지 않으며, 획득 취소/보류는 호출자가 처리합니다.
    /// </summary>
    public bool TryAddUnit(Unit unit)
    {
        if (unit == null)
            return false;

        if (unit.CurrentTarget != null)
        {
            Debug.LogError($"이미 슬롯에 등록된 유닛은 다시 획득 배치할 수 없습니다: {unit.name}", unit);
            return false;
        }

        if (_slots == null)
        {
            Debug.LogError("WaitField 초기화 전에 유닛 배치를 요청했습니다.", this);
            return false;
        }

        Slot emptySlot = GetEmptySlot();

        if (emptySlot == null)
            return false;

        emptySlot.OnDrop(unit);

        return ReferenceEquals(unit.CurrentTarget, emptySlot) && ReferenceEquals(emptySlot.Occupant, unit);
    }
    #endregion

    #region ─────────────────────────▶ 내부 메서드 ◀─────────────────────────

    #endregion

    #region ─────────────────────────▶ 메시지 함수 ◀─────────────────────────
    protected override void Awake()
    {
        base.Awake();
        _slots = GetComponentsInChildren<Slot>();
        UDebug.Print($"{_slots.Length} slots");
    }
    #endregion

    #region ─────────────────────────▶ 중첩 타입 ◀─────────────────────────

    #endregion
}
