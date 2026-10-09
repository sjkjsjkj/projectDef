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
    [SerializeField] private bool startMinimized;
    private ResourceManager _resources;

    protected override void Awake()
    {
        base.Awake();
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
    }

    public void Open() => SetMinimized(false);
    public void Minimize() => SetMinimized(true);

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
            resourcesText.text = $"객잔   |   명예 {_resources.HonorLevel}   |   {_resources.Gold}원   |   초기화권 {_resources.InnRefreshTickets}장";
        if (refreshButton != null)
            refreshButton.interactable = innManager.IsInitialized && _resources.InnRefreshTickets > 0;
        if (offerViews == null) return;
        for (int i = 0; i < offerViews.Length; i++)
            if (offerViews[i] != null)
                offerViews[i].Refresh(innManager.GetOffer(i), innManager.IsSold(i), _resources.Gold);
    }
}
