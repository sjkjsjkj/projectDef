using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using Cysharp.Threading.Tasks;
using System.Threading;

/// <summary>
/// 싱글톤 클래스의 설계 의도입니다.
/// </summary>
public class AddressableManager : Singleton<AddressableManager>
{
    #region ─────────────────────────▶ 인스펙터 ◀─────────────────────────
    //[Header("주제")]
    //[SerializeField] private Class _class;
    #endregion

    #region ─────────────────────────▶ 내부 변수 ◀─────────────────────────
    private bool _isInitialized = false;

    private AsyncOperationHandle<GameObject> _loadHandle;

    #endregion

    #region ─────────────────────────▶ 공개 멤버 ◀─────────────────────────
    //로드만
    public async UniTask<GameObject> Load(string address)
    {
        

        //UniTask 용 // 핸들러 제거
        GameObject loadGo = await Addressables.LoadAssetAsync<GameObject>(address).ToUniTask();

        //await Addressables.LoadAssetAsync<GameObject>(address) 이것 자체가 핸들러를 반환함.
        //await Addressables.LoadAssetAsync<GameObject>(address).ToUniTask는 핸들러.Result를 해준 셈.
        //await Addressables.LoadAssetAsync<GameObject>(address) > 끝날 때 까지 기다림.
        //await Addressables.LoadAssetAsync<GameObject>(address).ToUniTask() > 끝날 때 까지 기다리고 결과를 받음.


        return loadGo;
    }
    //생성까지
    public async UniTask<GameObject> Spawn(string address)
    {
        GameObject spawnGo = await Addressables.InstantiateAsync(address).ToUniTask();

        return spawnGo;
    }
    #endregion

    #region ─────────────────────────▶ 내부 메서드 ◀─────────────────────────
    public override void Initialize() {
        if (_isInitialized)
        {
            return;
        }

        // ↑ 필요한 초기화 로직 / 부모 클래스에서 자동 실행
        _isInitialized = true;
    }
    #endregion
}
