using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;

/// <summary>UnitInfo/UnitStat CSV를 UnitData SO와 UnitTable로 가져오는 에디터 유틸리티입니다.</summary>
public static class UnitInfoUtility
{
    public const string InfoCsvPath = "Assets/_Project/05_Docs/Data/UnitInfo.csv";
    public const string StatCsvPath = "Assets/_Project/05_Docs/Data/UnitStat.csv";
    public const string UnitFolderPath = "Assets/_Project/04_Data/Unit/Generated";
    public const string TableAssetPath = "Assets/Resources/Table/UnitTable.asset";

    private const string MakeMenu = "Window/Tools/UnitInfo/UnitInfoMake";
    private const string DeleteMenu = "Window/Tools/UnitInfo/UnitInfoDelete";

    [MenuItem(MakeMenu)]
    public static void UnitInfoMake()
    {
        try
        {
            // 두 CSV 전체를 검증한 뒤에만 에셋을 변경합니다.
            List<UnitRow> rows = ReadUnits(AbsolutePath(InfoCsvPath), AbsolutePath(StatCsvPath));
            CheckAssetType<UnitTable>(TableAssetPath);
            foreach (UnitRow row in rows) CheckAssetType<UnitData>(UnitPath(row));

            EnsureFolder(UnitFolderPath);
            EnsureFolder(Path.GetDirectoryName(TableAssetPath).Replace('\\', '/'));
            UnitTable table;
            int created = 0;
            AssetDatabase.StartAssetEditing();
            try
            {
                var units = new List<UnitData>(rows.Count);
                foreach (UnitRow row in rows)
                {
                    string path = UnitPath(row);
                    UnitData unit = AssetDatabase.LoadAssetAtPath<UnitData>(path);
                    bool isNew = unit == null;
                    if (isNew) unit = ScriptableObject.CreateInstance<UnitData>();

                    // CSV에 없는 이미지, 설명, 기본 공격, 스킬의 기존 연결은 유지합니다.
                    var serialized = new SerializedObject(unit);
                    serialized.FindProperty("id").stringValue = row.Id;
                    serialized.FindProperty("type").intValue = (int)EResourceType.Unit;
                    serialized.FindProperty("unitName").stringValue = row.Name;
                    serialized.FindProperty("grade").intValue = row.Grade;
                    serialized.FindProperty("region").stringValue = row.Info["REGION"];
                    serialized.FindProperty("region2").stringValue = row.Info["REGION_2"];
                    serialized.FindProperty("weaponType").stringValue = row.Info["WEAPONTYPE"];
                    serialized.FindProperty("weaponType2").stringValue = row.Info["WEAPONTYPE_2"];
                    serialized.FindProperty("maxHp").floatValue = row.MaxHp;
                    serialized.FindProperty("attack").floatValue = row.Attack;
                    serialized.FindProperty("defense").floatValue = row.Defense;
                    serialized.FindProperty("attackSpd").floatValue = row.AttackSpd;
                    serialized.FindProperty("range").floatValue = row.AttackRange;
                    serialized.ApplyModifiedPropertiesWithoutUndo();

                    if (isNew)
                    {
                        AssetDatabase.CreateAsset(unit, path);
                        created++;
                    }
                    EditorUtility.SetDirty(unit);
                    units.Add(unit);
                }

                table = AssetDatabase.LoadAssetAtPath<UnitTable>(TableAssetPath);
                bool isNewTable = table == null;
                if (isNewTable) table = ScriptableObject.CreateInstance<UnitTable>();
                var serializedTable = new SerializedObject(table);
                SerializedProperty elements = serializedTable.FindProperty("_elements");
                elements.arraySize = units.Count;
                for (int i = 0; i < units.Count; i++)
                    elements.GetArrayElementAtIndex(i).objectReferenceValue = units[i];
                serializedTable.ApplyModifiedPropertiesWithoutUndo();
                if (isNewTable) AssetDatabase.CreateAsset(table, TableAssetPath);
                EditorUtility.SetDirty(table);
            }
            finally
            {
                AssetDatabase.StopAssetEditing();
            }

            AssetDatabase.SaveAssets();
            table.Initialize();
            Selection.activeObject = table;
            EditorGUIUtility.PingObject(table);
            Debug.Log($"[UnitInfoMake] 생성 {created}개, 갱신 {rows.Count - created}개. " +
                $"총 {rows.Count}개를 {TableAssetPath}에 등록했습니다.", table);
        }
        catch (Exception exception)
        {
            Debug.LogError($"[UnitInfoMake] {exception.Message}");
        }
    }

