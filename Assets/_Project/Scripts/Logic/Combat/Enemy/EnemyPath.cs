using UnityEngine;

/// <summary>
/// 클래스의 설계 의도입니다.
/// </summary>
public class EnemyPath : BaseMono
{
    #region ─────────────────────────▶ 인스펙터 ◀─────────────────────────
    [Header("웨이포인트")]
    [SerializeField] private float waypointRadius = 0.15f;
    [SerializeField] private float directionSize = 0.25f;
    #endregion

    #region ─────────────────────────▶ 내부 변수 ◀─────────────────────────

    #endregion

    #region ─────────────────────────▶ 공개 멤버 ◀─────────────────────────
    public int WaypointCount => transform.childCount;

    public Vector3 GetWaypoint(int index)
    {
        return transform.GetChild(index).position;
    }

    #endregion

    #region ─────────────────────────▶ 내부 메서드 ◀─────────────────────────
    private void OnDrawGizmos()
    {
        int count = transform.childCount;

        if (count == 0)
            return;

        Gizmos.color = Color.yellow;

        for (int i = 0; i < count; i++)
        {
            Transform current = transform.GetChild(i);

            Gizmos.DrawSphere(current.position, waypointRadius);

            if (i >= count - 1)
                continue;

            Transform next = transform.GetChild(i + 1);

            Gizmos.DrawLine(current.position, next.position);

            DrawDirection(current.position, next.position);
        }
    }

    private void DrawDirection(Vector3 from, Vector3 to)
    {
        Vector3 direction = (to - from).normalized;

        Vector3 center = Vector3.Lerp(from, to, 0.5f);

        Vector3 left =
            Quaternion.Euler(0f, 0f, 135f) * direction;

        Vector3 right =
            Quaternion.Euler(0f, 0f, -135f) * direction;

        Gizmos.DrawLine(
            center,
            center + left * directionSize
        );

        Gizmos.DrawLine(
            center,
            center + right * directionSize
        );
    }
    #endregion

    #region ─────────────────────────▶ 메시지 함수 ◀─────────────────────────

    #endregion

    #region ─────────────────────────▶ 중첩 타입 ◀─────────────────────────

    #endregion
}
