using UnityEngine;

/// <summary>이벤트 연결 확인용 기본 효과입니다. 다른 효과는 RoundEventEffect를 상속하여 추가합니다.</summary>
[CreateAssetMenu(fileName = "EventGold_", menuName = "ScriptableObjects/Round Event/Effects/Gold")]
public class RoundEventGoldEffect : RoundEventEffect
{
    [SerializeField, Min(1)] private int amount = 100;

    public override bool IsValid() => base.IsValid() && amount > 0;

    public override bool TryApply(RoundEventContext context, out string reason)
    {
        reason = null;
        if (!IsValid() || context?.Resources == null)
        { reason = "골드 보상 설정과 ResourceManager를 확인하세요."; return false; }
        context.Resources.GrantGold(amount);
        return true;
    }
}
