/// <summary>
/// 도감을 열었을 때
/// </summary>
public readonly struct OnPictorialOpen
{
    public static void Publish()
    {
        EventBus<OnPictorialOpen>.Publish(new());
    }
}
