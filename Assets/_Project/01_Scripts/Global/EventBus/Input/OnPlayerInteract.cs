/// <summary>
/// 플레이어가 상호작용 키를 눌렀을 때
/// 키를 눌렀을 때 1회 발행
/// </summary>
public readonly struct OnPlayerInteract
{
    public static void Publish()
    {
        EventBus<OnPlayerInteract>.Publish(new OnPlayerInteract());
    }
}
