using UnityEngine;

/// <summary>
/// 모든 UI의 베이스
/// </summary>
public class UI : BaseMono
{
    #region ─────────────────────────▶ 인스펙터 ◀─────────────────────────
    [Header("크기 및 위치")]
    [SerializeField] private float _width;
    [SerializeField] private float _height;
    [SerializeField] private Vector2 _pos;
    #endregion

    #region ─────────────────────────▶ 내부 변수 ◀─────────────────────────

    #endregion

    #region ─────────────────────────▶ 공개 멤버 ◀─────────────────────────
    public void SetInfo(Vector2 size, Vector2 pos)
    {
        SetResolution();
  
        _width = size.x;
        _height = size.y;
        _pos = pos;

        transform.position = _pos;
    
    }
    #endregion

    #region ─────────────────────────▶ 내부 메서드 ◀─────────────────────────
    private void SetResolution()
    {
        Screen.SetResolution(1200, 800, FullScreenMode.Windowed);
    }
    #endregion

    #region ─────────────────────────▶ 메시지 함수 ◀─────────────────────────
    private void Update()
    {
        
    }
    #endregion

    #region ─────────────────────────▶ 중첩 타입 ◀─────────────────────────

    #endregion
}
