using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>기존 전투 UI와 같은 색상으로 라운드 진행 UI를 생성합니다.</summary>
public static class RoundProgressUIFactory
{
    public static RoundProgressPanel Create(WaveManager manager = null, TMP_FontAsset font = null)
    {
        TMP_FontAsset displayFont = InnUIFactory.ResolveFont(font);
        var root = new GameObject("RoundProgressCanvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler));
        root.SetActive(false);
        Canvas canvas = root.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 50;
        CanvasScaler scaler = root.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1280, 720);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.Expand;

        RectTransform panel = InnUIFactory.Rect("RoundProgress", root.transform, 0, -20, 640, 110);
        panel.anchorMin = panel.anchorMax = panel.pivot = new Vector2(0.5f, 1);
        Image background = panel.gameObject.AddComponent<Image>();
        background.color = new Color32(22, 25, 33, 245);
        background.raycastTarget = false;

        TMP_Text round = Metric(panel, "Round", 12, 110, "현재 라운드", "—", displayFont);
        TMP_Text time = Metric(panel, "Time", 132, 126, "남은 시간", "00:00", displayFont);
        TMP_Text enemies = Metric(panel, "Enemies", 268, 208, "필드 적군 / 최대 허용", "0 / —", displayFont);
        TMP_Text remaining = Metric(panel, "RemainingSpawn", 486, 142, "남은 소환", "0", displayFont);
        TMP_Text hint = InnUIFactory.Label("EnemyLimitHint", panel, 268, 8, 208, 18,
            "최대 허용 수 초과 시 게임오버", 11, displayFont);
        hint.color = new Color32(163, 173, 190, 255);
        round.color = new Color32(226, 188, 112, 255);

        RoundProgressPanel view = root.AddComponent<RoundProgressPanel>();
        view.Configure(manager, font, round, time, enemies, remaining);
        root.SetActive(true);
        return view;
    }

    private static TMP_Text Metric(Transform parent, string name, float x, float width,
        string title, string value, TMP_FontAsset font)
    {
        TMP_Text label = InnUIFactory.Label(name + "Label", parent, x, 76, width, 22, title, 14, font);
        label.color = new Color32(163, 173, 190, 255);
        TMP_Text text = InnUIFactory.Label(name + "Value", parent, x, 29, width, 42, value, 30, font);
        text.enableAutoSizing = true;
        text.fontSizeMin = 16;
        text.fontSizeMax = 30;
        return text;
    }
}
