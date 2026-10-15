using System;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using Object = UnityEngine.Object;

public class RoundEventTests
{
    private readonly List<Object> _objects = new List<Object>();
    private ResourceManager _resources;
    private RoundEventData _data;
    private RoundEventGoldEffect _effect;

    [SetUp]
    public void SetUp()
    {
        var go = new GameObject("EventResources");
        _objects.Add(go);
        _resources = go.AddComponent<ResourceManager>();
        _resources.AddGold(1000);
        _effect = Make<RoundEventGoldEffect>("gold");
        _data = Make<RoundEventData>("event");
        var options = new List<RoundEventOptionData>();
        for (int i = 0; i < 6; i++)
        {
            RoundEventOptionData option = Make<RoundEventOptionData>($"option_{i}");
            Set(option, "title", $"선택지 {i}");
            Set(option, "effect", _effect);
            options.Add(option);
        }
        Set(_data, "options", options);
    }

    [TearDown]
    public void TearDown()
    {
        for (int i = _objects.Count - 1; i >= 0; i--)
            if (_objects[i] != null) Object.DestroyImmediate(_objects[i]);
        _objects.Clear();
    }

    [TestCase(3)]
    [TestCase(4)]
    public void OffersAreDistinctAndRefreshAlwaysChangesOnlyRequestedSlot(int count)
    {
        Set(_data, "choiceCount", count);
        RoundEventSession session = Session();
        for (int iteration = 0; iteration < 3; iteration++)
        {
            var before = new List<RoundEventOptionData>();
            for (int i = 0; i < count; i++) before.Add(session.GetOffer(i));
            Assert.That(new HashSet<RoundEventOptionData>(before).Count, Is.EqualTo(count));
            Assert.That(session.TryRefresh(0, out _), Is.True);
            Assert.That(before.Contains(session.GetOffer(0)), Is.False);
            for (int i = 1; i < count; i++) Assert.That(session.GetOffer(i), Is.SameAs(before[i]));
        }
    }

    [Test]
    public void FreeRefreshBelongsToEachSlotAndPaidCostIsShared()
    {
        RoundEventSession session = Session();
        Assert.That(session.TryRefresh(0, out _), Is.True);
        Assert.That(_resources.Gold, Is.EqualTo(1000));
        Assert.That(session.HasFreeRefresh(0), Is.False);
        Assert.That(session.HasFreeRefresh(1), Is.True);
        Assert.That(session.TryRefresh(0, out _), Is.True);
        Assert.That(_resources.Gold, Is.EqualTo(980));
        Assert.That(session.PaidRefreshCost, Is.EqualTo(30));
        Assert.That(session.TryRefresh(1, out _), Is.True);
        Assert.That(session.PaidRefreshCost, Is.EqualTo(30));
        Assert.That(session.TryRefresh(1, out _), Is.True);
        Assert.That(_resources.Gold, Is.EqualTo(950));
        Assert.That(session.GetRefreshCost(0), Is.EqualTo(40));
        Assert.That(session.GetRefreshCost(1), Is.EqualTo(40));
        Assert.That(session.GetRefreshCost(2), Is.Zero);
    }

    [Test]
    public void InsufficientGoldDoesNotChangeOfferOrPaidCount()
    {
        RoundEventSession session = Session();
        session.TryRefresh(0, out _);
        _resources.TrySpendGold(1000);
        RoundEventOptionData before = session.GetOffer(0);
        Assert.That(session.TryRefresh(0, out _), Is.False);
        Assert.That(session.GetOffer(0), Is.SameAs(before));
        Assert.That(session.PaidRefreshCount, Is.Zero);
        Assert.That(session.TryRefresh(1, out _), Is.True);
    }

    [Test]
    public void MissingCandidatesDoNotConsumeFreeRefresh()
    {
        RoundEventSession session = Session();
        var shown = new HashSet<RoundEventOptionData>();
        for (int i = 0; i < session.ChoiceCount; i++) shown.Add(session.GetOffer(i));
        foreach (RoundEventOptionData option in _data.Options)
            if (!shown.Contains(option)) Set(option, "effect", null);
        Assert.That(session.TryRefresh(0, out _), Is.False);
        Assert.That(session.HasFreeRefresh(0), Is.True);
        Assert.That(_resources.Gold, Is.EqualTo(1000));
    }

