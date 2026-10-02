/// <summary>
/// 저장될 수 있는 객체
/// </summary>
public interface ISaveable
{
    string UnitId { get; set; }
    string UniqueId { get; set; }
    string SaveData();
    void LoadData(string stateJson);
}
