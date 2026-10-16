using System.Collections.Generic;
using UnityEngine;

/// <summary>효과 수신/해제와 합산을 담당합니다. 자체 Update는 없으며 대상별 어댑터가 상속합니다.</summary>
public abstract class StatusEffectController : BaseMono, IStatusEffectTarget
{
    private readonly List<StatusEffectInstance> _effects = new List<StatusEffectInstance>(4);
    private int _version;

    public int EffectCount => _effects.Count;
    public float MovementSpeedMultiplier { get; private set; } = 1f;
    public bool IsMovementBlocked { get; private set; }
    public float ExtraHitDamage { get; private set; }
    public abstract bool IsStatusEffectTargetAlive { get; }
    public abstract void TakeStatusDamage(float damage);

    // 등록을 해제했다 다시 등록한 객체를 같은 프레임의 오래된 스냅샷으로 갱신하지 않습니다.
    internal uint RegistrationVersion { get; set; }

    public bool TryApply(StatusEffectData data)
    {
        if (!isActiveAndEnabled || !IsStatusEffectTargetAlive || data == null || !data.IsValid()) return false;
        foreach (StatusEffectInstance instance in _effects)
        {
            if (instance.Data.Id != data.Id) continue;
            instance.Refresh(data);
            _version++;
            Recalculate();
            return true;
        }

        _effects.Add(new StatusEffectInstance(data));
        _version++;
        Recalculate();
        StatusEffectSystem.Register(this);
        return true;
    }

    public bool HasEffect(string id) => GetRemainingTime(id) > 0f;

    public float GetRemainingTime(string id)
    {
        foreach (StatusEffectInstance instance in _effects)
            if (instance.Data.Id == id) return instance.RemainingTime;
        return 0f;
    }

    public bool RemoveEffect(string id)
    {
        for (int i = 0; i < _effects.Count; i++)
        {
            if (_effects[i].Data.Id != id) continue;
            _effects[i].Cancel();
            _effects.RemoveAt(i);
            _version++;
            Recalculate();
            if (_effects.Count == 0) StatusEffectSystem.Unregister(this);
            return true;
        }
        return false;
    }

    public void ClearEffects()
    {
        foreach (StatusEffectInstance instance in _effects) instance.Cancel();
        _effects.Clear();
        _version++;
        Recalculate();
        StatusEffectSystem.Unregister(this);
    }

    /// <summary>중앙 갱신기 또는 테스트에서 호출합니다. 게임 중 별도로 중복 호출하지 않습니다.</summary>
    public void Tick(float deltaTime)
    {
        if (!isActiveAndEnabled || !IsStatusEffectTargetAlive) { ClearEffects(); return; }
        if (deltaTime <= 0f || float.IsNaN(deltaTime) || float.IsInfinity(deltaTime)) return;
        int version = _version;
        for (int i = _effects.Count - 1; i >= 0; i--)
        {
            StatusEffectInstance instance = _effects[i];
            instance.Tick(deltaTime, this);
            // 외부 피해 콜백이 목록을 바꾸면 그 콜백에서 재계산한 결과를 유지합니다.
            if (version != _version) return;
            if (!IsStatusEffectTargetAlive) { ClearEffects(); return; }
            if (!instance.IsActive) _effects.RemoveAt(i);
        }
        Recalculate();
        if (_effects.Count == 0) StatusEffectSystem.Unregister(this);
    }

    private void Recalculate()
    {
        MovementSpeedMultiplier = 1f;
        IsMovementBlocked = false;
        ExtraHitDamage = 0f;
        foreach (StatusEffectInstance instance in _effects)
        {
            if (!instance.IsActive) continue;
            MovementSpeedMultiplier *= instance.Data.MovementSpeedMultiplier;
            IsMovementBlocked |= instance.IsMovementBlocked;
            ExtraHitDamage += instance.Data.ExtraHitDamage;
        }
    }

    protected virtual void OnDisable() => ClearEffects();
    protected virtual void OnDestroy() => ClearEffects();
}
