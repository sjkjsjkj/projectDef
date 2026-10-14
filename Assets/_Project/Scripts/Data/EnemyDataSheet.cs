using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;

/// <summary>CSV 원본의 한 행입니다. 체력 등 실행 중 상태와 보상은 보관하지 않습니다.</summary>
public sealed class EnemyData
{
    public int Index { get; }
    // Resources 기준 확장자 없는 Sprite 경로. 빈 값이면 공통 프리팹의 스프라이트 유지.
    public string SpritePath { get; }
    public float Hp { get; }
    public float Speed { get; }

    public EnemyData(int index, string spritePath, float hp, float speed)
    {
        Index = index;
        SpritePath = spritePath;
        Hp = hp;
        Speed = speed;
    }
}

/// <summary>INDEX를 키로 조회하는 읽기 전용 적 데이터 시트입니다.</summary>
public sealed class EnemyDataSheet
{
    private readonly Dictionary<int, EnemyData> _rows = new Dictionary<int, EnemyData>();
    public int Count => _rows.Count;
    public bool TryGet(int index, out EnemyData data) => _rows.TryGetValue(index, out data);

    public static bool TryParse(string csv, out EnemyDataSheet sheet, out string error)
    {
        sheet = null;
        error = null;
        var parsed = new EnemyDataSheet();
        using (var reader = new StringReader(csv ?? string.Empty))
        {
            string header = reader.ReadLine();
            if (header == null || header.TrimStart('\uFEFF').Trim() != "INDEX,SPRITE,HP,SPEED")
            {
                error = "EnemyData.csv 헤더는 INDEX,SPRITE,HP,SPEED여야 합니다.";
                return false;
            }
            string line;
            int lineNumber = 1;
            while ((line = reader.ReadLine()) != null)
            {
                lineNumber++;
                if (string.IsNullOrWhiteSpace(line)) continue;
                if (!TryReadRow(line, out List<string> cells) || cells.Count != 4 ||
                    !int.TryParse(cells[0], NumberStyles.Integer, CultureInfo.InvariantCulture, out int index) || index < 1 ||
                    !TryNumber(cells[2], out float hp) || hp <= 0f ||
                    !TryNumber(cells[3], out float speed) || speed < 0f)
                {
                    error = $"EnemyData.csv {lineNumber}행: INDEX는 1 이상, HP는 양수, SPEED는 0 이상의 유한한 수여야 합니다.";
                    return false;
                }
                if (!parsed._rows.TryAdd(index, new EnemyData(index, cells[1].Trim(), hp, speed)))
                {
                    error = $"EnemyData.csv {lineNumber}행: 중복 INDEX {index}.";
                    return false;
                }
            }
        }
        if (parsed.Count == 0) { error = "EnemyData.csv에 데이터가 없습니다."; return false; }
        sheet = parsed;
        return true;
    }

    private static bool TryNumber(string value, out float number)
    {
        return float.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out number) &&
            !float.IsNaN(number) && !float.IsInfinity(number);
    }

    // 쉼표/따옴표를 포함하는 스프라이트 경로도 CSV 인용 규칙대로 읽습니다.
    private static bool TryReadRow(string line, out List<string> cells)
    {
        cells = new List<string>();
        var cell = new StringBuilder();
        bool quoted = false;
        bool closed = false;
        foreach (char c in line + ",")
        {
            if (quoted)
            {
                if (c == '"') { quoted = false; closed = true; }
                else cell.Append(c);
            }
            else if (c == ',' )
            {
                cells.Add(cell.ToString());
                cell.Clear();
                closed = false;
            }
            else if (c == '"' && (closed || cell.Length == 0))
            {
                if (closed) cell.Append('"');
                quoted = true;
                closed = false;
            }
            else
            {
                if (closed || c == '"') return false;
                cell.Append(c);
            }
        }
        return !quoted;
    }
}
