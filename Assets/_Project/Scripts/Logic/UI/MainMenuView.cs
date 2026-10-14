using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>메인 메뉴 입력과 표시만 담당합니다. 씬 이동은 SceneFlowManager에 요청합니다.</summary>
public class MainMenuView : MonoBehaviour
{
    [SerializeField] private Button startButton;
    [SerializeField] private Button upgradeButton;
    [SerializeField] private Button settingsButton;
    [SerializeField] private Button quitButton;
    [SerializeField] private TMP_Text message;
    private SceneFlowManager _flow;

    public void Configure(Button start, Button upgrade, Button settings, Button quit, TMP_Text status)
    {
        startButton = start;
        upgradeButton = upgrade;
        settingsButton = settings;
        quitButton = quit;
        message = status;
    }

    private void OnEnable()
    {
        _flow = SceneFlowManager.Ins;
        startButton.onClick.AddListener(StartStage);
        quitButton.onClick.AddListener(Quit);
        _flow.OnStateChanged += Refresh;
        _flow.OnLoadFailed += ShowError;
        Refresh(_flow.State);
    }

    private void OnDisable()
    {
        startButton.onClick.RemoveListener(StartStage);
        quitButton.onClick.RemoveListener(Quit);
        if (_flow == null) return;
        _flow.OnStateChanged -= Refresh;
        _flow.OnLoadFailed -= ShowError;
    }

    private void Refresh(SceneFlowState state)
    {
        bool ready = state == SceneFlowState.Ready;
        startButton.interactable = ready;
        quitButton.interactable = ready;
        // 메타 성장/설정은 다음 단계에서 각 화면과 연결합니다.
        upgradeButton.interactable = false;
        settingsButton.interactable = false;
        message.text = ready ? (_flow.LastError ?? "능력 강화와 설정은 준비 중입니다.") : "게임을 준비하고 있습니다…";
    }

    private void StartStage() => _flow.StartStage();
    private void ShowError(string error) => message.text = error;

    private void Quit()
    {
        if (_flow.IsTransitioning) return;
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
