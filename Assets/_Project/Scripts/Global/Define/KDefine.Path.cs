#pragma warning disable IDE1006

public static partial class K
{
    // SO를 수집하여 생성된 Csv 파일 경로
    public static readonly string AUTO_SO_EXPORT_PATH = "Assets/Export";
    // 부트 프리펩 리소스 경로
    public static readonly string BOOT_PREFAB_RESOURCE_PATH = "BootPrefab";
    public static readonly string BOOT_CANVAS_RESOURCE_PATH = "CanvasPrefab";
    // SO 테이블 리소스 경로
    public static readonly string TABLE_RESOURCE_PATH = "Table";
    public static readonly string PREFAB_RESOURCE_PATH = "Prefab";
    // 타일 리소스 경로
    public static readonly string TILE_RESOURCE_SHEET_PATH = "Tile/TileSheet - TileData";
    public static readonly string TILE_RESOURCE_MAIN_JSON_PATH = "Tile/MainLogicMap";
    public static readonly string TILE_RESOURCE_FOREST_JSON_PATH = "Tile/ForestLogicMap";
    public static readonly string TILE_RESOURCE_CAVE_JSON_PATH = "Tile/CaveLogicMap";
    // 타일 파일 경로
    public static readonly string TILE_BASE_PATH = "Assets/Farm RPG - Tiny Asset Pack - (All in One)/Farm/Tileset/Palette";
    public static readonly string TILE_EXPORT_PATH = "Assets/Export";
    // 타일 파일 이름
    public static readonly string TILE_CSV_EXPORT_NAME = "TileData.csv";
    public static readonly string TILE_JSON_EXPORT_NAME = "TileIdMap.json";
    public static readonly string TILE_MAP_EXPORT_NAME = "MapData.json";
    // 문자열 ID 스크립트 경로
    public static readonly string STRING_ID_EXPORT_PATH = "Assets/Scripts/Global/Define";

    // 저장 경로
    // AppData\LocalLow\<CompanyName>\<ProductName>
    //string path = $"{Application.persistentDataPath}/{PRESISTENT_OBJECT_PATH}/{class.UniqueId}.json";
    public static readonly string PRESISTENT_OBJECT_PATH = "Objects";
}
