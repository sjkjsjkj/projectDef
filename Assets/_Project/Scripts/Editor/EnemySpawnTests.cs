using System.Collections.Generic;
using System.Reflection;
using System.Text.RegularExpressions;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class EnemySpawnTests
{
    private readonly List<Object> _objects = new List<Object>();
    private EnemyPool _pool;
    private EnemyMovement _prefab;
    private EnemyPath _path;

    [SetUp]
    public void SetUp()
    {
        GameObject prefab = CreateObject("EnemyTemplate", false);
        prefab.AddComponent<EnemyHealth>();
        prefab.AddComponent<SpriteRenderer>();
        _prefab = prefab.AddComponent<EnemyMovement>();
        _path = CreateObject("Path").AddComponent<EnemyPath>();
        CreateObject("Waypoint").transform.SetParent(_path.transform);
        _pool = new EnemyPool(CreateObject("PoolOwner").transform);
    }

    [TearDown]
    public void TearDown()
    {
        _pool?.Dispose();
        for (int i = _objects.Count - 1; i >= 0; i--)
            if (_objects[i] != null) Object.DestroyImmediate(_objects[i]);
        _objects.Clear();
    }

    [Test]
    public void CsvUsesIndexRatherThanRowPosition()
    {
        Assert.That(EnemyDataSheet.TryParse("INDEX,SPRITE,HP,SPEED\n100,,1090,2\n1,,100,2\n",
            out EnemyDataSheet sheet, out string error), Is.True, error);
        Assert.That(sheet.TryGet(1, out EnemyData first), Is.True);
        Assert.That(first.Hp, Is.EqualTo(100));
        Assert.That(sheet.TryGet(100, out EnemyData last), Is.True);
        Assert.That(last.Hp, Is.EqualTo(1090));
        Assert.That(sheet.TryGet(2, out _), Is.False);
    }

    [TestCase("1,,100,2\n1,,200,2")]
    [TestCase("0,,100,2")]
    [TestCase("1,,0,2")]
    [TestCase("1,,NaN,2")]
    [TestCase("1,,100,Infinity")]
    [TestCase("1,,100,-1")]
    public void CsvRejectsInvalidRows(string row)
    {
        Assert.That(EnemyDataSheet.TryParse("INDEX,SPRITE,HP,SPEED\n" + row, out _, out _), Is.False);
    }

    [Test]
    public void CsvSupportsQuotedSpritePaths()
    {
        Assert.That(EnemyDataSheet.TryParse("\uFEFFINDEX,SPRITE,HP,SPEED\r\n1,\"Enemies/A,B\",100,2\r\n",
            out EnemyDataSheet sheet, out string error), Is.True, error);
        sheet.TryGet(1, out EnemyData row);
        Assert.That(row.SpritePath, Is.EqualTo("Enemies/A,B"));
    }

    [Test]
    public void PrewarmingAndInactiveRentDoNotRegisterEnemies()
    {
        int before = EnemyHealth.ActiveEnemyCount;
        using (var pool = new ObjectPool<EnemyMovement>(_prefab, false, 3))
        {
            Assert.That(EnemyHealth.ActiveEnemyCount, Is.EqualTo(before));
            Assert.That(pool.PullInactive(out EnemyMovement enemy), Is.True);
            Assert.That(enemy.gameObject.activeInHierarchy, Is.False);
            Assert.That(enemy.GetComponent<EnemyHealth>().CurrentHealth, Is.EqualTo(100));
            Assert.That(EnemyHealth.ActiveEnemyCount, Is.EqualTo(before));
        }
    }

    [Test]
    public void ReturnedEnemyResetsStatsSpriteAndBounties()
    {
        ResourceManager resources = CreateObject("Resources").AddComponent<ResourceManager>();
        EnemyMovement enemy = _pool.Rent(_prefab);
        EnemyHealth health = enemy.GetComponent<EnemyHealth>();
        Texture2D texture = new Texture2D(2, 2);
        _objects.Add(texture);
        Sprite sprite = Sprite.Create(texture, new Rect(0, 0, 2, 2), Vector2.zero);
        _objects.Add(sprite);
        enemy.ApplyData(new EnemyData(10, "", 190, 4), sprite);
        health.InitializeBounties(resources, new List<Bounty> { new GoldBounty(7) });
        Activate(enemy);
        Assert.That(health.CurrentHealth, Is.EqualTo(190), "Awake must preserve injected HP.");
        health.TakeDamage(190);
        health.TakeDamage(190);
        Assert.That(resources.Gold, Is.EqualTo(7), "One reward per death.");
        Assert.That(enemy.gameObject.activeSelf, Is.False);

        EnemyMovement reused = _pool.Rent(_prefab);
        Assert.That(reused, Is.SameAs(enemy));
        Assert.That(reused.MoveSpeed, Is.EqualTo(2));
        Assert.That(reused.GetComponent<SpriteRenderer>().sprite, Is.Null);
        Assert.That(health.IsDead, Is.False);
        Assert.That(health.CurrentHealth, Is.EqualTo(100));
        Activate(reused);
        health.TakeDamage(100);
        Assert.That(resources.Gold, Is.EqualTo(7), "The previous bounty must not carry over.");
    }

    [Test]
    public void DoubleReturnDoesNotLeaseSameObjectTwice()
    {
        EnemyMovement first = _pool.Rent(_prefab);
        first.Despawn();
        first.Despawn();
        EnemyMovement a = _pool.Rent(_prefab);
        EnemyMovement b = _pool.Rent(_prefab);
        Assert.That(a, Is.SameAs(first));
        Assert.That(b, Is.Not.SameAs(a));
    }

    [Test]
    public void PoolRejectsObjectsOwnedByAnotherPool()
    {
        using (var first = new ObjectPool<EnemyMovement>(_prefab, false, 1))
        using (var second = new ObjectPool<EnemyMovement>(_prefab, false, 0))
        {
            first.PullInactive(out EnemyMovement enemy);
            second.Push(enemy);
            Assert.That(second.PullInactive(out _), Is.False);
            first.Push(enemy);
            Assert.That(first.PullInactive(out EnemyMovement returned), Is.True);
            Assert.That(returned, Is.SameAs(enemy));
        }
    }

    [Test]
    public void FinishedVfxReturnsAndReusesSameInstance()
    {
        GameObject template = CreateObject("VfxTemplate", false);
        ParticleSystem particles = template.AddComponent<ParticleSystem>();
        var main = particles.main;
        main.loop = false;
        main.stopAction = ParticleSystemStopAction.Destroy;
        VFXEmitter prefab = template.AddComponent<VFXEmitter>();
        VFXPool pool = CreateObject("VfxPool").AddComponent<VFXPool>();
        VFXEmitter first = pool.Play(prefab, Vector3.one, Quaternion.identity);
        Assert.That(first.GetComponent<ParticleSystem>().main.stopAction, Is.EqualTo(ParticleSystemStopAction.None));
        first.GetComponent<ParticleSystem>().Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        Invoke(first, "Update");
        Assert.That(first.gameObject.activeSelf, Is.False);
        VFXEmitter second = pool.Play(prefab, Vector3.zero, Quaternion.identity);
        Assert.That(second, Is.SameAs(first));
        Assert.That(second.transform.position, Is.EqualTo(Vector3.zero));
        pool.Clear();
        Assert.That(second == null, Is.True);
    }

    [Test]
    public void DifferentPrefabsUseDifferentPools()
    {
        EnemyMovement regular = _pool.Rent(_prefab);
        regular.Despawn();
        GameObject bossObject = CreateObject("BossTemplate", false);
        EnemyHealth health = bossObject.AddComponent<EnemyHealth>();
        Set(health, "maxHealth", 5000f);
        EnemyMovement boss = _pool.Rent(bossObject.AddComponent<EnemyMovement>());
        Assert.That(boss, Is.Not.SameAs(regular));
        Assert.That(boss.GetComponent<EnemyHealth>().CurrentHealth, Is.EqualTo(5000));
    }

    [Test]
    public void AssignedPrefabDoesNotRequireCsvEvenOutsideSheetRounds()
    {
        EnemySpawner spawner = CreateSpawner(null);
        Set(_prefab.GetComponent<EnemyHealth>(), "maxHealth", 5000f);
        WaveData wave = CreateWave(200, _prefab);
        Assert.That(spawner.SpawnWave(wave), Is.True);
        EnemyMovement enemy = SpawnPreparedWave(spawner);
        Assert.That(enemy.GetComponent<EnemyHealth>().CurrentHealth, Is.EqualTo(5000));
    }

    [Test]
    public void EmptyPrefabUsesWaveIndexAndExistingEnemiesKeepTheirStats()
    {
        EnemySpawner spawner = CreateSpawner("INDEX,SPRITE,HP,SPEED\n1,,100,2\n10,,190,3\n");
        Assert.That(spawner.SpawnWave(CreateWave(10, null)), Is.True);
        EnemyMovement first = SpawnPreparedWave(spawner);
        spawner.StopSpawning();
        Assert.That(spawner.SpawnWave(CreateWave(1, null)), Is.True);
        EnemyMovement second = SpawnPreparedWave(spawner);
        Assert.That(first.GetComponent<EnemyHealth>().CurrentHealth, Is.EqualTo(190));
        Assert.That(first.MoveSpeed, Is.EqualTo(3));
        Assert.That(second.GetComponent<EnemyHealth>().CurrentHealth, Is.EqualTo(100));
        Assert.That(spawner.ActiveEnemyCount, Is.EqualTo(2));
        spawner.ClearEnemies();
        Assert.That(spawner.ActiveEnemyCount, Is.Zero);
        Assert.That(first.gameObject.activeSelf, Is.False);
        Assert.That(second.gameObject.activeSelf, Is.False);
    }

    [Test]
    public void MissingRoundFailsBeforeSpawning()
    {
        EnemySpawner spawner = CreateSpawner("INDEX,SPRITE,HP,SPEED\n1,,100,2\n");
        LogAssert.Expect(LogType.Error, new Regex("현재 라운드 INDEX 2"));
        Assert.That(spawner.SpawnWave(CreateWave(2, null)), Is.False);
        Assert.That(spawner.IsSpawning, Is.False);
        Assert.That(spawner.ActiveEnemyCount, Is.Zero);
    }

    private EnemySpawner CreateSpawner(string csv)
    {
        EnemySpawner spawner = CreateObject("Spawner").AddComponent<EnemySpawner>();
        Set(spawner, "enemyPath", _path);
        Set(spawner, "enemyPrefab", _prefab);
        TextAsset asset = csv == null ? null : new TextAsset(csv);
        if (asset != null) _objects.Add(asset);
        Set(spawner, "enemyDataSheet", asset);
        return spawner;
    }

    private WaveData CreateWave(int index, EnemyMovement prefab)
    {
        WaveData wave = ScriptableObject.CreateInstance<WaveData>();
        _objects.Add(wave);
        Set(typeof(BaseSO), wave, "id", "test-wave");
        Set(wave, "waveIndex", index);
        var group = new EnemySpawnData();
        Set(group, "enemyPrefab", prefab);
        Set(wave, "enemySpawnDatas", new List<EnemySpawnData> { group });
        return wave;
    }

    private static EnemyMovement SpawnPreparedWave(EnemySpawner spawner)
    {
        WaveData wave = (WaveData)Get(spawner, "_wave");
        EnemyMovement specified = wave.EnemySpawnDatas[0].EnemyPrefab;
        return (EnemyMovement)typeof(EnemySpawner).GetMethod("SpawnEnemy", BindingFlags.NonPublic | BindingFlags.Instance)
            .Invoke(spawner, new object[] { specified != null ? specified : Get(spawner, "enemyPrefab"),
                specified == null ? Get(spawner, "_waveEnemyData") : null, null, wave.EnemySpawnDatas[0].Bounties });
    }

    private GameObject CreateObject(string name, bool active = true)
    {
        var go = new GameObject(name);
        go.SetActive(active);
        _objects.Add(go);
        return go;
    }

    private static void Activate(EnemyMovement enemy)
    {
        // EditMode에서는 Awake/OnEnable을 직접 호출하여 동일한 순서를 검증합니다.
        Invoke(enemy.GetComponent<EnemyHealth>(), "Awake");
        Invoke(enemy, "Awake");
        enemy.gameObject.SetActive(true);
        Invoke(enemy.GetComponent<EnemyHealth>(), "OnEnable");
        Invoke(enemy, "OnEnable");
    }

    private static void Invoke(object target, string method) => target.GetType()
        .GetMethod(method, BindingFlags.NonPublic | BindingFlags.Instance).Invoke(target, null);
    private static object Get(object target, string field) => target.GetType()
        .GetField(field, BindingFlags.NonPublic | BindingFlags.Instance).GetValue(target);
    private static void Set(object target, string field, object value) => Set(target.GetType(), target, field, value);
    private static void Set(System.Type type, object target, string field, object value) => type
        .GetField(field, BindingFlags.NonPublic | BindingFlags.Instance).SetValue(target, value);
}
