using UnityEngine;

/// <summary>
/// 리소스 폴더의 특정 경로에서 프리펩을 로드 및 전역 적용
/// </summary>
public class GlobalPrefabLoader : BaseMono
{
    private bool _isInitialized = false;

    #region ─────────────────────────▶ 공개 멤버 ◀─────────────────────────
    public void Initialize(Transform root, string path)
    {
        if (_isInitialized)
        {
            UDebug.Print("글로벌 프리펩 로더가 중복 호출되었습니다.", LogType.Assert);
            return;
        }
        LoadPrefabs(root, path);
        _isInitialized = true;
    }
    #endregion

    #region ─────────────────────────▶ 내부 메서드 ◀─────────────────────────
    // 모든 프리펩 로드
    private void LoadPrefabs(Transform root, string path)
    {
        GameObject[] prefabs = Resources.LoadAll<GameObject>(path);
        if (prefabs == null || prefabs.Length <= 0)
        {
            UDebug.Print($"경로(Resources/{path})에서 로드할 프리팹이 존재하지 않습니다.");
            return;
        }
        // 글로벌 프리펩을 모두 생성
        int length = prefabs.Length;
        for (int i = 0; i < length; i++)
        {
            SpawnPrefab(prefabs[i], root);
        }
    }

    /// <summary>
    /// 단일 프리팹을 스폰하고 글로벌 부모 하위로 정돈합니다.
    /// </summary>
    private void SpawnPrefab(GameObject prefab, Transform root)
    {
        if (prefab == null)
        {
            UDebug.Print($"리소스 폴더에서 로드했으나 프리펩이 비어있습니다.", LogType.Warning);
            return;
        }
        // 생성
        GameObject instance = UObject.Spawn(prefab, root, false);
        instance.name = prefab.name; // Clone 이름 제거
        // 재조정
        RectTransform rectTr = instance.GetComponent<RectTransform>();
        if (rectTr != null)
        {
            rectTr.localScale = Vector3.one;
            rectTr.offsetMin = Vector2.zero;
            rectTr.offsetMax = Vector2.zero;
        }
        UDebug.Print($"글로벌 프리펩({instance.name})을 생성했습니다.");
    }
    #endregion
}
