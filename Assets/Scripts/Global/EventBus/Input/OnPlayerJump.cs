/// <summary>
/// 플레이어가 점프 키를 눌렀을 때
/// 키를 눌렀을 때 1회 발행
/// </summary>
public readonly struct OnPlayerJump
{
    public static void Publish()
    {
        EventBus<OnPlayerJump>.Publish(new OnPlayerJump());
    }
}
