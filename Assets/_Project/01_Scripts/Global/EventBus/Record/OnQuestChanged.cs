/// <summary>
/// 목표가 변경되었을 때
/// </summary>
public readonly struct OnQuestChanged
{
    public readonly int newIndex;
    public readonly string title;

    public OnQuestChanged(int newIndex, string title)
    {
        this.newIndex = newIndex;
        this.title = title;
    }

    /// <param name="newIndex">새로운 퀘스트 인덱스</param>
    /// <param name="title">퀘스트 제목</param>
    public static void Publish(int newIndex, string title)
    {
        EventBus<OnQuestChanged>.Publish(new OnQuestChanged(newIndex, title));
    }
}
