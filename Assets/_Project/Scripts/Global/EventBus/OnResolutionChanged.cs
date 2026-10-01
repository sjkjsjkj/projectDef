/// <summary>
/// 해상도가 변경되었을 경우
/// </summary>
public readonly struct OnResolutionChanged
{
    public readonly int x;
    public readonly int y;

    public OnResolutionChanged(int x, int y)
    {
        this.x = x;
        this.y = y;
    }

    /// <param name="x">가로 픽셀</param>
    /// <param name="y">세로 픽셀</param>
    public static void Publish(int x, int y)
    {
        EventBus<OnResolutionChanged>.Publish(new OnResolutionChanged(x, y));
    }
}