    [MenuItem(DeleteMenu)]
    public static void UnitInfoDelete()
    {
        try
        {
            CheckAssetType<UnitTable>(TableAssetPath);
            if (AssetDatabase.LoadAssetAtPath<UnitTable>(TableAssetPath) == null)
            {
                Debug.Log("[UnitInfoDelete] 삭제할 UnitTable이 없습니다.");
                return;
            }

            // 생성된 유닛 SO와 수동 연결한 스킬/이미지는 삭제하지 않습니다.
            if (!AssetDatabase.DeleteAsset(TableAssetPath))
                throw new IOException($"UnitTable 삭제에 실패했습니다: {TableAssetPath}");
            Debug.Log($"[UnitInfoDelete] {TableAssetPath}을 삭제했습니다. 유닛 SO는 유지됩니다.");
        }
        catch (Exception exception)
        {
            Debug.LogError($"[UnitInfoDelete] {exception.Message}");
        }
    }

    [MenuItem(MakeMenu, true)]
    [MenuItem(DeleteMenu, true)]
    private static bool CanExecute() => !EditorApplication.isPlayingOrWillChangePlaymode && !EditorApplication.isCompiling;

    private static string AbsolutePath(string assetPath)
        => Path.Combine(Application.dataPath, "..", assetPath);

    // 이름이 바뀌어도 에셋 경로와 GUID가 유지되도록 INDEX만 사용합니다.
    private static string UnitPath(UnitRow row) => $"{UnitFolderPath}/UnitData_{row.Id}.asset";

    private static void CheckAssetType<T>(string path) where T : UnityEngine.Object
    {
        if (!string.IsNullOrEmpty(AssetDatabase.AssetPathToGUID(path)) &&
            AssetDatabase.LoadAssetAtPath<T>(path) == null)
            throw new InvalidDataException($"다른 종류의 에셋이 경로를 사용하고 있습니다: {path}");
    }

    private static void EnsureFolder(string path)
    {
        if (AssetDatabase.IsValidFolder(path)) return;
        string parent = Path.GetDirectoryName(path).Replace('\\', '/');
        EnsureFolder(parent);
        if (string.IsNullOrEmpty(AssetDatabase.CreateFolder(parent, Path.GetFileName(path))))
            throw new IOException($"폴더 생성에 실패했습니다: {path}");
    }

    private sealed class UnitRow
    {
        public string Id;
        public string Name;
        public int Grade;
        public Dictionary<string, string> Info;
        public float MaxHp, Attack, Defense, AttackSpd, AttackRange;
    }

    private static List<UnitRow> ReadUnits(string infoPath, string statPath)
    {
        var info = ReadCsv(infoPath, "INDEX", "NAME", "RANK", "REGION", "REGION_2", "WEAPONTYPE", "WEAPONTYPE_2");
        var stats = ReadCsv(statPath, "INDEX", "NAME", "RANK", "MaxHp", "Attack", "Def", "AttackSpd", "AttackRange");
        if (info.Count == 0 || info.Count != stats.Count)
            throw new InvalidDataException($"CSV 행 수가 비어 있거나 서로 다릅니다. UnitInfo={info.Count}, UnitStat={stats.Count}");

        var units = new List<UnitRow>(info.Count);
        foreach (var entry in info)
        {
            int index = entry.Key;
            var row = entry.Value;
            if (!stats.TryGetValue(index, out var stat))
                throw new InvalidDataException($"UnitStat.csv에 INDEX={index}가 없습니다.");
            int grade = ReadInt(row["RANK"], $"UnitInfo INDEX={index} RANK", 1, 5);
            int statGrade = ReadInt(stat["RANK"], $"UnitStat INDEX={index} RANK", 1, 5);
            if (row["NAME"].Length == 0 || row["NAME"] != stat["NAME"] || grade != statGrade)
                throw new InvalidDataException($"INDEX={index}: NAME이 비어 있거나 두 CSV의 NAME/RANK가 일치하지 않습니다.");
            if (row["REGION"].Length == 0 || row["WEAPONTYPE"].Length == 0)
                throw new InvalidDataException($"INDEX={index}: REGION과 WEAPONTYPE은 필수입니다.");
            units.Add(new UnitRow
            {
                Id = index.ToString("D3", CultureInfo.InvariantCulture),
                Name = row["NAME"], Grade = grade, Info = row,
                MaxHp = ReadFloat(stat, "MaxHp", index, 1f),
                Attack = ReadFloat(stat, "Attack", index, 1f),
                Defense = ReadFloat(stat, "Def", index, 0f),
                AttackSpd = ReadFloat(stat, "AttackSpd", index, 0.5f),
                AttackRange = ReadFloat(stat, "AttackRange", index, 1f)
            });
        }
        units.Sort((left, right) => int.Parse(left.Id, CultureInfo.InvariantCulture)
            .CompareTo(int.Parse(right.Id, CultureInfo.InvariantCulture)));
        return units;
    }

