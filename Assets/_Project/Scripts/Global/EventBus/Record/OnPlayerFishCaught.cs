/// <summary>
/// 플레이어가 물고기를 낚았을 때
/// </summary>
public readonly struct OnPlayerFishCaught
{
    public readonly string fishId;

    public OnPlayerFishCaught(string fishId)
    {
        this.fishId = fishId;
    }

    /// <param name="fishId">물고기 Id</param>
    public static void Publish(string fishId)
    {
        EventBus<OnPlayerFishCaught>.Publish(new OnPlayerFishCaught(fishId));
    }
}
