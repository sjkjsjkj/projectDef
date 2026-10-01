using UnityEngine;

/// <summary>
/// 게임 오브젝트를 다루는 유틸리티 클래스입니다.
/// </summary>
public static class UObject
{
    #region ─────────────────────────▶ 게임 오브젝트 ◀─────────────────────────
    public static void ResetRect(RectTransform rectTr)
    {
        if (rectTr != null)
        {
            rectTr.localScale = Vector3.one;
            rectTr.offsetMin = Vector2.zero;
            rectTr.offsetMax = Vector2.zero;
        }
    }

    /// <summary>
    /// 빈 게임 오브젝트를 생성합니다.
    /// </summary>
    /// <param name="name">이름</param>
    /// <param name="parent">부모 오브젝트</param>
    /// <returns></returns>
    public static GameObject Create(string name, Transform parent = null)
    {
        GameObject go = new GameObject(name);
        if (parent != null)
        {
            go.transform.SetParent(parent, false);
        }
        return go;
    }

    /// <summary>
    /// 프리팹을 생성합니다.
    /// </summary>
    /// <param name="prefab">프리펩 게임 오브젝트</param>
    /// <param name="parent">부모 오브젝트</param>
    /// <returns></returns>
    public static GameObject Spawn(GameObject prefab, Transform parent = null, bool useLocal = false)
    {
        if (prefab == null)
        {
            return null;
        }
        return UnityEngine.Object.Instantiate(prefab, parent, useLocal);
    }

    /// <summary>
    /// 프리팹을 생성합니다.
    /// </summary>
    /// <param name="prefab">프리펩 게임 오브젝트</param>
    /// <param name="position">좌표</param>
    /// <param name="rotation">회전</param>
    /// <param name="parent">부모 오브젝트</param>
    /// <returns></returns>
    public static GameObject Spawn(GameObject prefab, Vector3 position, Quaternion rotation, Transform parent = null)
    {
        if (prefab == null)
        {
            return null;
        }
        return UnityEngine.Object.Instantiate(prefab, position, rotation, parent);
    }

    /// <summary>
    /// 프리팹을 생성합니다.
    /// </summary>
    /// <param name="prefab">프리펩 게임 오브젝트</param>
    /// <param name="position">좌표</param>
    /// <param name="rotation">회전</param>
    /// <param name="parent">부모 오브젝트</param>
    /// <returns></returns>
    public static T Spawn<T>
        (T prefab, Vector3 position, Quaternion rotation, Transform parent = null)
        where T : UnityEngine.Object
    {
        if (prefab == null)
        {
            return null;
        }
        return UnityEngine.Object.Instantiate(prefab, position, rotation, parent);
    }

    /// <summary>
    /// 게임 오브젝트를 파괴합니다.
    /// </summary>
    /// <param name="go">게임 오브젝트</param>
    /// <param name="delay">시간이 지난 후 파괴(초)</param>
    public static void Destroy(GameObject go, float delay = 0f)
    {
        if (go != null)
        {
            if (go.name.Contains("Inventory"))
            {
                UDebug.Print($"너가 범인이야?", LogType.Assert);
                return;
            }
            UnityEngine.Object.Destroy(go, delay);
        }
    }

    /// <summary>
    /// 프리팹을 파괴합니다.
    /// </summary>
    /// <param name="prefab">프리팹</param>
    /// <param name="delay">시간이 지난 후 파괴(초)</param>
    public static void Destroy<T>(T prefab, float delay = 0f) where T : Component
    {
        if (prefab != null)
        {
            UObject.Destroy(prefab.gameObject, delay);
        }
    }

    /// <summary>
    /// 특정 트랜스폼의 모든 자식을 파괴합니다.
    /// </summary>
    /// <param name="parent">트랜스폼</param>
    public static void DestroyChildren(Transform parent)
    {
        if (parent == null)
        {
            return;
        }
        int length = parent.childCount;
        for (int i = length - 1; i >= 0; --i)
        {
            UObject.Destroy(parent.GetChild(i).gameObject);
        }
    }

    /// <summary>
    /// 게임 오브젝트를 활성화 또는 비활성화합니다.
    /// </summary>
    /// <param name="go">게임 오브젝트</param>
    /// <param name="isActive">활성화 여부</param>
    public static void SetActive(GameObject go, bool isActive)
    {
        if (go != null && go.activeSelf != isActive)
        {
            go.SetActive(isActive);
        }
    }

    /// <summary>
    /// 트랜스폼의 좌표, 회전, 크기를 초기화합니다.
    /// </summary>
    /// <param name="tr">트랜스폼</param>
    public static void ResetTransform(Transform tr)
    {
        if (tr == null)
        {
            return;
        }
        tr.localPosition = Vector3.zero;
        tr.localRotation = Quaternion.identity;
        tr.localScale = Vector3.one;
    }
    #endregion

    #region ─────────────────────────▶ 컴포넌트 ◀─────────────────────────
    /// <summary>
    /// 컴포넌트를 부착하며 중복으로 붙이지 않습니다.
    /// </summary>
    /// <typeparam name="T">컴포넌트</typeparam>
    /// <param name="go">게임 오브젝트</param>
    /// <returns>컴포넌트</returns>
    public static T AddComponent<T>(GameObject go) where T : Component
    {
        if (go == null)
        {
            return null;
        }
        if (!go.TryGetComponent(out T component))
        {
            component = go.AddComponent<T>();
        }
        return component;
    }

    /// <summary>
    /// 자신 또는 자식에게서 컴포넌트를 가져옵니다.
    /// </summary>
    /// <typeparam name="T">컴포넌트</typeparam>
    /// <param name="go">게임 오브젝트</param>
    /// <returns>컴포넌트</returns>
    public static T GetComponent<T>(GameObject go) where T : Component
    {
        if (go == null)
        {
            return null;
        }
        return go.GetComponentInChildren<T>();
    }

    /// <summary>
    /// 씬 전체에서 특정 게임 오브젝트를 찾습니다.
    /// </summary>
    /// <param name="name">게임 오브젝트</param>
    /// <returns></returns>
    public static GameObject Find(string name)
    {
        return GameObject.Find(name);
    }

    /// <summary>
    /// 씬에서 특정 게임 오브젝트의 특정 컴포넌트를 찾습니다.
    /// </summary>
    /// <typeparam name="T">컴포넌트</typeparam>
    /// <param name="name">게임 오브젝트</param>
    /// <returns>컴포넌트</returns>
    public static T FindComponent<T>(string name) where T : Component
    {
        GameObject go = Find(name);
        if(go != null)
        {
            return go.GetComponentInChildren<T>();
        }
        return null;
    }

    /// <summary>
    /// 게임오브젝트에서 특정 컴포넌트를 제거합니다.
    /// </summary>
    /// <typeparam name="T">컴포넌트</typeparam>
    /// <param name="go">게임 오브젝트</param>
    public static void RemoveComponent<T>(GameObject go) where T : Component
    {
        if (go != null && go.TryGetComponent(out T component))
        {
            UObject.Destroy(component);
        }
    }
    #endregion
}
