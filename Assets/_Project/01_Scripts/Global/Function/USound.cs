using UnityEngine;

/// <summary>
/// 요청받은 소리를 재생하는 사운드 유틸리티 클래스입니다.
/// </summary>

public static class USound
{
    private static AudioSource _mainSource;
    private static AudioSource _bgmSource; // BGM을 재생할 파괴되지 않는 글로벌 오디오 오브젝트
    private static string _curBgmId; // 현재 재생중인 BGM ID

    #region ─────────────────────────▶ 공개 멤버 ◀─────────────────────────
    /// <summary>
    /// 메인 카메라에서 사운드를 재생합니다.
    /// </summary>
    public static void PlaySfx(string id)
    {
        SoundSO soundSO = null;
        if (TryGetMainSource(out AudioSource mainSource)
            && TryGetSoundSO(id, out soundSO))
        {
            PlayOneSfx(mainSource, soundSO);
        }
        // 오디오 소스 또는 SO 누락
        else
        {
            UDebug.Print("효과음을 재생하지 못했습니다. " +
                $"(AudioSource = {mainSource}, soundSO = {(soundSO == null ? null : soundSO.name)})");
        }
    }

    /// <summary>
    /// 좌표에서 사운드를 재생합니다.
    /// </summary>
    public static void PlaySfx(string id, Vector3 pos)
    {
        SoundSO soundSO = null;
        if (TryGetMainSource(out AudioSource mainSource)
            && TryGetSoundSO(id, out soundSO))
        {
            PlaySfx(soundSO, pos);
        }
        // 오디오 소스 또는 SO 누락
        else
        {
            UDebug.Print("효과음을 재생하지 못했습니다. " +
                $"(AudioSource = {mainSource}, soundSO = {(soundSO == null ? null : soundSO.name)})");
        }
    }

    /// <summary>
    /// 오브젝트에서 사운드를 재생합니다.
    /// </summary>
    public static void PlaySfx(string id, Transform tr)
    {
        SoundSO soundSO = null;
        if (TryGetMainSource(out AudioSource mainSource)
            && TryGetSoundSO(id, out soundSO))
        {
            PlaySfx(soundSO, tr);
        }
        // 오디오 소스 또는 SO 누락
        else
        {
            UDebug.Print("효과음을 재생하지 못했습니다. " +
                $"(AudioSource = {mainSource}, soundSO = {(soundSO == null ? null : soundSO.name)})");
        }
    }

    /// <summary>
    /// BGM이 재생 중인지 여부를 반환합니다.
    /// </summary>
    public static bool IsPlayingBgm()
    {
        if (_bgmSource != null)
        {
            return _bgmSource.isPlaying;
        }
        return false;
    }

    /// <summary>
    /// BGM을 변경하고 재생합니다.
    /// </summary>
    /// <param name="id"></param>
    public static void PlayBgm(string id)
    {
        // 방어 코드
        if (id.IsEmpty())
        {
            return;
        }
        if (TryGetBgmSource(out AudioSource source))
        {
            PlayBgm(source, id);
        }
    }

    /// <summary>
    /// BGM 재생을 중단합니다.
    /// </summary>
    /// <param name="source">글로벌 BGM 오디오 소스</param>
    public static void StopBgm()
    {
        if (_bgmSource == null)
        {
            UDebug.Print($"BGM 오디오 소스가 존재하지 않습니다.", LogType.Assert);
            return;
        }
        _curBgmId = string.Empty;
        _bgmSource.Stop();
    }

    /// <summary>
    /// BGM 소리 크기가 변경되었을 경우 호출해주세요.
    /// </summary>
    public static void UpdateBgmVolume()
    {
        if (!TryGetBgmSource(out AudioSource source))
        {
            UDebug.Print($"BGM 오디오 소스가 존재하지 않습니다.", LogType.Assert);
            return;
        }
        if (_curBgmId.IsEmpty())
        {
            return;
        }
        if (!TryGetSoundSO(_curBgmId, out SoundSO so))
        {
            UDebug.Print($"ID와 일치하는 SoundSO 데이터가 존재하지 않습니다.", LogType.Assert);
            return;
        }
        var volume = DataManager.Ins.Volume;
        _bgmSource.volume = so.Volume * volume.Master * volume.Bgm;
    }
    #endregion

