using System;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using Object = UnityEngine.Object;

public class StatusEffectTests
{
    private readonly List<Object> _objects = new List<Object>();
    private EnemyPool _pool;

    [TearDown]
    public void TearDown()
    {
        // EditMode에서는 런타임 비활성화 콜백이 자동 호출되지 않을 수 있습니다.
        foreach (Object instance in _objects)
            if (instance is GameObject go && go != null)
                foreach (StatusEffectController controller in go.GetComponentsInChildren<StatusEffectController>(true))
                    controller.ClearEffects();
        _pool?.Dispose();
        _pool = null;
        for (int i = _objects.Count - 1; i >= 0; i--)
            if (_objects[i] != null) Object.DestroyImmediate(_objects[i]);
        _objects.Clear();
    }

    [TestCase(EnemyStatusEffectType.Poison)]
    [TestCase(EnemyStatusEffectType.Bleeding)]
    public void PeriodicDamageStartsAfterIntervalAndIncludesLastTick(EnemyStatusEffectType type)
    {
        EnemyStatusController status = Enemy();
        EnemyHealth health = status.GetComponent<EnemyHealth>();
        status.TryApply(Data(type, 3f));
        status.Tick(0.5f);
        Assert.That(health.CurrentHealth, Is.EqualTo(100f));
        status.Tick(0.5f);
        Assert.That(health.CurrentHealth, Is.EqualTo(95f));
        status.Tick(10f);
        Assert.That(health.CurrentHealth, Is.EqualTo(85f), "만료 후의 틱은 실행하면 안 됩니다.");
        Assert.That(status.EffectCount, Is.Zero);
    }

