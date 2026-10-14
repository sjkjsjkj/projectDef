using System;
using UnityEngine;

/// <summary>자식 파티클까지 종료되면 반환합니다. 반복 효과는 Despawn으로 명시적으로 종료합니다.</summary>
[RequireComponent(typeof(ParticleSystem))]
public class VFXEmitter : BaseMono, IPoolable
{
    private ParticleSystem _particles;
    private Action<VFXEmitter> _returnToPool;
    private bool _playing;
    private bool _returning;

    public void Setup()
    {
        if (_particles == null) _particles = GetComponent<ParticleSystem>();
        foreach (ParticleSystem particles in GetComponentsInChildren<ParticleSystem>(true))
        {
            var main = particles.main;
            main.playOnAwake = false;
            main.stopAction = ParticleSystemStopAction.None;
        }
        _particles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        _playing = false;
        _returning = false;
    }

    internal void SetPoolReturn(Action<VFXEmitter> callback) => _returnToPool = callback;

    public void Play()
    {
        _playing = true;
        _particles.Play(true);
    }

    public void Despawn()
    {
        if (_returning) return;
        _returning = true;
        _playing = false;
        if (_particles != null) _particles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        if (_returnToPool != null) _returnToPool(this);
        else gameObject.SetActive(false);
    }

    private void Update()
    {
        if (_playing && !_particles.IsAlive(true)) Despawn();
    }

    private void OnDisable()
    {
        if (!_returning) Despawn();
    }
}
