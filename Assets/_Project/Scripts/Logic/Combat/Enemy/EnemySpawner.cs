using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 웨이브 데이터에 따라 적을 소환합니다. 웨이브 전환 시 기존 적은 유지합니다.
/// </summary>
public class EnemySpawner : BaseMono
{
    [Header("적 생성")]
    [SerializeField] private EnemyPath enemyPath;
    [SerializeField] private EnemyMovement enemyPrefab;
    [Tooltip("적 처치 보상을 받을 전투의 자원 관리자입니다.")]
    [SerializeField] private ResourceManager resources;
    [Tooltip("비워두면 경로 첫 지점에서 생성합니다. 지정하면 목록 순서대로 사용합니다.")]
    [SerializeField] private List<Transform> spawnPoints = new List<Transform>();

    private readonly List<EnemyMovement> _activeEnemies = new List<EnemyMovement>();
    private WaveData _wave;
    private int _spawnDataIndex;
    private int _spawnedInGroup;
    private int _spawnPointIndex;
    private float _spawnCooldown;

    public int ActiveEnemyCount => _activeEnemies.Count;
    public bool IsSpawning => _wave != null;

    public bool SpawnWave(WaveData wave)
    {
        if (!isActiveAndEnabled || IsSpawning)
            return false;

        if (!HasValidPath() || wave == null || !wave.IsValid())
        {
            Debug.LogError("경로와 WaveData의 Id, 웨이브 번호, 적 프리팹, 소환 수, 보상을 확인하세요.", this);
            return false;
        }

        foreach (EnemySpawnData data in wave.EnemySpawnDatas)
        {
            if (data.Bounties.Count > 0 && resources == null)
            {
                Debug.LogError("적 보상을 지급할 ResourceManager를 EnemySpawner에 연결하세요.", this);
                return false;
            }
        }

        _wave = wave;
        _spawnDataIndex = 0;
        _spawnedInGroup = 0;
        _spawnPointIndex = 0;
        _spawnCooldown = 0f;
        return true;
    }

    // 기존 단일 소환 호출과의 호환용입니다.
    public void SpawnEnemy()
    {
        if (!IsSpawning)
            SpawnEnemy(enemyPrefab);
    }

    private EnemyMovement SpawnEnemy(EnemyMovement prefab, IReadOnlyList<Bounty> bounties = null)
    {
        if (!isActiveAndEnabled)
            return null;

        if (bounties != null && bounties.Count > 0 && resources == null)
        {
            Debug.LogError("적 보상을 지급할 ResourceManager가 없습니다.", this);
            return null;
        }

        if (!HasValidPath() || prefab == null || !prefab.gameObject.activeSelf ||
            !prefab.enabled || prefab.GetComponent<EnemyHealth>() == null)
        {
            Debug.LogError("활성 적 프리팹에 EnemyMovement/EnemyHealth와 유효한 EnemyPath가 필요합니다.", this);
            return null;
        }

        Transform spawnPoint = null;
        if (spawnPoints.Count > 0)
        {
            spawnPoint = spawnPoints[_spawnPointIndex % spawnPoints.Count];
            _spawnPointIndex++;
        }

        Vector3 position = spawnPoint != null ? spawnPoint.position : enemyPath.GetWaypoint(0);
        EnemyMovement enemy = Instantiate(prefab, position, prefab.transform.rotation, transform);
        enemy.GetComponent<EnemyHealth>().InitializeBounties(resources, bounties);
        _activeEnemies.Add(enemy);
        enemy.OnRemoved += OnEnemyRemoved;
        enemy.Init(enemyPath, spawnPoint);
        return enemy;
    }

    public void StopSpawning()
    {
        _wave = null;
        _spawnCooldown = 0f;
    }

    public void ClearEnemies()
    {
        StopSpawning();
        foreach (EnemyMovement enemy in _activeEnemies)
        {
            if (enemy == null)
                continue;

            enemy.OnRemoved -= OnEnemyRemoved;
            enemy.gameObject.SetActive(false);
            Destroy(enemy.gameObject);
        }
        _activeEnemies.Clear();
    }

    private bool HasValidPath()
    {
        return enemyPath != null && enemyPath.WaypointCount > 0;
    }

    private void OnEnemyRemoved(EnemyMovement enemy)
    {
        enemy.OnRemoved -= OnEnemyRemoved;
        _activeEnemies.Remove(enemy);
    }

    private void Update()
    {
        if (!IsSpawning || Time.deltaTime <= 0f)
            return;

        _spawnCooldown = Mathf.Max(0f, _spawnCooldown - Time.deltaTime);
        if (_spawnCooldown > 0f)
            return;

        WaveData wave = _wave;
        EnemySpawnData data = wave.EnemySpawnDatas[_spawnDataIndex];
        if (SpawnEnemy(data.EnemyPrefab, data.Bounties) == null)
        {
            enabled = false;
            return;
        }

        // Instantiate 중 EnemyHealth 등록으로 게임 오버가 발생할 수 있습니다.
        if (_wave != wave)
            return;

        _spawnCooldown = wave.SpawnInterval;
        _spawnedInGroup++;
        if (_spawnedInGroup >= data.Count)
        {
            _spawnedInGroup = 0;
            _spawnDataIndex++;
            if (_spawnDataIndex >= _wave.EnemySpawnDatas.Count)
                StopSpawning();
        }
    }

    private void OnDisable()
    {
        ClearEnemies();
    }
}
