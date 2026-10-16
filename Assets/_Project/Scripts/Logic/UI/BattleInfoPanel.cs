using System;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

/// <summary>전투 정보 탭과 출전 장수/시너지 목록을 연결합니다.</summary>
public class BattleInfoPanel : UI
{
    [Serializable]
    public class Tab
    {
        public Button button;
        public GameObject content;
    }

    [SerializeField] private Tab[] tabs = new Tab[0];
    [SerializeField] private int initialTabIndex = 0;
    [SerializeField] private bool enableTabShortcut = true;
    [SerializeField] private TMP_FontAsset uiFont;
    [SerializeField] private Color selectedColor = new Color32(116, 87, 43, 255);
    [SerializeField] private Color idleColor = new Color32(45, 51, 65, 255);
    [Header("특수 관계 시너지 (UnitData.Id)")]
    [SerializeField] private SpecialSynergyDefinition[] specialSynergies = SpecialSynergyDefinition.CreateDefaults();

    private UnityAction[] _clickHandlers;
    public int CurrentTabIndex { get; private set; } = -1;
    public int TabCount => tabs != null ? tabs.Length : 0;
    public SpecialSynergyDefinition[] SpecialSynergies => specialSynergies;

    // 비활성 UI 루트에서 Awake 전에 호출합니다.
    public void Configure(Tab[] entries, TMP_FontAsset font = null)
    {
        tabs = entries;
        uiFont = font;
    }

    protected override void Awake()
    {
        base.Awake();
        _clickHandlers = new UnityAction[TabCount];
        for (int i = 0; i < TabCount; i++)
        {
            if (tabs[i] == null || tabs[i].button == null || tabs[i].content == null)
            {
                Debug.LogError("전투 정보 UI의 탭 버튼과 본문을 모두 연결하세요.", this);
                enabled = false;
                return;
            }
            int index = i;
            _clickHandlers[i] = () => SelectTab(index);
            tabs[i].button.onClick.AddListener(_clickHandlers[i]);
        }

        TMP_FontAsset font = InnUIFactory.ResolveFont(uiFont);
        // 이미 씬에 저장된 빈 UI도 다시 만들지 않고 첫 번째 탭을 채웁니다.
        EnsureUnitList(font);
        EnsureSynergyList(font);
        if (font != null)
            foreach (TMP_Text label in GetComponentsInChildren<TMP_Text>(true))
                label.font = font;

        if (TabCount > 0) SelectTab(Mathf.Clamp(initialTabIndex, 0, TabCount - 1));
    }

    public void EnsureUnitList(TMP_FontAsset font = null)
    {
        if (TabCount == 0 || tabs[0] == null || tabs[0].content == null) return;
        GameObject content = tabs[0].content;
        BattleUnitListView list = content.GetComponent<BattleUnitListView>();
        if (list == null) list = content.AddComponent<BattleUnitListView>();
        list.EnsureView(font != null ? font : InnUIFactory.ResolveFont(uiFont));
    }

    public void EnsureSynergyList(TMP_FontAsset font = null)
    {
        if (TabCount < 2 || tabs[1] == null || tabs[1].content == null) return;
        GameObject content = tabs[1].content;
        BattleSynergyListView list = content.GetComponent<BattleSynergyListView>();
        if (list == null) list = content.AddComponent<BattleSynergyListView>();
        list.Configure(specialSynergies, font != null ? font : InnUIFactory.ResolveFont(uiFont));
    }

    public void SelectTab(int index)
    {
        if (index < 0 || index >= TabCount) return;
        CurrentTabIndex = index;
        for (int i = 0; i < TabCount; i++)
        {
            Tab tab = tabs[i];
            if (tab == null) continue;
            bool selected = i == index;
            if (tab.content != null) tab.content.SetActive(selected);
            if (tab.button != null && tab.button.targetGraphic != null)
                tab.button.targetGraphic.color = selected ? selectedColor : idleColor;
        }

        // 클릭했던 이전 탭에 포커스가 남아 Submit으로 되돌아가는 것을 방지합니다.
        EventSystem events = EventSystem.current;
        GameObject focused = events != null ? events.currentSelectedGameObject : null;
        if (focused != null)
            foreach (Tab tab in tabs)
                if (tab != null && tab.button != null && focused == tab.button.gameObject)
                {
                    events.SetSelectedGameObject(tabs[index].button.gameObject);
                    break;
                }
    }

    public void SelectNextTab()
    {
        if (TabCount > 0) SelectTab((CurrentTabIndex + 1) % TabCount);
    }

    private void Update()
    {
        if (SettingsPanel.IsOpen || !enableTabShortcut || Keyboard.current == null || !Keyboard.current.tabKey.wasPressedThisFrame)
            return;

        GameObject focused = EventSystem.current != null ? EventSystem.current.currentSelectedGameObject : null;
        // 텍스트 입력 중에는 입력 필드의 Tab 처리를 우선합니다.
        if (focused != null && (focused.GetComponent<TMP_InputField>() != null || focused.GetComponent<InputField>() != null))
            return;
        SelectNextTab();
    }

    private void OnDestroy()
    {
        if (_clickHandlers == null) return;
        for (int i = 0; i < _clickHandlers.Length && i < TabCount; i++)
            if (tabs[i] != null && tabs[i].button != null && _clickHandlers[i] != null)
                tabs[i].button.onClick.RemoveListener(_clickHandlers[i]);
    }
}
