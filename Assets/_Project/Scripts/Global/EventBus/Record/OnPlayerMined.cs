/// <summary>
/// 플레이어가 광물을 채광했을 때
/// </summary>
public readonly struct OnPlayerMined
{
    public readonly string oreId;

    public OnPlayerMined(string oreId)
    {
        this.oreId = oreId;
    }

    /// <param name="oreId">광물 Id</param>
    public static void Publish(string oreId)
    {
        EventBus<OnPlayerMined>.Publish(new OnPlayerMined(oreId));
    }
}
