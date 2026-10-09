using UnityEngine;
using UnityEngine.InputSystem;
/// <summary>
/// 드래그 / 드랍을 관리하는 컨트롤러
/// </summary>
[System.Serializable]
public class DragDropController : BaseMono
{
    #region ─────────────────────────▶ 인스펙터 ◀─────────────────────────
    [Header("카메라")]
    [SerializeField] private Camera targetCamera;

    [Header("드래그 N 드롭 레이어")]
    [SerializeField] private LayerMask draggableLayer;
    [SerializeField] private LayerMask dropTargetLayer;

    [Header("드래그 N 드롭 영역")]
    [SerializeField] private Transform dragPlaneTransform;
    #endregion

    #region ─────────────────────────▶ 내부 변수 ◀─────────────────────────
    private IDraggable _draggingObject;
    private IDropTarget _sourceTarget;
    private IDropTarget _hoverTarget;

    private bool _isDragging;
    #endregion

    #region ─────────────────────────▶ 공개 멤버 ◀─────────────────────────

    #endregion

    #region ─────────────────────────▶ 내부 메서드 ◀─────────────────────────
    private void TryBeginDrag()
    {
        #region 3D Physics ver
        //Vector2 mousePosition = Mouse.current.position.ReadValue();
        //Ray ray = targetCamera.ScreenPointToRay(mousePosition);

        //if (!Physics.Raycast(
        //        ray,
        //        out RaycastHit hit,
        //        Mathf.Infinity,
        //        draggableLayer))
        //{
        //    return;
        //}

        //IDraggable draggable =
        //    hit.collider.GetComponentInParent<IDraggable>();

        //if (draggable == null)
        //    return;



        //_draggingObject = draggable;
        //_sourceTarget = draggable.CurrentTarget;

        //_isDragging = true;

        //Debug.Log($"Drag Started: {_draggingObject.Origin.name}");
        #endregion

        UDebug.Print($"TryBeginDrag");

        #region 2D Physics ver
        Vector2 mousePosition = Mouse.current.position.ReadValue();

        Vector2 worldPosition =
            targetCamera.ScreenToWorldPoint(mousePosition);

        Collider2D hit = Physics2D.OverlapPoint(
            worldPosition,
            draggableLayer);

        if (hit == null)
            return;

        IDraggable draggable =
            hit.GetComponentInParent<IDraggable>();

        if (draggable == null)
            return;

        if (!HasValidPlacement(draggable, draggable.CurrentTarget))
        {
            Debug.LogError("슬롯과 유닛의 배치 연결이 올바르지 않아 드래그를 시작할 수 없습니다.", hit);
            return;
        }

        _draggingObject = draggable;
        _sourceTarget = draggable.CurrentTarget;

        _isDragging = true;

        Debug.Log($"Drag Started: {_draggingObject.Origin.name}");
        #endregion
    }

    private void UpdateDrag()
    {
        #region 3D Physics ver
        if (!ValidateSource())
            return;

        //마우스 위치에 따라 드래그 오브젝트 이동
        if (TryGetDragPosition(out Vector3 position))
        {
            _draggingObject.Origin.position = position;
        }

        //현재 마우스 아래의 DropTarget 탐색
        IDropTarget newTarget = FindDropTarget();

        //이전 Target과 동일하면 아무것도 하지 않음
        if (_hoverTarget == newTarget)
            return;

        //hover 갱신
        if (IsAlive(_hoverTarget))
            _hoverTarget.SetHighlight(false);

        _hoverTarget = newTarget;

        if (IsAlive(_hoverTarget))
            _hoverTarget.SetHighlight(true);
        #endregion
    }

    private void EndDrag()
    {
        #region 3D Physics ver
        // 목적지의 점유자를 제거하기 전에 출발 슬롯의 연결부터 확인합니다.
        if (!ValidateSource())
            return;

        // 유효한 DropTarget이 있고 배치 가능
        if (IsAlive(_hoverTarget) &&
            _hoverTarget.CanDrop(_draggingObject))
        {
            MoveToTarget(_hoverTarget);
        }
        else
        {
            CancelDrag();
        }

        ClearDragState();
        #endregion
    }

