using System;
using UnityEngine;

/// <summary>객잔의 다섯 판매 칸, 초기화권 사용 및 대기 필드로의 구매 배치를 담당합니다.</summary>
public class InnManager : BaseMono
{
    public const int OfferCount = 5;

    [SerializeField] private InnData innData;
    [SerializeField] private ResourceManager resources;
    [SerializeField] private WaitField waitField;
    [Tooltip("UnitData.Prefab이 없는 장수에 공통으로 사용하는 월드 유닛 프리팹입니다.")]
    [SerializeField] private Unit defaultUnitPrefab;
    [SerializeField] private bool createUIOnStart = true;
    [SerializeField] private TMPro.TMP_FontAsset uiFont;
    [Tooltip("구매 유닛의 부모입니다. 객잔 UI 아래에 두지 마세요.")]
    [SerializeField] private Transform unitRoot;

    private UnitData[] _offers = new UnitData[OfferCount];
    private Transform _spawnRoot;
    private InnPanel _generatedPanel;
    private readonly bool[] _sold = new bool[OfferCount];
    private readonly System.Random _random = new System.Random();
    private bool _busy;

    public ResourceManager Resources => resources;
    public bool IsInitialized { get; private set; }
    public event Action OnOffersChanged;

    public UnitData GetOffer(int index) => IsValidIndex(index) ? _offers[index] : null;
    public bool IsSold(int index) => IsValidIndex(index) && _sold[index];

    /// <summary>첫 목록만 무료로 구성합니다. UI를 다시 열어도 목록은 유지됩니다.</summary>
    public bool TryInitialize()
    {
        if (IsInitialized) return true;
        if (_busy || resources == null || innData == null ||
            !TryRollOffers(out UnitData[] offers))
            return false;
        SetOffers(offers);
        return true;
    }

    public bool TryRefresh(out string reason)
    {
        reason = null;
        if (_busy) { reason = "객잔 처리 중입니다."; return false; }
        if (!IsInitialized || resources == null || innData == null)
        { reason = "객잔 설정을 확인하세요."; return false; }
        if (resources.InnRefreshTickets < 1)
        { reason = "객잔 목록 초기화권이 부족합니다."; return false; }
        if (!TryRollOffers(out UnitData[] offers))
        { reason = "유닛 테이블과 등급별 등장 확률을 확인하세요."; return false; }

        _busy = true;
        try
        {
            if (!resources.TrySpendInnRefreshTicket())
            { reason = "객잔 목록 초기화권이 부족합니다."; return false; }
            SetOffers(offers);
            return true;
        }
        finally { _busy = false; }
    }

    public bool TryPurchase(int index, out string reason)
    {
        reason = null;
        if (_busy) { reason = "객잔 처리 중입니다."; return false; }
        UnitData data = GetOffer(index);
        if (!IsInitialized || data == null || IsSold(index))
        { reason = "구매할 수 없는 장수입니다."; return false; }
        if (resources == null || resources.Gold < data.Price)
        { reason = "돈이 부족합니다."; return false; }
        if (waitField == null || waitField.GetEmptySlot() == null)
        { reason = "빈 대기 슬롯이 없습니다."; return false; }
        if (unitRoot != null && !unitRoot.gameObject.activeInHierarchy)
        { reason = "유닛 배치 루트가 비활성 상태입니다."; return false; }
        Unit prefab = data.Prefab != null ? data.Prefab : defaultUnitPrefab;
        if (prefab == null || !prefab.enabled)
        { reason = "장수 생성용 프리팹을 연결하세요."; return false; }

        _busy = true;
        Unit unit = null;
        bool purchased = false;
        try
        {
            if (_spawnRoot == null)
            {
                var staging = new GameObject("InnSpawnStaging");
                staging.SetActive(false);
                staging.transform.SetParent(transform, false);
                _spawnRoot = staging.transform;
            }
            // 활성화 전에 데이터를 주입해야 전투 컨트롤러도 선택한 장수의 스킬로 초기화됩니다.
            unit = Instantiate(prefab, _spawnRoot);
            unit.gameObject.SetActive(false);
            unit.InitializeForSpawn(data);
            unit.name = data.UnitName;
            unit.transform.SetParent(unitRoot, false);
            if (!waitField.TryAddUnit(unit))
            { reason = "대기 슬롯에 장수를 배치할 수 없습니다."; return false; }

            // 자원 변경 이벤트가 같은 판매 칸을 다시 구매하지 못하게 잠급니다.
            _sold[index] = true;
            if (!resources.TrySpendGold(data.Price))
            {
                _sold[index] = false;
                reason = "돈이 부족합니다.";
                return false;
            }
            purchased = true;
            unit.gameObject.SetActive(true);
            OnOffersChanged?.Invoke();
            return true;
        }
        finally
        {
            if (!purchased && unit != null)
            {
                unit.CurrentTarget?.Remove(unit);
                unit.gameObject.SetActive(false);
                Destroy(unit.gameObject);
            }
            _busy = false;
        }
    }

    private static bool IsValidIndex(int index) => index >= 0 && index < OfferCount;

    private bool TryRollOffers(out UnitData[] offers)
    {
        offers = null;
        DatabaseManager database = DatabaseManager.Ins;
        return database != null && innData.TryRollOffers(database.Units, resources.HonorLevel, _random, out offers);
    }

    private void SetOffers(UnitData[] offers)
    {
        _offers = offers;
        Array.Clear(_sold, 0, _sold.Length);
        IsInitialized = true;
        OnOffersChanged?.Invoke();
    }

    private void Start()
    {
        if (!TryInitialize())
            Debug.LogError("객잔 초기화 실패: InnData, ResourceManager, Resources/Table의 UnitTable을 확인하세요.", this);
        if (createUIOnStart)
        {
            foreach (InnPanel panel in FindObjectsByType<InnPanel>(FindObjectsInactive.Include, FindObjectsSortMode.None))
                if (panel.Manager == this) return;
            _generatedPanel = InnUIFactory.Create(this, uiFont);
        }
    }

    private void OnDestroy()
    {
        if (_generatedPanel != null) Destroy(_generatedPanel.gameObject);
    }
}
