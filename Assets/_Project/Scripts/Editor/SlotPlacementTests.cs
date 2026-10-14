using System.Collections.Generic;
using System.Reflection;
using System.Text.RegularExpressions;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;

public class SlotPlacementTests
{
    private readonly List<GameObject> _objects = new List<GameObject>();
    private WaitField _field;
    private Slot _first;
    private Slot _second;
    private Unit _prefab;

    [SetUp]
    public void SetUp()
    {
        string path = AssetDatabase.GUIDToAssetPath("2e325a539d1693241be320f7f2a8d072");
        _prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path)?.GetComponent<Unit>();
        Assert.That(_prefab, Is.Not.Null, "StageScene에서 사용하는 테스트 유닛 프리팹이 필요합니다.");

        GameObject root = CreateObject("SlotPlacementTests");
        _field = root.AddComponent<WaitField>();
        _first = CreateSlot("First", root.transform);
        _second = CreateSlot("Second", root.transform);
        // EditMode에서는 MonoBehaviour.Awake가 자동으로 호출되지 않습니다.
        Invoke(_field, "Awake");
    }

    [TearDown]
    public void TearDown()
    {
        foreach (Slot slot in new[] { _first, _second })
            if (slot != null && slot.Occupant is Unit unit && unit != null && unit.gameObject.scene.IsValid())
                Object.DestroyImmediate(unit.gameObject);
        for (int i = _objects.Count - 1; i >= 0; i--)
            if (_objects[i] != null) Object.DestroyImmediate(_objects[i]);
        _objects.Clear();
    }

    [Test]
    public void PrefabCannotOccupyAnEmptySlot()
    {
        IDropTarget originalTarget = _prefab.CurrentTarget;
        Vector3 originalPosition = _prefab.transform.position;
        Assert.That(_first.CanDrop(_prefab), Is.False);
        LogAssert.Expect(LogType.Error, new Regex("프리팹 원본은 슬롯에 배치할 수 없습니다"));

        _first.OnDrop(_prefab);

        Assert.That(_first.IsEmpty, Is.True);
        Assert.That(_prefab.CurrentTarget, Is.SameAs(originalTarget));
        Assert.That(_prefab.transform.position, Is.EqualTo(originalPosition));
    }

    [Test]
    public void RejectedPrefabDoesNotMakeNextUnitSkipFirstSlot()
    {
        LogAssert.Expect(LogType.Error, new Regex("프리팹 원본은 슬롯에 배치할 수 없습니다"));
        Assert.That(_field.TryAddUnit(_prefab), Is.False);
        Unit instance = CreateObject("AcquiredUnit").AddComponent<Unit>();

        Assert.That(_field.TryAddUnit(instance), Is.True);

        Assert.That(_first.Occupant, Is.SameAs(instance));
        Assert.That(instance.CurrentTarget, Is.SameAs(_first));
        Assert.That(_second.IsEmpty, Is.True);
    }

    [Test]
    public void TestManagerCreatesSceneInstanceBeforeOccupyingFirstSlot()
    {
        IDropTarget originalTarget = _prefab.CurrentTarget;
        Vector3 originalPosition = _prefab.transform.position;
        TestManager manager = CreateManager(_prefab);

        Invoke(manager, "UnitTest");

        Unit placed = _first.Occupant as Unit;
        Assert.That(placed, Is.Not.Null);
        Assert.That(placed, Is.Not.SameAs(_prefab));
        Assert.That(placed.gameObject.scene.IsValid(), Is.True);
        Assert.That(placed.gameObject.activeInHierarchy, Is.True);
        Assert.That(placed.CurrentTarget, Is.SameAs(_first));
        Assert.That(placed.transform.position, Is.EqualTo(_first.transform.position));
        Assert.That(_prefab.CurrentTarget, Is.SameAs(originalTarget));
        Assert.That(_prefab.transform.position, Is.EqualTo(originalPosition));
        Assert.That(_field.GetEmptySlot(), Is.SameAs(_second));
    }

    [Test]
    public void TestManagerReusesSceneUnitAndFreedFirstSlotCanBeFilledAgain()
    {
        Unit existing = CreateObject("ExistingUnit").AddComponent<Unit>();
        Invoke(CreateManager(existing), "UnitTest");
        Assert.That(_first.Occupant, Is.SameAs(existing));

        _first.Remove(existing);
        _second.OnDrop(existing);
        Unit next = CreateObject("NextUnit").AddComponent<Unit>();

        Assert.That(_field.TryAddUnit(next), Is.True);
        Assert.That(_first.Occupant, Is.SameAs(next));
        Assert.That(_second.Occupant, Is.SameAs(existing));
    }

    private TestManager CreateManager(Unit unit)
    {
        TestManager manager = CreateObject("TestManager").AddComponent<TestManager>();
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        typeof(TestManager).GetField("units", flags).SetValue(manager, new[] { unit });
        typeof(TestManager).GetField("waitField", flags).SetValue(manager, _field);
        return manager;
    }

    private GameObject CreateObject(string name)
    {
        var result = new GameObject(name);
        _objects.Add(result);
        return result;
    }

    private Slot CreateSlot(string name, Transform parent)
    {
        GameObject result = CreateObject(name);
        result.transform.SetParent(parent);
        return result.AddComponent<Slot>();
    }

    private static void Invoke(object target, string method)
    {
        target.GetType().GetMethod(method, BindingFlags.Instance | BindingFlags.NonPublic)
            .Invoke(target, null);
    }
}
