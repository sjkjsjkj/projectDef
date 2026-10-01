/// <summary>
/// 플레이어가 작물을 수확했을 때
/// </summary>
public readonly struct OnPlayerHarvesting
{
    public readonly string cropId;

    public OnPlayerHarvesting(string cropId)
    {
        this.cropId = cropId;
    }

    /// <param name="cropId">작물 Id</param>
    public static void Publish(string cropId)
    {
        EventBus<OnPlayerHarvesting>.Publish(new OnPlayerHarvesting(cropId));
    }
}
