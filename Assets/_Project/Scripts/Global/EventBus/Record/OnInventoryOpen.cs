/// <summary>
/// 인벤토리를 열었을 때
/// </summary>
public readonly struct OnInventoryOpen
{
    public static void Publish()
    {
        EventBus<OnInventoryOpen>.Publish(new());
    }
}
