using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>전투 씬에 설정/배속/자동 진행 버튼과 피해 숫자 표시기를 자동 연결합니다.</summary>
public class BattleSettingsButton : MonoBehaviour
{
    private Button _button;
    private DataManager _data;
    private Button _speedButton;
    private Button _autoButton;
    private TMP_Text _speedLabel;
    private TMP_Text _autoLabel;
    private BattlePlaybackController _playback;

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
        TMP_FontAsset font = StageUIStyle.LoadButtonFont();
        Button button = InnUIFactory.MakeButton("Settings", canvas.transform, -160, -62, 140, 42, "", font);
        ((RectTransform)button.transform).anchorMin = ((RectTransform)button.transform).anchorMax = Vector2.one;
        // 중앙 라운드 정보와 겹치지 않도록 설정 버튼 아래에 세로로 배치합니다.
        Button speed = InnUIFactory.MakeButton("Speed", canvas.transform, -160, -112, 140, 42, "", font);
        Button auto = InnUIFactory.MakeButton("AutoAdvance", canvas.transform, -160, -162, 140, 42, "", font);
        ((RectTransform)speed.transform).anchorMin = ((RectTransform)speed.transform).anchorMax = Vector2.one;
        ((RectTransform)auto.transform).anchorMin = ((RectTransform)auto.transform).anchorMax = Vector2.one;
        WaveManager waves = null;
        foreach (GameObject root in scene.GetRootGameObjects())
        {
            waves = root.GetComponentInChildren<WaveManager>(true);
            if (waves != null) break;
        }
        BattlePlaybackController playback = canvas.AddComponent<BattlePlaybackController>();
        playback.Configure(waves);
        var view = canvas.AddComponent<BattleSettingsButton>();
        view._button = button;
        view._data = DataManager.Ins;
        view._playback = playback;
        view._speedButton = speed;
        view._autoButton = auto;
        view._speedLabel = speed.GetComponentInChildren<TMP_Text>();
        view._autoLabel = auto.GetComponentInChildren<TMP_Text>();
        view._autoLabel.enableAutoSizing = true;
        view._autoLabel.fontSizeMin = 12;
        view._autoLabel.fontSizeMax = 18;
        speed.onClick.AddListener(playback.CycleSpeed);
        auto.onClick.AddListener(playback.ToggleAutoAdvance);
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
        if (_playback != null) _playback.OnChanged += Refresh;
        Refresh();
    }
    private void OnDisable()
    {
        if (_data != null) _data.OnOptionsChanged -= Refresh;
        if (_playback != null) _playback.OnChanged -= Refresh;
    }

    private void Refresh()
    {
        _button.GetComponentInChildren<TMP_Text>().text = SettingsPanel.Text("설정", "Settings");
        if (_playback == null) return;
        string speed = _playback.Speed.ToString("0.#", System.Globalization.CultureInfo.InvariantCulture);
        _speedLabel.text = SettingsPanel.Text($"배속 {speed}x", $"Speed {speed}x");
        _autoLabel.text = SettingsPanel.Text("자동 진행", "Auto") + (_playback.AutoAdvance ? " ON" : " OFF");
        _autoButton.GetComponent<Image>().color = _playback.AutoAdvance
            ? new Color32(48, 116, 77, 255) : new Color32(88, 67, 37, 255);
    }

    private void Update()
    {
        _button.interactable = !SceneFlowManager.Ins.IsTransitioning && !SettingsPanel.IsOpen;
        if (_playback == null) return;
        _speedButton.interactable = _autoButton.interactable = _playback.CanInteract;
    }
}
