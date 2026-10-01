using System;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// 플레이어의 이동을 관리하는 스크립트
/// </summary>


public class PlayerMover : BaseMono
{
    #region ─────────────────────────▶ 인스펙터 ◀─────────────────────────
    [Header("테스트용")]
    [SerializeField] private float _moveSpeed=50.0f;
    #endregion

    #region ─────────────────────────▶ 내부 변수 ◀─────────────────────────
    private Player _master;
    private Vector2 _moveDir;
    private float _tileSize = 0.5f;

    private Vector2 targetPosition;

    private bool _isMoving;
    #endregion

    #region ─────────────────────────▶ 공개 멤버 ◀─────────────────────────
    public bool IsMoving => _isMoving;
    public void SetInfo(Player player, Rigidbody2D rb)
    {
        _master = player;
    }
    public void Move(Vector2 moveDir)
    {
        if (_isMoving) return;

        targetPosition = (Vector2)transform.position + (Vector2)(moveDir * _tileSize);
        _isMoving = true;
        //todo 한칸씩 이동 (0.5, 0.5)
    }
    #endregion

    #region ─────────────────────────▶ 내부 메서드 ◀─────────────────────────
    private void Update()
    {
        if (!_isMoving) return;

        transform.position = Vector2.MoveTowards(transform.position, targetPosition, _moveSpeed * Time.deltaTime);

        if(Vector2.Distance(transform.position, targetPosition) < 0.001f)
        {
            transform.position = targetPosition;
            _isMoving = false;
        }
    }
    #endregion

    #region ─────────────────────────▶ 메시지 함수 ◀─────────────────────────

    #endregion

}
