using System;
using UnityEngine;

/// <summary>공격/피격 이미지를 짧게 확대하고 페이드아웃하는 풀링 이펙트입니다.</summary>
[RequireComponent(typeof(SpriteRenderer))]
public sealed class CombatSpriteEffect : BaseMono, IPoolable
{
    private SpriteRenderer _renderer;
    private Action<CombatSpriteEffect> _return;
    private AbilityTargetHandle _followTarget;
    private Unit _pauseWithOwner;
    private bool _followsTarget, _hasOwner, _playing;
    private float _elapsed, _duration, _baseScale;
    private Vector3 _ownerOffset;

    public void Setup()
    {
        if (_renderer == null) _renderer = GetComponent<SpriteRenderer>();
        ResetState();
    }

    public void Play(Sprite sprite, float size, float duration, Vector3 position, Quaternion rotation,
        Action<CombatSpriteEffect> returnToPool, Unit pauseWithOwner = null, AbilityTargetHandle? followTarget = null)
    {
        _return = returnToPool;
        _pauseWithOwner = pauseWithOwner;
        _hasOwner = pauseWithOwner != null;
        _ownerOffset = _hasOwner ? position - pauseWithOwner.transform.position : Vector3.zero;
        _followsTarget = followTarget.HasValue;
        _followTarget = followTarget.GetValueOrDefault();
        _duration = Mathf.Max(0.001f, duration);
        ConfigureRenderer(_renderer, sprite, size, 11);
        _baseScale = transform.localScale.x;
        transform.SetPositionAndRotation(position, rotation);
        _playing = true;
        gameObject.SetActive(true);
    }

    public void Tick(float deltaTime)
    {
        if (!_playing) return;
        if (_hasOwner)
        {
            if (_pauseWithOwner == null || !_pauseWithOwner.isActiveAndEnabled) { Despawn(); return; }
            transform.position = _pauseWithOwner.transform.position + _ownerOffset;
            if (_pauseWithOwner.IsBeingDragged) return;
        }
        if (_followsTarget)
        {
            if (!_followTarget.IsValid) { Despawn(); return; }
            transform.position = _followTarget.Position;
        }
        _elapsed += Mathf.Max(0f, deltaTime);
        float t = Mathf.Clamp01(_elapsed / _duration);
        transform.localScale = Vector3.one * (_baseScale * Mathf.Lerp(0.8f, 1.2f, t));
        _renderer.color = new Color(1f, 1f, 1f, 1f - t * t);
        if (_elapsed >= _duration) Despawn();
    }

    internal static void ConfigureRenderer(SpriteRenderer renderer, Sprite sprite, float size, int order)
    {
        renderer.sprite = sprite;
        renderer.color = Color.white;
        renderer.sortingOrder = order;
        float extent = sprite != null ? Mathf.Max(sprite.bounds.size.x, sprite.bounds.size.y) : 1f;
        renderer.transform.localScale = Vector3.one * (size / Mathf.Max(0.001f, extent));
    }

    private void Despawn()
    {
        if (!_playing) return;
        Action<CombatSpriteEffect> callback = _return;
        ResetState();
        if (callback != null) callback(this);
        else gameObject.SetActive(false);
    }

    private void ResetState()
    {
        _playing = _followsTarget = _hasOwner = false;
        _elapsed = _duration = _baseScale = 0f;
        _followTarget = default;
        _pauseWithOwner = null;
        _return = null;
        _ownerOffset = Vector3.zero;
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;
        transform.localScale = Vector3.one;
        if (_renderer != null) { _renderer.sprite = null; _renderer.color = Color.white; }
    }

    private void OnDisable() => Despawn();
}
