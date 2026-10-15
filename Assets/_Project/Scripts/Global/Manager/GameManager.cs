using System;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// 핵심 오브젝트 접근과 디펜스 게임 진행을 관리합니다. 씬 전환은 SceneFlowManager에 위임합니다.
/// </summary>
public class GameManager : GlobalSingleton<GameManager>
{
    [Header("게임 진행 (런타임 확인용)")]
    [SerializeField] private GameState gameState = GameState.Ready;
    [SerializeField] private int currentWave;
    [SerializeField] private int currentEnemyCount;

    private WaveManager _waveManager;
    private bool _enemyLimitExceeded;
    private RoundEventManager _roundEventManager;
    private string _metaRunId;
    private int _completedWaves;

    public GameState State => gameState;
    public int CurrentWave => currentWave;
    public int CurrentEnemyCount => EnemyHealth.ActiveEnemyCount;
    public WaveManager WaveManager => _waveManager;
    public bool IsRoundEventActive => gameState == GameState.RoundEvent;
    public bool IsCombatPaused => IsRoundEventActive || gameState == GameState.Victory || gameState == GameState.Defeat;
    public event Action<GameState> OnGameStateChanged;

    /// <summary>부팅 시 생성되는 GameManager에 씬의 WaveManager를 연결합니다.</summary>
    public void RegisterWaveManager(WaveManager manager)
    {
        if (manager == null || _waveManager == manager)
            return;

        if (_waveManager != null)
        {
            Debug.LogError("이미 연결된 WaveManager가 있습니다. 전투 씬에는 하나만 배치하세요.", manager);
            return;
        }

        _waveManager = manager;
        _waveManager.OnWaveStart += HandleWaveStart;
        _waveManager.OnWaveEnd += HandleWaveEnd;
        _waveManager.OnWavesReset += HandleWavesReset;
        currentWave = 0;
        _enemyLimitExceeded = false;
        ChangeGameState(GameState.Ready);
    }

    public void UnregisterWaveManager(WaveManager manager)
    {
        if (_waveManager != manager)
            return;

        CancelRoundEvent();
        _waveManager.OnWaveStart -= HandleWaveStart;
        _waveManager.OnWaveEnd -= HandleWaveEnd;
        _waveManager.OnWavesReset -= HandleWavesReset;
        _waveManager.StopWave();
        _waveManager = null;
        _enemyLimitExceeded = false;
        currentWave = 0;
        ChangeGameState(GameState.Ready);
    }

    public bool StartGame()
    {
        if (gameState == GameState.Playing || gameState == GameState.RoundEvent)
            return false;
        if (_waveManager == null || !_waveManager.isActiveAndEnabled)
        {
            Debug.LogError("활성 WaveManager를 씬에 배치하고 웨이브를 설정하세요.", this);
            return false;
        }

        if (Application.isPlaying && !MetaProgressionManager.Ins.TrySettlePending())
        {
            Debug.LogError(MetaProgressionManager.Ins.LastError, this);
            return false;
        }

        _waveManager.ResetWaves();
        _metaRunId = Guid.NewGuid().ToString("N");
        _completedWaves = 0;
        _enemyLimitExceeded = false;
        currentWave = 0;
        ChangeGameState(GameState.Playing);
        HandleEnemyCountChanged(EnemyHealth.ActiveEnemyCount);
        if (_enemyLimitExceeded)
        {
            EndGame();
            return false;
        }

        if (_waveManager.StartWave())
            return true;

        ChangeGameState(GameState.Ready);
        _metaRunId = null;
        return false;
    }

    /// <summary>웨이브/소환을 멈추고 결과를 통지합니다. 결과 화면을 위해 적은 유지합니다.</summary>
    public void EndGame(bool victory = false)
    {
        if (gameState != GameState.Playing && gameState != GameState.RoundEvent)
            return;

        CancelRoundEvent();
        if (_waveManager != null)
            _waveManager.StopWave();
        string runId = _metaRunId;
        _metaRunId = null;
        if (Application.isPlaying && runId != null)
            MetaProgressionManager.Ins.CompleteRun(runId, _completedWaves, victory);
        ChangeGameState(victory ? GameState.Victory : GameState.Defeat);
        if (Application.isPlaying && !ApplicationLifetime.IsQuitting)
            MetaRunResultView.Show(victory);
    }

