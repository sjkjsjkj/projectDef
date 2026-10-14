using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>비활성 상태로 생성/초기화하며, 대여와 반환을 중복 없이 관리합니다.</summary>
public class ObjectPool<T> : IDisposable where T : BaseMono, IPoolable
{
    private readonly Queue<T> _available = new Queue<T>();
    private readonly HashSet<T> _owned = new HashSet<T>();
    private readonly HashSet<T> _leased = new HashSet<T>();
    private readonly T _prefab;
    private readonly Transform _root;
    private readonly Transform _storage;
    private bool _useOverFlow;
    private bool _disposed;

    public ObjectPool(T prefab, bool useOverFlow = true, int capacity = 10, Transform parent = null)
    {
        if (prefab == null) throw new ArgumentNullException(nameof(prefab));
        if (capacity < 0) throw new ArgumentOutOfRangeException(nameof(capacity));
        _prefab = prefab;
        _root = parent;
        _useOverFlow = useOverFlow;
        var storage = new GameObject(prefab.name + "_Inactive");
        storage.SetActive(false);
        storage.transform.SetParent(parent, false);
        _storage = storage.transform;
        for (int i = 0; i < capacity; i++) _available.Enqueue(CreateInstance());
    }

    public void SetOverFlow(bool active) => _useOverFlow = active;

    /// <summary>대기 객체 수를 조절합니다. 사용 중인 객체는 유지합니다.</summary>
    public void SetCapacity(int capacity)
    {
        if (capacity < 0) throw new ArgumentOutOfRangeException(nameof(capacity));
        if (_disposed) return;
        while (_available.Count > capacity)
        {
            T instance = _available.Dequeue();
            _owned.Remove(instance);
            if (instance != null) DestroyObject(instance.gameObject);
        }
        while (_available.Count < capacity) _available.Enqueue(CreateInstance());
    }

    public void Push(T instance)
    {
        // OnDisable에서 다시 반환해도 한 번만 처리합니다. 다른 풀의 객체도 받지 않습니다.
        if (_disposed || instance == null || !_leased.Remove(instance)) return;
        instance.gameObject.SetActive(false);
        if (instance == null) return;
        // 부모 비활성화 중 OnDisable에서 반환될 수 있어 여기서는 계층을 바꾸지 않습니다.
        _available.Enqueue(instance);
    }

    public bool Pull(out T instance)
    {
        if (!PullInactive(out instance)) return false;
        instance.gameObject.SetActive(true);
        return true;
    }

    /// <summary>Setup 후 비활성 객체를 반환합니다. 호출자가 데이터 주입 후 활성화합니다.</summary>
    public bool PullInactive(out T instance)
    {
        instance = null;
        if (_disposed) return false;
        while (_available.Count > 0)
        {
            T candidate = _available.Dequeue();
            if (candidate != null) { instance = candidate; break; }
            _owned.Remove(candidate);
        }
        if (instance == null)
        {
            if (!_useOverFlow) return false;
            instance = CreateInstance();
        }
        _leased.Add(instance);
        instance.transform.SetParent(_root, false);
        instance.transform.localScale = _prefab.transform.localScale;
        try { instance.Setup(); }
        catch { Push(instance); throw; }
        return true;
    }

    private T CreateInstance()
    {
        T instance = UnityEngine.Object.Instantiate(_prefab, _storage);
        instance.gameObject.SetActive(false);
        _owned.Add(instance);
        return instance;
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        foreach (T instance in _owned)
        {
            if (instance == null) continue;
            instance.gameObject.SetActive(false);
            DestroyObject(instance.gameObject);
        }
        _owned.Clear();
        _leased.Clear();
        _available.Clear();
        if (_storage != null) DestroyObject(_storage.gameObject);
    }

    private static void DestroyObject(GameObject instance)
    {
        if (Application.isPlaying) UnityEngine.Object.Destroy(instance);
        else UnityEngine.Object.DestroyImmediate(instance);
    }
}
