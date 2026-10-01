/// <summary>
/// 인벤토리의 아이템이 변경되었을 때
/// </summary>
public readonly struct OnInventoryItemChanged
{
    public readonly int slot;
    public readonly string id;
    public readonly int amount;

    public OnInventoryItemChanged(int slot, string id, int amount)
    {
        this.slot = slot;
        this.id = id;
        this.amount = amount;
    }

    /// <param name="slot">슬롯 번호</param>
    /// <param name="id">아이템 ID</param>
    /// <param name="amount">현재 수량</param>
    public static void Publish(int slot, string id, int amount)
    {
        EventBus<OnInventoryItemChanged>.Publish(new(slot, id, amount));
    }
}