    public void UpdateGameState()
    {
        currentEnemyCount = EnemyHealth.ActiveEnemyCount;
        if (gameState != GameState.Playing || _waveManager == null)
            return;

        // 최대값과 같을 때는 계속 진행하며 초과한 순간 패배를 확정합니다.
        if (_enemyLimitExceeded || currentEnemyCount > _waveManager.MaxEnemyCount)
        {
            EndGame();
            return;
        }

        // 마지막 웨이브의 시간까지 끝난 뒤 남아 있는 적을 모두 처치하면 승리합니다.
        if (_waveManager.IsComplete && currentEnemyCount == 0)
            EndGame(true);
    }

    private void HandleWaveStart(int waveIndex)
    {
        currentWave = waveIndex;
    }

    private void HandleWaveEnd(int waveIndex)
    {
        if (gameState != GameState.Playing || _waveManager == null) return;
        _completedWaves++;
        if (_enemyLimitExceeded || CurrentEnemyCount > _waveManager.MaxEnemyCount)
        { EndGame(); return; }

        RoundEventData data = _waveManager.LastCompletedWaveData?.CompletionEvent;
        // 최종 라운드에 연결된 이벤트도 승리 판정보다 먼저 처리합니다.
        if (data != null)
        {
            _roundEventManager = _waveManager.RoundEventManager;
            if (_roundEventManager == null)
            {
                Debug.LogError("웨이브에 이벤트가 설정되어 있지만 RoundEventManager가 연결되지 않았습니다.", this);
                EndGame();
                return;
            }
            _roundEventManager.OnCompleted += HandleRoundEventCompleted;
            _roundEventManager.OnCancelled += HandleRoundEventCancelled;
            ChangeGameState(GameState.RoundEvent);
            // 상태 변경 구독자가 게임을 종료했을 수 있습니다.
            if (gameState != GameState.RoundEvent || _roundEventManager == null) return;
            if (!_roundEventManager.TryBegin(data, _waveManager, waveIndex, out string reason))
            {
                Debug.LogError(reason, this);
                EndGame();
            }
            return;
        }
        ContinueWaves();
    }

    private void ContinueWaves()
    {
        UpdateGameState();
        if (gameState == GameState.Playing && !_waveManager.IsComplete && !_waveManager.StartWave())
            EndGame();
    }

    private void HandleRoundEventCompleted()
    {
        CancelRoundEvent();
        if (gameState != GameState.RoundEvent || _waveManager == null) return;
        ChangeGameState(GameState.Playing);
        ContinueWaves();
    }

    private void HandleRoundEventCancelled() => EndGame();

    private void CancelRoundEvent()
    {
        if (_roundEventManager == null) return;
        RoundEventManager manager = _roundEventManager;
        _roundEventManager = null;
        manager.OnCompleted -= HandleRoundEventCompleted;
        manager.OnCancelled -= HandleRoundEventCancelled;
        manager.Cancel();
    }

    private void HandleWavesReset()
    {
        CancelRoundEvent();
        if (gameState == GameState.RoundEvent) ChangeGameState(GameState.Ready);
    }

    private void HandleEnemyCountChanged(int count)
    {
        currentEnemyCount = count;
        if (gameState == GameState.Playing && _waveManager != null && count > _waveManager.MaxEnemyCount)
        {
            _enemyLimitExceeded = true;
            EndGame();
        }
    }

    private void ChangeGameState(GameState state)
    {
        if (gameState == state)
            return;
        gameState = state;
        OnGameStateChanged?.Invoke(state);
    }

    private void Update()
    {
        UpdateGameState();
    }

    protected override void OnDestroy()
    {
        EnemyHealth.OnActiveEnemyCountChanged -= HandleEnemyCountChanged;
        if (_waveManager != null)
            UnregisterWaveManager(_waveManager);
        base.OnDestroy();
    }

    private static Transform _uiRoot;
    private static Transform _objectRoot;
    private static Transform _enableObjectRoot;
    private static Transform _disableObjectRoot;
    private bool _isInitialized;

