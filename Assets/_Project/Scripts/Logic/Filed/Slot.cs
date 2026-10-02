using UnityEngine;

/// <summary>
/// 클래스의 설계 의도입니다.
/// </summary>
public class Slot : BaseMono , IDropTarget
{
    #region ─────────────────────────▶ 인스펙터 ◀─────────────────────────
    [Header("슬롯 정보")]
    [SerializeField] private int slotIndex;
    [SerializeField] private Unit initialUnit;
    #endregion

    #region ─────────────────────────▶ 내부 변수 ◀─────────────────────────
    private Unit _unit;
    #endregion

    #region ─────────────────────────▶ 공개 멤버 ◀─────────────────────────
    public bool IsEmpty => _unit == null;

    public void SetUnit(Unit unit)
    {
        _unit = unit;

        unit.transform.position = transform.position;
    }

    public void Remove(IDraggable draggable)
    {
        if (draggable is not Unit unit)
            return;

        if (_unit != unit)
            return;

        _unit = null;

        if (unit.CurrentTarget == this)
        {
            unit.CurrentTarget = null;
        }
    }

    public bool CanDrop(IDraggable draggable)
    {
        if (!IsEmpty)
            return false;

        return draggable is Unit;
    }
    public void OnDrop(IDraggable draggable)
    {
        if (draggable is not Unit unit)
            return;

        _unit = unit;
    }
    #endregion

    #region ─────────────────────────▶ 내부 메서드 ◀─────────────────────────

    #endregion

    #region ─────────────────────────▶ 메시지 함수 ◀─────────────────────────
    protected override void Awake()
    {
        if (initialUnit != null)
        {
            SetUnit(initialUnit);
        }
    }
    #endregion

    #region ─────────────────────────▶ 중첩 타입 ◀─────────────────────────

    #endregion
}
