using UnityEngine;

/// <summary>
/// 클래스의 설계 의도입니다.
/// </summary>

public enum EPlayerState
{
    None = 0,
    Idle = 1,
    Move = 2,
    Run = 3,
    Pause = 4
}

public class Player : BaseMono
{
    #region ─────────────────────────▶ 인스펙터 ◀─────────────────────────
    //[Header("주제")]
    //[SerializeField] private Class _class;
    #endregion

    #region ─────────────────────────▶ 내부 변수 ◀─────────────────────────
    private PlayerTrigger _playerTrigger;
    private PlayerMover _playerMover;
    private PlayerAnim _playerAnim;
    private PlayerInputHandler _playerInputHandler;
    private Rigidbody2D _rb;

    private EPlayerState _state;
    #endregion

    #region ─────────────────────────▶ 공개 멤버 ◀─────────────────────────
    public PlayerTrigger PlayerTrigger => _playerTrigger;
    public PlayerMover PlayerMover => _playerMover;
    public PlayerAnim PlayerAnim => _playerAnim;
    public PlayerInputHandler PlayerInput => _playerInputHandler;

    #endregion

    #region ─────────────────────────▶ 내부 메서드 ◀─────────────────────────
    private void SetState(EPlayerState nextState)
    {
        EPlayerState prevState =_state;

        ExitState(prevState);

        EnterState(_state);

        _state = nextState;

    }
    private void EnterState(EPlayerState state)
    {
        switch (state)
        {
            case EPlayerState.Idle:
                break;
            case EPlayerState.Move:
                break;
            case EPlayerState.Run:
                break;
            case EPlayerState.Pause:
                break;
            default:
                break;
        }
    }
    private void UpdateState(EPlayerState state)
    {
        switch (state)
        {
            case EPlayerState.Idle:
                UDebug.Print("Idle");
                if (_playerInputHandler.MoveInput != Vector2.zero)
                {
                    SetState(EPlayerState.Move);
                    return;
                }
                break;
            case EPlayerState.Move:
                UDebug.Print("Move");
                Vector2 inputDir = PlayerInput.MoveInput;

                //if (inputDir == Vector2.zero) SetState(EPlayerState.Idle);
                if(IsMoveCheck(inputDir)) SetState(EPlayerState.Idle);

                Vector2 moveDir = SetMoveDirection(inputDir);
                
                moveDir = SetMoveDirection(moveDir);

                PlayerMover.Move(moveDir);
                SetState(EPlayerState.Move);
                break;
            case EPlayerState.Run:
                break;
            case EPlayerState.Pause:
                break;
            default:
                break;
        }
    }
    private void ExitState(EPlayerState state)
    {
        switch (state)
        {
            case EPlayerState.Idle:
                break;
            case EPlayerState.Move:
                break;
            case EPlayerState.Run:
                break;
            case EPlayerState.Pause:
                break;
            default:
                break;
        }
    }
    private Vector2 SetMoveDirection(Vector2 moveDir)
    {
        if(Mathf.Abs(moveDir.x) > Mathf.Abs(moveDir.y))
        {
            return moveDir.x > 0 ? Vector2.right : Vector2.left;
        }
        else
        {
            return moveDir.y > 0 ? Vector2.up: Vector2.down;
        }
    }
    private bool IsMoveCheck(Vector2 inputDir)
    {
        if(Mathf.Abs( inputDir.x) >= 0.01f || Mathf.Abs(inputDir.y) >= 0.01f)
        {
            return true;
        }
        return false;
    }
    private void InitSetting()
    {
        SetState(EPlayerState.Idle);
    }

    #endregion

    #region ─────────────────────────▶ 메시지 함수 ◀─────────────────────────
    protected override void Awake()
    {
        base.Awake();
        _rb = GetComponent<Rigidbody2D>();
        _playerTrigger = GetComponentInChildren< PlayerTrigger>();
        _playerMover = GetComponentInChildren<PlayerMover>();
        _playerAnim = GetComponentInChildren<PlayerAnim>();
        _playerInputHandler = GetComponentInChildren<PlayerInputHandler>();
        if (_rb==null)
        {
            UDebug.Print("adfasf", LogType.Assert);
        }
        InitSetting();
        _playerMover.SetInfo(this, _rb);
    }

    private void Update()
    {
        UpdateState(_state);
    }
    #endregion

    #region ─────────────────────────▶ 중첩 타입 ◀─────────────────────────

    #endregion
}
