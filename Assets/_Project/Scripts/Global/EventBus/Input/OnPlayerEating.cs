/// <summary>
/// 플레이어가 음식을 섭취하기 시작했을 때
/// </summary>
public readonly struct OnPlayerEating
{
    public readonly float eatingTime;

    public OnPlayerEating(float eatingTime)
    {
        this.eatingTime = eatingTime;
    }

    /// <param name="eatingTime">애니메이션 지속 시간</param>
    public static void Publish(float eatingTime)
    {
        EventBus<OnPlayerEating>.Publish(new OnPlayerEating(eatingTime));
    }
}
