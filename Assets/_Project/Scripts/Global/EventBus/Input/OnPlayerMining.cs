using UnityEngine;

/// <summary>
/// 플레이어가 광석에 상호작용했을 때
/// </summary>
public readonly struct OnPlayerMining
{
    public readonly Vector2 orePos;
    public readonly float duration;

    public OnPlayerMining(Vector2 orePos, float duration)
    {
        this.orePos = orePos;
        this.duration = duration;
    }

    /// <param name="orePos">나무 좌표</param>
    /// <param name="duration">지속시간</param>
    public static void Publish(Vector2 orePos, float duration)
    {
        EventBus<OnPlayerMining>.Publish(new OnPlayerMining(orePos, duration));
    }
}
