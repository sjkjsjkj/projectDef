/// <summary>
/// 낮 또는 밤이 되었을 때
/// </summary>
public readonly struct OnTimeChanged
{
    public readonly bool isDay;

    public OnTimeChanged(bool isDay)
    {
        this.isDay = isDay;
    }

    /// <param name="isDay">낮?</param>
    public static void Publish(bool isDay)
    {
        EventBus<OnTimeChanged>.Publish(new(isDay));
    }
}
