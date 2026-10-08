using UnityEngine;

/// <summary>
/// 유닛별 Ability의 실행 문맥과 쿨다운을 관리합니다.
/// </summary>
public class AbilityInstance
{
    #region ─────────────────────────▶ 내부 변수 ◀─────────────────────────
    private readonly AbilityContext _context;

    private float _remainingCooldown;
    #endregion

    #region ─────────────────────────▶ 공개 멤버 ◀─────────────────────────
    public AbilityData Data { get; }

    public float RemainingCooldown => _remainingCooldown;
    public bool IsReady => _remainingCooldown <= 0f;

    public AbilityInstance(AbilityData data, AbilityContext context)
    {
        if (data == null)
            throw new System.ArgumentNullException(nameof(data));
        if (context == null)
            throw new System.ArgumentNullException(nameof(context));

        Data = data;
        _context = context;
    }

    public void Tick(float deltaTime)
    {
        if (_remainingCooldown <= 0f)
            return;

        _remainingCooldown = Mathf.Max(0f, _remainingCooldown - Mathf.Max(0f, deltaTime));
    }

    public bool TryActivate()
    {
        if (!IsReady)
            return false;

        if (!Data.TryExecute(_context))
            return false;

        _remainingCooldown = Mathf.Max(0f, Data.GetCooldown(_context));

        return true;
    }
    #endregion

    #region ─────────────────────────▶ 내부 메서드 ◀─────────────────────────

    #endregion
}
