using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// 플레이어의 입력을 감지하는 스크립트 (필드 용)
/// </summary>
public class PlayerInputHandler : BaseMono
{
    #region ─────────────────────────▶ 인스펙터 ◀─────────────────────────
    //[Header("주제")]
    //[SerializeField] private Class _class;
    #endregion

    #region ─────────────────────────▶ 내부 변수 ◀─────────────────────────
    private Vector2 _moveInput;
    private bool _isRun;
    private bool _isInteract;
    private bool _isMenuOpen;
    #endregion

    #region ─────────────────────────▶ 공개 멤버 ◀─────────────────────────
    public Vector2 MoveInput => _moveInput;
    public bool InteractBtnPress => _isInteract;
    public bool MenuBtnPress => _isMenuOpen;
    public bool RunBtnPress => _isRun;

    public void OnMove(InputValue value)
    {
        _moveInput = value.Get<Vector2>();
    }
    public void OnInteract(InputValue value)
    {
        UDebug.Print($"{value}");
    }
    public void OnSprint(InputValue value)
    {
        UDebug.Print($"{value}");
    }
    public void OnMenuOpen(InputValue value)
    {
        UDebug.Print($"{value}");
    }
    #endregion

    #region ─────────────────────────▶ 내부 메서드 ◀─────────────────────────

    #endregion

    #region ─────────────────────────▶ 메시지 함수 ◀─────────────────────────

    #endregion

    #region ─────────────────────────▶ 중첩 타입 ◀─────────────────────────

    #endregion
}
