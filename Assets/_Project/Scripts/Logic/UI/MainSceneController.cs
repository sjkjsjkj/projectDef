using TMPro;
using UnityEngine;

/// <summary>MainScene에만 존재하는 진입점입니다. 메뉴는 씬을 나갈 때 함께 제거됩니다.</summary>
public class MainSceneController : MonoBehaviour
{
    [SerializeField] private TMP_FontAsset uiFont;

    private void Awake()
    {
        if (Camera.main == null)
        {
            var cameraObject = new GameObject("Main Camera", typeof(Camera), typeof(AudioListener));
            cameraObject.tag = "MainCamera";
            Camera camera = cameraObject.GetComponent<Camera>();
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color32(16, 22, 30, 255);
            camera.cullingMask = 0;
        }
        if (FindAnyObjectByType<MainMenuView>() == null)
            MainMenuUIFactory.Create(uiFont);
        InnUIFactory.EnsureEventSystem();
    }
}
