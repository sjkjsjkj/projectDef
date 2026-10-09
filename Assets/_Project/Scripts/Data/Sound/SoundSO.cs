using UnityEngine;

/// <summary>
/// 사운드를 올려두는 SO입니다.
/// </summary>
[CreateAssetMenu(fileName = "SoundSO_", menuName = "ScriptableObjects/Sound", order = 4)]
public class SoundSO : BaseSO
{
    #region ─────────────────────────▶ 인스펙터 ◀─────────────────────────
    [Header("기본 정보")]
    [SerializeField] protected AudioClip _clip;
    [SerializeField, Range(0f, 1f)] protected float _volume = 0.5f;
    #endregion

    #region ─────────────────────────▶ 공개 멤버 ◀─────────────────────────
    public AudioClip Clip => _clip;
    public float Volume => _volume;

    // 값 유효성 검사
    public override bool IsValid()
    {
        if (!base.IsValid()) return false;
        if (type != EResourceType.Audio) return false;
        if (_clip == null) return false;
        if (_volume <= 0f) return false;
        return true;
    }

    /// <summary>
    /// 해당 클립의 최종 볼륨을 계산해서 가져옵니다.
    /// </summary>
    /// <param name="userVolume">사용자 볼륨 설정</param>
    public float CalcVolume(float userVolume)
    {
        userVolume = Mathf.Clamp01(userVolume);
        return Mathf.Min(_volume * userVolume, 1f);
    }

    /// <summary>
    /// 에디터 스크립트에서 SoundSO를 생성할 때 사용할 함수
    /// </summary>
    public void InitSO(string name, AudioClip clip, float volume = 0.5f)
    {
        type = EResourceType.Audio;
        id = name;
        _clip = clip;
        _volume = volume;
    }
    #endregion

    #region ─────────────────────────▶ 내부 메서드 ◀─────────────────────────
    private void AutomaticallyId()
    {
        // 클립 없거나 이름 이미 설정했으면 건드리지 않기
        /*if (_clip == null || _id.HasValue())
        {
            return;
        }*/
        // 클립 이름 가져와서 Id에 넣기
        id = _clip.name;
    }
    #endregion

    #region ─────────────────────────▶ 메시지 함수 ◀─────────────────────────
    protected override void OnValidate()
    {
        AutomaticallyId();
        type = EResourceType.Audio;
        if (!IsValid())
        {
            UDebug.PrintOnce($"SO 인스턴스({this.name})의 값이 올바르지 않습니다. (ID = {id}, Type = {this.GetType().Name})", LogType.Warning);
        }
    }
    #endregion
}
