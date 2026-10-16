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
    private DataManager _data;

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
        settingsButton.onClick.AddListener(OpenSettings);
        quitButton.onClick.AddListener(Quit);
        _flow.OnStateChanged += Refresh;
        _flow.OnLoadFailed += ShowError;
        _meta = MetaProgressionManager.Ins;
        _meta.TrySettlePending();
        _meta.OnChanged += RefreshMeta;
        _data = DataManager.Ins;
        _data.OnOptionsChanged += RefreshMeta;
        Refresh(_flow.State);
    }

    private void OnDisable()
    {
        startButton.onClick.RemoveListener(StartStage);
        upgradeButton.onClick.RemoveListener(OpenUpgrades);
        settingsButton.onClick.RemoveListener(OpenSettings);
        quitButton.onClick.RemoveListener(Quit);
        if (_meta != null) _meta.OnChanged -= RefreshMeta;
        if (_data != null) _data.OnOptionsChanged -= RefreshMeta;
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
        settingsButton.interactable = ready;
        Localize();
        message.text = ready ? (_flow.LastError ?? _meta.LastError ??
            SettingsPanel.Text($"메타포인트  {_meta.Progression?.Points ?? 0:N0}  ·  완료한 웨이브에 따라 획득합니다.",
                $"Meta points  {_meta.Progression?.Points ?? 0:N0}  ·  Earned by completing waves."))
            : SettingsPanel.Text("게임을 준비하고 있습니다…", "Preparing the game…");
    }

    private void StartStage() => _flow.StartStage();
    private void OpenSettings()
    {
        if (!_flow.IsTransitioning) SettingsPanel.Show(false, message.font);
    }

    private void Localize()
    {
        startButton.GetComponentInChildren<TMP_Text>().text = SettingsPanel.Text("게임시작", "Start game");
        upgradeButton.GetComponentInChildren<TMP_Text>().text = SettingsPanel.Text("능력 강화", "Upgrades");
        settingsButton.GetComponentInChildren<TMP_Text>().text = SettingsPanel.Text("설정", "Settings");
        quitButton.GetComponentInChildren<TMP_Text>().text = SettingsPanel.Text("게임 종료", "Quit");
        TMP_Text title = transform.Find("Title")?.GetComponent<TMP_Text>();
        if (title != null) title.text = SettingsPanel.Text("삼국지 디펜스", "Three Kingdoms Defense");
        TMP_Text subtitle = transform.Find("Subtitle")?.GetComponent<TMP_Text>();
        if (subtitle != null) subtitle.text = SettingsPanel.Text("장수를 모으고, 밀려오는 적을 막아내세요", "Recruit generals and defend against enemy waves");
    }
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
