using UnityEngine;

/// <summary>
/// 화면 설정을 담는 데이터 클래스입니다.
/// </summary>
[System.Serializable]
public class ScreenData
{
    #region ─────────────────────────▶ 내부 변수 ◀─────────────────────────
    [SerializeField] private int _width = K.SCREEN_WIDTH;
    [SerializeField] private int _height = K.SCREEN_HEIGHT;
    [SerializeField] private EScreenMode _mode = EScreenMode.FullScreen;
    #endregion

    #region ─────────────────────────▶ 프로퍼티 ◀─────────────────────────
    public int Width
    {
        get => _width;
        // 창 크기가 너무 작아지는 것을 방지하기 위함
        set => _width = Mathf.Max(800, value);
    }
    public int Height
    {
        get => _height;
        // 창 크기가 너무 작아지는 것을 방지하기 위함
        set => _height = Mathf.Max(600, value);
    }
    public EScreenMode Mode
    {
        get => _mode;
        set => _mode = value;
    }
    #endregion
}
