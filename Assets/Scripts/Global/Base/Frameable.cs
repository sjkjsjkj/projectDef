/// <summary>
/// 프레임 매니저에 가입하게 해주는 추상화 클래스
/// </summary>
public abstract class Frameable : BaseMono, IFrameable
{
    #region ─────────────────────────▶ 공개 멤버 ◀─────────────────────────
    public abstract EPriority Priority { get; }
    public abstract void ExecuteFrame();
    #endregion

    #region ─────────────────────────▶ 메시지 함수 ◀─────────────────────────
    // 프레임 매니저에 자동으로 가입
    protected virtual void OnEnable()
    {
        if (FrameManager.Ins != null)
        {
            FrameManager.Ins.Register(this);
        }
    }

    // 프레임 매니저에서 자동으로 탈퇴
    protected virtual void OnDisable()
    {
        if (FrameManager.Ins != null)
        {
            FrameManager.Ins.Unregister(this);
        }
    }
    #endregion
}
