using System;
using System.IO;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.U2D.Sprites;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Object = UnityEngine.Object;

/// <summary>복사된 전투정보 PNG의 스프라이트 영역을 가져오고 현재 씬에 연결합니다.</summary>
public static class BattleInfoSkinInstaller
{
    public const string AssetFolder = "Assets/DontCommitFolder/Images/UI/BattlePanel";
    private const string RequestPath = AssetFolder + "/.install-request";

    [Serializable] private class Manifest { public AssetEntry[] assets; }
    [Serializable] private class AssetEntry { public string file; public SpriteRectData unitySpriteRect; }
    [Serializable] private class SpriteRectData { public float x, y, width, height; }

    [InitializeOnLoadMethod]
    private static void OnReload()
    {
        // 외부에서 명시적으로 만든 일회성 요청이 있을 때만 실행합니다.
        EditorApplication.delayCall += ConsumeRequest;
    }

    private static void ConsumeRequest()
    {
        if (!File.Exists(RequestPath)) return;
        if (EditorApplication.isCompiling || EditorApplication.isUpdating ||
            EditorApplication.isPlayingOrWillChangePlaymode)
        {
            EditorApplication.delayCall += ConsumeRequest;
            return;
        }
        string scenePath = File.ReadAllText(RequestPath).Trim();
        File.Move(RequestPath, RequestPath + "." + DateTime.UtcNow.Ticks + ".processed");
        try
        {
            if (SceneManager.GetActiveScene().path != scenePath)
                throw new InvalidOperationException("요청한 씬이 활성 씬이 아닙니다: " + scenePath);
            Install();
            if (!EditorSceneManager.SaveScene(SceneManager.GetActiveScene()))
                throw new IOException("전투정보 UI를 적용한 씬을 저장하지 못했습니다.");
            File.WriteAllText(AssetFolder + "/installation-result.txt",
                $"SUCCESS\nScene: {scenePath}\nSprites: 9\nTabs: 2\nTab cycling and row sprite/grade checks: PASS\nUTC: {DateTime.UtcNow:O}\n");
        }
        catch (Exception exception)
        {
            File.WriteAllText(AssetFolder + "/installation-result.txt", "FAILED\n" + exception);
            Debug.LogException(exception);
        }
    }

