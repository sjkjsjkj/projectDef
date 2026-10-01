using UnityEngine;

/// <summary>
/// 게임의 각종 설정을 저장하는 데이터 클래스입니다.
/// </summary>
[System.Serializable]
public class OptionData
{
    [SerializeField] private VolumeData _volume = new();
    [SerializeField] private ScreenData _screen = new();

    public VolumeData Volume => _volume;
    public ScreenData Screen => _screen;
}
