using System.Collections.Generic;
using UnityEngine;

/// <summary>상태이상이 있는 대상만 갱신합니다. 이동보다 먼저 실행하며 프레임별 목록 할당을 피합니다.</summary>
[DefaultExecutionOrder(-100)]
[DisallowMultipleComponent]
public sealed class StatusEffectSystem : MonoBehaviour
{
    private struct Entry
    {
        public StatusEffectController Controller;
        public uint Version;
    }

    private static readonly HashSet<StatusEffectController> Active = new HashSet<StatusEffectController>();
    private static readonly List<Entry> Snapshot = new List<Entry>();
    private static StatusEffectSystem _instance;
    private static bool _ticking;

    public static int ActiveControllerCount => Active.Count;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics()
    {
        Active.Clear();
        Snapshot.Clear();
        _instance = null;
        _ticking = false;
    }

    internal static void Register(StatusEffectController controller)
    {
        if (Active.Add(controller)) controller.RegistrationVersion++;
        if (!Application.isPlaying || ApplicationLifetime.IsQuitting || _instance != null) return;
        _instance = FindAnyObjectByType<StatusEffectSystem>();
        if (_instance == null) _instance = new GameObject(nameof(StatusEffectSystem)).AddComponent<StatusEffectSystem>();
    }

    internal static void Unregister(StatusEffectController controller)
    {
        if (Active.Remove(controller)) controller.RegistrationVersion++;
    }

    private void Awake()
    {
        if (_instance != null && _instance != this) { Destroy(this); return; }
        _instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Update()
    {
        if (Active.Count == 0) return;
        TickAll(Time.deltaTime, GameManager.Ins != null && GameManager.Ins.IsCombatPaused);
    }

    /// <summary>갱신 도중 사망/해제/신규 등록이 발생해도 안전하게 처리합니다.</summary>
    public static void TickAll(float deltaTime, bool paused = false)
    {
        if (paused || _ticking || deltaTime <= 0f || float.IsNaN(deltaTime) || float.IsInfinity(deltaTime)) return;
        _ticking = true;
        try
        {
            Snapshot.Clear();
            foreach (StatusEffectController controller in Active)
                if (controller != null) Snapshot.Add(new Entry { Controller = controller, Version = controller.RegistrationVersion });
            foreach (Entry entry in Snapshot)
            {
                StatusEffectController controller = entry.Controller;
                if (controller != null && Active.Contains(controller) && controller.RegistrationVersion == entry.Version)
                    controller.Tick(deltaTime);
            }
        }
        finally
        {
            Snapshot.Clear();
            _ticking = false;
        }
    }

    private void OnDestroy()
    {
        if (_instance == this) _instance = null;
    }
}
