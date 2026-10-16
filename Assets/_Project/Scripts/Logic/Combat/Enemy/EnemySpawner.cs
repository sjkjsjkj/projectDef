using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>웨이브별 적 생성과 풀 수명을 관리합니다. 웨이브 전환 시 기존 적은 유지합니다.</summary>
public class EnemySpawner : BaseMono
{
    [Header("적 생성")]
    [SerializeField] private EnemyPath enemyPath;
    [Tooltip("WaveData의 프리팹이 비어 있을 때 사용할 공통 Enemy 프리팹입니다.")]
    [SerializeField] private EnemyMovement enemyPrefab;
    [Tooltip("INDEX,SPRITE,HP,SPEED 형식의 EnemyData.csv입니다. INDEX는 WaveIndex와 대응합니다.")]
    [SerializeField] private TextAsset enemyDataSheet;
    [Tooltip("적 처치 보상을 받을 전투의 자원 관리자입니다.")]
    [SerializeField] private ResourceManager resources;
    [Tooltip("비워두면 경로 첫 지점에서 생성합니다. 지정하면 목록 순서대로 사용합니다.")]
    [SerializeField] private List<Transform> spawnPoints = new List<Transform>();

    private readonly List<EnemyMovement> _activeEnemies = new List<EnemyMovement>();
    private EnemyPool _pool;
    private EnemyDataSheet _sheet;
    private string _loadedCsv;
    private EnemyData _waveEnemyData;
    private Sprite _waveSprite;
    private WaveData _wave;
    private int _spawnDataIndex;
    private int _spawnedInGroup;
    private int _spawnPointIndex;
    private float _spawnCooldown;

    public int ActiveEnemyCount => _activeEnemies.Count;
    public bool IsSpawning => _wave != null;

    /// <summary>현재 웨이브에서 아직 생성하지 않은 적 수입니다. 중단으로 취소된 소환은 제외합니다.</summary>
    public int RemainingSpawnCount
    {
        get
        {
            if (_wave == null) return 0;
            long remaining = -_spawnedInGroup;
            for (int i = _spawnDataIndex; i < _wave.EnemySpawnDatas.Count; i++)
                remaining += _wave.EnemySpawnDatas[i].Count;
            return (int)Math.Min(int.MaxValue, Math.Max(0, remaining));
        }
    }

    public bool SpawnWave(WaveData wave)
    {
        if (!isActiveAndEnabled || IsSpawning) return false;
        if (!HasValidPath() || wave == null || !wave.IsValid())
        {
            Debug.LogError("경로와 WaveData의 Id, 웨이브 번호, 적 프리팹, 소환 수, 보상을 확인하세요.", this);
            return false;
        }

        EnemyData roundData = null;
        Sprite sprite = null;
        foreach (EnemySpawnData data in wave.EnemySpawnDatas)
        {
            if (data.Bounties.Count > 0 && resources == null)
            {
                Debug.LogError("적 보상을 지급할 ResourceManager를 EnemySpawner에 연결하세요.", this);
                return false;
            }
            EnemyMovement prefab = data.EnemyPrefab != null ? data.EnemyPrefab : enemyPrefab;
            if (!IsValidPrefab(prefab))
            {
                Debug.LogError("유효한 EnemyMovement/EnemyHealth를 가진 적 프리팹을 연결하세요.", this);
                return false;
            }
            if (data.EnemyPrefab == null && roundData == null)
            {
                if (!TryGetRoundData(wave.WaveIndex, out roundData, out sprite)) return false;
                if (sprite != null && prefab.GetComponentInChildren<SpriteRenderer>(true) == null)
                {
                    Debug.LogError("CSV 스프라이트를 적용할 SpriteRenderer가 공통 Enemy에 없습니다.", this);
                    return false;
                }
            }
        }

        _waveEnemyData = roundData;
        _waveSprite = sprite;
        _wave = wave;
        _spawnDataIndex = 0;
        _spawnedInGroup = 0;
        _spawnPointIndex = 0;
        _spawnCooldown = 0f;
        return true;
    }

    // 기존 단일 소환은 공통 프리팹 자체의 능력치를 사용합니다.
    public void SpawnEnemy()
    {
        if (!IsSpawning) SpawnEnemy(enemyPrefab, null, null, null);
    }

    private EnemyMovement SpawnEnemy(EnemyMovement prefab, EnemyData data, Sprite sprite, IReadOnlyList<Bounty> bounties)
    {
        if (!isActiveAndEnabled) return null;
        if (!HasValidPath() || !IsValidPrefab(prefab) ||
            (bounties != null && bounties.Count > 0 && resources == null))
        {
            Debug.LogError("적 프리팹, 경로 또는 보상 지급 설정이 올바르지 않습니다.", this);
            return null;
        }

        Transform spawnPoint = null;
        if (spawnPoints.Count > 0)
        {
            spawnPoint = spawnPoints[_spawnPointIndex % spawnPoints.Count];
            _spawnPointIndex++;
        }

        EnemyMovement enemy = null;
        try
        {
            if (_pool == null) _pool = new EnemyPool(transform);
            enemy = _pool.Rent(prefab);
            if (enemy == null) return null;
            enemy.transform.rotation = prefab.transform.rotation;
            enemy.ApplyData(data, sprite);
            enemy.GetComponent<EnemyHealth>().InitializeBounties(resources, bounties);
            enemy.Init(enemyPath, spawnPoint);
            _activeEnemies.Add(enemy);
            enemy.OnRemoved += OnEnemyRemoved;
            // 등록 이벤트가 게임 오버를 발생시킬 수 있으므로 모든 상태를 준비한 뒤 활성화합니다.
            enemy.gameObject.SetActive(true);
            return enemy;
        }
        catch (Exception exception)
        {
            if (enemy != null) enemy.Despawn();
            Debug.LogException(exception, this);
            return null;
        }
    }

