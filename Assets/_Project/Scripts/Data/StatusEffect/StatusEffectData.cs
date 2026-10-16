using UnityEngine;

/// <summary>공유 효과 정의. 대상별 시간/중첩 상태는 이 SO에 저장하지 않습니다.</summary>
public abstract class StatusEffectData : ScriptableObject
{
    [Tooltip("같은 ID는 중첩 대신 지속 시간을 갱신합니다. 비어 있으면 효과 종류를 사용합니다.")]
    [SerializeField] private string id;
    [Min(0.01f)] [SerializeField] private float duration = 3f;
    [Min(0.01f)] [SerializeField] private float tickInterval = 1f;

    public virtual string Id => string.IsNullOrWhiteSpace(id) ? GetType().Name : id;
    public float Duration => duration;
    public float TickInterval => tickInterval;
    public virtual bool UsesPeriodicTicks => false;
    public virtual float MovementSpeedMultiplier => 1f;
    public virtual bool PreventsMovement => false;
    public virtual float ExtraHitDamage => 0f;

    public virtual void OnTick(IStatusEffectTarget target, StatusEffectInstance instance) { }

    public virtual bool IsValid()
    {
        return IsFinite(duration) && duration > 0f &&
            (!UsesPeriodicTicks || (IsFinite(tickInterval) && tickInterval >= 0.01f));
    }

    protected static bool IsFinite(float value) => !float.IsNaN(value) && !float.IsInfinity(value);
}
