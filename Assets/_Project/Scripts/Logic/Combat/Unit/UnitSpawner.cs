using UnityEngine;

/// <summary>상점과 이벤트에서 공통으로 사용하는 아군 유닛 생성 및 대기 슬롯 배치 담당입니다.</summary>
[DisallowMultipleComponent]
public class UnitSpawner : BaseMono
{
    [SerializeField] private WaitField waitField;
    [Tooltip("UnitData.Prefab이 비어 있을 때 사용하는 공통 유닛 프리팹입니다.")]
    [SerializeField] private Unit defaultUnitPrefab;
    [Tooltip("생성한 유닛의 부모입니다. UI 밖의 항상 활성인 월드 오브젝트를 지정하세요.")]
    [SerializeField] private Transform unitRoot;

    private Transform _spawnRoot;
    private bool _isSpawning;

    /// <summary>기존 씬 설정 이전 또는 코드 기반 초기화에 사용합니다.</summary>
    public void Configure(WaitField field, Unit fallbackPrefab, Transform root)
    {
        if (_isSpawning)
            throw new System.InvalidOperationException("유닛 생성 중에는 배치 설정을 변경할 수 없습니다.");
        waitField = field;
        defaultUnitPrefab = fallbackPrefab;
        unitRoot = root;
    }

    /// <summary>
    /// 데이터를 받아 유닛 생성과 배치를 완료합니다. 골드나 판매 상태는 호출자가 관리합니다.
    /// 실패하면 이번 호출에서 생성한 유닛과 슬롯 점유를 정리합니다.
    /// 향후 동일 유닛 합성 판정은 이 진입점에서 빈 슬롯 검사보다 먼저 처리합니다.
    /// </summary>
    public bool TrySpawn(UnitData data, out Unit unit, out string reason)
    {
        unit = null;
        reason = null;
        if (_isSpawning) { reason = "유닛 생성 처리 중입니다."; return false; }
        if (!isActiveAndEnabled) { reason = "유닛 생성기가 비활성 상태입니다."; return false; }
        if (data == null) { reason = "생성할 장수 데이터가 없습니다."; return false; }
        if (waitField == null || waitField.GetEmptySlot() == null)
        { reason = "빈 대기 슬롯이 없습니다."; return false; }
        if (unitRoot != null && !unitRoot.gameObject.activeInHierarchy)
        { reason = "유닛 배치 루트가 비활성 상태입니다."; return false; }

        Unit prefab = data.Prefab != null ? data.Prefab : defaultUnitPrefab;
        if (prefab == null || !prefab.enabled)
        { reason = "장수 생성용 프리팹을 연결하세요."; return false; }

        _isSpawning = true;
        Unit created = null;
        bool completed = false;
        try
        {
            if (_spawnRoot == null)
            {
                var staging = new GameObject("UnitSpawnStaging");
                staging.SetActive(false);
                staging.transform.SetParent(transform, false);
                _spawnRoot = staging.transform;
            }

            // Awake가 실행되기 전에 선택한 장수 데이터를 주입합니다.
            created = Instantiate(prefab, _spawnRoot);
            created.gameObject.SetActive(false);
            created.InitializeForSpawn(data);
            created.name = data.UnitName;
            created.transform.SetParent(unitRoot, false);
            if (!waitField.TryAddUnit(created))
            { reason = "대기 슬롯에 장수를 배치할 수 없습니다."; return false; }

            created.gameObject.SetActive(true);
            unit = created;
            completed = true;
            return true;
        }
        finally
        {
            try
            {
                if (!completed && created != null)
                {
                    created.CurrentTarget?.Remove(created);
                    created.gameObject.SetActive(false);
                    Destroy(created.gameObject);
                }
            }
            finally { _isSpawning = false; }
        }
    }
}
