using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using Object = UnityEngine.Object;

public class MetaProgressionTests
{
    private MetaProgressionSettings _settings;
    private readonly List<Object> _objects = new List<Object>();
    private string _saved;

    [SetUp]
    public void SetUp()
    {
        _settings = ScriptableObject.CreateInstance<MetaProgressionSettings>();
        _objects.Add(_settings);
        _saved = null;
    }

    [TearDown]
    public void TearDown()
    {
        for (int i = _objects.Count - 1; i >= 0; i--)
            if (_objects[i] != null) Object.DestroyImmediate(_objects[i]);
        _objects.Clear();
    }

    private MetaProgression Progression(int points = 10000, Action<string> save = null) =>
        new MetaProgression(_settings, new MetaProgression.SaveData { points = points }, save ?? (json => _saved = json));

    [Test]
    public void ShippedSettingsLoadAndEveryUpgradeIsIndependent()
    {
        var asset = Resources.Load<MetaProgressionSettings>("MetaProgressionSettings");
        Assert.That(asset, Is.Not.Null);
        Assert.That(asset.IsValid, Is.True);
        Assert.That(asset.upgrades.Count, Is.EqualTo(12));
        foreach (MetaUpgradeDefinition definition in asset.upgrades)
        {
            var progression = new MetaProgression(asset, new MetaProgression.SaveData { points = definition.baseCost }, _ => { });
            Assert.That(progression.TryPurchase(definition.id, out string reason), Is.True, reason);
            Assert.That(progression.Level(definition.id), Is.EqualTo(1));
            Assert.That(progression.Points, Is.Zero);
        }
    }

    [Test]
    public void CostIncreasesAndMaximumAndInsufficientFundsDoNotSpend()
    {
        MetaProgression progression = Progression(25);
        Assert.That(progression.TryPurchase("attack", out _), Is.True);
        Assert.That(progression.TryPurchase("attack", out _), Is.True);
        Assert.That(progression.Points, Is.Zero);
        Assert.That(progression.Level("attack"), Is.EqualTo(2));
        string before = _saved;
        Assert.That(progression.TryPurchase("attack", out _), Is.False);
        Assert.That(progression.TryPurchase("missing", out _), Is.False);
        Assert.That(_saved, Is.EqualTo(before));
        progression = Progression();
        MetaUpgradeDefinition ticket = progression.Find("starting_recruitment");
        for (int i = 0; i < ticket.maxLevel; i++) Assert.That(progression.TryPurchase(ticket.id, out _), Is.True);
        int points = progression.Points;
        Assert.That(progression.TryPurchase(ticket.id, out _), Is.False);
        Assert.That(progression.Points, Is.EqualTo(points));
    }

    [Test]
    public void FailedSaveDoesNotCommitPurchaseOrRewardAndCanRetry()
    {
        bool fail = true;
        MetaProgression progression = Progression(100, json =>
        {
            if (fail) throw new IOException("disk full");
            _saved = json;
        });
        Assert.That(progression.TryPurchase("attack", out _), Is.False);
        Assert.That(progression.Level("attack"), Is.Zero);
        Assert.That(progression.Points, Is.EqualTo(100));
        Assert.That(progression.TryReward("run", 3, true, out int awarded, out _), Is.False);
        Assert.That(awarded, Is.Zero);
        fail = false;
        Assert.That(progression.TryReward("run", 3, true, out awarded, out _), Is.True);
        Assert.That(awarded, Is.EqualTo(130));
        Assert.That(progression.Points, Is.EqualTo(230));
    }

