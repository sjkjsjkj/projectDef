using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>전투 씬에 설정 진입 버튼과 피해 숫자 표시기를 자동 연결합니다.</summary>
public class BattleSettingsButton : MonoBehaviour
{
    private Button _button;
    private DataManager _data;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Install()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (SceneCatalog.Identify(scene.name) != EScene.Stage) return;
        foreach (GameObject root in scene.GetRootGameObjects())
            if (root.GetComponentInChildren<BattleSettingsButton>(true) != null) return;
        GameObject canvas = new GameObject("BattleSettingsCanvas", typeof(RectTransform), typeof(Canvas),
            typeof(CanvasScaler), typeof(GraphicRaycaster));
        canvas.SetActive(false);
        SceneManager.MoveGameObjectToScene(canvas, scene);
        canvas.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.GetComponent<Canvas>().sortingOrder = 150;
        CanvasScaler scaler = canvas.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1280, 720);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.Expand;
        TMP_FontAsset font = InnUIFactory.ResolveFont(null);
        Button button = InnUIFactory.MakeButton("Settings", canvas.transform, -160, -62, 140, 42, "", font);
        ((RectTransform)button.transform).anchorMin = ((RectTransform)button.transform).anchorMax = Vector2.one;
        var view = canvas.AddComponent<BattleSettingsButton>();
        view._button = button;
        view._data = DataManager.Ins;
        button.onClick.AddListener(() =>
        {
            if (!SceneFlowManager.Ins.IsTransitioning) SettingsPanel.Show(true, font);
        });
        canvas.AddComponent<DamageNumbersView>();
        canvas.SetActive(true);
        InnUIFactory.EnsureEventSystem();
    }

    private void OnEnable()
    {
        if (_data == null) return;
        _data.OnOptionsChanged += Refresh;
        Refresh();
    }
    private void OnDisable() { if (_data != null) _data.OnOptionsChanged -= Refresh; }
    private void Refresh() => _button.GetComponentInChildren<TMP_Text>().text = SettingsPanel.Text("설정", "Settings");
    private void Update() => _button.interactable = !SceneFlowManager.Ins.IsTransitioning && !SettingsPanel.IsOpen;
}
