using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 정적 데이터를 보관하는 매니저입니다.
/// </summary>
public class DatabaseManager : GlobalSingleton<DatabaseManager>
{
    #region ─────────────────────────▶ 내부 변수 ◀─────────────────────────
    private bool _isInitialized = false;

    // 테이블
    
    private SoundTableSO[] _soundTables;

    //Todo : private PokemonTableSO[] _pokemonTables;

    //private RecipeTableSO[] _recipeTables;
    // 프리펩
    private SoundEmitter _soundEmiiterPrefab;
    #endregion

    #region ─────────────────────────▶ 공개 멤버 ◀─────────────────────────
    public bool IsInit => _isInitialized;
    //Todo : public PokemonTableSO[] pokemonTables => _pokemonTables;
    //public RecipeTableSO[] RecipeTeables => _recipeTables;


    private void CallBack()
    {

    }
    private void Progress(float progress)
    {

    }
    
    /// <summary>
    /// 오디오의 정적 데이터를 반환합니다.
    /// </summary>
    /// <param name="id">사운드 ID</param>
    public SoundSO Sound(string id)
        => FindData<SoundTableSO, SoundSO>(_soundTables, id);


    /// <summary>
    /// 레시피 정적 데이터를 반환합니다.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    /// 

    //Todo : 
    //public PokemonSO ReturnPokemonSOById(string id)
    //    => FindData<PokemonTableSO, PokemonSO>(_pokemonTables, id);


    //public RecipeTableSO AllRecipe()
    //{
    //    return _recipeTables[0];
    //}

    /// <summary>
    /// 사운드 이미터 프리펩을 반환합니다.
    /// </summary>
    public SoundEmitter SoundPrefab() => GetSafePrefab(_soundEmiiterPrefab);
    #endregion

    #region ─────────────────────────▶ 내부 메서드 ◀─────────────────────────
    public override void Initialize()
    {
        if (_isInitialized) return;


        //_pokemonTables = LoadTables<PokemonTableSO, PokemonSO>();
        //SettingUnitDict<PokemonTableSO, PokemonSO>(_pokemonTables);

        // UnitSO를 상속받지 않는 SO 작성
        //_soundTables = LoadTables<SoundTableSO, SoundSO>();
        // 프리펩 로드
        //_soundEmiiterPrefab = LoadPrefab<SoundEmitter>(K.NAME_SOUND_EMITTER);
        // 완료
        _isInitialized = true;
        UDebug.Print("모든 테이블 로드가 완료되었습니다.");
    }

    // 프리펩을 컴포넌트로 반환합니다.
    private T LoadPrefab<T>(string prefabName) where T : Component
    {
        if (prefabName.IsEmpty())
        {
            UDebug.Print($"리소스 폴더에서 로드할 프리펩 이름이 비어있습니다.", LogType.Assert);
            return null;
        }
        T prefab = Resources.Load<T>($"{K.PREFAB_RESOURCE_PATH}/{prefabName}");
        if (prefab == null)
        {
            UDebug.Print($"{prefabName} 이름을 가진 프리펩을 Resources/{K.PREFAB_RESOURCE_PATH}에서 찾을 수 없습니다." +
                $"\n또는 {typeof(T).Name} 컴포넌트가 부착되지 않았습니다.", LogType.Assert);
            return null;
        }
        return prefab;
    }

    // 리소스 폴더에서 특정 타입의 모든 테이블 SO를 찾아 배열로 반환
    private TTable[] LoadTables<TTable, TData>()
        where TTable : TableSO<TData> where TData : BaseSO
    {
        TTable[] tables = Resources.LoadAll<TTable>(K.TABLE_RESOURCE_PATH);
        // 탐색 실패
        if (tables == null || tables.Length <= 0)
        {
            UDebug.Print($"경로({K.TABLE_RESOURCE_PATH})에서 테이블({typeof(TTable).Name})을 찾을 수 없습니다.", LogType.Warning);
        }
        // 탐색 성공
        else
        {
            for (int i = 0; i < tables.Length; ++i)
            {
                tables[i].Initialize();
            }
            UDebug.Print($"경로({K.TABLE_RESOURCE_PATH})에서 테이블({typeof(TTable).Name})을 찾았습니다.");
        }
        return tables;
    }
    // 테이블 배열을 받기 위한 진입점
    private void SettingUnitDict<TTable, TData>(TTable[] tables)
        where TTable : TableSO<TData> where TData : BaseSO
    {
        if (tables == null)
        {
            UDebug.Print($"unitDict 작성 도중 비어있는 테이블 배열({typeof(TTable).Name})을 받았습니다.", LogType.Assert);
            return;
        }
        // 테이블 순회
        for (int i = 0; i < tables.Length; i++)
        {
            SettingUnitDict<TTable, TData>(tables[i]);
        }
    }

    // 전역적인 데이터에 접근할 수 있도록 UnitDict 작성
    private void SettingUnitDict<TTable, TData>(TTable table)
        where TTable : TableSO<TData> where TData : BaseSO
    {
        // 방어 코드
        if (table == null)
        {
            UDebug.Print($"unitDict 작성 도중 비어있는 테이블({typeof(TTable).Name})을 받았습니다.", LogType.Assert);
            return;
        }
        // 테이블에서 SO가 담긴 원본 리스트를 가져오기
        IReadOnlyList<TData> list = table.ReadList();
        int length = list.Count;
        //for (int i = 0; i < length; ++i)
        //{
        //    // 유닛 변환 + 중복 아닐 경우 등록
        //    if (list[i] is not UnitSO unit)
        //    {
        //        UDebug.Print($"unitDict 작성 도중 {list[i].Id}({typeof(TData).Name})를 UnitSO로 변환하지 못했습니다.", LogType.Assert);
        //        continue;
        //    }
        //    if (!_unitDict.TryAdd(unit.Id, unit))
        //    {
        //        UDebug.Print($"unitDict 작성 도중 중복 등록이 감지되었습니다. {list[i].Id}({typeof(TData).Name})", LogType.Assert);
        //        continue;
        //    }
        //}
    }

    // Null 검사해서 프리펩 가져오기
    private T GetSafePrefab<T>(T prefab) where T : Component
    {
        if (prefab == null)
        {
            UDebug.Print($"가져올 {typeof(T).Name} 타입 프리펩이 존재하지 않습니다.");
            return null;
        }
        return prefab;
    }

    // 테이블에서 ID로 SO 인스턴스를 가져옵니다.
    private TData FindData<TTable, TData>(TTable[] tables, string id)
        where TTable : TableSO<TData>
        where TData : BaseSO
    {
        // 혹시나 방어 코드
        if (tables == null || tables.Length == 0)
        {
            UDebug.Print($"테이블을 찾을 수 없습니다. ({typeof(TTable).Name}, {typeof(TData).Name})", LogType.Assert);
            return null;
        }
        // 해당 테이블을 모두 순회하며 데이터 탐색
        for (int i = 0; i < tables.Length; i++)
        {
            TData data = tables[i].GetSO(id);
            // 탐색을 성공하여 SO 반환
            if (data != null)
            {
                return data;
            }
        }
        // 테이블에 ID가 존재하지 않음
        UDebug.Print($"테이블({typeof(TTable).Name})에서 SO 에셋을 찾을 수 없습니다.(ID = {id})", LogType.Assert);
        return null;
    }
    // UnitSO의 타입에 따라 어떤 데이터베이스에서 ItemSO를 반환해야 하는지 분류합니다.
    // 조금 무식한 방법이라도 넘어갑시다.
   

    #endregion
}
