/// <summary>
/// 매니저에서 팩토리를 관리하기 위한 인터페이스
/// </summary>
public interface IPoolFactory
{
    BaseMono Spawn();
    void Despawn(BaseMono instance);
    void Clear();
}
