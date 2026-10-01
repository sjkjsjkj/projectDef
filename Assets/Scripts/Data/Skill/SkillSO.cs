using UnityEngine;

/// <summary>
/// 포켓몬의 기술 SO
/// </summary>
public enum ESkillType { None, Phicsic, Magic }
[CreateAssetMenu(fileName = "SkillSO_", menuName = "ScriptableObjects/SkillSO", order = 1)]
public class SkillSO : ScriptableObject
{
    #region ─────────────────────────▶ 인스펙터 ◀─────────────────────────
    [Header("기본 정보")]
    [SerializeField] protected RuntimeAnimatorController _anim;
    [SerializeField] protected string _name = "이름";
    [SerializeField] protected string _description = "설명";
    [SerializeField] protected int _powerPoint = 0;  // 사용 가능 횟수

    [SerializeField] protected int _accuracy = 0;   //명중률
    [SerializeField] protected EType _eleType = EType.None;
    [SerializeField] protected ESkillType _skillType = ESkillType.None;
    [SerializeField] protected int _priority;

    [SerializeField] protected SkillPostProgressingSO[] _postProgressEffects;
    [SerializeField] protected SkillPreProgressingSO[] _preProgressEffects;
    [SerializeField] protected SkillDamageSO[] _skillDamage;
    [SerializeField] protected SkillRankChangeSO[] _rankChangeSelf;
    [SerializeField] protected SkillRankChangeSO[] _rankChangeTarget;
    #endregion

    #region ─────────────────────────▶ 공개 멤버 ◀─────────────────────────
    //public Sprite Image => _image;
    public string Name => _name;
    public string Description => _description;
    public int PowerPoint => _powerPoint;

    public int Accuracy => _accuracy;
    public EType EleType => _eleType;
    public ESkillType SkillType => _skillType;

    public int Priority => _priority;
    public SkillPostProgressingSO[] PostProgressingEffects => _postProgressEffects;
    public SkillPreProgressingSO[] PreProgressingEffectgs => _preProgressEffects;
    public SkillDamageSO[] SkillDamages => _skillDamage;
    public SkillRankChangeSO[] RankChangeSelf => _rankChangeSelf;
    public SkillRankChangeSO[] RankChangeTarget => _rankChangeTarget;

    // 값 유효성 검사
    public virtual bool IsValid()
    {
        if (_name.IsEmpty()) return false;
        if (_description.IsEmpty()) return false;
        if (_powerPoint == 0) return false;
        if (_accuracy == 0) return false;
        return true;
    }
    #endregion

    #region ─────────────────────────▶ 메시지 함수 ◀─────────────────────────
    protected virtual void OnValidate()
    {
        if (!IsValid())
        {
            UDebug.PrintOnce($"SO 인스턴스({this.name})의 값이 올바르지 않습니다. (Name = {_name}, Type = {this.GetType().Name})", LogType.Warning);
        }
    }
    #endregion
}
