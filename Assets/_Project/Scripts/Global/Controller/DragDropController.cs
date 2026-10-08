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
    private Transform _draggingTransform;
    

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

        _draggingObject = draggable;
        _sourceTarget = draggable.CurrentTarget;

        _isDragging = true;

        Debug.Log($"Drag Started: {_draggingObject.Origin.name}");
        #endregion
    }

    private void UpdateDrag()
    {
        #region 3D Physics ver
        if (_draggingObject == null)
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
        _hoverTarget?.SetHighlight(false);

        _hoverTarget = newTarget;

        _hoverTarget?.SetHighlight(true);
        #endregion
    }

    private void EndDrag()
    {
        #region 3D Physics ver
        if (_draggingObject == null)
            return;

        // 유효한 DropTarget이 있고 배치 가능
        if (_hoverTarget != null &&
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
        //if (_sourceTarget == null)
        //    return;

        _sourceTarget?.OnDrop(_draggingObject);
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

        // 목적지의 기존 객체 제거
        if (destinationOccupant != null)
        {
            destination.Remove(destinationOccupant);
        }

        // 드래그 중인 객체를 원래 슬롯에서 제거
        _sourceTarget?.Remove(_draggingObject);

        // 드래그 객체를 목적지에 배치
        destination.OnDrop(_draggingObject);

        // 목적지에 원래 객체가 있었다면 출발지로 이동
        if (destinationOccupant != null && _sourceTarget != null)
        {
            _sourceTarget.OnDrop(destinationOccupant);
        }
    }

    private void ClearDragState()
    {
        _hoverTarget?.SetHighlight(false);

        _draggingObject = null;
        _sourceTarget = null;
        _hoverTarget = null;

        _isDragging = false;
    }
    #endregion

    #region ─────────────────────────▶ 메시지 함수 ◀─────────────────────────
    protected override void Awake()
    {
        if (targetCamera == null)
            targetCamera = Camera.main;
    }

    private void Update()
    {
        if (Mouse.current == null)
            return;

        if (!_isDragging)
        {
            if (Mouse.current.leftButton.wasPressedThisFrame)
            {
                TryBeginDrag();
            }

            return;
        }

        UpdateDrag();

        if (Mouse.current.leftButton.wasReleasedThisFrame)
        {
            EndDrag();
        }
    }
    #endregion

    #region ─────────────────────────▶ 중첩 타입 ◀─────────────────────────

    #endregion
}
