using System.Runtime.CompilerServices;

/// <summary>
/// 스트링 클래스의 확장 메서드를 제공합니다.
/// </summary>
public static class StringExtensions
{
    /// <summary>
    /// 문자열이 비어있거나 공백일 경우 True를 반환합니다.
    /// </summary>
    /// <param name="text">문자열</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsEmpty(this string text)
    {
        return string.IsNullOrWhiteSpace(text);
    }

    /// <summary>
    /// 문자열이 채워져있을 경우 True를 반환합니다.
    /// </summary>
    /// <param name="text">문자열</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool HasValue(this string text)
    {
        return !text.IsEmpty();
    }
}