    [Test]
    public void EffectAppliesOnceAndCompletedSessionCannotRefresh()
    {
        RoundEventSession session = Session();
        Assert.That(session.TrySelect(0, out _), Is.True);
        Assert.That(_resources.Gold, Is.EqualTo(1100));
        Assert.That(session.IsCompleted, Is.True);
        Assert.That(session.TrySelect(1, out _), Is.False);
        Assert.That(session.TryRefresh(1, out _), Is.False);
        Assert.That(_resources.Gold, Is.EqualTo(1100));
    }

    [Test]
    public void FailedEffectKeepsEventOpen()
    {
        var session = new RoundEventSession(_data, new RoundEventContext(null, null, 5), new System.Random(1));
        Assert.That(session.TrySelect(0, out _), Is.False);
        Assert.That(session.IsActive, Is.True);
        Assert.That(session.IsCompleted, Is.False);
    }

    [Test]
    public void NewSessionResetsCostsWithoutChangingSharedData()
    {
        RoundEventSession first = Session();
        first.TryRefresh(0, out _);
        first.TryRefresh(0, out _);
        RoundEventSession second = Session();
        Assert.That(second.PaidRefreshCost, Is.EqualTo(20));
        Assert.That(second.HasFreeRefresh(0), Is.True);
        Assert.That(first.PaidRefreshCost, Is.EqualTo(30));
        Assert.That(_data.InitialRefreshCost, Is.EqualTo(20));
    }

    [Test]
    public void ResourceCallbacksCannotReenterRefreshOrSelection()
    {
        RoundEventSession session = Session();
        session.TryRefresh(0, out _);
        bool refreshed = true;
        bool selected = true;
        _resources.OnChanged += () =>
        {
            refreshed = session.TryRefresh(0, out _);
            selected = session.TrySelect(0, out _);
        };
        Assert.That(session.TryRefresh(0, out _), Is.True);
        Assert.That(refreshed, Is.False);
        Assert.That(selected, Is.False);
        Assert.That(_resources.Gold, Is.EqualTo(980));
        Assert.That(session.PaidRefreshCount, Is.EqualTo(1));
    }

    [Test]
    public void CancellationDuringPaymentRefundsAndClosesSession()
    {
        RoundEventSession session = Session();
        session.TryRefresh(0, out _);
        _resources.OnChanged += session.Cancel;
        Assert.That(session.TryRefresh(0, out _), Is.False);
        Assert.That(_resources.Gold, Is.EqualTo(1000));
        Assert.That(session.PaidRefreshCount, Is.Zero);
        Assert.That(session.TrySelect(0, out _), Is.False);
    }

    [Test]
    public void InvalidIndexDoesNotConsumeAnything()
    {
        RoundEventSession session = Session();
        Assert.That(session.TryRefresh(-1, out _), Is.False);
        Assert.That(session.TrySelect(session.ChoiceCount, out _), Is.False);
        Assert.That(session.HasFreeRefresh(0), Is.True);
        Assert.That(_resources.Gold, Is.EqualTo(1000));
    }

    [Test]
    public void CostSaturatesInsteadOfOverflowing()
    {
        Set(_data, "initialRefreshCost", int.MaxValue - 1);
        Set(_data, "refreshCostIncrease", int.MaxValue);
        _resources.AddGold(int.MaxValue);
        RoundEventSession session = Session();
        session.TryRefresh(0, out _);
        Assert.That(session.TryRefresh(0, out _), Is.True);
        Assert.That(session.PaidRefreshCost, Is.EqualTo(int.MaxValue));
    }

    [Test]
    public void ConfigurationRejectsDuplicateIdsAndInsufficientPool()
    {
        Set(_data.Options[1], "id", _data.Options[0].Id);
        Assert.That(_data.IsValid(), Is.False);
        Assert.Throws<ArgumentException>(() => Session());
        Set(_data, "options", new List<RoundEventOptionData> { _data.Options[0], _data.Options[2], _data.Options[3] });
        Assert.That(_data.IsValid(), Is.False);
    }

    private RoundEventSession Session() => new RoundEventSession(_data,
        new RoundEventContext(_resources, null, 5), new System.Random(1));

    private T Make<T>(string id) where T : BaseSO
    {
        T data = ScriptableObject.CreateInstance<T>();
        _objects.Add(data);
        Set(data, "id", id);
        return data;
    }

    private static void Set(object target, string name, object value)
    {
        for (Type type = target.GetType(); type != null; type = type.BaseType)
        {
            FieldInfo field = type.GetField(name, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.DeclaredOnly);
            if (field == null) continue;
            field.SetValue(target, value);
            return;
        }
        throw new MissingFieldException(name);
    }
}