    #region ─────────────────────────▶ 내부 메서드 ◀─────────────────────────
    // 메인 오디오 소스를 갱신합니다.
    private static bool TryGetMainSource(out AudioSource source)
    {
        // 이미 메인 오디오 소스가 있음
        if (_mainSource != null && _mainSource.gameObject != null)
        {
            source = _mainSource;
            return true;
        }
        // 메인 카메라가 존재하지 않음
        Camera mainCam = UCamera.MainCamera;
        if (mainCam == null)
        {
            source = null;
            return false;
        }
        // 오디오 컴포넌트가 없다면 생성
        if (UCamera.MainCamera.TryGetComponent(out source))
        {
            UDebug.Print($"메인 카메라에서 새로운 오디오 소스를 캐싱했습니다.");
        }
        // 없다면 생성
        else
        {
            source = UObject.AddComponent<AudioSource>(mainCam.gameObject);
            UDebug.Print($"메인 카메라에 오디오 소스가 없어서 새로 생성했습니다.", LogType.Warning);
        }
        _mainSource = source;
        return true;
    }
    // 글로벌 BGM 소스를 갱신합니다.
    private static bool TryGetBgmSource(out AudioSource source)
    {
        // 이미 메인 오디오 소스가 있음
        if (_bgmSource != null && _bgmSource.gameObject != null)
        {
            source = _bgmSource;
            return true;
        }
        // 메인 오디오 소스가 없다면 생성
        GameObject go = UObject.Create(K.NAME_BGM_OBJECT);
        Object.DontDestroyOnLoad(go); // 글로벌 
        source = UObject.AddComponent<AudioSource>(go);
        source.playOnAwake = false;
        source.loop = true;
        _bgmSource = source;
        return true;
    }

    // 효과음 재생
    private static void PlayOneSfx(AudioSource source, SoundSO so)
    {
        var volume = DataManager.Ins.Volume;
        source.volume = so.Volume * volume.Master * volume.Sfx;
        source.PlayOneShot(so.Clip);
    }

    // 효과음 재생
    private static void PlaySfx(AudioSource source, SoundSO so)
    {
        var volume = DataManager.Ins.Volume;
        source.volume = so.Volume * volume.Master * volume.Sfx;
        source.clip = so.Clip;
        source.Play();
    }

    // 효과음 재생 ─ 좌표
    private static void PlaySfx(SoundSO so, Vector3 pos)
    {
        var emitter = PoolManager.Ins.Spawn<SoundEmitter>(K.NAME_SOUND_EMITTER);
        emitter.Follow(pos);
        AudioSource source = emitter.Source;
        if(source != null)
        {
            PlaySfx(source, so);
        }
    }

    // 효과음 재생 ─ 트랜스폼
    private static void PlaySfx(SoundSO so, Transform tr)
    {
        var emitter = PoolManager.Ins.Spawn<SoundEmitter>(K.NAME_SOUND_EMITTER);
        emitter.Follow(tr);
        AudioSource source = emitter.Source;
        if(source != null)
        {
            PlaySfx(source, so);
        }
    }

    // 새로운 BGM 재생 시작
    private static void PlayBgm(AudioSource source, string id)
    {
        // 중복 재생 방어
        if(_curBgmId == id && source.isPlaying)
        {
            return;
        }
        // SO 검증
        if(!TryGetSoundSO(id, out SoundSO so))
        {
            UDebug.Print($"ID와 일치하는 SoundSO 데이터가 존재하지 않습니다.", LogType.Assert);
            return;
        }
        // BGM 재생하기
        var volume = DataManager.Ins.Volume;
        source.clip = so.Clip;
        source.volume = so.Volume * volume.Bgm * volume.Master;
        source.Play();
        _curBgmId = id;
    }

    // 사운드 SO를 안전하게 가져오기
    private static bool TryGetSoundSO(string id, out SoundSO so)
    {
        so = DatabaseManager.Ins.Sound(id);
        return so != null;
    }
    #endregion
}
