using UnityEngine;
/// <summary>
/// 
/// </summary>
public interface IDamageable
{
    bool IsAlive { get; }

    Transform TargetTransform { get; }

    void TakeDamage(float damage);
}
