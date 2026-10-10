using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 시너지/이벤트의 전체 유닛 스탯 효과를 출처별로 관리합니다.
/// 고정값은 합산하고 배율은 곱산합니다. 씬 전환 시 함께 제거되는 관리자입니다.
/// </summary>
[DisallowMultipleComponent]
public class GlobalUnitStatManager : Singleton<GlobalUnitStatManager>
{
    private readonly Dictionary<string, UnitStatModifier[]> _effects =
        new Dictionary<string, UnitStatModifier[]>(StringComparer.Ordinal);
    private readonly Dictionary<Unit, UnitStats> _units = new Dictionary<Unit, UnitStats>();
    private readonly List<Unit> _destroyedUnits = new List<Unit>();
    private Dictionary<UnitStatType, UnitStatModifier> _combined =
        new Dictionary<UnitStatType, UnitStatModifier>();

    public int EffectCount => _effects.Count;

    // Singleton.Ins와 Awake에서 여러 번 호출해도 현재 효과를 초기화하지 않습니다.
    public override void Initialize() { }

    /// <summary>
    /// 같은 ID는 기존 효과 전체를 교체합니다. 서로 다른 ID의 효과만 중첩됩니다.
    /// 빈 목록은 해당 효과를 제거합니다. 전달한 배열은 복사해 보관합니다.
    /// </summary>
    public void SetEffect(string sourceId, params UnitStatModifier[] modifiers)
    {
        if (string.IsNullOrWhiteSpace(sourceId))
            throw new ArgumentException("효과의 출처 ID가 필요합니다.", nameof(sourceId));
        if (modifiers == null)
            throw new ArgumentNullException(nameof(modifiers));
        if (modifiers.Length == 0)
        {
            RemoveEffect(sourceId);
            return;
        }

        UnitStatModifier[] copy = (UnitStatModifier[])modifiers.Clone();
        // 새 합계를 먼저 계산해 잘못된 값/오버플로가 기존 상태를 훼손하지 않게 합니다.
        Dictionary<UnitStatType, UnitStatModifier> combined = BuildCombined(sourceId, copy);
        _effects[sourceId] = copy;
        _combined = combined;
        RefreshAllUnits();
    }

    public bool RemoveEffect(string sourceId)
    {
        if (sourceId == null || !_effects.ContainsKey(sourceId)) return false;
        Dictionary<UnitStatType, UnitStatModifier> combined = BuildCombined(sourceId, null);
        _effects.Remove(sourceId);
        _combined = combined;
        RefreshAllUnits();
        return true;
    }

    /// <summary>같은 씬에서 새 게임을 시작할 때도 호출할 수 있습니다.</summary>
    public void ClearEffects()
    {
        _effects.Clear();
        _combined = new Dictionary<UnitStatType, UnitStatModifier>();
        RefreshAllUnits();
    }

    public UnitStatModifier GetModifier(UnitStatType stat)
    {
        return _combined.TryGetValue(stat, out UnitStatModifier modifier)
            ? modifier : new UnitStatModifier(stat);
    }

    // 비활성 유닛도 소유 중인 유닛이므로 파괴될 때까지 등록을 유지합니다.
    internal void Register(Unit unit, UnitStats stats)
    {
        _units[unit] = stats;
        stats.ApplyGlobalModifiers(_combined);
    }

    internal void Unregister(Unit unit)
    {
        _units.Remove(unit);
    }

    public void RefreshAllUnits()
    {
        _destroyedUnits.Clear();
        foreach (KeyValuePair<Unit, UnitStats> entry in _units)
        {
            if (entry.Key == null)
                _destroyedUnits.Add(entry.Key);
            else
                entry.Value.ApplyGlobalModifiers(_combined);
        }
        foreach (Unit unit in _destroyedUnits)
            _units.Remove(unit);
        _destroyedUnits.Clear();
    }

    private Dictionary<UnitStatType, UnitStatModifier> BuildCombined(
        string replacedId, UnitStatModifier[] replacement)
    {
        var combined = new Dictionary<UnitStatType, UnitStatModifier>();
        foreach (KeyValuePair<string, UnitStatModifier[]> entry in _effects)
            if (entry.Key != replacedId) Combine(combined, entry.Value);
        if (replacement != null) Combine(combined, replacement);
        return combined;
    }

    private static void Combine(Dictionary<UnitStatType, UnitStatModifier> combined,
        UnitStatModifier[] modifiers)
    {
        foreach (UnitStatModifier modifier in modifiers)
        {
            if (!combined.TryGetValue(modifier.Stat, out UnitStatModifier previous))
                previous = new UnitStatModifier(modifier.Stat);
            combined[modifier.Stat] = new UnitStatModifier(modifier.Stat,
                previous.Flat + modifier.Flat, previous.Multiplier * modifier.Multiplier);
        }
    }

    protected override void OnDestroy()
    {
        foreach (KeyValuePair<Unit, UnitStats> entry in _units)
            if (entry.Key != null) entry.Value.ApplyGlobalModifiers(null);
        _units.Clear();
        base.OnDestroy();
    }
}