    private static int ReadInt(string text, string label, int min, int max)
    {
        if (!int.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out int value) || value < min || value > max)
            throw new InvalidDataException($"{label}: {min}~{max} 사이의 정수가 필요합니다. 입력값='{text}'");
        return value;
    }

    private static float ReadFloat(Dictionary<string, string> row, string column, int index, float min)
    {
        if (!float.TryParse(row[column], NumberStyles.Float, CultureInfo.InvariantCulture, out float value) ||
            float.IsNaN(value) || float.IsInfinity(value) || value < min)
            throw new InvalidDataException($"UnitStat INDEX={index} {column}: {min} 이상의 유한한 숫자가 필요합니다. 입력값='{row[column]}'");
        return value;
    }

    private static Dictionary<int, Dictionary<string, string>> ReadCsv(string path, params string[] requiredHeaders)
    {
        // 잘못된 인코딩을 조용히 대체하지 않고 오류로 알립니다. UTF-8 BOM도 처리됩니다.
        using var reader = new StreamReader(path, new UTF8Encoding(false, true), true);
        List<List<string>> records = ParseCsv(reader.ReadToEnd());
        if (records.Count == 0) throw new InvalidDataException($"빈 CSV입니다: {path}");
        List<string> headers = records[0];
        var headerSet = new HashSet<string>(headers, StringComparer.Ordinal);
        if (headerSet.Count != headers.Count || headerSet.Contains(""))
            throw new InvalidDataException($"빈 헤더 또는 중복 헤더가 있습니다: {path}");
        foreach (string header in requiredHeaders)
            if (!headerSet.Contains(header)) throw new InvalidDataException($"{Path.GetFileName(path)}에 {header} 열이 없습니다.");

        var result = new Dictionary<int, Dictionary<string, string>>();
        for (int i = 1; i < records.Count; i++)
        {
            List<string> values = records[i];
            if (values.Count != headers.Count)
                throw new InvalidDataException($"{Path.GetFileName(path)} 레코드 {i + 1}: 열 수가 {headers.Count}개여야 합니다.");
            var row = new Dictionary<string, string>(StringComparer.Ordinal);
            for (int c = 0; c < headers.Count; c++) row.Add(headers[c], values[c]);
            int index = ReadInt(row["INDEX"], $"{Path.GetFileName(path)} 레코드 {i + 1} INDEX", 1, int.MaxValue);
            if (!result.TryAdd(index, row))
                throw new InvalidDataException($"{Path.GetFileName(path)}의 INDEX={index}가 중복되었습니다.");
        }
        return result;
    }

    // 따옴표 내부의 쉼표/줄바꿈, 이중 따옴표 이스케이프, CRLF/LF를 처리합니다.
    private static List<List<string>> ParseCsv(string text)
    {
        var records = new List<List<string>>();
        var row = new List<string>();
        var field = new StringBuilder();
        bool quoted = false;
        bool closedQuote = false;
        for (int i = 0; i <= text.Length; i++)
        {
            bool end = i == text.Length;
            char ch = end ? '\n' : text[i];
            if (quoted)
            {
                if (end) throw new InvalidDataException("CSV의 따옴표가 닫히지 않았습니다.");
                if (ch == '"')
                {
                    if (i + 1 < text.Length && text[i + 1] == '"') { field.Append('"'); i++; }
                    else { quoted = false; closedQuote = true; }
                }
                else field.Append(ch);
                continue;
            }
            if (ch == ',' || ch == '\r' || ch == '\n')
            {
                row.Add(field.ToString().Trim());
                field.Clear();
                closedQuote = false;
                if (ch == ',') continue;
                if (row.Exists(value => value.Length > 0)) records.Add(row);
                row = new List<string>();
                if (ch == '\r' && i + 1 < text.Length && text[i + 1] == '\n') i++;
            }
            else if (ch == '"' && field.Length == 0 && !closedQuote) quoted = true;
            else
            {
                if (ch == '"' || (closedQuote && !char.IsWhiteSpace(ch)))
                    throw new InvalidDataException("CSV의 따옴표 위치가 올바르지 않습니다.");
                field.Append(ch);
            }
        }
        return records;
    }
}
