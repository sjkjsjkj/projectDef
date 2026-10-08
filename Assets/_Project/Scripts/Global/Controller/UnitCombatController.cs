using UnityEngine;

/// <summary>
/// 유닛의 기본 공격과 스킬을 사용하게 하는 컨트롤러 클래스.
/// </summary>
[RequireComponent(typeof(Unit))]
[DisallowMultipleComponent]
public class UnitCombatController : BaseMono
{
    #region ─────────────────────────▶ 인스펙터 ◀─────────────────────────
    [Header("Combat")]
    [SerializeField] private bool combatEnabled = false;

    [SerializeField, Min(0.02f)] private float targetRetryInterval = 0.1f;

    [Header("Skill")]
    [SerializeField] private bool autoUseSkill = false;
    #endregion

    #region ─────────────────────────▶ 내부 변수 ◀─────────────────────────
    private Unit _unit;

    private AbilityInstance _basicAttack;
    private AbilityInstance _skill;

    // 실행 실패 시 재시도 간격입니다. Ability의 쿨다운과는 별개입니다.
    private float _nextBasicAttackRetryTime;
    private float _nextSkillRetryTime;
    #endregion

    #region ─────────────────────────▶ 공개 멤버 ◀─────────────────────────
    public void SetCombatEnabled(bool enabled)
    {
        combatEnabled = enabled;
    }

    public bool TryUseSkill()
    {
        if (!combatEnabled)
            return false;

        if (_skill == null)
            return false;

        return _skill.TryActivate();
    }
    #endregion

    #region ─────────────────────────▶ 내부 메서드 ◀─────────────────────────
    private void CreateAbilities()
    {
        if (_unit.Data.BasicAttack != null)
        {
            _basicAttack =
                _unit.Data.BasicAttack.CreateInstance(new AbilityContext(_unit));
        }

        if (_unit.Data.Skill != null)
        {
            _skill =
                _unit.Data.Skill.CreateInstance(new AbilityContext(_unit));
        }
    }

    private bool TryActivateAutomatically(AbilityInstance ability, ref float nextRetryTime)
    {
        if (ability == null || !ability.IsReady || Time.time < nextRetryTime)
            return false;

        if (ability.TryActivate())
        {
            nextRetryTime = 0f;
            return true;
        }

        nextRetryTime = Time.time + Mathf.Max(0.02f, targetRetryInterval);
        return false;
    }

    #endregion

    #region ─────────────────────────▶ 메시지 함수 ◀─────────────────────────
    protected override void Awake()
    {
        base.Awake();
        _unit = GetComponent<Unit>();

        if (_unit.Data == null)
        {
            enabled = false;
            return;
        }

        CreateAbilities();
    }

    private void Update()
    {
        // 전투가 비활성화되어도 이미 시작된 쿨다운은 경과합니다.
        _basicAttack?.Tick(Time.deltaTime);
        _skill?.Tick(Time.deltaTime);

        if (!combatEnabled)
            return;

        if (autoUseSkill && TryActivateAutomatically(_skill, ref _nextSkillRetryTime))
        {
            return;
        }

        TryActivateAutomatically(_basicAttack, ref _nextBasicAttackRetryTime);
    }
    #endregion

    #region ─────────────────────────▶ 중첩 타입 ◀─────────────────────────

    #endregion
}
