/// <summary>
/// 상자를 열었을 때
/// </summary>
public readonly struct OnChestOpen
{
    public static void Publish()
    {
        EventBus<OnChestOpen>.Publish(new());
    }
}
