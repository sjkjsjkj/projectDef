using System.Collections;
using NUnit.Framework;
using TMPro;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;
using Object = UnityEngine.Object;

public class SettingsPlayModeTests
{
    [UnityTest]
    public IEnumerator MainTabsLocalizationAndBattleSettingsShareSavedOptions()
    {
        yield return new EnterPlayMode();
        float deadline = Time.realtimeSinceStartup + 20f;
        while (SceneFlowManager.Ins.IsTransitioning && Time.realtimeSinceStartup < deadline) yield return null;
        Assert.That(SceneFlowManager.Ins.CurrentScene, Is.EqualTo(EScene.Main));
        DataManager data = DataManager.Ins;
        var meta = MetaProgressionManager.Ins;
        var metaSettings = Resources.Load<MetaProgressionSettings>("MetaProgressionSettings");
        var progression = new MetaProgression(metaSettings, new MetaProgression.SaveData(), _ => { });
        typeof(MetaProgressionManager).GetProperty("Progression").SetValue(meta, progression);
        typeof(MetaProgressionManager).GetProperty("Bonuses").SetValue(meta, MetaUpgradeBonuses.None);
        // 사용자 설정은 테스트가 끝나면 원래 값으로 되돌립니다.
        string before = JsonUtility.ToJson(data.Option);
        bool hadSave = PlayerPrefs.HasKey(DataManager.OptionsKey);
        string saved = PlayerPrefs.GetString(DataManager.OptionsKey);
        try
        {
            Button open = GameObject.Find("MainMenuCanvas/Settings").GetComponent<Button>();
            Assert.That(open.interactable, Is.True);
            open.onClick.Invoke();
            SettingsPanel panel = Object.FindAnyObjectByType<SettingsPanel>();
            Assert.That(panel, Is.Not.Null);
            Assert.That(GameObject.Find("MainMenuCanvas/StartGame").GetComponent<Button>().IsInteractable(), Is.False);
            Assert.That(panel.transform.Find("Panel/Display").gameObject.activeSelf, Is.True);
            TMP_Dropdown resolution = panel.transform.Find("Panel/Display/Resolution").GetComponent<TMP_Dropdown>();
            Assert.That(resolution.options, Is.Not.Empty);
            resolution.Show();
            yield return null;
            Assert.That(resolution.IsExpanded, Is.True);
            resolution.Hide();
            panel.transform.Find("Panel/Tab2").GetComponent<Button>().onClick.Invoke();
            panel.transform.Find("Panel/Game/Language").GetComponent<TMP_Dropdown>().value = 1;
            Assert.That(panel.transform.Find("Panel/Title").GetComponent<TMP_Text>().text, Is.EqualTo("Settings"));
            Assert.That(open.GetComponentInChildren<TMP_Text>().text, Is.EqualTo("Settings"));
            panel.transform.Find("Panel/Game/ResetSave").GetComponent<Button>().onClick.Invoke();
            Assert.That(panel.transform.Find("ResetConfirmation").gameObject.activeSelf, Is.True);
            panel.transform.Find("ResetConfirmation/Panel/Cancel").GetComponent<Button>().onClick.Invoke();
            panel.transform.Find("Panel/Game/ShowDamage").GetComponent<Toggle>().isOn = false;
            panel.transform.Find("Panel/Tab1").GetComponent<Button>().onClick.Invoke();
            panel.transform.Find("Panel/Sound/Volume0").GetComponent<Slider>().value = .23f;
            panel.Close();
            yield return null;
            Assert.That(SettingsPanel.IsOpen, Is.False);
            OptionData stored = JsonUtility.FromJson<OptionData>(PlayerPrefs.GetString(DataManager.OptionsKey));
            Assert.That(stored.Language, Is.EqualTo(GameLanguage.English));
            Assert.That(stored.ShowDamage, Is.False);
            Assert.That(stored.Volume.Master, Is.EqualTo(.23f).Within(.001f));

            Assert.That(SceneFlowManager.Ins.StartStage(), Is.True);
            deadline = Time.realtimeSinceStartup + 20f;
            while (SceneFlowManager.Ins.IsTransitioning && Time.realtimeSinceStartup < deadline) yield return null;
            Button battleOpen = GameObject.Find("BattleSettingsCanvas/Settings").GetComponent<Button>();
            Button speedButton = GameObject.Find("BattleSettingsCanvas/Speed").GetComponent<Button>();
            Button autoButton = GameObject.Find("BattleSettingsCanvas/AutoAdvance").GetComponent<Button>();
            var playback = battleOpen.GetComponentInParent<BattlePlaybackController>();
            Assert.That(battleOpen.GetComponentInChildren<TMP_Text>().font.name, Is.EqualTo("Shilla_Culture(B) SDF"));
            Assert.That(playback.Speed, Is.EqualTo(1f));
            foreach (float expected in new[] { 2f, 3f, 0.5f, 1f })
            {
                speedButton.onClick.Invoke();
                Assert.That(playback.Speed, Is.EqualTo(expected));
                Assert.That(Time.timeScale, Is.EqualTo(expected));
            }
            Assert.That(GameManager.Ins.WaveManager.AutoAdvance, Is.False);
            autoButton.onClick.Invoke();
            Assert.That(GameManager.Ins.WaveManager.AutoAdvance, Is.True);
            Assert.That(autoButton.GetComponentInChildren<TMP_Text>().text, Does.Contain("ON"));
            autoButton.onClick.Invoke();
            Assert.That(GameManager.Ins.WaveManager.AutoAdvance, Is.False);
            Time.timeScale = 1.5f;
            battleOpen.onClick.Invoke();
            panel = Object.FindAnyObjectByType<SettingsPanel>();
            Assert.That(Time.timeScale, Is.Zero);
            speedButton.onClick.Invoke();
            autoButton.onClick.Invoke();
            Assert.That(Time.timeScale, Is.Zero, "설정창의 일시정지를 배속 버튼이 해제하면 안 됩니다.");
            Assert.That(GameManager.Ins.WaveManager.AutoAdvance, Is.False);
            Assert.That(GameManager.Ins.IsCombatPaused, Is.True);
            Assert.That(panel.GetComponentsInChildren<Slider>().Length, Is.EqualTo(3));
            Assert.That(panel.GetComponentsInChildren<Toggle>().Length, Is.EqualTo(1));
            Assert.That(panel.GetComponentsInChildren<TMP_Dropdown>(true), Is.Empty);
            Assert.That(panel.transform.Find("ResetConfirmation"), Is.Null);
            Assert.That(panel.transform.Find("Panel/GameSettings/Volume0").GetComponent<Slider>().value, Is.EqualTo(.23f).Within(.001f));
            Assert.That(panel.transform.Find("Panel/GameSettings/ShowDamage").GetComponent<Toggle>().isOn, Is.False);
            panel.Close();
            Assert.That(Time.timeScale, Is.EqualTo(1.5f));
        }
        finally
        {
            SettingsPanel panel = Object.FindAnyObjectByType<SettingsPanel>();
            if (panel != null) panel.Close();
            JsonUtility.FromJsonOverwrite(before, data.Option);
            data.OptionsChanged();
            data.SaveOptions();
            if (hadSave) PlayerPrefs.SetString(DataManager.OptionsKey, saved);
            else PlayerPrefs.DeleteKey(DataManager.OptionsKey);
            PlayerPrefs.Save();
            Time.timeScale = 1f;
        }
        yield return new ExitPlayMode();
    }
}
