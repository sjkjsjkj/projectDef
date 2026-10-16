using UnityEngine;

/// <summary>
/// 게임의 각종 설정을 저장하는 데이터 클래스입니다.
/// </summary>
[System.Serializable]
public class OptionData
{
    [SerializeField] private VolumeData _volume = new();
    [SerializeField] private ScreenData _screen = new();
    [SerializeField] private bool _showDamage = true;
    [SerializeField] private GameLanguage _language = GameLanguage.Korean;

    public VolumeData Volume => _volume;
    public ScreenData Screen => _screen;
    public bool ShowDamage { get => _showDamage; set => _showDamage = value; }
    public GameLanguage Language { get => _language; set => _language = value; }

    public void Validate()
    {
        _volume ??= new VolumeData();
        _screen ??= new ScreenData();
        _volume.Validate();
        _screen.Width = _screen.Width;
        _screen.Height = _screen.Height;
        if (!System.Enum.IsDefined(typeof(EScreenMode), _screen.Mode)) _screen.Mode = EScreenMode.FullScreen;
        if (!System.Enum.IsDefined(typeof(GameLanguage), _language)) _language = GameLanguage.Korean;
    }
}

public enum GameLanguage { Korean, English }
