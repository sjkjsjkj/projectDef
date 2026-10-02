using UnityEngine;

/// <summary>
/// 게임에 존재할 SO 클래스가 상속받아야하는 유닛 데이터입니다.
/// </summary>
public abstract class UnitSO : BaseSO
{
    #region ─────────────────────────▶ 인스펙터 ◀─────────────────────────
    [Header("기본 정보")]
    [SerializeField] protected string _name = "이름";
    [SerializeField] protected string _description = "설명";
    [SerializeField] protected ERarity _rarity = ERarity.Basic; // 아이템 희귀도
    [SerializeField] protected Sprite _image;
    [SerializeField] protected GameObject _prefab;
    #endregion

    #region ─────────────────────────▶ 공개 멤버 ◀─────────────────────────
    public string Name => _name;
    public string Description => _description;
    public ERarity Rarity => _rarity;
    public Sprite Image => _image;
    public GameObject Prefab => _prefab;

    // 정상 값을 가지는지 검사
    public override bool IsValid()
    {
        if (_name.IsEmpty()) return false;
        if (_description.IsEmpty()) return false;
        if (_rarity == ERarity.None) return false;
        //if (_image == null) return false;
        //if (_prefab == null) return false;
        return true;
    }
    #endregion

    #region ─────────────────────────▶ 메시지 함수 ◀─────────────────────────
    // 인스펙터 변수 유효성 검사
    protected override void OnValidate()
    {
        base.OnValidate();
        if (!IsValid())
        {
            UDebug.PrintOnce($"SO 인스턴스({this.name})의 값이 올바르지 않습니다. (ID = {_id}, Type = {this.GetType().Name})", LogType.Warning);
        }
    }
    #endregion
}
