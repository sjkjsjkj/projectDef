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
    /// <summary>기본 공격이 실제로 성공한 뒤, 이번 공격 간격을 전달합니다.</summary>
    public event System.Action<float> BasicAttackPerformed;
    /// <summary>공격 접수 시 목표 위치와 공격 간격을 전달합니다. 피해는 지연 실행됩니다.</summary>
    public event System.Action<Vector3, float> AttackStarted;

    public void SetCombatEnabled(bool enabled)
    {
        combatEnabled = enabled;
    }

    public bool TryUseSkill()
    {
        if (!combatEnabled || _unit == null || _unit.IsBeingDragged ||
            (GameManager.Ins != null && GameManager.Ins.IsCombatPaused))
            return false;

        if (_skill == null)
            return false;

        if (!_skill.TryActivate()) return false;
        AttackStarted?.Invoke(_skill.AimPosition, _skill.EffectiveCooldown);
        return true;
    }
    #endregion

    #region ─────────────────────────▶ 내부 메서드 ◀─────────────────────────
    private void CreateAbilities()
    {
        if (_unit.Data.BasicAttack != null)
        {
            _basicAttack =
                _unit.Data.BasicAttack.CreateInstance(new AbilityContext(_unit, isBasicAttack: true));
        }

        RefreshSkill();
    }

    private void RefreshSkill()
    {
        AbilityData data = _unit.CurrentSkill;
        if (data == null)
        {
            _skill = null;
            return;
        }

        if (_skill == null)
            _skill = data.CreateInstance(new AbilityContext(_unit));
        else if (_skill.Data != data)
            _skill.ChangeData(data);
    }

    private bool TryActivateAutomatically(AbilityInstance ability, ref float nextRetryTime)
    {
        if (ability == null || !ability.IsReady || nextRetryTime > 0f)
            return false;

        if (ability.TryActivate())
        {
            nextRetryTime = 0f;
            return true;
        }

        nextRetryTime = Mathf.Max(0.02f, targetRetryInterval);
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

    private void OnEnable()
    {
        if (_unit == null || _unit.Data == null) return;
        _unit.OnLevelChanged += RefreshSkill;
        // 비활성 상태에서 레벨이 바뀌었더라도 현재 단계로 동기화합니다.
        RefreshSkill();
    }

    private void OnDisable()
    {
        if (_unit != null)
            _unit.OnLevelChanged -= RefreshSkill;
    }

    private void Update()
    {
        if (GameManager.Ins != null && GameManager.Ins.IsCombatPaused) return;
        Tick(Time.deltaTime);
    }

    public void Tick(float deltaTime)
    {
        if (_unit == null || _unit.IsBeingDragged || !isActiveAndEnabled) return;
        float dt = Mathf.Max(0f, deltaTime);
        _nextBasicAttackRetryTime = Mathf.Max(0f, _nextBasicAttackRetryTime - dt);
        _nextSkillRetryTime = Mathf.Max(0f, _nextSkillRetryTime - dt);
        // 전투가 비활성화되어도 이미 시작된 쿨다운은 경과합니다.
        _basicAttack?.Tick(dt);
        _skill?.Tick(dt);

        if (!combatEnabled)
            return;

        if (autoUseSkill && TryActivateAutomatically(_skill, ref _nextSkillRetryTime))
        {
            AttackStarted?.Invoke(_skill.AimPosition, _skill.EffectiveCooldown);
            return;
        }

        if (TryActivateAutomatically(_basicAttack, ref _nextBasicAttackRetryTime))
        {
            AttackStarted?.Invoke(_basicAttack.AimPosition, _basicAttack.EffectiveCooldown);
            BasicAttackPerformed?.Invoke(_basicAttack.EffectiveCooldown);
        }
    }
    #endregion

    #region ─────────────────────────▶ 중첩 타입 ◀─────────────────────────

    #endregion
}
