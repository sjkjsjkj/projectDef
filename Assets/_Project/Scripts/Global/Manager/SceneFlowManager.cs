using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>앱 수명의 씬 전환 담당. 전투 상태와 씬 오브젝트를 소유하지 않습니다.</summary>
public class SceneFlowManager : GlobalSingleton<SceneFlowManager>
{
    [SerializeField, Min(0f)] private float fadeOutTime = 0.2f;
    [SerializeField, Min(0f)] private float fadeInTime = 0.2f;
    [SerializeField] private SceneFlowState state = SceneFlowState.Booting;
    private bool _initialized;
    private bool _bootCompletionStarted;

    public SceneFlowState State => state;
    public bool IsTransitioning => state != SceneFlowState.Ready;
    public EScene CurrentScene { get; private set; } = EScene.Unknown;
    public float Progress { get; private set; }
    public string LastError { get; private set; }
    public event Action<SceneFlowState> OnStateChanged;
    public event Action<string> OnLoadFailed;

    public override void Initialize()
    {
        if (_initialized) return;
        _initialized = true;
        UFade.SetOverlay(1f, true, true);
    }

    public void CompleteBoot()
    {
        if (_bootCompletionStarted) return;
        _bootCompletionStarted = true;
        StartCoroutine(FinishBoot());
    }

    private IEnumerator FinishBoot()
    {
        // 첫 씬의 Awake/Start와 글로벌 프리팹 초기화를 마친 뒤 화면을 엽니다.
        yield return null;
        yield return new WaitForSecondsRealtime(0.15f);
        CurrentScene = SceneCatalog.Identify(SceneManager.GetActiveScene().name);
        UFade.SetOverlay(1f, true, false);
        SetState(SceneFlowState.FadingIn);
        UFade.FadeIn(fadeInTime, true);
        while (UFade.IsFading) yield return null;
        UFade.SetOverlay(0f, false, false);
        Progress = 1f;
        SetState(SceneFlowState.Ready);
        OnSceneLoadEnd.Publish(EScene.Boot, CurrentScene);
    }

    public bool StartStage()
    {
        if (CurrentScene != EScene.Main) return false;
        return TryLoadScene(SceneCatalog.StageScene, callback: () =>
        {
            GameManager game = GameManager.Ins;
            if (game.State != GameState.Playing) game.StartGame();
        });
    }

    public bool ReturnToMain() => TryLoadScene(SceneCatalog.MainScene);

    public bool TryLoadScene(string sceneName, Action callback = null, Action<float> onProgress = null,
        float delay = 0f, float? fadeOut = null, float? fadeIn = null,
        LoadSceneMode mode = LoadSceneMode.Single)
    {
        // 페이드 아웃을 시작하는 프레임부터 중복 클릭/요청을 차단합니다.
        if (IsTransitioning) return false;
        if (string.IsNullOrWhiteSpace(sceneName) || !Application.CanStreamedLevelBeLoaded(sceneName))
        {
            Fail($"씬 '{sceneName}'을 로드할 수 없습니다. Build Settings의 활성 씬을 확인하세요.");
            return false;
        }
        LastError = null;
        Progress = 0f;
        SetState(SceneFlowState.FadingOut);
        StartCoroutine(Transition(sceneName, callback, onProgress, Mathf.Max(0f, delay),
            Mathf.Max(0f, fadeOut ?? fadeOutTime), Mathf.Max(0f, fadeIn ?? fadeInTime), mode));
        return true;
    }

    private IEnumerator Transition(string sceneName, Action callback, Action<float> onProgress,
        float delay, float fadeOut, float fadeIn, LoadSceneMode mode)
    {
        EScene previous = CurrentScene;
        EScene next = SceneCatalog.Identify(sceneName);
        bool loaded = false;
        try
        {
            UFade.SetOverlay(0f, true, false);
            OnSceneLoadStart.Publish(previous, next);
            if (delay > 0f) yield return new WaitForSecondsRealtime(delay);
            UFade.FadeOut(fadeOut, true);
            while (UFade.IsFading) yield return null;
            UFade.SetOverlay(1f, true, true);
            SetState(SceneFlowState.Loading);
            AsyncOperation operation = BeginLoad(sceneName, mode);
            if (operation == null) yield break;
            if (mode == LoadSceneMode.Single) GameManager.Ins.PrepareSceneChange();
            while (!operation.isDone)
            {
                Progress = Mathf.Clamp01(operation.progress / 0.9f);
                onProgress?.Invoke(Progress);
                yield return null;
            }
            // 새 씬의 Start가 실행된 뒤 페이드 인합니다. 웨이브는 Ready까지 대기합니다.
            yield return null;
            if (mode == LoadSceneMode.Single) CurrentScene = next;
            Progress = 1f;
            onProgress?.Invoke(Progress);
            UFade.SetOverlay(1f, true, false);
            SetState(SceneFlowState.FadingIn);
            UFade.FadeIn(fadeIn, true);
            while (UFade.IsFading) yield return null;
            loaded = true;
        }
        finally
        {
            if (!ApplicationLifetime.IsQuitting)
            {
                UFade.SetOverlay(0f, false, false);
                SetState(SceneFlowState.Ready);
            }
        }
        if (loaded)
        {
            // 일시정지 상태에서 메인으로 돌아온 뒤에도 다음 게임은 정상 속도로 시작합니다.
            if (mode == LoadSceneMode.Single) Time.timeScale = 1f;
            OnSceneLoadEnd.Publish(previous, next);
            callback?.Invoke();
        }
    }

    private AsyncOperation BeginLoad(string sceneName, LoadSceneMode mode)
    {
        try
        {
            AsyncOperation operation = SceneManager.LoadSceneAsync(sceneName, mode);
            if (operation == null) Fail($"씬 '{sceneName}' 로드를 시작하지 못했습니다.");
            return operation;
        }
        catch (Exception exception)
        {
            Fail(exception.Message);
            return null;
        }
    }

    private void Fail(string message)
    {
        LastError = message;
        Debug.LogError(message, this);
        OnLoadFailed?.Invoke(message);
    }

    private void SetState(SceneFlowState value)
    {
        state = value;
        OnStateChanged?.Invoke(value);
    }
}
