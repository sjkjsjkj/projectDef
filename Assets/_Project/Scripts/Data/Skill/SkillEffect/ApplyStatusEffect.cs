using UnityEngine;

/// <summary>선택된 대상의 공통 효과 수신기에 상태이상을 부여합니다.</summary>
[CreateAssetMenu(fileName = "ApplyStatusEffect_", menuName = "ScriptableObjects/Combat/SkillEffect/ApplyStatusEffect", order = 2)]
public sealed class ApplyStatusEffect : SkillEffect
{
    [SerializeField] private StatusEffectData statusEffect;

    public override void Apply(AbilityContext context)
    {
        if (context == null || statusEffect == null || !statusEffect.IsValid()) return;
        foreach (IDamageable target in context.Targets)
        {
            if (target == null || !target.IsAlive || target.TargetTransform == null) continue;
            if (target.TargetTransform.TryGetComponent(out StatusEffectController controller))
                controller.TryApply(statusEffect);
        }
    }

    public override bool IsValid() => base.IsValid() && statusEffect != null && statusEffect.IsValid();
}