    [Test]
    public void RewardIsIdempotentAcrossReloadAndUsesCompletedWaveCount()
    {
        MetaProgression progression = Progression(0);
        Assert.That(progression.TryReward("first", 4, false, out int awarded, out _), Is.True);
        Assert.That(awarded, Is.EqualTo(40));
        var reload = new MetaProgression(_settings, MetaProgression.Parse(_saved), json => _saved = json);
        Assert.That(reload.TryReward("first", 4, false, out awarded, out _), Is.True);
        Assert.That(awarded, Is.Zero);
        Assert.That(reload.Points, Is.EqualTo(40));
        Assert.That(reload.TryReward("second", 10, true, out awarded, out _), Is.True);
        Assert.That(awarded, Is.EqualTo(200));
        Assert.That(reload.TryReward("empty", 0, false, out awarded, out _), Is.True);
        Assert.That(awarded, Is.Zero);
    }

    [Test]
    public void StableIdsSurviveReorderingAndNewNodesStartAtZero()
    {
        MetaProgression progression = Progression();
        progression.TryPurchase("skill_range", out _);
        _settings.upgrades.Reverse();
        var reload = new MetaProgression(_settings, MetaProgression.Parse(_saved), _ => { });
        Assert.That(reload.Level("skill_range"), Is.EqualTo(1));
        Assert.That(reload.Level("attack"), Is.Zero);
    }

    [TestCase("{}")]
    [TestCase("not json")]
    [TestCase("{\"version\":2,\"points\":0,\"levels\":[]}")]
    [TestCase("{\"version\":1,\"points\":-1,\"levels\":[]}")]
    [TestCase("{\"version\":1,\"points\":0,\"levels\":[{\"id\":\"a\",\"level\":1},{\"id\":\"a\",\"level\":2}]}")]
    public void InvalidSavesAreRejected(string json) => Assert.Throws<FormatException>(() => MetaProgression.Parse(json));

    [Test]
    public void FileStoreRecoversBackupAndNeverOverwritesBothInvalidSaves()
    {
        string directory = Path.Combine(Path.GetTempPath(), "MonoeyeMetaTests_" + Guid.NewGuid().ToString("N"));
        string path = Path.Combine(directory, "meta.json");
        try
        {
            var store = new MetaProgressionSaveStore(path);
            var progression = new MetaProgression(_settings, store.Load(out _), store.Save);
            progression.TryReward("first", 1, false, out _, out _);
            progression.TryReward("second", 2, false, out _, out _);
            Assert.That(store.Load(out _).points, Is.EqualTo(30));
            File.WriteAllText(path, "broken");
            var recovered = new MetaProgressionSaveStore(path);
            Assert.That(recovered.Load(out string notice).points, Is.EqualTo(10));
            Assert.That(notice, Is.Not.Empty);
            var retry = new MetaProgression(_settings, recovered.Load(out _), recovered.Save);
            Assert.That(retry.TryReward("second", 2, false, out _, out _), Is.True);
            Assert.That(recovered.Load(out _).points, Is.EqualTo(30));
            File.WriteAllText(path, "bad primary");
            File.WriteAllText(path + ".bak", "bad backup");
            Assert.Throws<IOException>(() => new MetaProgressionSaveStore(path).Load(out _));
            Assert.That(File.ReadAllText(path), Is.EqualTo("bad primary"));
            Assert.That(File.ReadAllText(path + ".bak"), Is.EqualTo("bad backup"));
        }
        finally
        {
            // 테스트가 직접 만든 고유 디렉터리만 정리합니다.
            if (Directory.Exists(directory)) Directory.Delete(directory, true);
        }
    }

    [Test]
    public void InitialResourcesApplyOnceAndOnlyEarnedGoldIsMultiplied()
    {
        MetaProgression progression = Progression();
        foreach (string id in new[] { "starting_gold", "starting_honor", "starting_refresh", "starting_recruitment", "gold_gain" })
            Assert.That(progression.TryPurchase(id, out _), Is.True);
        MetaUpgradeBonuses bonuses = MetaUpgradeBonuses.From(progression);
        var go = new GameObject("MetaResources");
        _objects.Add(go);
        ResourceManager resources = go.AddComponent<ResourceManager>();
        resources.ApplyStartingMetaBonuses(bonuses);
        resources.ApplyStartingMetaBonuses(bonuses);
        Assert.That(resources.Gold, Is.EqualTo(5));
        Assert.That(resources.HonorLevel, Is.EqualTo(2));
        Assert.That(resources.HonorExperience, Is.Zero);
        Assert.That(resources.InnRefreshTickets, Is.EqualTo(1));
        Assert.That(resources.GetUnitSelectionTickets(5), Is.EqualTo(1));
        resources.GrantGold(100);
        Assert.That(resources.Gold, Is.EqualTo(110));
        resources.AddGold(100);
        Assert.That(resources.Gold, Is.EqualTo(210));
        resources.GrountEnemyBounty(new List<Bounty> { new GoldBounty(100) });
        Assert.That(resources.Gold, Is.EqualTo(315));
    }

