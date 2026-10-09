using UnityEngine;

/// <summary>
/// 유닛을 놓을 슬롯에게 부착될 스크립트
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
        if (draggable is not Unit unit || unit == null)
            return;

        if (_unit != unit)
            return;

        _unit = null;

        if (ReferenceEquals(unit.CurrentTarget, this))
        {
            unit.CurrentTarget = null;
        }
    }

    public bool CanDrop(IDraggable draggable)
    {
        // 점유 중이어도 교환 후보가 될 수 있습니다. 실제 교환은 컨트롤러가 처리합니다.
        return draggable is Unit unit && unit != null;
    }

    public void OnDrop(IDraggable draggable)
    {
        if (draggable is not Unit unit || unit == null)
            return;

        if (_unit != null && _unit != unit)
        {
            Debug.LogError($"기존 유닛을 제거하지 않고 슬롯을 덮어쓸 수 없습니다: {name}", this);
            return;
        }

        if (unit.CurrentTarget != null && !ReferenceEquals(unit.CurrentTarget, this))
        {
            Debug.LogError($"기존 슬롯에서 제거하지 않고 유닛을 중복 배치할 수 없습니다: {unit.name}", unit);
            return;
        }

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
        base.Awake();

        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();

        SetHighlight(false);
        if (initialUnit != null)
        {
            OnDrop(initialUnit);
        }
    }
    #endregion

    #region ─────────────────────────▶ 중첩 타입 ◀─────────────────────────

    #endregion
}
