using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>현재 씬에 편집 가능한 라운드 진행 UI를 생성합니다.</summary>
public static class RoundProgressUIBuilder
{
    [MenuItem("GameObject/삼국지 디펜스/라운드 진행 UI 생성", false, 13)]
    private static void Create()
    {
        Scene scene = SceneManager.GetActiveScene();
        WaveManager manager = null;
        foreach (GameObject root in scene.GetRootGameObjects())
        {
            RoundProgressPanel existing = root.GetComponentInChildren<RoundProgressPanel>(true);
            if (existing != null)
            {
                Selection.activeGameObject = existing.gameObject;
                return;
            }
            if (manager == null) manager = root.GetComponentInChildren<WaveManager>(true);
        }
        RoundProgressPanel panel = RoundProgressUIFactory.Create(manager);
        Undo.RegisterCreatedObjectUndo(panel.gameObject, "라운드 진행 UI 생성");
        Selection.activeGameObject = panel.gameObject;
    }

    [MenuItem("GameObject/삼국지 디펜스/라운드 진행 UI 생성", true)]
    private static bool CanCreate() => !EditorApplication.isPlayingOrWillChangePlaymode;
}
