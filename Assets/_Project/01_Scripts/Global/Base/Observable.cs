using System;
using System.Collections.Generic;

/// <summary>
/// 변할 시 자동으로 이벤트를 발행하는 옵저버입니다.
/// </summary>
public class Observable<T>
{
    // 값 변경을 인식하기 위해 저장하는 값
    private T _value;

    #region ─────────────────────────▶ 공개 멤버 ◀─────────────────────────
    private event Action<T> OnValueChanged;
    public T Value
    {
        get { return _value; }
        set
        {
            // 새로운 값이 들어왔을 때 호출
            if (!EqualityComparer<T>.Default.Equals(_value, value))
            {
                OnValueChanged?.Invoke(value);
                _value = value;
            }
        }
    }
    /// <summary>
    /// 해당 옵저버를 구독합니다.
    /// </summary>
    public void Bind(Action<T> action) => OnValueChanged += action;

    /// <summary>
    /// 해당 옵저버를 구독 해제합니다.
    /// </summary>
    public void UnBind(Action<T> action) => OnValueChanged -= action;
    #endregion
}
