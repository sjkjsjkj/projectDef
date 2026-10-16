using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>공유 옵션을 편집하는 메인 3탭 / 전투 단일 화면입니다.</summary>
public class SettingsPanel : MonoBehaviour
{
    private static SettingsPanel _current;
    public static bool IsOpen => _current != null && _current.isActiveAndEnabled;
    private DataManager _data;
    private TMP_FontAsset _font;
    private bool _battle;
    private float _previousTimeScale;
    private readonly List<(TMP_Text label, string ko, string en)> _labels = new();
    private readonly List<Vector2Int> _resolutions = new();
    private readonly List<GameObject> _pages = new();
    private readonly List<Button> _tabs = new();
    private readonly Slider[] _volumes = new Slider[3];
    private readonly TMP_Text[] _percentages = new TMP_Text[3];
    private TMP_Dropdown _resolution, _mode, _language;
    private Toggle _damage;
    private TMP_Text _message;
    private GameObject _confirmation;
    private int _tab;
    private GameLanguage _shownLanguage = (GameLanguage)(-1);
    private CanvasGroup _surfaceGroup;
    private GameObject _previousSelection;
    private readonly List<(CanvasGroup group, bool interactable, bool added)> _blockedUI = new();

    public static string Text(string ko, string en) =>
        DataManager.Ins.Option.Language == GameLanguage.English ? en : ko;

    public static SettingsPanel Show(bool battle, TMP_FontAsset font = null)
    {
        if (IsOpen) return _current;
        GameObject root = MetaUpgradePanel.CreateCanvas("SettingsCanvas", 900);
        var view = root.AddComponent<SettingsPanel>();
        view._data = DataManager.Ins;
        view._font = InnUIFactory.ResolveFont(font);
        view._battle = battle;
        view.Build();
        root.SetActive(true);
        return view;
    }

    private TMP_Text Label(Transform parent, string name, float x, float y, float w, float h,
        string ko, string en, int size = 20)
    {
        TMP_Text label = InnUIFactory.Label(name, parent, x, y, w, h, Text(ko, en), size, _font,
            TextAlignmentOptions.MidlineLeft);
        _labels.Add((label, ko, en));
        return label;
    }

    private Button Button(Transform parent, string name, float x, float y, float w,
        string ko, string en, UnityEngine.Events.UnityAction click)
    {
        Button button = InnUIFactory.MakeButton(name, parent, x, y, w, 42, Text(ko, en), _font);
        _labels.Add((button.GetComponentInChildren<TMP_Text>(), ko, en));
        button.onClick.AddListener(click);
        return button;
    }

    private void Build()
    {
        RectTransform panel = MetaUpgradePanel.CreateSurface(transform, 840, 580);
        _surfaceGroup = panel.gameObject.AddComponent<CanvasGroup>();
        Label(panel, "Title", 32, 506, 540, 44, _battle ? "게임 설정" : "설정", _battle ? "Game settings" : "Settings", 32);
        Button(panel, "Close", 670, 508, 138, "닫기", "Close", Close);
        if (!_battle)
        {
            string[] ko = { "디스플레이", "사운드", "게임" };
            string[] en = { "Display", "Sound", "Game" };
            for (int i = 0; i < 3; i++)
            {
                int index = i;
                _tabs.Add(Button(panel, "Tab" + i, 32 + i * 264, 442, 248, ko[i], en[i], () => SelectTab(index)));
                _pages.Add(InnUIFactory.Rect(en[i], panel, 32, 88, 776, 340).gameObject);
            }
            BuildDisplay(_pages[0].transform);
            BuildSound(_pages[1].transform);
            BuildGame(_pages[2].transform);
        }
        else
        {
            _pages.Add(InnUIFactory.Rect("GameSettings", panel, 32, 88, 776, 340).gameObject);
            BuildSound(_pages[0].transform);
            BuildDamage(_pages[0].transform, 0);
        }
        _message = Label(panel, "Message", 32, 22, 776, 52,
            "변경한 설정은 자동으로 적용·저장됩니다.", "Changes are applied and saved automatically.", 16);
        if (!_battle) BuildConfirmation();
        SelectTab(0);
    }

