using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// SO 클래스의 설계 의도입니다.
/// </summary>
[CreateAssetMenu(fileName = "AbilityDataSO_", menuName = "ScriptableObjects/Combat/AbilityDataSO", order = 2)]
public class AbilityData : BaseSO
{
    #region ─────────────────────────▶ 인스펙터 ◀─────────────────────────
    [Header("스킬 정보")]
    [SerializeField] private string skillName;
    [SerializeField] private string skillDescription;
    [SerializeField] private Sprite image;

    [SerializeField] private float range = 3f;
    [SerializeField] private float cooldown = 1f;
    [SerializeField] private LayerMask targetLayer;

    [SerializeField] private TargetSelector targetSelector;

    [SerializeField] private List<SkillCondition> useConditions;
    [SerializeField] private List<SkillCondition> activationConditions;
    [SerializeField] private List<SkillEffect> effects;
    #endregion

    #region ─────────────────────────▶ 공개 멤버 ◀─────────────────────────
    public float Range => range;
    public float Cooldown => cooldown;
    public LayerMask TargetLayer => targetLayer;

    public TargetSelector TargetSelector => targetSelector;
    public IReadOnlyList<SkillCondition> UseConditions => useConditions;
    public IReadOnlyList<SkillCondition> ActivationConditions => activationConditions;
    public IReadOnlyList<SkillEffect> Effects => effects;
    // 값 유효성 검사
    public override bool IsValid()
    {
        base.IsValid();
        if (id.IsEmpty()) return false;
        if (skillName.IsEmpty()) return false;
        if (skillDescription.IsEmpty()) return false;
        if (image == null) return false;

        return true;
    }
    #endregion

    #region ─────────────────────────▶ 메시지 함수 ◀─────────────────────────
    protected override void OnValidate()
    {
        base.OnValidate();
        if (!IsValid())
        {
            UDebug.PrintOnce($"SO 인스턴스({this.name})의 값이 올바르지 않습니다. (ID = {id}, Type = {this.GetType().Name})", LogType.Warning);
        }
    }
    #endregion
}
