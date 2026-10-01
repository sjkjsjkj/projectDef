using UnityEngine;

/// <summary>
/// 플레이어가 씨앗을 심을 때
/// </summary>
public readonly struct OnPlayerCrouching
{
    public readonly Vector2 targetPos;
    public readonly float duration;

    public OnPlayerCrouching(Vector2 targetPos, float duration)
    {
        this.targetPos = targetPos;
        this.duration = duration;
    }

    /// <param name="targetPos">타겟 좌표</param>
    /// <param name="duration">애니메이션 지속시간</param>
    public static void Publish(Vector2 targetPos, float duration)
    {
        EventBus<OnPlayerCrouching>.Publish(new OnPlayerCrouching(targetPos, duration));
    }
}
