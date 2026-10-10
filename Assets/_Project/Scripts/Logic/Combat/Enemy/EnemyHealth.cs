using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 적의 체력과 사망을 관리하고, 사망 시 보상 목록을 자원 관리자에게 전달합니다.
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
    private ResourceManager _resources;
    private List<Bounty> _bounties = new List<Bounty>();
    #endregion

    #region ─────────────────────────▶ 공개 멤버 ◀─────────────────────────
    public float CurrentHealth => _currentHealth;
    public float MaxHealth => maxHealth;
    public bool IsDead => _isDead;

    public event Action OnDead;

    public bool IsAlive => _currentHealth > 0f;

    public Transform TargetTransform => transform;

    /// <summary>생성 시 호출합니다. 웨이브 원본을 복사해 이후 웨이브 변경의 영향을 받지 않습니다.</summary>
    public void InitializeBounties(ResourceManager resources, IReadOnlyList<Bounty> bounties)
    {
        if (_isDead) throw new InvalidOperationException("사망한 적의 보상을 초기화할 수 없습니다.");
        if (bounties != null && bounties.Count > 0 && resources == null)
            throw new ArgumentNullException(nameof(resources));

        var copies = new List<Bounty>();
        if (bounties != null)
        {
            foreach (Bounty bounty in bounties)
            {
                if (bounty == null || !bounty.IsValid)
                    throw new ArgumentException("유효한 보상 목록이 필요합니다.", nameof(bounties));
                copies.Add(bounty.Copy());
            }
        }
        _resources = resources;
        _bounties = copies;
    }

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
        // 제거/비활성화 경로에서는 지급하지 않습니다. 사망 이벤트 전에 보상을 확정합니다.
        if (_resources != null)
            _resources.GrountEnemyBounty(_bounties);
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
