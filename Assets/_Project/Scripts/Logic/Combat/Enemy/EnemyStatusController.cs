using UnityEngine;

/// <summary>공통 효과 실행기와 적의 체력/이동을 연결하는 고정 컴포넌트입니다.</summary>
[DisallowMultipleComponent]
public sealed class EnemyStatusController : StatusEffectController
{
    private EnemyHealth _health;
    private EnemyHealth Health => _health != null ? _health : (_health = GetComponent<EnemyHealth>());

    public override bool IsStatusEffectTargetAlive => isActiveAndEnabled && Health != null && Health.IsAlive;

    public override void TakeStatusDamage(float damage)
    {
        if (IsStatusEffectTargetAlive) Health.TakeStatusDamage(damage);
    }
}
