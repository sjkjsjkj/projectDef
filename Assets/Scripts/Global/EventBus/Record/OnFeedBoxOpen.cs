/// <summary>
/// 플레이어가 먹이통을 열었을 때
/// </summary>
public readonly struct OnFeedBoxOpen
{
    public static void Publish()
    {
        EventBus<OnFeedBoxOpen>.Publish(new OnFeedBoxOpen());
    }
}
