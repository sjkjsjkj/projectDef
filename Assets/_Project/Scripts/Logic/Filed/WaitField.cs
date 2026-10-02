using UnityEngine;

/// <summary>
/// 클래스의 설계 의도입니다.
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
        foreach (Slot slot in _slots)
        {
            if (slot.IsEmpty)
            {
                return slot;
            }
        }

        return null;
    }

    public bool TryAddUnit(Unit unit)
    {
        Slot emptySlot = GetEmptySlot();

        if (emptySlot == null)
            return false;

        emptySlot.OnDrop(unit);

        return true;
    }
    #endregion

    #region ─────────────────────────▶ 내부 메서드 ◀─────────────────────────

    #endregion

    #region ─────────────────────────▶ 메시지 함수 ◀─────────────────────────
    protected override void Awake()
    {
        _slots = GetComponentsInChildren<Slot>();
        UDebug.Print($"{_slots.Length} slots");
    }
    #endregion

    #region ─────────────────────────▶ 중첩 타입 ◀─────────────────────────

    #endregion
}
