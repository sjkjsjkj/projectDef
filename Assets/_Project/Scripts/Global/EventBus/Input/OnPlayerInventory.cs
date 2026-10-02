/// <summary>
/// 플레이어가 인벤토리 키를 눌렀을 때
/// 키를 눌렀을 때 1회 발행
/// </summary>
public readonly struct OnPlayerInventory
{
    public static void Publish()
    {
        EventBus<OnPlayerInventory>.Publish(new OnPlayerInventory());
    }
}
