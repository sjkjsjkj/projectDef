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
    #endregion

    #region ─────────────────────────▶ 공개 멤버 ◀─────────────────────────
    public UnitData Data => unitData;
    public Transform Origin => transform;
    public IDropTarget CurrentTarget { get; set; }

    public UnitStats Stat
    {
        get
        {
            if (_stats == null)
                _stats = new UnitStats(unitData);

            return _stats;
        }
    }
    #endregion

    #region ─────────────────────────▶ 내부 메서드 ◀─────────────────────────
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

        _stats = new UnitStats(unitData);
    }
    #endregion

    #region ─────────────────────────▶ 중첩 타입 ◀─────────────────────────

    #endregion
}
