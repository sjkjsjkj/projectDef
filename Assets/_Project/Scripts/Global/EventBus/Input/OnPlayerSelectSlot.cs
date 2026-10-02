/// <summary>
/// 플레이어가 퀵슬롯을 변경했을 때
/// 퀵슬롯을 변경했을 때 1회 발행
/// 1번부터 0번까지 순서대로 숫자 0부터 9까지 가집니다.
/// </summary>
public readonly struct OnPlayerSelectSlot
{
    public readonly int slot;

    public OnPlayerSelectSlot(int slot)
    {
        this.slot = slot;
    }

    public static void Publish(int slot)
    {
        EventBus<OnPlayerSelectSlot>.Publish(new OnPlayerSelectSlot(slot));
    }
}
