using System.Collections.Generic;

/// <summary>
/// 매 프레임 실행되는 로직을 총괄하는 매니저
/// </summary>
public class FrameManager : GlobalSingleton<FrameManager>
{
    // 배열 안에 리스트가 있는 구조
    private readonly List<IFrameable>[] _frames = new List<IFrameable>[K.MANAGER_PRIORITY_SIZE];
    private bool _isInitialized = false;

    #region ─────────────────────────▶ 공개 멤버 ◀─────────────────────────
    /// <summary>
    /// 인터페이스를 준수하는 스크립트를 프레임 매니저에 가입
    /// </summary>
    public void Register(IFrameable frame)
    {
        _frames[(int)frame.Priority].Add(frame);
    }

    /// <summary>
    /// 인터페이스를 준수하는 스크립트를 프레임 매니저에서 탈퇴
    /// </summary>
    public void Unregister(IFrameable frame)
    {
        var list = _frames[(int)frame.Priority];
        int index = list.IndexOf(frame); // 삭제해야 할 번호
        UArray.SwapLastAndRemove(list, index); // 스왑 앤 팝
    }
    #endregion

    #region ─────────────────────────▶ 내부 메서드 ◀─────────────────────────
    public override void Initialize()
    {
        if (_isInitialized)
        {
            return;
        }
        // 각 우선순위 안에 리스트 초기화
        for (int i = 0; i < K.MANAGER_PRIORITY_SIZE; ++i)
        {
            _frames[i] = new List<IFrameable>(K.MANAGER_FRAMEABLE_SIZE);
        }
        _isInitialized = true;
    }
    #endregion

    #region ─────────────────────────▶ 메시지 함수 ◀─────────────────────────
    private void Update()
    {
        // First → Late 순회
        for (int i = 0; i < _frames.Length; ++i)
        {
            var curList = _frames[i];
            int count = curList.Count - 1;
            // 내부 리스트 순회하며 실행
            for (int j = count; j >= 0; --j)
            {
                curList[j].ExecuteFrame();
            }
        }
    }
    #endregion
}
