using System;
using System.IO;

/// <summary>같은 디렉터리의 임시 파일을 원자적으로 교체하고 이전 저장을 백업합니다.</summary>
public sealed class MetaProgressionSaveStore
{
    public string Path { get; }
    private bool _recoveredBackup;
    public MetaProgressionSaveStore(string path) { Path = path; }

    public MetaProgression.SaveData Load(out string notice)
    {
        notice = null;
        if (!File.Exists(Path) && !File.Exists(Path + ".bak")) return new MetaProgression.SaveData();
        try { return MetaProgression.Parse(File.ReadAllText(Path)); }
        catch (Exception primary)
        {
            try
            {
                MetaProgression.SaveData data = MetaProgression.Parse(File.ReadAllText(Path + ".bak"));
                _recoveredBackup = true;
                notice = "최근 저장을 읽지 못해 이전 백업을 불러왔습니다.";
                return data;
            }
            catch (Exception backup)
            {
                throw new IOException("메타 성장 저장과 백업을 읽지 못했습니다. 원본 파일은 유지됩니다.",
                    new AggregateException(primary, backup));
            }
        }
    }

    public void Save(string json)
    {
        MetaProgression.Parse(json);
        Directory.CreateDirectory(System.IO.Path.GetDirectoryName(Path));
        string temp = Path + ".tmp";
        File.WriteAllText(temp, json);
        if (File.Exists(Path))
            File.Replace(temp, Path, _recoveredBackup ? null : Path + ".bak");
        else
            File.Move(temp, Path);
        _recoveredBackup = false;
    }

    /// <summary>이전 성장 데이터가 백업 복구로 되살아나지 않도록 백업도 초기화합니다.</summary>
    public void Reset(string json)
    {
        MetaProgression.Parse(json);
        Directory.CreateDirectory(System.IO.Path.GetDirectoryName(Path));
        string backup = Path + ".bak";
        string temp = Path + ".tmp";
        File.WriteAllText(temp, json);
        if (File.Exists(backup)) File.Replace(temp, backup, null);
        else File.Move(temp, backup);
        File.WriteAllText(temp, json);
        if (File.Exists(Path)) File.Replace(temp, Path, null);
        else File.Move(temp, Path);
        _recoveredBackup = false;
    }
}
