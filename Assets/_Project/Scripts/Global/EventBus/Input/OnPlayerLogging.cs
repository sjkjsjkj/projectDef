using UnityEngine;

/// <summary>
/// 플레이어가 나무에 상호작용했을 때
/// </summary>
public readonly struct OnPlayerLogging
{
    public readonly Vector2 woodPos;
    public readonly float duration;

    public OnPlayerLogging(Vector2 woodPos, float duration)
    {
        this.woodPos = woodPos;
        this.duration = duration;
    }

    /// <param name="woodPos">나무 좌표</param>
    /// <param name="duration">애니메이션 지속시간</param>
    public static void Publish(Vector2 woodPos, float duration)
    {
        EventBus<OnPlayerLogging>.Publish(new OnPlayerLogging(woodPos, duration));
    }
}
