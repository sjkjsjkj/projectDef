using System;
using UnityEngine;

/// <summary>객잔의 다섯 판매 칸, 초기화권 사용 및 대기 필드로의 구매 배치를 담당합니다.</summary>
public class InnManager : BaseMono
{
    public const int OfferCount = 5;

    [SerializeField] private InnData innData;
    [SerializeField] private ResourceManager resources;
    [SerializeField] private WaitField waitField;
    [Tooltip("구매 유닛의 부모입니다. 객잔 UI 아래에 두지 마세요.")]
    [SerializeField] private Transform unitRoot;

    private Unit[] _offers = new Unit[OfferCount];
    private readonly bool[] _sold = new bool[OfferCount];
    private readonly System.Random _random = new System.Random();
    private bool _busy;

    public ResourceManager Resources => resources;
    public bool IsInitialized { get; private set; }
    public event Action OnOffersChanged;

    public UnitData GetOffer(int index) => IsValidIndex(index) && _offers[index] != null ? _offers[index].Data : null;
    public bool IsSold(int index) => IsValidIndex(index) && _sold[index];

    /// <summary>첫 목록만 무료로 구성합니다. UI를 다시 열어도 목록은 유지됩니다.</summary>
    public bool TryInitialize()
    {
        if (IsInitialized) return true;
        if (_busy || resources == null || innData == null ||
            !innData.TryRollOffers(resources.HonorLevel, _random, out Unit[] offers))
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
        if (!innData.TryRollOffers(resources.HonorLevel, _random, out Unit[] offers))
        { reason = "등장 확률과 등급별 장수 프리팹을 확인하세요."; return false; }

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

        _busy = true;
        Unit unit = null;
        bool purchased = false;
        try
        {
            unit = Instantiate(_offers[index], unitRoot);
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

    private void SetOffers(Unit[] offers)
    {
        _offers = offers;
        Array.Clear(_sold, 0, _sold.Length);
        IsInitialized = true;
        OnOffersChanged?.Invoke();
    }

    private void Start()
    {
        if (!TryInitialize())
            Debug.LogError("객잔 초기화 실패: InnData, ResourceManager, 등장 가능한 등급의 프리팹을 확인하세요.", this);
    }
}
