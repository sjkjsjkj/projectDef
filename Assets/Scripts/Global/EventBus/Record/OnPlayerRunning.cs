/// <summary>
/// 플레이어가 달리는 중일 때
/// </summary>
public readonly struct OnPlayerRunning
{
    public readonly float movement;

    public OnPlayerRunning(float movement)
    {
        this.movement = movement;
    }

    /// <param name="movement">이동량</param>
    public static void Publish(float movement)
    {
        EventBus<OnPlayerRunning>.Publish(new OnPlayerRunning(movement));
    }
}
