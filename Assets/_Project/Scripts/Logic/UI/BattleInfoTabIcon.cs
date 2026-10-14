using UnityEngine;
using UnityEngine.UI;

/// <summary>내장 스프라이트를 표시하며, 빌드에서 UI를 동적으로 생성할 때는 같은 도형을 그립니다.</summary>
public class BattleInfoTabIcon : Image
{
    [SerializeField] private int shapeIndex;
    public int ShapeIndex
    {
        get => shapeIndex;
        set { shapeIndex = value; SetVerticesDirty(); }
    }

    protected override void OnPopulateMesh(VertexHelper mesh)
    {
        if (sprite != null || shapeIndex == 1)
        {
            base.OnPopulateMesh(mesh);
            return;
        }

        mesh.Clear();
        Rect rect = GetPixelAdjustedRect();
        Vector2 center = rect.center;
        float radius = Mathf.Min(rect.width, rect.height) * 0.5f;
        int sides = shapeIndex == 2 ? 3 : 32;
        mesh.AddVert(center, color, new Vector2(0.5f, 0.5f));
        for (int i = 0; i < sides; i++)
        {
            float angle = (-90f + i * 360f / sides) * Mathf.Deg2Rad;
            Vector2 direction = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
            mesh.AddVert(center + direction * radius, color, direction * 0.5f + Vector2.one * 0.5f);
        }
        for (int i = 0; i < sides; i++)
            mesh.AddTriangle(0, i + 1, (i + 1) % sides + 1);
    }
}
