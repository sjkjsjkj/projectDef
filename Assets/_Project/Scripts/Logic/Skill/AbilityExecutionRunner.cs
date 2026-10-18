using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>씬 수명에 종속된 공격 실행기. SO/AbilityInstance와 분리된 지연 타격 및 풀을 관리합니다.</summary>
public sealed class AbilityExecutionRunner : MonoBehaviour
{
    private static readonly Dictionary<int, AbilityExecutionRunner> Runners = new();
    private readonly List<PendingHit> _pending = new();
    private readonly List<AbilityProjectile> _projectiles = new();
    private readonly List<CombatSpriteEffect> _visuals = new();
    private ObjectPool<AbilityProjectile> _projectilePool;
    private ObjectPool<CombatSpriteEffect> _visualPool;
    private GameObject _templates;
    private bool _clearing;

    private sealed class PendingHit
    {
        public AbilityData Data;
        public AbilityContext Context;
        public AbilityTargetHandle Target;
        public SkillEffect[] Effects;
        public bool Melee, ShowingHit;
        public float Remaining;
    }

    public int ActiveProjectileCount => _projectiles.Count;
    public int PendingHitCount => _pending.Count;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetRegistry() => Runners.Clear();

    public static AbilityExecutionRunner ForScene(Scene scene)
    {
        if (Runners.TryGetValue(scene.handle, out var runner) && runner != null && runner.isActiveAndEnabled)
            return runner;
        var go = new GameObject("AbilityExecutionRunner");
        SceneManager.MoveGameObjectToScene(go, scene);
        runner = go.AddComponent<AbilityExecutionRunner>();
        Runners[scene.handle] = runner;
        return runner;
    }

    public bool TryLaunch(AbilityData data, AbilityContext context)
    {
        if (!isActiveAndEnabled || _clearing || data == null || context?.Owner == null || context.Owner.IsBeingDragged)
            return false;
        var targets = new List<AbilityTargetHandle>();
        var seen = new HashSet<IDamageable>();
        foreach (IDamageable candidate in context.Targets)
        {
            var target = new AbilityTargetHandle(candidate);
            if (target.IsValid && seen.Add(candidate)) targets.Add(target);
        }
        if (targets.Count == 0) return false;
        EnsurePools();
        context.AimPosition = targets[0].Position;
        Vector3 origin = context.Origin.position + (Vector3)data.AttackOriginOffset;
        var effects = new SkillEffect[data.Effects.Count];
        for (int i = 0; i < effects.Length; i++) effects[i] = data.Effects[i];
        bool melee = data.DeliveryType == AttackDeliveryType.Melee;
        if (melee)
        {
            Vector3 direction = (context.AimPosition - origin).normalized;
            PlayVisual(data.AttackEffect, data.AttackEffectSize, data.MeleeAttackDuration,
                origin + direction * data.MeleeEffectOffset, DirectionRotation(direction, data.AttackImageAngle), context.Owner);
        }
        foreach (AbilityTargetHandle target in targets)
        {
            var hit = new PendingHit
            {
                Data = data, Context = context.ForTarget(target.Target), Target = target, Effects = effects,
                Melee = melee, Remaining = data.MeleeAttackDuration
            };
            if (melee) _pending.Add(hit);
            else if (_projectilePool.PullInactive(out AbilityProjectile projectile))
            {
                _projectiles.Add(projectile);
                projectile.Launch(data, target, origin, () => BeginHit(hit), ReturnProjectile);
            }
        }
        return true;
    }

    private void BeginHit(PendingHit hit)
    {
        if (_clearing || !hit.Target.IsValid) return;
        hit.ShowingHit = true;
        hit.Remaining = hit.Data.HitEffectDuration;
        PlayVisual(hit.Data.HitEffect, hit.Data.HitEffectSize, hit.Data.HitEffectDuration,
            hit.Target.Position, Quaternion.identity, hit.Melee ? hit.Context.Owner : null, hit.Target);
        _pending.Add(hit);
    }

