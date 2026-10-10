using UnityEngine;
using UnityEngine.Serialization;

/// <summary>
/// 각 유닛을의 기본 데이터들이 들어갈 SO
/// </summary>
[CreateAssetMenu(fileName = "UnitData_", menuName = "ScriptableObjects/Unit/UnitData", order = 2)]
public class UnitData : BaseSO
{
    #region ─────────────────────────▶ 인스펙터 ◀─────────────────────────
    [Header("기본 정보")]
    [SerializeField] protected Sprite image;
    [SerializeField] protected string unitName;
    [SerializeField] protected string description = "설명";
    [SerializeField, Range(1, 5)] private int grade = 1;
    [Tooltip("선택 사항. 비워 두면 객잔의 공통 유닛 프리팹을 사용합니다.")]
    [SerializeField] private Unit prefab;

    [Header("소속 및 병종")]
    [SerializeField] private string region;
    [SerializeField] private string region2;
    [SerializeField] private string weaponType;
    [SerializeField] private string weaponType2;

    [Header("유닛 스탯")]
    [SerializeField, Min(1f)] float maxHp = 100;
    [SerializeField, Min(1f)] protected float attack = 100;
    [SerializeField, Min(0f)] private float defense;
    [SerializeField, Min(0.5f)] protected float attackSpd = 1.0f;
    [SerializeField, Min(1f)] protected float range = 1;

    [Header("스킬")]
    [SerializeField] protected AbilityData basicAttack;
    [Tooltip("유닛 1~3레벨에서 사용하는 스킬입니다. 기존 Skill 참조를 보존합니다.")]
    [FormerlySerializedAs("skill"), SerializeField] private AbilityData skillLevel1;
    [Tooltip("유닛 4~6레벨에서 사용하는 스킬입니다. 비워 두면 이전 단계 스킬을 사용합니다.")]
    [SerializeField] private AbilityData skillLevel2;
    [Tooltip("유닛 7~10레벨에서 사용하는 스킬입니다. 비워 두면 이전 단계 스킬을 사용합니다.")]
    [SerializeField] private AbilityData skillLevel3;
    #endregion

    #region ─────────────────────────▶ 공개 멤버 ◀─────────────────────────
    public string UnitName => unitName;
    public Sprite Image => image;
    public string Description => description;
    public int Grade => Mathf.Clamp(grade, 1, 5);
    public int Price => Grade;
    public Unit Prefab => prefab;
    public string Region => region;
    // CSV의 복수 소속 및 주석 표기를 원문 그대로 보관합니다.
    public string Region2 => region2;
    public string WeaponType => weaponType;
    public string WeaponType2 => weaponType2;

    public float Attack => attack;
    public float Defense => defense;
    public float Range => range;
    public float MaxHp => maxHp;
    public float AttackSpd => attackSpd;
    public AbilityData BasicAttack => basicAttack; 
    public AbilityData Skill => skillLevel1;
    public AbilityData SkillLevel1 => skillLevel1;
    public AbilityData SkillLevel2 => skillLevel2;
    public AbilityData SkillLevel3 => skillLevel3;

    public AbilityData GetSkillForLevel(int level)
    {
        int skillLevel = UnitLevelProgression.GetSkillLevel(level);
        if (skillLevel >= 3 && skillLevel3 != null) return skillLevel3;
        if (skillLevel >= 2 && skillLevel2 != null) return skillLevel2;
        return skillLevel1;
    }
    
    // 값 유효성 검사
    public override bool IsValid()
    {
        base.IsValid();
        if (unitName.IsEmpty()) return false;
        if (description.IsEmpty()) return false;
        //if (image == null) return false;
        
        return true;
    }
    #endregion

    #region ─────────────────────────▶ 메시지 함수 ◀─────────────────────────
    protected override void OnValidate()
    {
        if (!IsValid())
        {
            UDebug.PrintOnce($"SO 인스턴스({this.name})의 값이 올바르지 않습니다. (ID = {id}, Type = {this.GetType().Name})", LogType.Warning);
        }
    }
    #endregion
}
