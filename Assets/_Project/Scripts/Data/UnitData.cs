using UnityEngine;

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

    [Header("유닛 스탯")]
    [SerializeField, Min(1f)] float maxHp = 100;
    [SerializeField, Min(1f)] protected float attack = 100;
    [SerializeField, Min(0.5f)] protected float attackSpd = 1.0f;
    [SerializeField, Min(1f)] protected float range = 1;

    [Header("스킬")]
    [SerializeField] protected AbilityData basicAttack;
    [SerializeField] protected AbilityData skill;
    #endregion

    #region ─────────────────────────▶ 공개 멤버 ◀─────────────────────────
    public string UnitName => unitName;
    public Sprite Image => image;
    public string Description => description;

    public float Attack => attack;
    public float Range => range;
    public float MaxHp => maxHp;
    public float AttackSpd => attackSpd;
    public AbilityData BasicAttack => basicAttack; 
    public AbilityData Skill => skill;
    
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
