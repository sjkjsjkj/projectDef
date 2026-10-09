using UnityEngine;
/// <summary>
/// 공격자와 피격자의 시스템 연결점.
/// </summary>
public interface IDamageable
{
    bool IsAlive { get; }

    Transform TargetTransform { get; }

    void TakeDamage(float damage);
}
