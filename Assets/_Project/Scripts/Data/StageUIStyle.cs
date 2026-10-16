using TMPro;
using UnityEngine;

/// <summary>자동 생성하는 전투 UI가 빌드에서도 사용할 실제 폰트 에셋 참조입니다.</summary>
[CreateAssetMenu(fileName = "StageUIStyle", menuName = "ScriptableObjects/UI/Stage Style")]
public class StageUIStyle : ScriptableObject
{
    [SerializeField] private TMP_FontAsset buttonFont;
    public TMP_FontAsset ButtonFont => buttonFont;

    public static TMP_FontAsset LoadButtonFont()
    {
        StageUIStyle style = Resources.Load<StageUIStyle>("StageUIStyle");
        return InnUIFactory.ResolveFont(style != null ? style.ButtonFont : null);
    }
}
