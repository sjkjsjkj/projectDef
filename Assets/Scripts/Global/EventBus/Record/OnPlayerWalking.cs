/// <summary>
/// 플레이어가 걷는 중일 때
/// </summary>
public readonly struct OnPlayerWalking
{
    public readonly float movement;

    public OnPlayerWalking(float movement)
    {
        this.movement = movement;
    }

    /// <param name="movement">이동량</param>
    public static void Publish(float movement)
    {
        EventBus<OnPlayerWalking>.Publish(new OnPlayerWalking(movement));
    }
}
