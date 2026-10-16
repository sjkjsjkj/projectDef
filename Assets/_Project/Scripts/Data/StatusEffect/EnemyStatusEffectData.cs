using UnityEngine;

public enum EnemyStatusEffectType
{
    Poison = 0,
    Bleeding = 1,
    Burn = 2,
    Paralysis = 3,
    Chill = 4,
    Fracture = 5,
    Freeze = 6,
    Stun = 7
}

/// <summary>현재 적에게 사용하는 8종 상태이상. 공통 실행기는 이 열거형에 의존하지 않습니다.</summary>
[CreateAssetMenu(fileName = "StatusEffect_", menuName = "ScriptableObjects/Combat/StatusEffect/EnemyStatusEffect")]
public sealed class EnemyStatusEffectData : StatusEffectData
{
    [SerializeField] private EnemyStatusEffectType effectType;
    [Tooltip("독/출혈: 틱당 고정 피해. 화상: 직접 피격당 고정 추가 피해.")]
    [Min(0f)] [SerializeField] private float damage = 5f;
    [Tooltip("오한/골절: 0.3이면 30% 감속. 서로 다른 감속의 배율은 곱합니다.")]
    [Range(0f, 1f)] [SerializeField] private float slowRatio = 0.3f;

    public EnemyStatusEffectType EffectType => effectType;
    public override string Id => base.Id == GetType().Name ? "EnemyStatus." + effectType : base.Id;
    public override bool UsesPeriodicTicks => effectType == EnemyStatusEffectType.Poison ||
        effectType == EnemyStatusEffectType.Bleeding || effectType == EnemyStatusEffectType.Paralysis;
    public override float MovementSpeedMultiplier =>
        effectType == EnemyStatusEffectType.Chill || effectType == EnemyStatusEffectType.Fracture
            ? 1f - slowRatio : 1f;
    public override bool PreventsMovement => effectType == EnemyStatusEffectType.Freeze ||
        effectType == EnemyStatusEffectType.Stun;
    public override float ExtraHitDamage => effectType == EnemyStatusEffectType.Burn ? damage : 0f;

    public override void OnTick(IStatusEffectTarget target, StatusEffectInstance instance)
    {
        switch (effectType)
        {
            case EnemyStatusEffectType.Poison:
            case EnemyStatusEffectType.Bleeding:
                target.TakeStatusDamage(damage);
                break;
            case EnemyStatusEffectType.Paralysis:
                instance.StopMovementFor(0.1f);
                break;
        }
    }

    public override bool IsValid()
    {
        return base.IsValid() && System.Enum.IsDefined(typeof(EnemyStatusEffectType), effectType) &&
            IsFinite(damage) && damage >= 0f && IsFinite(slowRatio) && slowRatio >= 0f && slowRatio <= 1f;
    }
}
