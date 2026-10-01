using Unity.VisualScripting;
using UnityEngine;

/// <summary>
/// 본 프로젝트의 표준 모노비헤이비어입니다.
/// </summary>
public abstract class BaseMono : MonoBehaviour
{
    [HideInInspector][SerializeField] private string _uniqueId; // UUID
    [HideInInspector][SerializeField] private int _instanceId; // 유니티가 오브젝트에 부여하는 고유 ID

    public string UniqueId { get => _uniqueId; set => _uniqueId = value; }

    // 스크립트 로드 or 인스펙터 값 변경 시 호출
    protected virtual void OnValidate()
    {
#if UNITY_EDITOR
        // 프리펩 에셋일 경우
        if (UnityEditor.PrefabUtility.IsPartOfPrefabAsset(this))
        {
            _uniqueId = string.Empty;
            _instanceId = 0;
            return;
        }
#endif
        if (_uniqueId.IsEmpty() || _instanceId != GetInstanceID())
        {
            _instanceId = GetInstanceID();
            NewGuid();
        }
    }

    // 새로 생성된 오브젝트일 경우 Awake 단계에서 UUID 부여
    protected virtual void Awake()
    {
        if (_uniqueId.IsEmpty())
        {
            _instanceId = GetInstanceID();
            NewGuid();
        }
    }

    // 고유 UUID 부여
    private void NewGuid()
    {
        _uniqueId = System.Guid.NewGuid().ToString();
    }
}
