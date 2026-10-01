/// <summary>
/// 플레이어가 도감 키를 눌렀을 때
/// 키를 눌렀을 때 1회 발행
/// </summary>
public readonly struct OnPlayerPictorial
{
    public static void Publish()
    {
        EventBus<OnPlayerPictorial>.Publish(new OnPlayerPictorial());
    }
}
