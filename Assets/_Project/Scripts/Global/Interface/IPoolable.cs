/// <summary>
/// 풀링되는 객체가 준수하는 인터페이스 → 초기화 메서드 강제
/// </summary>
public interface IPoolable
{
    // 대여할 때마다 비활성 상태에서 호출됩니다. Awake 이전 호출도 지원해야 합니다.
    void Setup();
}
