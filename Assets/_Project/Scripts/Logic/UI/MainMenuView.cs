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
    private MetaUpgradePanel _upgrades;
    private MetaProgressionManager _meta;

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
        upgradeButton.onClick.AddListener(OpenUpgrades);
        quitButton.onClick.AddListener(Quit);
        _flow.OnStateChanged += Refresh;
        _flow.OnLoadFailed += ShowError;
        _meta = MetaProgressionManager.Ins;
        _meta.TrySettlePending();
        _meta.OnChanged += RefreshMeta;
        Refresh(_flow.State);
    }

    private void OnDisable()
    {
        startButton.onClick.RemoveListener(StartStage);
        upgradeButton.onClick.RemoveListener(OpenUpgrades);
        quitButton.onClick.RemoveListener(Quit);
        if (_meta != null) _meta.OnChanged -= RefreshMeta;
        if (_flow == null) return;
        _flow.OnStateChanged -= Refresh;
        _flow.OnLoadFailed -= ShowError;
    }

    private void Refresh(SceneFlowState state)
    {
        bool ready = state == SceneFlowState.Ready;
        startButton.interactable = ready && _meta != null && _meta.IsReady;
        quitButton.interactable = ready;
        upgradeButton.interactable = ready;
        settingsButton.interactable = false;
        message.text = ready ? (_flow.LastError ?? _meta.LastError ??
            $"메타포인트  {_meta.Progression?.Points ?? 0:N0}  ·  완료한 웨이브에 따라 획득합니다.") : "게임을 준비하고 있습니다…";
    }

    private void StartStage() => _flow.StartStage();
    private void RefreshMeta() => Refresh(_flow.State);
    private void OpenUpgrades()
    {
        if (_flow.IsTransitioning) return;
        if (_upgrades == null) _upgrades = MetaUpgradePanel.Create(message.font);
        _upgrades.gameObject.SetActive(true);
    }
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
