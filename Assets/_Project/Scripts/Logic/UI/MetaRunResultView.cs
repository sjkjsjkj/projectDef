using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>정산 결과를 보여주고 메인 화면의 영구 성장으로 돌아갑니다.</summary>
public class MetaRunResultView : MonoBehaviour
{
    private MetaProgressionManager _meta;
    private TMP_Text _reward, _message;
    private Button _return;

    public static void Show(bool victory)
    {
        if (FindAnyObjectByType<MetaRunResultView>() != null) return;
        TMP_FontAsset font = InnUIFactory.ResolveFont(null);
        GameObject root = MetaUpgradePanel.CreateCanvas("RunResult", 700);
        var view = root.AddComponent<MetaRunResultView>();
        view._meta = MetaProgressionManager.Ins;
        RectTransform panel = MetaUpgradePanel.CreateSurface(root.transform, 760, 420);
        InnUIFactory.Label("Title", panel, 30, 320, 700, 60, victory ? "전투 승리" : "전투 종료", 40, font);
        view._reward = InnUIFactory.Label("Reward", panel, 30, 172, 700, 135, "", 26, font);
        view._message = InnUIFactory.Label("Message", panel, 30, 102, 700, 65, "", 17, font);
        view._return = InnUIFactory.MakeButton("Return", panel, 220, 36, 320, 50, "메인 화면으로", font);
        view._return.onClick.AddListener(() =>
        {
            view._meta.TrySettlePending();
            SceneFlowManager flow = SceneFlowManager.Ins;
            if (flow.ReturnToMain()) view._return.interactable = false;
            else view._message.text = flow.LastError ?? "화면 전환이 끝난 뒤 다시 시도하세요.";
        });
        root.SetActive(true);
    }

    private void OnEnable()
    {
        if (_meta == null) return;
        _meta.OnChanged += Refresh;
        Refresh();
    }
    private void OnDisable() { if (_meta != null) _meta.OnChanged -= Refresh; }
    private void Refresh()
    {
        _reward.text = $"완료한 웨이브  {_meta.LastCompletedWaves}\n획득 메타포인트  +{_meta.LastReward:N0}\n보유 메타포인트  {_meta.Progression?.Points ?? 0:N0}";
        _message.text = _meta.LastError ?? "메인 화면의 능력 강화에서 다음 전투를 준비하세요.";
    }
}
