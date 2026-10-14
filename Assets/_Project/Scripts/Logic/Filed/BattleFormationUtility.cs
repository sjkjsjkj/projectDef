using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>장수 목록과 시너지 목록이 동일한 출전 기준을 사용합니다.</summary>
public static class BattleFormationUtility
{
    public static void CollectUnits(Scene scene, List<Unit> units)
    {
        units.Clear();
        Slot[] slots = Object.FindObjectsByType<Slot>(FindObjectsSortMode.None);
        System.Array.Sort(slots, (a, b) =>
        {
            int order = a.SlotIndex.CompareTo(b.SlotIndex);
            return order != 0 ? order : a.GetInstanceID().CompareTo(b.GetInstanceID());
        });
        foreach (Slot slot in slots)
        {
            if (slot.gameObject.scene != scene || !slot.isActiveAndEnabled || !slot.IsBattleSlot ||
                slot.Occupant is not Unit unit || unit == null || !unit.isActiveAndEnabled ||
                !ReferenceEquals(unit.CurrentTarget, slot)) continue;
            units.Add(unit);
        }
    }
}
