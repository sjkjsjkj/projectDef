/// <summary>
/// 플레이어가 무언가를 채집 완료했을 때
/// </summary>
public readonly struct OnPlayerCollected
{
    public readonly string fieldId;

    public OnPlayerCollected(string fieldId)
    {
        this.fieldId = fieldId;
    }

    /// <param name="fieldId">채집물 Id</param>
    public static void Publish(string fieldId)
    {
        EventBus<OnPlayerCollected>.Publish(new OnPlayerCollected(fieldId));
    }
}