    [MenuItem("GameObject/삼국지 디펜스/전투 정보 이미지 적용", false, 13)]
    public static void Install()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode)
            throw new InvalidOperationException("편집 모드에서 실행하세요.");
        string manifestPath = AssetFolder + "/manifest.json";
        if (!File.Exists(manifestPath)) throw new FileNotFoundException("UI 이미지와 manifest.json을 먼저 복사하세요.", manifestPath);
        Manifest manifest = JsonUtility.FromJson<Manifest>(File.ReadAllText(manifestPath));
        if (manifest.assets == null || manifest.assets.Length != 9)
            throw new InvalidDataException("전투정보 이미지 9개가 필요합니다.");
        foreach (AssetEntry entry in manifest.assets)
            if (!File.Exists(AssetFolder + "/" + entry.file))
                throw new FileNotFoundException(entry.file);

        Sprite[] sprites = manifest.assets.Select(ImportSprite).ToArray();
        string resourceFolder = AssetFolder + "/Resources/UI";
        Directory.CreateDirectory(resourceFolder);
        AssetDatabase.Refresh();
        string skinPath = resourceFolder + "/BattleInfoSkin.asset";
        BattleInfoSkin skin = AssetDatabase.LoadAssetAtPath<BattleInfoSkin>(skinPath);
        if (skin == null)
        {
            skin = ScriptableObject.CreateInstance<BattleInfoSkin>();
            AssetDatabase.CreateAsset(skin, skinPath);
        }
        skin.panel = sprites[0];
        skin.title = sprites[1];
        skin.tabNormal = sprites[2];
        skin.tabSelected = sprites[3];
        skin.countStrip = sprites[4];
        skin.unitRow = sprites[5];
        skin.synergyRow = sprites[6];
        skin.emptyFrame = sprites[7];
        skin.gradeBadge = sprites[8];
        EditorUtility.SetDirty(skin);
        AssetDatabase.SaveAssets();

        BattleInfoPanel panel = Object.FindObjectsByType<BattleInfoPanel>(
            FindObjectsInactive.Include, FindObjectsSortMode.None)
            .FirstOrDefault(p => p.gameObject.scene == SceneManager.GetActiveScene());
        if (panel == null)
        {
            panel = BattleInfoUIFactory.Create();
            Undo.RegisterCreatedObjectUndo(panel.gameObject, "전투정보 UI 이미지 적용");
        }
        else Undo.RegisterFullObjectHierarchyUndo(panel.gameObject, "전투정보 UI 이미지 적용");
        skin.ApplyPanel(panel);
        panel.EnsureUnitList();
        panel.EnsureSynergyList();
        if (panel.UIFont != null)
            foreach (TMP_Text label in panel.GetComponentsInChildren<TMP_Text>(true))
                label.font = panel.UIFont;
        Validate(panel, skin);
        foreach (Component component in panel.GetComponentsInChildren<Component>(true))
        {
            if (component == null) continue;
            EditorUtility.SetDirty(component);
            PrefabUtility.RecordPrefabInstancePropertyModifications(component);
        }
        EditorSceneManager.MarkSceneDirty(panel.gameObject.scene);
        Selection.activeGameObject = panel.transform.Find("BattleInfo").gameObject;
        Debug.Log("전투정보 UI: 이미지 9개 연결, 출전장수/시너지 탭 및 목록 검증 완료.", panel);
    }

    private static Sprite ImportSprite(AssetEntry entry)
    {
        string path = AssetFolder + "/" + entry.file;
        AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceSynchronousImport);
        var importer = (TextureImporter)AssetImporter.GetAtPath(path);
        importer.textureType = TextureImporterType.Sprite;
        importer.spriteImportMode = SpriteImportMode.Multiple;
        importer.spritePixelsPerUnit = 100;
        importer.alphaIsTransparency = true;
        importer.mipmapEnabled = false;
        importer.wrapMode = TextureWrapMode.Clamp;
        importer.filterMode = FilterMode.Bilinear;
        importer.maxTextureSize = 4096;
        importer.textureCompression = TextureImporterCompression.Uncompressed;
        var settings = new TextureImporterSettings();
        importer.ReadTextureSettings(settings);
        settings.spriteMeshType = SpriteMeshType.FullRect;
        importer.SetTextureSettings(settings);
        var factories = new SpriteDataProviderFactories();
        factories.Init();
        var provider = factories.GetSpriteEditorDataProviderFromObject(importer);
        provider.InitSpriteEditorDataProvider();
        string name = Path.GetFileNameWithoutExtension(entry.file);
        var previous = provider.GetSpriteRects().FirstOrDefault(s => s.name == name);
        var rect = new SpriteRect
        {
            name = name,
            spriteID = previous != null ? previous.spriteID : GUID.Generate(),
            rect = new Rect(entry.unitySpriteRect.x, entry.unitySpriteRect.y,
                entry.unitySpriteRect.width, entry.unitySpriteRect.height),
            pivot = new Vector2(0.5f, 0.5f),
            alignment = SpriteAlignment.Center
        };
        provider.SetSpriteRects(new[] { rect });
        provider.GetDataProvider<ISpriteNameFileIdDataProvider>()
            .SetNameFileIdPairs(new[] { new SpriteNameFileIdPair(name, rect.spriteID) });
        provider.Apply();
        importer.SaveAndReimport();
        Sprite sprite = AssetDatabase.LoadAllAssetsAtPath(path).OfType<Sprite>().FirstOrDefault();
        if (sprite == null) throw new InvalidDataException("스프라이트 생성 실패: " + path);
        return sprite;
    }

    private static void Validate(BattleInfoPanel panel, BattleInfoSkin skin)
    {
        if (panel.TabCount != 2) throw new InvalidOperationException("탭 수가 2개가 아닙니다.");
        Transform root = panel.transform.Find("BattleInfo");
        Image units = root.Find("UnitsTab").GetComponent<Image>();
        Image synergies = root.Find("SynergiesTab").GetComponent<Image>();
        panel.SelectTab(0);
        if (units.sprite != skin.tabSelected || synergies.sprite != skin.tabNormal)
            throw new InvalidOperationException("출전장수 선택 이미지 연결 오류");
        panel.SelectNextTab();
        if (panel.CurrentTabIndex != 1 || synergies.sprite != skin.tabSelected || units.sprite != skin.tabNormal)
            throw new InvalidOperationException("시너지 선택 이미지 연결 오류");
        panel.SelectNextTab();
        if (panel.CurrentTabIndex != 0) throw new InvalidOperationException("탭 순환 오류");
        var temporary = new GameObject("BattlePanelValidation", typeof(RectTransform));
        temporary.SetActive(false);
        try
        {
            BattleUnitRowView unit = BattleInfoSkin.Child("Unit", temporary.transform).gameObject.AddComponent<BattleUnitRowView>();
            unit.Initialize(panel.UIFont);
            if (unit.GetComponent<Image>().sprite != skin.unitRow ||
                unit.transform.Find("PortraitFrame").GetComponent<Image>().sprite != skin.emptyFrame)
                throw new InvalidOperationException("장수 행 이미지 연결 오류");
            BattleSynergyRowView synergy = BattleInfoSkin.Child("Synergy", temporary.transform).gameObject.AddComponent<BattleSynergyRowView>();
            synergy.Initialize(null, panel.UIFont);
            foreach (int count in new[] { 1, 2, 8 })
            {
                synergy.Bind(new BattleSynergyEntry("검증", BattleSynergyCategory.Faction, count));
                string expected = count < 2 ? "대기" : $"{count / 2}단계";
                if (synergy.transform.Find("GradeBadge/Grade").GetComponent<TMP_Text>().text != expected)
                    throw new InvalidOperationException("시너지 활성 등급 표시 오류");
            }
            if (synergy.GetComponent<Image>().sprite != skin.synergyRow ||
                synergy.transform.Find("GradeBadge").GetComponent<Image>().sprite != skin.gradeBadge)
                throw new InvalidOperationException("시너지 행 이미지 연결 오류");
        }
        finally { Object.DestroyImmediate(temporary); }
    }
}
