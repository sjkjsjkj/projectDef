/// <summary>
/// 달리기 입력 상태
/// </summary>
public readonly struct OnPlayerRun
{
    public readonly bool isRun;

    public OnPlayerRun(bool isRun)
    {
        this.isRun = isRun;
    }

    /// <param name="isRun">isRun</param>
    public static void Publish(bool isRun)
    {
        EventBus<OnPlayerRun>.Publish(new OnPlayerRun(isRun));
    }
}