    /// <summary>전체 일시정지는 Update에서 판정합니다. 고정 deltaTime으로 실행 순서도 검증할 수 있습니다.</summary>
    public void Tick(float deltaTime)
    {
        if (_clearing || !isActiveAndEnabled || deltaTime <= 0f) return;
        // 새로 발생한 피격 연출은 다음 Tick 이후에 완료하여 최소 한 프레임은 표시합니다.
        for (int i = _visuals.Count - 1; i >= 0; i--)
        {
            if (_visuals[i] == null) _visuals.RemoveAt(i);
            else _visuals[i].Tick(deltaTime);
        }
        for (int i = _pending.Count - 1; i >= 0; i--)
        {
            PendingHit hit = _pending[i];
            if (!hit.Target.IsValid || (hit.Melee && (hit.Context.Owner == null || !hit.Context.Owner.isActiveAndEnabled)))
            { _pending.RemoveAt(i); continue; }
            if (hit.Melee && hit.Context.Owner.IsBeingDragged) continue;
            hit.Remaining -= deltaTime;
            if (hit.Remaining > 0f) continue;
            _pending.RemoveAt(i);
            if (!hit.ShowingHit) BeginHit(hit);
            else
            {
                foreach (SkillEffect effect in hit.Effects)
                {
                    // 앞선 효과로 사망/풀 반환된 대상에는 후속 효과를 적용하지 않습니다.
                    if (!hit.Target.IsValid) break;
                    if (effect != null) effect.Apply(hit.Context);
                }
            }
        }
        for (int i = _projectiles.Count - 1; i >= 0; i--)
        {
            if (_projectiles[i] == null) _projectiles.RemoveAt(i);
            else _projectiles[i].Tick(deltaTime);
        }
    }

    private void PlayVisual(Sprite sprite, float size, float duration, Vector3 position, Quaternion rotation,
        Unit owner = null, AbilityTargetHandle? target = null)
    {
        if (sprite == null || !_visualPool.PullInactive(out CombatSpriteEffect effect)) return;
        _visuals.Add(effect);
        effect.Play(sprite, size, duration, position, rotation, ReturnVisual, owner, target);
    }

    private void ReturnProjectile(AbilityProjectile instance)
    {
        _projectiles.Remove(instance);
        if (!_clearing) _projectilePool?.Push(instance);
    }

    private void ReturnVisual(CombatSpriteEffect instance)
    {
        _visuals.Remove(instance);
        if (!_clearing) _visualPool?.Push(instance);
    }

    private void EnsurePools()
    {
        if (_projectilePool != null) return;
        _templates = new GameObject("CombatPoolTemplates");
        _templates.SetActive(false);
        _templates.transform.SetParent(transform, false);
        AbilityProjectile projectile = Template<AbilityProjectile>("ProjectileTemplate");
        CombatSpriteEffect visual = Template<CombatSpriteEffect>("HitEffectTemplate");
        _projectilePool = new ObjectPool<AbilityProjectile>(projectile, true, 0, transform);
        _visualPool = new ObjectPool<CombatSpriteEffect>(visual, true, 0, transform);
    }

    private T Template<T>(string name) where T : BaseMono
    {
        var go = new GameObject(name);
        go.SetActive(false);
        go.transform.SetParent(_templates.transform, false);
        var renderer = go.AddComponent<SpriteRenderer>();
        Material material = Resources.Load<Material>("CombatSpriteMaterial");
        if (material != null) renderer.sharedMaterial = material;
        return go.AddComponent<T>();
    }

    private static Quaternion DirectionRotation(Vector3 direction, float angle)
        => Quaternion.Euler(0, 0, Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg + angle);

    private void Update()
    {
        if (GameManager.Ins != null && GameManager.Ins.IsCombatPaused) return;
        Tick(Time.deltaTime);
    }

    public void Clear()
    {
        if (_clearing) return;
        _clearing = true;
        _pending.Clear();
        _projectilePool?.Dispose();
        _visualPool?.Dispose();
        _projectilePool = null;
        _visualPool = null;
        _projectiles.Clear();
        _visuals.Clear();
        if (_templates != null)
        {
            if (Application.isPlaying) Destroy(_templates); else DestroyImmediate(_templates);
        }
        _templates = null;
        _clearing = false;
    }

    private void OnDisable() => Clear();
    private void OnDestroy()
    {
        Clear();
        int scene = gameObject.scene.handle;
        if (Runners.TryGetValue(scene, out var runner) && runner == this) Runners.Remove(scene);
    }
}
