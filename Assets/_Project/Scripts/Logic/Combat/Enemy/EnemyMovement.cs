using DG.Tweening.Plugins.Core.PathCore;
using System.IO;
using UnityEngine;

/// <summary>
/// 에너미에게 부착될 스크립트.
/// 에너미에게 Path를 따라 움직이게 할 스크립트.
/// </summary>
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

    #endregion

    #region ─────────────────────────▶ 공개 멤버 ◀─────────────────────────
    public void Init(EnemyPath enemyPath)
    {
        _path = enemyPath;

        if (_path == null || _path.WaypointCount == 0)
            return;

        _currentWaypointIndex = 0;

        transform.position =
            _path.GetWaypoint(_currentWaypointIndex);

        _currentWaypointIndex++;

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
        _isMoving = false;

        Debug.Log($"{name} 경로 끝 도착");

        Destroy(gameObject);
    }

    private void StopMovement()
    {
        _canMove = false;
    }
    #endregion

    #region ─────────────────────────▶ 메시지 함수 ◀─────────────────────────
    protected override void Awake()
    {
        _enemyHealth = GetComponent<EnemyHealth>();
    }
    private void Start()
    {
        _currentWaypointIndex = 0;
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
