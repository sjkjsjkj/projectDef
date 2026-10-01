using UnityEngine;

/// <summary>
/// 플레이어의 상호작용 범위 내에서 자동 호출될 오브젝트들이 준수할 인터페이스
/// </summary>
public interface IInteractable
{
    bool CanInteract(GameObject player); // 자신이 상호작용 가능한 상태인지 검사하는 로직을 채운다.
    // UI 부분에서 상호작용 가능한 대상을 하이라이트 처리할 수 있도록 하기 위함

    void Interact(GameObject player); // 실제 상호작용 실행. 

    string GetMessage(); // UI나 로그에 출력할 메시지
    // 상호작용의 결과를 어떤 문구로 띄워줄지 각 스크립트에서 작성하도록 하기 위함
}
