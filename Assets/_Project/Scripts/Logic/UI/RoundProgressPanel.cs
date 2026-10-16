using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>실제 전투 상태를 중앙 상단에 표시합니다. 타이머와 소환은 각 관리자가 담당합니다.</summary>
public class RoundProgressPanel : MonoBehaviour
{
    [SerializeField] private WaveManager waveManager;
    [SerializeField] private TMP_FontAsset uiFont;
    [SerializeField] private TMP_Text roundText;
    [SerializeField] private TMP_Text timeText;
    [SerializeField] private TMP_Text enemyText;
    [SerializeField] private TMP_Text remainingSpawnText;

    private int _round = -1;
    private int _seconds = -1;
    private int _enemies = -1;
    private int _capacity = -1;
    private int _remaining = -1;
    private GameManager _gameManager;

    public void Configure(WaveManager manager, TMP_FontAsset font, TMP_Text round, TMP_Text time,
        TMP_Text enemies, TMP_Text remaining)
    {
        waveManager = manager;
        uiFont = font;
        roundText = round;
        timeText = time;
        enemyText = enemies;
        remainingSpawnText = remaining;
        _round = _seconds = _enemies = _capacity = _remaining = -1;
    }

    private void Awake()
    {
        _gameManager = GameManager.Ins;
        TMP_FontAsset font = InnUIFactory.ResolveFont(uiFont);
        if (font != null)
            foreach (TMP_Text label in GetComponentsInChildren<TMP_Text>(true)) label.font = font;
        if (waveManager == null)
            foreach (GameObject root in gameObject.scene.GetRootGameObjects())
            {
                waveManager = root.GetComponentInChildren<WaveManager>();
                if (waveManager != null) break;
            }
    }

    // Update에서 처리되는 타이머, 소환, 사망 결과를 같은 프레임에 반영합니다.
    private void LateUpdate()
    {
        if (waveManager == null) return;
        WaveData current = waveManager.IsRunning ? waveManager.GetCurrentWaveData()
            : waveManager.LastCompletedWaveData ?? waveManager.GetCurrentWaveData();
        int round = current != null ? current.WaveIndex : 0;
        // 이벤트/패배로 StopWave가 호출되어도 실제 진행했던 라운드 번호를 유지합니다.
        if (_gameManager != null && _gameManager.WaveManager == waveManager && _gameManager.CurrentWave > 0)
            round = _gameManager.CurrentWave;
        int seconds = Mathf.CeilToInt(Mathf.Max(0f, waveManager.RemainingTime));
        int enemies = EnemyHealth.ActiveEnemyCount;
        int capacity = waveManager.MaxEnemyCount;
        int remaining = waveManager.EnemySpawner != null ? waveManager.EnemySpawner.RemainingSpawnCount : 0;

        if (_round != round && roundText != null) roundText.text = round > 0 ? round.ToString("00") : "—";
        if (_seconds != seconds && timeText != null)
            timeText.text = $"{seconds / 60:00}:{seconds % 60:00}";
        if ((_enemies != enemies || _capacity != capacity) && enemyText != null)
        {
            enemyText.text = $"{enemies} / {capacity}";
            enemyText.color = enemies >= capacity
                ? new Color32(244, 116, 102, 255) : new Color32(236, 231, 215, 255);
        }
        if (_remaining != remaining && remainingSpawnText != null) remainingSpawnText.text = remaining.ToString();
        _round = round;
        _seconds = seconds;
        _enemies = enemies;
        _capacity = capacity;
        _remaining = remaining;
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Install()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (SceneCatalog.Identify(scene.name) != EScene.Stage) return;
        WaveManager manager = null;
        foreach (GameObject root in scene.GetRootGameObjects())
        {
            if (root.GetComponentInChildren<RoundProgressPanel>(true) != null) return;
            if (manager == null) manager = root.GetComponentInChildren<WaveManager>(true);
        }
        RoundProgressPanel panel = RoundProgressUIFactory.Create(manager);
        SceneManager.MoveGameObjectToScene(panel.gameObject, scene);
    }
}
