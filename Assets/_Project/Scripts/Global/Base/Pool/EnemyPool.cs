using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>스포너 수명에 종속된 프리팹별 적 풀. 서로 다른 보스 프리팹은 섞지 않습니다.</summary>
public sealed class EnemyPool : IDisposable
{
    private readonly Dictionary<EnemyMovement, ObjectPool<EnemyMovement>> _pools =
        new Dictionary<EnemyMovement, ObjectPool<EnemyMovement>>();
    private readonly Transform _root;
    private bool _disposed;

    public EnemyPool(Transform parent)
    {
        _root = new GameObject("EnemyPool").transform;
        _root.SetParent(parent, false);
    }

    public EnemyMovement Rent(EnemyMovement prefab)
    {
        if (_disposed || prefab == null) return null;
        if (!_pools.TryGetValue(prefab, out ObjectPool<EnemyMovement> pool))
        {
            pool = new ObjectPool<EnemyMovement>(prefab, true, 0, _root);
            _pools.Add(prefab, pool);
        }
        if (!pool.PullInactive(out EnemyMovement enemy)) return null;
        enemy.SetPoolReturn(pool.Push);
        return enemy;
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        foreach (ObjectPool<EnemyMovement> pool in _pools.Values) pool.Dispose();
        _pools.Clear();
        if (_root == null) return;
        if (Application.isPlaying) UnityEngine.Object.Destroy(_root.gameObject);
        else UnityEngine.Object.DestroyImmediate(_root.gameObject);
    }
}
