using UnityEngine;

/// <summary>도메인 리로드를 끈 에디터에서도 매 실행마다 종료 상태를 초기화합니다.</summary>
public static class ApplicationLifetime
{
    public static bool IsQuitting { get; private set; }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void Reset()
    {
        IsQuitting = false;
        Application.quitting -= OnQuitting;
        Application.quitting += OnQuitting;
    }

    private static void OnQuitting() => IsQuitting = true;
}
