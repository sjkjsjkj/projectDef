using System;
using UnityEngine;

/// <summary>이벤트 세션과 UI를 연결합니다. 다음 라운드로의 전환은 GameManager가 담당합니다.</summary>
public class RoundEventManager : BaseMono
{
    [SerializeField] private ResourceManager resources;
    [SerializeField] private TMPro.TMP_FontAsset uiFont;
    private RoundEventPanel _panel;

    public ResourceManager Resources => resources;
    public RoundEventSession Session { get; private set; }
    public bool IsActive => Session != null && Session.IsActive;
    public event Action OnChanged;
    public event Action OnCompleted;
    public event Action OnCancelled;

    public bool TryBegin(RoundEventData data, WaveManager waves, int completedWaveNumber, out string reason)
    {
        reason = null;
        if (!isActiveAndEnabled || Session != null || resources == null || data == null || !data.IsValid())
        { reason = "RoundEventManager의 재화 참조, 이벤트 ID, 선택지와 효과 설정을 확인하세요. 선택지 후보는 표시 개수보다 많아야 합니다."; return false; }
        Session = new RoundEventSession(data, new RoundEventContext(resources, waves, completedWaveNumber));
        if (_panel == null) _panel = RoundEventUIFactory.Create(this, uiFont);
        _panel.gameObject.SetActive(true);
        OnChanged?.Invoke();
        return true;
    }

    public bool TryRefresh(int index, out string reason)
    {
        reason = "진행 중인 이벤트가 없습니다.";
        if (Session == null) return false;
        bool result = Session.TryRefresh(index, out reason);
        OnChanged?.Invoke();
        return result;
    }

    public bool TrySelect(int index, out string reason)
    {
        reason = "진행 중인 이벤트가 없습니다.";
        RoundEventSession session = Session;
        if (session == null || !session.TrySelect(index, out reason)) return false;
        // 효과 콜백이 게임을 종료/초기화했을 때 다음 라운드를 시작하지 않습니다.
        if (Session != session) return true;
        Session = null;
        if (_panel != null) _panel.gameObject.SetActive(false);
        OnChanged?.Invoke();
        OnCompleted?.Invoke();
        return true;
    }

    public void Cancel()
    {
        if (Session == null) return;
        Session.Cancel();
        Session = null;
        if (_panel != null) _panel.gameObject.SetActive(false);
        OnChanged?.Invoke();
        OnCancelled?.Invoke();
    }

    private void OnDisable() => Cancel();

    private void OnDestroy()
    {
        if (_panel != null) Destroy(_panel.gameObject);
    }
}
