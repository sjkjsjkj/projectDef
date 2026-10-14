using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>현재 씬에 편집 가능한 전투 정보 탭 UI를 생성합니다.</summary>
public static class BattleInfoUIBuilder
{
    [MenuItem("GameObject/삼국지 디펜스/전투 정보 UI 생성", false, 11)]
    private static void Create()
    {
        foreach (BattleInfoPanel existing in Object.FindObjectsByType<BattleInfoPanel>(
            FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            if (existing.gameObject.scene != SceneManager.GetActiveScene()) continue;
            ApplyCompactLayout(existing);
            return;
        }

        Undo.IncrementCurrentGroup();
        int group = Undo.GetCurrentGroup();
        Undo.SetCurrentGroupName("전투 정보 UI 생성");
        BattleInfoPanel panel = BattleInfoUIFactory.Create();
        Undo.RegisterCreatedObjectUndo(panel.gameObject, "전투 정보 UI 생성");
        GameObject events = InnUIFactory.EnsureEventSystem();
        if (events != null) Undo.RegisterCreatedObjectUndo(events, "전투 정보 입력 생성");
        Undo.CollapseUndoOperations(group);
        Selection.activeGameObject = panel.gameObject;
        Debug.Log("전투 정보 UI를 생성했습니다. 씬을 저장하고 Play 모드에서 클릭 또는 Tab 키로 전환하세요.", panel);
    }

    [MenuItem("GameObject/삼국지 디펜스/전투 정보 UI 생성", true)]
    private static bool CanCreate() => !EditorApplication.isPlayingOrWillChangePlaymode;

    [MenuItem("GameObject/삼국지 디펜스/전투 정보 UI 아이콘형으로 변경", false, 12)]
    private static void UpdateExisting()
    {
        foreach (BattleInfoPanel existing in Object.FindObjectsByType<BattleInfoPanel>(
            FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            if (existing.gameObject.scene != SceneManager.GetActiveScene()) continue;
            ApplyCompactLayout(existing);
            return;
        }
        Create();
    }

    [MenuItem("GameObject/삼국지 디펜스/전투 정보 UI 아이콘형으로 변경", true)]
    private static bool CanUpdate() => CanCreate();

    private static void ApplyCompactLayout(BattleInfoPanel panel)
    {
        Undo.IncrementCurrentGroup();
        int group = Undo.GetCurrentGroup();
        Undo.SetCurrentGroupName("전투 정보 UI 아이콘형으로 변경");
        var previous = new HashSet<Transform>(panel.GetComponentsInChildren<Transform>(true));
        Undo.RegisterFullObjectHierarchyUndo(panel.gameObject, "전투 정보 UI 레이아웃 변경");
        BattleInfoUIFactory.ApplyCompactLayout(panel);
        foreach (Transform child in panel.GetComponentsInChildren<Transform>(true))
        {
            if (!previous.Contains(child))
                Undo.RegisterCreatedObjectUndo(child.gameObject, "전투 정보 탭 아이콘 생성");
            foreach (Component component in child.GetComponents<Component>())
                if (component != null) PrefabUtility.RecordPrefabInstancePropertyModifications(component);
            PrefabUtility.RecordPrefabInstancePropertyModifications(child.gameObject);
        }
        Undo.CollapseUndoOperations(group);
        EditorSceneManager.MarkSceneDirty(panel.gameObject.scene);
        Selection.activeGameObject = panel.transform.Find("BattleInfo").gameObject;
        Debug.Log("가로 폭을 260으로 줄이고 장수/시너지/DFS 탭에 원/네모/삼각형 아이콘을 적용했습니다. BattleInfo의 RectTransform Width로 폭을 조절할 수 있습니다.", panel);
    }
}
