using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;

public class WaveAutoAdvanceTests
{
    private readonly List<Object> _objects = new List<Object>();
    private WaveManager _waves;
    private EnemySpawner _spawner;
    private WaveData _first;
    private int _endCount;
    private int _endedNumber;

    [SetUp]
    public void SetUp()
    {
        _endCount = 0;
        _endedNumber = 0;
        // 비활성 WaveManager를 사용하여 테스트 중 글로벌 게임/세이브를 만들지 않습니다.
        _waves = CreateObject("Waves", false).AddComponent<WaveManager>();
        _spawner = CreateObject("Spawner").AddComponent<EnemySpawner>();
        _first = ScriptableObject.CreateInstance<WaveData>();
        _objects.Add(_first);
        Set(_first, "waveIndex", 7);
        Set(_waves, "waveDatas", new List<WaveData> { _first, _first });
        Set(_waves, "enemySpawner", _spawner);
        Set(_waves, "<IsRunning>k__BackingField", true);
        Set(_waves, "<RemainingTime>k__BackingField", 30f);
        _waves.OnWaveEnd += number => { _endCount++; _endedNumber = number; };
    }

    [TearDown]
    public void TearDown()
    {
        for (int i = _objects.Count - 1; i >= 0; i--)
            if (_objects[i] != null) Object.DestroyImmediate(_objects[i]);
        _objects.Clear();
    }

    [Test]
    public void DisabledAutoWaitsForDeadlineEvenWhenFieldIsEmpty()
    {
        Tick(1f);
        Assert.That(_waves.IsRunning, Is.True);
        Assert.That(_waves.RemainingTime, Is.EqualTo(29f));
        Assert.That(_endCount, Is.Zero);
        Tick(29f);
        Assert.That(_endCount, Is.EqualTo(1));
        Assert.That(_waves.CurrentWaveIndex, Is.EqualTo(1));
    }

    [Test]
    public void EnabledAutoDoesNotSkipUnspawnedEnemies()
    {
        _waves.SetAutoAdvance(true);
        Set(_spawner, "_wave", _first);
        Tick(1f);
        Assert.That(_spawner.IsSpawning, Is.True);
        Assert.That(_waves.IsRunning, Is.True);
        Assert.That(_endCount, Is.Zero);
    }

    [Test]
    public void EnabledAutoWaitsForEnemiesOutsideItsOwnSpawner()
    {
        var enemy = CreateObject("OtherSpawnerEnemy").AddComponent<EnemyHealth>();
        // EditMode에서 MonoBehaviour 생명주기 호출은 명시적으로 재현합니다.
        Invoke(enemy, "Awake");
        Invoke(enemy, "OnEnable");
        try
        {
            _waves.SetAutoAdvance(true);
            Assert.That(_spawner.ActiveEnemyCount, Is.Zero);
            Tick(1f);
            Assert.That(_endCount, Is.Zero);
            enemy.gameObject.SetActive(false);
            Invoke(enemy, "OnDisable");
            Tick(1f);
            Assert.That(_endCount, Is.EqualTo(1));
        }
        finally { Invoke(enemy, "OnDisable"); }
    }

    [Test]
    public void AutoCompletionUsesNormalWaveEndExactlyOnceAndPreservesEvent()
    {
        var roundEvent = ScriptableObject.CreateInstance<RoundEventData>();
        _objects.Add(roundEvent);
        Set(_first, "completionEvent", roundEvent);
        _waves.SetAutoAdvance(true);
        Tick(0.01f);
        Tick(0.01f);
        Assert.That(_endCount, Is.EqualTo(1));
        Assert.That(_endedNumber, Is.EqualTo(7));
        Assert.That(_waves.CurrentWaveIndex, Is.EqualTo(1));
        Assert.That(_waves.LastCompletedWaveData.CompletionEvent, Is.SameAs(roundEvent));
        Assert.That(_waves.IsRunning, Is.False);
        Assert.That(_waves.RemainingTime, Is.Zero);
    }

    [Test]
    public void EnablingAutoOnClearedFieldCompletesOnNextTick()
    {
        Tick(1f);
        Assert.That(_endCount, Is.Zero);
        _waves.SetAutoAdvance(true);
        Tick(0.01f);
        Assert.That(_endCount, Is.EqualTo(1));
    }

    [Test]
    public void DisablingAutoReturnsToWaitingForTime()
    {
        _waves.SetAutoAdvance(true);
        _waves.SetAutoAdvance(false);
        Tick(1f);
        Assert.That(_endCount, Is.Zero);
        Assert.That(_waves.IsRunning, Is.True);
    }

    [Test]
    public void PausedTimeDoesNotAutomaticallyCompleteClearedWave()
    {
        _waves.SetAutoAdvance(true);
        Tick(0f);
        Assert.That(_endCount, Is.Zero);
        Assert.That(_waves.RemainingTime, Is.EqualTo(30f));
    }

    [Test]
    public void DeadlineStillCompletesWhenAutoIsEnabledAndSpawnsRemain()
    {
        _waves.SetAutoAdvance(true);
        Set(_spawner, "_wave", _first);
        Tick(30f);
        Assert.That(_endCount, Is.EqualTo(1));
        Assert.That(_spawner.IsSpawning, Is.False);
    }

    private void Tick(float seconds) => typeof(WaveManager)
        .GetMethod("Tick", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(_waves, new object[] { seconds });

    private GameObject CreateObject(string name, bool active = true)
    {
        var go = new GameObject(name);
        go.SetActive(active);
        _objects.Add(go);
        return go;
    }

    private static void Set(object target, string name, object value) => target.GetType()
        .GetField(name, BindingFlags.Instance | BindingFlags.NonPublic).SetValue(target, value);

    private static void Invoke(object target, string method) => target.GetType()
        .GetMethod(method, BindingFlags.Instance | BindingFlags.NonPublic).Invoke(target, null);
}
