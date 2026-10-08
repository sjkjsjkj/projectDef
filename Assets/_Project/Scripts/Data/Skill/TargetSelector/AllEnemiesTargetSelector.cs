using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 범위 내의 모든 대상을 타겟하는 타겟 셀렉터
/// </summary>
[CreateAssetMenu(fileName = "AllEnemiesTarget_", menuName = "ScriptableObjects/Combat/TargetSelector/AllEnemiesTargetSelector", order = 2)]
public class AllEnemiesTargetSelector : TargetSelector
{
    #region ─────────────────────────▶ 공개 멤버 ◀─────────────────────────
    public override void SelectTargets(
        AbilityContext context,
        AbilityData data,
        List<IDamageable> results)
    {
        var position = context.Origin.position;
        var range = data.GetRange(context.OwnerStats);

        var colliders = Physics2D.OverlapCircleAll(
            position,
            range,
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
