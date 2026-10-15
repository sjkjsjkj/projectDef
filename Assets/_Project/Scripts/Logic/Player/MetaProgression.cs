using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>구매/정산은 저장 성공 후에만 메모리 상태를 교체합니다. 테스트에서는 저장 함수를 주입합니다.</summary>
public sealed class MetaProgression
{
    [Serializable] public class Entry { public string id; public int level; }
    [Serializable] public class SaveData
    {
        public int version = 1;
        public int points;
        public string lastRewardRunId;
        public List<Entry> levels = new List<Entry>();
    }

    private SaveData _data;
    private readonly Action<string> _save;
    public MetaProgressionSettings Settings { get; }
    public int Points => _data.points;
    public MetaProgression(MetaProgressionSettings settings, SaveData data, Action<string> save)
    {
        if (settings == null || !settings.IsValid) throw new ArgumentException("메타 성장 설정이 올바르지 않습니다.");
        Settings = settings;
        _data = Validate(data ?? new SaveData());
        _save = save ?? throw new ArgumentNullException(nameof(save));
    }

    public static SaveData Parse(string json)
    {
        if (string.IsNullOrWhiteSpace(json) || !json.TrimStart().StartsWith("{") ||
            !json.Contains("\"version\"") || !json.Contains("\"points\"") || !json.Contains("\"levels\""))
            throw new FormatException("메타 성장 저장 형식이 올바르지 않습니다.");
        return Validate(JsonUtility.FromJson<SaveData>(json));
    }

    private static SaveData Validate(SaveData data)
    {
        if (data == null || data.version != 1 || data.points < 0 || data.levels == null)
            throw new FormatException("메타 성장 저장 버전 또는 값이 올바르지 않습니다.");
        var ids = new HashSet<string>(StringComparer.Ordinal);
        foreach (Entry entry in data.levels)
            if (entry == null || string.IsNullOrWhiteSpace(entry.id) || entry.level < 0 || !ids.Add(entry.id))
                throw new FormatException("메타 성장 레벨 목록이 올바르지 않습니다.");
        return data;
    }

    public MetaUpgradeDefinition Find(string id) => Settings.upgrades.Find(item => item.id == id);
    public int Level(string id)
    {
        MetaUpgradeDefinition definition = Find(id);
        Entry entry = _data.levels.Find(item => item.id == id);
        return definition == null || entry == null ? 0 : Math.Min(entry.level, definition.maxLevel);
    }

    public bool CanPurchase(string id, out string reason)
    {
        reason = null;
        MetaUpgradeDefinition definition = Find(id);
        if (definition == null || !definition.IsValid) { reason = "강화 설정을 확인하세요."; return false; }
        int level = Level(id);
        if (level >= definition.maxLevel) { reason = "최대 레벨입니다."; return false; }
        if (Points < definition.CostAt(level)) { reason = "메타포인트가 부족합니다."; return false; }
        return true;
    }

    public bool TryPurchase(string id, out string reason)
    {
        if (!CanPurchase(id, out reason)) return false;
        SaveData next = Copy();
        int level = Level(id);
        next.points -= Find(id).CostAt(level);
        Entry entry = next.levels.Find(item => item.id == id);
        if (entry == null) next.levels.Add(new Entry { id = id, level = level + 1 });
        else entry.level = level + 1;
        return TryCommit(next, out reason);
    }

    public bool TryReward(string runId, int completedWaves, bool victory, out int awarded, out string reason)
    {
        awarded = 0;
        reason = null;
        if (string.IsNullOrWhiteSpace(runId)) { reason = "게임 정산 ID가 없습니다."; return false; }
        if (_data.lastRewardRunId == runId) return true;
        SaveData next = Copy();
        next.points = (int)Math.Min(int.MaxValue, (long)Points + Settings.RewardFor(completedWaves, victory));
        next.lastRewardRunId = runId;
        int difference = next.points - Points;
        if (!TryCommit(next, out reason)) return false;
        awarded = difference;
        return true;
    }

    private SaveData Copy() => Parse(JsonUtility.ToJson(_data));
    private bool TryCommit(SaveData next, out string reason)
    {
        reason = null;
        try { _save(JsonUtility.ToJson(next, true)); }
        catch (Exception exception) { reason = $"메타 성장 저장 실패: {exception.Message}"; return false; }
        _data = next;
        return true;
    }
}
