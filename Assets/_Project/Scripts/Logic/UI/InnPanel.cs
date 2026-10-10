using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>항상 활성인 UI 루트에 부착합니다. 최소화는 본문만 숨기고 열기 버튼을 남깁니다.</summary>
public class InnPanel : UI
{
    [SerializeField] private InnManager innManager;
    [SerializeField] private GameObject contentRoot;
    [SerializeField] private Button openButton;
    [SerializeField] private Button minimizeButton;
    [SerializeField] private Button refreshButton;
    [SerializeField] private TMP_Text resourcesText;
    [SerializeField] private TMP_Text messageText;
    [SerializeField] private InnOfferView[] offerViews = new InnOfferView[InnManager.OfferCount];
    [SerializeField] private bool startMinimized = true;
    [SerializeField] private Button honorButton;
    [SerializeField] private TMP_Text honorButtonText;
    [SerializeField] private TMP_Text honorText;
    [SerializeField] private TMP_Text goldText;
    [SerializeField] private TMP_Text ticketsText;
    private ResourceManager _resources;
    public InnManager Manager => innManager;

    // 비활성 UI 루트에서 Awake 전에 호출합니다.
    public void Configure(InnManager manager, GameObject content, Button open, Button close, Button refresh,
        Button honor, TMP_Text honorLabel, TMP_Text honorStatus, TMP_Text gold, TMP_Text tickets,
        TMP_Text message, InnOfferView[] views)
    {
        innManager = manager;
        contentRoot = content;
        openButton = open;
        minimizeButton = close;
        refreshButton = refresh;
        honorButton = honor;
        honorButtonText = honorLabel;
        honorText = honorStatus;
        goldText = gold;
        ticketsText = tickets;
        messageText = message;
        offerViews = views;
        startMinimized = true;
    }

    protected override void Awake()
    {
        base.Awake();
        foreach (TMP_Text label in GetComponentsInChildren<TMP_Text>(true))
            if (label.font == null || !label.font.HasCharacter('객'))
                label.font = InnUIFactory.ResolveFont(null);
        if (offerViews == null || offerViews.Length != InnManager.OfferCount)
        {
            Debug.LogError("객잔 UI에는 정확히 다섯 개의 InnOfferView를 연결하세요.", this);
            enabled = false;
            return;
        }
        for (int i = 0; i < offerViews.Length; i++)
            if (offerViews[i] != null) offerViews[i].Bind(this, i);

        if (openButton != null) openButton.onClick.AddListener(Open);
        if (minimizeButton != null) minimizeButton.onClick.AddListener(Minimize);
        if (refreshButton != null) refreshButton.onClick.AddListener(RefreshOffers);
        if (honorButton != null) honorButton.onClick.AddListener(PurchaseHonor);
        SetMinimized(startMinimized);
    }

    private void OnEnable()
    {
        if (innManager == null) return;
        innManager.OnOffersChanged += RefreshView;
        _resources = innManager.Resources;
        if (_resources != null) _resources.OnChanged += RefreshView;
        RefreshView();
    }

    private void OnDisable()
    {
        if (innManager != null) innManager.OnOffersChanged -= RefreshView;
        if (_resources != null) _resources.OnChanged -= RefreshView;
        _resources = null;
    }

    private void OnDestroy()
    {
        if (openButton != null) openButton.onClick.RemoveListener(Open);
        if (minimizeButton != null) minimizeButton.onClick.RemoveListener(Minimize);
        if (refreshButton != null) refreshButton.onClick.RemoveListener(RefreshOffers);
        if (honorButton != null) honorButton.onClick.RemoveListener(PurchaseHonor);
    }

    public void Open()
    {
        SetMinimized(false);
        RefreshView();
    }
    public void Minimize() => SetMinimized(true);
    public void Close() => SetMinimized(true);

    private void PurchaseHonor()
    {
        if (_resources == null) return;
        int before = _resources.HonorLevel;
        bool success = _resources.TryPurchaseHonor(out string reason);
        if (messageText != null)
            messageText.text = !success ? reason : _resources.HonorLevel > before
                ? $"명예 레벨 {_resources.HonorLevel} 달성! 새 확률은 다음 초기화부터 적용됩니다."
                : "명예 경험치를 획득했습니다.";
        RefreshView();
    }

    public void Purchase(int index)
    {
        if (innManager == null) return;
        bool success = innManager.TryPurchase(index, out string reason);
        if (messageText != null) messageText.text = success ? "장수가 대기 슬롯에 합류했습니다." : reason;
        RefreshView();
    }

    private void RefreshOffers()
    {
        if (innManager == null) return;
        bool success = innManager.TryRefresh(out string reason);
        if (messageText != null) messageText.text = success ? "새 장수들이 객잔을 찾았습니다." : reason;
        RefreshView();
    }

    private void SetMinimized(bool minimized)
    {
        if (contentRoot != null) contentRoot.SetActive(!minimized);
        if (openButton != null) openButton.gameObject.SetActive(minimized);
    }

    private void RefreshView()
    {
        if (innManager == null || _resources == null) return;
        if (resourcesText != null)
            resourcesText.text = $"명예 {_resources.HonorLevel} ({_resources.HonorExperience}/{_resources.RequiredHonorExperience})   |   {_resources.Gold}골드   |   초기화권 {_resources.InnRefreshTickets}장";
        if (honorText != null) honorText.text = _resources.IsHonorMax ? "명예 Lv.10 · MAX"
            : $"명예 Lv.{_resources.HonorLevel}   {_resources.HonorExperience} / {_resources.RequiredHonorExperience} XP";
        if (goldText != null) goldText.text = $"보유 골드   {_resources.Gold}";
        if (ticketsText != null) ticketsText.text = $"초기화권   {_resources.InnRefreshTickets}장";
        if (honorButton != null) honorButton.interactable = _resources.CanPurchaseHonor;
        if (honorButtonText != null) honorButtonText.text = _resources.IsHonorMax ? "명예 최고 레벨"
            : $"명예 +{_resources.HonorPerPurchase} XP · {_resources.HonorPurchaseCost}골드";
        if (refreshButton != null)
            refreshButton.interactable = innManager.IsInitialized && _resources.InnRefreshTickets > 0;
        if (offerViews == null) return;
        for (int i = 0; i < offerViews.Length; i++)
            if (offerViews[i] != null)
                offerViews[i].Refresh(innManager.GetOffer(i), innManager.IsSold(i), _resources.Gold);
    }
}