    private bool TryGetRoundData(int round, out EnemyData data, out Sprite sprite)
    {
        data = null;
        sprite = null;
        if (enemyDataSheet == null)
        {
            Debug.LogError("EnemySpawner에 EnemyData.csv를 연결하세요.", this);
            return false;
        }
        string csv = enemyDataSheet.text;
        if (_sheet == null || _loadedCsv != csv)
        {
            if (!EnemyDataSheet.TryParse(csv, out _sheet, out string error))
            {
                Debug.LogError(error, this);
                return false;
            }
            _loadedCsv = csv;
        }
        if (!_sheet.TryGet(round, out data))
        {
            Debug.LogError($"EnemyData.csv에 현재 라운드 INDEX {round}가 없습니다.", this);
            return false;
        }
        if (!string.IsNullOrEmpty(data.SpritePath))
        {
            sprite = Resources.Load<Sprite>(data.SpritePath);
            if (sprite == null)
            {
                Debug.LogError($"EnemyData.csv INDEX {round}: Resources 스프라이트를 찾을 수 없습니다: {data.SpritePath}", this);
                return false;
            }
        }
        return true;
    }

    private static bool IsValidPrefab(EnemyMovement prefab)
    {
        if (prefab == null || !prefab.enabled || !prefab.TryGetComponent(out EnemyHealth health) || !health.enabled)
            return false;
        float hp = health.MaxHealth;
        float speed = prefab.MoveSpeed;
        return hp > 0f && !float.IsNaN(hp) && !float.IsInfinity(hp) &&
            speed >= 0f && !float.IsNaN(speed) && !float.IsInfinity(speed);
    }

    public void StopSpawning()
    {
        _wave = null;
        _waveEnemyData = null;
        _waveSprite = null;
        _spawnCooldown = 0f;
    }

    public void ClearEnemies()
    {
        StopSpawning();
        // 반환 이벤트가 목록을 수정하므로 먼저 목록에서 제거합니다.
        while (_activeEnemies.Count > 0)
        {
            int last = _activeEnemies.Count - 1;
            EnemyMovement enemy = _activeEnemies[last];
            _activeEnemies.RemoveAt(last);
            if (enemy == null) continue;
            enemy.OnRemoved -= OnEnemyRemoved;
            enemy.Despawn();
        }
    }

    private bool HasValidPath() => enemyPath != null && enemyPath.WaypointCount > 0;

    private void OnEnemyRemoved(EnemyMovement enemy)
    {
        enemy.OnRemoved -= OnEnemyRemoved;
        _activeEnemies.Remove(enemy);
    }

    private void Update()
    {
        if (!IsSpawning || Time.deltaTime <= 0f) return;
        _spawnCooldown = Mathf.Max(0f, _spawnCooldown - Time.deltaTime);
        if (_spawnCooldown > 0f) return;

        WaveData wave = _wave;
        EnemySpawnData data = wave.EnemySpawnDatas[_spawnDataIndex];
        bool useSheet = data.EnemyPrefab == null;
        if (SpawnEnemy(useSheet ? enemyPrefab : data.EnemyPrefab,
            useSheet ? _waveEnemyData : null, useSheet ? _waveSprite : null, data.Bounties) == null)
        {
            enabled = false;
            return;
        }
        if (_wave != wave) return;
        _spawnCooldown = wave.SpawnInterval;
        _spawnedInGroup++;
        if (_spawnedInGroup >= data.Count)
        {
            _spawnedInGroup = 0;
            _spawnDataIndex++;
            if (_spawnDataIndex >= wave.EnemySpawnDatas.Count) StopSpawning();
        }
    }

    private void OnDisable() => ClearEnemies();

    private void OnDestroy()
    {
        ClearEnemies();
        _pool?.Dispose();
        _pool = null;
    }

#if UNITY_EDITOR
    protected override void OnValidate()
    {
        base.OnValidate();
        // 실제 참조를 직렬화하므로 플레이어 빌드에서도 프로젝트 폴더를 직접 읽지 않습니다.
        if (enemyPrefab == null)
            enemyPrefab = UnityEditor.AssetDatabase.LoadAssetAtPath<EnemyMovement>(
                "Assets/_Project/02_Prefab/BattleScene/Enemy.prefab");
        if (enemyDataSheet == null)
            enemyDataSheet = UnityEditor.AssetDatabase.LoadAssetAtPath<TextAsset>(
                "Assets/_Project/05_Docs/Data/EnemyData.csv");
    }
#endif
}
