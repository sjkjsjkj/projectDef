using UnityEngine;

/// <summary>
/// SO 클래스의 설계 의도입니다.
/// </summary>
[CreateAssetMenu(fileName = "DamageEffect_", menuName = "ScriptableObjects/Combat/SkillEffect/DamageEffect", order = 2)]
public class DamageEffect : SkillEffect
{
    #region ─────────────────────────▶ 인스펙터 ◀─────────────────────────
    [Header("데미지 비율")]
    [SerializeField] private float damageMultiplier = 1f;
    #endregion

    #region ─────────────────────────▶ 공개 멤버 ◀─────────────────────────
    public override void Apply(AbilityContext context)
    {
        var damage =
            context.OwnerStats.AttackPower *
            damageMultiplier;

        foreach (var target in context.Targets)
        {
            if (target == null || !target.IsAlive)
                continue;

            target.TakeDamage(damage);
        }
    }
    #endregion
}
