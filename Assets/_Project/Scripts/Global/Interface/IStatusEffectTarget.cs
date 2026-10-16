/// <summary>상태이상 실행에 필요한 대상 계약. 이후 아군 버프 수신기도 구현할 수 있습니다.</summary>
public interface IStatusEffectTarget
{
    bool IsStatusEffectTargetAlive { get; }
    void TakeStatusDamage(float damage);
}
