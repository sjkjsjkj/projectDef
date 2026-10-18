using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

/// <summary>여포를 완성 이미지와 짧은 반동 모션으로 구성합니다.</summary>
public static class LuBuAnimationSetup
{
    public const string PrefabPath = "Assets/_Project/02_Prefab/Units/Unit_LuBu.prefab";
    public const string DataPath = "Assets/_Project/04_Data/Unit/Generated/UnitData_070.asset";
    private const string ImageFolder = "Assets/DontCommitFolder/Images/Unit/lu-bu/";

    [MenuItem("Window/Tools/Unit Animation/Create Lu Bu Example")]
    public static void CreateExample()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode)
            throw new InvalidOperationException("편집 모드에서 실행하세요.");
        UnitData data = AssetDatabase.LoadAssetAtPath<UnitData>(DataPath);
        if (data == null) throw new InvalidOperationException("여포 UnitData_070이 없습니다.");
        Sprite assembled = ImportSprite("assembled");
        Sprite portrait = ImportSprite("portrait");
        bool exists = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath) != null;
        GameObject root = exists ? PrefabUtility.LoadPrefabContents(PrefabPath) : new GameObject("Unit_LuBu");
        try
        {
            root.SetActive(false);
            if (!exists) root.layer = 9;
            Unit unit = GetOrAdd<Unit>(root);
            SetReference(unit, "unitData", data);
            var combat = GetOrAdd<UnitCombatController>(root);
            if (!exists)
            {
                var settings = new SerializedObject(combat);
                settings.FindProperty("combatEnabled").boolValue = true;
                settings.ApplyModifiedPropertiesWithoutUndo();
            }
            Transform visual = root.transform.Find("Visual") ?? Child(root.transform, "Visual");
            bool migrating = visual.Find("AttackPivot") != null;
            foreach (string name in new[] { "AttackPivot", "Body" })
            {
                Transform old = visual.Find(name);
                if (old != null) UnityEngine.Object.DestroyImmediate(old.gameObject);
            }
            var group = GetOrAdd<SortingGroup>(visual.gameObject);
            group.sortingOrder = 3;
            Transform image = visual.Find("Assembled") ?? Child(visual, "Assembled");
            image.localPosition = new Vector3(0.157f, 0.583f, 0f);
            var renderer = GetOrAdd<SpriteRenderer>(image.gameObject);
            renderer.sprite = assembled;
            renderer.color = Color.white;
            renderer.sharedMaterial = CombatPresentationSetup.EnsureMaterial();
            var collider = GetOrAdd<BoxCollider2D>(root);
            if (!exists) { collider.offset = new Vector2(0.01f, 0.32f); collider.size = new Vector2(0.58f, 0.64f); }
            var animation = GetOrAdd<UnitAttackAnimation>(root);
            SetReference(animation, "visual", visual);
            if (!exists || migrating)
            {
                var settings = new SerializedObject(animation);
                settings.FindProperty("duration").floatValue = 0.18f;
                settings.FindProperty("moveDistance").floatValue = 0.07f;
                settings.FindProperty("tiltAngle").floatValue = 3f;
                settings.ApplyModifiedPropertiesWithoutUndo();
            }
            root.SetActive(true);
            var prefab = PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
            SetReference(data, "prefab", prefab.GetComponent<Unit>());
            SetReference(data, "image", portrait);
            AssetDatabase.SaveAssets();
            Selection.activeObject = prefab;
        }
        finally
        {
            if (exists) PrefabUtility.UnloadPrefabContents(root);
            else UnityEngine.Object.DestroyImmediate(root);
        }
    }

    private static Sprite ImportSprite(string name)
    {
        string path = ImageFolder + name + ".png";
        if (AssetImporter.GetAtPath(path) is not TextureImporter importer)
            throw new InvalidOperationException($"여포 이미지가 없습니다: {path}");
        importer.textureType = TextureImporterType.Sprite;
        importer.spriteImportMode = SpriteImportMode.Single;
        importer.spritePixelsPerUnit = 1000f;
        var settings = new TextureImporterSettings();
        importer.ReadTextureSettings(settings);
        settings.spriteAlignment = (int)SpriteAlignment.Center;
        settings.spritePivot = new Vector2(0.5f, 0.5f);
        settings.spriteMeshType = SpriteMeshType.FullRect;
        importer.SetTextureSettings(settings);
        importer.alphaIsTransparency = true;
        importer.mipmapEnabled = false;
        importer.SaveAndReimport();
        return AssetDatabase.LoadAssetAtPath<Sprite>(path);
    }

    private static T GetOrAdd<T>(GameObject go) where T : Component
    {
        T component = go.GetComponent<T>();
        return component != null ? component : go.AddComponent<T>();
    }

    private static Transform Child(Transform parent, string name)
    {
        var child = new GameObject(name).transform;
        child.SetParent(parent, false);
        child.gameObject.layer = parent.gameObject.layer;
        return child;
    }

    private static void SetReference(UnityEngine.Object target, string property, UnityEngine.Object value)
    {
        var serialized = new SerializedObject(target);
        serialized.FindProperty(property).objectReferenceValue = value;
        serialized.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(target);
    }
}

