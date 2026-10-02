using UnityEngine;

/// <summary>
/// 클래스의 설계 의도입니다.
/// </summary>
public class EnemySpawner : BaseMono
{
    #region ─────────────────────────▶ 인스펙터 ◀─────────────────────────
    [Header("에너미")]
    [SerializeField] private EnemyPath enemyPath;
    [SerializeField] private EnemyMovement enemyPrefab;
    #endregion

    #region ─────────────────────────▶ 내부 변수 ◀─────────────────────────

    #endregion

    #region ─────────────────────────▶ 공개 멤버 ◀─────────────────────────
    public void SpawnEnemy()
    {
        if (enemyPath == null)
        {
            Debug.LogError("EnemyPath가 연결되지 않았습니다.");
            return;
        }

        if (enemyPrefab == null)
        {
            Debug.LogError("EnemyPrefab이 연결되지 않았습니다.");
            return;
        }

        EnemyMovement enemy = Instantiate(enemyPrefab);

        enemy.Init(enemyPath);
    }
    #endregion

    #region ─────────────────────────▶ 내부 메서드 ◀─────────────────────────

    #endregion

    #region ─────────────────────────▶ 메시지 함수 ◀─────────────────────────
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            SpawnEnemy();
        }
    }
    #endregion

    #region ─────────────────────────▶ 중첩 타입 ◀─────────────────────────

    #endregion
}
