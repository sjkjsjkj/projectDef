using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>편집 중인 씬과 관계없이 Play를 MainScene에서 시작합니다.</summary>
[InitializeOnLoad]
public static class MainScenePlayModeSetup
{
    private const string MainScenePath = "Assets/Scenes/MainScene.unity";

    static MainScenePlayModeSetup()
    {
        // 스크립트 리로드 직후에는 씬 에셋의 임포트가 끝나지 않았을 수 있습니다.
        EditorApplication.delayCall += ApplyAfterReload;
        EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
    }

    private static void ApplyAfterReload()
    {
        if (!EditorApplication.isPlayingOrWillChangePlaymode)
            ConfigureStartScene();
    }

    private static void OnPlayModeStateChanged(PlayModeStateChange state)
    {
        if (state == PlayModeStateChange.EnteredEditMode)
            ConfigureStartScene();
        else if (state == PlayModeStateChange.ExitingEditMode && !ConfigureStartScene())
        {
            Debug.LogError($"Play 시작 씬을 찾을 수 없습니다: {MainScenePath}");
            EditorApplication.isPlaying = false;
        }
    }

    private static bool ConfigureStartScene()
    {
        SceneAsset mainScene = AssetDatabase.LoadAssetAtPath<SceneAsset>(MainScenePath);
        // Unity의 시작 씬 설정을 사용하므로 종료 시 원래 편집하던 씬이 복원됩니다.
        EditorSceneManager.playModeStartScene = mainScene;
        return mainScene != null;
    }
}
