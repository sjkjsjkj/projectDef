using System.Collections.Generic;
using UnityEngine;

/// <summary>프리팹별 파티클 풀입니다. 전투 씬에 배치하고 Play로 재생합니다.</summary>
public class VFXPool : BaseMono
{
    private readonly Dictionary<VFXEmitter, ObjectPool<VFXEmitter>> _pools =
        new Dictionary<VFXEmitter, ObjectPool<VFXEmitter>>();

    public VFXEmitter Play(VFXEmitter prefab, Vector3 position, Quaternion rotation)
    {
        if (!isActiveAndEnabled || prefab == null || !prefab.enabled) return null;
        if (!_pools.TryGetValue(prefab, out ObjectPool<VFXEmitter> pool))
        {
            pool = new ObjectPool<VFXEmitter>(prefab, true, 0, transform);
            _pools.Add(prefab, pool);
        }
        if (!pool.PullInactive(out VFXEmitter effect)) return null;
        effect.SetPoolReturn(pool.Push);
        effect.transform.SetPositionAndRotation(position, rotation);
        effect.gameObject.SetActive(true);
        effect.Play();
        return effect;
    }

    public void Clear()
    {
        foreach (ObjectPool<VFXEmitter> pool in _pools.Values) pool.Dispose();
        _pools.Clear();
    }

    private void OnDisable() => Clear();
    private void OnDestroy() => Clear();
}
