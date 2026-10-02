/// <summary>
/// 제작대를 열었을 때
/// </summary>
public readonly struct OnCraftingOpen
{
    public static void Publish()
    {
        EventBus<OnCraftingOpen>.Publish(new());
    }
}
