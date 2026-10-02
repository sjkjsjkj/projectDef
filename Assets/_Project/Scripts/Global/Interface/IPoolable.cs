/// <summary>
/// 풀링되는 객체가 준수하는 인터페이스 → 초기화 메서드 강제
/// </summary>
public interface IPoolable
{
    void Setup();
}