    [Test]
    public void SkillEffectAppliesToTheSelectedTarget()
    {
        GameObject ownerObject = NewObject("StatusCaster");
        ownerObject.SetActive(false);
        UnitData unitData = ScriptableObject.CreateInstance<UnitData>();
        _objects.Add(unitData);
        Unit owner = ownerObject.AddComponent<Unit>();
        owner.InitializeForSpawn(unitData);
        var context = new AbilityContext(owner);
        EnemyStatusController selected = Enemy();
        EnemyStatusController other = Enemy();
        var targets = (List<IDamageable>)typeof(AbilityContext)
            .GetProperty("TargetBuffer", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(context);
        targets.Add(selected.GetComponent<EnemyHealth>());
        ApplyStatusEffect effect = ScriptableObject.CreateInstance<ApplyStatusEffect>();
        _objects.Add(effect);
        Set(effect, "statusEffect", Data(EnemyStatusEffectType.Stun));
        effect.Apply(context);
        Assert.That(selected.IsMovementBlocked, Is.True);
        Assert.That(other.IsMovementBlocked, Is.False);
    }

    [Test]
    public void AllEightPresetAssetsLoadWithDistinctIds()
    {
        var ids = new HashSet<string>();
        foreach (EnemyStatusEffectType type in Enum.GetValues(typeof(EnemyStatusEffectType)))
        {
            var data = UnityEditor.AssetDatabase.LoadAssetAtPath<EnemyStatusEffectData>(
                "Assets/_Project/Scripts/Data/StatusEffect/Presets/" + type + ".asset");
            Assert.That(data, Is.Not.Null, type.ToString());
            Assert.That(data.IsValid(), Is.True, type.ToString());
            Assert.That(data.EffectType, Is.EqualTo(type));
            Assert.That(ids.Add(data.Id), Is.True, "기본 상태이상 ID는 종류별로 달라야 합니다.");
        }
    }

    [Test]
    public void BurnAddsDamageOnceToDirectHitsButNotToPeriodicDamage()
    {
        EnemyStatusController status = Enemy();
        EnemyHealth health = status.GetComponent<EnemyHealth>();
        status.TryApply(Data(EnemyStatusEffectType.Burn));
        status.TryApply(Data(EnemyStatusEffectType.Poison));
        health.TakeDamage(10f);
        Assert.That(health.CurrentHealth, Is.EqualTo(85f));
        status.Tick(1f);
        Assert.That(health.CurrentHealth, Is.EqualTo(80f));
        health.TakeDamage(0f);
        health.TakeDamage(float.NaN);
        Assert.That(health.CurrentHealth, Is.EqualTo(80f));
        status.ClearEffects();
        health.TakeDamage(10f);
        Assert.That(health.CurrentHealth, Is.EqualTo(70f));
    }

    [Test]
    public void ParalysisStopsForPointOneSecondsAtEveryTick()
    {
        EnemyStatusController status = Enemy();
        status.TryApply(Data(EnemyStatusEffectType.Paralysis));
        status.Tick(0.5f);
        Assert.That(status.IsMovementBlocked, Is.False);
        status.Tick(0.5f);
        Assert.That(status.IsMovementBlocked, Is.True);
        status.Tick(0.05f);
        Assert.That(status.IsMovementBlocked, Is.True);
        status.Tick(0.06f);
        Assert.That(status.IsMovementBlocked, Is.False);
        status.Tick(0.90f);
        Assert.That(status.IsMovementBlocked, Is.True);
    }

    [Test]
    public void LongFrameOnlyKeepsUnelapsedPartOfParalysisPulse()
    {
        EnemyStatusController status = Enemy();
        status.TryApply(Data(EnemyStatusEffectType.Paralysis));
        status.Tick(1.25f);
        Assert.That(status.IsMovementBlocked, Is.False);
        status.Tick(0.80f);
        Assert.That(status.IsMovementBlocked, Is.True);
        status.Tick(0.06f);
        Assert.That(status.IsMovementBlocked, Is.False);
    }

    [Test]
    public void DifferentSlowsMultiplyAndExpireIndependently()
    {
        EnemyStatusController status = Enemy();
        EnemyMovement movement = status.GetComponent<EnemyMovement>();
        float baseSpeed = movement.MoveSpeed;
        status.TryApply(Data(EnemyStatusEffectType.Chill, 1f, slow: 0.3f));
        status.TryApply(Data(EnemyStatusEffectType.Fracture, 2f, slow: 0.5f));
        Assert.That(movement.MoveSpeed, Is.EqualTo(baseSpeed * 0.35f).Within(0.0001f));
        status.Tick(1f);
        Assert.That(movement.MoveSpeed, Is.EqualTo(baseSpeed * 0.5f).Within(0.0001f));
        status.Tick(1f);
        Assert.That(movement.MoveSpeed, Is.EqualTo(baseSpeed));
    }

    [Test]
    public void RemovingFreezeDoesNotReleaseRemainingStun()
    {
        EnemyStatusController status = Enemy();
        EnemyStatusEffectData freeze = Data(EnemyStatusEffectType.Freeze);
        status.TryApply(freeze);
        status.TryApply(Data(EnemyStatusEffectType.Stun, 2f));
        Assert.That(status.IsMovementBlocked, Is.True);
        Assert.That(status.RemoveEffect(freeze.Id), Is.True);
        Assert.That(status.IsMovementBlocked, Is.True);
        status.Tick(2f);
        Assert.That(status.IsMovementBlocked, Is.False);
    }

    [Test]
    public void ReapplicationRefreshesDurationWithoutPostponingPeriodicDamage()
    {
        EnemyStatusController status = Enemy();
        EnemyStatusEffectData poison = Data(EnemyStatusEffectType.Poison, 3f);
        status.TryApply(poison);
        status.Tick(0.5f);
        status.TryApply(poison);
        Assert.That(status.EffectCount, Is.EqualTo(1));
        Assert.That(status.GetRemainingTime(poison.Id), Is.EqualTo(3f));
        status.Tick(0.5f);
        Assert.That(status.GetComponent<EnemyHealth>().CurrentHealth, Is.EqualTo(95f));
    }

    [Test]
    public void SharedDefinitionDoesNotShareRemainingTime()
    {
        EnemyStatusController first = Enemy();
        EnemyStatusController second = Enemy();
        EnemyStatusEffectData data = Data(EnemyStatusEffectType.Stun, 3f);
        first.TryApply(data);
        second.TryApply(data);
        first.Tick(1f);
        Assert.That(first.GetRemainingTime(data.Id), Is.EqualTo(2f));
        Assert.That(second.GetRemainingTime(data.Id), Is.EqualTo(3f));
        Assert.That(data.Duration, Is.EqualTo(3f));
    }

    [Test]
    public void PausedSystemPreservesDamageAndDurationAndUnregistersOnExpiry()
    {
        int before = StatusEffectSystem.ActiveControllerCount;
        EnemyStatusController status = Enemy();
        EnemyStatusEffectData poison = Data(EnemyStatusEffectType.Poison, 1f);
        status.TryApply(poison);
        Assert.That(StatusEffectSystem.ActiveControllerCount, Is.EqualTo(before + 1));
        StatusEffectSystem.TickAll(5f, true);
        Assert.That(status.GetRemainingTime(poison.Id), Is.EqualTo(1f));
        Assert.That(status.GetComponent<EnemyHealth>().CurrentHealth, Is.EqualTo(100f));
        StatusEffectSystem.TickAll(1f);
        Assert.That(status.GetComponent<EnemyHealth>().CurrentHealth, Is.EqualTo(95f));
        Assert.That(StatusEffectSystem.ActiveControllerCount, Is.EqualTo(before));
    }

    [Test]
    public void PeriodicDeathDuringCentralIterationDoesNotSkipOtherEnemies()
    {
        EnemyMovement prefab = Enemy(false).GetComponent<EnemyMovement>();
        _pool = new EnemyPool(NewObject("PoolOwner").transform);
        EnemyMovement first = _pool.Rent(prefab);
        EnemyMovement second = _pool.Rent(prefab);
        first.gameObject.SetActive(true);
        second.gameObject.SetActive(true);
        first.GetComponent<EnemyStatusController>().TryApply(Data(EnemyStatusEffectType.Poison, damage: 100f));
        second.GetComponent<EnemyStatusController>().TryApply(Data(EnemyStatusEffectType.Bleeding));
        StatusEffectSystem.TickAll(1f);
        Assert.That(first.gameObject.activeSelf, Is.False);
        Assert.That(first.GetComponent<EnemyStatusController>().EffectCount, Is.Zero);
        Assert.That(second.GetComponent<EnemyHealth>().CurrentHealth, Is.EqualTo(95f));
    }

    [Test]
    public void PoolReuseClearsDamageSpeedAndMovementRestrictions()
    {
        EnemyMovement prefab = Enemy(false).GetComponent<EnemyMovement>();
        _pool = new EnemyPool(NewObject("PoolOwner").transform);
        EnemyMovement enemy = _pool.Rent(prefab);
        enemy.gameObject.SetActive(true);
        EnemyStatusController status = enemy.GetComponent<EnemyStatusController>();
        status.TryApply(Data(EnemyStatusEffectType.Burn));
        status.TryApply(Data(EnemyStatusEffectType.Chill));
        status.TryApply(Data(EnemyStatusEffectType.Stun));
        enemy.Despawn();
        EnemyMovement reused = _pool.Rent(prefab);
        reused.gameObject.SetActive(true);
        Assert.That(reused, Is.SameAs(enemy));
        Assert.That(status.EffectCount, Is.Zero);
        Assert.That(status.ExtraHitDamage, Is.Zero);
        Assert.That(status.MovementSpeedMultiplier, Is.EqualTo(1f));
        Assert.That(status.IsMovementBlocked, Is.False);
        reused.GetComponent<EnemyHealth>().TakeDamage(10f);
        Assert.That(reused.GetComponent<EnemyHealth>().CurrentHealth, Is.EqualTo(90f));
    }

    [Test]
    public void ClearInsideDamageCallbackStopsCatchUpTicksSafely()
    {
        EnemyStatusController status = Enemy();
        status.TryApply(Data(EnemyStatusEffectType.Poison));
        Action<Vector3, float> clear = (_, __) => status.ClearEffects();
        EnemyHealth.OnDamageTaken += clear;
        try { status.Tick(3f); }
        finally { EnemyHealth.OnDamageTaken -= clear; }
        Assert.That(status.EffectCount, Is.Zero);
        Assert.That(status.GetComponent<EnemyHealth>().CurrentHealth, Is.EqualTo(95f));
    }

    [Test]
    public void DisableClearsEffectsAndInactiveTargetsRejectApplication()
    {
        EnemyStatusController status = Enemy();
        EnemyStatusEffectData data = Data(EnemyStatusEffectType.Stun);
        status.TryApply(data);
        status.enabled = false;
        // ExecuteAlways가 아닌 컴포넌트의 런타임 OnDisable 경로를 명시적으로 재현합니다.
        typeof(StatusEffectController).GetMethod("OnDisable", BindingFlags.Instance | BindingFlags.NonPublic)
            .Invoke(status, null);
        Assert.That(status.EffectCount, Is.Zero);
        Assert.That(status.TryApply(data), Is.False);
        status.enabled = true;
        Assert.That(status.IsMovementBlocked, Is.False);
    }

    [TestCase(0f)]
    [TestCase(-1f)]
    [TestCase(float.NaN)]
    [TestCase(float.PositiveInfinity)]
    public void InvalidPeriodicIntervalsAreRejected(float interval)
    {
        EnemyStatusEffectData poison = Data(EnemyStatusEffectType.Poison);
        Set(poison, "tickInterval", interval);
        Assert.That(Enemy().TryApply(poison), Is.False);
    }

    private EnemyStatusController Enemy(bool active = true)
    {
        GameObject go = NewObject("StatusEnemy");
        go.SetActive(false);
        go.AddComponent<EnemyHealth>();
        EnemyMovement movement = go.AddComponent<EnemyMovement>();
        movement.Setup();
        go.SetActive(active);
        return go.GetComponent<EnemyStatusController>();
    }

    private EnemyStatusEffectData Data(EnemyStatusEffectType type, float duration = 3f, float damage = 5f, float slow = 0.3f)
    {
        EnemyStatusEffectData data = ScriptableObject.CreateInstance<EnemyStatusEffectData>();
        _objects.Add(data);
        Set(data, "effectType", type);
        Set(data, "duration", duration);
        Set(data, "damage", damage);
        Set(data, "slowRatio", slow);
        return data;
    }

    private GameObject NewObject(string name)
    {
        var go = new GameObject(name);
        _objects.Add(go);
        return go;
    }

    private static void Set(object target, string name, object value)
    {
        for (Type type = target.GetType(); type != null; type = type.BaseType)
        {
            FieldInfo field = type.GetField(name, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.DeclaredOnly);
            if (field == null) continue;
            field.SetValue(target, value);
            return;
        }
        throw new MissingFieldException(name);
    }
}
