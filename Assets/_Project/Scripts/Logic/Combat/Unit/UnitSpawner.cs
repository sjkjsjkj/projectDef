using UnityEngine;

/// <summary>상점과 이벤트에서 공통으로 사용하는 유닛 획득(생성 또는 중복 레벨업) 담당입니다.</summary>
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
    /// 동일 유닛을 보유 중이면 레벨업하고 반환하며, 처음 획득할 때만 생성 및 배치합니다.
    /// 골드나 판매 상태는 호출자가 관리합니다. 최대 레벨 중복은 실패로 반환합니다.
    /// 실패하면 이번 호출에서 생성한 유닛과 슬롯 점유를 정리합니다.
    /// </summary>
    public bool TrySpawn(UnitData data, out Unit unit, out string reason)
    {
        unit = null;
        reason = null;
        if (_isSpawning) { reason = "유닛 생성 처리 중입니다."; return false; }
        if (!isActiveAndEnabled) { reason = "유닛 생성기가 비활성 상태입니다."; return false; }
        if (data == null) { reason = "생성할 장수 데이터가 없습니다."; return false; }

        // 대기 칸/프리팹이 없어도 전투 또는 대기 중인 기존 장수는 강화할 수 있습니다.
        Unit owned = FindOwnedUnit(data);
        if (owned != null)
        {
            if (owned.IsMaxLevel)
            { reason = "이미 최대 레벨(10)인 장수입니다."; return false; }

            _isSpawning = true;
            try
            {
                if (!owned.TryLevelUp())
                { reason = "장수의 레벨을 올릴 수 없습니다."; return false; }
                unit = owned;
                return true;
            }
            finally { _isSpawning = false; }
        }

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

    private Unit FindOwnedUnit(UnitData data)
    {
        // 현재 구조에서 보유 유닛은 같은 필드 씬의 슬롯에 등록된 아군입니다.
        // 비활성 슬롯도 포함하고, 미배치 테스트 유닛/생성 대기 객체는 제외합니다.
        var fieldScene = waitField != null ? waitField.gameObject.scene : gameObject.scene;
        foreach (Slot slot in FindObjectsByType<Slot>(FindObjectsInactive.Include, FindObjectsSortMode.InstanceID))
        {
            if (slot.gameObject.scene != fieldScene || slot.Occupant is not Unit candidate ||
                candidate == null || !ReferenceEquals(candidate.CurrentTarget, slot))
                continue;

            UnitData ownedData = candidate.Data;
            if (ownedData == data || (ownedData != null && !string.IsNullOrWhiteSpace(data.Id) &&
                string.Equals(ownedData.Id, data.Id, System.StringComparison.Ordinal)))
                return candidate;
        }
        return null;
    }
}
