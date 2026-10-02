using UnityEngine;
/// <summary>
/// 드래그가 가능한 물체에게 부착할 인터페이스
/// </summary>
public interface IDraggable
{
    Transform Transform { get; }

    IDropTarget CurrentTarget { get; set; }
}
