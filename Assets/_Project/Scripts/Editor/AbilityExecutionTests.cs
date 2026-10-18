using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using Object = UnityEngine.Object;

public class AbilityExecutionTests
{
    private readonly List<Object> _objects = new();
    private AbilityExecutionRunner _runner;
    private Unit _owner;
    private AbilityData _ability;
    private FixedTargets _selector;
    private AbilityContext _context;

    public class FixedTargets : TargetSelector
    {
        public readonly List<IDamageable> Selected = new();
        public override void SelectTargets(AbilityContext context, AbilityData data, List<IDamageable> results)
            => results.AddRange(Selected);
    }

    [SetUp]
    public void SetUp()
    {
        _ability = Data<AbilityData>();
        _selector = Data<FixedTargets>();
        Set(_ability, "targetSelector", _selector);
        Set(_ability, "effects", new List<SkillEffect> { Data<DamageEffect>() });
        Set(_ability, "meleeAttackDuration", 0.1f);
        Set(_ability, "hitEffectDuration", 0.1f);
        Set(_ability, "projectileSpeed", 10f);
        UnitData data = Data<UnitData>();
        Set(data, "attack", 10f);
        Set(data, "basicAttack", _ability);
        Set(data, "skillLevel1", _ability);
        var go = NewObject("AttackTestOwner", false);
        _owner = go.AddComponent<Unit>();
        _owner.InitializeForSpawn(data);
        go.SetActive(true);
        _context = new AbilityContext(_owner, true);
        _runner = AbilityExecutionRunner.ForScene(go.scene);
    }

    [TearDown]
    public void TearDown()
    {
        if (_runner != null) { _runner.Clear(); Object.DestroyImmediate(_runner.gameObject); }
        for (int i = _objects.Count - 1; i >= 0; i--)
            if (_objects[i] != null) Object.DestroyImmediate(_objects[i]);
        _objects.Clear();
    }

    [Test]
    public void MeleeAppliesDamageOnlyAfterAttackAndHitDurations()
    {
        EnemyHealth enemy = Enemy(1f);
        _selector.Selected.Add(enemy);
        Assert.That(_ability.TryExecute(_context), Is.True);
        Assert.That(enemy.CurrentHealth, Is.EqualTo(1000f));
        _runner.Tick(0.11f);
        Assert.That(enemy.CurrentHealth, Is.EqualTo(1000f), "피격 이펙트가 시작된 프레임에는 피해를 주지 않습니다.");
        _runner.Tick(0.05f);
        Assert.That(enemy.CurrentHealth, Is.EqualTo(1000f));
        _runner.Tick(0.06f);
        Assert.That(enemy.CurrentHealth, Is.EqualTo(990f));
        _runner.Tick(1f);
        Assert.That(enemy.CurrentHealth, Is.EqualTo(990f), "한 번의 공격은 한 번만 적용됩니다.");
    }

    [Test]
    public void RangedReturnsProjectileAtImpactThenDelaysDamage()
    {
        Ranged();
        EnemyHealth enemy = Enemy(2f);
        _selector.Selected.Add(enemy);
        Assert.That(_ability.TryExecute(_context), Is.True);
        Assert.That(_runner.ActiveProjectileCount, Is.EqualTo(1));
        _runner.Tick(0.05f);
        Assert.That(enemy.CurrentHealth, Is.EqualTo(1000));
        _runner.Tick(0.3f);
        Assert.That(_runner.ActiveProjectileCount, Is.Zero);
        Assert.That(enemy.CurrentHealth, Is.EqualTo(1000));
        _runner.Tick(0.11f);
        Assert.That(enemy.CurrentHealth, Is.EqualTo(990));
    }

    [Test]
    public void PoolReusesProjectileWithNewTargetAndNoOldHitCallback()
    {
        Ranged();
        EnemyHealth first = Enemy(2f), second = Enemy(3f);
        _selector.Selected.Add(first);
        _ability.TryExecute(_context);
        AbilityProjectile projectile = Flying();
        _runner.Tick(0.4f);
        Assert.That(projectile.IsFlying, Is.False);
        Assert.That(projectile.gameObject.activeSelf, Is.False);
        Assert.That(projectile.GetComponent<SpriteRenderer>().sprite, Is.Null);
        Assert.That(projectile.transform.localRotation, Is.EqualTo(Quaternion.identity));
        _runner.Tick(0.11f);
        _selector.Selected.Clear();
        _selector.Selected.Add(second);
        _ability.TryExecute(_context);
        Assert.That(Flying(), Is.SameAs(projectile));
        _runner.Tick(0.4f);
        _runner.Tick(0.11f);
        Assert.That(first.CurrentHealth, Is.EqualTo(990));
        Assert.That(second.CurrentHealth, Is.EqualTo(990));
    }

    [Test]
    public void ConcurrentCastsKeepTargetsAndLaunchAttackPower()
    {
        EnemyHealth first = Enemy(1), second = Enemy(2);
        _selector.Selected.Add(first);
        _ability.TryExecute(_context);
        _selector.Selected.Clear();
        _selector.Selected.Add(second);
        _ability.TryExecute(_context);
        _owner.TryLevelUp();
        _runner.Tick(0.11f);
        _runner.Tick(0.11f);
        Assert.That(first.CurrentHealth, Is.EqualTo(990));
        Assert.That(second.CurrentHealth, Is.EqualTo(990));
    }

    [TestCase(false)]
    [TestCase(true)]
    public void RecycledTargetCannotReceiveOldMeleeOrProjectile(bool ranged)
    {
        if (ranged) Ranged();
        EnemyHealth enemy = Enemy(2);
        _selector.Selected.Add(enemy);
        _ability.TryExecute(_context);
        enemy.gameObject.SetActive(false);
        enemy.InitializeForSpawn(1000);
        enemy.gameObject.SetActive(true);
        _runner.Tick(1);
        _runner.Tick(1);
        Assert.That(enemy.CurrentHealth, Is.EqualTo(1000));
        Assert.That(_runner.ActiveProjectileCount, Is.Zero);
        Assert.That(_runner.PendingHitCount, Is.Zero);
    }

