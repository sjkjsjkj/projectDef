using UnityEngine;

/// <summary>
/// 상속받은 클래스는 항상 싱글톤을 보장합니다.
/// </summary>
public abstract class GlobalSingleton<T> : BaseMono where T : BaseMono
{
    private static T _instance = null;
    private static bool _isQuitting = false;

    public static T Ins
    {
        get
        {
            if (_instance == null)
            {
                // 플레이 모드가 종료 중
                if (_isQuitting)
                {
                    UDebug.Print($"글로벌 싱글톤({typeof(T).ToString()})이 호출당했지만 앱 종료중이므로 무시합니다.");
                    return null;
                }
                // 씬에 있는 싱글톤 컴포넌트를 우선 탐색
                T globalSingleton = FindAnyObjectByType<T>();
                if (globalSingleton != null)
                {
                    _instance = globalSingleton;
                    DontDestroyOnLoad(globalSingleton.gameObject);
                    UDebug.Print($"글로벌 싱글톤({globalSingleton.gameObject.name}<{typeof(T).ToString()}>)을 탐색하여 등록했습니다.");
                }
                // 씬에 배치되어 있지 않으므로 생성
                else
                {
                    GameObject go = new GameObject(typeof(T).ToString());
                    _instance = go.AddComponent<T>();
                    DontDestroyOnLoad(go);
                    UDebug.Print($"인스턴스가 호출당하여 글로벌 싱글톤({go.name}<{typeof(T).ToString()}>)을 생성했습니다.");
                }
                (_instance as GlobalSingleton<T>)?.Initialize();
            }
            return _instance;
        }
    }

    /// <summary>
    /// 인스턴스 생성 시 필요한 초기화 로직을 구현하는 함수
    /// </summary>
    public abstract void Initialize();

    /// <summary>
    /// GlobalSingleton Awake Function
    /// </summary>
    protected override void Awake()
    {
        base.Awake();
        if (_instance != null && _instance != this)
        {
            Destroy(this);
            UDebug.Print($"글로벌 싱글톤({gameObject.name}<{typeof(T).ToString()}>)이 Awake에서 삭제되었습니다.");
            return;
        }
        _instance = this as T;
        DontDestroyOnLoad(gameObject);
        UDebug.Print($"글로벌 싱글톤({gameObject.name}<{typeof(T).ToString()}>)이 Awake를 통해 등록했습니다.");
        (_instance as GlobalSingleton<T>)?.Initialize();
    }

    /// <summary>
    /// GlobalSingleton OnDestroy Function
    /// </summary>
    protected virtual void OnDestroy()
    {
        if (_instance == this)
        {
            _instance = null;
            UDebug.Print($"글로벌 싱글톤 인스턴스({typeof(T).ToString()})를 청소했습니다.");
        }
    }

    // 플레이 모드가 종료될 경우 호출
    private void OnApplicationQuit()
    {
        _isQuitting = true;
    }
}
