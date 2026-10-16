using System;
using UnityEngine;

/// <summary>전투 씬의 배속과 자동 진행 입력을 처리합니다. 일시정지 중에는 배속을 덮어쓰지 않습니다.</summary>
public class BattlePlaybackController : MonoBehaviour
{
    private static readonly float[] Speeds = { 0.5f, 1f, 2f, 3f };
    private int _speedIndex = 1;
    private WaveManager _waves;

    public float Speed => Speeds[_speedIndex];
    public bool AutoAdvance => _waves != null && _waves.AutoAdvance;
    public bool CanInteract => _waves != null && _waves.isActiveAndEnabled &&
        !SettingsPanel.IsOpen && !SceneFlowManager.Ins.IsTransitioning &&
        GameManager.Ins.State == GameState.Playing;
    public event Action OnChanged;

    // 비활성 UI 루트에서 활성화 전에 호출합니다.
    public void Configure(WaveManager waves) => _waves = waves;

    public void CycleSpeed()
    {
        if (!CanInteract) return;
        _speedIndex = (_speedIndex + 1) % Speeds.Length;
        Time.timeScale = Speed;
        OnChanged?.Invoke();
    }

    public void ToggleAutoAdvance()
    {
        if (CanInteract) _waves.SetAutoAdvance(!_waves.AutoAdvance);
    }

    private void OnEnable()
    {
        if (_waves == null) return;
        _waves.OnAutoAdvanceChanged += HandleAutoAdvanceChanged;
        _waves.OnWavesReset += ResetSpeed;
    }

    private void OnDisable()
    {
        if (_waves != null)
        {
            _waves.OnAutoAdvanceChanged -= HandleAutoAdvanceChanged;
            _waves.OnWavesReset -= ResetSpeed;
        }
        // 설정창의 정지/복원은 SettingsPanel, 씬 전환의 최종 복원은 SceneFlowManager가 소유합니다.
        if (!SettingsPanel.IsOpen) Time.timeScale = 1f;
    }

    private void ResetSpeed()
    {
        _speedIndex = 1;
        if (!SettingsPanel.IsOpen) Time.timeScale = Speed;
        OnChanged?.Invoke();
    }

    private void HandleAutoAdvanceChanged(bool value) => OnChanged?.Invoke();
}
