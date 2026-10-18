using System.Collections;
using System.Reflection;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

public class AttackPipelinePlayModeTests
{
    [UnityTest]
    public IEnumerator RealFramesDeliverMeleeAndPooledRangedAndFreezeDuringDrag()
    {
        yield return new EnterPlayMode();
        float deadline = Time.realtimeSinceStartup + 20f;
        while (SceneFlowManager.Ins.IsTransitioning && Time.realtimeSinceStartup < deadline) yield return null;
        Assert.That(GameManager.Ins.IsCombatPaused, Is.False);
        var scene = SceneManager.CreateScene("AttackPipelineTest");
        var enemyObject = new GameObject("PipelineTarget");
        enemyObject.SetActive(false);
        SceneManager.MoveGameObjectToScene(enemyObject, scene);
        enemyObject.layer = 8;
        enemyObject.transform.position = new Vector3(1001.2f, 1000, 0);
        enemyObject.AddComponent<CircleCollider2D>().radius = 0.15f;
        EnemyHealth enemy = enemyObject.AddComponent<EnemyHealth>();
        enemy.InitializeForSpawn(10000f);
        enemyObject.SetActive(true);

        AbilityData melee = Object.Instantiate(AssetDatabase.LoadAssetAtPath<AbilityData>(
            "Assets/_Project/04_Data/Unit/Skill/AbilityData_BasicAttack.asset"));
        Set(melee, "meleeAttackDuration", 0.15f);
        Set(melee, "hitEffectDuration", 0.15f);
        Unit unit = CreateUnit(scene, melee);
        var combat = unit.GetComponent<UnitCombatController>();
        var animation = unit.GetComponent<UnitAttackAnimation>();
        var basic = (AbilityInstance)Get(combat, "_basicAttack");
        Physics2D.SyncTransforms();
        combat.SetCombatEnabled(true);
        combat.Tick(0f);
        combat.SetCombatEnabled(false);
        Assert.That(animation.IsPlaying, Is.True);
        Assert.That(basic.RemainingCooldown, Is.GreaterThan(0));
        float hp = enemy.CurrentHealth;
        float cooldown = basic.RemainingCooldown;
        unit.SetDragging(true);
        yield return new WaitForSeconds(0.2f);
        Assert.That(basic.RemainingCooldown, Is.EqualTo(cooldown));
        Assert.That(enemy.CurrentHealth, Is.EqualTo(hp));
        unit.SetDragging(false);
        yield return null;
        Assert.That(enemy.CurrentHealth, Is.EqualTo(hp));
        deadline = Time.realtimeSinceStartup + 3f;
        while (enemy.CurrentHealth == hp && Time.realtimeSinceStartup < deadline) yield return null;
        Assert.That(enemy.CurrentHealth, Is.LessThan(hp));
        unit.gameObject.SetActive(false);

        AbilityData ranged = Object.Instantiate(AssetDatabase.LoadAssetAtPath<AbilityData>(CombatPresentationSetup.RangedExamplePath));
        Set(ranged, "projectileSpeed", 2f);
        Set(ranged, "hitEffectDuration", 0.15f);
        Unit archer = CreateUnit(scene, ranged);
        combat = archer.GetComponent<UnitCombatController>();
        combat.SetCombatEnabled(true);
        combat.Tick(0f);
        combat.SetCombatEnabled(false);
        var runner = AbilityExecutionRunner.ForScene(scene);
        Assert.That(runner.ActiveProjectileCount, Is.EqualTo(1));
        hp = enemy.CurrentHealth;
        archer.SetDragging(true);
        deadline = Time.realtimeSinceStartup + 3f;
        while (runner.ActiveProjectileCount > 0 && Time.realtimeSinceStartup < deadline) yield return null;
        Assert.That(runner.ActiveProjectileCount, Is.Zero);
        Assert.That(enemy.CurrentHealth, Is.EqualTo(hp), "투사체 반환 시점에는 피격 연출이 진행 중입니다.");
        deadline = Time.realtimeSinceStartup + 3f;
        while (enemy.CurrentHealth == hp && Time.realtimeSinceStartup < deadline) yield return null;
        Assert.That(enemy.CurrentHealth, Is.LessThan(hp), "이미 발사한 투사체는 시전자가 드래그 중이어도 명중합니다.");
        Object.Destroy(melee);
        Object.Destroy(ranged);
        yield return new ExitPlayMode();
    }

    [UnityTearDown]
    public IEnumerator RestoreEditor()
    {
        if (Application.isPlaying) yield return new ExitPlayMode();
    }

    private static Unit CreateUnit(Scene scene, AbilityData ability)
    {
        var staging = new GameObject("PipelineUnitStaging");
        staging.SetActive(false);
        SceneManager.MoveGameObjectToScene(staging, scene);
        GameObject go = Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>(LuBuAnimationSetup.PrefabPath), staging.transform);
        go.SetActive(false);
        Unit unit = go.GetComponent<Unit>();
        var data = Object.Instantiate(unit.Data);
        Set(data, "basicAttack", ability);
        unit.InitializeForSpawn(data);
        go.transform.SetParent(null, false);
        go.transform.position = new Vector3(1000, 1000, 0);
        go.GetComponent<UnitCombatController>().SetCombatEnabled(false);
        go.SetActive(true);
        Object.Destroy(staging);
        return unit;
    }

    private static object Get(object target, string field) => target.GetType().GetField(field, BindingFlags.Instance | BindingFlags.NonPublic).GetValue(target);
    private static void Set(object target, string field, object value) => target.GetType().GetField(field, BindingFlags.Instance | BindingFlags.NonPublic).SetValue(target, value);
}
