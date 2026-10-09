using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 가장 가까운 적 1개를 대상으로 하는 타겟 셀렉터
/// </summary>
[CreateAssetMenu(fileName = "NearestEnemyTarget_", menuName = "ScriptableObjects/Combat/TargetSelector/NearestEnemyTargetSelector", order = 2)]
public class NearestEnemyTargetSelector : TargetSelector
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

        IDamageable nearestTarget = null;
        var nearestDistance = float.MaxValue;

        foreach (var collider in colliders)
        {
            var target = collider.GetComponentInParent<IDamageable>();

            if (target == null || !target.IsAlive)
                continue;

            var distance = (
                target.TargetTransform.position -
                position).sqrMagnitude;

            if (distance >= nearestDistance)
                continue;

            nearestTarget = target;
            nearestDistance = distance;
        }

        if (nearestTarget != null)
            results.Add(nearestTarget);
    }
    #endregion

    #region ─────────────────────────▶ 메시지 함수 ◀─────────────────────────
    #endregion
}
