using UnityEngine;

/// <summary>
/// 볼륨 설정을 담는 데이터 클래스입니다.
/// </summary>
[System.Serializable]
public class VolumeData
{
    #region ─────────────────────────▶ 내부 변수 ◀─────────────────────────
    [SerializeField, Range(0f, 1f)] private float _master = 0.7f;
    [SerializeField, Range(0f, 1f)] private float _bgm = 0.5f;
    [SerializeField, Range(0f, 1f)] private float _sfx = 0.8f;
    [SerializeField] private bool _isBgmMute = false;
    [SerializeField] private bool _isSfxMute = false;
    #endregion

    #region ─────────────────────────▶ 프로퍼티 ◀─────────────────────────
    public float Master
    {
        get => _master;
        set
        {
            float newVolume = Mathf.Clamp01(value);
            if(_master == newVolume)
            {
                return;
            }
            _master = newVolume;
            USound.UpdateVolumes();
        }
    }
    public float Bgm
    {
        get => _bgm;
        set
        {
            float newVolume = Mathf.Clamp01(value);
            if (_bgm == newVolume)
            {
                return;
            }
            _bgm = newVolume;
            USound.UpdateVolumes();
        }
    }
    public float Sfx
    {
        get => _sfx;
        set { _sfx = Mathf.Clamp01(value); USound.UpdateVolumes(); }
    }
    public bool IsBgmMute
    {
        get => _isBgmMute;
        set { _isBgmMute = value; USound.UpdateVolumes(); }
    }
    public bool IsSfxMute
    {
        get => _isSfxMute;
        set { _isSfxMute = value; USound.UpdateVolumes(); }
    }
    public void Validate()
    {
        _master = ValidVolume(_master, .7f);
        _bgm = ValidVolume(_bgm, .5f);
        _sfx = ValidVolume(_sfx, .8f);
    }
    private static float ValidVolume(float value, float fallback) =>
        float.IsNaN(value) || float.IsInfinity(value) ? fallback : Mathf.Clamp01(value);
    #endregion
}
