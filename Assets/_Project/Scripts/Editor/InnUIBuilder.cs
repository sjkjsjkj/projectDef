using UnityEditor;
using UnityEngine;

/// <summary>현재 씬에 편집 가능한 객잔 UI를 생성합니다.</summary>
public static class InnUIBuilder
{
    [MenuItem("GameObject/삼국지 디펜스/객잔 UI 생성", false, 10)]
    private static void Create()
    {
        InnManager manager = Selection.activeGameObject != null
            ? Selection.activeGameObject.GetComponent<InnManager>() : null;
        if (manager == null)
        {
            Debug.LogWarning("InnManager가 부착된 씬 오브젝트를 선택한 뒤 실행하세요.");
            return;
        }
        foreach (InnPanel existing in Object.FindObjectsByType<InnPanel>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            if (existing.Manager != manager) continue;
            Selection.activeGameObject = existing.gameObject;
            Debug.LogWarning("이미 연결된 객잔 UI가 있습니다. 교체하려면 기존 Canvas를 삭제한 뒤 다시 생성하세요.", existing);
            return;
        }
        Undo.IncrementCurrentGroup();
        int group = Undo.GetCurrentGroup();
        Undo.SetCurrentGroupName("객잔 UI 생성");
        InnPanel panel = InnUIFactory.Create(manager);
        Undo.RegisterCreatedObjectUndo(panel.gameObject, "객잔 UI 생성");
        GameObject events = InnUIFactory.EnsureEventSystem();
        if (events != null) Undo.RegisterCreatedObjectUndo(events, "객잔 입력 생성");
        Undo.CollapseUndoOperations(group);
        Selection.activeGameObject = panel.gameObject;
        Debug.Log("객잔 UI를 생성했습니다. 필요하면 한글 TMP 폰트를 지정하고 씬을 저장하세요.", panel);
    }

    [MenuItem("GameObject/삼국지 디펜스/객잔 UI 생성", true)]
    private static bool CanCreate() => !EditorApplication.isPlayingOrWillChangePlaymode;
}
