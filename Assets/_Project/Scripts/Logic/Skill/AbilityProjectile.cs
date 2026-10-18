using System;
using UnityEngine;

/// <summary>지정된 적을 추적하는 투사체. 반환 전에 대상·콜백·이미지·이동 상태를 비웁니다.</summary>
[RequireComponent(typeof(SpriteRenderer))]
public sealed class AbilityProjectile : BaseMono, IPoolable
{
    private SpriteRenderer _renderer;
    private AbilityTargetHandle _target;
    private Action _onImpact;
    private Action<AbilityProjectile> _return;
    private float _speed, _remainingLife, _hitRadius, _imageAngle;
    public bool IsFlying { get; private set; }

    public void Setup()
    {
        if (_renderer == null) _renderer = GetComponent<SpriteRenderer>();
        ResetState();
    }

    public void Launch(AbilityData data, AbilityTargetHandle target, Vector3 origin, Action impact,
        Action<AbilityProjectile> returnToPool)
    {
        _target = target;
        _speed = data.ProjectileSpeed;
        _remainingLife = data.ProjectileLifetime;
        _hitRadius = data.ProjectileHitRadius;
        _imageAngle = data.AttackImageAngle;
        _onImpact = impact;
        _return = returnToPool;
        transform.position = origin;
        CombatSpriteEffect.ConfigureRenderer(_renderer, data.AttackEffect, data.AttackEffectSize, 10);
        PointAt(target.Position);
        IsFlying = true;
        gameObject.SetActive(true);
    }

    public void Tick(float deltaTime)
    {
        if (!IsFlying) return;
        if (!_target.IsValid) { Despawn(); return; }
        float dt = Mathf.Max(0f, deltaTime);
        if (dt <= 0f) return;
        float travelTime = Mathf.Min(dt, _remainingLife);
        Vector3 contact = _target.ContactPoint(transform.position);
        PointAt(_target.Position);
        float step = _speed * travelTime;
        if (Vector3.Distance(transform.position, contact) <= step + _hitRadius)
        {
            transform.position = contact;
            Action impact = _onImpact;
            // 충돌한 투사체는 즉시 반환하고 피격 연출/피해 대기는 실행기가 이어갑니다.
            Despawn();
            impact?.Invoke();
            return;
        }
        transform.position = Vector3.MoveTowards(transform.position, contact, step);
        _remainingLife -= dt;
        if (_remainingLife <= 0f) Despawn();
    }

    private void PointAt(Vector3 position)
    {
        Vector3 direction = position - transform.position;
        transform.rotation = Quaternion.Euler(0f, 0f, Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg + _imageAngle);
    }

    public void Despawn()
    {
        if (!IsFlying) return;
        Action<AbilityProjectile> callback = _return;
        ResetState();
        if (callback != null) callback(this);
        else gameObject.SetActive(false);
    }

    private void ResetState()
    {
        IsFlying = false;
        _target = default;
        _onImpact = null;
        _return = null;
        _speed = _remainingLife = _hitRadius = _imageAngle = 0f;
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;
        transform.localScale = Vector3.one;
        if (_renderer != null) { _renderer.sprite = null; _renderer.color = Color.white; }
    }

    private void OnDisable() => Despawn();
}