    private void BuildDisplay(Transform parent)
    {
        foreach (Resolution resolution in Screen.resolutions) AddResolution(resolution.width, resolution.height);
        AddResolution(Screen.width, Screen.height);
        AddResolution(_data.Screen.Width, _data.Screen.Height);
        _resolutions.Sort((a, b) => a.x != b.x ? a.x.CompareTo(b.x) : a.y.CompareTo(b.y));
        Label(parent, "ResolutionLabel", 0, 260, 280, 42, "해상도", "Resolution");
        _resolution = Dropdown(parent, "Resolution", 260);
        foreach (Vector2Int size in _resolutions) _resolution.options.Add(new TMP_Dropdown.OptionData($"{size.x} × {size.y}"));
        _resolution.onValueChanged.AddListener(index =>
        {
            _data.Screen.Width = _resolutions[index].x;
            _data.Screen.Height = _resolutions[index].y;
            _data.ApplyDisplay();
            _data.OptionsChanged();
        });
        Label(parent, "ScreenModeLabel", 0, 180, 280, 42, "화면 설정", "Screen mode");
        _mode = Dropdown(parent, "ScreenMode", 180);
        _mode.onValueChanged.AddListener(index =>
        {
            _data.Screen.Mode = (EScreenMode)index;
            _data.ApplyDisplay();
            _data.OptionsChanged();
        });
    }

    private void AddResolution(int width, int height)
    {
        var size = new Vector2Int(width, height);
        if (width >= 800 && height >= 600 && !_resolutions.Contains(size)) _resolutions.Add(size);
    }

    private TMP_Dropdown Dropdown(Transform parent, string name, float y)
    {
        var resources = new TMP_DefaultControls.Resources
        {
            standard = Resources.GetBuiltinResource<Sprite>("UI/Skin/UISprite.psd"),
            background = Resources.GetBuiltinResource<Sprite>("UI/Skin/Background.psd"),
            knob = Resources.GetBuiltinResource<Sprite>("UI/Skin/Knob.psd"),
            checkmark = Resources.GetBuiltinResource<Sprite>("UI/Skin/Checkmark.psd"),
            dropdown = Resources.GetBuiltinResource<Sprite>("UI/Skin/DropdownArrow.psd"),
            mask = Resources.GetBuiltinResource<Sprite>("UI/Skin/UIMask.psd")
        };
        GameObject go = TMP_DefaultControls.CreateDropdown(resources);
        go.name = name;
        RectTransform rect = (RectTransform)go.transform;
        rect.SetParent(parent, false);
        rect.anchorMin = rect.anchorMax = rect.pivot = Vector2.zero;
        rect.anchoredPosition = new Vector2(316, y);
        rect.sizeDelta = new Vector2(460, 42);
        foreach (TMP_Text label in go.GetComponentsInChildren<TMP_Text>(true))
        { label.font = _font; label.fontSize = 16; }
        TMP_Dropdown dropdown = go.GetComponent<TMP_Dropdown>();
        dropdown.ClearOptions();
        return dropdown;
    }

    private void BuildSound(Transform parent)
    {
        string[] ko = { "마스터 볼륨", "배경음", "효과음" };
        string[] en = { "Master volume", "Music", "Sound effects" };
        for (int i = 0; i < 3; i++)
        {
            int index = i;
            float y = 260 - i * 80;
            Label(parent, "VolumeLabel" + i, 0, y, 290, 42, ko[i], en[i]);
            GameObject go = DefaultControls.CreateSlider(new DefaultControls.Resources
            {
                background = Resources.GetBuiltinResource<Sprite>("UI/Skin/Background.psd"),
                standard = Resources.GetBuiltinResource<Sprite>("UI/Skin/UISprite.psd"),
                knob = Resources.GetBuiltinResource<Sprite>("UI/Skin/Knob.psd")
            });
            go.name = "Volume" + i;
            RectTransform rect = (RectTransform)go.transform;
            rect.SetParent(parent, false);
            rect.anchorMin = rect.anchorMax = rect.pivot = Vector2.zero;
            rect.anchoredPosition = new Vector2(316, y);
            rect.sizeDelta = new Vector2(370, 42);
            Slider slider = go.GetComponent<Slider>();
            _volumes[i] = slider;
            _percentages[i] = InnUIFactory.Label("Percent" + i, parent, 696, y, 80, 42, "", 18, _font);
            slider.onValueChanged.AddListener(value =>
            {
                if (index == 0) _data.Volume.Master = value;
                else if (index == 1) _data.Volume.Bgm = value;
                else _data.Volume.Sfx = value;
                _data.OptionsChanged();
            });
        }
    }

