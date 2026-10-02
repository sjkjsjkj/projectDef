using UnityEngine;

/// <summary>
/// 플레이어가 낚시 포인트에 상호작용했을 때
/// </summary>
public readonly struct OnPlayerFishing
{
    public readonly Vector2 fishingPointPos;
    public readonly float duration;
    public readonly bool isSuccess;

    public OnPlayerFishing(Vector2 fishingPointPos, float duration, bool isSuccess)
    {
        this.fishingPointPos = fishingPointPos;
        this.duration = duration;
        this.isSuccess = isSuccess;
    }

    /// <param name="fishingPointPos">플레이어가 바라볼 낚시터 좌표</param>
    /// <param name="duration">애니메이션 길이</param>
    /// <param name="isSuccess">낚시 성공 여부</param>
    public static void Publish(Vector2 fishingPointPos, float duration, bool isSuccess)
    {
        EventBus<OnPlayerFishing>.Publish(new OnPlayerFishing(fishingPointPos, duration, isSuccess));
    }
}
