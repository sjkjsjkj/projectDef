using UnityEngine;

/// <summary>배치 루트는 고정하고 그림만 목표 방향으로 짧게 밀었다가 복귀시킵니다.</summary>
[DisallowMultipleComponent]
[RequireComponent(typeof(UnitCombatController))]
public class UnitAttackAnimation : MonoBehaviour
{
    [SerializeField] private Transform visual;
    [SerializeField, Min(0.01f)] private float duration = 0.18f;
    [SerializeField, Min(0f)] private float moveDistance = 0.07f;
    [SerializeField, Range(0f, 10f)] private float tiltAngle = 3f;

    private UnitCombatController _combat;
    private Unit _unit;
    private Vector3 _restPosition;
    private Quaternion _restRotation;
    private Vector3 _localDirection;
    private float _elapsed, _playDuration;
    private bool _hasRestPose;

    public Transform Visual => visual;
    public float Duration => Mathf.Max(0.01f, duration);
    public bool IsPlaying { get; private set; }

    private void Awake()
    {
        _combat = GetComponent<UnitCombatController>();
        _unit = GetComponent<Unit>();
        CaptureRestPose();
    }

    private void OnEnable()
    {
        if (_combat == null) _combat = GetComponent<UnitCombatController>();
        if (_unit == null) _unit = GetComponent<Unit>();
        CaptureRestPose();
        _combat.AttackStarted += PlayAttack;
    }

    private void OnDisable()
    {
        if (_combat != null) _combat.AttackStarted -= PlayAttack;
        ResetPose();
    }

    private void CaptureRestPose()
    {
        if (visual == null || visual == transform || _hasRestPose) return;
        _restPosition = visual.localPosition;
        _restRotation = visual.localRotation;
        _hasRestPose = true;
    }

    public void PlayAttack(Vector3 targetPosition, float attackInterval)
    {
        if (!isActiveAndEnabled || visual == null || visual == transform || (_unit != null && _unit.IsBeingDragged)) return;
        CaptureRestPose();
        ResetPose();
        Vector3 worldDirection = targetPosition - transform.position;
        worldDirection.z = 0f;
        if (worldDirection.sqrMagnitude < 0.0001f) worldDirection = Vector3.right;
        _localDirection = visual.parent.InverseTransformVector(worldDirection.normalized).normalized;
        _playDuration = Mathf.Min(Duration, Mathf.Max(0.01f, attackInterval * 0.85f));
        _elapsed = 0f;
        IsPlaying = true;
    }

    public void ResetPose()
    {
        if (_hasRestPose && visual != null)
        {
            visual.localPosition = _restPosition;
            visual.localRotation = _restRotation;
        }
        IsPlaying = false;
    }

    public float EvaluatePulse(float normalizedTime)
    {
        float t = Mathf.Clamp01(normalizedTime);
        return t < 0.3f ? Mathf.SmoothStep(0f, 1f, t / 0.3f) : Mathf.SmoothStep(1f, 0f, (t - 0.3f) / 0.7f);
    }

    public Vector3 EvaluateOffset(float t, Vector3 localDirection) => localDirection * (moveDistance * EvaluatePulse(t));
    public float EvaluateAngle(float t, Vector3 localDirection) => -localDirection.x * tiltAngle * EvaluatePulse(t);

    private void Update()
    {
        if (GameManager.Ins != null && GameManager.Ins.IsCombatPaused) return;
        Tick(Time.deltaTime);
    }

    public void Tick(float deltaTime)
    {
        if (!IsPlaying || visual == null || (_unit != null && _unit.IsBeingDragged)) return;
        _elapsed += Mathf.Max(0f, deltaTime);
        if (_elapsed >= _playDuration) { ResetPose(); return; }
        float t = _elapsed / _playDuration;
        visual.localPosition = _restPosition + EvaluateOffset(t, _localDirection);
        visual.localRotation = _restRotation * Quaternion.Euler(0f, 0f, EvaluateAngle(t, _localDirection));
    }
}
