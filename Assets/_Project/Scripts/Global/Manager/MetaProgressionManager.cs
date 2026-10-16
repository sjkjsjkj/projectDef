using System;
using System.IO;
using UnityEngine;

/// <summary>메인 메뉴와 전투가 공유하는 영구 성장 서비스입니다. 전투 재화와 저장을 분리합니다.</summary>
public class MetaProgressionManager : GlobalSingleton<MetaProgressionManager>
{
    [SerializeField] private MetaProgressionSettings settings;
    private bool _initialized;
    private bool _ownsSettings;
    private string _pendingRun;
    private int _pendingWaves;
    private bool _pendingVictory;
    public MetaProgression Progression { get; private set; }
    public MetaUpgradeBonuses Bonuses { get; private set; } = MetaUpgradeBonuses.None;
    public string LastError { get; private set; }
    public string LoadNotice { get; private set; }
    public int LastReward { get; private set; }
    public int LastCompletedWaves { get; private set; }
    public bool HasPendingReward => _pendingRun != null;
    public bool IsReady => Progression != null && !HasPendingReward;
    public event Action OnChanged;

    // 에디터에서 스탯 미리보기/테스트를 할 때 실제 사용자 저장을 읽거나 싱글턴을 만들지 않습니다.
    public static MetaUpgradeBonuses CurrentBonuses => Application.isPlaying
        ? Ins?.Bonuses ?? MetaUpgradeBonuses.None : MetaUpgradeBonuses.None;

    public override void Initialize()
    {
        if (_initialized) return;
        _initialized = true;
        if (settings == null) settings = Resources.Load<MetaProgressionSettings>("MetaProgressionSettings");
        if (settings == null)
        {
            settings = ScriptableObject.CreateInstance<MetaProgressionSettings>();
            _ownsSettings = true;
        }
        try
        {
            var store = new MetaProgressionSaveStore(Path.Combine(Application.persistentDataPath, "meta-progression.json"));
            MetaProgression.SaveData data = store.Load(out string notice);
            LoadNotice = notice;
            Progression = new MetaProgression(settings, data, store.Save);
            Bonuses = MetaUpgradeBonuses.From(Progression);
        }
        catch (Exception exception)
        {
            LastError = exception.Message;
            Debug.LogError(LastError, this);
        }
    }

    public bool TryPurchase(string id, out string reason)
    {
        reason = LastError;
        SceneFlowManager flow = SceneFlowManager.Ins;
        if (flow == null || flow.CurrentScene != EScene.Main || flow.IsTransitioning)
        { reason = "능력 강화는 메인 화면에서만 구매할 수 있습니다."; return false; }
        if (!TrySettlePending() || Progression == null) { reason = LastError; return false; }
        bool result = Progression.TryPurchase(id, out reason);
        LastError = result ? null : reason;
        if (result) Bonuses = MetaUpgradeBonuses.From(Progression);
        OnChanged?.Invoke();
        return result;
    }

    public void CompleteRun(string runId, int completedWaves, bool victory)
    {
        if (string.IsNullOrEmpty(runId)) return;
        // 미정산 보상이 있으면 다음 게임 시작을 막으므로 서로 다른 정산이 덮어써지지 않습니다.
        if (_pendingRun != null && _pendingRun != runId) return;
        _pendingRun = runId;
        _pendingWaves = completedWaves;
        _pendingVictory = victory;
        LastCompletedWaves = completedWaves;
        LastReward = 0;
        TrySettlePending();
    }

    public bool TryResetSave(out string reason)
    {
        SceneFlowManager flow = SceneFlowManager.Ins;
        reason = null;
        if (flow == null || flow.CurrentScene != EScene.Main || flow.IsTransitioning)
        { reason = "세이브 초기화는 메인 화면에서만 가능합니다."; return false; }
        try
        {
            var store = new MetaProgressionSaveStore(Path.Combine(Application.persistentDataPath, "meta-progression.json"));
            var data = new MetaProgression.SaveData();
            var next = new MetaProgression(settings, data, store.Save);
            store.Reset(JsonUtility.ToJson(data, true));
            Progression = next;
            Bonuses = MetaUpgradeBonuses.None;
            _pendingRun = null;
            _pendingWaves = 0;
            _pendingVictory = false;
            LastReward = LastCompletedWaves = 0;
            LastError = LoadNotice = null;
        }
        catch (Exception exception)
        { reason = LastError = exception.Message; return false; }
        OnChanged?.Invoke();
        return true;
    }

    public bool TrySettlePending()
    {
        if (Progression == null) return false;
        if (_pendingRun == null) return true;
        bool success = Progression.TryReward(_pendingRun, _pendingWaves, _pendingVictory, out int reward, out string reason);
        LastError = reason;
        if (success)
        {
            LastReward = reward;
            _pendingRun = null;
        }
        OnChanged?.Invoke();
        return success;
    }

    protected override void OnDestroy()
    {
        if (_ownsSettings && settings != null) Destroy(settings);
        base.OnDestroy();
    }
}
