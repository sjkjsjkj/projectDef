/// <summary>
/// 플레이어의 현재 애니메이션을 취소시킬 때
/// </summary>
public readonly struct OnPlayerCanceled
{
    public static void Publish()
    {
        EventBus<OnPlayerCanceled>.Publish(new OnPlayerCanceled());
    }
}
