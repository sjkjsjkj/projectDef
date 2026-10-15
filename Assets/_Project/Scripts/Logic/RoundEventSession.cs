using System;
using System.Collections.Generic;

/// <summary>이벤트 한 번의 실행 상태입니다. 카드별 무료 횟수와 공통 유료 횟수를 분리합니다.</summary>
public sealed class RoundEventSession
{
    private readonly RoundEventOptionData[] _offers;
    private readonly bool[] _freeRefreshUsed;
    private readonly List<RoundEventOptionData> _pool;
    private readonly Random _random;
    private readonly RoundEventContext _context;
    private bool _busy;
    private bool _cancelled;

    public RoundEventData Data { get; }
    public int ChoiceCount => _offers.Length;
    public int PaidRefreshCount { get; private set; }
    public int PaidRefreshCost => (int)Math.Min(int.MaxValue,
        (long)Data.InitialRefreshCost + (long)PaidRefreshCount * Data.RefreshCostIncrease);
    public bool IsCompleted { get; private set; }
    public bool IsActive => !IsCompleted && !_cancelled;

    public RoundEventSession(RoundEventData data, RoundEventContext context, Random random = null)
    {
        if (data == null || !data.IsValid()) throw new ArgumentException("이벤트 데이터가 올바르지 않습니다.", nameof(data));
        _context = context ?? throw new ArgumentNullException(nameof(context));
        Data = data;
        _random = random ?? new Random();
        _pool = new List<RoundEventOptionData>(data.Options);
        _offers = new RoundEventOptionData[data.ChoiceCount];
        _freeRefreshUsed = new bool[data.ChoiceCount];
        var candidates = new List<RoundEventOptionData>(_pool);
        for (int i = 0; i < _offers.Length; i++)
        {
            int pick = _random.Next(candidates.Count);
            _offers[i] = candidates[pick];
            candidates.RemoveAt(pick);
        }
    }

    public RoundEventOptionData GetOffer(int index) => ValidIndex(index) ? _offers[index] : null;
    public bool HasFreeRefresh(int index) => ValidIndex(index) && !_freeRefreshUsed[index];
    public int GetRefreshCost(int index) => HasFreeRefresh(index) ? 0 : PaidRefreshCost;
    public void Cancel() => _cancelled = true;

    public bool CanRefresh(int index)
    {
        return IsActive && !_busy && ValidIndex(index) && Candidates().Count > 0 &&
            (HasFreeRefresh(index) || (_context.Resources != null && _context.Resources.Gold >= PaidRefreshCost));
    }

    public bool TryRefresh(int index, out string reason)
    {
        reason = null;
        if (!IsActive || _busy || !ValidIndex(index))
        { reason = "새로고침할 수 없는 선택지입니다."; return false; }
        List<RoundEventOptionData> candidates = Candidates();
        if (candidates.Count == 0)
        { reason = "다른 선택지 후보가 없습니다."; return false; }
        bool free = HasFreeRefresh(index);
        int cost = PaidRefreshCost;
        if (!free && (_context.Resources == null || _context.Resources.Gold < cost))
        { reason = "골드가 부족합니다."; return false; }
        _busy = true;
        try
        {
            if (!free && !_context.Resources.TrySpendGold(cost))
            { reason = "골드가 부족합니다."; return false; }
            // 재화 변경 콜백 중 게임이 종료되었다면 교체하지 않고 결제도 되돌립니다.
            if (!IsActive)
            {
                if (!free) _context.Resources.AddGold(cost);
                reason = "이벤트가 종료되었습니다.";
                return false;
            }
            _offers[index] = candidates[_random.Next(candidates.Count)];
            if (free) _freeRefreshUsed[index] = true;
            else if (PaidRefreshCount < int.MaxValue) PaidRefreshCount++;
            return true;
        }
        finally { _busy = false; }
    }

    public bool TrySelect(int index, out string reason)
    {
        reason = null;
        if (!IsActive || _busy || !ValidIndex(index))
        { reason = "선택할 수 없는 상태입니다."; return false; }
        RoundEventOptionData option = _offers[index];
        if (option == null || !option.IsValid())
        { reason = "선택지 효과 설정을 확인하세요."; return false; }
        _busy = true;
        try
        {
            if (!option.Effect.TryApply(_context, out reason)) return false;
            IsCompleted = true;
            return true;
        }
        finally { _busy = false; }
    }

    private bool ValidIndex(int index) => index >= 0 && index < _offers.Length;

    private List<RoundEventOptionData> Candidates()
    {
        var candidates = new List<RoundEventOptionData>();
        foreach (RoundEventOptionData option in _pool)
            if (option != null && option.IsValid() && Array.IndexOf(_offers, option) < 0)
                candidates.Add(option);
        return candidates;
    }
}
