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

    [Header("Highlight")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color highlightColor = Color.yellow;
    #endregion

    #region ─────────────────────────▶ 내부 변수 ◀─────────────────────────
    private Unit _unit;
    #endregion

    #region ─────────────────────────▶ 공개 멤버 ◀─────────────────────────
    public bool IsEmpty => _unit == null;
    public IDraggable Occupant => _unit;


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
        //swap을 하기 위해 
        //if (!IsEmpty)
        //    return false;

        return draggable is Unit;
    }

    public void OnDrop(IDraggable draggable)
    {
        if (draggable is not Unit unit)
            return;

        SetUnit(unit);
    }
    public void SetHighlight(bool active)
    {
        if (spriteRenderer == null)
            return;

        spriteRenderer.color =
            active ? highlightColor : normalColor;
    }

    private void SetUnit(Unit unit)
    {
        _unit = unit;

        unit.CurrentTarget = this;
        unit.transform.position = transform.position;
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

        spriteRenderer = GetComponent<SpriteRenderer>();

        SetHighlight(false);
        if (initialUnit != null)
        {
            SetUnit(initialUnit);
        }
    }
    #endregion

    #region ─────────────────────────▶ 중첩 타입 ◀─────────────────────────

    #endregion
}
