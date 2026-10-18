using System.IO;
using UnityEditor;
using UnityEngine;

/// <summary>기본 샘플 이펙트와 원거리 예제 SO를 생성합니다. 수동으로 연결한 이미지는 보존합니다.</summary>
public static class CombatPresentationSetup
{
    public const string Folder = "Assets/_Project/Scripts/Resources/CombatEffects";
    public const string RangedExamplePath = Folder + "/AbilityData_RangedExample.asset";
    private const string BasicPath = "Assets/_Project/04_Data/Unit/Skill/AbilityData_BasicAttack.asset";

    [MenuItem("Window/Tools/Unit Animation/Setup Combat Effects")]
    public static void Setup()
    {
        EnsureMaterial();
        Sprite slash = CreateSprite("Slash", 0);
        Sprite impact = CreateSprite("Impact", 1);
        Sprite projectile = CreateSprite("Projectile", 2);
        AbilityData basic = AssetDatabase.LoadAssetAtPath<AbilityData>(BasicPath);
        if (basic == null) throw new System.InvalidOperationException("기본 공격 SO를 찾을 수 없습니다.");
        SetDefaultImages(basic, slash, impact);
        AbilityData ranged = AssetDatabase.LoadAssetAtPath<AbilityData>(RangedExamplePath);
        if (ranged == null)
        {
            ranged = Object.Instantiate(basic);
            ranged.name = "AbilityData_RangedExample";
            var settings = new SerializedObject(ranged);
            settings.FindProperty("skillName").stringValue = "원거리 기본 공격 예제";
            settings.FindProperty("skillDescription").stringValue = "대상에게 투사체가 도착하고 피격 이펙트가 끝나면 피해를 줍니다.";
            settings.FindProperty("id").stringValue = "ranged-example";
            settings.FindProperty("deliveryType").enumValueIndex = (int)AttackDeliveryType.Ranged;
            settings.FindProperty("attackEffect").objectReferenceValue = projectile;
            settings.FindProperty("attackEffectSize").floatValue = 0.3f;
            settings.ApplyModifiedPropertiesWithoutUndo();
            AssetDatabase.CreateAsset(ranged, RangedExamplePath);
        }
        AssetDatabase.SaveAssets();
    }

    public static Material EnsureMaterial()
    {
        const string path = "Assets/_Project/Scripts/Resources/CombatSpriteMaterial.mat";
        Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (material != null) return material;
        Shader shader = Shader.Find("Universal Render Pipeline/2D/Sprite-Unlit-Default") ?? Shader.Find("Sprites/Default");
        material = new Material(shader);
        AssetDatabase.CreateAsset(material, path);
        return material;
    }

    private static void SetDefaultImages(AbilityData ability, Sprite attack, Sprite hit)
    {
        var settings = new SerializedObject(ability);
        if (ability.AttackEffect == null) settings.FindProperty("attackEffect").objectReferenceValue = attack;
        if (ability.HitEffect == null) settings.FindProperty("hitEffect").objectReferenceValue = hit;
        settings.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(ability);
    }

    private static Sprite CreateSprite(string name, int shape)
    {
        string path = Folder + "/" + name + ".png";
        Sprite existing = AssetDatabase.LoadAssetAtPath<Sprite>(path);
        if (existing != null) return existing;
        if (!AssetDatabase.IsValidFolder(Folder)) AssetDatabase.CreateFolder("Assets/_Project/Scripts/Resources", "CombatEffects");
        const int size = 128;
        var texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
        try
        {
            var pixels = new Color[size * size];
            for (int y = 0; y < size; y++)
            for (int x = 0; x < size; x++)
            {
                Vector2 p = new Vector2((x + 0.5f) / size * 2f - 1f, (y + 0.5f) / size * 2f - 1f);
                float mask;
                if (shape == 0)
                {
                    float outer = new Vector2(p.x + 0.12f, p.y).magnitude;
                    float inner = new Vector2(p.x + 0.4f, p.y).magnitude;
                    mask = Mathf.Clamp01((0.88f - outer) * 70f) * Mathf.Clamp01((inner - 0.78f) * 45f);
                }
                else if (shape == 1)
                {
                    float angle = Mathf.Atan2(p.y, p.x);
                    float radius = 0.24f + 0.56f * Mathf.Pow(Mathf.Abs(Mathf.Cos(angle * 4f)), 8f);
                    mask = Mathf.Clamp01((radius - p.magnitude) * 50f);
                }
                else
                {
                    float width = 0.12f + (1f - Mathf.Abs(p.x)) * 0.12f;
                    mask = Mathf.Clamp01((width - Mathf.Abs(p.y)) * 40f) * Mathf.Clamp01((0.9f - Mathf.Abs(p.x)) * 40f);
                    mask *= Mathf.Clamp01((0.9f - p.x - Mathf.Abs(p.y)) * 40f);
                }
                Color color = Color.Lerp(new Color(1f, 0.3f, 0.05f), new Color(1f, 0.95f, 0.65f), 1f - Mathf.Clamp01(p.magnitude));
                color.a = mask;
                pixels[y * size + x] = color;
            }
            texture.SetPixels(pixels);
            texture.Apply();
            File.WriteAllBytes(path, texture.EncodeToPNG());
        }
        finally { Object.DestroyImmediate(texture); }
        AssetDatabase.ImportAsset(path);
        var importer = (TextureImporter)AssetImporter.GetAtPath(path);
        importer.textureType = TextureImporterType.Sprite;
        importer.spriteImportMode = SpriteImportMode.Single;
        importer.spritePixelsPerUnit = size;
        importer.alphaIsTransparency = true;
        importer.mipmapEnabled = false;
        importer.textureCompression = TextureImporterCompression.Uncompressed;
        importer.SaveAndReimport();
        return AssetDatabase.LoadAssetAtPath<Sprite>(path);
    }
}
