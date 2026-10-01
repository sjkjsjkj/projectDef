using UnityEngine;

/// <summary>
/// 플레이어가 WASD를 입력했을 때
/// 키를 누를 때마다 이벤트 발행 (캐싱 필요) → 매 프레임 이동 입력 상태를 발행으로 수정
/// </summary>
public readonly struct OnPlayerMove
{
    public readonly Vector2 moved;

    public OnPlayerMove(Vector2 moved)
    {
        this.moved = moved;
    }

    /// <param name="moved">이동량</param>
    public static void Publish(Vector2 moved)
    {
        EventBus<OnPlayerMove>.Publish(new OnPlayerMove(moved));
    }
}
