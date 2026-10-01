/// <summary>
/// 플레이어가 음료를 마시기 시작했을 때
/// </summary>
public readonly struct OnPlayerDrinking
{
    public readonly float drinkingTime;

    public OnPlayerDrinking(float drinkingTime)
    {
        this.drinkingTime = drinkingTime;
    }

    /// <param name="drinkingTime">애니메이션 지속시간</param>
    public static void Publish(float drinkingTime)
    {
        EventBus<OnPlayerDrinking>.Publish(new OnPlayerDrinking(drinkingTime));
    }
}
