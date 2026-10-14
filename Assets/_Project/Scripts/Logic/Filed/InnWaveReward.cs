using UnityEngine;

/// <summary>완료한 웨이브 횟수마다 초기화권과 지명영입권을 지급합니다.</summary>
public class InnWaveReward : BaseMono
{
    [SerializeField] private WaveManager waveManager;
    [SerializeField] private ResourceManager resources;
    [Tooltip("0이면 웨이브 지급을 사용하지 않습니다. 지급 주기 확정 후 설정하세요.")]
    [SerializeField, Min(0)] private int wavesPerReward;
    [SerializeField, Min(1)] private int ticketsPerReward = 1;
    private int _completedWaves;

    [Header("지명영입권 보상")]
    [Tooltip("완료한 라운드 몇 회마다 지급할지 설정합니다. 0이면 지급하지 않습니다.")]
    [SerializeField, Min(0)] private int wavesPerRecruitmentReward;
    [SerializeField] private RecruitmentTicketAmounts recruitmentTicketsPerReward = new RecruitmentTicketAmounts();
    private int _recruitmentCompletedWaves;

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

    private void ResetProgress()
    {
        _completedWaves = 0;
        _recruitmentCompletedWaves = 0;
    }

    private void HandleWaveEnd(int waveNumber)
    {
        if (resources == null) return;
        if (wavesPerReward > 0 && ++_completedWaves >= wavesPerReward)
        {
            _completedWaves = 0;
            resources.AddInnRefreshTickets(Mathf.Max(1, ticketsPerReward));
        }
        if (wavesPerRecruitmentReward > 0 && ++_recruitmentCompletedWaves >= wavesPerRecruitmentReward)
        {
            _recruitmentCompletedWaves = 0;
            recruitmentTicketsPerReward?.GrantTo(resources);
        }
    }
}
