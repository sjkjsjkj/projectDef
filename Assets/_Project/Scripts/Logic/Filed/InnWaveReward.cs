using UnityEngine;

/// <summary>완료한 웨이브 횟수마다 초기화권을 지급합니다. 웨이브 번호에는 의존하지 않습니다.</summary>
public class InnWaveReward : BaseMono
{
    [SerializeField] private WaveManager waveManager;
    [SerializeField] private ResourceManager resources;
    [Tooltip("0이면 웨이브 지급을 사용하지 않습니다. 지급 주기 확정 후 설정하세요.")]
    [SerializeField, Min(0)] private int wavesPerReward;
    [SerializeField, Min(1)] private int ticketsPerReward = 1;
    private int _completedWaves;

    private void OnEnable()
    {
        if (waveManager == null) return;
        waveManager.OnWaveEnd += HandleWaveEnd;
        waveManager.OnWavesReset += ResetProgress;
    }

    private void OnDisable()
    {
        if (waveManager == null) return;
        waveManager.OnWaveEnd -= HandleWaveEnd;
        waveManager.OnWavesReset -= ResetProgress;
    }

    private void ResetProgress() => _completedWaves = 0;

    private void HandleWaveEnd(int waveNumber)
    {
        if (wavesPerReward <= 0 || resources == null) return;
        _completedWaves++;
        if (_completedWaves < wavesPerReward) return;
        _completedWaves = 0;
        resources.AddInnRefreshTickets(Mathf.Max(1, ticketsPerReward));
    }
}
