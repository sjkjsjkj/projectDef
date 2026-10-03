using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// SO 클래스의 설계 의도입니다.
/// </summary>
[CreateAssetMenu(fileName = "AllEnemiesTargetSelector_", menuName = "ScriptableObjects/Item/AllEnemiesTargetSelector", order = 2)]
public class AllEnemiesTargetSelector : TargetSelector
{
    #region ─────────────────────────▶ 공개 멤버 ◀─────────────────────────
    public override void SelectTargets(
        AbilityContext context,
        List<IDamageable> results)
    {
        var position = context.Origin.position;
        var data = context.Ability.Data;

        var colliders = Physics2D.OverlapCircleAll(
            position,
            data.Range,
            data.TargetLayer);

        foreach (var collider in colliders)
        {
            var target = collider.GetComponentInParent<IDamageable>();

            if (target == null || !target.IsAlive)
                continue;

            if (results.Contains(target))
                continue;

            results.Add(target);
        }
    }
    #endregion
}
