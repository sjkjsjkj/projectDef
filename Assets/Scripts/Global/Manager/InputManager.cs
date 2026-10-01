using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Input System으로 키 입력을 받아 이벤트를 뿌립니다.
/// </summary>d
public sealed class InputManager : GlobalSingleton<InputManager>, InputDispatcher.IMainActionMapActions
{
    #region ─────────────────────────▶ 내부 변수 ◀─────────────────────────
    private bool _isInitialized = false;
    private InputDispatcher _input; // 디스페처 주소

    private Vector2 _moveInput; // WASD 이동 변수

    private int _curSlot; // 현재 슬롯 변수
    private float _nextSlotChangeTime;
    private const float SCROLL_INTERVAL = 0.01f;
    #endregion

    #region ─────────────────────────▶ 공개 멤버 ◀─────────────────────────
    // 공개 멤버 함수 모두 외부 호출 용도가 아닙니다.
    public void OnMove(InputAction.CallbackContext context)
    {
        Vector2 moveInput = context.ReadValue<Vector2>();
        if (context.performed || context.canceled)
        {
            OnPlayerMove.Publish(moveInput);
        }
    }
    public void OnJump(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            OnPlayerJump.Publish();
        }
    }
    public void OnInteract(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            OnPlayerInteract.Publish();
        }
    }
    public void OnInventory(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            OnPlayerInventory.Publish();
        }
    }
    public void OnPictorial(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            OnPlayerPictorial.Publish();
        }
    }
    public void OnRun(InputAction.CallbackContext context)
    {
        if (context.started || context.performed)
        {
            OnPlayerRun.Publish(true);
        }
        else if (context.canceled)
        {
            OnPlayerRun.Publish(false);
        }
    }
    public void OnUse(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            OnPlayerItemUse.Publish();
        }
    }
    public void OnSelectSlot(InputAction.CallbackContext context)
    {
        if (!context.performed) return;
        // 숫자로 변환
        string name = context.control.name;
        if (int.TryParse(name, out int slot))
        {
            slot = slot == 0 ? 9 : slot - 1;
            ChangeSlot(slot);
        }
        else
        {
            UDebug.PrintOnce($"숫자로 변환할 수 없는 슬롯 키({name})를 받았습니다.", LogType.Assert);
        }
    }
    public void OnScrollSlot(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            // 최소 대기 시간
            if (_nextSlotChangeTime > Time.unscaledTime)
            {
                return;
            }
            // 증가시키기엔 밀린 시간이 많을 경우 재설정
            if (_nextSlotChangeTime < Time.unscaledTime - SCROLL_INTERVAL)
            {
                _nextSlotChangeTime = Time.unscaledTime + SCROLL_INTERVAL;
            }
            else
            {
                _nextSlotChangeTime += SCROLL_INTERVAL;
            }
            float scrollVal = context.ReadValue<Vector2>().y;
            // 약한 스크롤 무시
            if (Mathf.Approximately(scrollVal, 0))
            {
                return;
            }
            int dir = scrollVal > 0 ? 1 : -1; // 스크롤 방향
            int slot = _curSlot + dir; // 방향으로 슬롯 이동하고 범위 밖 처리는 ChangeSlot에 맡기기
            ChangeSlot(slot);
        }
    }
    public void OnMenu(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            OnPlayerEsc.Publish();
        }
    }
    // Awake 대용
    public override void Initialize()
    {
        if (_isInitialized) return;

        _input = new InputDispatcher();
        _input.MainActionMap.SetCallbacks(this);
        _input.Enable();
        _isInitialized = true;
    }
    #endregion

    #region ─────────────────────────▶ 내부 메서드 ◀─────────────────────────
    private void ChangeSlot(int slot)
    {
        slot = (slot + 10) % 10;
        OnPlayerSelectSlot.Publish(slot);
        _curSlot = slot;
    }
    #endregion

    #region ─────────────────────────▶ 메세지 함수 ◀─────────────────────────
    // ↓ 외부에서 호출해도 OK
    public void OnEnable()
    {
        if (_input == null)
        {
            return;
        }

        _input.Enable();
    }
    public void OnDisable()
    {
        if (_input == null)
        {
            return;
        }

        _input.Disable();
    }
    #endregion
}
