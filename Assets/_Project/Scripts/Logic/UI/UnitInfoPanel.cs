using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static UnitInfoUIFactory;

/// <summary>선택한 장수의 현재 능력치와 스킬을 우측 패널에 표시합니다.</summary>
public class UnitInfoPanel : UI
{
    private GameObject _panel;
    private Image _portrait;
    private TMP_Text _portraitFallback, _name, _identity, _description, _stats, _synergies;
    private SkillView _basicAttack, _skill;
    private ScrollRect _scroll;
    private float _nextRefresh;
    private bool _formationDirty = true;
    private readonly List<Unit> _deployed = new List<Unit>();

    public Unit SelectedUnit { get; private set; }
    public bool IsOpen => _panel != null && _panel.activeSelf;

    public void EnsureView()
    {
        if (_panel != null) return;
        TMP_FontAsset font = InnUIFactory.ResolveFont(null);
        RectTransform panel = Rect("UnitInfo", transform);
        panel.anchorMin = new Vector2(1, 0);
        panel.anchorMax = Vector2.one;
        panel.pivot = Vector2.one;
        panel.offsetMin = new Vector2(-360, 100);
        panel.offsetMax = new Vector2(-20, -80);
        panel.gameObject.AddComponent<Image>().color = new Color32(22, 25, 33, 248);
        _panel = panel.gameObject;

        TMP_Text title = Label("Title", panel, font, 21, "장수 상세");
        Stretch(title.rectTransform, 16, 0, 80, 12);
        title.rectTransform.anchorMin = new Vector2(0, 1);
        title.rectTransform.offsetMin = new Vector2(16, -44);
        RectTransform closeRect = Rect("Close", panel);
        closeRect.anchorMin = closeRect.anchorMax = closeRect.pivot = Vector2.one;
        closeRect.anchoredPosition = new Vector2(-12, -10);
        closeRect.sizeDelta = new Vector2(56, 32);
        Image closeFace = closeRect.gameObject.AddComponent<Image>();
        closeFace.color = new Color32(45, 51, 65, 255);
        Button close = closeRect.gameObject.AddComponent<Button>();
        close.targetGraphic = closeFace;
        close.navigation = new Navigation { mode = Navigation.Mode.None };
        close.onClick.AddListener(Close);
        TMP_Text closeText = Label("Label", closeRect, font, 15, "닫기");
        Stretch(closeText.rectTransform);
        closeText.alignment = TextAlignmentOptions.Center;

        RectTransform viewport = Rect("Viewport", panel);
        Stretch(viewport, 16, 16, 16, 54);
        viewport.gameObject.AddComponent<Image>().color = Color.clear;
        viewport.gameObject.AddComponent<RectMask2D>();
        RectTransform content = Rect("Content", viewport);
        content.anchorMin = new Vector2(0, 1);
        content.anchorMax = Vector2.one;
        content.pivot = new Vector2(.5f, 1);
        content.sizeDelta = Vector2.zero;
        VerticalContent(content, 12);
        content.gameObject.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        _scroll = viewport.gameObject.AddComponent<ScrollRect>();
        _scroll.viewport = viewport;
        _scroll.content = content;
        _scroll.horizontal = false;
        _scroll.movementType = ScrollRect.MovementType.Clamped;
        _scroll.scrollSensitivity = 28;

        RectTransform header = Rect("Identity", content);
        header.gameObject.AddComponent<LayoutElement>().preferredHeight = 96;
        _portrait = Icon("Portrait", header, 88, font, out _portraitFallback);
        _name = Label("Name", header, font, 24);
        Stretch(_name.rectTransform, 104, 36);
        _name.enableAutoSizing = true;
        _name.fontSizeMin = 16;
        _name.fontSizeMax = 24;
        _identity = Label("Level", header, font, 15);
        Stretch(_identity.rectTransform, 104, 0, 0, 62);
        _description = Label("Description", content, font, 16);
        Heading(content, font, "시너지");
        _synergies = Label("Synergies", content, font, 16);
        Heading(content, font, "현재 스탯");
        _stats = Label("Stats", content, font, 17);
        Heading(content, font, "기본 공격 / 스킬");
        _basicAttack = new SkillView(content, font, "BasicAttack");
        _skill = new SkillView(content, font, "Skill");
        _panel.SetActive(false);
    }

