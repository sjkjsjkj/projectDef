using UnityEngine;

/// <summary>
/// 유닛 객체에게 부착될 클래스
/// </summary>
[DefaultExecutionOrder(-100)] [DisallowMultipleComponent]
public class Unit : CharacterBaseMono, IDraggable
{
    #region ─────────────────────────▶ 인스펙터 ◀─────────────────────────
    [Header("테스트용 외부 인스펙터 노출 멤버")]
    [SerializeField] private UnitData unitData;
    #endregion

    #region ─────────────────────────▶ 내부 변수 ◀─────────────────────────
    private UnitStats _stats;
    private GlobalUnitStatManager _statManager;
    private int _level = 1;
    #endregion

    #region ─────────────────────────▶ 공개 멤버 ◀─────────────────────────
    public UnitData Data => unitData;
    public int Level => _level;
    public bool IsMaxLevel => Level >= UnitLevelProgression.MaxLevel;
    public int SkillLevel => UnitLevelProgression.GetSkillLevel(Level);
    public AbilityData CurrentSkill => unitData != null ? unitData.GetSkillForLevel(Level) : null;
    public event System.Action OnLevelChanged;
    public Transform Origin => transform;
    public IDropTarget CurrentTarget { get; set; }

    /// <summary>비활성 상태로 생성한 유닛에 Awake 이전에 테이블 데이터를 주입합니다.</summary>
    public void InitializeForSpawn(UnitData data)
    {
        if (gameObject.activeInHierarchy || data == null || CurrentTarget != null)
            throw new System.InvalidOperationException("유닛 데이터는 배치 전 비활성 상태에서 지정해야 합니다.");
        unitData = data;
        _level = 1;
        _stats = new UnitStats(data);
        RegisterStats();
    }

    public bool TryLevelUp()
    {
        if (unitData == null || IsMaxLevel)
            return false;

        UnitStats stats = Stat;
        _level++;
        stats.ApplyLevel(unitData, Level);
        OnLevelChanged?.Invoke();
        return true;
    }

    public UnitStats Stat
    {
        get
        {
            if (_stats == null)
            {
                _stats = new UnitStats(unitData, Level);
                RegisterStats();
            }
            else if (_statManager == null && Application.isPlaying)
                RegisterStats();

            return _stats;
        }
    }
    #endregion

    #region ─────────────────────────▶ 내부 메서드 ◀─────────────────────────
    private void RegisterStats()
    {
        if (!Application.isPlaying) return;
        if (_statManager == null)
            _statManager = GlobalUnitStatManager.Ins;
        if (_statManager != null)
            _statManager.Register(this, _stats);
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        if (unitData == null)
            return;

        float range = Application.isPlaying && Stat != null
            ? Stat.AttackRange
            : unitData.Range;

        UnityEditor.Handles.color = Color.yellow;

        UnityEditor.Handles.DrawWireDisc(
            transform.position,
            Vector3.forward,
            range
        );
    }
#endif
    #endregion

    #region ─────────────────────────▶ 메시지 함수 ◀─────────────────────────
    protected override void Awake()
    {
        if (unitData == null)
        {
            Debug.LogError(
                $"UnitData is null : {gameObject.name}",
                this);

            enabled = false;
            return;
        }

        // 비활성 생성/레벨업 중 먼저 만들어진 스탯과 문맥을 보존합니다.
        if (_stats == null)
            _stats = new UnitStats(unitData, Level);
        RegisterStats();
    }

    private void OnEnable()
    {
        if (unitData != null && _stats != null) RegisterStats();
    }

    private void OnDestroy()
    {
        // 종료 중 새 싱글톤을 만들지 않도록 보관한 참조만 사용합니다.
        if (_statManager != null) _statManager.Unregister(this);
    }
    #endregion

    #region ─────────────────────────▶ 중첩 타입 ◀─────────────────────────

    #endregion
}
