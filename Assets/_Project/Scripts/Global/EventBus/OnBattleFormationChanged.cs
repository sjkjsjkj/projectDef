using UnityEngine.SceneManagement;

/// <summary>전투 슬롯의 배치가 변경되었습니다. 교환 중 여러 번 발행될 수 있으므로 프레임 끝에 조회합니다.</summary>
public readonly struct OnBattleFormationChanged
{
    public readonly Scene Scene;

    public OnBattleFormationChanged(Scene scene) => Scene = scene;

    public static void Publish(Scene scene)
    {
        EventBus<OnBattleFormationChanged>.Publish(new OnBattleFormationChanged(scene));
    }
}
