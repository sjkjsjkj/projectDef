using UnityEngine;

/// <summary>지연 타격이 풀에서 재사용된 다른 적에게 적용되지 않도록 생성 회차를 함께 보관합니다.</summary>
public readonly struct AbilityTargetHandle
{
    public readonly IDamageable Target;
    private readonly EnemyHealth _enemy;
    private readonly uint _spawnVersion;
    private readonly Transform _transform;

    public AbilityTargetHandle(IDamageable target)
    {
        Target = target;
        _enemy = target as EnemyHealth;
        _spawnVersion = _enemy != null ? _enemy.SpawnVersion : 0;
        _transform = target != null && (!(target is Object obj) || obj != null) ? target.TargetTransform : null;
    }

    public bool IsValid => Target != null && (!(Target is Object obj) || obj != null) &&
        _transform != null && _transform.gameObject.activeInHierarchy && Target.IsAlive &&
        (_enemy == null || _enemy.SpawnVersion == _spawnVersion);
    public Vector3 Position => _transform != null ? _transform.position : Vector3.zero;

    public Vector3 ContactPoint(Vector3 from)
    {
        if (_transform != null && _transform.TryGetComponent(out Collider2D collider) && collider.enabled)
            return collider.ClosestPoint(from);
        return Position;
    }
}