    private void BuildGame(Transform parent)
    {
        BuildDamage(parent, 260);
        Label(parent, "LanguageLabel", 0, 180, 280, 42, "언어", "Language");
        _language = Dropdown(parent, "Language", 180);
        _language.AddOptions(new List<string> { "한국어", "English" });
        _language.onValueChanged.AddListener(value =>
        { _data.Option.Language = (GameLanguage)value; _data.OptionsChanged(); });
        Label(parent, "ResetLabel", 0, 100, 290, 42, "세이브 초기화", "Reset save");
        Button(parent, "ResetSave", 316, 100, 460, "세이브 초기화…", "Reset save…", () => SetConfirmation(true));
        Label(parent, "ResetHint", 0, 28, 776, 54, "메타포인트와 영구 강화가 초기화됩니다.",
            "Resets meta points and permanent upgrades.", 16);
    }

    private void BuildDamage(Transform parent, float y)
    {
        Label(parent, "DamageLabel", 0, y, 290, 42, "피해량 표시", "Show damage numbers");
        RectTransform rect = InnUIFactory.Rect("ShowDamage", parent, 316, y, 42, 42);
        Image box = rect.gameObject.AddComponent<Image>();
        box.color = new Color32(70, 80, 96, 255);
        RectTransform check = InnUIFactory.Rect("Check", rect, 6, 6, 30, 30);
        Image mark = check.gameObject.AddComponent<Image>();
        mark.sprite = Resources.GetBuiltinResource<Sprite>("UI/Skin/Checkmark.psd");
        _damage = rect.gameObject.AddComponent<Toggle>();
        _damage.targetGraphic = box;
        _damage.graphic = mark;
        _damage.onValueChanged.AddListener(value => { _data.Option.ShowDamage = value; _data.OptionsChanged(); });
    }

    private void BuildConfirmation()
    {
        RectTransform shade = InnUIFactory.Rect("ResetConfirmation", transform, 0, 0, 0, 0);
        shade.anchorMax = Vector2.one;
        shade.gameObject.AddComponent<Image>().color = new Color(0, 0, 0, .85f);
        RectTransform panel = MetaUpgradePanel.CreateSurface(shade, 660, 300);
        Label(panel, "Title", 30, 224, 600, 42, "세이브를 초기화할까요?", "Reset your save?", 26);
        Label(panel, "Description", 30, 106, 600, 100,
            "메타포인트와 모든 영구 강화가 삭제됩니다.\n되돌릴 수 없습니다. 화면·사운드·언어 설정은 유지됩니다.",
            "All meta points and permanent upgrades will be erased.\nThis cannot be undone. Your settings will be kept.", 18);
        Button(panel, "Cancel", 30, 32, 280, "취소", "Cancel", () => SetConfirmation(false));
        Button(panel, "ConfirmReset", 350, 32, 280, "초기화", "Reset", () =>
        {
            bool success = MetaProgressionManager.Ins.TryResetSave(out string reason);
            SetConfirmation(false);
            _message.text = success ? Text("세이브를 초기화했습니다.", "Save reset successfully.")
                : Text("초기화 실패: ", "Reset failed: ") + reason;
        });
        _confirmation = shade.gameObject;
        _confirmation.SetActive(false);
    }

    private void SetConfirmation(bool visible)
    {
        _confirmation.SetActive(visible);
        _surfaceGroup.interactable = !visible;
        if (EventSystem.current != null) EventSystem.current.SetSelectedGameObject(visible
            ? _confirmation.transform.Find("Panel/Cancel").gameObject : transform.Find("Panel/Game/ResetSave").gameObject);
    }

