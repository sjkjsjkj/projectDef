/// <summary>
/// 플레이어가 아이템을 획득했을 때
/// </summary>
public readonly struct OnPlayerGetItem
{
    public readonly string itemId;
    public readonly int amount;

    public OnPlayerGetItem(string itemId, int amount)
    {
        this.itemId = itemId;
        this.amount = amount;
    }

    /// <param name="itemId">아이템 Id</param>
    /// <param name="amount">숫자</param>
    public static void Publish(string itemId, int amount)
    {
        EventBus<OnPlayerGetItem>.Publish(new OnPlayerGetItem(itemId, amount));
    }
}
