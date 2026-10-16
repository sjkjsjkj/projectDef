using System;

/// <summary>한 대상에게 적용된 효과의 실행 상태. Unity 컴포넌트를 생성하지 않습니다.</summary>
public sealed class StatusEffectInstance
{
    private double _remainingTime;
    private double _untilTick;
    private double _movementStopTime;
    private int _revision;

    public StatusEffectData Data { get; private set; }
    public float RemainingTime => (float)_remainingTime;
    public bool IsActive { get; private set; }
    public bool IsMovementBlocked => IsActive && (Data.PreventsMovement || _movementStopTime > 0d);

    public StatusEffectInstance(StatusEffectData data)
    {
        if (data == null || !data.IsValid()) throw new ArgumentException("유효한 상태이상 정의가 필요합니다.", nameof(data));
        Data = data;
        _remainingTime = data.Duration;
        _untilTick = data.TickInterval;
        IsActive = true;
    }

    internal void Refresh(StatusEffectData data)
    {
        // 연속 타격이 독/마비의 다음 틱을 계속 뒤로 미루지 않도록 기존 주기를 보존합니다.
        _untilTick = data.UsesPeriodicTicks && Data.UsesPeriodicTicks
            ? Math.Min(_untilTick, data.TickInterval) : data.TickInterval;
        Data = data;
        _remainingTime = data.Duration;
        _revision++;
    }

    public void StopMovementFor(float seconds)
    {
        if (IsActive && seconds > 0f && !float.IsInfinity(seconds))
            _movementStopTime = Math.Max(_movementStopTime, seconds);
    }

    internal void Cancel()
    {
        IsActive = false;
        _remainingTime = 0d;
        _movementStopTime = 0d;
        _revision++;
    }

    public void Tick(float deltaTime, IStatusEffectTarget target)
    {
        if (!IsActive || target == null || !target.IsStatusEffectTargetAlive ||
            deltaTime <= 0f || float.IsNaN(deltaTime) || float.IsInfinity(deltaTime)) return;

        double remainingDelta = Math.Min(deltaTime, _remainingTime);
        int revision = _revision;
        while (remainingDelta > 0d && IsActive)
        {
            double step = Data.UsesPeriodicTicks ? Math.Min(remainingDelta, _untilTick) : remainingDelta;
            _movementStopTime = Math.Max(0d, _movementStopTime - step);
            _remainingTime = Math.Max(0d, _remainingTime - step);
            remainingDelta -= step;
            if (Data.UsesPeriodicTicks)
            {
                _untilTick -= step;
                if (_untilTick <= 0d)
                {
                    _untilTick = Data.TickInterval;
                    Data.OnTick(target, this);
                    // 피해 콜백에서 해제/재부여/풀 반환이 일어나면 이전 실행을 즉시 끝냅니다.
                    if (!IsActive || revision != _revision || !target.IsStatusEffectTargetAlive) return;
                }
            }
        }

        // 종료 시각과 일치하는 마지막 주기 피해까지 적용한 후 해제합니다.
        if (_remainingTime <= 0d) Cancel();
    }
}
