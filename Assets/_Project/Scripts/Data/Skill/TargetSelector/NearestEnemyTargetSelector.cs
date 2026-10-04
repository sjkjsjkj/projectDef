using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// SO 클래스의 설계 의도입니다.
/// </summary>
[CreateAssetMenu(fileName = "NearestEnemyTargetSelector_", menuName = "ScriptableObjects/Combat/TargetSelector/NearestEnemyTargetSelector", order = 2)]
public class NearestEnemyTargetSelector : TargetSelector
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