    public void Select(Unit unit)
    {
        if (unit == null || !unit.isActiveAndEnabled || unit.Data == null || unit.gameObject.scene != gameObject.scene)
            return;
        EnsureView();
        SelectedUnit = unit;
        _formationDirty = true;
        _panel.SetActive(true);
        Refresh();
        Canvas.ForceUpdateCanvases();
        _scroll.StopMovement();
        _scroll.verticalNormalizedPosition = 1;
    }

    public void Close()
    {
        SelectedUnit = null;
        if (_panel != null) _panel.SetActive(false);
    }

    private void OnEnable()
    {
        EventBus<OnUnitSelected>.Subscribe(OnSelected);
        EventBus<OnBattleFormationChanged>.Subscribe(OnFormationChanged);
    }

    private void OnDisable()
    {
        EventBus<OnUnitSelected>.Unsubscribe(OnSelected);
        EventBus<OnBattleFormationChanged>.Unsubscribe(OnFormationChanged);
        Close();
    }

    private void OnSelected(OnUnitSelected selection) => Select(selection.Unit);
    private void OnFormationChanged(OnBattleFormationChanged change)
    {
        if (change.Scene == gameObject.scene) _formationDirty = true;
    }

    private void LateUpdate()
    {
        if (!IsOpen) return;
        if (SelectedUnit == null || !SelectedUnit.isActiveAndEnabled || SelectedUnit.Data == null ||
            SelectedUnit.gameObject.scene != gameObject.scene)
        {
            Close();
            return;
        }
        // 일시정지 중의 레벨/강화 변경도 반영하며, 매 프레임 문자열 생성을 피합니다.
        if (_formationDirty || Time.unscaledTime >= _nextRefresh) Refresh();
    }

    private void Refresh()
    {
        Unit unit = SelectedUnit;
        UnitData data = unit.Data;
        UnitStats stats = unit.Stat;
        _name.text = string.IsNullOrWhiteSpace(data.UnitName) ? unit.name : data.UnitName;
        _identity.text = $"{data.Grade}등급  ·  Lv.{unit.Level}";
        _identity.color = InnOfferView.GradeColor(data.Grade);
        BindImage(_portrait, _portraitFallback, data.Image);
        _description.text = string.IsNullOrWhiteSpace(data.Description) ? "등록된 장수 설명이 없습니다." : data.Description;
        _stats.text = $"체력    {stats.CurrentHp:0.#} / {stats.MaxHp:0.#}\n공격력    {stats.AttackPower:0.#}     방어력    {stats.Defense:0.#}\n공격 속도    {stats.AttackSpd:0.##}\n공격 사거리    {stats.AttackRange:0.##}";
        _basicAttack.Bind(data.BasicAttack, unit, true);
        _skill.Bind(unit.CurrentSkill, unit, false);
        if (_formationDirty) RefreshSynergies(data);
        _nextRefresh = Time.unscaledTime + .15f;
    }