    private void SelectTab(int index)
    {
        _tab = index;
        for (int i = 0; i < _pages.Count; i++) _pages[i].SetActive(i == index);
        for (int i = 0; i < _tabs.Count; i++) _tabs[i].GetComponent<Image>().color = i == _tab
            ? new Color32(160, 122, 65, 255) : new Color32(49, 58, 73, 255);
    }

    private void Refresh()
    {
        if (_shownLanguage != _data.Option.Language)
        {
            _shownLanguage = _data.Option.Language;
            foreach (var entry in _labels) entry.label.text = Text(entry.ko, entry.en);
            if (_mode != null)
            {
                _mode.ClearOptions();
                _mode.AddOptions(new List<string> { Text("전체화면", "Fullscreen"), Text("창화면", "Windowed"), Text("테두리없는 화면", "Borderless") });
            }
        }
        float[] values = { _data.Volume.Master, _data.Volume.Bgm, _data.Volume.Sfx };
        for (int i = 0; i < 3; i++)
        { _volumes[i].SetValueWithoutNotify(values[i]); _percentages[i].text = $"{Mathf.RoundToInt(values[i] * 100)}%"; }
        _damage.SetIsOnWithoutNotify(_data.Option.ShowDamage);
        if (_language != null) _language.SetValueWithoutNotify((int)_data.Option.Language);
        if (_mode != null) _mode.SetValueWithoutNotify((int)_data.Screen.Mode);
        if (_resolution != null)
        {
            _resolution.SetValueWithoutNotify(_resolutions.IndexOf(new Vector2Int(_data.Screen.Width, _data.Screen.Height)));
            _resolution.RefreshShownValue();
        }
    }

    private void OnEnable()
    {
        if (_data == null) return;
        _current = this;
        _previousSelection = EventSystem.current != null ? EventSystem.current.currentSelectedGameObject : null;
        foreach (Canvas canvas in FindObjectsByType<Canvas>(FindObjectsSortMode.None))
        {
            if (!canvas.isRootCanvas || canvas.transform == transform) continue;
            CanvasGroup group = canvas.GetComponent<CanvasGroup>();
            bool added = group == null;
            if (added) group = canvas.gameObject.AddComponent<CanvasGroup>();
            _blockedUI.Add((group, group.interactable, added));
            group.interactable = false;
        }
        if (_battle) { _previousTimeScale = Time.timeScale; Time.timeScale = 0f; }
        _data.OnOptionsChanged += Refresh;
        Refresh();
        if (EventSystem.current != null) EventSystem.current.SetSelectedGameObject(transform.Find("Panel/Close").gameObject);
    }

    private void OnDisable()
    {
        if (_data == null) return;
        _data.OnOptionsChanged -= Refresh;
        _data.SaveOptions();
        foreach (var entry in _blockedUI)
        {
            if (entry.group == null) continue;
            entry.group.interactable = entry.interactable;
            if (entry.added) Destroy(entry.group);
        }
        _blockedUI.Clear();
        if (EventSystem.current != null && _previousSelection != null)
            EventSystem.current.SetSelectedGameObject(_previousSelection);
        if (_current == this)
        {
            _current = null;
            if (_battle) Time.timeScale = _previousTimeScale;
        }
    }

    private void Update()
    {
        if (_data.LastSaveError != null)
            _message.text = Text("설정 저장 실패: ", "Settings save failed: ") + _data.LastSaveError;
        if (Keyboard.current == null || !Keyboard.current.escapeKey.wasPressedThisFrame) return;
        foreach (TMP_Dropdown dropdown in GetComponentsInChildren<TMP_Dropdown>())
            if (dropdown.IsExpanded) { dropdown.Hide(); return; }
        if (_confirmation != null && _confirmation.activeSelf) SetConfirmation(false);
        else Close();
    }

    public void Close()
    {
        if (!_data.SaveOptions())
        { _message.text = Text("설정 저장 실패: ", "Settings save failed: ") + _data.LastSaveError; return; }
        gameObject.SetActive(false);
        Destroy(gameObject);
    }
}
