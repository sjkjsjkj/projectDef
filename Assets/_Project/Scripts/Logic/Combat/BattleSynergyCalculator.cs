using System;
using System.Collections.Generic;

public enum BattleSynergyCategory { Special, Faction, Troop }

/// <summary>구성원은 UnitData.Id입니다. 모든 구성원이 출전해야 표시됩니다.</summary>
[Serializable]
public class SpecialSynergyDefinition
{
    public string name;
    public string[] memberIds;

    public static SpecialSynergyDefinition[] CreateDefaults() => new[]
    {
        new SpecialSynergyDefinition { name = "도원결의", memberIds = new[] { "1", "2", "3" } },
        new SpecialSynergyDefinition { name = "오호대장군", memberIds = new[] { "2", "3", "4", "5", "6" } }
    };
}

public readonly struct BattleSynergyUnit
{
    public readonly string Id;
    public readonly string Faction;
    public readonly string Troop;

    public BattleSynergyUnit(string id, string faction, string troop)
    {
        Id = id;
        Faction = faction;
        Troop = troop;
    }
}

public readonly struct BattleSynergyEntry
{
    public readonly string Name;
    public readonly BattleSynergyCategory Category;
    public readonly int Count;
    public int Stage => Category == BattleSynergyCategory.Special ? 1 : Math.Min(Count / 2, 4);
    public bool IsActive => Stage > 0;

    public BattleSynergyEntry(string name, BattleSynergyCategory category, int count)
    {
        Name = name;
        Category = category;
        Count = count;
    }
}

/// <summary>출전 기물의 기본 진영/병종만 집계합니다. 표시용 계산이며 전투 능력치는 변경하지 않습니다.</summary>
public static class BattleSynergyCalculator
{
    public static List<BattleSynergyEntry> Calculate(IEnumerable<BattleSynergyUnit> units,
        IEnumerable<SpecialSynergyDefinition> specialSynergies)
    {
        var factions = new Dictionary<string, int>(StringComparer.Ordinal);
        var troops = new Dictionary<string, int>(StringComparer.Ordinal);
        var deployedIds = new HashSet<string>(StringComparer.Ordinal);
        foreach (BattleSynergyUnit unit in units)
        {
            if (!string.IsNullOrWhiteSpace(unit.Id)) deployedIds.Add(unit.Id.Trim());
            AddCount(factions, unit.Faction);
            AddCount(troops, unit.Troop);
        }

        var result = new List<BattleSynergyEntry>();
        if (specialSynergies != null)
        {
            foreach (SpecialSynergyDefinition special in specialSynergies)
            {
                if (special == null || string.IsNullOrWhiteSpace(special.name) ||
                    special.memberIds == null || special.memberIds.Length == 0) continue;
                var members = new HashSet<string>(StringComparer.Ordinal);
                bool complete = true;
                foreach (string id in special.memberIds)
                {
                    if (string.IsNullOrWhiteSpace(id) || !members.Add(id.Trim()) || !deployedIds.Contains(id.Trim()))
                    {
                        complete = false;
                        break;
                    }
                }
                if (complete)
                    result.Add(new BattleSynergyEntry(special.name.Trim(), BattleSynergyCategory.Special, members.Count));
            }
        }
        foreach (var pair in factions)
            result.Add(new BattleSynergyEntry(pair.Key, BattleSynergyCategory.Faction, pair.Value));
        foreach (var pair in troops)
            result.Add(new BattleSynergyEntry(pair.Key, BattleSynergyCategory.Troop, pair.Value));

        result.Sort((a, b) =>
        {
            int order = Group(a).CompareTo(Group(b));
            if (order == 0) order = a.Category.CompareTo(b.Category);
            return order != 0 ? order : StringComparer.Ordinal.Compare(a.Name, b.Name);
        });
        return result;
    }

    private static int Group(BattleSynergyEntry entry) =>
        entry.Category == BattleSynergyCategory.Special ? 0 : entry.IsActive ? 1 : 2;

    private static void AddCount(Dictionary<string, int> counts, string value)
    {
        if (string.IsNullOrWhiteSpace(value)) return;
        string key = value.Trim();
        counts.TryGetValue(key, out int count);
        counts[key] = count + 1;
    }
}