    [Test]
    public void DestroyedTargetAndLifetimeExpiryReturnProjectiles()
    {
        Ranged();
        EnemyHealth enemy = Enemy(2);
        _selector.Selected.Add(enemy);
        _ability.TryExecute(_context);
        Object.DestroyImmediate(enemy.gameObject);
        Assert.DoesNotThrow(() => _runner.Tick(0.1f));
        Assert.That(_runner.ActiveProjectileCount, Is.Zero);
        _selector.Selected.Clear();
        EnemyHealth far = Enemy(100);
        _selector.Selected.Add(far);
        Set(_ability, "projectileLifetime", 0.1f);
        _ability.TryExecute(_context);
        _runner.Tick(0.2f);
        Assert.That(_runner.ActiveProjectileCount, Is.Zero);
        Assert.That(far.CurrentHealth, Is.EqualTo(1000));
    }

    [Test]
    public void DragFreezesCooldownsRetriesAndMeleeButNotReleasedProjectiles()
    {
        EnemyHealth enemy = Enemy(2);
        _selector.Selected.Add(enemy);
        var combat = _owner.gameObject.AddComponent<UnitCombatController>();
        Invoke(combat, "Awake");
        combat.SetCombatEnabled(true);
        combat.Tick(0f);
        var basic = (AbilityInstance)Get(combat, "_basicAttack");
        float cooldown = basic.RemainingCooldown;
        Set(combat, "_nextBasicAttackRetryTime", 0.5f);
        var skill = (AbilityInstance)Get(combat, "_skill");
        Assert.That(skill.TryActivate(), Is.True);
        float skillCooldown = skill.RemainingCooldown;
        _owner.SetDragging(true);
        combat.Tick(5);
        basic.Tick(5);
        skill.Tick(5);
        _runner.Tick(5);
        Assert.That(basic.RemainingCooldown, Is.EqualTo(cooldown));
        Assert.That(skill.RemainingCooldown, Is.EqualTo(skillCooldown));
        Assert.That(Get(combat, "_nextBasicAttackRetryTime"), Is.EqualTo(0.5f));
        Assert.That(combat.TryUseSkill(), Is.False);
        Assert.That(_ability.TryExecute(_context), Is.False);
        Assert.That(enemy.CurrentHealth, Is.EqualTo(1000));
        _owner.SetDragging(false);
        _runner.Tick(0.11f);
        _runner.Tick(0.11f);
        Assert.That(enemy.CurrentHealth, Is.EqualTo(980));
        Ranged();
        _ability.TryExecute(_context);
        _owner.SetDragging(true);
        _runner.Tick(0.4f);
        _runner.Tick(0.11f);
        Assert.That(enemy.CurrentHealth, Is.EqualTo(970));
    }

    [Test]
    public void FailedCastDoesNotStartCooldownAndClearCancelsPendingDamage()
    {
        var instance = _ability.CreateInstance(_context);
        Assert.That(instance.TryActivate(), Is.False);
        Assert.That(instance.RemainingCooldown, Is.Zero);
        EnemyHealth enemy = Enemy(1);
        _selector.Selected.Add(enemy);
        Assert.That(instance.TryActivate(), Is.True);
        _runner.Clear();
        _runner.Tick(1);
        Assert.That(enemy.CurrentHealth, Is.EqualTo(1000));
    }

    [Test]
    public void MultiTargetRangedLaunchesOneProjectilePerDistinctTarget()
    {
        Ranged();
        EnemyHealth first = Enemy(1), second = Enemy(2);
        _selector.Selected.AddRange(new IDamageable[] { first, first, second });
        _ability.TryExecute(_context);
        Assert.That(_runner.ActiveProjectileCount, Is.EqualTo(2));
        _runner.Tick(0.4f);
        _runner.Tick(0.11f);
        Assert.That(first.CurrentHealth, Is.EqualTo(990));
        Assert.That(second.CurrentHealth, Is.EqualTo(990));
    }

    private void Ranged() => Set(_ability, "deliveryType", AttackDeliveryType.Ranged);
    private AbilityProjectile Flying() => _runner.GetComponentsInChildren<AbilityProjectile>(true).Single(p => p.IsFlying);
    private EnemyHealth Enemy(float x)
    {
        var go = NewObject("AttackTarget", false);
        go.transform.position = new Vector3(x, 0, 0);
        go.AddComponent<CircleCollider2D>().radius = 0.15f;
        var enemy = go.AddComponent<EnemyHealth>();
        enemy.InitializeForSpawn(1000);
        go.SetActive(true);
        Physics2D.SyncTransforms();
        return enemy;
    }
    private GameObject NewObject(string name, bool active = true)
    {
        var go = new GameObject(name);
        go.SetActive(active);
        _objects.Add(go);
        return go;
    }
    private T Data<T>() where T : ScriptableObject
    {
        T data = ScriptableObject.CreateInstance<T>();
        _objects.Add(data);
        return data;
    }
    private static void Set(object target, string name, object value) => target.GetType().GetField(name, BindingFlags.Instance | BindingFlags.NonPublic).SetValue(target, value);
    private static object Get(object target, string name) => target.GetType().GetField(name, BindingFlags.Instance | BindingFlags.NonPublic).GetValue(target);
    private static void Invoke(object target, string name) => target.GetType().GetMethod(name, BindingFlags.Instance | BindingFlags.NonPublic).Invoke(target, null);
}
