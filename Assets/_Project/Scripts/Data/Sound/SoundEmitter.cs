using UnityEngine;

/// <summary>
/// 소리를 재생하고, 재생이 끝나는 스스로 반환되는 객체
/// </summary>
[RequireComponent(typeof(AudioSource))]
public class SoundEmitter : Frameable, IPoolable
{
    private AudioSource _source;
    private Transform _tr;
    
    #region ─────────────────────────▶ 공개 멤버 ◀─────────────────────────
    // 초기화
    public void Setup()
    {
        _tr = null;
        _source = UObject.AddComponent<AudioSource>(this.gameObject);
        _source.playOnAwake = false;
        _source.spatialBlend = 1f;
        _source.rolloffMode = AudioRolloffMode.Linear;
        _source.minDistance = 10f;
        _source.maxDistance = 20f;
    }

    /// <summary>
    /// 오디오 소스 가져오기
    /// </summary>
    public AudioSource Source => _source;

    /// <summary>
    /// 따라갈 오브젝트
    /// </summary>
    public void Follow(Transform tr) => _tr = tr;

    /// <summary>
    /// 위치할 좌표
    /// </summary>
    public void Follow(Vector3 pos) => transform.position = pos;

    // 프레임 매니저에게 호출당하는 순서
    public override EPriority Priority => EPriority.Last;

    // 프레임 매니저가 실행할 메서드
    public override void ExecuteFrame()
    {
        // 아직 재생중
        if (_source.isPlaying)
        {
            // 오브젝트 따라가기
            if(_tr != null)
            {
                transform.position = _tr.position;
            }
            return;
        }
        // 재생이 끝났으므로 풀에 스스로 반납
        PoolManager.Ins.Despawn(K.NAME_SOUND_EMITTER, this);
    }
    #endregion
}
