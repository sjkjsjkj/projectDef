using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 오브젝트를 풀링으로 저장 및 관리해주는 추상화 클래스
/// </summary>
public class ObjectPool<T> where T : BaseMono, IPoolable
{
    #region ─────────────────────────▶ 내부 변수 ◀─────────────────────────
    private Queue<T> _pool;
    private T _prefab;
    private bool _useOverFlow; // 동적 생성을 허용하는가?
    #endregion

    #region ─────────────────────────▶ 공개 멤버 ◀─────────────────────────
    // 생성자
    public ObjectPool(T prefab, bool useOverFlow = true, int capacity = 10)
    {
        Initialize(prefab, useOverFlow, capacity);
    }

    /// <summary>
    /// 크기 초과시 동적 생성 허용 여부를 결정합니다.
    /// </summary>
    public void SetOverFlow(bool active) => _useOverFlow = active;

    /// <summary>
    /// 풀 크기를 개발자가 직접 재설정합니다.
    /// </summary>
    public void SetCapacity(int capacity)
    {
        // 새로운 풀 생성
        Queue<T> newPool = new(capacity);
        int activeCount = _pool.Count;
        int length = activeCount > capacity ? capacity : activeCount; // 기존 크기보다 축소되는 경우를 대비
        // 새로운 풀로 인스턴스 옮기기
        for (int i = 0; i < length; ++i)
        {
            T instance = _pool.Dequeue();
            newPool.Enqueue(instance);
        }
        // 새로운 풀에 들어가지 못한 인스턴스는 삭제
        while(_pool.Count > 0)
        {
            T instance = _pool.Dequeue();
            UObject.Destroy(instance);
        }
        // 마무리 → 주소 덮어씌우고 기존 풀은 가비지가 되게 하기
        _pool = newPool;
    }

    /// <summary>
    /// 오브젝트를 풀에 넣습니다.
    /// </summary>
    public void Push(T instance)
    {
        if(instance == null)
        {
            UDebug.Print($"오브젝트 풀({(_prefab == null ? null : _prefab.name)})에서 비어있는 인스턴스를 푸쉬받았습니다.", LogType.Warning);
            return;
        }
        _pool.Enqueue(instance);
        instance.gameObject.SetActive(false);
    }

    /// <summary>
    /// 오브젝트를 풀에서 꺼냅니다.
    /// </summary>
    public bool Pull(out T instance)
    {
        // 꺼내기 시도
        while(_pool.TryDequeue(out instance))
        {
            if(instance != null)
            {
                instance.gameObject.SetActive(true);
                return true;
            }
        }
        // 동적 생성이 허용
        if (_useOverFlow)
        {
            instance = CreateInstance();
            return true;
        }
        // 꺼낼 수 없음
        instance = null;
        UDebug.Print($"오브젝트 풀({_prefab.name})에서 인스턴스를 꺼내지 못했습니다.", LogType.Warning);
        return false;
    }
    #endregion

    #region ─────────────────────────▶ 내부 메서드 ◀─────────────────────────
    // 프리펩을 생성하여 반환합니다.
    private T CreateInstance()
    {
        if(_prefab == null)
        {
            UDebug.Print($"오브젝트 풀에서 프리펩이 없는 상태에서 인스턴스 생성을 시도했습니다.", LogType.Warning);
            return null;
        }
        T instance = UObject.Spawn(_prefab, Vector3.zero, Quaternion.identity, GameManager.ObjectRoot);
        instance.gameObject.SetActive(true); // 비활성화되어있는 프리펩일 경우를 위한 활성화 코드
        return instance;
    }

    // 오브젝트 풀링의 초기 세팅 → 크기만큼 오브젝트를 생성해둡니다.
    private void Initialize(T prefab, bool useOverFlow, int capacity)
    {
        this._prefab = prefab;
        this._pool = new(capacity);
        this._useOverFlow = useOverFlow;
        // 초기 크기만큼 인스턴스 생성
        for (int i = 0; i < capacity; ++i)
        {
            T instance = CreateInstance();
            Push(instance);
        }
    }
    #endregion
}
