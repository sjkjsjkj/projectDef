/// <summary>
/// 풀링 객체를 생성 및 반환하는 팩토리 클래스
/// </summary>
public sealed class PoolFactory<T> : IPoolFactory where T : BaseMono, IPoolable
{
    private ObjectPool<T> _pool;

    /// <summary>
    /// 팩토리의 생성자입니다.
    /// </summary>
    /// <param name="prefab">프리펩</param>
    /// <param name="capacity">초기 풀 크기</param>
    public PoolFactory(T prefab, int capacity)
    {
        _pool = new ObjectPool<T>(prefab, true, capacity);
    }

    /// <summary>
    /// 객체를 스폰하고 초기화 함수를 실행합니다.
    /// </summary>
    public BaseMono Spawn()
    {
        if (_pool.Pull(out T instance))
        {
            instance.Setup();
            instance.transform.SetParent(GameManager.EnableObjectRoot);
            return instance;
        }
        return null;
    }

    /// <summary>
    /// 객체를 풀로 반환합니다.
    /// </summary>
    public void Despawn(BaseMono instance)
    {
        if (instance is T ins)
        {
            _pool.Push(ins);
            instance.transform.SetParent(GameManager.DisableObjectRoot);
        }
        else
        {
            UObject.Destroy(instance);
        }
    }

    /// <summary>
    /// 팩토리를 정리합니다.
    /// </summary>
    public void Clear()
    {
        _pool = null;
    }
}
