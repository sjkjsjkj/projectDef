using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// 핵심 오브젝트 접근, 씬 로드 및 디펜스 게임 진행을 관리합니다.
/// </summary>
public class GameManager : GlobalSingleton<GameManager>
{
    [Header("게임 진행 (런타임 확인용)")]
    [SerializeField] private GameState gameState = GameState.Ready;
    [SerializeField] private int currentWave;
    [SerializeField] private int currentEnemyCount;

    private WaveManager _waveManager;
    private bool _enemyLimitExceeded;

    public GameState State => gameState;
    public int CurrentWave => currentWave;
    public int CurrentEnemyCount => EnemyHealth.ActiveEnemyCount;
    public WaveManager WaveManager => _waveManager;
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
        currentWave = 0;
        _enemyLimitExceeded = false;
        ChangeGameState(GameState.Ready);
    }

    public void UnregisterWaveManager(WaveManager manager)
    {
        if (_waveManager != manager)
            return;

        _waveManager.OnWaveStart -= HandleWaveStart;
        _waveManager.OnWaveEnd -= HandleWaveEnd;
        _waveManager.StopWave();
        _waveManager = null;
        _enemyLimitExceeded = false;
        currentWave = 0;
        ChangeGameState(GameState.Ready);
    }

    public bool StartGame()
    {
        if (gameState == GameState.Playing)
            return false;
        if (_waveManager == null || !_waveManager.isActiveAndEnabled)
        {
            Debug.LogError("활성 WaveManager를 씬에 배치하고 웨이브를 설정하세요.", this);
            return false;
        }

        _waveManager.ResetWaves();
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
        return false;
    }

    /// <summary>웨이브/소환을 멈추고 결과를 통지합니다. 결과 화면을 위해 적은 유지합니다.</summary>
    public void EndGame(bool victory = false)
    {
        if (gameState != GameState.Playing)
            return;

        if (_waveManager != null)
            _waveManager.StopWave();
        ChangeGameState(victory ? GameState.Victory : GameState.Defeat);
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
        UpdateGameState();
        if (gameState == GameState.Playing && !_waveManager.IsComplete && !_waveManager.StartWave())
            EndGame();
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

    #region ─────────────────────────▶ 내부 변수 ◀─────────────────────────
    private static Transform _uiRoot;
    private static Transform _objectRoot;
    private static Transform _enableObjectRoot;
    private static Transform _disableObjectRoot;
    private bool _isInitialized = false;
    private EScene _curScene;
    private bool _isBooted = false;
    private Coroutine _bootCoroutine;
    #endregion

    #region ─────────────────────────▶ 공개 멤버 ◀─────────────────────────
    public bool IsSceneLoading { get; private set; }
    public bool IsPlayerWakeUp { get; set; }
    public EScene Scene => _curScene;
    public int NextSpawnPointIndex { get; set; } = -1;
    public void BootComplete() => _isBooted = true;

    public static Transform UIRoot => RootProvider(_uiRoot, K.NAME_UI_ROOT);
    public static Transform ObjectRoot => RootProvider(_objectRoot, K.NAME_OBJECT_ROOT);
    public static Transform EnableObjectRoot => RootProvider(_enableObjectRoot, K.NAME_ENABLE_OBJECT_ROOT);
    public static Transform DisableObjectRoot => RootProvider(_disableObjectRoot, K.NAME_DISABLE_OBJECT_ROOT);

    public override void Initialize()
    {
        if (_isInitialized)
        {
            return;
        }
        // 생성 및 초기화
        EnemyHealth.OnActiveEnemyCountChanged += HandleEnemyCountChanged;
        currentEnemyCount = EnemyHealth.ActiveEnemyCount;
        _curScene = (EScene)SceneManager.GetActiveScene().buildIndex;
        // 초기 부팅 시 씬 전환 이벤트 뿌리기
        if (_bootCoroutine == null)
        {
            _bootCoroutine = StartCoroutine(FirstLoadComplete(EScene.Boot, _curScene));
        }
        else
        {
            UDebug.Print($"부트 코루틴이 중복 호출되었습니다.", LogType.Assert);
        }
        _isInitialized = true;
    }

    /// <summary>
    /// 해당 씬을 동기 로드합니다.
    /// 동일한 이름을 가지는 씬도 있을 수 있기 때문에 표준적으로는 인덱스 사용이 권장됩니다.
    /// </summary>
    /// <param name="index">씬 인덱스</param>
    [Obsolete("비동기 씬 로드를 권장합니다.")]
    public void LoadScene(int index)
    {
        if (!IsValidScene(index))
        {
            return;
        }
        string scenePath = SceneUtility.GetScenePathByBuildIndex(index);
        LoadScene(scenePath); // 경로를 넣어도 씬 매니저에서 알아서 해준다.
    }

    /// <summary>
    /// 해당 씬을 동기 로드합니다.
    /// </summary>
    /// <param name="name">씬 이름</param>
    [Obsolete("비동기 씬 로드를 권장합니다.")]
    public void LoadScene(string name)
    {
        if (!IsValidScene(name))
        {
            return;
        }
        PreProcessing(_curScene, name);
        SceneManager.LoadScene(name, LoadSceneMode.Single);
        PostProcessing(_curScene, name);
    }

    /// <summary>
    /// 해당 씬을 비동기 로드합니다.
    /// 동일한 이름을 가지는 씬도 있을 수 있기 때문에 표준적으로는 인덱스 사용이 권장됩니다.
    /// </summary>
    /// <param name="index">씬 인덱스</param>
    /// <param name="callback">씬 로드 완료 시 호출할 메서드</param>
    /// <param name="onProgress">씬 로드 진행율을 받을 메서드</param>
    /// <param name="delay">씬 로드 시작 전에 대기할 시간(초)</param>
    /// <param name="loadSceneMode">씬 로드 모드</param>
    public void LoadSceneAsync(
        int index, Action callback = null, Action<float> onProgress = null, float delay = 0f,
        LoadSceneMode loadSceneMode = LoadSceneMode.Single)
    {
        if (!IsValidScene(index))
        {
            return;
        }
        string scenePath = SceneUtility.GetScenePathByBuildIndex(index); // 경로를 넣어도 씬 매니저에서 알아서 해준다.
        LoadSceneAsync(scenePath, callback, onProgress, delay, loadSceneMode);
    }

    /// <summary>
    /// 해당 씬을 비동기 로드합니다.
    /// </summary>
    /// <param name="name">씬 이름</param>
    /// <param name="callback">씬 로드 완료 시 호출할 메서드</param>
    /// <param name="onProgress">씬 로드 진행율을 받을 메서드</param>
    /// <param name="delay">씬 로드 시작 전에 대기할 시간(초)</param>
    /// <param name="loadSceneMode">씬 로드 모드</param>
    public void LoadSceneAsync(
        string name, Action callback = null, Action<float> onProgress = null, float delay = 0f,
        LoadSceneMode loadSceneMode = LoadSceneMode.Single)
    {
        if (!IsValidScene(name))
        {
            return;
        }
        PreProcessing(_curScene, name);
        StartCoroutine(DoLoadSceneAsync(name, callback, onProgress, delay, loadSceneMode));
    }

    /// <summary>
    /// 해당 씬을 페이드 효과로 비동기 로드합니다.
    /// 동일한 이름을 가지는 씬도 있을 수 있기 때문에 표준적으로는 인덱스 사용이 권장됩니다.
    /// </summary>
    /// <param name="index">씬 인덱스</param>
    /// <param name="callback">씬 로드 완료 시 호출할 메서드</param>
    /// <param name="onProgress">씬 로드 진행율을 받을 메서드</param>
    /// <param name="delay">씬 로드 시작 전에 대기할 시간(초)</param>
    /// <param name="loadSceneMode">씬 로드 모드</param>
    public void LoadSceneAsyncWithFade(
        int index, float delay = 0f, float fadeOutTime = 0.9f, float fadeInTime = 0.9f,
        Action callback = null, Action<float> onProgress = null, 
        LoadSceneMode loadSceneMode = LoadSceneMode.Single)
    {
        if (!IsValidScene(index))
        {
            return;
        }
        string scenePath = SceneUtility.GetScenePathByBuildIndex(index); // 경로를 넣어도 씬 매니저에서 알아서 해준다.
        LoadSceneAsyncWithFade(scenePath, delay, fadeOutTime, fadeInTime, callback, onProgress, loadSceneMode);
    }

    /// <summary>
    /// 해당 씬을 페이드 효과로 비동기 로드합니다.
    /// </summary>
    /// <param name="name">씬 이름</param>
    /// <param name="callback">씬 로드 완료 시 호출할 메서드</param>
    /// <param name="onProgress">씬 로드 진행율을 받을 메서드</param>
    /// <param name="delay">씬 로드 시작 전에 대기할 시간(초)</param>
    /// <param name="loadSceneMode">씬 로드 모드</param>
    public void LoadSceneAsyncWithFade(
        string name, float delay = 0f, float fadeOutTime = 0.2f, float fadeInTime = 0.2f,
        Action callback = null, Action<float> onProgress = null, 
        LoadSceneMode loadSceneMode = LoadSceneMode.Single)
    {
        if (!IsValidScene(name))
        {
            return;
        }
        PreProcessing(_curScene, name);
        StartCoroutine(DoLoadSceneAsyncWithFade
            (name, delay, fadeOutTime, fadeInTime, callback, onProgress, loadSceneMode));
    }
    #endregion

    #region ─────────────────────────▶ 내부 메서드 ◀─────────────────────────
    // 루트 오브젝트를 안전하게 가져오고 없으면 새로 생성
    private static Transform RootProvider(Transform root, string name)
    {
        if (root == null)
        {
            GameObject go = GameObject.Find(name);
            if (go == null)
            {
                root = UObject.Create(name).transform;
                UDebug.Print($"{name} 루트를 찾지 못하여 빈 오브젝트를 새로 생성했습니다.");
            }
            else
            {
                root = go.transform;
            }
        }
        return root;
    }

    // 씬 로드 선행처리
    private void PreProcessing(EScene prevScene, string nextScenePath)
    {
        if (gameState == GameState.Playing)
            EndGame();
        _uiRoot = null;
        _objectRoot = null;
        EScene nextScene = (EScene)SceneUtility.GetBuildIndexByScenePath(nextScenePath);
        OnSceneLoadStart.Publish(prevScene, nextScene);
        _curScene = nextScene;
        IsSceneLoading = true;
    }
    // 씬 로드 후처리
    private void PostProcessing(EScene prevScene, string nextScenePath)
    {
        EScene nextScene = (EScene)SceneUtility.GetBuildIndexByScenePath(nextScenePath);
        // 루트 생성
        {
            Transform root = ObjectRoot;
        }
        {
            Transform root = EnableObjectRoot;
        }
        {
            Transform root = DisableObjectRoot;
        }
        {
            Transform root = UIRoot;
        }
        PublishLoadEnd(prevScene, nextScene);
        _curScene = nextScene;
        IsSceneLoading = false;
    }

    // 씬 유효성 검증
    private static bool IsValidScene(int index)
    {
        // 존재할 수 없는 인덱스인지 검사
        if (index < 0 || index >= SceneManager.sceneCountInBuildSettings)
        {
            UDebug.Print($"존재하지 않는 씬 인덱스({index})를 호출했습니다.");
            return false;
        }
        return true;
    }
    private static bool IsValidScene(string name)
    {
        // 존재할 수 없는 인덱스인지 검사
        if (name.IsEmpty() || !Application.CanStreamedLevelBeLoaded(name))
        {
            UDebug.Print($"존재하지 않는 씬 이름({name})을 호출했습니다.");
            return false;
        }
        return true;
    }

    // 비동기 코루틴
    private IEnumerator DoLoadSceneAsync(
        string name, Action callback, Action<float> onProgress, float delay, LoadSceneMode loadSceneMode)
    {
        if (delay > 0f)
        {
            yield return UCoroutine.GetWait(delay);
        }
        // 유니티 기본 로드 함수 (비동기 대기)
        var asyncOperation = SceneManager.LoadSceneAsync(name, loadSceneMode);
        // 유니티 비동기 씬 로드 유틸리티
        yield return UCoroutine.WaitAsyncOperation(asyncOperation, onProgress);
        // 씬 로드 완료 → 콜백 호출
        callback?.Invoke();
        PostProcessing(_curScene, name);
    }

    // 비동기 페이드 코루틴
    private IEnumerator DoLoadSceneAsyncWithFade(
        string name, float delay, float fadeOutTime, float fadeInTime,
        Action callback, Action<float> onProgress, LoadSceneMode loadSceneMode)
    {
        if (delay > 0f)
        {
            yield return UCoroutine.GetWait(delay);
        }
        // 유니티 기본 로드 함수 (비동기 대기)
        var asyncOperation = SceneManager.LoadSceneAsync(name, loadSceneMode);
        asyncOperation.allowSceneActivation = false; // 씬 로드가 완료되어도 대기
        // 페이드 시작
        UFade.FadeOut(fadeOutTime, true);
        // 모두 완료될때까지 대기
        while(asyncOperation.progress < 0.9f || UFade.IsFading)
        {
            onProgress?.Invoke(asyncOperation.progress);
            yield return null;
        }
        asyncOperation.allowSceneActivation = true;
        onProgress?.Invoke(1f);
        // 씬 전환이 완전히 종료될때까지 대기
        while (!asyncOperation.isDone)
        {
            yield return null;
        }
        // 씬 전환 완료
        callback?.Invoke();
        PostProcessing(_curScene, name);
        // 새로운 씬에서 페이드 인
        UFade.FadeIn(fadeInTime, true);
    }

    private void PublishLoadEnd(EScene prevScene, EScene nextScene)
    {
        OnSceneLoadEnd.Publish(prevScene, nextScene);
        //var provider = DataManager.Ins.Player;
        //if (provider != null)
        //{
        //    provider.SaveSceneId(_curScene);
        //}
    }

    // 유니티 에디터 용도, 어느 씬에서 시작하던 그 씬을 로드하는 효과를 내기 위함
    private IEnumerator FirstLoadComplete(EScene prevScene, EScene nextScene)
    {
        while (!_isBooted)
        {
            yield return null;
        }
        PublishLoadEnd(prevScene, nextScene);
        _bootCoroutine = null;
    }
    #endregion
}
