using System;
using UnityEngine;

/// <summary>
/// 풀을 사용하지 않는 순수 제네릭 팩토리입니다.
/// </summary>
public class BasicFactory<TObject, TData>
    where TObject : BaseMono where TData : UnitSO
{
    // 외부에서 주입받는 SO
    private readonly Func<string, TData> _dataExplorer;

    /// <summary>
    /// 팩토리 생성자
    /// </summary>
    /// <param name="dataExplorer">데이터베이스 매니저의 함수</param>
    public BasicFactory(Func<string, TData> dataExplorer)
    {
        _dataExplorer = dataExplorer;
    }

    #region ─────────────────────────▶ 내부 메서드 ◀─────────────────────────
    public GameObject Spawn(string id)
    {
        // 주입받은 함수로 SO 가져오기
        TData so = _dataExplorer?.Invoke(id);
        // 존재하지 않는 ID
        if (so == null)
        {
            UDebug.Print($"[BasicFactory] ID({id})에 해당하는 데이터를 찾을 수 없어 스폰을 취소합니다.", LogType.Assert);
            return null;
        }
        return Create(so);
    }

    // 프리펩을 안전하게 생성 및 초기화
    private GameObject Create(TData so)
    {
        // 인스턴스 생성
        GameObject go = UnityEngine.Object.Instantiate(so.Prefab);
        TObject comp = go.GetComponent<TObject>();
        // 혹시나 null 검사
        if (comp == null)
        {
            UDebug.Print($"프리팹({so.Prefab.name})에 컴포넌트({typeof(TObject).Name})가 존재하지 않습니다.", LogType.Assert);
        }
        // 초기화 및 반환
        //comp.SetInfo(so);
        return go;
    }
    #endregion
}
