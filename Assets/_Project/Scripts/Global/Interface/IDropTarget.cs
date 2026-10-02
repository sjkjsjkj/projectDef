/// <summary>
/// 드래그N드랍으로 드랍할 수 있는 객체에게 부착될 인터페이스
/// </summary>
public interface IDropTarget
{
    IDraggable Occupant { get; } //점유자
    bool CanDrop(IDraggable draggable);
    void OnDrop(IDraggable draggable);
    void Remove(IDraggable draggable);

    void SetHighlight(bool active);

}
