using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 초기화 순서를 제어합니다.
/// </summary>
public class BootManager : MonoBehaviour
{
    private Coroutine _co;
    public void StartBootSequence(GameObject root)
    {
        if(_co != null)
        {
            UDebug.Print("부트 시퀀스가 중복 호출되었습니다.", LogType.Assert);
            return;
        }
        _co = StartCoroutine(CoInitialize(root));
    }

    private IEnumerator CoInitialize(GameObject root)
    {
        UDebug.Print("▷ 매니저 생성을 시작합니다. ◁");
        // 매니저 생성 및 초기화
        ManagerSpawner(root);
        UDebug.Print("▷ 글로벌 프리펩 생성을 시작합니다. ◁");
        // 글로벌 오브젝트 생성 및 초기화
        GameObject prefabRoot = GlobalPrefabSpawner();
        var globalPrefab = prefabRoot.AddComponent<GlobalPrefabLoader>();
        globalPrefab.Initialize(prefabRoot.transform, K.BOOT_PREFAB_RESOURCE_PATH);
        GameObject canvasRoot = GlobalCanvasSpawner();
        var globalCanvas = canvasRoot.AddComponent<GlobalPrefabLoader>();
        globalCanvas.Initialize(canvasRoot.transform, K.BOOT_CANVAS_RESOURCE_PATH);
        // 완료
        yield return null;
        UDebug.Print("▷ 부트 시퀀스가 완료되었습니다. ◁");
        _co = null;
    }

    private GameObject GlobalPrefabSpawner()
    {
        GameObject prefabRoot = new GameObject(K.NAME_GLOBAL_PREFAB_ROOT);
        Object.DontDestroyOnLoad(prefabRoot);
        return prefabRoot;
    }

    private GameObject GlobalCanvasSpawner()
    {
        GameObject canvasRoot = new GameObject(K.NAME_GLOBAL_CANVAS_ROOT);
        Object.DontDestroyOnLoad(canvasRoot);
        Canvas canvas = UObject.AddComponent<Canvas>(canvasRoot);
        CanvasScaler canvasScaler = UObject.AddComponent<CanvasScaler>(canvasRoot);
        GraphicRaycaster graphicRaycaster = UObject.AddComponent<GraphicRaycaster>(canvasRoot);
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.pixelPerfect = false;
        canvas.sortingOrder = 0;
        canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        canvasScaler.referenceResolution = new Vector2(K.SCREEN_WIDTH, K.SCREEN_HEIGHT);
        canvasScaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        canvasScaler.matchWidthOrHeight = 0.5f;
        graphicRaycaster.ignoreReversedGraphics = true;
        graphicRaycaster.blockingObjects = GraphicRaycaster.BlockingObjects.None;
        graphicRaycaster.blockingMask = LayerMask.GetMask("Everything");
        return canvasRoot;
    }

    private void ManagerSpawner(GameObject root)
    {
        // 순서 의존성 없음 ↓
        var inputManager = UObject.AddComponent<InputManager>(root); // Input Action을 읽음
        inputManager.Initialize();
        var databaseManager = UObject.AddComponent<DatabaseManager>(root); // 리소스 폴더의 테이블 SO를 읽음
        databaseManager.Initialize();
        
        // DatabaseManager 이후 실행 ↓
        
        var dataManager = UObject.AddComponent<DataManager>(root);
        dataManager.Initialize();
        // Databasemanager, DataManager 이후 실행 ↓
        var gameManager = UObject.AddComponent<GameManager>(root);
        gameManager.Initialize();

        // DataManager 이후 실행 ↓
       
        // 다른 매니저들을 위해 마지막에 실행 ↓
        var frameManager = UObject.AddComponent<FrameManager>(root);
        frameManager.Initialize();
        // 검증
        if (root == null)
        {
            UDebug.Print($"글로벌 매니저 루트가 파괴되었습니다!", LogType.Assert);
            return;
        }
        gameManager.BootComplete();
    }
}
