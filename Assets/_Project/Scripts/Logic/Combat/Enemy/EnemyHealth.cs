using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 클래스의 설계 의도입니다.
/// </summary>
public class EnemyHealth : BaseMono, IDamageable
{
    // 스포너 종류와 관계없이 활성 상태인 생존 적 전체를 집계합니다.
    private static readonly HashSet<EnemyHealth> ActiveEnemies = new HashSet<EnemyHealth>();
    public static int ActiveEnemyCount => ActiveEnemies.Count;
    public static event Action<int> OnActiveEnemyCountChanged;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetRegistry()
    {
        ActiveEnemies.Clear();
        OnActiveEnemyCountChanged = null;
    }

    private void OnEnable()
    {
        if (IsAlive && ActiveEnemies.Add(this))
            OnActiveEnemyCountChanged?.Invoke(ActiveEnemyCount);
    }

    private void OnDisable()
    {
        UnregisterEnemy();
    }

    private void UnregisterEnemy()
    {
        if (ActiveEnemies.Remove(this))
            OnActiveEnemyCountChanged?.Invoke(ActiveEnemyCount);
    }

    #region ─────────────────────────▶ 인스펙터 ◀─────────────────────────
    [Header("체력")]
    [SerializeField] private float maxHealth = 100;
    #endregion

    #region ─────────────────────────▶ 내부 변수 ◀─────────────────────────
    private float _currentHealth;
    private bool _isDead;
    #endregion

    #region ─────────────────────────▶ 공개 멤버 ◀─────────────────────────
    public float CurrentHealth => _currentHealth;
    public float MaxHealth => maxHealth;
    public bool IsDead => _isDead;

    public event Action OnDead;

    public bool IsAlive => _currentHealth > 0f;

    public Transform TargetTransform => transform;

    public void TakeDamage(float damage)
    {
        if (_isDead)
            return;

        if (damage <= 0)
            return;

        _currentHealth -= damage;
        _currentHealth = Mathf.Max(_currentHealth, 0);

        Debug.Log($"{name} 피격: {damage} / 남은 체력: {_currentHealth}");

        if (_currentHealth <= 0)
        {
            Die();
        }
    }
    #endregion

    #region ─────────────────────────▶ 내부 메서드 ◀─────────────────────────
    private void Die()
    {
        if (_isDead)
            return;

        _isDead = true;
        UnregisterEnemy();

        OnDead?.Invoke();
        Debug.Log($"{name} 사망");

        Destroy(gameObject);
    }
    #endregion

    #region ─────────────────────────▶ 메시지 함수 ◀─────────────────────────
    protected override void Awake()
    {
        base.Awake();
        _currentHealth = maxHealth;
    }
    #endregion

    #region ─────────────────────────▶ 중첩 타입 ◀─────────────────────────

    #endregion
}
