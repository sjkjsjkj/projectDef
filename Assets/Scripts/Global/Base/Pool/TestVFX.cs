/// <summary>
/// 테스트용 객체의 컴포넌트
/// </summary>
public class TestVFX : BaseMono, IPoolable
{
    #region ─────────────────────────▶ 내부 메서드 ◀─────────────────────────
    public void Setup()
    {
        UDebug.Print("TestVFX 객체 초기화를 수행했습니다.");
    }
    #endregion
}
