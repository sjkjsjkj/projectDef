using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 클래스의 설계 의도입니다.
/// </summary>
public class AbilityInstance
{
    #region ─────────────────────────▶ 내부 변수 ◀─────────────────────────
    private readonly List<IDamageable> _targets = new();

    private float _remainingCooldown;
    #endregion

    #region ─────────────────────────▶ 공개 멤버 ◀─────────────────────────
    public AbilityData Data { get; }

    public float RemainingCooldown => _remainingCooldown;

    public AbilityInstance(AbilityData data)
    {
        Data = data;
    }

    public void Tick(float deltaTime)
    {
        if (_remainingCooldown <= 0f)
            return;

        _remainingCooldown -= deltaTime;
    }

    public bool TryActivate(
    UnitStats ownerStats,
    Transform ownerTransform)
    {
        if (_remainingCooldown > 0f)
            return false;

        _targets.Clear();

        var context = new AbilityContext(
            ownerStats,
            ownerTransform,
            this,
            _targets);

        foreach (var condition in Data.UseConditions)
        {
            if (!condition.IsSatisfied(context))
                return false;
        }

        Data.TargetSelector.SelectTargets(context, _targets);

        foreach (var condition in Data.ActivationConditions)
        {
            if (!condition.IsSatisfied(context))
                return false;
        }

        foreach (var effect in Data.Effects)
        {
            effect.Apply(context);
        }

        _remainingCooldown = Data.Cooldown;

        return true;
    }
    #endregion

    #region ─────────────────────────▶ 내부 메서드 ◀─────────────────────────

    #endregion
}
