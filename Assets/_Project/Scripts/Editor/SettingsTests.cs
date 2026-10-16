using System;
using System.IO;
using NUnit.Framework;
using UnityEngine;

public class SettingsTests
{
    [Test]
    public void OptionsRoundTripAndOldSavesKeepNewDefaults()
    {
        var options = new OptionData { ShowDamage = false, Language = GameLanguage.English };
        options.Screen.Width = 1600;
        options.Screen.Height = 900;
        options.Screen.Mode = EScreenMode.Borderless;
        JsonUtility.FromJsonOverwrite("{\"_volume\":{\"_master\":0.25,\"_bgm\":0.4,\"_sfx\":0.9}}", options);
        var reload = new OptionData();
        JsonUtility.FromJsonOverwrite(JsonUtility.ToJson(options), reload);
        reload.Validate();
        Assert.That(reload.ShowDamage, Is.False);
        Assert.That(reload.Language, Is.EqualTo(GameLanguage.English));
        Assert.That(reload.Screen.Width, Is.EqualTo(1600));
        Assert.That(reload.Screen.Height, Is.EqualTo(900));
        Assert.That(reload.Screen.Mode, Is.EqualTo(EScreenMode.Borderless));
        Assert.That(reload.Volume.Master, Is.EqualTo(.25f));
        Assert.That(reload.Volume.Bgm, Is.EqualTo(.4f));
        Assert.That(reload.Volume.Sfx, Is.EqualTo(.9f));
        var old = new OptionData();
        JsonUtility.FromJsonOverwrite("{\"_screen\":{\"_width\":1280,\"_height\":720}}", old);
        old.Validate();
        Assert.That(old.ShowDamage, Is.True);
        Assert.That(old.Language, Is.EqualTo(GameLanguage.Korean));
    }

    [Test]
    public void InvalidStoredValuesAreSanitized()
    {
        var options = new OptionData();
        JsonUtility.FromJsonOverwrite("{\"_volume\":{\"_master\":-2,\"_bgm\":8},\"_screen\":{\"_width\":-1,\"_height\":0,\"_mode\":99},\"_language\":99}", options);
        options.Validate();
        Assert.That(options.Volume.Master, Is.Zero);
        Assert.That(options.Volume.Bgm, Is.EqualTo(1));
        Assert.That(options.Screen.Width, Is.EqualTo(800));
        Assert.That(options.Screen.Height, Is.EqualTo(600));
        Assert.That(options.Screen.Mode, Is.EqualTo(EScreenMode.FullScreen));
        Assert.That(options.Language, Is.EqualTo(GameLanguage.Korean));
    }

    [TestCase(EScreenMode.FullScreen, FullScreenMode.ExclusiveFullScreen)]
    [TestCase(EScreenMode.Windowed, FullScreenMode.Windowed)]
    [TestCase(EScreenMode.Borderless, FullScreenMode.FullScreenWindow)]
    public void ScreenModesMapExplicitly(EScreenMode mode, FullScreenMode expected) =>
        Assert.That(DataManager.ToFullScreenMode(mode), Is.EqualTo(expected));

    [Test]
    public void ResetAlsoReplacesBackupAndCannotRestoreOldProgress()
    {
        string directory = Path.Combine(Path.GetTempPath(), "MonoeyeSettingsTest_" + Guid.NewGuid().ToString("N"));
        string path = Path.Combine(directory, "meta.json");
        try
        {
            var store = new MetaProgressionSaveStore(path);
            store.Save(JsonUtility.ToJson(new MetaProgression.SaveData { points = 100 }));
            store.Save(JsonUtility.ToJson(new MetaProgression.SaveData { points = 200 }));
            store.Reset(JsonUtility.ToJson(new MetaProgression.SaveData()));
            Assert.That(store.Load(out _).points, Is.Zero);
            File.WriteAllText(path, "broken");
            Assert.That(new MetaProgressionSaveStore(path).Load(out string notice).points, Is.Zero);
            Assert.That(notice, Is.Not.Empty);
            Assert.That(MetaProgression.Parse(File.ReadAllText(path + ".bak")).levels, Is.Empty);
        }
        finally
        {
            // 이 테스트에서 만든 고유 임시 디렉터리만 삭제합니다.
            if (Directory.Exists(directory)) Directory.Delete(directory, true);
        }
    }
}
