using System;
using UnityEngine;

/// <summary>
/// 에너미에게 부착될 스크립트.
/// 에너미에게 Path를 따라 움직이게 할 스크립트.
/// </summary>
[RequireComponent(typeof(EnemyHealth))]
public class EnemyMovement : BaseMono
{
    #region ─────────────────────────▶ 인스펙터 ◀─────────────────────────
    [Header("이동 관련")]
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private float arriveDistance = 0.02f;

    #endregion

    #region ─────────────────────────▶ 내부 변수 ◀─────────────────────────
    private int _currentWaypointIndex;
    private EnemyPath _path;

    private EnemyHealth _enemyHealth;
    private bool _canMove = true;

    private bool _isMoving = false;
    private bool _removalReported;

    #endregion

    #region ─────────────────────────▶ 공개 멤버 ◀─────────────────────────
    public event Action<EnemyMovement> OnRemoved;

    public void Init(EnemyPath enemyPath, Transform spawnPoint = null)
    {
        _path = enemyPath;

        if (_path == null || _path.WaypointCount == 0)
            return;

        transform.position = spawnPoint != null ? spawnPoint.position : _path.GetWaypoint(0);
        _currentWaypointIndex = spawnPoint != null ? 0 : 1;
        _canMove = true;
        _removalReported = false;
        _isMoving = true;
    }
    #endregion

    #region ─────────────────────────▶ 내부 메서드 ◀─────────────────────────
    private void OnEnable()
    {
        _enemyHealth.OnDead += StopMovement;
    }
    private void OnDisable()
    {
        _enemyHealth.OnDead -= StopMovement;
        ReportRemoval();
    }
    private void Move()
    {
        if (_currentWaypointIndex >= _path.WaypointCount)
        {
            ReachGoal();
            return;
        }

        Vector3 targetPosition =
            _path.GetWaypoint(_currentWaypointIndex);

        transform.position =
            Vector3.MoveTowards(
                transform.position,
                targetPosition,
                moveSpeed * Time.deltaTime
            );

        float distance =
            Vector3.Distance(
                transform.position,
                targetPosition
            );

        if (distance <= arriveDistance)
        {
            transform.position = targetPosition;
            _currentWaypointIndex++;
        }
    }

    private void ReachGoal()
    {
        // 마지막 지점에서 첫 지점으로 이동하며 경로를 반복합니다.
        _currentWaypointIndex = 0;
    }

    private void StopMovement()
    {
        _canMove = false;
        ReportRemoval();
    }

    private void ReportRemoval()
    {
        if (_removalReported)
            return;

        _removalReported = true;
        _isMoving = false;
        OnRemoved?.Invoke(this);
    }
    #endregion

    #region ─────────────────────────▶ 메시지 함수 ◀─────────────────────────
    protected override void Awake()
    {
        base.Awake();
        _enemyHealth = GetComponent<EnemyHealth>();
    }

    private void Update()
    {
        if (!_canMove)
            return;
        if (!_isMoving || _path == null)
            return;

        Move();

    }

    #endregion

    #region ─────────────────────────▶ 중첩 타입 ◀─────────────────────────

    #endregion
}
