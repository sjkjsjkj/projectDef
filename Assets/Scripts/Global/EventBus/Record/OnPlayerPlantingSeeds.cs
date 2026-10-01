/// <summary>
/// 플레이어가 씨앗을 심었을 때
/// </summary>
public readonly struct OnPlayerPlantingSeeds
{
    public readonly string seedId;

    public OnPlayerPlantingSeeds(string seedId)
    {
        this.seedId = seedId;
    }

    /// <param name="seedId">씨앗 Id</param>
    public static void Publish(string seedId)
    {
        EventBus<OnPlayerPlantingSeeds>.Publish(new OnPlayerPlantingSeeds(seedId));
    }
}