    private bool TryGetDragPosition(out Vector3 position)
    {
        #region 3D Physics ver
        //Vector2 mousePosition = Mouse.current.position.ReadValue();
        //Ray ray = targetCamera.ScreenPointToRay(mousePosition);

        //Plane plane = new Plane(
        //    dragPlaneTransform.up,
        //    dragPlaneTransform.position
        //);

        //if (plane.Raycast(ray, out float distance))
        //{
        //    position = ray.GetPoint(distance);
        //    return true;
        //}

        //position = default;
        //return false;
        #endregion

        Vector2 mousePosition = Mouse.current.position.ReadValue();

        Vector3 worldPosition =
            targetCamera.ScreenToWorldPoint(mousePosition);

        position = new Vector3(
            worldPosition.x,
            worldPosition.y,
            _draggingObject.Origin.position.z
        );

        return true;
    }
    private IDropTarget FindDropTarget()
    {
        #region 3D Physics ver
        //Vector2 mousePosition = Mouse.current.position.ReadValue();
        //Ray ray = targetCamera.ScreenPointToRay(mousePosition);

        //if (!Physics.Raycast(
        //        ray,
        //        out RaycastHit hit,
        //        Mathf.Infinity,
        //        dropTargetLayer))
        //{
        //    return null;
        //}

        //return hit.collider.GetComponentInParent<IDropTarget>();
        #endregion

        Vector2 mousePosition = Mouse.current.position.ReadValue();

        Vector2 worldPosition =
            targetCamera.ScreenToWorldPoint(mousePosition);

        Collider2D hit = Physics2D.OverlapPoint(
            worldPosition,
            dropTargetLayer);

        if (hit == null)
            return null;

        return hit.GetComponentInParent<IDropTarget>();
    }

    private void CancelDrag()
    {
        // 외부에서 변경된 배치에는 이전 슬롯을 강제로 덮어쓰지 않습니다.
        if (HasValidPlacement(_draggingObject, _sourceTarget))
            _sourceTarget.OnDrop(_draggingObject);
    }

    private void MoveToTarget(IDropTarget destination)
    {
        // 같은 슬롯이면 아무것도 하지 않음
        if (destination == _sourceTarget)
        {
            destination.OnDrop(_draggingObject);
            return;
        }

        IDraggable destinationOccupant = destination.Occupant;

        // 교환은 양쪽 배치 관계와 출발 슬롯의 수용 여부를 확인한 뒤 시작합니다.
        if (destinationOccupant != null &&
            (!HasValidPlacement(destinationOccupant, destination) ||
             !_sourceTarget.CanDrop(destinationOccupant)))
        {
            Debug.LogError("목적지 유닛의 배치 연결 또는 교환 조건이 올바르지 않습니다.", this);
            CancelDrag();
            return;
        }

        // 목적지의 기존 객체 제거
        if (destinationOccupant != null)
        {
            destination.Remove(destinationOccupant);
        }

        // 드래그 중인 객체를 원래 슬롯에서 제거
        _sourceTarget.Remove(_draggingObject);

        // 드래그 객체를 목적지에 배치
        destination.OnDrop(_draggingObject);

        // 목적지에 원래 객체가 있었다면 출발지로 이동
        if (destinationOccupant != null)
        {
            _sourceTarget.OnDrop(destinationOccupant);
        }
    }

    private static bool IsAlive(object value)
    {
        // 인터페이스 참조에서도 Unity 오브젝트의 파괴 여부를 확인합니다.
        return value != null && (value is not Object unityObject || unityObject != null);
    }

    private static bool HasValidPlacement(IDraggable draggable, IDropTarget target)
    {
        return IsAlive(draggable) && IsAlive(target) &&
               draggable.CurrentTarget == target && target.Occupant == draggable;
    }

    private bool ValidateSource()
    {
        if (HasValidPlacement(_draggingObject, _sourceTarget) &&
            _draggingObject.Origin.gameObject.activeInHierarchy)
            return true;

        Debug.LogError("드래그 중 유닛 또는 출발 슬롯의 상태가 변경되어 드래그를 중단합니다.", this);
        ClearDragState();
        return false;
    }

    private void ClearDragState()
    {
        if (IsAlive(_hoverTarget))
            _hoverTarget.SetHighlight(false);

        _draggingObject = null;
        _sourceTarget = null;
        _hoverTarget = null;

        _isDragging = false;
    }
    #endregion

    #region ─────────────────────────▶ 메시지 함수 ◀─────────────────────────
    protected override void Awake()
    {
        base.Awake();
        if (targetCamera == null)
            targetCamera = Camera.main;

        if (targetCamera == null)
        {
            Debug.LogError("드래그에 사용할 카메라가 없습니다.", this);
            enabled = false;
        }
    }

    private void OnDisable()
    {
        if (!_isDragging)
            return;

        CancelDrag();
        ClearDragState();
    }

    private void Update()
    {
        if (Mouse.current == null)
        {
            if (_isDragging)
            {
                CancelDrag();
                ClearDragState();
            }
            return;
        }

        if (!_isDragging)
        {
            if (Mouse.current.leftButton.wasPressedThisFrame)
            {
                TryBeginDrag();
            }
        }

        if (!_isDragging)
            return;

        UpdateDrag();

        if (_isDragging && Mouse.current.leftButton.wasReleasedThisFrame)
        {
            EndDrag();
        }
    }
    #endregion

    #region ─────────────────────────▶ 중첩 타입 ◀─────────────────────────

    #endregion
}
