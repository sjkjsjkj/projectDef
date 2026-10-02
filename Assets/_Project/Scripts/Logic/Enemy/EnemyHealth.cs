using System;
using UnityEngine;

/// <summary>
/// 클래스의 설계 의도입니다.
/// </summary>
public class EnemyHealth : BaseMono
{
    #region ─────────────────────────▶ 인스펙터 ◀─────────────────────────
    [Header("체력")]
    [SerializeField] private int maxHealth = 100;
    #endregion

    #region ─────────────────────────▶ 내부 변수 ◀─────────────────────────
    private int _currentHealth;
    private bool _isDead;
    #endregion

    #region ─────────────────────────▶ 공개 멤버 ◀─────────────────────────
    public int CurrentHealth => _currentHealth;
    public int MaxHealth => maxHealth;
    public bool IsDead => _isDead;

    public event Action OnDead;

    public void TakeDamage(int damage)
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

        OnDead?.Invoke();
        Debug.Log($"{name} 사망");

        Destroy(gameObject);
    }
    #endregion

    #region ─────────────────────────▶ 메시지 함수 ◀─────────────────────────
    protected override void Awake()
    {
        _currentHealth = maxHealth;
    }
    #endregion

    #region ─────────────────────────▶ 중첩 타입 ◀─────────────────────────

    #endregion
}
