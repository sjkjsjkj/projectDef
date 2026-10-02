using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 오브젝트 풀을 관리하는 매니저 (허브 역할)
/// </summary>
public class PoolManager : GlobalSingleton<PoolManager>
{
    #region ─────────────────────────▶ 내부 변수 ◀─────────────────────────
    private readonly Dictionary<string, IPoolFactory> _factoryDict = new();
    private bool _isInitialized = false;
    #endregion

    #region ─────────────────────────▶ 공개 멤버 ◀─────────────────────────
    /// <summary>
    /// 유닛을 가져옵니다.
    /// </summary>
    /// <typeparam name="T">풀링 인터페이스가 부착된 해당 유닛이 가지는 컴포넌트</typeparam>
    /// <param name="id">유닛 ID</param>
    public T Spawn<T>(string id) where T : BaseMono, IPoolable
    {
        // 해당 ID 유닛을 생성하는 팩토리 가져오기
        if (_factoryDict.TryGetValue(id, out IPoolFactory factory))
        {
            return factory.Spawn() as T;
        }
        // 해당 유닛은 풀링 생성을 지원하지 않습니다.
        else
        {
            UDebug.Print($"유닛 ({id})는 풀링 생성을 지원하지 않습니다.", LogType.Assert);
            return null;
        }
    }

    /// <summary>
    /// 유닛을 반환합니다.
    /// </summary>
    /// <typeparam name="T">풀링 인터페이스가 부착된 해당 유닛이 가지는 컴포넌트</typeparam>
    /// <param name="id">유닛 ID</param>
    public void Despawn<T>(string id, T instance) where T : BaseMono, IPoolable
    {
        // 해당 ID 유닛을 생성하는 팩토리 가져오기
        if (_factoryDict.TryGetValue(id, out IPoolFactory factory))
        {
            factory.Despawn(instance);
        }
        // 해당 유닛은 풀링 생성을 지원하지 않습니다.
        else
        {
            UDebug.Print($"유닛 ({id})에 해당하는 풀링 팩토리가 없으므로 인스턴스를 파괴합니다.", LogType.Assert);
            UObject.Destroy(instance.gameObject);
        }
    }

    public override void Initialize()
    {
        if (_isInitialized)
        {
            return;
        }
        var data = DatabaseManager.Ins;
        // 일반 프리펩
        CreateFactory(data.SoundPrefab(), K.NAME_SOUND_EMITTER);
        /*{
            string id = Id.World_Animal_Chicken;
            GameObject prefab = data.AnimalWorld(id).Prefab;
            if (prefab.TryGetComponent(out BaseMono component) && component is IPoolable)
            {
                CreateFactory(component, id);
            }
        }*/
        // ↑ 필요한 초기화 로직 / 부모 클래스에서 자동 실행
        _isInitialized = true;
    }
    #endregion

    #region ─────────────────────────▶ 내부 메서드 ◀─────────────────────────
    // 팩토리를 생성합니다.
    private void CreateFactory<T>(T prefab, string id) where T : BaseMono, IPoolable
    {
        // 방어 코드
        if (prefab == null || id.IsEmpty())
        {
            UDebug.Print($"팩토리 생성 함수에서 초기화되지 않은 매개변수를 받았습니다.", LogType.Assert);
        }
        // 팩토리 생성
        var factory = new PoolFactory<T>(prefab, 10);
        if (_factoryDict.TryAdd(id, factory))
        {
            return;
        }
        // 팩토리 중복 생성
        UDebug.Print($"이미 존재하는 팩토리를 중복 생성했습니다. (ID = {id}, Factory = {factory})", LogType.Assert);
    }
    #endregion
}
