using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>적 생존 여부와 관계없이 제한 시간으로 웨이브를 진행합니다.</summary>
public class WaveManager : BaseMono
{
    [Header("웨이브 구성")]
    [SerializeField] private List<WaveData> waveDatas = new List<WaveData>();
    [SerializeField] private EnemySpawner enemySpawner;
    [SerializeField] private bool startGameOnStart = true;
    [Tooltip("맵 전체의 살아 있는 적 수가 이 값을 초과하면 패배합니다.")]
    [SerializeField, Min(1)] private int maxEnemyCount = 100;

    private int _currentWaveIndex;
    private GameManager _gameManager;

    public int CurrentWaveIndex => _currentWaveIndex;
    public bool IsRunning { get; private set; }
    public bool IsComplete => _currentWaveIndex >= waveDatas.Count;
    public float RemainingTime { get; private set; }
    public int MaxEnemyCount => Mathf.Max(1, maxEnemyCount);
    public EnemySpawner EnemySpawner => enemySpawner;

    // 이벤트 인자는 목록 인덱스가 아닌 WaveData의 웨이브 번호입니다.
    public event Action<int> OnWaveStart;
    public event Action<int> OnWaveEnd;

    public WaveData GetCurrentWaveData()
    {
        return IsComplete ? null : waveDatas[_currentWaveIndex];
    }

    public bool StartWave()
    {
        if (IsRunning || IsComplete || !isActiveAndEnabled)
            return false;

        WaveData wave = GetCurrentWaveData();
        if (enemySpawner == null || !enemySpawner.SpawnWave(wave))
        {
            Debug.LogError("웨이브를 시작할 수 없습니다. EnemySpawner와 WaveData 설정을 확인하세요.", this);
            return false;
        }

        RemainingTime = wave.Duration;
        IsRunning = true;
        OnWaveStart?.Invoke(wave.WaveIndex);
        return true;
    }

    /// <summary>소환과 타이머만 중단합니다. 이미 생성된 적은 유지합니다.</summary>
    public void StopWave()
    {
        IsRunning = false;
        RemainingTime = 0f;
        if (enemySpawner != null)
            enemySpawner.StopSpawning();
    }

    public void ResetWaves()
    {
        StopWave();
        if (enemySpawner != null)
            enemySpawner.ClearEnemies();
        _currentWaveIndex = 0;
    }

    private void OnEnable()
    {
        _gameManager = GameManager.Ins;
        if (_gameManager != null)
            _gameManager.RegisterWaveManager(this);
    }

    private void Start()
    {
        if (startGameOnStart && _gameManager != null && _gameManager.WaveManager == this)
            _gameManager.StartGame();
    }

    private void Update()
    {
        if (!IsRunning)
            return;

        if (enemySpawner == null || !enemySpawner.isActiveAndEnabled)
        {
            if (_gameManager != null)
                _gameManager.EndGame();
            StopWave();
            return;
        }

        RemainingTime = Mathf.Max(0f, RemainingTime - Time.deltaTime);
        if (RemainingTime > 0f)
            return;

        int waveIndex = GetCurrentWaveData().WaveIndex;
        StopWave();
        _currentWaveIndex++;
        OnWaveEnd?.Invoke(waveIndex);
    }

    private void OnDisable()
    {
        StopWave();
        if (_gameManager != null)
            _gameManager.UnregisterWaveManager(this);
        _gameManager = null;
    }
}
