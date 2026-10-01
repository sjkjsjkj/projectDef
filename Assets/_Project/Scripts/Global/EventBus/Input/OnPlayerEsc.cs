/// <summary>
/// 플레이어가 Esc 키를 눌렀을 때
/// 키를 눌렀을 때 1회 발행
/// </summary>
public readonly struct OnPlayerEsc
{
    public static void Publish()
    {
        EventBus<OnPlayerEsc>.Publish(new OnPlayerEsc());
    }
}
