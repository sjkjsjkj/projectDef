using UnityEngine;

/// <summary>
/// 플레이어가 물뿌리개를 사용했을 때
/// </summary>
public readonly struct OnPlayerWatering
{
    public readonly Vector2 targetPos;
    public readonly float duration;

    public OnPlayerWatering(Vector2 targetPos, float duration)
    {
        this.targetPos = targetPos;
        this.duration = duration;
    }

    /// <param name="targetPos">타겟 좌표</param>
    /// <param name="duration">애니메이션 지속시간</param>
    public static void Publish(Vector2 targetPos, float duration)
    {
        EventBus<OnPlayerWatering>.Publish(new OnPlayerWatering(targetPos, duration));
    }
}
