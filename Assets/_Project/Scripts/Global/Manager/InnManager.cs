using System;
using UnityEngine;
using UnityEngine.Serialization;

/// <summary>객잔의 판매 목록과 구매 비용을 관리하며 유닛 획득은 UnitSpawner에 요청합니다.</summary>
public class InnManager : BaseMono
{
    public const int OfferCount = 5;

    [SerializeField] private InnData innData;
    [SerializeField] private ResourceManager resources;
    [SerializeField] private UnitSpawner unitSpawner;
    [SerializeField] private bool createUIOnStart = true;
    [SerializeField] private TMPro.TMP_FontAsset uiFont;

    // 기존 씬/프리팹의 참조를 보존하기 위한 이전 전용 필드입니다. 구매 로직에서는 사용하지 않습니다.
    [FormerlySerializedAs("waitField"), SerializeField, HideInInspector] private WaitField legacyWaitField;
    [FormerlySerializedAs("defaultUnitPrefab"), SerializeField, HideInInspector] private Unit legacyDefaultUnitPrefab;
    [FormerlySerializedAs("unitRoot"), SerializeField, HideInInspector] private Transform legacyUnitRoot;

    private UnitData[] _offers = new UnitData[OfferCount];
    private InnPanel _generatedPanel;
    private readonly bool[] _sold = new bool[OfferCount];
    private readonly System.Random _random = new System.Random();
    private bool _busy;

    public ResourceManager Resources => resources;
    public TMPro.TMP_FontAsset UIFont => uiFont;
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
        EnsureUnitSpawner();
        if (unitSpawner == null)
        { reason = "유닛 생성기를 연결하세요."; return false; }

        _busy = true;
        int price = data.Price;
        bool paid = false;
        bool purchased = false;
        try
        {
            // 활성화 콜백보다 먼저 비용을 확보합니다. 획득 실패 시 비용과 판매 상태를 복구합니다.
            _sold[index] = true;
            paid = resources.TrySpendGold(price);
            if (!paid)
            {
                reason = "돈이 부족합니다.";
                return false;
            }
            if (!unitSpawner.TrySpawn(data, out _, out reason))
                return false;

            purchased = true;
            OnOffersChanged?.Invoke();
            return true;
        }
        finally
        {
            try
            {
                if (!purchased)
                {
                    _sold[index] = false;
                    if (paid) resources.AddGold(price);
                    OnOffersChanged?.Invoke();
                }
            }
            finally { _busy = false; }
        }
    }

    // 예전 InnManager만 연결된 씬도 실행할 수 있도록 설정을 한 번 옮깁니다.
    [ContextMenu("기존 유닛 생성 설정을 UnitSpawner로 이전")]
    private void EnsureUnitSpawner()
    {
        if (unitSpawner != null) return;
#if UNITY_EDITOR
        if (!Application.isPlaying) UnityEditor.Undo.RecordObject(this, "유닛 생성 설정 이전");
#endif
        unitSpawner = GetComponent<UnitSpawner>();
        if (unitSpawner != null || legacyWaitField == null) return;
#if UNITY_EDITOR
        unitSpawner = Application.isPlaying ? gameObject.AddComponent<UnitSpawner>()
            : UnityEditor.Undo.AddComponent<UnitSpawner>(gameObject);
#else
        unitSpawner = gameObject.AddComponent<UnitSpawner>();
#endif
        unitSpawner.Configure(legacyWaitField, legacyDefaultUnitPrefab, legacyUnitRoot);
        legacyWaitField = null;
        legacyDefaultUnitPrefab = null;
        legacyUnitRoot = null;
#if UNITY_EDITOR
        if (!Application.isPlaying)
        {
            UnityEditor.EditorUtility.SetDirty(this);
            UnityEditor.EditorUtility.SetDirty(unitSpawner);
            UnityEditor.PrefabUtility.RecordPrefabInstancePropertyModifications(this);
        }
#endif
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
        EnsureUnitSpawner();
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