    private void RefreshSynergies(UnitData data)
    {
        _formationDirty = false;
        BattleFormationUtility.CollectUnits(gameObject.scene, _deployed);
        var units = new List<BattleSynergyUnit>();
        foreach (Unit unit in _deployed)
            if (unit.Data != null) units.Add(new BattleSynergyUnit(unit.Data.Id, unit.Data.Region, unit.Data.WeaponType));
        var text = new StringBuilder();
        var entries = BattleSynergyCalculator.Calculate(units, null);
        AppendTrait(text, entries, data.Region, BattleSynergyCategory.Faction, "진영");
        AppendTrait(text, entries, data.WeaponType, BattleSynergyCategory.Troop, "병종");
        // 추가 표기는 기존 시너지 계산에 포함되지 않으므로 출전 수를 붙이지 않습니다.
        if (!string.IsNullOrWhiteSpace(data.Region2)) text.AppendLine($"추가 소속 · {data.Region2}");
        if (!string.IsNullOrWhiteSpace(data.WeaponType2)) text.AppendLine($"추가 병종 · {data.WeaponType2}");
        SpecialSynergyDefinition[] specials = SpecialSynergyDefinition.CreateDefaults();
        foreach (GameObject root in gameObject.scene.GetRootGameObjects())
        {
            BattleInfoPanel info = root.GetComponentInChildren<BattleInfoPanel>(true);
            if (info != null) { specials = info.SpecialSynergies; break; }
        }
        var ids = new HashSet<string>();
        foreach (BattleSynergyUnit unit in units)
            if (!string.IsNullOrWhiteSpace(unit.Id)) ids.Add(unit.Id.Trim());
        if (specials != null)
            foreach (SpecialSynergyDefinition special in specials)
            {
                if (special == null || string.IsNullOrWhiteSpace(special.name) || special.memberIds == null) continue;
                bool belongs = false;
                int count = 0;
                var members = new HashSet<string>();
                bool valid = true;
                foreach (string id in special.memberIds)
                {
                    if (string.IsNullOrWhiteSpace(id) || !members.Add(id.Trim())) { valid = false; break; }
                    if (id.Trim() == data.Id?.Trim()) belongs = true;
                    if (ids.Contains(id.Trim())) count++;
                }
                if (valid && belongs) text.AppendLine($"{special.name} · 출전 {count}/{members.Count} · {(count == members.Count ? "활성" : "대기")}");
            }
        _synergies.text = text.Length == 0 ? "등록된 시너지가 없습니다." : text.ToString().TrimEnd();
    }

    private static void AppendTrait(StringBuilder text, List<BattleSynergyEntry> entries, string value,
        BattleSynergyCategory category, string label)
    {
        if (string.IsNullOrWhiteSpace(value)) return;
        int count = 0;
        foreach (BattleSynergyEntry entry in entries)
            if (entry.Category == category && entry.Name == value.Trim()) { count = entry.Count; break; }
        var synergy = new BattleSynergyEntry(value, category, count);
        text.AppendLine($"{label} · {value} · 출전 {count} · {(synergy.IsActive ? $"{synergy.Stage}단계" : "대기")}");
    }

    private static void Heading(Transform parent, TMP_FontAsset font, string text)
    {
        TMP_Text label = Label(text, parent, font, 18, text);
        label.color = new Color32(215, 181, 115, 255);
    }

    private static void BindImage(Image image, TMP_Text fallback, Sprite sprite)
    {
        image.sprite = sprite;
        image.enabled = sprite != null;
        fallback.gameObject.SetActive(sprite == null);
    }

    private sealed class SkillView
    {
        private readonly Image _icon;
        private readonly TMP_Text _fallback, _title, _description;

        public SkillView(Transform parent, TMP_FontAsset font, string name)
        {
            RectTransform root = Rect(name, parent);
            VerticalContent(root, 6);
            RectTransform header = Rect("Header", root);
            header.gameObject.AddComponent<LayoutElement>().preferredHeight = 52;
            _icon = Icon("Icon", header, 48, font, out _fallback);
            _title = Label("Name", header, font, 17);
            Stretch(_title.rectTransform, 60);
            _title.enableAutoSizing = true;
            _title.fontSizeMin = 13;
            _title.fontSizeMax = 17;
            _description = Label("Description", root, font, 16);
        }

        public void Bind(AbilityData data, Unit unit, bool basic)
        {
            string category = basic ? "기본 공격" : $"스킬 Lv.{unit.SkillLevel}";
            BindImage(_icon, _fallback, data != null ? data.Image : null);
            _title.text = data == null ? category : $"{category}\n{(string.IsNullOrWhiteSpace(data.SkillName) ? data.name : data.SkillName)}";
            _description.text = data == null ? "등록된 스킬이 없습니다." :
                (string.IsNullOrWhiteSpace(data.SkillDescription) ? "등록된 스킬 설명이 없습니다." : data.SkillDescription);
        }
    }
}
