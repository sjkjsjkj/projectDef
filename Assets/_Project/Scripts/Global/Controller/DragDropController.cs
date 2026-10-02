using UnityEngine;
using UnityEngine.InputSystem;
/// <summary>
/// 클래스의 설계 의도입니다.
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
    private Transform _draggingTransform;
    

    private bool _isDragging;
    #endregion

    #region ─────────────────────────▶ 공개 멤버 ◀─────────────────────────

    #endregion

    #region ─────────────────────────▶ 내부 메서드 ◀─────────────────────────
    private void TryBeginDrag()
    {
        Vector2 mousePosition =
        Mouse.current.position.ReadValue();

        Ray ray =
            targetCamera.ScreenPointToRay(mousePosition);

        if (!Physics.Raycast(
                ray,
                out RaycastHit hit,
                Mathf.Infinity,
                draggableLayer))
        {
            return;
        }

        IDraggable draggable =
            hit.collider.GetComponentInParent<IDraggable>();

        if (draggable == null)
            return;



        _draggingObject = draggable;
        _sourceTarget = draggable.CurrentTarget;

        _isDragging = true;

        Debug.Log($"Drag Started: {_draggingObject.Transform.name}");

    }

    private void UpdateDrag()
    {
        Debug.Log("UpdateDrag");

        if (_draggingObject == null)
            return;

        if (TryGetDragPosition(out Vector3 position))
        {
            _draggingObject.Transform.position = position;
        }
    }

    private void EndDrag()
    {
        IDropTarget destination = FindDropTarget();

        if (destination != null &&
            destination.CanDrop(_draggingObject))
        {
            MoveToTarget(destination);
        }
        else
        {
            CancelDrag();
        }

        ClearDragState();
    }

    private bool TryGetDragPosition(out Vector3 position)
    {
        Vector2 mousePosition =
        Mouse.current.position.ReadValue();

        Ray ray =
            targetCamera.ScreenPointToRay(mousePosition);

        Plane plane = new Plane(
            dragPlaneTransform.up,
            dragPlaneTransform.position
        );

        if (plane.Raycast(ray, out float distance))
        {
            position = ray.GetPoint(distance);
            return true;
        }

        position = default;
        return false;
    }
    private IDropTarget FindDropTarget()
    {
        Vector2 mousePosition =
        Mouse.current.position.ReadValue();

        Ray ray =
            targetCamera.ScreenPointToRay(mousePosition);

        if (!Physics.Raycast(
                ray,
                out RaycastHit hit,
                Mathf.Infinity,
                dropTargetLayer))
        {
            return null;
        }

        return hit.collider.GetComponentInParent<IDropTarget>();
    }

    private void CancelDrag()
    {
        if (_sourceTarget == null)
            return;

        _sourceTarget?.OnDrop(_draggingObject);
    }

    private void MoveToTarget(IDropTarget destination)
    {
        _sourceTarget?.Remove(_draggingObject);

        destination.OnDrop(_draggingObject);
    }

    private void ClearDragState()
    {
        _draggingObject = null;
        _sourceTarget = null;
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
