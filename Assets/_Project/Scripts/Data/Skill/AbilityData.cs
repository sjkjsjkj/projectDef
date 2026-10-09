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

    [Header("스킬 조건 및 효과")]
    [SerializeField] private List<SkillCondition> useConditions = new();
    [SerializeField] private List<SkillCondition> activationConditions = new();
    [SerializeField] private List<SkillEffect> effects = new();
    #endregion

    #region ─────────────────────────▶ 공개 멤버 ◀─────────────────────────
    public float Range => range;
    public float Cooldown => cooldown;
    public LayerMask TargetLayer => targetLayer;

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

    public virtual float GetCooldown(AbilityContext context)
    {
        return cooldown;
    }

    public virtual bool TryExecute(AbilityContext context)
    {
        if (context == null)
            return false;

        context.TargetBuffer.Clear();

        if (context.Owner == null || context.Origin == null || targetSelector == null)
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

        foreach (var effect in effects)
        {
            effect.Apply(context);
        }

        return true;
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
