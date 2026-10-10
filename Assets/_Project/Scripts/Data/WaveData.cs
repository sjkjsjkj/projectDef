using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>웨이브 정의 데이터입니다. 실행 중 타이머와 소환 상태는 매니저가 관리합니다.</summary>
[CreateAssetMenu(fileName = "WaveData_", menuName = "ScriptableObjects/Wave/WaveData")]
public class WaveData : BaseSO
{
    [Header("웨이브")]
    [SerializeField, Min(1)] private int waveIndex = 1;
    [SerializeField, Min(0.1f)] private float duration = 30f;
    [SerializeField, Min(0f)] private float spawnInterval = 1f;
    [Tooltip("목록 순서대로 소환합니다. 시간이 끝나면 아직 소환하지 않은 수량은 취소됩니다.")]
    [SerializeField] private List<EnemySpawnData> enemySpawnDatas = new List<EnemySpawnData>();

    public int WaveIndex => waveIndex;
    public float Duration => Mathf.Max(0.1f, duration);
    public float SpawnInterval => Mathf.Max(0f, spawnInterval);
    public IReadOnlyList<EnemySpawnData> EnemySpawnDatas => enemySpawnDatas;

    public override bool IsValid()
    {
        if (!base.IsValid() || waveIndex < 1 || enemySpawnDatas == null || enemySpawnDatas.Count == 0)
            return false;

        foreach (EnemySpawnData data in enemySpawnDatas)
        {
            if (data == null || data.Count < 1 || data.EnemyPrefab == null ||
                !data.EnemyPrefab.gameObject.activeSelf || !data.EnemyPrefab.enabled ||
                data.EnemyPrefab.GetComponent<EnemyHealth>() == null || !data.HasValidBounties())
                return false;
        }
        return true;
    }
}

[Serializable]
public class EnemySpawnData
{
    [SerializeField] private EnemyMovement enemyPrefab;
    [SerializeField, Min(1)] private int count = 30;
    [Tooltip("적 한 마리를 처치했을 때 지급하는 보상입니다. 빈 목록은 보상 없음입니다.")]
    [SerializeReference] private List<Bounty> bounties = new List<Bounty>();

    public EnemyMovement EnemyPrefab => enemyPrefab;
    public int Count => count;
    public IReadOnlyList<Bounty> Bounties => bounties;

    public bool HasValidBounties()
    {
        if (bounties == null) return false;
        foreach (Bounty bounty in bounties)
            if (bounty == null || !bounty.IsValid) return false;
        return true;
    }
}
