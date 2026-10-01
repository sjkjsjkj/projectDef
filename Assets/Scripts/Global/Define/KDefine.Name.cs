#pragma warning disable IDE1006

public static partial class K
{
    // 카메라 이름 규칙 → 이름으로 탐색, 자동 세팅
    public static readonly string NAME_UI_CAMERA = "UI Camera";
    public static readonly string NAME_MAIN_CAMERA = "Main Camera";

    // 사운드 오브젝트 이름
    public static readonly string NAME_BGM_OBJECT = "Bgm_Source_Global";

    // 루트 오브젝트 이름
    public static readonly string NAME_UI_ROOT = "Canvas";
    public static readonly string NAME_OBJECT_ROOT = "@ObjectRoot";
    public static readonly string NAME_ENABLE_OBJECT_ROOT = "@EnableObjectRoot";
    public static readonly string NAME_DISABLE_OBJECT_ROOT = "@DisableObjectRoot";
    public static readonly string NAME_GLOBAL_MANAGER_ROOT = "@GlobalManager";
    public static readonly string NAME_GLOBAL_PREFAB_ROOT = "@GlobalPrefab";
    public static readonly string NAME_GLOBAL_CANVAS_ROOT = "@GlobalCanvas";

    // 테이블 이름
    public static readonly string NAME_TABLE_SOUND_BGM = "SoundTableSO_BGM";
    public static readonly string NAME_TABLE_SOUND_SFX = "SoundTableSO_SFX";
    public static readonly string NAME_TABLE_ANIMAL = "AnimalTableSO";

    // 사운드 이미터 인스턴스
    public static readonly string NAME_SOUND_EMITTER = "SoundEmitterInstance";

    // 아이템 수집 필수 클래스
    public static readonly string NAME_ITEM_COLLECT_ADMIN = "Item_Collection_Coordinator";

    // 플레이어 오브젝트 이름
    public static readonly string NAME_PLAYER = "Player";
}
