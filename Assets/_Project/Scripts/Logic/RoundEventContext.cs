/// <summary>효과가 사용하는 이번 실행의 맥락입니다. 효과별 대상 선택은 효과 구현에서 확장합니다.</summary>
public sealed class RoundEventContext
{
    public ResourceManager Resources { get; }
    public WaveManager Waves { get; }
    public int CompletedWaveNumber { get; }

    public RoundEventContext(ResourceManager resources, WaveManager waves, int completedWaveNumber)
    {
        Resources = resources;
        Waves = waves;
        CompletedWaveNumber = completedWaveNumber;
    }
}
