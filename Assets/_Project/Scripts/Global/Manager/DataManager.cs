using System;
using UnityEngine;

/// <summary>메인/전투에서 공유하는 옵션을 불러오고 적용, 저장합니다.</summary>
public class DataManager : GlobalSingleton<DataManager>
{
    public const string OptionsKey = "Monoeye.Options.v1";
    private bool _isInitialized;
    private OptionData _gameOption;
    private bool _dirty;
    private float _saveAt;
    public OptionData Option => _gameOption;
    public VolumeData Volume => _gameOption.Volume;
    public ScreenData Screen => _gameOption.Screen;
    public string LastSaveError { get; private set; }
    public event Action OnOptionsChanged;

    public override void Initialize()
    {
        if (_isInitialized) return;
        _isInitialized = true;
        _gameOption = new OptionData();
        try
        {
            if (PlayerPrefs.HasKey(OptionsKey))
                JsonUtility.FromJsonOverwrite(PlayerPrefs.GetString(OptionsKey), _gameOption);
        }
        catch (Exception exception)
        {
            _gameOption = new OptionData();
            Debug.LogWarning($"설정을 읽지 못해 기본값을 사용합니다: {exception.Message}");
        }
        _gameOption.Validate();
        ApplyDisplay();
    }

    public void ResetGameData()
    {
        _gameOption = new OptionData();
        ApplyDisplay();
        USound.UpdateVolumes();
        OptionsChanged();
    }

    public void OptionsChanged()
    {
        _dirty = true;
        _saveAt = Time.unscaledTime + .4f;
        OnOptionsChanged?.Invoke();
    }

    public bool SaveOptions()
    {
        if (!_dirty) return LastSaveError == null;
        try
        {
            PlayerPrefs.SetString(OptionsKey, JsonUtility.ToJson(_gameOption));
            PlayerPrefs.Save();
            _dirty = false;
            LastSaveError = null;
            return true;
        }
        catch (Exception exception)
        {
            LastSaveError = exception.Message;
            _saveAt = Time.unscaledTime + 5f;
            return false;
        }
    }

    public static FullScreenMode ToFullScreenMode(EScreenMode mode) => mode switch
    {
        EScreenMode.Windowed => FullScreenMode.Windowed,
        EScreenMode.Borderless => FullScreenMode.FullScreenWindow,
        _ => FullScreenMode.ExclusiveFullScreen
    };

    public void ApplyDisplay()
    {
        UnityEngine.Screen.SetResolution(Screen.Width, Screen.Height, ToFullScreenMode(Screen.Mode));
        OnResolutionChanged.Publish(Screen.Width, Screen.Height);
    }

    private void Update() { if (_dirty && Time.unscaledTime >= _saveAt) SaveOptions(); }
    private void OnApplicationPause(bool paused) { if (paused) SaveOptions(); }
    private void OnApplicationQuit() => SaveOptions();
}