    public bool IsBooted { get; private set; }
    public bool IsSceneLoading => SceneFlowManager.Ins.IsTransitioning;
    public bool IsPlayerWakeUp { get; set; }
    public EScene Scene => SceneFlowManager.Ins.CurrentScene;
    public int NextSpawnPointIndex { get; set; } = -1;
    public void BootComplete() => IsBooted = true;

    public static Transform UIRoot => _uiRoot = RootProvider(_uiRoot, K.NAME_UI_ROOT);
    public static Transform ObjectRoot => _objectRoot = RootProvider(_objectRoot, K.NAME_OBJECT_ROOT);
    public static Transform EnableObjectRoot => _enableObjectRoot = RootProvider(_enableObjectRoot, K.NAME_ENABLE_OBJECT_ROOT);
    public static Transform DisableObjectRoot => _disableObjectRoot = RootProvider(_disableObjectRoot, K.NAME_DISABLE_OBJECT_ROOT);

    public override void Initialize()
    {
        if (_isInitialized) return;
        _isInitialized = true;
        EnemyHealth.OnActiveEnemyCountChanged += HandleEnemyCountChanged;
        currentEnemyCount = EnemyHealth.ActiveEnemyCount;
    }

    // 기존 호출부의 API를 유지하며 모든 씬 전환을 한 곳에서 처리합니다.
    [Obsolete("LoadSceneAsync 또는 SceneFlowManager를 사용하세요. 씬 전환은 비동기로 처리됩니다.")]
    public void LoadScene(int index) => LoadSceneAsync(index);

    [Obsolete("LoadSceneAsync 또는 SceneFlowManager를 사용하세요. 씬 전환은 비동기로 처리됩니다.")]
    public void LoadScene(string name) => LoadSceneAsync(name);

    public void LoadSceneAsync(int index, Action callback = null, Action<float> onProgress = null,
        float delay = 0f, LoadSceneMode loadSceneMode = LoadSceneMode.Single)
    {
        string path = ScenePath(index);
        if (path != null) LoadSceneAsync(path, callback, onProgress, delay, loadSceneMode);
    }

    public void LoadSceneAsync(string name, Action callback = null, Action<float> onProgress = null,
        float delay = 0f, LoadSceneMode loadSceneMode = LoadSceneMode.Single)
    {
        SceneFlowManager.Ins.TryLoadScene(name, callback, onProgress, delay, 0f, 0f, loadSceneMode);
    }

    public void LoadSceneAsyncWithFade(int index, float delay = 0f, float fadeOutTime = 0.2f,
        float fadeInTime = 0.2f, Action callback = null, Action<float> onProgress = null,
        LoadSceneMode loadSceneMode = LoadSceneMode.Single)
    {
        string path = ScenePath(index);
        if (path != null) LoadSceneAsyncWithFade(path, delay, fadeOutTime, fadeInTime, callback, onProgress, loadSceneMode);
    }

    public void LoadSceneAsyncWithFade(string name, float delay = 0f, float fadeOutTime = 0.2f,
        float fadeInTime = 0.2f, Action callback = null, Action<float> onProgress = null,
        LoadSceneMode loadSceneMode = LoadSceneMode.Single)
    {
        SceneFlowManager.Ins.TryLoadScene(name, callback, onProgress, delay, fadeOutTime, fadeInTime, loadSceneMode);
    }

    internal void PrepareSceneChange()
    {
        if (gameState == GameState.Playing || gameState == GameState.RoundEvent) EndGame();
        _uiRoot = null;
        _objectRoot = null;
        _enableObjectRoot = null;
        _disableObjectRoot = null;
        IsPlayerWakeUp = false;
    }

    private static string ScenePath(int index)
    {
        if (index >= 0 && index < SceneManager.sceneCountInBuildSettings)
            return SceneUtility.GetScenePathByBuildIndex(index);
        Debug.LogError($"존재하지 않는 씬 인덱스({index})를 호출했습니다.");
        return null;
    }

    private static Transform RootProvider(Transform root, string name)
    {
        if (root != null) return root;
        GameObject go = GameObject.Find(name);
        return go != null ? go.transform : UObject.Create(name).transform;
    }
}