    [Test]
    public void SmallGoldDropsAccumulateFractionalUpgradeRewards()
    {
        var go = new GameObject("SmallGoldRewards");
        _objects.Add(go);
        ResourceManager resources = go.AddComponent<ResourceManager>();
        resources.SetGoldGainEffect("meta", multiplier: 1.05f);
        for (int i = 0; i < 20; i++) resources.GrountEnemyBounty(new List<Bounty> { new GoldBounty(1) });
        Assert.That(resources.Gold, Is.EqualTo(21));
    }

    [Test]
    public void BasicAttackAndSkillUseSeparateRangesAndCooldownsWithoutMutatingData()
    {
        MetaProgression progression = Progression();
        foreach (string id in new[] { "attack", "attack_speed", "basic_range", "skill_range", "skill_cooldown" })
            progression.TryPurchase(id, out _);
        progression.TryPurchase("basic_range", out _);
        UnitData data = ScriptableObject.CreateInstance<UnitData>();
        _objects.Add(data);
        var go = new GameObject("MetaUnit");
        go.SetActive(false);
        _objects.Add(go);
        Unit unit = go.AddComponent<Unit>();
        unit.InitializeForSpawn(data);
        unit.Stat.ApplyMetaBonuses(MetaUpgradeBonuses.From(progression));
        var ability = ScriptableObject.CreateInstance<AbilityData>();
        _objects.Add(ability);
        Set(ability, "cooldown", 10f);
        Set(ability, "range", 3f);
        Set(ability, "rangeType", EAbilityRangeType.OwnerAttackRange);
        var basic = new AbilityContext(unit, true);
        var skill = new AbilityContext(unit);
        Assert.That(unit.Stat.AttackPower, Is.EqualTo(data.Attack * 1.05f).Within(.001f));
        Assert.That(ability.GetEffectiveRange(basic), Is.EqualTo(data.Range * 1.1f).Within(.001f));
        Assert.That(ability.GetEffectiveRange(skill), Is.EqualTo(data.Range * 1.05f).Within(.001f));
        Assert.That(ability.CreateInstance(basic).EffectiveCooldown, Is.EqualTo(10f / (data.AttackSpd * 1.05f)).Within(.001f));
        Assert.That(ability.CreateInstance(skill).EffectiveCooldown, Is.EqualTo(9.7f).Within(.001f));
        unit.Stat.ApplyMetaBonuses(MetaUpgradeBonuses.From(progression));
        Assert.That(unit.Stat.AttackPower, Is.EqualTo(data.Attack * 1.05f).Within(.001f));
        Assert.That(ability.Cooldown, Is.EqualTo(10f));
        Assert.That(ability.Range, Is.EqualTo(3f));
        Set(ability, "rangeType", (EAbilityRangeType)0);
        if ((EAbilityRangeType)0 != EAbilityRangeType.OwnerAttackRange)
        {
            Assert.That(ability.GetEffectiveRange(basic), Is.EqualTo(3.3f).Within(.001f));
            Assert.That(ability.GetEffectiveRange(skill), Is.EqualTo(3.15f).Within(.001f));
        }
    }

    private static void Set(object target, string name, object value) => target.GetType()
        .GetField(name, BindingFlags.Instance | BindingFlags.NonPublic).SetValue(target, value);
}
