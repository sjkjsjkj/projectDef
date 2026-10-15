/// <summary>상태를 SO에 저장하지 않습니다. 실패 시 아무것도 변경하지 않고 false를 반환해야 합니다.</summary>
public abstract class RoundEventEffect : BaseSO
{
    public abstract bool TryApply(RoundEventContext context, out string reason);
}
