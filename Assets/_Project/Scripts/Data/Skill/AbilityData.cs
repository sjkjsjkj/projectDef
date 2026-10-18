using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Ability의 공유 설정과 실행 규칙입니다. 유닛별 상태는 저장하지 않습니다.
/// </summary>
[CreateAssetMenu(fileName = "AbilityData_", menuName = "ScriptableObjects/Combat/AbilityData", order = 2)]
public class AbilityData : BaseSO
{
    #region ─────────────────────────▶ 인스펙터 ◀─────────────────────────
    [Header("스킬 정보")]
    [SerializeField] private string skillName;
    [SerializeField] private string skillDescription;
    [SerializeField] private Sprite image;

    [SerializeField] EAbilityRangeType rangeType;

    [Header("스킬 스펙")]
    [SerializeField] private float range = 3f;
    [SerializeField] private float cooldown = 1f;
    [SerializeField] private LayerMask targetLayer;

    [SerializeField] private TargetSelector targetSelector;

    [Header("공격 전달 방식")]
    [SerializeField] private AttackDeliveryType deliveryType = AttackDeliveryType.Melee;
    [Tooltip("근거리: 베기 등 공격 이미지. 원거리: 대상에게 날아가는 투사체 이미지. 오른쪽(+X)을 향한 이미지를 사용합니다.")]
    [SerializeField] private Sprite attackEffect;
    [SerializeField] private Sprite hitEffect;
    [SerializeField, Min(0.01f)] private float attackEffectSize = 0.7f;
    [SerializeField, Min(0.01f)] private float hitEffectSize = 0.35f;
    [Tooltip("근거리 공격 이미지를 발사 위치에서 대상 방향으로 띄울 거리입니다.")]
    [SerializeField, Min(0f)] private float meleeEffectOffset = 0.55f;
    [Tooltip("근거리 공격 이펙트가 끝난 뒤 피격 이펙트를 시작합니다.")]
    [SerializeField, Min(0f)] private float meleeAttackDuration = 0.08f;
    [Tooltip("피격 이펙트를 표시한 뒤 효과(피해/상태이상)를 적용하기까지의 시간입니다.")]
    [SerializeField, Min(0f)] private float hitEffectDuration = 0.06f;
    [SerializeField, Min(0.01f)] private float projectileSpeed = 8f;
    [SerializeField, Min(0.1f)] private float projectileLifetime = 5f;
    [SerializeField, Min(0f)] private float projectileHitRadius = 0.05f;
    [SerializeField] private Vector2 attackOriginOffset = new Vector2(0f, 0.45f);
    [SerializeField] private float attackImageAngle;

    [Header("스킬 조건 및 효과")]
    [SerializeField] private List<SkillCondition> useConditions = new();
    [SerializeField] private List<SkillCondition> activationConditions = new();
    [SerializeField] private List<SkillEffect> effects = new();
    #endregion

    #region ─────────────────────────▶ 공개 멤버 ◀─────────────────────────
    public string SkillName => skillName;
    public string SkillDescription => skillDescription;
    public Sprite Image => image;
    public float Range => range;
    public float Cooldown => cooldown;
    public LayerMask TargetLayer => targetLayer;
    public AttackDeliveryType DeliveryType => deliveryType;
    public Sprite AttackEffect => attackEffect;
    public Sprite HitEffect => hitEffect;
    public float AttackEffectSize => Mathf.Max(0.01f, attackEffectSize);
    public float HitEffectSize => Mathf.Max(0.01f, hitEffectSize);
    public float MeleeEffectOffset => Mathf.Max(0f, meleeEffectOffset);
    public float MeleeAttackDuration => Mathf.Max(0f, meleeAttackDuration);
    public float HitEffectDuration => Mathf.Max(0f, hitEffectDuration);
    public float ProjectileSpeed => Mathf.Max(0.01f, projectileSpeed);
    public float ProjectileLifetime => Mathf.Max(0.1f, projectileLifetime);
    public float ProjectileHitRadius => Mathf.Max(0f, projectileHitRadius);
    public Vector2 AttackOriginOffset => attackOriginOffset;
    public float AttackImageAngle => attackImageAngle;

    public TargetSelector TargetSelector => targetSelector;
    public IReadOnlyList<SkillCondition> UseConditions => useConditions;
    public IReadOnlyList<SkillCondition> ActivationConditions => activationConditions;
    public IReadOnlyList<SkillEffect> Effects => effects;

    public float GetRange(UnitStats ownerStats)
    {
        return rangeType switch
        {
            EAbilityRangeType.OwnerAttackRange => ownerStats.AttackRange,
            _ => range
        };
    }

    public AbilityInstance CreateInstance(AbilityContext context)
    {
        return new AbilityInstance(this, context);
    }

    /// <summary>같은 SO를 기본 공격/스킬이 공유해도 각 실행 문맥의 강화만 적용합니다.</summary>
    public float GetEffectiveRange(AbilityContext context)
    {
        float baseRange = rangeType == EAbilityRangeType.OwnerAttackRange
            ? context.OwnerStats.RangeBeforeMeta : range;
        return Mathf.Max(0f, baseRange * (context.IsBasicAttack
            ? context.OwnerStats.BasicRangeMultiplier : context.OwnerStats.SkillRangeMultiplier));
    }

    public virtual float GetCooldown(AbilityContext context)
    {
        return cooldown;
    }

    public virtual bool TryExecute(AbilityContext context)
    {
        if (context == null)
            return false;

        context.TargetBuffer.Clear();

        if (context.Owner == null || context.Owner.IsBeingDragged || context.Origin == null || targetSelector == null)
            return false;

        // 불완전한 효과 설정으로 쿨다운만 소비하거나 일부 효과만 실행하지 않습니다.
        if (effects == null || effects.Count == 0)
            return false;

        foreach (var effect in effects)
        {
            if (effect == null)
                return false;
        }

        if (!AreConditionsSatisfied(useConditions, context))
            return false;

        targetSelector.SelectTargets(context, this, context.TargetBuffer);

        if (!AreConditionsSatisfied(activationConditions, context))
            return false;

        // SO와 재사용 문맥에 실행 상태를 남기지 않고, 공격마다 독립된 대상을 보관합니다.
        return AbilityExecutionRunner.ForScene(context.Owner.gameObject.scene).TryLaunch(this, context);
    }

    // 값 유효성 검사
    public override bool IsValid()
    {
        base.IsValid();
        if (skillName.IsEmpty()) return false;
        if (skillDescription.IsEmpty()) return false;
        //if (image == null) return false;

        return true;
    }
    #endregion

    #region ─────────────────────────▶ 내부 메서드 ◀─────────────────────────
    private static bool AreConditionsSatisfied(
        IReadOnlyList<SkillCondition> conditions,
        AbilityContext context)
    {
        if (conditions == null)
            return true;

        foreach (var condition in conditions)
        {
            if (condition == null || !condition.IsSatisfied(context))
                return false;
        }

        return true;
    }
    #endregion

    #region ─────────────────────────▶ 메시지 함수 ◀─────────────────────────
    protected override void OnValidate()
    {
        base.OnValidate();
        if (!IsValid())
        {
            UDebug.PrintOnce($"SO 인스턴스({this.name})의 값이 올바르지 않습니다. (ID = {id}, Type = {this.GetType().Name})", LogType.Warning);
        }
    }
    #endregion
}
